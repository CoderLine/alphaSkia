using System;
using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

static class VersionInfo
{
    public const string Company = "CoderLine";
    public const string Description = "A Skia based rendering backend for alphaTab.";
    public static readonly Version FileVersion = GetVariable<Version>("ALPHASKIA_VERSION") ?? new Version(1, 0, 0, 0);
    public static readonly string Copyright = $"Copyright © {DateTime.Now.Year}, Daniel Kuschny";
    public const string AuthorId = "danielku15";
    public const string AuthorName = "Daniel Kuschny";
    public const string ProductName = "alphaSkia";
    public const string ProjectUrl = "https://github.com/CoderLine/alphaSkia";
    public const string GitUrlHttp = "https://github.com/CoderLine/alphaSkia.git";
    public const string GitUrlGit = "git://github.com/CoderLine/alphaSkia.git";
    public const string IssuesUrl = "https://github.com/CoderLine/alphaSkia/issues";
    public const string LicenseSpdx = "BSD-3-Clause";
    public const string LicenseUrl = "https://opensource.org/license/bsd-3-clause";
    public const string OrgUrl = "https://github.com/CoderLine";
}

partial class Build : NukeBuild
{
    // Path handling
    static readonly string ExeExtension = OperatingSystem.IsWindows() ? ".exe" : "";
    static readonly string ScriptExtension = OperatingSystem.IsWindows() ? ".bat" : "";
    static readonly bool IsGitHubActions = GetVariable<bool>("GITHUB_ACTIONS");
    static readonly AbsolutePath DistBasePath = RootDirectory / "dist";
    static readonly AbsolutePath ArtifactBasePath = RootDirectory / "artifacts";
    
    [Parameter] readonly bool IsReleaseBuild = GetVariable<bool?>("IS_RELEASE_BUILD") ?? false;
    [Parameter] readonly bool Rebuild;
    
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
        .OnlyWhenStatic(() => IsGitHubActions)
        .Executes(() =>
        {
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
    
    bool CanUseCachedBinaries(string buildTarget, Variant variant)
    {
        if (!UseCache)
        {
            Log.Debug($"Cache for {GetLibDirectory(buildTarget, variant: variant)} is disabled");
            return false;
        }

        if (!HasCachedFiles(buildTarget, variant))
        {
            return false;
        }
        
        return true;
    }
}