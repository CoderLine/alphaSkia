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
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-android-x64-shared/")) {
            include("*.so")
        }
    }
    into("native/android-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-android-x86-shared/")) {
            include("*.so")
        }
    }
    into("native/android-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-android-arm64-shared/")) {
            include("*.so")
        }
    }
    into("native/android-arm/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-android-arm-shared/")) {
            include("*.so")
        }
    }
}