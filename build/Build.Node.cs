using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

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
        .DependsOn(NodeBuild)
        .Executes(() =>
        {
            if (Rebuild)
            {
                (RootDirectory / "lib" / "node")
                    .GetFiles("*.tgz", int.MaxValue)
                    .DeleteFiles();
            }

            foreach (var nodePackage in AllNodePackages)
            {
                NpmTasks.Npm("pack", nodePackage);
            }

            if (IsLocalBuild)
            {
                if (Rebuild)
                {
                    (RootDirectory / "dist" / "nodetars").DeleteDirectory();
                }

                foreach (var tgz in (RootDirectory / "lib" / "node").GetFiles("*.tgz", int.MaxValue))
                {
                    FileSystemTasks.CopyFile(tgz,
                        RootDirectory / "dist" / "nodetars" / tgz.Name,
                        FileExistsPolicy.OverwriteIfNewer);
                }

                PrepareTgzForTest();
            }
        });

    void PrepareTgzForTest()
    {
        Log.Information("Preparing TGZ files for test by creating copy without version");
        var files = new System.Collections.Generic.List<string>();
        foreach (var tgz in (RootDirectory / "dist" / "nodetars").GetFiles("*.tgz"))
        {
            // coderline-alphaskia-2.3.0-local.0.tgz
            var nameWithoutVersion = string.Join("-",
                                         tgz.NameWithoutExtension.Split('-').TakeWhile(p => !char.IsDigit(p[0])))
                                     + tgz.Extension;
            FileSystemTasks.CopyFile(tgz,
                RootDirectory / "dist" / "nodetars" / nameWithoutVersion,
                FileExistsPolicy.OverwriteIfNewer);
            files.Add(nameWithoutVersion);
        }

        Log.Information("Preparing TGZ files done {Files}", string.Join(", ", files));
    }

    public Target NodeBuild => _ => _
        .Unlisted()
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            NodeWritePackageJson();

            if (Rebuild)
            {
                NpmTasks.NpmRun(_ => _
                    .SetProcessWorkingDirectory(RootDirectory / "lib" / "node" / "alphaskia")
                    .SetCommand("clean"));
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

    public Target NodeTest => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            PrepareTgzForTest();

            // need to delete package-lock.json due to tgz hash mismatch
            Log.Information("Deleting package-lock.json to avoid integrity checks failing");
            (RootDirectory / "test" / "node" / "package-lock.json").DeleteFile();

            NpmTasks.NpmInstall(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "test" / "node")
                .SetForce(true));
            
            Log.Information("Testing with Node with (OS fonts)", JavaHome, JavaVersion);
            NpmTasks.NpmRun(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "test" / "node")
                .SetCommand("start"));
            
            Log.Information("Testing with Node with (FreeType fonts)", JavaHome, JavaVersion);
            NpmTasks.NpmRun(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "test" / "node")
                .SetCommand("start")
                .SetArguments("--freetype"));
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
                packageName = "alphaskia-linux";
            }
            else if (parts[1] == TargetOperatingSystem.MacOs.RuntimeIdentifier)
            {
                packageName = "alphaskia-macos";
            }
            else
            {
                continue;
            }

            FileSystemTasks.CopyDirectoryRecursively(subDirectory,
                RootDirectory / "lib" / "node" / packageName / "lib" / subDirectory.Name, DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer, excludeFile: fi => fi.Extension != ".node");
        }
    }

    [Parameter] [Secret] readonly string NpmjsAuthToken = GetVariable<string>("NPMJS_AUTH_TOKEN");

    public Target NodePublish => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Requires(() => IsGitHubActions)
        .Executes(() =>
        {
            foreach (var tar in (DistBasePath / "nodetars").GlobFiles("*.tgz"))
            {
                var tag = (IsReleaseBuild) ? "latest" : "alpha";
                NpmTasks.Npm($"publish {tar} --access public --tag {tag}",
                    environmentVariables: Variables
                        .ToDictionary(x => x.Key, x => x.Value)
                        .SetKeyValue("NODE_AUTH_TOKEN", NpmjsAuthToken)
                        .AsReadOnly()
                );
            }
        });
}