using System.Collections.Generic;

partial class Build
{
    void BuildLibAlphaSkiaLinux()
    {
        var gnArgs = new Dictionary<string, string>();
        string[] filesToCopy;
        var isShared = Variant == Variant.Shared;
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

        BuildSkiaLinux("libAlphaSkia", gnArgs, filesToCopy);
    }

    void BuildLibAlphaSkiaJniLinux()
    {
        var gnArgs = new Dictionary<string, string>();
        var alphaSkiaInclude = DistBasePath / "include";
        var jniInclude = JavaHome / "include";
        var jniWinInclude = JavaHome / "include" / "linux";
        gnArgs["extra_cflags"] = $"[ '-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniWinInclude}' ]";

        // Add Libs and lib search paths
        var staticLibPath = DistBasePath / GetLibDirectory(variant: Variant.Static);
        gnArgs["extra_ldflags"] =
            $"[ '-L{staticLibPath}', '-lAlphaSkia', '-lskia' ]";

        BuildSkiaLinux("libAlphaSkiaJni", gnArgs, new[] { "libAlphaSkiaJni.so" });
    }

    void BuildSkiaLinux(string buildTarget, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        gnArgs["skia_use_system_freetype2"] = "false";

        BuildSkia(buildTarget, gnArgs, filesToCopy);
    }

    void SetClangLinux(Dictionary<string, string> gnArgs)
    {
        AppendToFlagList(gnArgs, "extra_cflags", "'-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM'");

        if (Architecture == Architecture.X64)
        {
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }
        else if (Architecture == Architecture.X86)
        {
            // TODO
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }
        else if (Architecture == Architecture.Arm64) // aka AArch64
        {
            // TODO
            gnArgs["cc"] = "clang";
            gnArgs["cxx"] = "'clang++'";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }

        // gnArgs["ar"] = "llvm-ar";
    }
}