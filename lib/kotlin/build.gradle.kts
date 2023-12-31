import java.io.FileInputStream
import java.util.*

plugins {
    //trick: for the same plugin versions in all sub-modules
    alias(libs.plugins.androidLibrary).apply(false)
    alias(libs.plugins.kotlinMultiplatform).apply(false)
    alias(libs.plugins.kotlinCocoapods).apply(false)
    alias(libs.plugins.kotlinJvm).apply(false)
    alias(libs.plugins.nexusPublish)
}

repositories {
    google()
    mavenCentral()
}

val javaVersion = JavaVersion.VERSION_17

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

group = "net.alphatab"
version = libVersion

subprojects {
    apply<MavenPublishPlugin>()
    apply(plugin = "maven-publish")
    apply(plugin = "signing")

    repositories {
        google()
        mavenCentral()
    }

    group = "net.alphatab"
    version = libVersion

    configure<SigningExtension> {
        if (sonatypeSigningKeyId.isNotBlank() && sonatypeSigningKey.isNotBlank() && sonatypeSigningPassword.isNotBlank()) {
            useInMemoryPgpKeys(sonatypeSigningKeyId, sonatypeSigningKey, sonatypeSigningPassword)
            sign(extensions.getByType<PublishingExtension>().publications)
        } else if (System.getenv("GITHUB_ACTIONS") == "true") {
            throw Exception("no signing configured")
        }
    }

    afterEvaluate {
        configure<PublishingExtension> {
            repositories {
                maven {
                    name = "DistPath"
                    url = rootProject.projectDir.resolve("dist").toURI()
                }
            }

            publications {
                withType<MavenPublication> {
                    pom {
                        name = artifactId
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

nexusPublishing {
    repositories.sonatype {
        nexusUrl.set(uri("https://s01.oss.sonatype.org/service/local/"))
        snapshotRepositoryUrl.set(uri("https://s01.oss.sonatype.org/content/repositories/snapshots/"))
        username = ossrhUsername
        password = ossrhPassword
    }
}
