using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        bash($"{dependenciesScript}", workingDirectory: SkiaPath);

        var arch = Architecture.LinuxArch;
        sudo($"apt-get update");
        sudo("apt-get install -y aptitude");

        sudo($"dpkg --add-architecture {arch}");

        if (arch == Architecture.Arm || arch == Architecture.Arm64)
        {
            sudo("sed -i \"s/deb /deb [arch=amd64,i386] /\" /etc/apt/sources.list");
            sudo("sed -i \"s/deb-src /deb-src [arch=amd64,i386] /\" /etc/apt/sources.list");
            sudo(
                $"su -c \"echo 'deb [arch={arch}] http://ports.ubuntu.com/ubuntu-ports/ jammy main multiverse universe' >> /etc/apt/sources.list\"");
            sudo(
                $"su -c \"echo 'deb [arch={arch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-security main multiverse universe' >> /etc/apt/sources.list\"");
            sudo(
                $"su -c \"echo 'deb [arch={arch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-backports main multiverse universe' >> /etc/apt/sources.list\"");
            sudo(
                $"su -c \"echo 'deb [arch={arch}] http://ports.ubuntu.com/ubuntu-ports/ jammy-updates main multiverse universe' >> /etc/apt/sources.list\"");
        }

        sudo($"apt-get update");
        sudo($"aptitude install -y crossbuild-essential-{arch} libstdc++-11-dev-{arch}-cross");
        sudo($"aptitude install -y libfontconfig-dev:{arch} libgl1-mesa-dev:{arch} libglu1-mesa-dev:{arch}");
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

        gnArgs["cc"] = "clang";
        gnArgs["cxx"] = "'clang++'";

        string crossCompileToolchainArch = null;
        string crossCompileTargetArch = null;
        if (Architecture == Architecture.X64)
        {
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }
        else if (Architecture == Architecture.X86)
        {
            // TODO
            crossCompileToolchainArch = "i686-linux-gnu";
            crossCompileTargetArch = "i686-linux-gnu";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }
        else if (Architecture == Architecture.Arm64)
        {
            crossCompileToolchainArch = "aarch64-linux-gnu";
            crossCompileTargetArch = "aarch64-linux-gnu";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }
        else if (Architecture == Architecture.Arm)
        {
            crossCompileToolchainArch = "arm-linux-gnueabihf";
            crossCompileTargetArch = "armv7a-linux-gnueabihf";
            AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc'");
        }

        if (!string.IsNullOrEmpty(crossCompileToolchainArch))
        {
            var sysroot = $"/usr/{crossCompileToolchainArch}";
            var init = $"'--sysroot={sysroot}', '--target={crossCompileTargetArch}'";
            var bin = $"'-B{sysroot}/bin/' ";
            var libs = $"'-L{sysroot}/lib/' ";

            AbsolutePath sysRootPath = sysroot;
            var newestCpp = Directory.EnumerateDirectories(sysRootPath / "include" / "c++")
                .Select(Path.GetFileName)
                .OrderByDescending(x => x)
                .First();


            var includes =
                $"'-I{sysroot}/include', " +
                $"'-I{sysroot}/include/c++/{newestCpp}', " +
                $"'-I{sysroot}/include/c++/{newestCpp}/{crossCompileToolchainArch}', " +
                $"'-I/usr/include/' ";

            AppendToFlagList(gnArgs, "extra_asmflags", $"{init}, '-no-integrated-as', {bin}, {includes}");
            AppendToFlagList(gnArgs, "extra_ldflags", $"{init}, {bin}, {libs}");
            AppendToFlagList(gnArgs, "extra_cflags", $"{init}, {bin}, {includes}");
        }
    }
}