plugins {
    id("java-library")
    alias(libs.plugins.kotlinJvm)
    `maven-publish`
    signing
}

dependencies {
    implementation(project(":mpp"))
}

java {
    sourceCompatibility = JavaVersion.VERSION_17
    targetCompatibility = JavaVersion.VERSION_17
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