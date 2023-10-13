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
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-linux-x64-jni/")) {
            include("*.so")
        }
    }
    into("native/linux-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-linux-x86-jni/")) {
            include("*.so")
        }
    }
    into("native/linux-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-linux-arm64-jni/")) {
            include("*.so")
        }
    }
}