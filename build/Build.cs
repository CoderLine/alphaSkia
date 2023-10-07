using System;
using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

partial class Build : NukeBuild
{
    // Path handling
    static readonly string ExeExtension = OperatingSystem.IsWindows() ? ".exe" : "";
    static readonly string ScriptExtension = OperatingSystem.IsWindows() ? ".bat" : "";
    static readonly bool IsGitHubActions = GetVariable<bool>("GITHUB_ACTIONS");
    static readonly AbsolutePath DistBasePath = RootDirectory / "dist";
    static readonly AbsolutePath ArtifactBasePath = RootDirectory / "artifacts";

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
        .Executes(() =>
        {
            if (!IsGitHubActions)
            {
                Log.Debug("Skipping GitHub Artifact preparation, GITHUB_ACTIONS was not set");
                return;
            }
            
            // We auto download all artifacts of all dependencies
            // which results in a nested structure like
            // dist/<artifactname>/<files>
            // but we want them at dist/<files>
            var dist = DistBasePath;
            if (!dist.DirectoryExists())
            {
                Log.Debug("Skipping GitHub Artifact preparation, no dependencies");

                // nothing to do
                return;
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
        });
    
    bool CanUseCachedBinaries(string buildTarget, string targetOsDir)
    {
        if (!UseCache)
        {
            return false;
        }

        if (!HasCachedFiles(buildTarget, targetOsDir))
        {
            return false;
        }
        
        return true;
    }
}