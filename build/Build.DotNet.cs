using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

partial class Build
{
    public Target DotNet => _ => _
        .DependsOn(DotNetPack);

    public Target DotNetPack => _ => _
        .Unlisted()
        .DependsOn(DotNetBuild)
        .Executes(() =>
        {
            if (Rebuild)
            {
                (TemporaryDirectory / "packages").DeleteDirectory();
                (RootDirectory / "lib" / "dotnet")
                    .GetFiles("*.nupkg", int.MaxValue)
                    .DeleteFiles();
            }

            DotNetTasks.DotNetPack(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );

            if (IsLocalBuild)
            {
                if (Rebuild)
                {
                    (RootDirectory / "dist" / "NuPkgs").DeleteDirectory();
                }

                foreach (var nupkg in (RootDirectory / "lib" / "dotnet").GetFiles("*.nupkg", int.MaxValue))
                {
                    FileSystemTasks.CopyFile(nupkg,
                        RootDirectory / "dist" / "NuPkgs" / nupkg.Name,
                        FileExistsPolicy.OverwriteIfNewer);
                }
            }
        });

    public Target DotNetBuild => _ => _
        .Unlisted()
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            DotNetWriteVersionInfoProps();

            DotNetTasks.DotNetBuild(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
                .SetForce(Rebuild)
            );
        });

    void DotNetWriteVersionInfoProps()
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

        var props = $"""
            <Project>
                <PropertyGroup>
                    <Version>{semVer}</Version>
                    <AssemblyVersion>{VersionInfo.FileVersion.ToString(2)}</AssemblyVersion>
                    <FileVersion>{VersionInfo.FileVersion.ToString(4)}</FileVersion>
                    <Authors>{VersionInfo.AuthorName}</Authors>
                    <Company>{VersionInfo.Company}</Company>
                    <Product>{VersionInfo.ProductName}</Product>
                    <ProductDescription>{VersionInfo.Description}</ProductDescription>
                    <Copyright>{VersionInfo.Copyright}</Copyright>
                    <PackageLicenseExpression>{VersionInfo.LicenseSpdx}</PackageLicenseExpression>
                    <PackageProjectUrl>{VersionInfo.ProjectUrl}</PackageProjectUrl>
            		<RepositoryType>git</RepositoryType>
                    <RepositoryUrl>{VersionInfo.GitUrlHttp}</RepositoryUrl>
                </PropertyGroup>
            </Project>
        """;
        (RootDirectory / "lib" / "dotnet" / "Version.props").WriteAllText(props);
    }

    public Target DotNetTest => _ => _
        .Executes(() =>
        {
            // flatten tar directory first 
            Log.Information("Flattening NuPkgs files from github");
            foreach (var nupkg in (RootDirectory / "dist" / "NuPkgs").GetFiles("*.nupkg", 3))
            {
                FileSystemTasks.MoveFile(nupkg,
                    RootDirectory / "dist" / "NuPkgs" / nupkg.Name,
                    FileExistsPolicy.OverwriteIfNewer);
            }
            
            DotNetTasks.DotNetRun(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "test" / "dotnet" / "AlphaSkia.Test")
                .SetRuntime(TargetOperatingSystem.Current.DotNetRid + "-" +
                            (Architecture ?? Architecture.Current))
                .AddProcessEnvironmentVariable("NUGET_PACKAGES", TemporaryDirectory / "packages")
            );
        });
}