plugins {
    id("java-library")
}

java {
    toolchain{
        languageVersion.set(JavaLanguageVersion.of(17))
    }
}

tasks.jar {
    archiveBaseName = "net.alphatab.alphaskia.android"

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