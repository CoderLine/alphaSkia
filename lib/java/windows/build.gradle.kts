plugins {
    id("java-library")
    `maven-publish`
    signing
}

java {
    toolchain {
        languageVersion.set(JavaLanguageVersion.of(17))
    }
    withSourcesJar()
    withJavadocJar()
}


tasks.jar {
    archiveBaseName = "alphaSkia-windows"

    into("native/win-x64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-x64-jni/").absolutePath) {
            include("*.dll")
        }
    }
    into("native/win-x86/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-x86-jni/").absolutePath) {
            include("*.dll")
        }
    }
    into("native/win-arm64/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-win-arm64-jni/").absolutePath) {
            include("*.dll")
        }
    }
}