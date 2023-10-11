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
        if (OperatingSystem.IsWindows())
        {
            var candidates = new List<string>()
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Ndk")
            };

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

                foreach (var subDir in Directory.EnumerateDirectories(c).OrderByDescending(Path.GetFileName))
                {
                    if (File.Exists(Path.Combine(subDir, "package.xml")))
                    {
                        return subDir;
                    }
                }

                return null;
            }).FirstOrDefault(d => d != null) ?? string.Empty;
        }

        return string.Empty;
    }
}