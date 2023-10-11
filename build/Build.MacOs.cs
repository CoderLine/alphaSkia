using System.Collections.Generic;

partial class Build
{
    void SetClangMacOs(Dictionary<string, string> gnArgs)
    {
        AppendToFlagList(gnArgs, "extra_cflags", "'-DHAVE_ARC4RANDOM_BUF', '-stdlib=libc++'");
        AppendToFlagList(gnArgs, "extra_ldflags", "'-stdlib=libc++'");
    }
}