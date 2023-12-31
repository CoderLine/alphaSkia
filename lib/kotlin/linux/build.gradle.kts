plugins {
    id("java-library")
    alias(libs.plugins.kotlinJvm)
    `maven-publish`
    signing
}

dependencies {
    implementation(project(":alphaSkia"))
}

java {
    sourceCompatibility = JavaVersion.VERSION_17
    targetCompatibility = JavaVersion.VERSION_17
}

publishing {
    publications {
        create<MavenPublication>("mavenKotlin") {
            from(components.findByName("java"))
        }
    }
}

tasks.jar {
    archiveBaseName = "alphaSkia-linux"

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
    into("native/linux-arm/") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-linux-arm-jni/")) {
            include("*.so")
        }
    }
}