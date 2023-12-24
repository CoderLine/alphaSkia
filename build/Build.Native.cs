using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

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

    Tool GitTool
    {
        get
        {
            var gitTool = File.Exists(GitExe) ? ToolResolver.GetTool(GitExe) : ToolResolver.GetPathTool("git");
            return (arguments, directory, variables, timeout, output, invocation, logger, handler) =>
                gitTool(arguments, directory, variables, timeout, output, invocation, logger ?? ((_, s) =>
                {
                    // git logs a lot of stuff to stderr. we rather silence this
                    Log.Debug(s);
                }), handler);
        }
    }

    // Output Options
    [Parameter] readonly TargetOperatingSystem TargetOs;
    [Parameter] readonly Architecture Architecture;
    [Parameter] readonly Variant Variant;
    [Parameter] readonly bool GnVerbose;
    [Parameter] readonly bool NinjaVerbose;

    [Parameter(Name = "use-cache")] readonly string UseCacheParam;
    bool UseCache => "true".Equals(UseCacheParam, StringComparison.OrdinalIgnoreCase);

    string GetLibDirectory(string libName = "libskia", TargetOperatingSystem targetOs = null,
        Architecture arch = null, Variant variant = null)
    {
        targetOs ??= TargetOs;
        arch ??= Architecture;
        variant ??= Variant;

        return $"{libName.ToLowerInvariant()}-{targetOs?.RuntimeIdentifier}-{arch}-{variant}";
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
        .OnlyWhenStatic(() => !LibSkiaSkip.Value)
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

    void GnNinja(string outDir, string target, Dictionary<string, string> gnArgs,
        Dictionary<string, string> gnFlags,
        AbsolutePath workingDirectory,
        Action beforeCompile = null)
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

        gnFlags["script-executable"] = PythonExe;
        gnFlags["args"] = string.Join(" ", gnArgs.Select(o => $"{o.Key}={QuoteValue(o.Value)}"));
        if (GnVerbose)
        {
            gnFlags["-v"] = "";
        }

        var gnFlagsString = string.Join(" ",
            gnFlags.Select(kvp => kvp.Value.Length > 0 ? $"--{kvp.Key}={quote}{kvp.Value}{quote}" : $"--{kvp.Key}"));

        if (Rebuild)
        {
            (SkiaPath / outDir).DeleteDirectory();
        }

        // not inlined to avoid it being treated as FormattedString
        var gnToolArgs =
            $"gen {outDir} {gnFlagsString}";
        var argument = new ArgumentStringHandler(gnToolArgs.Length, 0, out _);
        argument.AppendLiteral(gnToolArgs);
        GnTool(
            arguments: argument,
            workingDirectory: workingDirectory
        );

        beforeCompile?.Invoke();

        var ninjaArgs = $"-d keeprsp -C {outDir} {target}";
        if (NinjaVerbose)
        {
            ninjaArgs = "-v " + ninjaArgs;
        }

        argument = new ArgumentStringHandler(ninjaArgs.Length, 0, out _);
        argument.AppendLiteral(ninjaArgs);
        NinjaTool(
            arguments: argument,
            workingDirectory: workingDirectory
        );
    }

    Dictionary<string, string> PrepareNativeBuild(Variant variant)
    {
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
            AppendToFlagList(gnArgs, "extra_ldflags", $"'-llog'");
        }

        gnArgs["target_os"] = TargetOs.SkiaTargetOs;
        gnArgs["target_cpu"] = Architecture;
        gnArgs["is_shared_alphaskia"] = variant.IsShared.ToString().ToLowerInvariant();

        return gnArgs;
    }

    string[] GetLibExtensions(Variant variant)
    {
        if (variant == Variant.Node)
        {
            return new[] { ".node" };
        }

        if (TargetOs == TargetOperatingSystem.Windows)
        {
            return variant.IsShared ? new[] { ".dll", ".lib" } : new[] { ".lib" };
        }

        if (TargetOs == TargetOperatingSystem.Linux ||
            TargetOs == TargetOperatingSystem.Android)
        {
            return variant.IsShared ? new[] { ".so" } : new[] { ".a" };
        }


        if (TargetOs == TargetOperatingSystem.MacOs ||
            TargetOs == TargetOperatingSystem.iOS ||
            TargetOs == TargetOperatingSystem.iOSSimulator)
        {
            return variant.IsShared ? new[] { ".dylib" } : new[] { ".a" };
        }

        throw new InvalidOperationException("Unhandled TargetOS: " + TargetOs);
    }
    string GetExeExtension()
    {
        if (TargetOs == TargetOperatingSystem.Windows)
        {
            return ".exe";
        }
        else
        {
            return "";
        }
    }

    bool HasCachedFiles(string buildTarget, Variant variant)
    {
        var libraryDir = GetLibDirectory(buildTarget, variant: variant);
        var expectedDirectory = DistBasePath / libraryDir;
        if (!expectedDirectory.DirectoryExists())
        {
            Log.Debug($"Cache for {libraryDir} is not usable, folder doesn't exist");
            return false;
        }

        if (!expectedDirectory.GetFiles().Any())
        {
            Log.Debug($"Cache for {libraryDir} is not usable, no files in folder");
            return false;
        }

        return true;
    }

    void PatchSkiaToolchain()
    {
        // Bug in skia toolchain, setenv.cmd is not existing in this path. 
        var toolChainBuildFile = SkiaPath / "gn" / "toolchain" / "BUILD.gn";
        var oldToolChainSource = toolChainBuildFile.ReadAllText();
        toolChainBuildFile.WriteAllText(oldToolChainSource.Replace(
            "env_setup = \"$shell $win_sdk/bin/SetEnv.cmd /x86",
            "# env_setup = \"$shell $win_sdk/bin/SetEnv.cmd /x86"
        ));
    }
}