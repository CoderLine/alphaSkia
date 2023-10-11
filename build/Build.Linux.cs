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
        var installDependencies = new StringBuilder();
        installDependencies.AppendLine("#!/bin/bash");
        installDependencies.AppendLine("set -e");

        // Using aptitude here because github actions runners have quite some packages preinstalled
        // which leads to many version conflicts after adding more sources. aptitude can resolve those 
        // conflicts easier
        installDependencies.AppendLine("echo Install Aptitude");
        installDependencies.AppendLine("apt-get update");
        installDependencies.AppendLine("apt-get install -y aptitude");
        
        // Main system build tools
        installDependencies.AppendLine("apt-get install -y build-essential");
        
        // Cross compilation build tools if needed
        var linuxArch = Architecture.LinuxArch;
        if (Architecture != Architecture.X64 && TargetOs == TargetOperatingSystem.Linux)
        {
            installDependencies.AppendLine($"echo Adding Arch {linuxArch}");
            installDependencies.AppendLine($"dpkg --add-architecture {linuxArch}");

            installDependencies.AppendLine("echo Modifying sources.list");
            installDependencies.AppendLine("sed -i \"s/deb /deb [arch=amd64,i386] /\" /etc/apt/sources.list");
            installDependencies.AppendLine("sed -i \"s/deb-src /deb-src [arch=amd64,i386] /\" /etc/apt/sources.list");

            if (Architecture == Architecture.Arm || Architecture == Architecture.Arm64)
            {
                installDependencies.AppendLine($"echo 'deb [arch={linuxArch}] http://ports.ubuntu.com/ubuntu-ports/ jammy main multiverse universe' >> /etc/apt/sources.list");
                installDependencies.AppendLine(
                    $"echo 'deb [arch={linuxArch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-security main multiverse universe' >> /etc/apt/sources.list");
                installDependencies.AppendLine(
                    $"echo 'deb [arch={linuxArch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-backports main multiverse universe' >> /etc/apt/sources.list");
                installDependencies.AppendLine(
                    $"echo 'deb [arch={linuxArch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-updates main multiverse universe' >> /etc/apt/sources.list");
            }
            
            installDependencies.AppendLine("echo Updating Packages");
            installDependencies.AppendLine("apt-get update");
            installDependencies.AppendLine("echo Installing main build tools");
            installDependencies.AppendLine($"aptitude install -y crossbuild-essential-{linuxArch} libstdc++-11-dev-{linuxArch}-cross");
        }
        else 
        {
            // no cross compilation packages
            linuxArch = "";
        }
        
        // skia libraries
        var libuxArchSuffix = string.IsNullOrEmpty(linuxArch) ? "" : $":{linuxArch}";
        installDependencies.AppendLine("echo Installing libs");
        installDependencies.AppendLine($"aptitude install -y libfontconfig-dev{libuxArchSuffix} libgl1-mesa-dev{libuxArchSuffix} libglu1-mesa-dev{libuxArchSuffix} freeglut3-dev{libuxArchSuffix}");

        var scriptFile = TemporaryDirectory / "install_dependencies.sh";
        File.WriteAllText(scriptFile, installDependencies.ToString());
        ToolResolver.GetPathTool("sudo")($"bash {scriptFile}");
    }

    void SetClangLinux(Dictionary<string, string> gnArgs)
    {
        AppendToFlagList(gnArgs, "extra_cflags", "'-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM'");
        AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");

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