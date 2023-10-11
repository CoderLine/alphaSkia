plugins {
    id("java-library")
}

java {
    toolchain {
        languageVersion.set(JavaLanguageVersion.of(17))
    }
}

tasks.jar {
    archiveBaseName = "net.alphatab.alphaskia.windows"

    into("native/win-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-x64-shared/").absolutePath) {
            include("*.dll")
        }
    }
    into("native/win-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-x86-shared/").absolutePath) {
            include("*.dll")
        }
    }
    into("native/win-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-arm64-shared/").absolutePath) {
            include("*.dll")
        }
    }
}