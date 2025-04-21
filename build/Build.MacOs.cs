using System.Collections.Generic;
using Nuke.Common.IO;

partial class Build
{
    void SetClangMacOs(Dictionary<string, string> gnArgs)
    {
        AppendToFlagList(gnArgs, "extra_cflags", "'-DHAVE_ARC4RANDOM_BUF', '-stdlib=libc++'");
        AppendToFlagList(gnArgs, "extra_ldflags", "'-stdlib=libc++'");
    }

    void PatchSkiaMacOsVersion()
    {
        // Skia has hard-coded x86_64-apple-macos10.13 as target, we want it slightly newer as x86_64-apple-macos10.15
        // to use some more C++ types 
        var buildFile = SkiaPath / "gn" / "skia" / "BUILD.gn";
        var source = buildFile.ReadAllText();
        buildFile.WriteAllText(source.Replace("x86_64-apple-macos10.13", "x86_64-apple-macos10.15"));
    }
}