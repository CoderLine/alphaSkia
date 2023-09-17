using System;
using System.Collections.Generic;
using Nuke.Common;

partial class Build
{
    public Target Linux => _ => _
        .DependsOn(PrepareBuild)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .OnlyWhenStatic(OperatingSystem.IsLinux)
        .Executes(() =>
        {
            BuildSkiaLinux(Architecture, Variant);
        });
    
    void BuildSkiaLinux(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();

        gnArgs["skia_enable_ganesh"] = "true";
        gnArgs["extra_cflags"] = "[ '-DALPHASKIA_DLL', '-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM' ]";
        gnArgs["extra_asmflags"] = "[]";

        string archString = arch;
        if (arch == Architecture.X64)
        {
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            gnArgs["extra_ldflags"] = "[ '-static-libstdc++', '-static-libgcc' ]";
        }
        else if (arch == Architecture.X86)
        {
            // TODO
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            gnArgs["extra_ldflags"] = "[ '-static-libstdc++', '-static-libgcc' ]";
        }
        else if (arch == Architecture.Arm64) // aka AArch64
        {
            // TODO
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            gnArgs["extra_ldflags"] = "[ ]";
        }
        
        // gnArgs["ar"] = "llvm-ar";
        gnArgs["skia_use_system_freetype2"] = "false";

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
                "libAlphaSkia.a"
            };
        }
        
        BuildSkia("linux", arch, variant, gnArgs, filesToCopy);
    }
}