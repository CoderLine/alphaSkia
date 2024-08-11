using System;
using System.Collections.Generic;
using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;

partial class Build
{
    Lazy<bool> LibSkiaSkip => new(() => CanUseCachedBinaries("libskia", Variant.Static));

    public Target LibSkiaGitSyncDeps => _ => _
        .Unlisted()
        .OnlyWhenStatic(() => !LibSkiaSkip.Value)
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

    public Target LibSkiaWithCache => _ => _
        .Unlisted()
        .Requires(() => Architecture)
        .Requires(() => TargetOs)
        // ensure it runs before any oher targets
        .Before(SetupDepotTools, LibSkiaPatchSkiaBuildFiles, LibSkiaGitSyncDeps)
        .Executes(() =>
        {
            if (IsGitHubActions)
            {
                var outputsFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
                if (File.Exists(outputsFile))
                {
                    File.AppendAllLines(outputsFile,
                        new[] { $"build-skipped={LibSkiaSkip.Value.ToString().ToLowerInvariant()}\n" });
                }
                else
                {
                    Log.Warning("Could not set output parameter, GITHUB_OUTPUT not found");
                }
            }

            if (LibSkiaSkip.Value)
            {
                FileSystemTasks.CopyDirectoryRecursively(DistBasePath, ArtifactBasePath, DirectoryExistsPolicy.Merge,
                    FileExistsPolicy.OverwriteIfNewer);
            }
            else
            {
                GitTool("submodule update --init --recursive --depth=1");
            }
        })
        .Triggers(LibSkia);

    public Target LibSkia => _ => _
        .DependsOn(LibSkiaGitSyncDeps, LibSkiaPatchSkiaBuildFiles, InstallDependenciesLinux)
        .OnlyWhenStatic(() => !LibSkiaSkip.Value)
        .Requires(() => Architecture)
        .Requires(() => TargetOs)
        .Executes(BuildSkia);

    public Target LibSkiaPatchSkiaBuildFiles => _ => _
        .Unlisted()
        .OnlyWhenStatic(() => !LibSkiaSkip.Value)
        .After(LibSkiaGitSyncDeps)
        .Executes(() =>
        {
            // add harfbuzz and freetype as dependency as we want it for alphaSkia
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
                var newDepsList = depsList.TrimEnd('\r', '\n', '\t', ' ', ',') 
                + ", \"//third_party/harfbuzz\", \"//third_party/freetype2\", "
                ;
                buildFileSource = buildFileSource[..depsListStart]
                                         + newDepsList
                                         + buildFileSource[depsListEnd..];
            }

            var sourcesStartMarker = "sources = []";
            var sourcesStart = buildFileSource.IndexOf(sourcesStartMarker, depsListStart, StringComparison.Ordinal);
            if (sourcesStart == -1)
            {
                throw new IOException("BUILD.gn of skia changed, cannot patch files");
            }

            var sourcesEnd = buildFileSource.IndexOf("libs = []", sourcesStart, StringComparison.Ordinal);
            if (sourcesEnd == -1)
            {
                throw new IOException("BUILD.gn of skia changed, cannot patch files");
            }

            var sources = buildFileSource.Substring(sourcesStart, sourcesEnd);
            if (!sources.Contains("# AlphaSkia Patch FreeType"))
            {
                var newSources = "# AlphaSkia Patch FreeType\n";
                // ensure we have freetype available
                newSources += "  include_dirs = [ \"externals/freetype/include\" ]\n";
                newSources += "  sources += [\n";
                newSources += "    \"src/ports/SkFontHost_FreeType.cpp\",\n";
                newSources += "    \"src/ports/SkFontHost_FreeType_common.cpp\",\n";
                newSources += "    \"src/ports/SkFontHost_FreeType_common.h\",\n";
                newSources += "  ]\n";
                // ensure we have the custom embedded FontMgr available (for in-memory freetype usage)
                newSources += "  sources += [\n";
                newSources += "    \"src/ports/SkFontMgr_custom.h\",\n";
                newSources += "    \"src/ports/SkFontMgr_custom.cpp\",\n";
                newSources += "    \"src/ports/SkFontMgr_custom_embedded.cpp\",\n";
                newSources += "  ]\n";
                // ensure we have the OS specific font managers available
                newSources += "  if (is_win) {\n";
                newSources += "    sources += [\n";
                newSources += "      \"include/ports/SkFontMgr_indirect.h\",\n";
                newSources += "      \"include/ports/SkRemotableFontMgr.h\",\n";
                newSources += "      \"src/fonts/SkFontMgr_indirect.cpp\",\n";
                newSources += "      \"src/ports/SkFontMgr_win_dw.cpp\",\n";
                newSources += "      \"src/ports/SkScalerContext_win_dw.cpp\",\n";
                newSources += "      \"src/ports/SkScalerContext_win_dw.h\",\n";
                newSources += "      \"src/ports/SkTypeface_win_dw.cpp\",\n";
                newSources += "      \"src/ports/SkTypeface_win_dw.h\",\n";
                newSources += "    ]\n";
                newSources += "  }\n";
                newSources += "  if (is_android) {\n";
                newSources += "    deps += [ \"//third_party/expat\" ]\n";
                newSources += "    sources += [\n";
                newSources += "      \"src/ports/SkFontMgr_android.cpp\",\n";
                newSources += "      \"src/ports/SkFontMgr_android_parser.cpp\",\n";
                newSources += "      \"src/ports/SkFontMgr_android_parser.h\",\n";
                newSources += "    ]\n";
                newSources += "  }\n";
                newSources += "  frameworks = []\n";
                newSources += "  if (is_mac) {\n";
                newSources += "    frameworks += [\n";
                newSources += "      \"AppKit.framework\",\n";
                newSources += "      \"ApplicationServices.framework\",\n";
                newSources += "    ]\n";
                newSources += "    sources += [\n";
                newSources += "      \"src/ports/SkFontMgr_mac_ct.cpp\",\n";
                newSources += "      \"src/ports/SkScalerContext_mac_ct.cpp\",\n";
                newSources += "      \"src/ports/SkScalerContext_mac_ct.h\",\n";
                newSources += "      \"src/ports/SkTypeface_mac_ct.cpp\",\n";
                newSources += "      \"src/ports/SkTypeface_mac_ct.h\",\n";
                newSources += "    ]\n";
                newSources += "  }\n";
                newSources += "  if (is_ios) {\n";
                newSources += "    frameworks += [\n";
                newSources += "      \"CoreFoundation.framework\",\n";
                newSources += "      \"CoreGraphics.framework\",\n";
                newSources += "      \"CoreText.framework\",\n";
                newSources += "      \"UIKit.framework\",\n";
                newSources += "    ]\n";
                newSources += "    sources += [\n";
                newSources += "      \"src/ports/SkFontMgr_mac_ct.cpp\",\n";
                newSources += "      \"src/ports/SkScalerContext_mac_ct.cpp\",\n";
                newSources += "      \"src/ports/SkScalerContext_mac_ct.h\",\n";
                newSources += "      \"src/ports/SkTypeface_mac_ct.cpp\",\n";
                newSources += "      \"src/ports/SkTypeface_mac_ct.h\",\n";
                newSources += "    ]\n";
                newSources += "  }\n";
                newSources += "  ";

                buildFileSource = buildFileSource[..sourcesEnd]
                                         + newSources
                                         + buildFileSource[sourcesEnd..];
            }

            const string emptyFactoryFile = "  sources = [ \"src/ports/SkFontMgr_empty_factory.cpp\" ]";
            var emptyFactoryIndex = buildFileSource.IndexOf(emptyFactoryFile);
            if(emptyFactoryIndex >= 0) 
            {
                var newSources = "  sources = [\n";
                newSources += "    \"../../wrapper/src/SkFontMgr_alphaskia_factory.cpp\",\n";
                newSources += "    \"../../wrapper/src/SkFontMgr_alphaskia.cpp\",\n";
                newSources += "  ]\n";
                newSources += "  defines = []";
                newSources += "  if (is_win) {\n";
                newSources += "    defines += [ \"ALPHASKIA_FONTMGR_WINDOWS\" ]\n";
                newSources += "  }\n";
                newSources += "  if (is_android) {\n";
                newSources += "    defines += [ \"ALPHASKIA_FONTMGR_ANDROID\" ]\n";
                newSources += "  }\n";
                newSources += "  if (is_mac) {\n";
                newSources += "    defines += [ \"ALPHASKIA_FONTMGR_MAC\" ]\n";
                newSources += "  }\n";
                newSources += "  if (is_ios) {\n";
                newSources += "    defines += [ \"ALPHASKIA_FONTMGR_IOS\" ]\n";
                newSources += "  }\n";

                buildFileSource = buildFileSource[..emptyFactoryIndex]
                                + newSources
                                + buildFileSource[(emptyFactoryIndex + emptyFactoryFile.Length)..];
            }

            buildFile.WriteAllText(buildFileSource);

            PatchSkiaToolchain();
            PatchSkiaMacOsVersion();
            PatchVulcanAllocatorIncludes();
        });

    void PatchVulcanAllocatorIncludes()
    {
        // https://github.com/microsoft/vcpkg/issues/31875
        var buildFile = SkiaPath / "third_party" / "externals" / "vulkanmemoryallocator" / "include" / "vk_mem_alloc.h";

        var include = """
            #if VMA_STATS_STRING_ENABLED
                #include <cstdio> // For snprintf
            #endif
        """;

        PatchSkiaFile(buildFile, include, "cstdio", "//",
            source =>
            {
                var stdLibInclude = source.IndexOf("#include <cstdlib>", StringComparison.Ordinal);
                if (stdLibInclude == -1)
                {
                    throw new IOException("Could not find patch position for vulcan memory allocator");
                }

                var endOfLine = source.IndexOf("\n", stdLibInclude, StringComparison.Ordinal);
                return endOfLine + 1;
            });
    }

    void BuildSkia()
    {
        var gnArgs = PrepareNativeBuild(Variant.Static);
        var gnFlags = new Dictionary<string, string>();
        var libDir = GetLibDirectory("libskia", TargetOs, Architecture, Variant.Static);
        var artifactsLibPath = IsGitHubActions ? ArtifactBasePath / libDir : null;
        var distPath = DistBasePath / libDir;
        var outDir = SkiaPath / "out" / libDir;
        var libExtension = new HashSet<string>(GetLibExtensions(Variant.Static), StringComparer.OrdinalIgnoreCase);

        // disable features we don't need
        gnArgs["skia_use_icu"] = "false";
        gnArgs["skia_use_piex"] = "false";
        gnArgs["skia_use_sfntly"] = "false";
        gnArgs["skia_enable_skshaper"] = "true";
        gnArgs["skia_pdf_subset_harfbuzz"] = "false";
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

        GnNinja($"out/{libDir}", "skia", gnArgs, gnFlags, SkiaPath);

        void CopyBuildOutputTo(AbsolutePath path)
        {
            // libs
            FileSystemTasks.CopyDirectoryRecursively(outDir, path, DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer, null, file => !libExtension.Contains(file.Extension));
        }

        CopyBuildOutputTo(distPath);
        if (artifactsLibPath != null)
        {
            CopyBuildOutputTo(artifactsLibPath);
        }
    }
}