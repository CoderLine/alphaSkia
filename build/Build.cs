using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>();

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("llvm")] readonly string LlvmHome = GetVariable<string>("LLVM_HOME") ?? "C:/Program Files/LLVM";
    [Parameter("supportVulkan")] readonly bool SupportVulcan = GetVariable<bool?>("SUPPORT_VULKAN") ?? true;
    [Parameter("gnArgs")] readonly string AdditionalGnArgs = GetVariable<string>("ADDITIONAL_GN_ARGS") ?? "";
    [Parameter("gn")] AbsolutePath GnExe => GetVariable<string>("GN_EXE") ?? SkiaPath / "bin" / $"gn{ExeExtension}";

    [Parameter("ninja")]
    AbsolutePath NinjaExe => GetVariable<string>("NINJA_EXE") ?? DepotPath / $"ninja{ExeExtension}";

    [Parameter("python")] string PythonExe => GetVariable<string>("PYTHON_EXE") ?? "python";

    AbsolutePath SkiaPath => RootDirectory / "externals" / "skia";
    AbsolutePath DepotPath => RootDirectory / "externals" / "depot_tools";

    AbsolutePath HarfbuzzPath =>
        RootDirectory / "externals" / "skia" / "third_party" / "externals" / "harfbuzz";

    public Target WindowsX64 => _ => _
        .DependsOn(SetupDepotTools, GitSyncDeps)
        .OnlyWhenStatic(OperatingSystem.IsWindows)
        .Executes(() =>
        {
            BuildSkiaWindows("x64", "x64", "x64");
            BuildHarfbuzzWindows("x64", "x64");
        });

    public Target WindowsX86 => _ => _
        .DependsOn(SetupDepotTools,GitSyncDeps)
        .OnlyWhenStatic(OperatingSystem.IsWindows)
        .Executes(() =>
        {
            BuildSkiaWindows("Win32", "x86", "x86");
            BuildHarfbuzzWindows("Win32", "x86");
        });

    public Target WindowsArm64 => _ => _
        .DependsOn(SetupDepotTools,GitSyncDeps)
        .OnlyWhenStatic(OperatingSystem.IsWindows)
        .Executes(() =>
        {
            BuildSkiaWindows("ARM64", "arm64", "ARM64");
            BuildHarfbuzzWindows("ARM64", "arm64");
        });
    
    public Target GitSyncDeps => _ => _
        .Executes(() =>
        {
            PythonTool(
                arguments: (SkiaPath / "tools" / "git-sync-deps").ToString(),
                workingDirectory: SkiaPath
            );
        });
    
    public Target SetupDepotTools => _ => _
        .Executes(() =>
        {
            var oldValue = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            var newValue = DepotPath + Path.PathSeparator + oldValue;
            Environment.SetEnvironmentVariable("PATH", newValue,EnvironmentVariableTarget.Process);
        });

    void BuildSkiaWindows(string arch, string skiaArch, string dir)
    {
        var clang = string.IsNullOrEmpty(LlvmHome) ? "" : $"clang_win='{LlvmHome}' ";
        var debugSuffix = Configuration == Configuration.Release ? "" : "d";

        GnNinja(RootDirectory / "windows" / arch, "AlphaSkia",
            "target_os='win'" +
            $"target_cpu='{skiaArch}' " +
            "skia_enable_fontmgr_win_gdi=false " +
            "skia_use_dng_sdk=true " +
            "skia_use_icu=false " +
            "skia_use_piex=true " +
            "skia_use_sfntly=false " +
            "skia_use_system_expat=false " +
            "skia_use_system_libjpeg_turbo=false " +
            "skia_use_system_libpng=false " +
            "skia_use_system_libwebp=false " +
            "skia_use_system_zlib=false " +
            $"skia_use_vulkan={SupportVulcan}".ToLower() +
            clang +
            $"extra_cflags=[ '-DSKIA_C_DLL', '/MT{debugSuffix}', '/EHsc', '/Z7', '-D_HAS_AUTO_PTR_ETC=1' ] " +
            "extra_ldflags=[ '/DEBUG:FULL' ] " + AdditionalGnArgs);

        var outDir = RootDirectory / "output" / "native" / "windows" / dir;

        outDir.CreateDirectory();
        CopyFileToDirectory(SkiaPath / "out" / "windows" / arch / "libAlphaSkia.dll", outDir);
        CopyFileToDirectory(SkiaPath / "out" / "windows" / arch / "libAlphaSkia.pdb", outDir);
    }

    void GnNinja(AbsolutePath outDir, string target, string skiaArgs)
    {
        skiaArgs +=
            " skia_enable_tools=false " +
            $" is_official_build={Configuration == Configuration.Release} ".ToLower();

        var quote = OperatingSystem.IsWindows() ? "\"" : "'";
        var innerQuote = OperatingSystem.IsWindows() ? "\\\"" : "\"";

        GnTool(
            arguments:
            $"gen out/{outDir} --script-executable={quote}{PythonExe}{quote} --args={quote}{skiaArgs.Replace("'", innerQuote)}{quote}",
            workingDirectory: SkiaPath
        );
        NinjaTool(
            arguments: $"-C out/{outDir} {target}",
            workingDirectory: skiaArgs
        );
    }

    string ExeExtension => OperatingSystem.IsWindows() ? ".exe" : "";
    Tool GnTool => ToolResolver.GetTool(GnExe);
    Tool NinjaTool => ToolResolver.GetTool(GnExe);
    Tool PythonTool => ToolResolver.GetEnvironmentOrPathTool("python");

    void BuildHarfbuzzWindows(string arch, string dir)
    {
        // TODO
    }
}