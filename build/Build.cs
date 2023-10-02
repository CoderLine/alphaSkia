using System;
using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Octokit;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

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

    Target PrepareGitHubArtifacts => _ => _
        .OnlyWhenDynamic(() => GetVariable("GITHUB_ACTION") != null)
        .Executes(() =>
        {
            // We auto download all artifacts of all dependencies
            // which results in a nested structure like
            // dist/<artifactname>/<files>
            // but we want them at dist/<files>
            var dist = RootDirectory / "dist";
            if (!dist.DirectoryExists())
            {
                // nothing to do
                return;
            }

            if (OperatingSystem.IsWindows())
            {
                ToolResolver.GetPathTool("tree")("/F", workingDirectory: dist);
            }
            
            var includeDir = dist / "include";
            if (includeDir.DirectoryExists())
            {
                // seems we have a proper structure already
                return;
            }
            
            foreach (var artifactDir in dist.GetDirectories())
            {
                Log.Information("Flattening artifact dir {artifactDir}", artifactDir);

                foreach (var artifactSubDir in artifactDir.GetDirectories())
                {
                    FileSystemTasks.MoveDirectoryToDirectory(artifactSubDir, dist, DirectoryExistsPolicy.Merge,
                        FileExistsPolicy.OverwriteIfNewer);
                }
            }
            
            if (OperatingSystem.IsWindows())
            {
                ToolResolver.GetPathTool("tree")("/F", workingDirectory: dist);
            }
        });
}