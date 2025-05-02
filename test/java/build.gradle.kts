plugins {
    id("java")
    id("application")
    id("idea")
}

// do not inline (updated dynamically via Nuke)
var libVersion = "3.3.135"

group = "alphaTab.alphaSkia"
version = libVersion
// Use any latest version
var alphaSkiaVersion = version

// Override with value from CI
val versionEnv = providers.environmentVariable("ALPHASKIA_TEST_VERSION")
if (versionEnv.isPresent) {
    version = versionEnv.get()
    alphaSkiaVersion = versionEnv.get()
}

repositories {
    mavenCentral()
    maven {
        url = projectDir.resolve("../../dist/maven/").toURI()

    }
}

dependencies {
    testImplementation(platform("org.junit:junit-bom:5.9.1"))
    testImplementation("org.junit.jupiter:junit-jupiter")

    implementation("net.alphatab:alphaSkia:$alphaSkiaVersion")
    implementation("net.alphatab:alphaSkia-macos:$alphaSkiaVersion")
    implementation("net.alphatab:alphaSkia-windows:$alphaSkiaVersion")
    implementation("net.alphatab:alphaSkia-linux:$alphaSkiaVersion")
}

java {
    toolchain {
        languageVersion = JavaLanguageVersion.of(17)
    }
}

val generatedSourcesPath = projectDir.resolve("src/main/generated")
java.sourceSets["main"].java.srcDir(generatedSourcesPath)
idea {
    module {
        generatedSourceDirs.add(generatedSourcesPath)
    }
}

tasks.test {
    useJUnitPlatform()
}

application {
    mainClass = "alphaTab.alphaSkia.test.Main"
}
