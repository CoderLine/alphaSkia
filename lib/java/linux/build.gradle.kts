plugins {
    id("java-library")
}

java {
    toolchain{
        languageVersion.set(JavaLanguageVersion.of(17))
    }
}

tasks.jar {
    archiveBaseName = "net.alphatab.alphaskia.windows"

    into("native/linux-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-linux-x64-shared/"))
    }
    into("native/linux-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-linux-x86-shared/"))
    }
    into("native/linux-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-linux-arm64-shared/"))
    }
}