using System;
using System.Collections.Generic;
using Nuke.Common;

partial class Build
{
    public Target MacOsSkia => _ => _
        .DependsOn(GitSyncDepsSkia, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .OnlyWhenStatic(OperatingSystem.IsMacOS)
        .Executes(() =>
        {
            BuildSkiaMacOsMain(Architecture, Variant);
        });

    public Target MacOsJni => _ => _
        .DependsOn(PrepareGitHubArtifacts, GitSyncDepsJni, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant == Variant.Shared)
        .OnlyWhenStatic(OperatingSystem.IsMacOS)
        .Executes(() =>
        {
            BuildSkiaMacOsJni(Architecture, Variant);
        });

    void BuildSkiaMacOsMain(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
        string[] filesToCopy;
        var isShared = variant == Variant.Shared;
        if (isShared)
        {
            filesToCopy = new[]
            {
                "libAlphaSkia.dylib"
            };
        }
        else
        {
            filesToCopy = new[]
            {
                "libAlphaSkia.a",
                "libskia.a"
            };
        }

        BuildSkiaMacOs("libAlphaSkia", arch, variant, gnArgs, filesToCopy);
    }

    void BuildSkiaMacOsJni(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
        var alphaSkiaInclude = DistBasePath / "include";
        var jniInclude = JavaHome / "include";
        var jniWinInclude = JavaHome / "include" / "darwin";
        gnArgs["extra_cflags"] = $"[ '-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniWinInclude}' ]";

        // Add Libs and lib search paths
        var staticLibPath = DistBasePath / $"libAlphaSkia-macos-{arch}-static";
        gnArgs["extra_ldflags"] =
            $"[ '-L{staticLibPath}', '-lAlphaSkia', '-lskia' ]";

        BuildSkiaMacOs("libAlphaSkiaJni", arch, variant, gnArgs, new[] { "libAlphaSkiaJni.so" });
    }

    void BuildSkiaMacOs(string buildTarget, Architecture arch, Variant variant, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        SetClangMacOs(arch, gnArgs);

        gnArgs["skia_use_system_freetype2"] = "false";
        gnArgs["skia_use_metal"] = "true";

        BuildSkia(buildTarget, "mac", arch, variant, gnArgs, filesToCopy);
    }

    void SetClangMacOs(Architecture arch, Dictionary<string, string> gnArgs)
    {
        AppendToFlagList(gnArgs, "extra_cflags", "'-DHAVE_ARC4RANDOM_BUF', '-stdlib=libc++'");

        gnArgs["cc"] = "clang";
        gnArgs["cxx"] = "'clang++'";
        AppendToFlagList(gnArgs, "extra_ldflags", "'-stdlib=libc++'");

        // gnArgs["ar"] = "llvm-ar";
    }
}