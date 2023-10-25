using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<Architecture>))]
public class Architecture : Enumeration
{
    public static readonly Architecture X64 = new()
        { Value = "x64", LinuxArch = "amd64", LinuxCrossToolchain = "", LinuxCrossTargetArch = "" };

    public static Architecture X86 = new()
    {
        Value = "x86", LinuxArch = "i386", LinuxCrossToolchain = "i686-linux-gnu",
        LinuxCrossTargetArch = "i686-linux-gnu"
    };

    public static readonly Architecture Arm = new()
    {
        Value = "arm", LinuxArch = "armhf", LinuxCrossToolchain = "arm-linux-gnueabihf",
        LinuxCrossTargetArch = "armv7a-linux-gnueabihf"
    };

    public static readonly Architecture Arm64 = new()
    {
        Value = "arm64", LinuxArch = "arm64", LinuxCrossToolchain = "aarch64-linux-gnu",
        LinuxCrossTargetArch = "aarch64-linux-gnu"
    };

    public static Architecture Current
    {
        get
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case System.Runtime.InteropServices.Architecture.X86:
                    return X86;
                case System.Runtime.InteropServices.Architecture.X64:
                    return X64;
                case System.Runtime.InteropServices.Architecture.Arm:
                    return Arm;
                case System.Runtime.InteropServices.Architecture.Arm64:
                    return Arm64;
                default:
                    throw new PlatformNotSupportedException("Building on this platform is not fully supported");
            }
        }
    }

    public string LinuxArch { get; private set; }
    public string LinuxCrossToolchain { get; private set; }
    public string LinuxCrossTargetArch { get; private set; }
}