using System;
using System.Collections.Generic;
using System.Linq;
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

    static readonly HashSet<string> AllLibExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll",
        ".lib",
        ".so",
        ".dylib",
        ".node",
        ".a",
        ".exe"
    };

    Target PrepareGitHubArtifacts => _ => _
        .OnlyWhenStatic(() => IsGitHubActions)
        .Executes(() =>
        {
            // The Github artifact actions are really messy as you can hardly control the folder structure
            // you end up with, this action tries to consolidate things to our knowledge.
            var dist = DistBasePath / ".organize";
            if (!dist.DirectoryExists())
            {
                Log.Debug("Skipping GitHub Artifact preparation, no dependencies to organize");
                return;
            }

            var directories = dist.GlobDirectories("**").ToArray();
            Log.Debug("Starting GitHub Artifact Preparation");

            foreach (var subDir in directories)
            {
                FlattenFolder(subDir);
            }
        });

    static void FlattenFolder(AbsolutePath subDir)
    {
        // could have been moved recusively
        if (!subDir.DirectoryExists())
        {
            return;
        }

        Log.Debug("Flattening folder {Folder}", subDir);

        // known include path 
        if (subDir.Name == "include")
        {
            FileSystemTasks.MoveDirectoryToDirectory(subDir, DistBasePath, DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer);
            Log.Debug("   Treated as header include dir");
            return;
        }

        var files = subDir.GetFiles().ToArray();

        foreach (var file in files)
        {
            if (!file.FileExists())
            {
                continue;
            }
            
            if (AllLibExtensions.Contains(file.Extension) || file.NameWithoutExtension == "libalphaskiatest")
            {
                // found a library or test executable  dir
                FileSystemTasks.MoveDirectoryToDirectory(subDir, DistBasePath, DirectoryExistsPolicy.Merge,
                    FileExistsPolicy.OverwriteIfNewer);
                Log.Debug("   Treated as library dir");
                return;
            }

            if (file.Extension is ".jar" or ".pom")
            {
                // find maven base. 
                // <Maven>/net/alphatab/net.alphatab.alphaskia/<version>/file
                var netDir = file / ".." / ".." / ".." / "..";
                if (netDir.DirectoryExists() && netDir.Name == "net")
                {
                    FileSystemTasks.MoveDirectoryToDirectory(netDir, DistBasePath / "Maven",
                        DirectoryExistsPolicy.Merge,
                        FileExistsPolicy.OverwriteIfNewer);
                    return;
                }
            }
            else if (file.Extension is ".nupkg" or ".snupkg")
            {
                // flatten nugets
                FileSystemTasks.MoveFileToDirectory(file, DistBasePath / "NuPkgs",
                    FileExistsPolicy.OverwriteIfNewer);
            }
            else if (file.Extension is ".tgz")
            {
                FileSystemTasks.MoveFileToDirectory(file, DistBasePath / "NodeTars",
                    FileExistsPolicy.OverwriteIfNewer);
            }
        }
    }

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