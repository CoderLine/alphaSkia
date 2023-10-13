using System.Runtime.InteropServices;

namespace AlphaSkia;

/// <summary>
/// This class provides a manual way of resolving and loading the native libraries required
/// by AlphaSkia. If AlphaSkia is installed via NuGet it should automatically resolve the
/// DLLs, but if this fails developers can call <see cref="LoadLibrary"/> to activate the manual loading procedure.
/// </summary>
public static class AlphaSkiaPlatform
{
    /// <summary>
    /// Tries to resolve the native library (libAlphaSkia) needed for AlphaSkia to trigger operations.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown if no native library for the current platform could be resolved.</exception>
    public static void LoadLibrary()
    {
        if (CheckNativeLib())
        {
            return;
        }

        if (TryLoadManually())
        {
            return;
        }

        var rid = BuildKnownRid(out var libExtension);
        throw new PlatformNotSupportedException(
            $"Could not load AlphaSkia native library, install AlphaSkia.Native.<Platform> to enable platform support. (rid: {rid}, libext: {libExtension})");
    }

    private static bool TryLoadManually()
    {
        // if calling does not work, go ahead and try to load the library
        var rid = BuildKnownRid(out var libExtension);
        try
        {
            var alphaSkiaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", rid, "native",
                NativeMethods.AlphaSkiaNativeLibName + libExtension);
            if (File.Exists(alphaSkiaPath))
            {
                if (NativeLibrary.TryLoad(alphaSkiaPath, out _) && CheckNativeLib())
                {
                    return true;
                }
            }
        }
        catch
        {
            // Ignore
        }

        return false;
    }

    private static string BuildKnownRid(out string libExtension)
    {
        var officialRid = RuntimeInformation.RuntimeIdentifier;
        var separator = officialRid.IndexOf('-');
        var officialOs = officialRid[..separator];
        var officialArch = officialRid[(separator + 1)..];

        libExtension = Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => ".so",
            PlatformID.Win32NT => ".dll",
            PlatformID.MacOSX => ".dylib",
            _ => ""
        };
        
        var os = Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => "linux",
            PlatformID.Win32NT => "win",
            PlatformID.MacOSX => "macos",
            _ => officialOs
        };
        if (string.IsNullOrEmpty(os))
        {
            return RuntimeInformation.RuntimeIdentifier;
        }

        var arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            _ => officialArch
        };

        return $"{os}-{arch}";
    }

    private static bool CheckNativeLib()
    {
        try
        {
            NativeMethods.alphaskia_canvas_free(IntPtr.Zero);
            return true;
        }
        catch
        {
            return false;
        }
    }
}