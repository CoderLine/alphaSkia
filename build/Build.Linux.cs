using System;
using System.Collections.Generic;
using Nuke.Common;

partial class Build
{
    public Target LinuxSkia => _ => _
        .DependsOn(SetupDepotTools, GitSyncDeps, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .OnlyWhenStatic(OperatingSystem.IsLinux)
        .Executes(() =>
        {
            BuildSkiaLinuxMain(Architecture, Variant);
        });

    public Target LinuxJni => _ => _
        .DependsOn(SetupDepotTools, PatchSkiaBuildFiles)
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
                "skia.a"
            };
        }

        BuildSkiaLinux("libAlphaSkia", arch, variant, gnArgs, filesToCopy);
    }

    void BuildSkiaLinuxJni(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();

        var filesToCopy = new[] { "libAlphaSkiaJni.so" };

        BuildSkiaLinux("libAlphaSkiaJni", arch, variant, gnArgs, filesToCopy);
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