using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;

partial class Build
{
    Lazy<bool> LibSkiaSkip => new(() => CanUseCachedBinaries("libskia", Variant.Static));

    [PublicAPI]
    public Target LibSkiaGitSyncDeps => t => t
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
                "third_party/externals/libgrapheme",
                "third_party/externals/icu",
                "third_party/externals/unicodetools",

                // Android font manager
                "third_party/externals/expat"
            };

            return GitSyncDepsCustom(requiredDependencies);
        });

    public Target LibSkiaWithCache => t => t
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
                        [$"build-skipped={LibSkiaSkip.Value.ToString().ToLowerInvariant()}\n"]);
                }
                else
                {
                    Log.Warning("Could not set output parameter, GITHUB_OUTPUT not found");
                }
            }

            if (LibSkiaSkip.Value)
            {
                DistBasePath.Copy(ArtifactBasePath, ExistsPolicy.MergeAndOverwrite);
            }
            else
            {
                GitTool("submodule update --init --recursive --depth=1");
            }
        })
        .Triggers(LibSkia);

    [PublicAPI]
    public Target LibSkia => t => t
        .DependsOn(LibSkiaGitSyncDeps, LibSkiaPatchSkiaBuildFiles, InstallDependenciesLinux)
        .OnlyWhenStatic(() => !LibSkiaSkip.Value)
        .Requires(() => Architecture)
        .Requires(() => TargetOs)
        .Executes(BuildSkia);

    [PublicAPI]
    public Target LibSkiaPatchSkiaBuildFiles => t => t
        .Unlisted()
        .OnlyWhenStatic(() => !LibSkiaSkip.Value)
        .After(LibSkiaGitSyncDeps)
        .Executes(() =>
        {

            // add harfbuzz and freetype as dependency as we want it for alphaSkia
            var buildFile = SkiaPath / "BUILD.gn";
            var buildFileSource = buildFile.ReadAllText();


            var skiaGniStart = buildFileSource.IndexOf("import(\"gn/skia.gni\")", StringComparison.Ordinal);
            if (skiaGniStart == -1)
            {
                throw new IOException("BUILD.gn of skia changed, cannot patch files");
            }

            var skiaGniEnd = buildFileSource.IndexOf("\n", skiaGniStart, StringComparison.Ordinal);
            if (skiaGniEnd == -1)
            {
                throw new IOException("BUILD.gn of skia changed, cannot patch files");
            }

            buildFileSource = buildFileSource[..skiaGniEnd]
                                        + "import(\"modules/skparagraph/skparagraph.gni\")\n"
                                        + "import(\"third_party/icu/icu.gni\")\n"
                                        + "import(\"modules/skunicode/skunicode.gni\")\n"
                                        + "import(\"modules/skshaper/skshaper.gni\")\n"
                                        + buildFileSource[skiaGniEnd..];


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
                + ", \"//third_party/harfbuzz\", \"//third_party/freetype2\", skia_libgrapheme_third_party_dir, skia_icu_bidi_third_party_dir"
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

            var sourcesEnd = buildFileSource.IndexOf("if (is_fuchsia)", sourcesStart, StringComparison.Ordinal);
            if (sourcesEnd == -1)
            {
                throw new IOException("BUILD.gn of skia changed, cannot patch files");
            }

            var sources = buildFileSource.Substring(sourcesStart, sourcesEnd);
            if (!sources.Contains("# AlphaSkia Patch FreeType and SkParagraph"))
            {
                var newSources = "# AlphaSkia Patch FreeType and SkParagraph\n";
                // ensure we have freetype available
                newSources += "  include_dirs = [ \"externals/freetype/include\" ]\n";
                newSources += "  sources += skia_ports_freetype_sources\n";
                // ensure we have the custom embedded FontMgr available (for in-memory freetype usage)
                newSources += "  sources += skia_ports_fontmgr_embedded_sources\n";
                newSources += "  sources += skia_ports_fontmgr_custom_sources\n";
                // ensure we have the OS specific font managers available
                newSources += "  if (is_win) {\n";
                newSources += "    sources += skia_ports_windows_fonts_sources\n";
                newSources += "  }\n";
                newSources += "  if (is_linux) {\n";
                newSources += "    public_deps += [ \"//third_party:fontconfig\" ]\n";
                newSources += "    sources += skia_ports_fontmgr_fontconfig_sources\n";
                newSources += "  }\n";
                newSources += "  if (is_android) {\n";
                newSources += "    deps += [ \"//third_party/expat\" ]\n";
                newSources += "    sources += skia_ports_fontmgr_android_sources\n";
                newSources += "  }\n";
                newSources += "  frameworks = []\n";
                newSources += "  if (is_mac) {\n";
                newSources += "    frameworks += [\n";
                newSources += "      \"AppKit.framework\",\n";
                newSources += "      \"ApplicationServices.framework\",\n";
                newSources += "    ]\n";
                newSources += "    sources += skia_ports_fontmgr_coretext_sources\n";
                newSources += "  }\n";
                newSources += "  if (is_ios) {\n";
                newSources += "    frameworks += [\n";
                newSources += "      \"CoreFoundation.framework\",\n";
                newSources += "      \"CoreGraphics.framework\",\n";
                newSources += "      \"CoreText.framework\",\n";
                newSources += "      \"UIKit.framework\",\n";
                newSources += "    ]\n";
                newSources += "    sources += skia_ports_fontmgr_coretext_sources\n";
                newSources += "  }\n";
                newSources += "  ";

                // Directly add some submodules to the main skia lib

                // SkShaper
                newSources += "  defines += [ \"SK_SHAPER_HARFBUZZ_AVAILABLE\", \"SK_SHAPER_UNICODE_AVAILABLE\", \"SKSHAPER_IMPLEMENTATION=1\" ]\n";
                newSources += "  sources += skia_shaper_primitive_sources\n";
                newSources += "  sources += skia_shaper_skunicode_sources\n";
                newSources += "  sources += skia_shaper_harfbuzz_sources\n";
                newSources += "  public += skia_unicode_public\n";

                // SkUnicode
                newSources += "  defines += [\"SK_UNICODE_AVAILABLE\", \"SKUNICODE_IMPLEMENTATION=1\", \"SK_UNICODE_LIBGRAPHEME_IMPLEMENTATION\" ]\n";
                newSources += "  configs += [\"//third_party/icu/config:no_cxx\"]\n";

                newSources += "  sources += skia_unicode_sources\n";
                newSources += "  sources += skia_unicode_icu_bidi_sources\n";
                newSources += "  sources += skia_unicode_bidi_subset_sources\n";
                newSources += "  sources += skia_unicode_libgrapheme_sources\n";
                newSources += "  public += skia_unicode_public\n";
                newSources += "  defines += [\n";
                newSources += "    \"U_DISABLE_RENAMING=0\",\n";
                newSources += "    \"U_USING_ICU_NAMESPACE=0\",\n";
                newSources += "    \"U_LIB_SUFFIX_C_NAME=_skia\",\n";
                newSources += "    \"U_HAVE_LIB_SUFFIX=1\",\n";
                newSources += "    \"U_DISABLE_VERSION_SUFFIX=1\",\n";
                newSources += "  ]\n";

                // SkParagraph
                newSources += "  defines += [ \"SK_ENABLE_PARAGRAPH\" ]\n";
                newSources += "  sources += skparagraph_sources\n";
                newSources += "  public += skparagraph_public\n";

                newSources += "  sources += [\n";
                newSources += "    \"../../wrapper/src/SkFontMgr_alphaskia.cpp\",\n";
                newSources += "    \"../../wrapper/include/SkFontMgr_alphaskia.h\",\n";
                newSources += "  ]\n";
                newSources += "  public += [ \"../../wrapper/include/SkFontMgr_alphaskia.h\" ]";
                newSources += "  if (is_win) {\n";
                newSources += "    defines += [ \"ALPHASKIA_FONTMGR_WINDOWS\" ]\n";
                newSources += "  }\n";
                newSources += "  if (is_linux) {\n";
                newSources += "    defines += [ \"ALPHASKIA_FONTMGR_LINUX\" ]\n";
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

                buildFileSource = buildFileSource[..sourcesEnd]
                                         + newSources
                                         + buildFileSource[sourcesEnd..];
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
        gnArgs["skia_use_libgrapheme"] = "true";
        gnArgs["skia_enable_skshaper"] = "true";
        gnArgs["skia_enable_skparagraph"] = "true";
        gnArgs["skia_enable_skunicode"] = "true";
        gnArgs["skia_use_harfbuzz"] = "true";
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
            outDir.Copy(path, ExistsPolicy.MergeAndOverwrite, null,
                file => !libExtension.Contains(file.Extension));
        }

        CopyBuildOutputTo(distPath);
        if (artifactsLibPath != null)
        {
            CopyBuildOutputTo(artifactsLibPath);
        }
    }
}