using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using static Nuke.Common.EnvironmentInfo;

partial class Build
{
    [Parameter]
    readonly AbsolutePath GradlewExe = GetVariable<string>("GRADLEW_EXE") ??
                                       RootDirectory / "lib" / "java" / $"gradlew{ScriptExtension}";

    [Parameter] readonly AbsolutePath JavaHome = GetVariable<string>("JAVA_HOME");

    Tool GradlewTool => ToolResolver.GetTool(GradlewExe);

    public Target Java => _ => _
        .DependsOn(JavaPack);

    public Target JavaPack => _ => _
        .DependsOn(JavaBuild)
        .Executes(() =>
        {
            GradlewTool("publishAllPublicationsToDistPathRepository",
                workingDirectory: RootDirectory / "lib" / "java");

            if (IsLocalBuild)
            {
                if (Rebuild)
                {
                    (RootDirectory / "dist" / "Maven").DeleteDirectory();
                }

                FileSystemTasks.CopyDirectoryRecursively(RootDirectory / "lib" / "java" / "dist",
                    RootDirectory / "dist" / "Maven",
                    DirectoryExistsPolicy.Merge, FileExistsPolicy.OverwriteIfNewer);
            }
        });


    public Target JavaBuild => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            JavaWriteVersionInfoProperties();

            if (Rebuild)
            {
                GradlewTool("clean",
                    workingDirectory: RootDirectory / "lib" / "java");
                (RootDirectory / "lib" / "java" / "dist").DeleteDirectory();
            }

            GradlewTool("build",
                workingDirectory: RootDirectory / "lib" / "java");
        });

    public Target JavaTest => _ => _
        .Executes(() =>
        {
            GradlewTool("run", workingDirectory: RootDirectory / "test" / "java");
        });

    void JavaWriteVersionInfoProperties()
    {
        string semVer;
        if (IsLocalBuild)
        {
            semVer = $"{VersionInfo.FileVersion.ToString(3)}-LOCAL";
        }
        else if (!IsReleaseBuild)
        {
            semVer = $"{VersionInfo.FileVersion.ToString(3)}-SNAPSHOT";
        }
        else
        {
            semVer = $"{VersionInfo.FileVersion.ToString(3)}";
        }

        var props = $"""
            alphaskiaDescription={VersionInfo.Description}
            alphaskiaAuthorId={VersionInfo.AuthorId}
            alphaskiaAuthorName={VersionInfo.AuthorName}
            alphaskiaOrgUrl={VersionInfo.OrgUrl}
            alphaskiaCompany={VersionInfo.Company}
            alphaskiaVersion={semVer}
            alphaskiaProjectUrl={VersionInfo.ProjectUrl}
            alphaskiaGitUrlHttp={VersionInfo.GitUrlHttp}
            alphaskiaGitUrlGit"={VersionInfo.GitUrlGit}
            alphaskiaLicenseSpdx={VersionInfo.LicenseSpdx}
            alphaskiaLicenseUrl={VersionInfo.LicenseUrl}
            alphaskiaIssuesUrl={VersionInfo.IssuesUrl}
        """;
        (RootDirectory / "lib" / "java" / "local.properties").WriteAllText(props);
    }
}