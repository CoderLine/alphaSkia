using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.IO;

partial class Build
{
    public Target LibAlphaSkiaGitSyncDeps => _ => _
        .Unlisted()
        .DependsOn(SetupDepotTools)
        .Executes(() =>
        {
            var requiredDependencies = new[]
            {
                "buildtools",
                "third_party/externals/harfbuzz"
            };

            return GitSyncDepsCustom(requiredDependencies);
        });

    public Target LibAlphaSkia => _ => _
        .DependsOn(PrepareGitHubArtifacts, LibAlphaSkiaGitSyncDeps, LibAlphaSkiaPatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .Requires(() => TargetOs)
        .Executes(BuildAlphaSkia);

    public Target LibAlphaSkiaPatchSkiaBuildFiles => _ => _
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
                set_defaults("alphaskia_build") {
                  configs = default_configs
                  if (!is_shared_alphaskia) {
                      complete_static_lib = true
                  }
                }
            """;
            PatchSkiaFile(SkiaPath / "gn" / "BUILDCONFIG.gn", buildConfigNew);

            const string buildNew = """
                alphaskia_wrapper_sources = [
                    "../../wrapper/src/AlphaSkiaCanvas.cpp",
                    "../../wrapper/src/alphaskia_canvas.cpp",
                    "../../wrapper/src/alphaskia_image.cpp",
                    "../../wrapper/src/alphaskia_typeface.cpp",
                    "../../wrapper/src/alphaskia_data.cpp"
                ]
                config("alphaskia_public") {
                  defines = [ "_SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING", "ALPHASKIA_IMPLEMENTATION=1" ]
                  include_dirs = [ "." ]
                  if (is_mac) {
                    frameworks = [
                      "AppKit.framework",
                      "ApplicationServices.framework",

                      "OpenGL.framework",
                      
                      "Metal.framework",
                      "Foundation.framework"
                    ]   
                  }
                  if (is_ios) {
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

                  if (is_shared_alphaskia) {
                    defines += [ "ALPHASKIA_DLL" ]
                  }
                  if( is_win) {
                    libs = [ "skia.lib", "user32.lib", "OpenGL32.lib" ]
                  } 
                  else {
                    libs = [ "skia" ]
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
            """;
            PatchSkiaFile(SkiaPath / "BUILD.gn", buildNew);
            PatchSkiaToolchain();
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

    void BuildAlphaSkia()
    {
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
            if (OperatingSystem.IsWindows())
            {
                jniPlatformInclude = jniInclude / "win32";
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

            AppendToFlagList(gnArgs, "extra_cflags",
                $"'-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniPlatformInclude}'");
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
            else if(OperatingSystem.IsMacOS() && TargetOs == TargetOperatingSystem.MacOs)
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
            AppendToFlagList(gnArgs, "extra_ldflags",
                $"'/LIBPATH:{staticLibPath}'");
        }
        else
        {
            AppendToFlagList(gnArgs, "extra_ldflags", $"'-L{staticLibPath}'");
        }

        var libDir = GetLibDirectory(buildTarget, TargetOs, Architecture, Variant);
        var artifactsLibPath = IsGitHubActions ? ArtifactBasePath / libDir : null;
        var distPath = DistBasePath / libDir;
        var outDir = SkiaPath / "out" / libDir;
        var libExtension = GetLibExtension(Variant);

        GnNinja($"out/{libDir}", buildTarget, gnArgs, gnFlags, SkiaPath);

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

    AbsolutePath DownloadNodeLib()
    {
        if (OperatingSystem.IsWindows() && TargetOs == TargetOperatingSystem.Windows)
        {
            // libs are available at urls like: 
            // https://nodejs.org/dist/latest/win-x64/node.lib
            // https://nodejs.org/dist/latest/win-x86/node.lib
            // https://nodejs.org/dist/latest/win-arm64/node.lib
            var url = $"https://nodejs.org/dist/latest/{TargetOs.RuntimeIdentifier}-{Architecture}/node.lib";
            var libDir = TemporaryDirectory / $"libnode-{TargetOs.RuntimeIdentifier}-{Architecture}";
            HttpTasks.HttpDownloadFile(url,
                libDir / "node.lib");
            return libDir;
        }

        return null;
    }
}