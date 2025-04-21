using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

partial class Build
{
    [PublicAPI]
    public Target DotNet => t => t
        .DependsOn(DotNetPack);

    [PublicAPI]
    public Target DotNetPack => t => t
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

            DotNetTasks.DotNetPack(t => t
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );

            if (IsLocalBuild)
            {
                if (Rebuild)
                {
                    (RootDirectory / "dist" / "nupkgs").DeleteDirectory();
                }

                foreach (var nupkg in (RootDirectory / "lib" / "dotnet").GetFiles("*.nupkg", int.MaxValue))
                {
                    nupkg.Copy(
                        RootDirectory / "dist" / "nupkgs" / nupkg.Name,
                        ExistsPolicy.MergeAndOverwriteIfNewer
                    );
                }
            }
        });

    [PublicAPI]
    public Target DotNetBuild => t => t
        .Unlisted()
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            DotNetWriteVersionInfoProps();

            DotNetTasks.DotNetBuild(t => t
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

    [Parameter] string Framework;
    
    public Target DotNetTest => t => t
        .DependsOn(PrepareGitHubArtifacts)
        .Requires(() => Framework)
        .Executes(() =>
        {
            Log.Information($"Running DotNet tests on {TargetOperatingSystem.Current.RuntimeIdentifier}-{Architecture.Current} host system (OS fonts)");
            DotNetTasks.DotNetRun(t => t
                .SetProcessWorkingDirectory(RootDirectory / "test" / "dotnet" / "AlphaSkia.Test")
                .SetRuntime(TargetOperatingSystem.Current.DotNetRid + "-" +
                            (Architecture ?? Architecture.Current))
                .SetFramework(Framework)
                .AddProcessEnvironmentVariable("NUGET_PACKAGES", TemporaryDirectory / "packages")
            );
            
            Log.Information($"Running DotNet tests on {TargetOperatingSystem.Current.RuntimeIdentifier}-{Architecture.Current} host system (FreeType fonts)");
            DotNetTasks.DotNetRun(t => t
                .SetProcessWorkingDirectory(RootDirectory / "test" / "dotnet" / "AlphaSkia.Test")
                .SetRuntime(TargetOperatingSystem.Current.DotNetRid + "-" +
                            (Architecture ?? Architecture.Current))
                .SetFramework(Framework)
                .AddProcessEnvironmentVariable("NUGET_PACKAGES", TemporaryDirectory / "packages")
                .SetApplicationArguments("--freetype")
            );
        });

    [Parameter] [Secret] readonly string NugetApiKey = GetVariable<string>("NUGET_API_KEY");

    public Target DotNetPublish => t => t
        .DependsOn(PrepareGitHubArtifacts)
        .Requires(() => IsGitHubActions)
        .Executes(() =>
        {
            DotNetTasks.DotNetNuGetPush(t => t
                    .SetSource("https://api.nuget.org/v3/index.json")
                    .SetApiKey(NugetApiKey)
                    .CombineWith(
                        (DistBasePath / "nupkgs").GlobFiles("*.nupkg")
                        , (s, v) => s
                            .SetTargetPath(v)));
        });
}