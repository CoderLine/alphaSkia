plugins {
    id("java-library")
}

tasks.jar {
    archiveBaseName = "net.alphatab.alphaskia.windows"

    into("native/win-x64/") {
        include(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-win-x64-shared/").absolutePath)
    }
    into("native/win-x86/") {
        include(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-win-x86-shared/").absolutePath)
    }
    into("native/win-arm64/") {
        include(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-win-arm64-shared/").absolutePath)
    }
}