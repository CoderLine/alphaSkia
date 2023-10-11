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
        if (!IsGitHubActions || TargetOs != TargetOperatingSystem.Linux)
        {
            return;
        }

        var bash = ToolResolver.GetPathTool("bash");
        var sudo = ToolResolver.GetPathTool("sudo");
        var dependenciesScript = SkiaPath / "tools" / "install_dependencies.sh";
        bash($"{dependenciesScript}", workingDirectory: SkiaPath);

        var arch = Architecture.LinuxArch;

        // cross compilation
        if (Architecture != Architecture.X64)
        {
            var crossInstallDependencies = new StringBuilder();
            crossInstallDependencies.AppendLine("echo Install Aptitude");
            crossInstallDependencies.AppendLine("apt-get update");
            crossInstallDependencies.AppendLine("apt-get install -y aptitude");
            
            crossInstallDependencies.AppendLine($"echo Adding Arch {arch}");
            crossInstallDependencies.AppendLine($"dpkg --add-architecture {arch}");

            crossInstallDependencies.AppendLine("echo Modifying sources.list");
            crossInstallDependencies.AppendLine("sed -i \"s/deb /deb [arch=amd64,i386] /\" /etc/apt/sources.list");
            crossInstallDependencies.AppendLine("sed -i \"s/deb-src /deb-src [arch=amd64,i386] /\" /etc/apt/sources.list");

            if (arch == Architecture.Arm || arch == Architecture.Arm64)
            {
                crossInstallDependencies.AppendLine($"echo 'deb [arch={arch}] http://ports.ubuntu.com/ubuntu-ports/ jammy main multiverse universe' >> /etc/apt/sources.list");
                crossInstallDependencies.AppendLine(
                    $"echo 'deb [arch={arch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-security main multiverse universe' >> /etc/apt/sources.list");
                crossInstallDependencies.AppendLine(
                    $"echo 'deb [arch={arch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-backports main multiverse universe' >> /etc/apt/sources.list");
                crossInstallDependencies.AppendLine(
                    $"echo 'deb [arch={arch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-updates main multiverse universe' >> /etc/apt/sources.list");
            }
            crossInstallDependencies.AppendLine("echo Installing Dependencies with sources:");
            crossInstallDependencies.AppendLine("cat /etc/apt/sources.list");
            crossInstallDependencies.AppendLine("echo Updating Packages");
            crossInstallDependencies.AppendLine("apt-get update");
            crossInstallDependencies.AppendLine("echo Installing main build tools");
            crossInstallDependencies.AppendLine($"aptitude install -y crossbuild-essential-{arch} libstdc++-11-dev-{arch}-cross");
            crossInstallDependencies.AppendLine("echo Installing arch libs");
            crossInstallDependencies.AppendLine($"aptitude install -y libfontconfig-dev:{arch} libgl1-mesa-dev:{arch} libglu1-mesa-dev:{arch} freeglut3-dev:{arch}");
            crossInstallDependencies.AppendLine("echo Libs");
            crossInstallDependencies.AppendLine($"ls -l /usr/lib/{Architecture.LinuxCrossToolchain}");
            crossInstallDependencies.AppendLine("echo Includes");
            crossInstallDependencies.AppendLine($"ls /usr/{Architecture.LinuxCrossToolchain}/include");

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
            var init = $"'--sysroot={sysroot}', '--target={crossCompileTargetArch}'";
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
                // last fallback to main headers. this can lead to wierd compilation errors if actually an arch specific header is required
                // in such cases we have to find out which headers are wrongly included and install potentially missing packages.
                // executing the failed clang/clang++ command with additionally "-H -fshow-skipped-includes" can help finding the problematic headers
                $"'-isystem/usr/include/' ";

            AppendToFlagList(gnArgs, "extra_asmflags", $"{init}, '-no-integrated-as', {bin}, {includes}");
            AppendToFlagList(gnArgs, "extra_ldflags", $"{init}, {bin}, {libs}");
            AppendToFlagList(gnArgs, "extra_cflags", $"{init}, {bin}, {includes}");
        }
    }
}