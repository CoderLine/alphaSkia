using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        .DependsOn(PrepareGitHubArtifacts, LibAlphaSkiaGitSyncDeps, LibAlphaSkiaPatchSkiaBuildFiles,
            InstallDependenciesLinux)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .Requires(() => TargetOs)
        .Executes(BuildAlphaSkia);

    [PublicAPI]
    public Target LibAlphaSkiaTest => t => t
        .DependsOn(PrepareGitHubArtifacts, LibAlphaSkiaGitSyncDeps, LibAlphaSkiaPatchSkiaBuildFiles,
            InstallDependenciesLinux)
        .After(LibAlphaSkia)
        .Requires(() => Architecture)
        .OnlyWhenStatic(() => Variant == Variant.Shared)
        .Requires(() => TargetOs)
        .Executes(BuildAlphaSkiaTest);

    [PublicAPI]
    public Target LibAlphaSkiaPatchSkiaBuildFiles => t => t
        .Unlisted()
        .Executes(() =>
        {
            // it is a lot easier to inject a bit of custom GN code into the BUILD.gn of skia
            // and build through that, instead of setting up the whole configuration on our own. 
            // this way we get the same compile settings and platform support
            const string buildConfigNew = """
                                              declare_args() {
                                                  is_shared_alphaskia = true
                                              }
                                              template("alphaskia_build") {
                                                  _alphaskia_mode = "shared_library"
                                                  if (!is_shared_alphaskia) {
                                                      _alphaskia_mode = "static_library"
                                                  }
                                              
                                                  target(_alphaskia_mode, target_name) {
                                                      forward_variables_from(invoker, "*")
                                                  }
                                              }
                                              template("alphaskia_executable") {
                                                  _alphaskia_mode = "executable"
                                                  target(_alphaskia_mode, target_name) {
                                                      forward_variables_from(invoker, "*")
                                                  }
                                              }
                                              set_defaults("alphaskia_build") {
                                                configs = default_configs
                                                if (!is_shared_alphaskia) {
                                                    complete_static_lib = true
                                                }
                                              }
                                          """;
            PatchSkiaFile(SkiaPath / "gn" / "BUILDCONFIG.gn", buildConfigNew, "BuildConfig", "#");

            const string buildNew = """
                                        alphaskia_wrapper_sources = [
                                            "../../wrapper/src/AlphaSkiaCanvas.cpp",
                                            "../../wrapper/src/alphaskia_canvas.cpp",
                                            "../../wrapper/src/alphaskia_image.cpp",
                                            "../../wrapper/src/alphaskia_typeface.cpp",
                                            "../../wrapper/src/alphaskia_text_style.cpp",
                                            "../../wrapper/src/alphaskia_text_metrics.cpp",
                                            "../../wrapper/src/alphaskia_data.cpp",
                                            "../../wrapper/src/alphaskia_string.cpp"
                                        ]
                                        if (is_win) {
                                            alphaskia_wrapper_sources += [ "../../wrapper/src/alphaskia.rc" ]
                                        }
                                        config("alphaskia_public") {
                                          defines = [ "_SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING", "ALPHASKIA_IMPLEMENTATION=1" ]
                                          include_dirs = [ "." ]
                                          
                                    
                                          if (is_shared_alphaskia) {
                                            defines += [ "ALPHASKIA_DLL" ]
                                          }
                                          
                                          if (is_win) {
                                            libs = [ "skia.lib", "user32.lib", "OpenGL32.lib", "alphaskia.rc.obj" ]
                                          }
                                          
                                          if (is_linux) {
                                            libs = [ "skia", "fontconfig" ]
                                          }
                                          
                                          if (is_android) {
                                            libs = [ "skia" ]
                                          }
                                          
                                          if (is_mac) {
                                            libs = [ "skia" ]
                                              
                                            frameworks = [
                                              "AppKit.framework",
                                              "ApplicationServices.framework",
                                    
                                              "OpenGL.framework",
                                              
                                              "Metal.framework",
                                              "Foundation.framework"
                                            ]   
                                          }
                                          
                                          if (is_ios) {
                                            libs = [ "skia" ]
                                              
                                            frameworks = [
                                              "Foundation.framework",
                                              "CoreFoundation.framework",
                                              "CoreGraphics.framework",
                                              "CoreText.framework",
                                              "ImageIO.framework",
                                              "MobileCoreServices.framework",
                                    
                                              "Metal.framework",
                                              "UIKit.framework"
                                            ]
                                          }
                                        }
                                    
                                        alphaskia_build("libalphaskia") {
                                          public_configs = [ ":alphaskia_public" ]
                                          configs += [ ":alphaskia_public" ]
                                          sources = alphaskia_wrapper_sources
                                        }
                                        alphaskia_build("libalphaskiajni") {
                                          public_configs = [ ":alphaskia_public" ]
                                          configs += [ ":alphaskia_public" ]
                                          sources = alphaskia_wrapper_sources
                                          sources += [
                                            "../../lib/java/jni/src/AlphaSkiaCanvas.cpp",
                                            "../../lib/java/jni/src/AlphaSkiaData.cpp",
                                            "../../lib/java/jni/src/AlphaSkiaImage.cpp",
                                            "../../lib/java/jni/src/AlphaSkiaTextStyle.cpp",
                                            "../../lib/java/jni/src/AlphaSkiaTextMetrics.cpp",
                                            "../../lib/java/jni/src/AlphaSkiaTypeface.cpp"
                                          ]
                                        }
                                        alphaskia_build("libalphaskianode") {
                                          public_configs = [ ":alphaskia_public" ]
                                          configs += [ ":alphaskia_public" ]
                                          defines = [ "NODE_GYP_MODULE_NAME=libalphaskianode", "USING_UV_SHARED=1", "USING_V8_SHARED=1", "V8_DEPRECATION_WARNINGS=1", "BUILDING_NODE_EXTENSION" ]
                                          sources = alphaskia_wrapper_sources
                                          output_extension = "node"
                                          sources += [
                                            "../../lib/node/addon/addon.cpp"
                                          ]
                                          if( is_win ) {
                                            sources += [ "../../lib/node/addon/win_delay_load_hook.cpp"]
                                          }
                                        }
                                    
                                        alphaskia_executable("libalphaskiatest") {
                                          sources = [
                                              "../../test/native/src/AlphaSkiaTestBridge.cpp",
                                              "../../test/native/src/AlphaTabGeneratedTest.cpp",
                                              "../../test/native/src/main.cpp",
                                              "../../test/native/src/PixelMatch.cpp"
                                          ]
                                          if (is_win) {
                                            libs = [ "libalphaskia.dll.lib" ]
                                          }
                                          else {
                                            libs = [ "alphaskia" ]
                                          }
                                        }
                                    """;
            PatchSkiaFile(SkiaPath / "BUILD.gn", buildNew, "Build", "#");
            PatchSkiaToolchain();
            PatchSkiaMacOsVersion();
        });


    void PatchSkiaFile(AbsolutePath file, string newText, string patchName, string patchCommentPrefix,
        Func<string, int> findInsertOffset = null)
    {
        var existingText = file.ReadAllText();
        string startMarker = patchCommentPrefix + " AlphaSkia Patch Start - " + patchName;
        string endMarker = patchCommentPrefix + " AlphaSkia Patch End - " + patchName;
        var newTextWithMarker = new StringBuilder();
        newTextWithMarker.AppendLine();
        newTextWithMarker.AppendLine(startMarker);
        newTextWithMarker.AppendLine(newText);
        newTextWithMarker.AppendLine(endMarker);

        var beforePatchIndex = existingText.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (beforePatchIndex == -1 && findInsertOffset != null)
        {
            beforePatchIndex = findInsertOffset.Invoke(existingText);
        }

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
                // corrupt patch (no end) or custom insert position
                file.WriteAllText(existingText[..beforePatchIndex].TrimEnd() + newTextWithMarker +
                                  existingText[beforePatchIndex..]);
            }
            else
            {
                // already patched
                file.WriteAllText(existingText[..beforePatchIndex].TrimEnd() + newTextWithMarker +
                                  existingText[(afterPatchIndex + endMarker.Length)..].TrimStart());
            }
        }
    }

    void BuildAlphaSkia()
    {
        SyncAlphaSkiaBaseVersion();
        var gnArgs = PrepareNativeBuild(Variant);
        var staticLibPath = DistBasePath / GetLibDirectory(variant: Variant.Static);
        var gnFlags = new Dictionary<string, string>();

        string buildTarget;
        if (Variant == Variant.Static)
        {
            buildTarget = "libalphaskia";
        }
        else if (Variant == Variant.Shared)
        {
            buildTarget = "libalphaskia";
        }
        else if (Variant == Variant.Jni)
        {
            buildTarget = "libalphaskiajni";

            var alphaSkiaInclude = DistBasePath / "include";
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
            else if (TargetOs == TargetOperatingSystem.Android)
            {
                jniPlatformInclude = null;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            AppendToFlagList(gnArgs, "extra_cflags",
                jniPlatformInclude != null
                    ? $"'-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniPlatformInclude}'"
                    : $"'-I{alphaSkiaInclude}'"
            );
        }
        else if (Variant == Variant.Node)
        {
            buildTarget = "libalphaskianode";

            if (OperatingSystem.IsWindows())
            {
                // windows requires a lib to link against, fetch it from the node downloads
                var nodeLibPath = DownloadNodeLib();
                AppendToFlagList(gnArgs, "extra_ldflags",
                    $"'/DELAYLOAD:node.exe', '/LIBPATH:{nodeLibPath}', 'node.lib', 'DelayImp.lib'");
            }
            else if (OperatingSystem.IsMacOS() && TargetOs == TargetOperatingSystem.MacOs)
            {
                // disable need of a libnode.dylib dependencies are resolve dynamically during runtime 
                // and as the node binary has them built-in
                AppendToFlagList(gnArgs, "extra_ldflags", "'-undefined', 'dynamic_lookup'");
            }
        }
        else
        {
            throw new ArgumentException("Unknown variant: " + Variant);
        }

        if (TargetOs == TargetOperatingSystem.Windows)
        {
            // TODO: check if clang-cl also works with the linux flags
            AppendToFlagList(gnArgs, "extra_ldflags", $"'/LIBPATH:{staticLibPath}'");
            NativeWriteVersionInfoHeader();
        }
        else
        {
            AppendToFlagList(gnArgs, "extra_ldflags", $"'-L{staticLibPath}'");
        }

        var libDir = GetLibDirectory(buildTarget, TargetOs, Architecture, Variant);
        var artifactsLibPath = IsGitHubActions ? ArtifactBasePath / libDir : null;
        var distPath = DistBasePath / libDir;
        var outDir = SkiaPath / "out" / libDir;
        var libExtensions = new HashSet<string>(GetLibExtensions(Variant), StringComparer.OrdinalIgnoreCase);

        AbsolutePath rcOutputDir = null;
        if (OperatingSystem.IsWindows() && TargetOs == TargetOperatingSystem.Windows)
        {
            rcOutputDir = outDir / "obj" / "alphaskia_wrapper";
            AppendToFlagList(gnArgs, "extra_ldflags", $"'/LIBPATH:{rcOutputDir}'");
        }

        GnNinja($"out/{libDir}", buildTarget, gnArgs, gnFlags, SkiaPath,
            () =>
            {
                if (OperatingSystem.IsWindows() && TargetOs == TargetOperatingSystem.Windows)
                {
                    // compile resource file. Is is added as "library" in the BUILD.gn as input
                    // "llvm-rc.exe" /FO alphaskia.rc.obj alphaskia.rc /D RC_INVOKED /C 65001

                    var input = RootDirectory / "wrapper" / "src" / "alphaskia.rc";
                    rcOutputDir.CreateDirectory();
                    var output = rcOutputDir / "alphaskia.rc.obj";

                    const int utf8CodePage = 65001;
                    ToolResolver.GetTool((AbsolutePath)LlvmHome / "bin" / "llvm-rc.exe")(
                        $"/FO {output} {input} /D RC_INVOKED /C {utf8CodePage}",
                        workingDirectory: SkiaPath);
                }
            });

        try
        {
            void CopyBuildOutputTo(AbsolutePath path)
            {
                // libs
                outDir.Copy(path,
                    ExistsPolicy.MergeAndOverwrite,
                    null,
                    file => !libExtensions.Contains(file.Extension)
                );
                // copy header
                (RootDirectory / "wrapper" / "include" / "alphaskia.h").Copy(
                    DistBasePath / "include" / "alphaskia" / "alphaskia.h",
                    ExistsPolicy.MergeAndOverwrite
                );
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

    void BuildAlphaSkiaTest()
    {
        if (Variant != Variant.Shared)
        {
            Log.Information("Skipping build of BuildAlphaSkiaTest, not relevant for variant {Variant}", Variant);
            return;
        }

        var gnArgs = PrepareNativeBuild(Variant);
        var sharedLibPath = DistBasePath / GetLibDirectory("libalphaskia", variant: Variant);
        var gnFlags = new Dictionary<string, string>();

        if (!sharedLibPath.DirectoryExists())
        {
            throw new IOException(sharedLibPath + " is fully missing, needed for linking!");
        }

        Log.Debug("Available libs for linking: " + string.Join(", ", sharedLibPath.GetFiles().Select(f => f.Name)));
        AppendToFlagList(gnArgs, "extra_cflags", $"'-DALPHASKIA_TEST_RID={TargetOs.RuntimeIdentifier}'");

        // TODO: check if clang-cl also works with the linux flags
        AppendToFlagList(gnArgs, "extra_ldflags",
            TargetOs == TargetOperatingSystem.Windows ? $"'/LIBPATH:{sharedLibPath}'" : $"'-L{sharedLibPath}'");

        var buildTarget = "libalphaskiatest";
        var libDir = GetLibDirectory(buildTarget, TargetOs, Architecture, Variant);
        var outDir = SkiaPath / "out" / libDir;
        var exeExtension = GetExeExtension();

        GnNinja($"out/{libDir}", buildTarget, gnArgs, gnFlags, SkiaPath);

        // copy for artifacts
        var distPath = DistBasePath / libDir;
        var exePath = outDir / (buildTarget + exeExtension);
        exePath.Copy(distPath / exePath.Name, ExistsPolicy.MergeAndOverwrite);

        // copy shared lib beside executable
        var libExtensions = new HashSet<string>(GetLibExtensions(Variant), StringComparer.OrdinalIgnoreCase);
        foreach (var file in sharedLibPath.GetFiles().Where(f => libExtensions.Contains(f.Extension)))
        {
            file.Copy(outDir / file.Name, ExistsPolicy.MergeAndOverwrite);
        }

        // run executable
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

    void NativeWriteVersionInfoHeader()
    {
        var libExtension = GetLibExtensions(Variant)[0];

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

        if (Variant == Variant.Shared)
        {
            versionInfo.AppendLine("#define VER_INTERNALNAME_STR \"libalphaskia\"");
            versionInfo.AppendLine($"#define VER_ORIGINALFILENAME_STR \"libalphaskia{libExtension}\"");
        }
        else if (Variant == Variant.Jni)
        {
            versionInfo.AppendLine("#define VER_INTERNALNAME_STR \"libalphaskiajni\"");
            versionInfo.AppendLine($"#define VER_ORIGINALFILENAME_STR \"libalphaskiajni{libExtension}\"");
        }
        else if (Variant == Variant.Node)
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
            var url = $"https://nodejs.org/dist/v22.9.0/{TargetOs.RuntimeIdentifier}-{Architecture}/node.lib";
            var libDir = TemporaryDirectory / $"libnode-{TargetOs.RuntimeIdentifier}-{Architecture}";
            HttpTasks.HttpDownloadFile(url,
                libDir / "node.lib");
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