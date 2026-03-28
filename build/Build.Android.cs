using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

partial class Build
{
    [Parameter] readonly string NdkPath = GetVariable<string>("ANDROID_NDK_HOME") ?? FindNdk();

    // Google Play Store requires 16KB page alignment for Android 15+ (API 35+)
    // https://developer.android.com/guide/practices/page-sizes
    const int AndroidRequiredLoadSegmentAlignment = 0x4000;

    [PublicAPI]
    public Target CheckAndroidAlignment => t => t
        .After(LibAlphaSkia)
        .OnlyWhenStatic(() => TargetOs == TargetOperatingSystem.Android)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .Executes(() =>
        {
            if (!Variant.IsShared)
            {
                Log.Information("Skipping alignment check for static variant {Variant}", Variant);
                return;
            }

            var llvmReadElf = GetNdkLlvmReadElf();
            if (!llvmReadElf.FileExists())
            {
                throw new FileNotFoundException($"llvm-readelf not found at {llvmReadElf}");
            }

            var readElfTool = ToolResolver.GetTool(llvmReadElf);

            var libDir = GetLibDirectory(GetAlphaSkiaLibName(Variant), variant: Variant);
            var distPath = DistBasePath / libDir;
            var soFiles = distPath.GlobFiles("*.so").ToArray();

            if (soFiles.Length == 0)
            {
                throw new IOException($"No .so files found in {distPath} to check alignment");
            }

            var failures = new List<string>();

            foreach (var soFile in soFiles)
            {
                Log.Information("Checking 16KB alignment of {File}", soFile.Name);
                var output = readElfTool($"-l {soFile}", logOutput: false);
                var stdOutput = string.Join("\n",
                    output.Where(o => o.Type == OutputType.Std).Select(o => o.Text));

                var loadAlignments = ParseLoadSegmentAlignments(stdOutput);
                if (loadAlignments.Count == 0)
                {
                    failures.Add($"{soFile.Name}: no PT_LOAD segments found");
                    continue;
                }

                foreach (var (index, alignment) in loadAlignments)
                {
                    if (alignment < AndroidRequiredLoadSegmentAlignment)
                    {
                        failures.Add(
                            $"{soFile.Name}: PT_LOAD[{index}] alignment is 0x{alignment:X}" +
                            $" (expected >= 0x{AndroidRequiredLoadSegmentAlignment:X})");
                    }
                    else
                    {
                        Log.Debug("  PT_LOAD[{Index}]: 0x{Alignment:X} OK", index, alignment);
                    }
                }
            }

            if (failures.Count > 0)
            {
                foreach (var failure in failures)
                {
                    Log.Error("Alignment failure: {Failure}", failure);
                }

                throw new InvalidOperationException(
                    $"{failures.Count} Android library alignment issue(s) found. " +
                    $"Google Play Store requires 16KB page alignment (0x{AndroidRequiredLoadSegmentAlignment:X}) for Android 15+. " +
                    "See https://developer.android.com/guide/practices/page-sizes");
            }

            Log.Information("All Android libraries pass 16KB page alignment check");
        });

    AbsolutePath GetNdkLlvmReadElf()
    {
        var prebuiltBase = (AbsolutePath)NdkPath / "toolchains" / "llvm" / "prebuilt";
        var hostDir = prebuiltBase.GetDirectories().FirstOrDefault()
                      ?? throw new DirectoryNotFoundException(
                          $"No prebuilt toolchain directory found under {prebuiltBase}");
        return hostDir / "bin" / $"llvm-readelf{ExeExtension}";
    }

    static List<(int Index, long Alignment)> ParseLoadSegmentAlignments(string readElfOutput)
    {
        var result = new List<(int Index, long Alignment)>();
        var index = 0;
        foreach (var line in readElfOutput.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("LOAD", StringComparison.Ordinal))
            {
                continue;
            }

            // The last whitespace-separated token on a LOAD line is the alignment value (e.g. 0x4000)
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var alignStr = parts[^1];
            if (alignStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                long.TryParse(alignStr[2..], NumberStyles.HexNumber, null, out var alignment))
            {
                result.Add((index, alignment));
            }

            index++;
        }

        return result;
    }

    static string FindNdk()
    {
        List<string> candidates;
        if (OperatingSystem.IsWindows())
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            candidates =
            [
                Path.Combine(localAppData, "Android", "Sdk", "ndk"),
                Path.Combine(localAppData, "Android", "Ndk")
            ];
        }
        else if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidates =
            [
                Path.Combine(home, "Library", "Android", "sdk", "ndk")
            ];
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidates =
            [
                Path.Combine(home, "Android", "Sdk", "ndk")
            ];
        }

        return candidates.Select(c =>
        {
            if (!Directory.Exists(c))
            {
                return null;
            }

            if (File.Exists(Path.Combine(c, "package.xml")))
            {
                return c;
            }

            return Directory.EnumerateDirectories(c)
                .Where(subDir => File.Exists(Path.Combine(subDir, "package.xml")))
                .OrderByDescending(subDir =>
                {
                    var name = Path.GetFileName(subDir);
                    return Version.TryParse(name, out var v) ? v : new Version(0, 0);
                })
                .FirstOrDefault();
        }).FirstOrDefault(d => d != null) ?? string.Empty;
    }
}