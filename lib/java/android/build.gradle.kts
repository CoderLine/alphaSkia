plugins {
    id("java-library")
    `maven-publish`
    signing
}

java {
    toolchain{
        languageVersion.set(JavaLanguageVersion.of(17))
    }
    withSourcesJar()
    withJavadocJar()
}

tasks.jar {
    archiveBaseName = "alphaSkia-android"

    into("native/android-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-x64-jni/")) {
            include("*.so")
        }
    }
    into("native/android-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-x86-jni/")) {
            include("*.so")
        }
    }
    into("native/android-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-arm64-jni/")) {
            include("*.so")
        }
    }
    into("native/android-arm/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-arm-jni/")) {
            include("*.so")
        }
    }
}