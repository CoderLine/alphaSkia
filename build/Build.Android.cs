using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using static Nuke.Common.EnvironmentInfo;

partial class Build
{
    [Parameter] readonly string NdkPath = GetVariable<string>("ANDROID_NDK_HOME") ?? FindNdk();

    static string FindNdk()
    {   
        if (OperatingSystem.IsWindows())
        {
            var candidates = new List<string>()
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Ndk")
            };

            return candidates.Select(c =>
            {
                if (!Directory.Exists(c))
                {
                    return null;
                }
                
                if (File.Exists(Path.Combine(c, "package.xml")))
                {
                    return c;
                }

                foreach (var subDir in Directory.EnumerateDirectories(c).OrderByDescending(Path.GetFileName))
                {
                    if (File.Exists(Path.Combine(subDir, "package.xml")))
                    {
                        return subDir;
                    }
                }

                return null;
            }).FirstOrDefault(d => d != null) ?? string.Empty;
        }

        return string.Empty;
    }

    public Target AndroidSkia => _ => _
        .OnlyWhenDynamic(() => !UseCache || !HasCachedFiles("libAlphaSkia", "android"))
        .DependsOn(GitSyncDepsSkia, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant)
        .Executes(() =>
        {
            BuildSkiaAndroidMain(Architecture, Variant);
        });

    public Target AndroidJni => _ => _
        .DependsOn(PrepareGitHubArtifacts, GitSyncDepsJni, PatchSkiaBuildFiles)
        .Requires(() => Architecture)
        .Requires(() => Variant == Variant.Shared)
        .Executes(() =>
        {
            BuildSkiaAndroidJni(Architecture, Variant);
        });

    void BuildSkiaAndroidMain(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
        gnArgs["ndk"] = NdkPath;

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

        BuildSkiaAndroid("libAlphaSkia", arch, variant, gnArgs, filesToCopy);
    }

    void BuildSkiaAndroidJni(Architecture arch, Variant variant)
    {
        var gnArgs = new Dictionary<string, string>();
        var alphaSkiaInclude = DistBasePath / "include";
        var jniInclude = JavaHome / "include";
        var jniWinInclude = JavaHome / "include" / "linux";
        gnArgs["ndk"] = NdkPath;
        gnArgs["extra_cflags"] = $"[ '-I{alphaSkiaInclude}', '-I{jniInclude}', '-I{jniWinInclude}' ]";

        // Add Libs and lib search paths
        var staticLibPath = DistBasePath / $"libAlphaSkia-android-{arch}-static";
        gnArgs["extra_ldflags"] =
            $"[ '-L{staticLibPath}', '-lAlphaSkia', '-lskia' ]";

        BuildSkiaAndroid("libAlphaSkiaJni", arch, variant, gnArgs, new[] { "libAlphaSkiaJni.so" });
    }

    void BuildSkiaAndroid(string buildTarget, Architecture arch, Variant variant, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        if (OperatingSystem.IsWindows())
        {
            SetClangWindows(gnArgs);
        }
        else if (OperatingSystem.IsLinux())
        {
            SetClangLinux(arch, gnArgs);
        }
        
        // gnArgs["skia_enable_ganesh"] = "true";
        gnArgs["skia_use_system_freetype2"] = "false";
        
        // -Wdeprecated-declarations

        BuildSkia(buildTarget, "android", arch, variant, gnArgs, filesToCopy);
    }
}