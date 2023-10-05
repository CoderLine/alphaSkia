using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

partial class Build
{
    public Target WindowsSkia => _ => _
        .OnlyWhenDynamic(() => !CanUseCachedBinaries("libAlphaSkia", "win"))
        .DependsOn(GitSyncDepsSkia, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .OnlyWhenStatic(OperatingSystem.IsWindows)
        .Executes(() =>
        {
            BuildSkiaWindowsMain(Architecture, Variant);
        });
    
    public Target WindowsJni => _ => _
        .DependsOn(PrepareGitHubArtifacts, GitSyncDepsJni, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant == Variant.Shared)
        .OnlyWhenStatic(OperatingSystem.IsWindows)
        .Executes(() =>
        {
            BuildSkiaWindowsJni(Architecture, Variant);
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

    void BuildSkiaWindowsMain(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
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
                "libAlphaSkia.lib",
                "skia.lib"
            };
        }

        BuildSkiaWindows("libAlphaSkia", arch, variant, gnArgs, filesToCopy);
    }

    void BuildSkiaWindowsJni(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
        var alphaSkiaInclude = DistBasePath / "include";
        var jniInclude = JavaHome / "include";
        var jniWinInclude = JavaHome / "include" / "win32";
        gnArgs["extra_cflags"] = $"[ '-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniWinInclude}' ]";

        // Add Libs and lib search paths
        var staticLibPath = DistBasePath / $"libAlphaSkia-win-{arch}-static";
        gnArgs["extra_ldflags"] =
            $"[ '/LIBPATH:{staticLibPath}', 'libAlphaSkia.lib', 'skia.lib', 'user32.lib', 'OpenGL32.lib' ]";

        BuildSkiaWindows("libAlphaSkiaJni", arch, variant, gnArgs, new[]
        {
            "libAlphaSkiaJni.dll",
            "libAlphaSkiaJni.dll.lib",
            "libAlphaSkiaJni.dll.pdb"
        });
    }

    void BuildSkiaWindows(string buildTarget, Architecture arch, Variant variant, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        SetClangWindows(gnArgs);

        gnArgs["skia_enable_fontmgr_win_gdi"] = "false";

        BuildSkia(buildTarget, "win", arch, variant, gnArgs, filesToCopy);
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

        gnArgs["cc"] = "clang";
        gnArgs["cxx"] = "'clang++'";

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