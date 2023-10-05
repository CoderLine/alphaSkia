using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;
using static Nuke.Common.EnvironmentInfo;


[TypeConverter(typeof(TypeConverter<Architecture>))]
public class Architecture : Enumeration
{
    public static Architecture X64 = new() { Value = "x64" };
    public static Architecture X86 = new() { Value = "x86" };
    public static Architecture Arm = new() { Value = "arm" };
    public static Architecture Arm64 = new() { Value = "arm64" };
}

[TypeConverter(typeof(TypeConverter<Variant>))]
public class Variant : Enumeration
{
    public static Variant Static = new() { Value = "static" };
    public static Variant Shared = new() { Value = "shared" };
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
    [Parameter] readonly Architecture Architecture;

    [Parameter] readonly Variant Variant;

    public Target GitSyncDepsSkia => _ => _
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

    public Target GitSyncDepsJni => _ => _
        .DependsOn(SetupDepotTools)
        .Executes(() =>
        {
            var requiredDependencies = new[]
            {
                "buildtools"
            };

            return GitSyncDepsCustom(requiredDependencies);
        });

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
        .Executes(() =>
        {
            var buildConfigNew = new StringBuilder();
            buildConfigNew.AppendLine("declare_args() {");
            buildConfigNew.AppendLine("    is_shared_alphaskia = true");
            buildConfigNew.AppendLine("}");
            buildConfigNew.AppendLine("template(\"alphaskia_build\") {");
            buildConfigNew.AppendLine("    _alphaskia_mode = \"shared_library\"");
            buildConfigNew.AppendLine("    if (!is_shared_alphaskia) {");
            buildConfigNew.AppendLine("        _alphaskia_mode = \"static_library\"");
            buildConfigNew.AppendLine("    }");
            buildConfigNew.AppendLine();
            buildConfigNew.AppendLine("    target(_alphaskia_mode, target_name) {");
            buildConfigNew.AppendLine("        forward_variables_from(invoker, \"*\")");
            buildConfigNew.AppendLine("    }");
            buildConfigNew.AppendLine("}");
            buildConfigNew.AppendLine("set_defaults(\"alphaskia_build\") {");
            buildConfigNew.AppendLine("  configs = default_configs");
            buildConfigNew.AppendLine("  if (!is_shared_alphaskia) {");
            buildConfigNew.AppendLine("      complete_static_lib = true");
            buildConfigNew.AppendLine("  }");
            buildConfigNew.AppendLine("}");

            buildConfigNew.AppendLine("template(\"alphaskia_jni_build\") {");
            buildConfigNew.AppendLine("    _alphaskia_jni_mode = \"shared_library\"");
            buildConfigNew.AppendLine();
            buildConfigNew.AppendLine("    target(_alphaskia_jni_mode, target_name) {");
            buildConfigNew.AppendLine("        forward_variables_from(invoker, \"*\")");
            buildConfigNew.AppendLine("    }");
            buildConfigNew.AppendLine("}");
            buildConfigNew.AppendLine("set_defaults(\"alphaskia_jni_build\") {");
            buildConfigNew.AppendLine("  configs = default_configs");
            buildConfigNew.AppendLine("}");

            PatchSkiaFile(SkiaPath / "gn" / "BUILDCONFIG.gn", buildConfigNew.ToString());

            var buildNew = new StringBuilder();
            buildNew.AppendLine("alphaskia_build(\"libAlphaSkia\") {");
            buildNew.AppendLine("  public_configs = [ \":skia_public\" ]");
            buildNew.AppendLine("  configs += skia_library_configs");
            buildNew.AppendLine();
            buildNew.AppendLine("  defines = [ \"_SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING\" ]");
            buildNew.AppendLine("  if (is_shared_alphaskia) {");
            buildNew.AppendLine("      defines += [ \"ALPHASKIA_DLL\" ]");
            buildNew.AppendLine("  }");
            buildNew.AppendLine();
            buildNew.AppendLine("  ");
            buildNew.AppendLine("  deps = [");
            buildNew.AppendLine("    \":skia\",");
            buildNew.AppendLine("    \"//third_party/harfbuzz\",");
            buildNew.AppendLine("  ]");
            buildNew.AppendLine();
            buildNew.AppendLine("  sources = [");
            buildNew.AppendLine("    \"../../wrapper/src/AlphaSkiaCanvas.cpp\",");
            buildNew.AppendLine("    \"../../wrapper/src/alphaskia_canvas.cpp\",");
            buildNew.AppendLine("    \"../../wrapper/src/alphaskia_image.cpp\",");
            buildNew.AppendLine("    \"../../wrapper/src/alphaskia_typeface.cpp\",");
            buildNew.AppendLine("    \"../../wrapper/src/alphaskia_data.cpp\"");
            buildNew.AppendLine("  ]");
            buildNew.AppendLine("}");
            buildNew.AppendLine("alphaskia_build(\"libAlphaSkiaJni\") {");
            buildNew.AppendLine("  sources = [");
            buildNew.AppendLine("    \"../../lib/java/jni/src/AlphaSkiaCanvas.cpp\",");
            buildNew.AppendLine("    \"../../lib/java/jni/src/AlphaSkiaData.cpp\",");
            buildNew.AppendLine("    \"../../lib/java/jni/src/AlphaSkiaImage.cpp\",");
            buildNew.AppendLine("    \"../../lib/java/jni/src/AlphaSkiaTypeface.cpp\"");
            buildNew.AppendLine("  ]");
            buildNew.AppendLine();
            buildNew.AppendLine("}");
            PatchSkiaFile(SkiaPath / "BUILD.gn", buildNew.ToString());

            // Bug in skia toolchain, setenv.cmd is not existing in this path. 
            var toolChainBuildFile = SkiaPath / "gn" / "toolchain" / "BUILD.gn";
            var oldToolChainSource = toolChainBuildFile.ReadAllText();
            toolChainBuildFile.WriteAllText(oldToolChainSource.Replace(
                "env_setup = \"$shell $win_sdk/bin/SetEnv.cmd /x86",
                "# env_setup = \"$shell $win_sdk/bin/SetEnv.cmd /x86"
            ));
        });

    void PatchSkiaFile(AbsolutePath file, string newText)
    {
        var existingText = file.ReadAllText();
        const string startMarker = "# AlphaSkia Patch Start";
        const string endMarker = "# AlphaSkia Patch End";
        var newTextWithMarker = new StringBuilder();
        newTextWithMarker.AppendLine();
        newTextWithMarker.AppendLine(startMarker);
        newTextWithMarker.AppendLine(newText);
        newTextWithMarker.AppendLine(endMarker);

        var beforePatchIndex = existingText.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (beforePatchIndex == -1)
        {
            // not yet patched
            file.WriteAllText(existingText + newTextWithMarker);
        }
        else
        {
            var afterPatchIndex = existingText.IndexOf(endMarker, beforePatchIndex, StringComparison.OrdinalIgnoreCase);
            if (afterPatchIndex == -1)
            {
                // corrupt patch (no end)
                file.WriteAllText(existingText[..beforePatchIndex].TrimEnd() + newTextWithMarker);
            }
            else
            {
                // already patched
                file.WriteAllText(existingText[..beforePatchIndex].TrimEnd() + newTextWithMarker +
                                  existingText[(afterPatchIndex + endMarker.Length)..].TrimStart());
            }
        }
    }

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

    void BuildSkia(
        string buildTarget,
        string targetOs,
        string arch,
        Variant variant,
        Dictionary<string, string> gnArgs,
        string[] filesToCopy,
        string targetOsOutDir = null)
    {
        targetOsOutDir ??= targetOs;
        var isShared = variant == Variant.Shared;
        var artifactDir = $"{buildTarget}-{targetOsOutDir}-{arch}-{variant}";
        var distPath = DistBasePath / artifactDir;
        var artifactsLibPath = IsGitHubActions ? ArtifactBasePath / artifactDir : null;

        gnArgs["target_os"] = targetOs;
        gnArgs["target_cpu"] = arch;
        gnArgs["is_shared_alphaskia"] = isShared.ToString().ToLowerInvariant();

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

        GnNinja($"out/{buildTarget}/{targetOsOutDir}/{arch}/{variant}", buildTarget, gnArgs, SkiaPath);

        var outDir = SkiaPath / "out" / buildTarget / targetOsOutDir / arch / variant;
        try
        {
            foreach (var file in filesToCopy)
            {
                FileSystemTasks.CopyFile(outDir / file,
                    distPath / file, FileExistsPolicy.Overwrite);
                if (artifactsLibPath != null)
                {
                    FileSystemTasks.CopyFile(outDir / file,
                        artifactsLibPath / file, FileExistsPolicy.Overwrite);
                }
            }

            FileSystemTasks.CopyFile(RootDirectory / "wrapper" / "include" / "libAlphaSkia.h",
                DistBasePath / "include" / "libAlphaSkia.h", FileExistsPolicy.OverwriteIfNewer);

            if (artifactsLibPath != null)
            {
                FileSystemTasks.CopyFile(RootDirectory / "wrapper" / "include" / "libAlphaSkia.h",
                    ArtifactBasePath / "include" / "libAlphaSkia.h", FileExistsPolicy.OverwriteIfNewer);
            }
        }
        catch (Exception e)
        {
            var fileList = Directory.EnumerateFileSystemEntries(outDir).Select(d =>
                File.GetAttributes(d).HasFlag(FileAttributes.Directory) ? "[" + d + "]" : d);
            throw new IOException("Copy files failed. existing files: " + string.Join(", ", fileList), e);
        }
    }
}