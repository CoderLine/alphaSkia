using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

partial class Build
{
    public Target Windows => _ => _
        .DependsOn(PrepareBuild)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .OnlyWhenStatic(OperatingSystem.IsWindows)
        .Executes(() =>
        {
            BuildSkiaWindows(Architecture, Variant);
        });

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
    
    void BuildSkiaWindows(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();

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

        gnArgs["skia_enable_fontmgr_win_gdi"] = "false";
        gnArgs["skia_use_dng_sdk"] = "true";
        gnArgs["extra_cflags"] = "[ '-DALPHASKIA_DLL', '/MT', '/EHsc', '/Z7', '-D_HAS_AUTO_PTR_ETC=1' ]";
        gnArgs["extra_ldflags"] = "[ '/DEBUG:FULL' ]";
        
        // override win_vc with the command line args
        var vsInstall = VsInstall;
        if (!string.IsNullOrEmpty(vsInstall))
        {
            AbsolutePath winVc = vsInstall;
            winVc /= "VC";
            gnArgs["win_vc"] = winVc;
        }

        string[] filesToCopy;
        var isShared = variant == Variant.Shared;
        if (isShared)
        {
            filesToCopy = new[]
            {
                "libAlphaSkia.dll",
                "libAlphaSkia.dll.lib",
                "libAlphaSkia.dll.pdb"
            };
        }
        else
        {
            filesToCopy = new[]
            {
                "libAlphaSkia.lib"
            };
        }
        
        BuildSkia("win", arch, variant, gnArgs, filesToCopy);
    }
}