using System;
using System.Collections.Generic;
using Nuke.Common;

partial class Build : NukeBuild
{
    // Path handling
    static readonly string ExeExtension = OperatingSystem.IsWindows() ? ".exe" : "";
    static readonly string ScriptExtension = OperatingSystem.IsWindows() ? ".bat" : "";

    public static int Main() => Execute<Build>();

    static void AppendToFlagList(
        Dictionary<string, string> gnArgs,
        string key, string value)
    {
        if (gnArgs.TryGetValue(key, out var flags))
        {
            flags = flags.Trim(' ', ']', '[') + ", ";
        }
        else
        {
            flags = "";
        }

        gnArgs[key] = $"[ {flags}{value} ]";
    }
}