buildscript {
    repositories {
        gradlePluginPortal()
        google()
        mavenCentral()
    }
}

plugins {
    `java-library`
}

subprojects {
    apply(plugin = "java-library")

    group = "net.alphatab"
    version = "1.0-SNAPSHOT"

    repositories {
        google()
        mavenCentral()
    }

    tasks.withType<JavaCompile>().configureEach {
        javaCompiler.set(javaToolchains.compilerFor {
            languageVersion.set(JavaLanguageVersion.of(17))
        })

        options.release.set(17)
    }


    tasks.withType<Test>().configureEach {
        val os = System.getProperty("os.name")
        val target = when {
            os == "Mac OS X" -> {
                "osx"
            }

            os.startsWith("Win") -> {
                "win"
            }

            os.startsWith("Linux") -> {
                "linux"
            }

            else -> {
                throw Error("Unsupported OS: $os")
            }
        }

        val arch = when (val jarch = System.getProperty("os.arch")) {
            "x86", "i368", "i486", "i586", "i686" -> "x86"
            "x86_64", "amd64" -> "x64"
            "arm" -> "arm"
            "aarch64" -> "arm64"
            else -> jarch
        }

        systemProperty("java.library.path", rootProject.projectDir.resolve("../../dist/libAlphaSkiaJni-$target-$arch-shared/"))
        systemProperty("testdata.path", rootProject.projectDir.resolve("../test/"))
    }
}