import com.android.build.gradle.internal.tasks.factory.dependsOn
import com.vanniktech.maven.publish.AndroidSingleVariantLibrary

plugins {
    alias(libs.plugins.android.library)
    alias(libs.plugins.mavenPublish)
    `maven-publish`
    signing
}

dependencies {
    implementation(project(":main"))
}

tasks.register<Copy>("copyJniForAndroid") {
    // https://developer.android.com/studio/projects/gradle-external-native-builds#jniLibs
    // https://developer.android.com/ndk/guides/abis#sa

    into("src/main/jniLibs/x86_64") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-x64-jni/")) {
            include("*.so")
        }
    }
    into("src/main/jniLibs/x86") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-x86-jni/")) {
            include("*.so")
        }
    }
    into("src/main/jniLibs/arm64-v8a") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-arm64-jni/")) {
            include("*.so")
        }
    }
    into("src/main/jniLibs/armeabi-v7a") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-arm-jni/")) {
            include("*.so")
        }
    }

    destinationDir = projectDir
}
tasks.preBuild.dependsOn("copyJniForAndroid")

android {
    namespace = "net.alphatab.alphaskia.android"
    compileSdk = 34

    defaultConfig {
        minSdk = 26
        ndk {
            abiFilters += listOf("x86", "x86_64", "armeabi-v7a", "arm64-v8a")
        }
        setProperty("archivesBaseName", "alphaSkia-android")
    }

    buildTypes {
        release {
            isMinifyEnabled = false
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
}

mavenPublishing {
    coordinates(rootProject.group.toString(), "alphaSkia-android", rootProject.version.toString())
    configure(
        AndroidSingleVariantLibrary(
            variant = "release",
            sourcesJar = true,
            publishJavadocJar = true
        )
    )
}
