using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

partial class Build
{
    Tool VsWhere => ToolResolver.GetPathTool(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio", "Installer",
        "vswhere.exe"));

    string VsInstall
    {
        get
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            try
            {
                var output = VsWhere("-latest -prerelease -property installationPath");
                return output.Select(o => o.Text).FirstOrDefault(Directory.Exists);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    void SetClangWindows(Dictionary<string, string> gnArgs)
    {
        if (!string.IsNullOrEmpty(LlvmHome))
        {
            gnArgs["clang_win"] = LlvmHome;

            // there is a problem in the BUILDCONFIG.gn of Skia looking for a version number like 16.0.0
            // but with the installation on windows it is simply 16. 
            var version = ((AbsolutePath)LlvmHome) / "lib" / "clang";
            var newestVersion = version.GetDirectories().MaxBy(d => d.Name);
            if (!string.IsNullOrEmpty(newestVersion))
            {
                gnArgs["clang_win_version"] = newestVersion.Name;
            }
        }

        // override win_vc with the command line args
        var vsInstall = VsInstall;
        if (!string.IsNullOrEmpty(vsInstall))
        {
            AbsolutePath winVc = vsInstall;
            winVc /= "VC";
            gnArgs["win_vc"] = winVc;
        }

        AppendToFlagList(gnArgs, "extra_cflags", "'/MT', '/EHsc', '/Z7', '-D_HAS_AUTO_PTR_ETC=1'");
        AppendToFlagList(gnArgs, "extra_ldflags", "'/DEBUG:FULL'");
    }
}