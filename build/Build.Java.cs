using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using static Nuke.Common.EnvironmentInfo;

partial class Build
{
    [Parameter]
    readonly AbsolutePath GradlewExe = GetVariable<string>("GRADLEW_EXE") ??
                                       RootDirectory / "lib" / "java" / $"gradlew{ScriptExtension}";
    [Parameter]
    readonly AbsolutePath JavaHome = GetVariable<string>("JAVA_HOME");

    Tool GradlewTool => ToolResolver.GetTool(GradlewExe);

    public Target Java => _ => _
        .DependsOn(JavaPack);
    
    public Target JavaPack => _ => _
        .DependsOn(JavaBuild)
        .Executes(() =>
        {
            GradlewTool("assemble",
                workingDirectory: RootDirectory / "lib" / "java");
        });
    
        
    public Target JavaBuild => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            GradlewTool("build",
                workingDirectory: RootDirectory / "lib" / "java");
        });
}