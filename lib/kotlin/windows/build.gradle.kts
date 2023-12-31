plugins {
    id("java-library")
    alias(libs.plugins.kotlinJvm)
    `maven-publish`
    signing
}

dependencies {
    implementation(project(":alphaSkia"))
}

publishing {
    publications {
        create<MavenPublication>("mavenKotlin") {
            from(components.findByName("java"))
        }
    }
}

java {
    sourceCompatibility = JavaVersion.VERSION_17
    targetCompatibility = JavaVersion.VERSION_17
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