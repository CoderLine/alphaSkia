import com.vanniktech.maven.publish.JavaLibrary
import com.vanniktech.maven.publish.JavadocJar

plugins {
    `java-library`
    alias(libs.plugins.mavenPublish)
    `maven-publish`
    signing
}

java {
    toolchain {
        languageVersion = JavaLanguageVersion.of(17)
    }
}

tasks.withType<JavaCompile>().configureEach {
    options.headerOutputDirectory = rootProject.projectDir.resolve("jni/include")
}

dependencies {
    testImplementation(platform(libs.junit.bom))
    testImplementation(libs.junit.jupiter)
}

tasks.test {
    useJUnitPlatform()
}

tasks.jar {
    archiveBaseName = "alphaSkia"
}

mavenPublishing {
    coordinates(rootProject.group.toString(), "alphaSkia", rootProject.version.toString())
    configure(JavaLibrary(JavadocJar.Javadoc(), true))
}