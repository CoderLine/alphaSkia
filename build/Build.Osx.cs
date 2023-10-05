using System;
using System.Collections.Generic;
using Nuke.Common;

partial class Build
{
    public Target OsxSkia => _ => _
        .DependsOn(GitSyncDepsSkia, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .OnlyWhenStatic(OperatingSystem.IsMacOS)
        .Executes(() =>
        {
            BuildSkiaOsxMain(Architecture, Variant);
        });

    public Target OsxJni => _ => _
        .DependsOn(PrepareGitHubArtifacts, GitSyncDepsJni, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant == Variant.Shared)
        .OnlyWhenStatic(OperatingSystem.IsMacOS)
        .Executes(() =>
        {
            BuildSkiaOsxJni(Architecture, Variant);
        });

    void BuildSkiaOsxMain(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
        string[] filesToCopy;
        var isShared = variant == Variant.Shared;
        if (isShared)
        {
            filesToCopy = new[]
            {
                "libAlphaSkia.so"
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

        BuildSkiaOsx("libAlphaSkia", arch, variant, gnArgs, filesToCopy);
    }

    void BuildSkiaOsxJni(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
        var alphaSkiaInclude = DistBasePath / "include";
        var jniInclude = JavaHome / "include";
        var jniWinInclude = JavaHome / "include" / "osx"; // TODO
        gnArgs["extra_cflags"] = $"[ '-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniWinInclude}' ]";

        // Add Libs and lib search paths
        var staticLibPath = DistBasePath / $"libAlphaSkia-osx-{arch}-static";
        gnArgs["extra_ldflags"] =
            $"[ '-L{staticLibPath}', '-lAlphaSkia', '-lskia' ]";

        BuildSkiaOsx("libAlphaSkiaJni", arch, variant, gnArgs, new[] { "libAlphaSkiaJni.so" });
    }

    void BuildSkiaOsx(string buildTarget, Architecture arch, Variant variant, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        SetClangOsx(arch, gnArgs);

        gnArgs["skia_use_system_freetype2"] = "false";

        BuildSkia(buildTarget, "mac", arch, variant, gnArgs, filesToCopy);
    }

    void SetClangOsx(Architecture arch, Dictionary<string, string> gnArgs)
    {
        AppendToFlagList(gnArgs, "extra_cflags", "'-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM'");

        if (arch == Architecture.X64)
        {
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }
        else if (arch == Architecture.X86)
        {
            // TODO
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }
        else if (arch == Architecture.Arm64) // aka AArch64
        {
            // TODO
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }

        // gnArgs["ar"] = "llvm-ar";
    }
}