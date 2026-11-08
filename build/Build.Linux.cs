using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

partial class Build
{
    Target InstallDependenciesLinux => t => t
        .Unlisted()
        .OnlyWhenStatic(() => OperatingSystem.IsLinux() && IsGitHubActions)
        .After(PrepareGitHubArtifacts,
            LibAlphaSkiaGitSyncDeps, LibAlphaSkiaPatchSkiaBuildFiles,
            LibSkiaGitSyncDeps, LibSkiaPatchSkiaBuildFiles)
        .Before(LibSkia, LibAlphaSkia, LibAlphaSkiaTest)
        .Executes(() =>
        {
            var installDependencies = new StringBuilder();
            installDependencies.AppendLine("#!/bin/bash");
            installDependencies.AppendLine("set -e");

            var packages = new List<string>
            {
                "build-essential"
            };

            // Cross compilation build tools if needed
            var linuxArch = Architecture.LinuxArch;
            if (Architecture != Architecture.X64 && TargetOs == TargetOperatingSystem.Linux)
            {
                installDependencies.AppendLine($"echo Adding Arch {linuxArch}");
                installDependencies.AppendLine($"dpkg --add-architecture {linuxArch}");

                // NOTE: This happens within Nuke, not in the shell script
                // https://github.com/actions/runner-images/issues/10901
                var ubuntu24Sources = TemporaryDirectory / "ubuntu.sources";
                ubuntu24Sources.WriteAllText(
                    """
                    Types: deb
                    URIs: http://archive.ubuntu.com/ubuntu/
                    Suites: noble
                    Components: main restricted universe
                    Architectures: amd64,i386

                    Types: deb
                    URIs: http://security.ubuntu.com/ubuntu/
                    Suites: noble-security
                    Components: main restricted universe
                    Architectures: amd64,i386

                    Types: deb
                    URIs: http://archive.ubuntu.com/ubuntu/
                    Suites: noble-updates
                    Components: main restricted universe
                    Architectures: amd64,i386

                    Types: deb
                    URIs: http://ports.ubuntu.com/ubuntu-ports/
                    Suites: noble
                    Components: main restricted multiverse universe
                    Architectures: arm64,armhf

                    Types: deb
                    URIs: http://ports.ubuntu.com/ubuntu-ports/
                    Suites: noble-security
                    Components: main restricted multiverse universe
                    Architectures: arm64,armhf

                    Types: deb
                    URIs: http://ports.ubuntu.com/ubuntu-ports/
                    Suites: noble-backports
                    Components: main restricted multiverse universe
                    Architectures: arm64,armhf

                    Types: deb
                    URIs: http://ports.ubuntu.com/ubuntu-ports/
                    Suites: noble-updates
                    Components: main restricted multiverse universe
                    Architectures: arm64,armhf
                    """
                );

                if (Architecture == Architecture.Arm || Architecture == Architecture.Arm64)
                {
                    ubuntu24Sources.AppendAllText(content:
                        $"""
                         Types: deb
                         URIs: http://ports.ubuntu.com/ubuntu-ports/
                         Suites: noble
                         Components: main restricted multiverse universe
                         Architectures: {linuxArch}

                         Types: deb
                         URIs: http://ports.ubuntu.com/ubuntu-ports/
                         Suites: noble-security
                         Components: main restricted multiverse universe
                         Architectures: {linuxArch}

                         Types: deb
                         URIs: http://ports.ubuntu.com/ubuntu-ports/
                         Suites: noble-backports
                         Components: main restricted multiverse universe
                         Architectures: {linuxArch}

                         Types: deb
                         URIs: http://ports.ubuntu.com/ubuntu-ports/
                         Suites: noble-updates
                         Components: main restricted multiverse universe
                         Architectures: {linuxArch}
                         """
                    );
                }

                installDependencies.AppendLine("echo Updating APT sources");
                installDependencies.AppendLine($"mv {ubuntu24Sources} /etc/apt/sources.list.d/ubuntu.sources");

                packages.Add($"crossbuild-essential-{linuxArch}");
                packages.Add($"libstdc++-11-dev-{linuxArch}-cross");
            }
            else
            {
                // no cross compilation packages
                linuxArch = "";
            }



            // dependent libraries
            var linuxArchSuffix = string.IsNullOrEmpty(linuxArch) ? "" : $":{linuxArch}";
            installDependencies.AppendLine("echo Installing libs");

            packages.Add($"libfontconfig-dev{linuxArchSuffix}");
            packages.Add($"libgl1-mesa-dev{linuxArchSuffix}");
            packages.Add($"libglu1-mesa-dev{linuxArchSuffix}");
            packages.Add($"freeglut3-dev{linuxArchSuffix}");

            installDependencies.AppendLine("apt-get update");
            installDependencies.AppendLine($"apt-get install -y {string.Join(" ", packages)}");
            
            var scriptFile = TemporaryDirectory / "install_dependencies.sh";
            File.WriteAllText(scriptFile, installDependencies.ToString());
            ToolResolver.GetPathTool("sudo")($"bash {scriptFile}");
        });

    void SetClangLinux(Dictionary<string, string> gnArgs)
    {
        AppendToFlagList(gnArgs, "extra_cflags", "'-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM'");
        // The statically linked runtime and Skia's FreeType must neither be exported nor interposed: only the
        // alphaSkia API (and the JNI / N-API entry points) leave the library, the rest is subject to --gc-sections.
        AppendToFlagList(gnArgs, "extra_ldflags", "'-static-libstdc++', '-static-libgcc', '-Wl,--exclude-libs,ALL', '-Wl,--strip-debug'");

        // exploit mitigations for Skia, its third party libraries and alphaSkia (used for Android builds too)
        var fortifyLevel = TargetOs == TargetOperatingSystem.Android ? 2 : 3;
        AppendToFlagList(gnArgs, "extra_cflags", $"'-fstack-protector-strong', '-U_FORTIFY_SOURCE', '-D_FORTIFY_SOURCE={fortifyLevel}'");
        if (Architecture == Architecture.X64 || Architecture == Architecture.X86)
        {
            AppendToFlagList(gnArgs, "extra_cflags", "'-fstack-clash-protection', '-fcf-protection=full'");
        }
        else if (Architecture == Architecture.Arm64)
        {
            AppendToFlagList(gnArgs, "extra_cflags", "'-fstack-clash-protection', '-mbranch-protection=standard'");
        }
        AppendToFlagList(gnArgs, "extra_ldflags", "'-Wl,-z,relro', '-Wl,-z,now', '-Wl,-z,noexecstack'");

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
                // non arch specific headers last, after all -I and -isystem directories: Skia passes its bundled third
                // party headers (e.g. ICU) as -isystem, which the host's versions in /usr/include must not shadow
                $"'-idirafter/usr/include/' ";

            AppendToFlagList(gnArgs, "extra_asmflags", $"{init}, '-no-integrated-as', {bin}, {includes}");
            AppendToFlagList(gnArgs, "extra_ldflags", $"{init}, {bin}, {libs}");
            AppendToFlagList(gnArgs, "extra_cflags", $"{init}, {bin}, {includes}");
        }
    }
}
