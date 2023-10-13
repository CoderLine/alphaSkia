using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;

partial class Build
{
    public Target Node => _ => _
        .DependsOn(NodePack);

    public Target NodePack => _ => _
        .Unlisted()
        .DependsOn(NodeTest)
        .Executes(() =>
        {
            NpmTasks.Npm("pack", RootDirectory / "lib" / "node" / "alphaskia");
            NpmTasks.Npm("pack", RootDirectory / "lib" / "node" / "alphaskia-windows");
            NpmTasks.Npm("pack", RootDirectory / "lib" / "node" / "alphaskia-linux");
            NpmTasks.Npm("pack", RootDirectory / "lib" / "node" / "alphaskia-macos");
        });

    public Target NodeTest => _ => _
        .Unlisted()
        .DependsOn(NodeBuild)
        .Executes(() =>
        {
            NpmTasks.NpmRun(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "node" / "alphaskia")
                .SetCommand("test"));
        });

    public Target NodeBuild => _ => _
        .Unlisted()
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            NpmTasks.NpmInstall(_ =>
                _.SetProcessWorkingDirectory(RootDirectory / "lib" / "node" / "alphaskia"));
            NpmTasks.NpmInstall(_ =>
                _.SetProcessWorkingDirectory(RootDirectory / "lib" / "node" / "alphaskia-windows"));
            NpmTasks.NpmInstall(_ =>
                _.SetProcessWorkingDirectory(RootDirectory / "lib" / "node" / "alphaskia-linux"));
            NpmTasks.NpmInstall(_ =>
                _.SetProcessWorkingDirectory(RootDirectory / "lib" / "node" / "alphaskia-macos"));

            CopyNodeAddonsToPackages();

            NpmTasks.NpmRun(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "node" / "alphaskia")
                .SetCommand("build"));
        });

    void CopyNodeAddonsToPackages()
    {
        foreach (var subDirectory in DistBasePath.GetDirectories())
        {
            var parts = subDirectory.Name.Split('-');
            if (parts.Length != 4 || parts[0] != "libalphaskianode" || parts[^1] != "node")
            {
                continue;
            }
            
            string packageName;
            if (parts[1] == TargetOperatingSystem.Windows.RuntimeIdentifier)
            {
                packageName = "alphaskia-windows";
            }
            else if (parts[1] == TargetOperatingSystem.Linux.RuntimeIdentifier)
            {
                packageName = "alphaskia-windows";
            }
            else if (parts[1] == TargetOperatingSystem.MacOs.RuntimeIdentifier)
            {
                packageName = "alphaskia-windows";
            }
            else
            {
                continue;
            }

            FileSystemTasks.CopyDirectoryRecursively(subDirectory,
                RootDirectory / "lib" / "node" / packageName / "lib", DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer, excludeFile: fi => fi.Extension != ".node");
        }
    }
}