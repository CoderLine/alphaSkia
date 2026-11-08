using System;
using System.Collections.Generic;
using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<TargetOperatingSystem>))]
public class TargetOperatingSystem : Enumeration
{
    public static readonly TargetOperatingSystem Windows = new()
    {
        Value = "windows",
        SkiaTargetOs = "win",
        SkiaGnArgs =
        {
            ["skia_enable_fontmgr_win_gdi"] = "false",
            ["skia_enable_fontmgr_win"] = "false",
            ["skia_enable_fontmgr_empty"] = "false",
            ["skia_use_system_freetype2"] = "false"
        },
        AlphaSkiaVariants = [Variant.Shared, Variant.Jni, Variant.Node],
        RuntimeIdentifier = "win",
        DotNetRid = "win"
    };

    public static readonly TargetOperatingSystem Linux = new()
    {
        Value = "linux",
        SkiaTargetOs = "linux",
        SkiaGnArgs =
        {
            ["skia_use_system_freetype2"] = "false",
            ["skia_use_fontconfig"] = "false",
            ["skia_enable_fontmgr_empty"] = "false",

            ["skia_enable_fontmgr_FontConfigInterface"] = "false",
            ["skia_enable_fontmgr_fontconfig"] = "false",
            ["skia_enable_fontmgr_custom_directory"] = "false",
            ["skia_enable_fontmgr_custom_embedded"] = "false",
            ["skia_enable_fontmgr_custom_empty"] = "false",
        },
        AlphaSkiaVariants = [Variant.Shared, Variant.Jni, Variant.Node],
        RuntimeIdentifier = "linux",
        DotNetRid = "linux"
    };

    public static readonly TargetOperatingSystem Android = new()
    {
        Value = "android",
        SkiaTargetOs = "android",
        SkiaGnArgs =
        {
            ["skia_use_system_freetype2"] = "false",
            ["skia_enable_fontmgr_android"] = "false",
            ["skia_enable_fontmgr_empty"] = "false",
            ["skia_enable_fontmgr_FontConfigInterface"] = "false",
            ["skia_enable_fontmgr_fontconfig"] = "false",
            ["skia_enable_fontmgr_custom_directory"] = "false",
            ["skia_enable_fontmgr_custom_embedded"] = "false",
            ["skia_enable_fontmgr_custom_empty"] = "false",
            ["skia_use_fontconfig"] = "false"
        },
        AlphaSkiaVariants = [Variant.Shared, Variant.Jni],
        RuntimeIdentifier = "android",
        DotNetRid = "android"
    };

    public static readonly TargetOperatingSystem MacOs = new()
    {
        Value = "macos",
        SkiaTargetOs = "mac",
        SkiaGnArgs =
        {
            ["skia_use_system_freetype2"] = "false",
            ["skia_use_fonthost_mac"] = "false",
            ["skia_enable_fontmgr_empty"] = "false",
        },
        AlphaSkiaVariants = [Variant.Shared, Variant.Jni, Variant.Node],
        RuntimeIdentifier = "macos",
        DotNetRid = "osx"
    };

    // ReSharper disable once InconsistentNaming
    public static readonly TargetOperatingSystem iOS = new()
    {
        Value = "ios",
        SkiaTargetOs = "ios",
        SkiaGnArgs =
        {
            ["skia_use_system_freetype2"] = "false",
            ["skia_use_fonthost_mac"] = "false",
            ["skia_enable_fontmgr_empty"] = "false"
        },
        AlphaSkiaVariants = [Variant.Shared],
        RuntimeIdentifier = "ios"
    };

    // ReSharper disable once InconsistentNaming
    public static readonly TargetOperatingSystem iOSSimulator = new()
    {
        Value = "ios",
        SkiaTargetOs = "ios",
        SkiaGnArgs =
        {
            ["skia_use_system_freetype2"] = "false",
            ["skia_use_fonthost_mac"] = "false",
            ["skia_enable_fontmgr_empty"] = "false"
        },
        ToolchainGnArgs =
        {
            ["ios_use_simulator"] = "true"
        },
        AlphaSkiaVariants = [Variant.Shared],
        RuntimeIdentifier = "iossimulator"
    };

    public static TargetOperatingSystem Current
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return Windows;
            }
            if (OperatingSystem.IsLinux())
            {
                return Linux;
            }

            if (OperatingSystem.IsMacOS())
            {
                return MacOs;
            }

            throw new PlatformNotSupportedException("Need Windows, Linux or MacOS");
        }        
    }
    
    public string SkiaTargetOs { get; private init; }

    /// <summary>
    /// GN args of the libskia build only.
    /// </summary>
    public Dictionary<string, string> SkiaGnArgs { get; } = new();

    /// <summary>
    /// GN args of the toolchain, used by the libskia and the alphaSkia builds.
    /// </summary>
    public Dictionary<string, string> ToolchainGnArgs { get; } = new();

    /// <summary>
    /// The alphaSkia library variants built for this operating system.
    /// </summary>
    public Variant[] AlphaSkiaVariants { get; private init; }

    public string RuntimeIdentifier { get; private init; }
    public object DotNetRid { get; private init; }
}