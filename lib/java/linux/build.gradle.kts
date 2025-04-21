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
}

tasks.jar {
    archiveBaseName = "alphaSkia-linux"

    into("native/linux-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-linux-x64-jni/")) {
            include("*.so")
        }
    }
    into("native/linux-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-linux-x86-jni/")) {
            include("*.so")
        }
    }
    into("native/linux-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-linux-arm64-jni/")) {
            include("*.so")
        }
    }
}

mavenPublishing {
    coordinates(rootProject.group.toString(), "alphaSkia-linux", rootProject.version.toString())
    configure(JavaLibrary(JavadocJar.Javadoc(), true))
}