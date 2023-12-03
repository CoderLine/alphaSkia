using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

partial class Build
{
    [Parameter]
    readonly AbsolutePath GradlewExe = GetVariable<string>("GRADLEW_EXE") ??
                                       RootDirectory / "lib" / "java" / $"gradlew{ScriptExtension}";

    [Parameter] readonly AbsolutePath JavaHome = GetVariable<string>("JAVA_HOME") ?? GetVariable<string>("JAVA_HOME_17_X64");

    Tool GradlewTool => ToolResolver.GetTool(GradlewExe);

    public Target Java => _ => _
        .DependsOn(JavaPack);

    public Target JavaPack => _ => _
        .DependsOn(JavaBuild)
        .Executes(() =>
        {
            GradlewTool("publishMavenJavaPublicationToDistPathRepository",
                environmentVariables: Variables
                    .ToDictionary(x => x.Key, x => x.Value)
                    .SetKeyValue("JAVA_HOME", JavaHome)
                    .AsReadOnly(),
                workingDirectory: RootDirectory / "lib" / "java");

            if (IsLocalBuild)
            {
                if (Rebuild)
                {
                    (RootDirectory / "dist" / "maven").DeleteDirectory();
                }

                FileSystemTasks.CopyDirectoryRecursively(RootDirectory / "lib" / "java" / "dist",
                    RootDirectory / "dist" / "maven",
                    DirectoryExistsPolicy.Merge, FileExistsPolicy.OverwriteIfNewer);
            }
        });


    public Target JavaBuild => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(JavaBuildInternal);

    void JavaBuildInternal()
    {
        JavaWriteVersionInfoProperties();

        if (Rebuild)
        {
            GradlewTool("clean",
                environmentVariables: Variables
                    .ToDictionary(x => x.Key, x => x.Value)
                    .SetKeyValue("JAVA_HOME", JavaHome)
                    .AsReadOnly(),
                workingDirectory: RootDirectory / "lib" / "java");
            (RootDirectory / "lib" / "java" / "dist").DeleteDirectory();
        }

        GradlewTool("build",
            environmentVariables:  Variables
                .ToDictionary(x => x.Key, x => x.Value)
                .SetKeyValue("JAVA_HOME", JavaHome)
                .AsReadOnly(),
            workingDirectory: RootDirectory / "lib" / "java");
    }

    public Target JavaTest => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            Log.Debug("Testing with Java at {JavaHome} and version {AlphaSkiaTestVersion}", JavaHome, JavaVersion);
            GradlewTool("run",
                environmentVariables:  Variables
                    .ToDictionary(x => x.Key, x => x.Value)
                    .SetKeyValue("JAVA_HOME", JavaHome)
                    .SetKeyValue("ALPHASKIA_TEST_VERSION", JavaVersion)
                    .AsReadOnly(),
                workingDirectory: RootDirectory / "test" / "java");
        });

    void JavaWriteVersionInfoProperties()
    {
        var semVer = JavaVersion;

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

    string JavaVersion
    {
        get
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

            return semVer;
        }
    }
    
    public Target JavaPublish => _ => _
        .DependsOn(PrepareGitHubArtifacts)
        .Requires(() => IsGitHubActions)
        .Executes(() =>
        {
            // workaround until we know how to upload existing maven packages
            // https://discuss.gradle.org/t/how-to-push-maven-to-ossrh-from-previous-local-publish/46875
            JavaBuildInternal();
            
            GradlewTool("publishMavenJavaPublicationToSonatypeRepository closeAndReleaseSonatypeStagingRepository",
                environmentVariables: Variables
                    .ToDictionary(x => x.Key, x => x.Value)
                    .SetKeyValue("JAVA_HOME", JavaHome)
                    .AsReadOnly(),
                workingDirectory: RootDirectory / "lib" / "java");
        });
}
