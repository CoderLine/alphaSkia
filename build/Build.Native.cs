using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

[TypeConverter(typeof(TypeConverter<TargetOperatingSystem>))]
public class TargetOperatingSystem : Enumeration
{
    public static TargetOperatingSystem Windows = new()
    {
        Value = "windows",
        SkiaTargetOs = "win",
        SkiaGnArgs =
        {
            ["skia_enable_fontmgr_win_gdi"] = "false"
        },
        RuntimeIdentifier = "win"
    };

    public static TargetOperatingSystem Linux = new()
    {
        Value = "linux",
        SkiaTargetOs = "linux",
        SkiaGnArgs =
        {
            ["skia_use_system_freetype2"] = "false"
        },
        RuntimeIdentifier = "linux"
    };

    public static TargetOperatingSystem Android = new()
    {
        Value = "android",
        SkiaTargetOs = "android",
        SkiaGnArgs =
        {
            ["skia_use_system_freetype2"] = "false"
        },
        RuntimeIdentifier = "android"
    };

    public static TargetOperatingSystem MacOs = new()
    {
        Value = "macos",
        SkiaTargetOs = "mac",
        SkiaGnArgs =
        {
            ["skia_use_system_freetype2"] = "false",
            ["skia_use_metal"] = "true"
        },
        RuntimeIdentifier = "macos"
    };

    public string SkiaTargetOs { get; private set; }
    public Dictionary<string, string> SkiaGnArgs { get; } = new();

    public string RuntimeIdentifier { get; private set; }
}

[TypeConverter(typeof(TypeConverter<Architecture>))]
public class Architecture : Enumeration
{
    public static Architecture X64 = new()
        { Value = "x64", LinuxArch = "amd64", LinuxCrossToolchain = "", LinuxCrossTargetArch = "" };

    public static Architecture X86 = new()
    {
        Value = "x86", LinuxArch = "i386", LinuxCrossToolchain = "i686-linux-gnu",
        LinuxCrossTargetArch = "i686-linux-gnu"
    };

    public static Architecture Arm = new()
    {
        Value = "arm", LinuxArch = "armhf", LinuxCrossToolchain = "arm-linux-gnueabihf",
        LinuxCrossTargetArch = "armv7a-linux-gnueabihf"
    };

    public static Architecture Arm64 = new()
    {
        Value = "arm64", LinuxArch = "arm64", LinuxCrossToolchain = "aarch64-linux-gnu",
        LinuxCrossTargetArch = "aarch64-linux-gnu"
    };

    public string LinuxArch { get; private set; }
    public string LinuxCrossToolchain { get; private set; }
    public string LinuxCrossTargetArch { get; private set; }
}

[TypeConverter(typeof(TypeConverter<Variant>))]
public class Variant : Enumeration
{
    public static Variant Static = new() { Value = "static", IsShared = false };
    public static Variant Shared = new() { Value = "shared", IsShared = true };
    public static Variant Jni = new() { Value = "jni", IsShared = true };
    public bool IsShared { get; private set; }
}

partial class Build
{
    [Parameter] static AbsolutePath SkiaPath = RootDirectory / "externals" / "skia";
    [Parameter] static AbsolutePath DepotPath = RootDirectory / "externals" / "depot_tools";

    // Compiler Option
    [Parameter]
    readonly string LlvmHome = GetVariable<string>("LLVM_HOME") ??
                               (OperatingSystem.IsWindows() ? "C:\\Program Files\\LLVM" : "");

    // Tools
    [Parameter] readonly AbsolutePath GnExe = GetVariable<string>("GN_EXE") ?? SkiaPath / "bin" / $"gn{ExeExtension}";
    Tool GnTool => ToolResolver.GetTool(GnExe);

    [Parameter]
    readonly AbsolutePath NinjaExe = GetVariable<string>("NINJA_EXE") ?? DepotPath / $"ninja{ScriptExtension}";

    Tool NinjaTool => ToolResolver.GetTool(NinjaExe);

    [Parameter] readonly string PythonExe = GetVariable<string>("PYTHON_EXE") ?? "python3";
    Tool PythonTool => File.Exists(PythonExe) ? ToolResolver.GetTool(PythonExe) : ToolResolver.GetPathTool("python3");

    [Parameter] readonly bool ParallelGitClone = GetVariable<bool?>("GIT_CLONE_PARALLEL") ?? true;

    [Parameter] readonly string GitExe = GetVariable<string>("GIT_EXE") ?? "git";
    Tool GitTool => File.Exists(GitExe) ? ToolResolver.GetTool(GitExe) : ToolResolver.GetPathTool("git");

    // Output Options
    [Parameter] readonly TargetOperatingSystem TargetOs;
    [Parameter] readonly Architecture Architecture;

    [Parameter] readonly Variant Variant;
    [Parameter(Name = "use-cache")] readonly string UseCacheParam;

    bool UseCache => "true".Equals(UseCacheParam, StringComparison.OrdinalIgnoreCase);

    bool SkipLibSkia => CanUseCachedBinaries("skia", TargetOs.RuntimeIdentifier);

    public Target GitSyncDepsLibSkia => _ => _
        .Unlisted()
        .OnlyWhenStatic(() => !SkipLibSkia)
        .DependsOn(SetupDepotTools)
        .Executes(() =>
        {
            // syncing all dependencies requires a lot of disk space
            // exceeding also the availabel space on GHA runners
            // here we try to only sync dependencies we know are needed
            // This list is created based on the compile logs indicating
            // which third party modules are needed
            var requiredDependencies = new[]
            {
                "buildtools",
                "third_party/externals/harfbuzz",
                "third_party/externals/freetype",
                "third_party/externals/libpng",
                "third_party/externals/zlib",
                "third_party/externals/wuffs",
                "third_party/externals/vulkanmemoryallocator",

                // Android font manager
                "third_party/externals/expat"
            };

            return GitSyncDepsCustom(requiredDependencies);
        });

    // public Target GitSyncDepsLibAlphaSkiaJni => _ => _
    //     .Unlisted()
    //     .DependsOn(SetupDepotTools)
    //     .Executes(() =>
    //     {
    //         var requiredDependencies = new[]
    //         {
    //             "buildtools"
    //         };
    //
    //         return GitSyncDepsCustom(requiredDependencies);
    //     });

    public Target LibSkiaWithCache => _ => _
        .Unlisted()
        .Requires(() => Architecture)
        .Requires(() => TargetOs)
        // ensure it runs before any oher targets
        .Before(SetupDepotTools, PatchSkiaBuildFiles, GitSyncDepsLibSkia)
        .Executes(() =>
        {
            if (SkipLibSkia)
            {
                FileSystemTasks.CopyDirectoryRecursively(DistBasePath, ArtifactBasePath, DirectoryExistsPolicy.Merge,
                    FileExistsPolicy.OverwriteIfNewer);
            }
            else
            {
                GitTool("submodule update --init --recursive");
                if (OperatingSystem.IsLinux())
                {
                    InstallDependenciesLinux();
                }
            }
        })
        .Triggers(LibSkia);

    public Target LibSkia => _ => _
        .DependsOn(GitSyncDepsLibSkia, PatchSkiaBuildFiles)
        .OnlyWhenStatic(() => !SkipLibSkia)
        .Requires(() => Architecture)
        .Requires(() => TargetOs)
        .Executes(BuildSkia);

    public Target LibAlphaSkia => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .Requires(() => TargetOs)
        .Executes(BuildAlphaSkia);

    string GetLibDirectory(string libName = "skia", TargetOperatingSystem targetOs = null,
        Architecture arch = null, Variant variant = null)
    {
        targetOs ??= TargetOs;
        arch ??= Architecture;
        variant ??= Variant;

        return $"{libName}-{targetOs.RuntimeIdentifier}-{arch}-{variant}";
    }

    Task GitSyncDepsCustom(string[] requiredDependencies)
    {
        var depsFile = SkiaPath / "DEPS";
        var depsData = ReadDeps(depsFile);

        if (ParallelGitClone)
        {
            var all = requiredDependencies.Select(d => Task.Run(() =>
            {
                if (!depsData.TryGetValue(d, out var url))
                {
                    throw new InvalidOperationException($"Could not find dependency {d} in DEPS file");
                }

                GitSyncDepsCustom(d, url);
            }));

            return Task.WhenAll(all.ToArray());
        }
        else
        {
            foreach (var d in requiredDependencies)
            {
                if (!depsData.TryGetValue(d, out var url))
                {
                    throw new InvalidOperationException($"Could not find dependency {d} in DEPS file");
                }

                GitSyncDepsCustom(d, url);
            }

            return Task.CompletedTask;
        }
    }

    void GitSyncDepsCustom(string dependencyName, string dependencyUrl)
    {
        var parts = dependencyUrl.Split('@', 2, StringSplitOptions.TrimEntries);
        var repo = parts[0];
        var commitHash = parts[1];

        var directory = SkiaPath / dependencyName;
        GitCheckoutToDirectory(repo, commitHash, directory);
    }

    void GitCheckoutToDirectory(string repo, string commitHash, AbsolutePath directory)
    {
        Log.Debug("Handling dependency {repo}@{commitHash}", repo, commitHash);
        if (!(directory / ".git").DirectoryExists())
        {
            directory.CreateDirectory();
            GitTool("init --quiet",
                workingDirectory: directory);
            GitTool($"remote add origin {repo}",
                workingDirectory: directory);
        }

        if (!commitHash.Equals(GetGitHash(directory), StringComparison.OrdinalIgnoreCase))
        {
            Log.Information("Fetching {repo}@{commitHash}", repo, commitHash);
            GitTool($"fetch origin {commitHash}",
                workingDirectory: directory, logOutput: false);
            GitTool($"reset --hard {commitHash}",
                workingDirectory: directory, logOutput: false);
        }
        else
        {
            Log.Debug("Dependency {repo}@{commitHash} is up to date", repo, commitHash);
        }
    }

    string GetGitHash(AbsolutePath directory)
    {
        var output = GitTool("rev-parse HEAD", workingDirectory: directory, exitHandler: _ =>
        {
        }, logOutput: false, logInvocation: false);

        // error
        if (output.Count == 0 || output.Any(o => o.Text == "fatal: "))
        {
            return "";
        }

        return output.First(o => o.Type == OutputType.Std && o.Text.Length > 0).Text;
    }

    Dictionary<string, string> ReadDeps(AbsolutePath depsFile)
    {
        using var reader = new StreamReader(depsFile);
        while (reader.ReadLine()?.Trim() is { } line)
        {
            if (line.StartsWith("#"))
            {
                continue;
            }

            if (line.StartsWith("deps = {"))
            {
                return ReadDepsData(reader);
            }
        }

        return null;
    }

    Dictionary<string, string> ReadDepsData(TextReader reader)
    {
        var deps = new Dictionary<string, string>();
        while (reader.ReadLine()?.Trim() is { } line)
        {
            if (line.StartsWith("#"))
            {
                continue;
            }

            var keySeparator = line.IndexOf(':');

            if (keySeparator > 0)
            {
                var key = line[..keySeparator].Trim(' ', '"', '\'', ',');
                var value = line[(keySeparator + 1)..].Trim(' ', '"', '\'', ',');
                if (value.StartsWith("http"))
                {
                    deps[key] = value;
                }
            }
        }

        return deps;
    }

    public Target SetupDepotTools => _ => _
        .Unlisted()
        .OnlyWhenStatic(() => !SkipLibSkia)
        .Executes(() =>
        {
            var oldValue = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            var newValue = DepotPath + Path.PathSeparator + oldValue;
            Environment.SetEnvironmentVariable("PATH", newValue, EnvironmentVariableTarget.Process);

            PythonTool(
                arguments: (SkiaPath / "bin" / "fetch-ninja").ToString(),
                workingDirectory: SkiaPath
            );

            PythonTool(
                arguments: (SkiaPath / "bin" / "fetch-gn").ToString(),
                workingDirectory: SkiaPath
            );
        });

    public Target PatchSkiaBuildFiles => _ => _
        .Unlisted()
        .OnlyWhenStatic(() => !SkipLibSkia)
        .Executes(() =>
        {
            // add harfbuzz as dependency as we want it for alphaSkia
            var buildFile = SkiaPath / "BUILD.gn";
            var buildFileSource = buildFile.ReadAllText();
            var skiaComponentStart = buildFileSource.IndexOf("skia_component(\"skia\")", StringComparison.Ordinal);
            if (skiaComponentStart == -1)
            {
                throw new IOException("BUILD.gn of skia changed, cannot patch files");
            }

            var depsStartMarker = "deps = [";
            var depsStart = buildFileSource.IndexOf(depsStartMarker, skiaComponentStart, StringComparison.Ordinal);
            if (depsStart == -1)
            {
                throw new IOException("BUILD.gn of skia changed, cannot patch files");
            }

            var depsListEnd = buildFileSource.IndexOf("]", depsStart, StringComparison.Ordinal);
            if (depsListEnd == -1)
            {
                throw new IOException("BUILD.gn of skia changed, cannot patch files");
            }

            var depsListStart = depsStart + depsStartMarker.Length;
            var depsList = buildFileSource.Substring(depsListStart, depsListEnd - depsListStart);
            if (!depsList.Contains("//third_party/harfbuzz"))
            {
                var newDepsList = depsList.TrimEnd('\r', '\n', '\t', ' ', ',') + ", \"//third_party/harfbuzz\", ";
                var newBuildFileSource = buildFileSource[..depsListStart]
                                         + newDepsList
                                         + buildFileSource[depsListEnd..];
                buildFile.WriteAllText(newBuildFileSource);
            }

            // Bug in skia toolchain, setenv.cmd is not existing in this path. 
            var toolChainBuildFile = SkiaPath / "gn" / "toolchain" / "BUILD.gn";
            var oldToolChainSource = toolChainBuildFile.ReadAllText();
            toolChainBuildFile.WriteAllText(oldToolChainSource.Replace(
                "env_setup = \"$shell $win_sdk/bin/SetEnv.cmd /x86",
                "# env_setup = \"$shell $win_sdk/bin/SetEnv.cmd /x86"
            ));
        });


    void GnNinja(string outDir, string target, Dictionary<string, string> gnArgs, AbsolutePath workingDirectory)
    {
        gnArgs["skia_enable_tools"] = "false";
        gnArgs["is_official_build"] = "true";

        const string quote = "\"";
        // To get a raw double-quote in the output we need as string with 3 double-quotes
        // https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.arguments?view=net-7.0
        const string innerQuote = "\"\"\"";

        string QuoteValue(string value)
        {
            // lists like [ 'prop' ] need inner escaping
            if (value.StartsWith('['))
            {
                return value.Replace("'", innerQuote);
            }

            // never quote boolean values GN doesn't like this
            if (value is "true" or "false")
            {
                return value;
            }

            // any other value beside bools are safe to quote
            return $"{innerQuote}{value}{innerQuote}";
        }

        var allArgs = string.Join(" ", gnArgs.Select(o => $"{o.Key}={QuoteValue(o.Value)}"));

        // not inlined to avoid it being treated as FormattedString
        var gnToolArgs =
            $"gen {outDir} --script-executable={quote}{PythonExe}{quote} --args={quote}{allArgs}{quote}";
        var argument = new ArgumentStringHandler(gnToolArgs.Length, 0, out _);
        argument.AppendLiteral(gnToolArgs);
        GnTool(
            arguments: argument,
            workingDirectory: workingDirectory
        );

        var ninjaArgs = $"-C {outDir} {target}";
        argument = new ArgumentStringHandler(ninjaArgs.Length, 0, out _);
        argument.AppendLiteral(ninjaArgs);
        NinjaTool(
            arguments: argument,
            workingDirectory: workingDirectory
        );
    }

    Dictionary<string, string> PrepareNativeBuild()
    {
        if (OperatingSystem.IsLinux() && IsGitHubActions)
        {
            InstallDependenciesLinux();
        }

        var gnArgs = new Dictionary<string, string>(TargetOs.SkiaGnArgs)
        {
            ["cc"] = "clang",
            ["cxx"] = "'clang++'"
        };

        if (OperatingSystem.IsWindows())
        {
            SetClangWindows(gnArgs);
        }
        else if (OperatingSystem.IsLinux())
        {
            SetClangLinux(gnArgs);
        }
        else if (OperatingSystem.IsMacOS())
        {
            SetClangMacOs(gnArgs);
        }

        if (TargetOs == TargetOperatingSystem.Android)
        {
            gnArgs["ndk"] = NdkPath;
        }

        gnArgs["target_os"] = TargetOs.SkiaTargetOs;
        gnArgs["target_cpu"] = Architecture;

        return gnArgs;
    }

    void BuildAlphaSkia()
    {
        var gnArgs = PrepareNativeBuild();
        var staticLibPath = DistBasePath / GetLibDirectory(variant: Variant.Static);
        
        string buildTarget;
        if (Variant == Variant.Static)
        {
            buildTarget = "libAlphaSkia";
            gnArgs["is_shared_alphaskia"] = "false";
        }
        else if (Variant == Variant.Shared)
        {
            buildTarget = "libAlphaSkia";
            gnArgs["is_shared_alphaskia"] = "true";
        }
        else if (Variant == Variant.Jni)
        {
            buildTarget = "libAlphaSkiaJni";
            gnArgs["is_shared_alphaskia"] = "true";

            var alphaSkiaInclude = DistBasePath / "include";
            var jniInclude = JavaHome / "include";
            AbsolutePath jniPlatformInclude;
            if (OperatingSystem.IsWindows())
            {
                jniPlatformInclude = jniInclude / "windows";
            }
            else if (OperatingSystem.IsLinux())
            {
                jniPlatformInclude = jniInclude / "linux";
            }
            else if (OperatingSystem.IsMacOS())
            {
                jniPlatformInclude = jniInclude / "darwin";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            gnArgs["extra_cflags"] = $"[ '-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniPlatformInclude}' ]";
        }
        else
        {
            throw new ArgumentException("Unknown variant: " + Variant);
        }

        if (TargetOs == TargetOperatingSystem.Windows)
        {
            // TODO: check if clang-cl also works with the linux flags
            AppendToFlagList(gnArgs, "extra_ldflags", $"'/LIBPATH:{staticLibPath}', 'skia.lib', 'user32.lib', 'OpenGL32.lib'");
        }
        else
        {
            AppendToFlagList(gnArgs, "extra_ldflags", $" '-L{staticLibPath}', '-lskia', '-lGL'");
        }
        
        var libDir = GetLibDirectory(buildTarget, TargetOs, Architecture, Variant);
        var artifactsLibPath = IsGitHubActions ? ArtifactBasePath / libDir : null;
        var distPath = DistBasePath / libDir;
        var outDir = SkiaPath / "out" / libDir;
        var libExtension = GetLibExtension(Variant);

        GnNinja($"out/{libDir}", "libAlphaSkia", gnArgs, RootDirectory / "wrapper");

        try
        {
            void CopyBuildOutputTo(AbsolutePath path)
            {
                // libs
                FileSystemTasks.CopyDirectoryRecursively(outDir, path, DirectoryExistsPolicy.Merge,
                    FileExistsPolicy.OverwriteIfNewer, null, file => file.Extension != libExtension);
                // copy header
                FileSystemTasks.CopyFile(RootDirectory / "wrapper" / "include" / "alphaskia.h",
                    DistBasePath / "include" / "alphaskia" / "alphaskia.h", FileExistsPolicy.OverwriteIfNewer);
            }

            CopyBuildOutputTo(distPath);
            if (artifactsLibPath != null)
            {
                CopyBuildOutputTo(artifactsLibPath);
            }
        }
        catch (Exception e)
        {
            var fileList = Directory.EnumerateFileSystemEntries(outDir).Select(d =>
                File.GetAttributes(d).HasFlag(FileAttributes.Directory) ? "[" + d + "]" : d);
            throw new IOException("Copy files failed. existing files: " + string.Join(", ", fileList), e);
        }
    }

    string GetLibExtension(Variant variant)
    {
        if (TargetOs == TargetOperatingSystem.Windows)
        {
            return variant.IsShared ? ".dll" : ".lib";
        }

        if (TargetOs == TargetOperatingSystem.Linux || TargetOs == TargetOperatingSystem.Android)
        {
            return variant.IsShared ? ".so" : ".a";
        }


        if (TargetOs == TargetOperatingSystem.MacOs)
        {
            return variant.IsShared ? ".dylib" : ".a";
        }

        throw new InvalidOperationException("Unhandled TargetOS: " + TargetOs);
    }

    void BuildSkia()
    {
        var gnArgs = PrepareNativeBuild();
        var libDir = GetLibDirectory("libSkia", TargetOs, Architecture, Variant.Static);
        var artifactsLibPath = IsGitHubActions ? ArtifactBasePath / libDir : null;
        var distPath = DistBasePath / libDir;
        var outDir = SkiaPath / "out" / libDir;
        var libExtension = GetLibExtension(Variant.Static);

        // disable features we don't need
        gnArgs["skia_use_icu"] = "false";
        gnArgs["skia_use_piex"] = "false";
        gnArgs["skia_use_sfntly"] = "false";
        gnArgs["skia_enable_skshaper"] = "true";
        gnArgs["skia_pdf_subset_harfbuzz"] = "false";
        gnArgs["skia_use_expat"] = "false";
        gnArgs["skia_enable_pdf"] = "false";
        gnArgs["skia_use_dng_sdk"] = "false";
        gnArgs["skia_use_libjpeg_turbo_decode"] = "false";
        gnArgs["skia_use_libjpeg_turbo_encode"] = "false";
        gnArgs["skia_use_libwebp_decode"] = "false";
        gnArgs["skia_use_libwebp_encode"] = "false";
        gnArgs["skia_use_xps"] = "false";
        gnArgs["skia_use_libavif"] = "false";
        gnArgs["skia_use_libjxl_decode"] = "false";
        gnArgs["skia_enable_vello_shaders"] = "false";

        gnArgs["skia_enable_sksl"] = "false";

        gnArgs["skia_use_system_expat"] = "false";
        gnArgs["skia_use_system_libjpeg_turbo"] = "false";
        gnArgs["skia_use_system_libpng"] = "false";
        gnArgs["skia_use_system_libwebp"] = "false";
        gnArgs["skia_use_system_zlib"] = "false";
        gnArgs["skia_use_system_harfbuzz"] = "false";

        // graphite is still in dev, stay on ganesh backend
        gnArgs["skia_enable_graphite"] = "false";
        gnArgs["skia_enable_ganesh"] = "true";
        gnArgs["skia_use_vulkan"] = "true";

        GnNinja($"out/{libDir}", "skia", gnArgs, SkiaPath);

        void CopyBuildOutputTo(AbsolutePath path)
        {
            // libs
            FileSystemTasks.CopyDirectoryRecursively(outDir, path, DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer, null, file => file.Extension != libExtension);

            // copy skia headers
            FileSystemTasks.CopyDirectoryRecursively(SkiaPath / "include",
                DistBasePath / "include" / "skia",
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer,
                null,
                file => file.Extension switch
                {
                    ".h" => false,
                    _ => true
                });

            // copy harfbuzz headers
            FileSystemTasks.CopyDirectoryRecursively(SkiaPath / "third_party" / "externals" / "harfbuzz" / "src",
                DistBasePath / "include" / "harfbuzz",
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer,
                null,
                file => file.Extension switch
                {
                    ".h" => false,
                    _ => true
                });
        }

        CopyBuildOutputTo(distPath);
        if (artifactsLibPath != null)
        {
            CopyBuildOutputTo(artifactsLibPath);
        }
    }

    bool HasCachedFiles(string buildTarget, string targetOsOutDir)
    {
        var expectedDirectory = DistBasePath / $"{buildTarget}-{targetOsOutDir}-{Architecture}-{Variant}";
        return expectedDirectory.DirectoryExists() && expectedDirectory.GetFiles().Any();
    }
}