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
    toolchain {
        languageVersion.set(JavaLanguageVersion.of(17))
    }
}

tasks.jar {
    archiveBaseName = "alphaSkia-windows"

    into("native/win-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-x64-jni/").absolutePath) {
            include("*.dll")
        }
    }
    into("native/win-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-x86-jni/").absolutePath) {
            include("*.dll")
        }
    }
    into("native/win-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-arm64-jni/").absolutePath) {
            include("*.dll")
        }
    }
}

mavenPublishing {
    coordinates(rootProject.group.toString(), "alphaSkia-windows", rootProject.version.toString())
    configure(JavaLibrary(JavadocJar.Javadoc(), true))
}