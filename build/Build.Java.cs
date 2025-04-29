using System.Collections.Generic;
using JetBrains.Annotations;
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

    [Parameter]
    readonly AbsolutePath JavaHome = GetVariable<string>("JAVA_HOME") ?? GetVariable<string>("JAVA_HOME_17_X64");

    Tool GradlewTool => ToolResolver.GetTool(GradlewExe);

    [PublicAPI]
    public Target Java => t => t
        .DependsOn(JavaPack);

    [PublicAPI]
    public Target JavaPack => t => t
        .DependsOn(JavaBuild)
        .Executes(() =>
        {
            GradlewTool("publishAllPublicationsToDistPathRepository",
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

                (RootDirectory / "lib" / "java" / "dist")
                    .Copy(
                        RootDirectory / "dist" / "maven",
                        ExistsPolicy.MergeAndOverwriteIfNewer
                    );
            }
        });


    [PublicAPI]
    public Target JavaBuild => t => t
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
            environmentVariables: Variables
                .ToDictionary(x => x.Key, x => x.Value)
                .SetKeyValue("JAVA_HOME", JavaHome)
                .AsReadOnly(),
            workingDirectory: RootDirectory / "lib" / "java");
    }

    [PublicAPI]
    public Target JavaTest => t => t
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            Log.Information("Testing with Java at {JavaHome} and version {AlphaSkiaTestVersion} (OS fonts)", JavaHome,
                JavaVersion);
            GradlewTool("run",
                environmentVariables: Variables
                    .ToDictionary(x => x.Key, x => x.Value)
                    .SetKeyValue("JAVA_HOME", JavaHome)
                    .SetKeyValue("ALPHASKIA_TEST_VERSION", JavaVersion)
                    .AsReadOnly(),
                workingDirectory: RootDirectory / "test" / "java");

            Log.Information("Testing with Java at {JavaHome} and version {AlphaSkiaTestVersion} (FreeType fonts)",
                JavaHome, JavaVersion);
            GradlewTool("run --args=--freetype",
                environmentVariables: Variables
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
            if (IsReleaseBuild)
            {
                semVer = $"{VersionInfo.FileVersion.ToString(3)}";
            }
            else if (IsLocalBuild)
            {
                semVer = $"{VersionInfo.FileVersion.ToString(3)}-LOCAL";
            }
            else
            {
                semVer = $"{VersionInfo.FileVersion.ToString(3)}-SNAPSHOT";
            }

            return semVer;
        }
    }

    [PublicAPI]
    public Target JavaPublish => t => t
        .DependsOn(PrepareGitHubArtifacts)
        .Executes(() =>
        {
            // workaround until we know how to upload existing maven packages
            // https://discuss.gradle.org/t/how-to-push-maven-to-ossrh-from-previous-local-publish/46875
            JavaBuildInternal();

            GradlewTool("publishToMavenCentral",
                environmentVariables: Variables
                    .ToDictionary(x => x.Key, x => x.Value)
                    .SetKeyValue("JAVA_HOME", JavaHome)
                    .AsReadOnly(),
                workingDirectory: RootDirectory / "lib" / "java");
        });
}