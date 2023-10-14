import org.gradle.api.tasks.testing.logging.TestExceptionFormat
import java.io.FileInputStream
import java.util.*

buildscript {
    repositories {
        gradlePluginPortal()
        google()
        mavenCentral()
    }
}


var sonatypeSigningKeyId = ""
var sonatypeSigningPassword = ""
var sonatypeSigningKey = ""
var ossrhUsername = ""
var ossrhPassword = ""
var sonatypeStagingProfileId = ""

var libDescription = ""
var libAuthorId = ""
var libAuthorName = ""
var libOrgUrl = ""
var libCompany = ""
var libVersion = ""
var libProjectUrl = ""
var libGitUrlHttp = ""
var libGitUrlGit = ""
var libLicenseSpdx = ""
var libLicenseUrl = ""
var libIssuesUrl = ""

val props = Properties()
val propsFile = project.rootProject.file("local.properties")
if (propsFile.exists()) {
    FileInputStream(propsFile).use {
        props.load(it)
    }
}

fun loadSetting(envKey: String, propKey: String, setter: (value: String) -> Unit) {
    if (props.containsKey(propKey)) {
        setter(props.getProperty(propKey))
    } else {
        val env = providers
            .environmentVariable(envKey)
        if (env.isPresent) {
            setter(env.get())
        }
    }
}

loadSetting("OSSRH_USERNAME", "ossrhUsername") { ossrhUsername = it }
loadSetting("OSSRH_PASSWORD", "ossrhPassword") { ossrhPassword = it }
loadSetting("SONATYPE_STAGING_PROFILE_ID", "sonatypeStagingProfileId") { sonatypeStagingProfileId = it }
loadSetting("SONATYPE_SIGNING_KEY_ID", "sonatypeSigningKeyId") { sonatypeSigningKeyId = it }
loadSetting("SONATYPE_SIGNING_PASSWORD", "sonatypeSigningPassword") { sonatypeSigningPassword = it }
loadSetting("SONATYPE_SIGNING_KEY", "sonatypeSigningKey") { sonatypeSigningKey = it }
loadSetting("ALPHASKIA_DESCRIPTION", "alphaskiaDescription") { libDescription = it }
loadSetting("ALPHASKIA_AUTHOR_ID", "alphaskiaAuthorId") { libAuthorId = it }
loadSetting("ALPHASKIA_AUTHOR_NAME", "alphaskiaAuthorName") { libAuthorName = it }
loadSetting("ALPHASKIA_ORG_URL", "alphaskiaOrgUrl") { libOrgUrl = it }
loadSetting("ALPHASKIA_COMPANY", "alphaskiaCompany") { libCompany = it }
loadSetting("ALPHASKIA_VERSION", "alphaskiaVersion") { libVersion = it }
loadSetting("ALPHASKIA_PROJECT_URL", "alphaskiaProjectUrl") { libProjectUrl = it }
loadSetting("ALPHASKIA_GIT_URL_HTTP", "alphaskiaGitUrlHttp") { libGitUrlHttp = it }
loadSetting("ALPHASKIA_GIT_URL_GIT", "alphaskiaGitUrlGit") { libGitUrlGit = it }
loadSetting("ALPHASKIA_LICENSE_SPDX", "alphaskiaLicenseSpdx") { libLicenseSpdx = it }
loadSetting("ALPHASKIA_LICENSE_URL", "alphaskiaLicenseUrl") { libLicenseUrl = it }
loadSetting("ALPHASKIA_ISSUES_URL", "alphaskiaIssuesUrl") { libIssuesUrl = it }



subprojects {
    apply<JavaLibraryPlugin>()
    apply<MavenPublishPlugin>()
    apply(plugin = "maven-publish")

    group = "net.alphatab"
    version = libVersion

    repositories {
        google()
        mavenCentral()
    }

    configure<JavaPluginExtension> {
        toolchain {
            languageVersion.set(JavaLanguageVersion.of(17))
        }
        withSourcesJar()
        withJavadocJar()
    }

    tasks.withType<Test>().configureEach {
        systemProperty("alphaskia.library.path", rootProject.projectDir.resolve("../../dist/"))
        systemProperty("testdata.path", rootProject.projectDir.resolve("../test/"))
        systemProperty("testoutput.path", projectDir.resolve("../test-outputs/"))
        testLogging {
            exceptionFormat = TestExceptionFormat.FULL
            showStackTraces = true
        }
    }

    configure<PublishingExtension> {
        repositories {
//                maven {
//                    name = "sonatype"
//                    url = uri(
//                        if (libVersion.endsWith("SNAPSHOT"))
//                            "https://s01.oss.sonatype.org/content/repositories/snapshots/"
//                        else
//                            "https://s01.oss.sonatype.org/service/local/staging/deploy/maven2/"
//                    )
//                    credentials {
//                        username = ossrhUsername
//                        password = ossrhPassword
//                    }
//                }

            maven {
                name = "DistPath"
                url = rootProject.projectDir.resolve("dist").toURI()
            }
        }

        publications {
            create<MavenPublication>("mavenJava") {
                from(components["java"])
                afterEvaluate {
                    artifactId = tasks.withType<Jar>().first().archiveBaseName.get()

                    pom {
                        description = libDescription
                        url = libProjectUrl
                        licenses {
                            license {
                                name = libLicenseSpdx
                                url = libLicenseUrl
                            }
                        }
                        developers {
                            developer {
                                id = libAuthorId
                                name = libAuthorName
                                organization = libCompany
                                organizationUrl = libOrgUrl
                            }
                        }
                        scm {
                            url = libGitUrlHttp
                            connection = "scm:git:$libGitUrlGit"
                            developerConnection = "scm:git:$libGitUrlGit"
                        }
                        issueManagement {
                            system = "GitHub"
                            url = libIssuesUrl
                        }
                    }
                }
            }
        }
    }
}
