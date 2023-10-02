using System;
using System.Collections.Generic;
using Nuke.Common;

partial class Build
{
    public Target LinuxSkia => _ => _
        .DependsOn(GitSyncDepsSkia, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .OnlyWhenStatic(OperatingSystem.IsLinux)
        .Executes(() =>
        {
            BuildSkiaLinuxMain(Architecture, Variant);
        });

    public Target LinuxJni => _ => _
        .DependsOn(PrepareGitHubArtifacts, GitSyncDepsJni, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant == Variant.Shared)
        .OnlyWhenStatic(OperatingSystem.IsLinux)
        .Executes(() =>
        {
            BuildSkiaLinuxJni(Architecture, Variant);
        });

    void BuildSkiaLinuxMain(Architecture arch, Variant variant)
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

        BuildSkiaLinux("libAlphaSkia", arch, variant, gnArgs, filesToCopy);
    }

    void BuildSkiaLinuxJni(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
        var alphaSkiaInclude = RootDirectory / "dist" / "include";
        var jniInclude = JavaHome / "include";
        var jniWinInclude = JavaHome / "include" / "linux";
        gnArgs["extra_cflags"] = $"[ '-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniWinInclude}' ]";

        // Add Libs and lib search paths
        var staticLibPath = RootDirectory / "dist" / $"libAlphaSkia-win-{arch}-static";
        gnArgs["extra_ldflags"] =
            $"[ '-L:{staticLibPath}', '-llibAlphaSkia.lib', '-llibskia.lib' ]";

        BuildSkiaLinux("libAlphaSkiaJni", arch, variant, gnArgs, new[] { "libAlphaSkiaJni.so" });
    }

    void BuildSkiaLinux(string buildTarget, Architecture arch, Variant variant, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        gnArgs["skia_enable_ganesh"] = "true";

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
        gnArgs["skia_use_system_freetype2"] = "false";


        BuildSkia(buildTarget, "linux", arch, variant, gnArgs, filesToCopy);
    }
}