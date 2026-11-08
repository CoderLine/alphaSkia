using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Serilog;
using Serilog.Core;

partial class Build
{
    private static readonly string[] RequiredDependencies =
    [
        "buildtools",
        "third_party/externals/harfbuzz"
    ];

    [PublicAPI]
    public Target LibAlphaSkiaGitSyncDeps => t => t
        .Unlisted()
        .DependsOn(SetupDepotTools)
        .Executes(() => GitSyncDepsCustom(RequiredDependencies));

    [PublicAPI]
    public Target LibAlphaSkia => t => t
        .DependsOn(PrepareGitHubArtifacts, LibAlphaSkiaGitSyncDeps, LibAlphaSkiaPrepareBuildFiles,
            InstallDependenciesLinux)
        .After(LibSkia)
        .Requires(() => Architecture)
        .Requires(() => TargetOs)
        .Executes(BuildAlphaSkia);

    [PublicAPI]
    public Target LibAlphaSkiaTest => t => t
        .DependsOn(PrepareGitHubArtifacts, LibAlphaSkiaGitSyncDeps, LibAlphaSkiaPrepareBuildFiles,
            InstallDependenciesLinux)
        .After(LibAlphaSkia)
        .Requires(() => Architecture)
        .Requires(() => TargetOs)
        .OnlyWhenDynamic(() => AlphaSkiaVariants.Contains(Variant.Shared))
        .Executes(BuildAlphaSkiaTest);

    [PublicAPI]
    public Target LibAlphaSkiaPrepareBuildFiles => t => t
        .Unlisted()
        .Executes(() =>
        {
            // Our targets live in their own build file within the Skia tree, this way they get the compile settings
            // and platform support of Skia without changes to Skia's build files.
            (RootDirectory / "build" / "gn" / "alphaskia" / "BUILD.gn").Copy(
                SkiaPath / "alphaskia" / "BUILD.gn", ExistsPolicy.FileOverwrite);

            // checkouts built by earlier versions of this build still contain our targets in Skia's build files
            RemoveSkiaFilePatch(SkiaPath / "gn" / "BUILDCONFIG.gn", "BuildConfig");
            RemoveSkiaFilePatch(SkiaPath / "BUILD.gn", "Build");

            PatchSkiaToolchain();
            PatchLlvm();
        });

    static void RemoveSkiaFilePatch(AbsolutePath file, string patchName)
    {
        var text = file.ReadAllText();
        var startMarker = "# AlphaSkia Patch Start - " + patchName;
        var start = text.IndexOf(startMarker, StringComparison.Ordinal);
        // "Build" is a prefix of "BuildConfig"
        while (start != -1 && text.Length > start + startMarker.Length &&
               text[start + startMarker.Length] is not ('\r' or '\n'))
        {
            start = text.IndexOf(startMarker, start + startMarker.Length, StringComparison.Ordinal);
        }

        if (start == -1)
        {
            return;
        }

        var endMarker = "# AlphaSkia Patch End - " + patchName;
        var end = text.IndexOf(endMarker, start, StringComparison.Ordinal);
        if (end == -1)
        {
            throw new IOException($"{file} contains an incomplete alphaSkia patch, restore the file from Git");
        }

        file.WriteAllText(text[..start].TrimEnd() + "\n" + text[(end + endMarker.Length)..].TrimStart());
    }

    const string AlphaSkiaRootTarget = "//alphaskia";

    string AlphaSkiaOutDir => $"out/libalphaskia-{TargetOs.RuntimeIdentifier}-{Architecture}";

    AbsolutePath AlphaSkiaRcObjDir => SkiaPath / AlphaSkiaOutDir / "obj" / "alphaskia_rc";

    /// <summary>
    /// The GN args of all alphaSkia targets, identical for the libraries and the tests so the build is only
    /// generated once.
    /// </summary>
    Dictionary<string, string> PrepareAlphaSkiaBuild()
    {
        var gnArgs = PrepareNativeBuild();
        gnArgs["alphaskia_skia_lib_dir"] = ToGnPath(DistBasePath / GetLibDirectory("libskia", Variant.Static));
        gnArgs["alphaskia_test_rid"] = TargetOs.RuntimeIdentifier;

        if (AlphaSkiaVariants.Contains(Variant.Jni))
        {
            gnArgs["alphaskia_jni_include_dirs"] = GetJniIncludeDirs();
        }

        if (AlphaSkiaVariants.Contains(Variant.Node))
        {
            if (OperatingSystem.IsWindows() && TargetOs == TargetOperatingSystem.Windows)
            {
                // windows requires a lib to link against, fetch it from the node downloads
                var nodeLibPath = DownloadNodeLib();
                gnArgs["alphaskia_node_ldflags"] =
                    $"[ '/DELAYLOAD:node.exe', '/LIBPATH:{nodeLibPath}', 'node.lib', 'DelayImp.lib' ]";
            }
            else if (OperatingSystem.IsMacOS() && TargetOs == TargetOperatingSystem.MacOs)
            {
                // the N-API symbols are resolved at runtime from the node binary, there is no libnode to link
                gnArgs["alphaskia_node_ldflags"] = "[ '-undefined', 'dynamic_lookup' ]";
            }
        }

        if (TargetOs == TargetOperatingSystem.Windows)
        {
            gnArgs["alphaskia_rc_obj_dir"] = ToGnPath(AlphaSkiaRcObjDir);
        }

        return gnArgs;
    }

    /// <summary>
    /// A system-absolute path as GN expects it in lib_dirs and include_dirs ("/C:/dir" on Windows).
    /// </summary>
    static string ToGnPath(AbsolutePath path)
    {
        var gnPath = path.ToString().Replace('\\', '/');
        return gnPath.Length > 1 && gnPath[1] == ':' ? "/" + gnPath : gnPath;
    }

    string GetJniIncludeDirs()
    {
        if (TargetOs == TargetOperatingSystem.Android)
        {
            // provided by the NDK
            return "[]";
        }

        var jniInclude = JavaHome / "include";
        AbsolutePath jniPlatformInclude;
        if (OperatingSystem.IsWindows() && TargetOs == TargetOperatingSystem.Windows)
        {
            jniPlatformInclude = jniInclude / "win32";
        }
        else if (OperatingSystem.IsLinux() && TargetOs == TargetOperatingSystem.Linux)
        {
            jniPlatformInclude = jniInclude / "linux";
        }
        else if (OperatingSystem.IsMacOS() && TargetOs == TargetOperatingSystem.MacOs)
        {
            jniPlatformInclude = jniInclude / "darwin";
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        return $"[ '{ToGnPath(jniInclude)}', '{ToGnPath(jniPlatformInclude)}' ]";
    }

    void BuildAlphaSkia()
    {
        SyncAlphaSkiaBaseVersion();
        var gnArgs = PrepareAlphaSkiaBuild();
        var variants = AlphaSkiaVariants;
        var targets = string.Join(" ", variants.Select(v => $"alphaskia:{GetAlphaSkiaLibName(v)}"));

        GnNinja(AlphaSkiaOutDir, targets, gnArgs, new Dictionary<string, string>(), SkiaPath,
            () =>
            {
                if (OperatingSystem.IsWindows() && TargetOs == TargetOperatingSystem.Windows)
                {
                    // version resources, linked as "library" <target>.rc.obj by the BUILD.gn
                    // "llvm-rc.exe" /FO alphaskia.rc.obj alphaskia.rc /D RC_INVOKED /C 65001
                    var input = RootDirectory / "wrapper" / "src" / "alphaskia.rc";
                    AlphaSkiaRcObjDir.CreateDirectory();
                    foreach (var variant in variants)
                    {
                        NativeWriteVersionInfoHeader(variant);
                        var output = AlphaSkiaRcObjDir / $"{GetAlphaSkiaLibName(variant)}.rc.obj";
                        const int utf8CodePage = 65001;
                        ToolResolver.GetTool((AbsolutePath)LlvmHome / "bin" / "llvm-rc.exe")(
                            $"/FO {output} {input} /D RC_INVOKED /C {utf8CodePage}",
                            workingDirectory: SkiaPath);
                    }
                }
            },
            AlphaSkiaRootTarget);

        var outDir = SkiaPath / AlphaSkiaOutDir;
        foreach (var variant in variants)
        {
            var libName = GetAlphaSkiaLibName(variant);
            var libDir = GetLibDirectory(libName, variant);
            var libExtensions = new HashSet<string>(GetLibExtensions(variant), StringComparer.OrdinalIgnoreCase);
            // all variants share the output directory, each one only takes the files of its own library
            var libFiles = outDir.GetFiles()
                .Where(f => f.Name.StartsWith(libName + ".", StringComparison.OrdinalIgnoreCase) &&
                            libExtensions.Contains(f.Extension))
                .ToArray();
            if (libFiles.Length == 0)
            {
                throw new IOException($"No {libName} files found in {outDir}");
            }

            foreach (var file in libFiles)
            {
                file.Copy(DistBasePath / libDir / file.Name, ExistsPolicy.FileOverwrite);
                if (IsGitHubActions)
                {
                    // one artifact root per variant, the workflow uploads each one as separate artifact
                    file.Copy(ArtifactBasePath / variant.ToString() / libDir / file.Name, ExistsPolicy.FileOverwrite);
                }
            }
        }

        (RootDirectory / "wrapper" / "include" / "alphaskia.h").Copy(
            DistBasePath / "include" / "alphaskia" / "alphaskia.h",
            ExistsPolicy.FileOverwrite
        );
    }

    void BuildAlphaSkiaTest()
    {
        var buildTarget = "libalphaskiatest";
        GnNinja(AlphaSkiaOutDir, $"alphaskia:{buildTarget}", PrepareAlphaSkiaBuild(),
            new Dictionary<string, string>(), SkiaPath, rootTarget: AlphaSkiaRootTarget);

        // copy for artifacts
        var outDir = SkiaPath / AlphaSkiaOutDir;
        var exePath = outDir / (buildTarget + GetExeExtension());
        exePath.Copy(DistBasePath / GetLibDirectory(buildTarget, Variant.Shared) / exePath.Name,
            ExistsPolicy.FileOverwrite);

        // run executable, the shared library is next to it in the output directory
        if (LibAlphaSkiaCanRunTests)
        {
            Log.Information(
                $"Running {TargetOs.RuntimeIdentifier}-{Architecture} tests on {TargetOperatingSystem.Current.RuntimeIdentifier}-{Architecture.Current} host system (OS fonts)");
            ToolResolver.GetTool(exePath)(
                "",
                workingDirectory: outDir
            );
            Log.Information(
                $"Running {TargetOs.RuntimeIdentifier}-{Architecture} tests on {TargetOperatingSystem.Current.RuntimeIdentifier}-{Architecture.Current} host system (FreeType fonts)");
            ToolResolver.GetTool(exePath)(
                "--freetype",
                workingDirectory: outDir
            );
        }
        else
        {
            Log.Information(
                $"Skipping test execution, cannot run {TargetOs.RuntimeIdentifier}-{Architecture} tests on {TargetOperatingSystem.Current.RuntimeIdentifier}-{Architecture.Current} host system");
        }
    }

    bool LibAlphaSkiaCanRunTests
    {
        get
        {
            if (TargetOs == TargetOperatingSystem.Current)
            {
                // If arch and OS match we can definitely run
                if (Architecture == Architecture.Current)
                {
                    return true;
                }

                // x64 can run x86 processes
                if (Architecture == Architecture.X86 && Architecture.Current == Architecture.X64)
                {
                    return true;
                }
            }

            return false;
        }
    }

    void NativeWriteVersionInfoHeader(Variant variant)
    {
        var libExtension = GetLibExtensions(variant)[0];

        var versionInfo = new StringBuilder();
        versionInfo.AppendLine("#pragma once");
        versionInfo.AppendLine("");
        versionInfo.AppendLine(
            $"#define VER_FILEVERSION {VersionInfo.FileVersion.Major},{VersionInfo.FileVersion.Minor},{VersionInfo.FileVersion.Build},{VersionInfo.FileVersion.Revision}");
        versionInfo.AppendLine(
            $"#define VER_FILEVERSION_STR \"{VersionInfo.FileVersion.Major}.{VersionInfo.FileVersion.Minor}.{VersionInfo.FileVersion.Build}.{VersionInfo.FileVersion.Revision}\\0\"");

        versionInfo.AppendLine(
            $"#define VER_PRODUCTVERSION {VersionInfo.FileVersion.Major},{VersionInfo.FileVersion.Minor},0,0");
        versionInfo.AppendLine(
            $"#define VER_PRODUCTVERSION_STR \"{VersionInfo.FileVersion.Major}.{VersionInfo.FileVersion.Minor}\\0\"");
        if (IsLocalBuild)
        {
            versionInfo.AppendLine("#define IS_LOCAL_BUILD 1");
        }
        else if (IsReleaseBuild)
        {
            versionInfo.AppendLine("#define IS_RELEASE_BUILD 1");
        }

        versionInfo.AppendLine($"#define VER_COMPANY_STR \"{VersionInfo.Company}\"");
        versionInfo.AppendLine($"#define VER_FILE_DESCRIPTION_STR \"{VersionInfo.Description}\"");

        if (variant == Variant.Shared)
        {
            versionInfo.AppendLine("#define VER_INTERNALNAME_STR \"libalphaskia\"");
            versionInfo.AppendLine($"#define VER_ORIGINALFILENAME_STR \"libalphaskia{libExtension}\"");
        }
        else if (variant == Variant.Jni)
        {
            versionInfo.AppendLine("#define VER_INTERNALNAME_STR \"libalphaskiajni\"");
            versionInfo.AppendLine($"#define VER_ORIGINALFILENAME_STR \"libalphaskiajni{libExtension}\"");
        }
        else if (variant == Variant.Node)
        {
            versionInfo.AppendLine("#define VER_INTERNALNAME_STR \"libalphaskianode\"");
            versionInfo.AppendLine($"#define VER_ORIGINALFILENAME_STR \"libalphaskianode{libExtension}\"");
        }

        versionInfo.AppendLine($"#define VER_LEGALCOPYRIGHT_STR \"{VersionInfo.Copyright}\"");
        versionInfo.AppendLine("#define VER_LEGALTRADEMARKS1_STR \"\"");
        versionInfo.AppendLine("#define VER_LEGALTRADEMARKS2_STR \"\"");
        versionInfo.AppendLine($"#define VER_PRODUCTNAME_STR \"{VersionInfo.Copyright}\"");

        var dir = RootDirectory / "wrapper" / "include" / "generated";
        dir.CreateDirectory();
        // NOTE: need guaranteed UTF-8 code page in this file
        (dir / "version_info.h").WriteAllBytes(Encoding.UTF8.GetBytes(versionInfo.ToString()));
    }

    AbsolutePath DownloadNodeLib()
    {
        if (OperatingSystem.IsWindows() && TargetOs == TargetOperatingSystem.Windows)
        {
            // libs are available at urls like: 
            // https://nodejs.org/dist/latest/win-x64/node.lib
            // https://nodejs.org/dist/latest/win-x86/node.lib
            // https://nodejs.org/dist/latest/win-arm64/node.lib
            // NOTE: Node v23 has no win-x86 either, remove in future: https://github.com/nodejs/node/issues/42543
            var platform = $"{TargetOs.RuntimeIdentifier}-{Architecture}";
            var url = $"https://nodejs.org/dist/v22.9.0/{platform}/node.lib";
            var libDir = TemporaryDirectory / $"libnode-{platform}";
            var nodeLib = libDir / "node.lib";
            HttpTasks.HttpDownloadFile(url, nodeLib);

            // from https://nodejs.org/dist/v22.9.0/SHASUMS256.txt, the import library is linked into the Node addon
            var expectedSha256 = platform switch
            {
                "win-x64" => "3581a06b68c4584d146372113eaa8c4d102127222e5041195ba38f185eef419c",
                "win-x86" => "45399070d1d247cf223d12e80d3e638635af24d2f7a4714bc8e38a6a918f162a",
                "win-arm64" => "ad65afe5b192644fec9d599c77f0e38a8421d0d7ad2389679882a288c8df444b",
                _ => throw new InvalidOperationException($"No known node.lib checksum for {platform}")
            };
            var actualSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(nodeLib))).ToLowerInvariant();
            if (actualSha256 != expectedSha256)
            {
                nodeLib.DeleteFile();
                throw new InvalidOperationException($"Checksum mismatch for {url}: expected {expectedSha256}, got {actualSha256}");
            }

            return libDir;
        }

        return null;
    }

    /// <summary>
    /// Syncs the main Major.Minor.Patch version into all relevant files to avoid inconsistencies
    /// between GitHub and locally.
    /// </summary>
    void SyncAlphaSkiaBaseVersion()
    {
        var baseVersion = VersionInfo.FileVersion.ToString(3);
        Log.Information("Syncing alphaSkia version to {Version}", baseVersion);
        AbsolutePath[] dotNetProps =
        [
            RootDirectory / "lib" / "dotnet" / "Directory.Build.props"
        ];
        foreach (var dotNetProp in dotNetProps)
        {
            dotNetProp.WriteAllText(
                dotNetProp.ReadAllText()
                    .ReplaceRegex(
                        @"<Version([^>]+)>[0-9]+\.[0-9]+\.[0-9]+</Version>",
                        m => $"<Version{m.Groups[1].Value}>{baseVersion}</Version>"
                    )
            );
        }

        AbsolutePath[] buildGradles =
        [
            RootDirectory / "lib" / "java" / "build.gradle.kts",
            RootDirectory / "test" / "java" / "build.gradle.kts"
        ];
        foreach (var buildGradle in buildGradles)
        {
            buildGradle.WriteAllText(
                buildGradle.ReadAllText()
                    .ReplaceRegex(
                        """
                        var libVersion = "[^"]+"
                        """,
                        _ => $"var libVersion = \"{baseVersion}\""
                    )
            );
        }


        AbsolutePath[] packageJsons =
        [
            RootDirectory / "lib" / "node" / "alphaskia" / "package.json",
            RootDirectory / "lib" / "node" / "alphaskia-linux" / "package.json",
            RootDirectory / "lib" / "node" / "alphaskia-macos" / "package.json",
            RootDirectory / "lib" / "node" / "alphaskia-windows" / "package.json",
            RootDirectory / "test" / "node" / "package.json"
        ];
        foreach (var packageJson in packageJsons)
        {
            packageJson.WriteAllText(
                packageJson.ReadAllText()
                    .ReplaceRegex(
                        """
                        "version": "[^"]+"
                        """,
                        _ =>
                            $"""
                             "version": "{baseVersion}"
                             """
                    )
            );
        }
    }
}