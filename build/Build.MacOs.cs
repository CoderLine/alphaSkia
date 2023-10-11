using System.Collections.Generic;

partial class Build
{
    void BuildLibAlphaSkiaMacOs()
    {
        var gnArgs = new Dictionary<string, string>();
        string[] filesToCopy;
        var isShared = Variant == Variant.Shared;
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

        BuildSkiaMacOs("libAlphaSkia", gnArgs, filesToCopy);
    }

    void BuildLibAlphaSkiaJniMacOs()
    {
        var gnArgs = new Dictionary<string, string>();
        var alphaSkiaInclude = DistBasePath / "include";
        var jniInclude = JavaHome / "include";
        var jniWinInclude = JavaHome / "include" / "darwin";
        gnArgs["extra_cflags"] = $"[ '-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniWinInclude}' ]";

        // Add Libs and lib search paths
        var staticLibPath = DistBasePath / GetLibDirectory(variant: Variant.Static);
        gnArgs["extra_ldflags"] =
            $"[ '-L{staticLibPath}', '-lAlphaSkia', '-lskia' ]";

        BuildSkiaMacOs("libAlphaSkiaJni", gnArgs, new[] { "libAlphaSkiaJni.dylib" });
    }

    void BuildSkiaMacOs(string buildTarget, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        gnArgs["skia_use_system_freetype2"] = "false";
        gnArgs["skia_use_metal"] = "true";

        BuildSkia(buildTarget, gnArgs, filesToCopy);
    }

    void SetClangMacOs(Dictionary<string, string> gnArgs)
    {
        AppendToFlagList(gnArgs, "extra_cflags", "'-DHAVE_ARC4RANDOM_BUF', '-stdlib=libc++'");

        gnArgs["cc"] = "clang";
        gnArgs["cxx"] = "'clang++'";
        AppendToFlagList(gnArgs, "extra_ldflags", "'-stdlib=libc++'");
    }
}