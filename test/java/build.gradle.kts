plugins {
    id("java")
    id("application")
    id("idea")
}

group = "alphaTab.alphaSkia"
version = "1.0.0-LOCAL"
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
    implementation("net.alphatab:alphaSkia-android:$alphaSkiaVersion")
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