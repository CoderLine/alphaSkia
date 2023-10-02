buildscript {
    repositories {
        gradlePluginPortal()
        google()
        mavenCentral()
    }
}

subprojects {
    group = "net.alphatab"
    version = "1.0-SNAPSHOT"

    repositories {
        google()
        mavenCentral()
    }

    tasks.withType<Test>().configureEach {
        systemProperty("alphaskia.library.path", rootProject.projectDir.resolve("../../dist/"))
        systemProperty("testdata.path", rootProject.projectDir.resolve("../test/"))
        systemProperty("testoutput.path", projectDir.resolve("../test-outputs/"))
    }
}