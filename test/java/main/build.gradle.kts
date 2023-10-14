plugins {
    id("java-library")
    `maven-publish`
    signing
}

java {
    toolchain {
        languageVersion = JavaLanguageVersion.of(17)
    }
    withSourcesJar()
    withJavadocJar()
}

tasks.withType<JavaCompile>().configureEach {
    options.headerOutputDirectory = rootProject.projectDir.resolve("jni/include")
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