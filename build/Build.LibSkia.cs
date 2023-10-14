using System;
using System.Collections.Generic;
using System.IO;
using Nuke.Common;
using Nuke.Common.IO;

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
        .DependsOn(LibSkiaGitSyncDeps, LibSkiaPatchSkiaBuildFiles)
        .OnlyWhenStatic(() => !LibSkiaSkip.Value)
        .Requires(() => Architecture)
        .Requires(() => TargetOs)
        .Executes(BuildSkia);

    public Target LibSkiaPatchSkiaBuildFiles => _ => _
        .Unlisted()
        .OnlyWhenStatic(() => !LibSkiaSkip.Value)
        .Executes(() =>
        {
            // add harfbuzz as dependency as we want it for alphaSkia
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
                var newDepsList = depsList.TrimEnd('\r', '\n', '\t', ' ', ',') + ", \"//third_party/harfbuzz\", ";
                var newBuildFileSource = buildFileSource[..depsListStart]
                                         + newDepsList
                                         + buildFileSource[depsListEnd..];
                buildFile.WriteAllText(newBuildFileSource);
            }

            PatchSkiaToolchain();
        });

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