using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

partial class Build
{
    public Target DotNet => _ => _
        .DependsOn(DotNetPack);
    
    public Target DotNetPack => _ => _
        .DependsOn(DotNetTest)
        .Executes(() =>
        {
            DotNetTasks.DotNetPack(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );
        });
    public Target DotNetTest => _ => _
        .DependsOn(DotNetBuild)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );
        });
    public Target DotNetBuild => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );
        });
    
}