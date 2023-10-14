using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Npm;

partial class Build
{
    public Target Node => _ => _
        .DependsOn(NodePack);

    AbsolutePath[] AllNodePackages => new[]
    {
        RootDirectory / "lib" / "node" / "alphaskia",
        RootDirectory / "lib" / "node" / "alphaskia-windows",
        RootDirectory / "lib" / "node" / "alphaskia-linux",
        RootDirectory / "lib" / "node" / "alphaskia-macos"
    };

    public Target NodePack => _ => _
        .Unlisted()
        .DependsOn(NodeTest)
        .Executes(() =>
        {
            foreach (var nodePackage in AllNodePackages)
            {
                NpmTasks.Npm("pack", nodePackage);
            }
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
            NodeWritePackageJson();

            if (Rebuild)
            {
                foreach (var nodePackage in AllNodePackages)
                {
                    NpmTasks.Npm("clean", nodePackage);
                }
            }
            
            foreach (var nodePackage in AllNodePackages)
            {
                NpmTasks.NpmInstall(_ =>
                    _.SetProcessWorkingDirectory(nodePackage));
            }
            
            CopyNodeAddonsToPackages();

            NpmTasks.NpmRun(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "node" / "alphaskia")
                .SetCommand("build"));
        });

    void NodeWritePackageJson()
    {
        string semVer;
        if (IsLocalBuild)
        {
            semVer = $"{VersionInfo.FileVersion.ToString(3)}-local.{VersionInfo.FileVersion.Revision}";
        }
        else if (!IsReleaseBuild)
        {
            semVer = $"{VersionInfo.FileVersion.ToString(3)}-alpha.{VersionInfo.FileVersion.Revision}";
        }
        else
        {
            semVer = $"{VersionInfo.FileVersion.ToString(3)}";
        }

        var packageJsons = new[]
        {
            RootDirectory / "lib" / "node" / "alphaskia" / "package.json",
            RootDirectory / "lib" / "node" / "alphaskia-linux" / "package.json",
            RootDirectory / "lib" / "node" / "alphaskia-macos" / "package.json",
            RootDirectory / "lib" / "node" / "alphaskia-windows" / "package.json"
        };

        foreach (var jsonPath in packageJsons)
        {
            var jsonContent = JsonNode.Parse(jsonPath.ReadAllText())!.AsObject();
            jsonContent["version"] = semVer;
            jsonContent["homepage"] = VersionInfo.ProjectUrl;
            jsonContent["bugs"] = new JsonObject
            {
                ["url"] = VersionInfo.IssuesUrl
            };
            jsonContent["license"] = VersionInfo.LicenseSpdx;
            jsonContent["author"] = new JsonObject
            {
                ["name"] = VersionInfo.AuthorName
            };
            jsonContent["repository"] = new JsonObject
            {
                ["type"] = "git",
                ["url"] = VersionInfo.GitUrlHttp,
                ["directory"] = RootDirectory.GetRelativePathTo(jsonPath.Parent).ToUnixRelativePath().ToString()
            };
            
            jsonPath.WriteAllText(jsonContent.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        }
    }

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