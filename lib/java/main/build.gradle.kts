plugins {
    id("java-library")
}

java {
    toolchain{
        languageVersion.set(JavaLanguageVersion.of(17))
    }
}

tasks.withType<JavaCompile>().configureEach {
    options.headerOutputDirectory.set(rootProject.projectDir.resolve("jni/include"))
}

dependencies {
    testImplementation(platform("org.junit:junit-bom:5.9.1"))
    testImplementation("org.junit.jupiter:junit-jupiter")
}

tasks.test {
    useJUnitPlatform()
}

tasks.jar {
    archiveBaseName = "net.alphatab.alphaskia"
}