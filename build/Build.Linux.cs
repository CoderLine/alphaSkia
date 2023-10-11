using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

partial class Build
{
    void InstallDependenciesLinux()
    {
        if (!IsGitHubActions)
        {
            return;
        }

        var bash = ToolResolver.GetPathTool("bash");
        var sudo = ToolResolver.GetPathTool("sudo");
        var dependenciesScript = SkiaPath / "tools" / "install_dependencies.sh";
        if (dependenciesScript.FileExists())
        {
            bash($"{dependenciesScript}", workingDirectory: SkiaPath);
        }

        // Linux cross compilation
        if (Architecture != Architecture.X64 && TargetOs == TargetOperatingSystem.Linux)
        {
            var linuxArch = Architecture.LinuxArch;

            var crossInstallDependencies = new StringBuilder();
            crossInstallDependencies.AppendLine("#!/bin/bash");
            crossInstallDependencies.AppendLine("set -e");

            // Using aptitude here because github actions runners have quite some packages preinstalled
            // which leads to many version conflicts after adding more sources. aptitude can resolve those 
            // conflicts easier
            crossInstallDependencies.AppendLine("echo Install Aptitude");
            crossInstallDependencies.AppendLine("apt-get update");
            crossInstallDependencies.AppendLine("apt-get install -y aptitude");
            
            crossInstallDependencies.AppendLine($"echo Adding Arch {linuxArch}");
            crossInstallDependencies.AppendLine($"dpkg --add-architecture {linuxArch}");

            crossInstallDependencies.AppendLine("echo Modifying sources.list");
            crossInstallDependencies.AppendLine("sed -i \"s/deb /deb [arch=amd64,i386] /\" /etc/apt/sources.list");
            crossInstallDependencies.AppendLine("sed -i \"s/deb-src /deb-src [arch=amd64,i386] /\" /etc/apt/sources.list");

            if (Architecture == Architecture.Arm || Architecture == Architecture.Arm64)
            {
                crossInstallDependencies.AppendLine($"echo 'deb [arch={linuxArch}] http://ports.ubuntu.com/ubuntu-ports/ jammy main multiverse universe' >> /etc/apt/sources.list");
                crossInstallDependencies.AppendLine(
                    $"echo 'deb [arch={linuxArch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-security main multiverse universe' >> /etc/apt/sources.list");
                crossInstallDependencies.AppendLine(
                    $"echo 'deb [arch={linuxArch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-backports main multiverse universe' >> /etc/apt/sources.list");
                crossInstallDependencies.AppendLine(
                    $"echo 'deb [arch={linuxArch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-updates main multiverse universe' >> /etc/apt/sources.list");
            }
            crossInstallDependencies.AppendLine("echo Updating Packages");
            crossInstallDependencies.AppendLine("apt-get update");
            crossInstallDependencies.AppendLine("echo Installing main build tools");
            crossInstallDependencies.AppendLine($"aptitude install -y crossbuild-essential-{linuxArch} libstdc++-11-dev-{linuxArch}-cross");
            crossInstallDependencies.AppendLine("echo Installing arch libs");
            crossInstallDependencies.AppendLine($"aptitude install -y libfontconfig-dev:{linuxArch} libgl1-mesa-dev:{linuxArch} libglu1-mesa-dev:{linuxArch} freeglut3-dev:{linuxArch}");

            var scriptFile = SkiaPath / "tools" / "cross_install_dependencies.sh";
            File.WriteAllText(scriptFile, crossInstallDependencies.ToString());
            sudo($"bash {scriptFile}");
        }
    }

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
        AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");

        gnArgs["cc"] = "clang";
        gnArgs["cxx"] = "'clang++'";

        var crossCompileToolchainArch = Architecture.LinuxCrossToolchain;
        var crossCompileTargetArch = Architecture.LinuxCrossTargetArch;
       
        if (!string.IsNullOrEmpty(crossCompileToolchainArch) && TargetOs == TargetOperatingSystem.Linux)
        {
            var sysroot = $"/usr/{crossCompileToolchainArch}";
            var init = $"'--target={crossCompileTargetArch}'";
            var bin = $"'-B{sysroot}/bin/' ";
            var libs = $"'-L/usr/lib/{crossCompileToolchainArch}'";

            AbsolutePath sysRootPath = sysroot;
            var newestCpp = Directory.EnumerateDirectories(sysRootPath / "include" / "c++")
                .Select(Path.GetFileName)
                .OrderByDescending(x => x)
                .First();

            var includes =
                $"'-I{sysroot}/include', " +
                $"'-I{sysroot}/include/c++/{newestCpp}', " +
                $"'-I{sysroot}/include/c++/{newestCpp}/{crossCompileToolchainArch}', " +
                $"'-I/usr/include/{crossCompileToolchainArch}', " +
                // non arch specific headers last (as system header to ensure it is really last, normal -I will fail)
                $"'-isystem/usr/include/' ";

            AppendToFlagList(gnArgs, "extra_asmflags", $"{init}, '-no-integrated-as', {bin}, {includes}");
            AppendToFlagList(gnArgs, "extra_ldflags", $"{init}, {bin}, {libs}");
            AppendToFlagList(gnArgs, "extra_cflags", $"{init}, {bin}, {includes}");
        }
    }
}