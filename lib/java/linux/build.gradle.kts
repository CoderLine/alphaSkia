plugins {
    id("java-library")
}

java {
    toolchain{
        languageVersion.set(JavaLanguageVersion.of(17))
    }
}

tasks.jar {
    archiveBaseName = "net.alphatab.alphaskia.linux"

    into("native/linux-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-linux-x64-shared/")) {
            include("*.so")
        }
    }
    into("native/linux-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-linux-x86-shared/")) {
            include("*.so")
        }
    }
    into("native/linux-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-linux-arm64-shared/")) {
            include("*.so")
        }
    }
}