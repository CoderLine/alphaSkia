using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using static Nuke.Common.EnvironmentInfo;

[TypeConverter(typeof(TypeConverter<Architecture>))]
public class Architecture : Enumeration
{
    public static Architecture X64 = new() { Value = "x64"};
    public static Architecture X86 = new() { Value = "x86" };
    public static Architecture Arm64 = new() { Value = "arm64" };
}

[TypeConverter(typeof(TypeConverter<Variant>))]
public class Variant : Enumeration
{
    public static Variant Static = new() { Value = "static" };
    public static Variant Shared = new() { Value = "shared" };
}

partial class Build : NukeBuild
{
    // Path handling
    static readonly string ExeExtension = OperatingSystem.IsWindows() ? ".exe" : "";
    static readonly string ScriptExtension = OperatingSystem.IsWindows() ? ".bat" : "";

    [Parameter] static AbsolutePath SkiaPath = RootDirectory / "externals" / "skia";
    [Parameter] static AbsolutePath DepotPath = RootDirectory / "externals" / "depot_tools";

    public static int Main() => Execute<Build>();

    // Compiler Option
    [Parameter]
    readonly string LlvmHome = GetVariable<string>("LLVM_HOME") ??
                               (OperatingSystem.IsAndroid() ? "C:/Program Files/LLVM" : "");

    // Tools
    [Parameter] readonly AbsolutePath GnExe = GetVariable<string>("GN_EXE") ?? SkiaPath / "bin" / $"gn{ExeExtension}";
    Tool GnTool => ToolResolver.GetTool(GnExe);

    [Parameter]
    readonly AbsolutePath NinjaExe = GetVariable<string>("NINJA_EXE") ?? DepotPath / $"ninja{ScriptExtension}";

    Tool NinjaTool => ToolResolver.GetTool(NinjaExe);

    [Parameter] readonly string PythonExe = GetVariable<string>("PYTHON_EXE") ?? "python3";
    Tool PythonTool => File.Exists(PythonExe) ? ToolResolver.GetTool(PythonExe) : ToolResolver.GetPathTool("python3");

    // Output Options
    [Parameter] readonly Architecture Architecture;

    [Parameter] readonly Variant Variant;


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
            Environment.SetEnvironmentVariable("PATH", newValue, EnvironmentVariableTarget.Process);

            PythonTool(
                arguments: (SkiaPath / "bin" / "fetch-ninja").ToString(),
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

            PatchSkiaFile(SkiaPath / "gn" / "BUILDCONFIG.gn", buildConfigNew.ToString());

            var buildNew = new StringBuilder();
            buildNew.AppendLine("alphaskia_build(\"libAlphaSkia\") {");
            buildNew.AppendLine("  public_configs = [ \":skia_public\" ]");
            buildNew.AppendLine("  configs += skia_library_configs");
            buildNew.AppendLine();
            buildNew.AppendLine(
                "  defines = [ \"ALPHASKIA_DLL\", \"_SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING\" ]");
            buildNew.AppendLine();
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
            buildNew.AppendLine();
            buildNew.AppendLine("  libs = []");
            buildNew.AppendLine("}");
            PatchSkiaFile(SkiaPath / "BUILD.gn", buildNew.ToString());
            
            // Bug in skia toolchain, setenv.cmd is not existing in this path. 
            var toolChainBuildFile = SkiaPath /"gn" / "toolchain" / "BUILD.gn";
            var oldToolChainSource = toolChainBuildFile.ReadAllText();
            toolChainBuildFile.WriteAllText(oldToolChainSource.Replace(
                "env_setup = \"$shell $win_sdk/bin/SetEnv.cmd /x86",
                "# env_setup = \"$shell $win_sdk/bin/SetEnv.cmd /x86"
            ));
        });

    public Target PrepareBuild => _ => _
        .DependsOn(SetupDepotTools, GitSyncDeps, PatchSkiaBuildFiles);

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
        
        var allArgs = string.Join(" ", gnArgs.Select(o =>  $"{o.Key}={QuoteValue(o.Value)}"));

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

    void BuildSkia(string targetOs,
        string arch, Variant variant, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        var isShared = variant == Variant.Shared;

        gnArgs["target_os"] = targetOs;
        gnArgs["target_cpu"] = arch;
        gnArgs["is_shared_alphaskia"] = isShared.ToString().ToLowerInvariant();
        gnArgs["skia_use_icu"] = "false";
        gnArgs["skia_use_piex"] = "true";
        gnArgs["skia_use_sfntly"] = "false";
        gnArgs["skia_use_system_expat"] = "false";
        gnArgs["skia_use_system_libjpeg_turbo"] = "false";
        gnArgs["skia_use_system_libpng"] = "false";
        gnArgs["skia_use_system_libwebp"] = "false";
        gnArgs["skia_use_system_zlib"] = "false";
        gnArgs["skia_use_system_harfbuzz"] = "false";
        gnArgs["skia_enable_skshaper"] = "true";
        gnArgs["skia_pdf_subset_harfbuzz"] = "false";
        gnArgs["skia_use_vulkan"] = "true";
        gnArgs["skia_use_expat"] = "false";
        gnArgs["skia_enable_pdf"] = "false";

        GnNinja($"out/{targetOs}/{arch}/{variant}", "libAlphaSkia", gnArgs, SkiaPath);
        
        
        var finalPath = RootDirectory / "dist" / $"{targetOs}-{arch}-{variant}";

        foreach (var file in filesToCopy)
        {
            FileSystemTasks.CopyFile(SkiaPath / "out" / targetOs / arch / variant / file,
                finalPath / file, FileExistsPolicy.Overwrite);
        }

        FileSystemTasks.CopyFile(RootDirectory / "wrapper" / "include" / "libAlphaSkia.h",
            RootDirectory / "dist" / "include" / "libAlphaSkia.h", FileExistsPolicy.OverwriteIfNewer);
    }
}