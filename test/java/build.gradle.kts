plugins {
    id("java")
    id("application")
    id("idea")
}

group = "net.alphatab.alphaskia"
version = "1.0.0-LOCAL"
// Use any latest version
var alphaSkiaVersion = version

// Override with value from CI
val versionEnv = providers.environmentVariable("ALPHASKIA_VERSION")
if (versionEnv.isPresent) {
    version = versionEnv.get()
    alphaSkiaVersion = versionEnv.get()
}

repositories {
    mavenCentral()
    maven {
        url = projectDir.resolve("../../dist/Maven/").toURI()

    }
}

dependencies {
    testImplementation(platform("org.junit:junit-bom:5.9.1"))
    testImplementation("org.junit.jupiter:junit-jupiter")

    implementation("net.alphatab:net.alphatab.alphaskia:$alphaSkiaVersion")
    implementation("net.alphatab:net.alphatab.alphaskia.macos:$alphaSkiaVersion")
    implementation("net.alphatab:net.alphatab.alphaskia.windows:$alphaSkiaVersion")
    implementation("net.alphatab:net.alphatab.alphaskia.linux:$alphaSkiaVersion")
    implementation("net.alphatab:net.alphatab.alphaskia.android:$alphaSkiaVersion")
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
    mainClass = "net.alphatab.alphaskia.test.Main"
}