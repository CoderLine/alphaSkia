using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

partial class Build
{
    public Target DotNet => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );
            DotNetTasks.DotNetTest(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );
            DotNetTasks.DotNetPack(_ => _
                .SetProcessWorkingDirectory(RootDirectory / "lib" / "dotnet")
                .SetConfiguration("Release")
            );
        });
}