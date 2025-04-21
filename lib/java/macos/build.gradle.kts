import com.vanniktech.maven.publish.JavaLibrary
import com.vanniktech.maven.publish.JavadocJar

plugins {
    `java-library`
    alias(libs.plugins.mavenPublish)
    `maven-publish`
    signing
}

dependencies {
    implementation(project(":main"))
}

java {
    toolchain{
        languageVersion.set(JavaLanguageVersion.of(17))
    }
    withSourcesJar()
    withJavadocJar()
}

tasks.jar {
    archiveBaseName = "alphaSkia-macos"

    into("native/macos-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-macos-x64-jni/")) {
            include("*.dylib")
        }
    }
    into("native/macos-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-macos-arm64-jni/")) {
            include("*.dylib")
        }
    }
}

mavenPublishing {
    coordinates(rootProject.group.toString(), "alphaSkia-windows", rootProject.version.toString())
    configure(JavaLibrary(JavadocJar.Javadoc(), true))
}