plugins {
    id("java-library")
}

java {
    toolchain{
        languageVersion.set(JavaLanguageVersion.of(17))
    }
}

tasks.jar {
    archiveBaseName = "net.alphatab.alphaskia.macos"

    into("native/macos-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-macos-x64-shared/")) {
            include("*.dylib")
        }
    }
    into("native/macos-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-macos-arm64-shared/")) {
            include("*.dylib")
        }
    }
}