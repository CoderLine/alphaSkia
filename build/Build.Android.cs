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
        List<string> candidates;
        if (OperatingSystem.IsWindows())
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            candidates =
            [
                Path.Combine(localAppData, "Android", "Sdk", "ndk"),
                Path.Combine(localAppData, "Android", "Ndk")
            ];
        }
        else if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidates =
            [
                Path.Combine(home, "Library", "Android", "sdk", "ndk")
            ];
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidates =
            [
                Path.Combine(home, "Android", "Sdk", "ndk")
            ];
        }

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

            return Directory.EnumerateDirectories(c)
                .Where(subDir => File.Exists(Path.Combine(subDir, "package.xml")))
                .OrderByDescending(subDir =>
                {
                    var name = Path.GetFileName(subDir);
                    return Version.TryParse(name, out var v) ? v : new Version(0, 0);
                })
                .FirstOrDefault();
        }).FirstOrDefault(d => d != null) ?? string.Empty;
    }
}