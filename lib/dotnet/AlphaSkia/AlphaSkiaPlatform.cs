using System.Reflection;
using System.Runtime.InteropServices;

namespace AlphaSkia;

/// <summary>
/// This class provides a manual way of resolving and loading the native libraries required
/// by alphaSkia. If alphaSkia is installed via NuGet it should automatically resolve the
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
                if (TryLoadLibrary(alphaSkiaPath) && CheckNativeLib())
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

    private static bool TryLoadLibrary(string path)
    {
        // on .net FX or some .netstandard environments the NativeLibrary might not be available
        // try to use it if available, otherwise fallback.
        var nativeLibraryTryLoadMethod = typeof(DllImportAttribute).Assembly
            .GetType("System.Runtime.InteropServices.NativeLibrary")
            ?.GetMethod("TryLoad", BindingFlags.Static | BindingFlags.Public);
        if (nativeLibraryTryLoadMethod != null)
        {
            return (bool)nativeLibraryTryLoadMethod.Invoke(null, new object[]
            {
                path, IntPtr.Zero
            })!;
        }

        var handle = LoadLibraryWindows(path);
        return handle != IntPtr.Zero;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryW",
        ExactSpelling = true)]
    private static extern nint LoadLibraryWindows(string fileName);

    private static string FallbackRuntimeIdentifier =>
#if NETSTANDARD2_0
        "windows-" + RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
#else
        RuntimeInformation.RuntimeIdentifier;
#endif

    private static string BuildKnownRid(out string libExtension)
    {
        var officialRid = FallbackRuntimeIdentifier;
        var separator = officialRid.IndexOf('-');
        var officialOs = officialRid.Substring(0, separator);
        var officialArch = officialRid.Substring(separator + 1);

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
            return FallbackRuntimeIdentifier;
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