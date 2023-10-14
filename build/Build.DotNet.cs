using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

partial class Build
{
    public Target DotNet => _ => _
        .DependsOn(DotNetPack);
    
    public Target DotNetPack => _ => _
        .Unlisted()
        .DependsOn(DotNetTest)
        .Executes(() =>
        {
            DotNetTasks.DotNetPack(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );
        });
    public Target DotNetTest => _ => _
        .Unlisted()
        .DependsOn(DotNetBuild)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );
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
}