import com.android.build.gradle.internal.tasks.factory.dependsOn
import org.jetbrains.kotlin.gradle.plugin.mpp.pm20.util.libsDirectory

plugins {
    alias(libs.plugins.kotlinMultiplatform)
//    alias(libs.plugins.kotlinCocoapods)
    alias(libs.plugins.androidLibrary)
    `maven-publish`
    signing
}

tasks.register<Copy>("copyJniForAndroid") {
    // https://developer.android.com/studio/projects/gradle-external-native-builds#jniLibs
    // https://developer.android.com/ndk/guides/abis#sa

    into("x86_64") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-x64-jni/")) {
            include("*.so")
        }
    }
    into("x86") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-x86-jni/")) {
            include("*.so")
        }
    }
    into("arm64-v8a") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-arm64-jni/")) {
            include("*.so")
        }
    }
    into("armeabi-v7a") {
        from(rootProject.projectDir.resolve("../../dist/libalphaskiajni-android-arm-jni/")) {
            include("*.so")
        }
    }

    destinationDir = projectDir.resolve("src/androidMain/jniLibs")
}
afterEvaluate {
    tasks.getByName("mergeReleaseJniLibFolders").dependsOn("copyJniForAndroid")
}

afterEvaluate {
    publishing {
        publications.withType<MavenPublication> {
            if(this.name != "kotlinMultiplatform" && this.name != "jvm" && !this.name.contains("android")) {
                this.artifactId = "alphaSkia-native-${this.name}"
                pom.name = this.artifactId
            }
        }
    }
}

kotlin {
    androidTarget {
        compilations.all {
            kotlinOptions {
                jvmTarget = "17"
            }
        }
        publishLibraryVariants("release")
    }
    jvm {
        compilations.all {
            kotlinOptions {
                jvmTarget = "17"
            }
        }
    }

    val hostOs = System.getProperty("os.name")
    val isMingwX64 = hostOs.startsWith("Windows")
    val nativeTargets = when {
        hostOs == "Mac OS X" -> arrayOf(macosX64(), macosArm64(), iosSimulatorArm64(), iosX64(), iosArm64())
        hostOs == "Linux" -> arrayOf(linuxX64())
        isMingwX64 -> arrayOf(mingwX64())
        else -> throw GradleException("Host OS is not supported in Kotlin/Native.")
    }

    data class NativeLibInfo(val dir:String, val lib:String, val file:String)

    val nativeLibLookup = hashMapOf(
        "macosX64" to NativeLibInfo("libalphaskia-macos-x64-shared", "libalphaskia", "libalphaskia.dylib"),
        "macosArm64" to NativeLibInfo("libalphaskia-macos-arm64-shared", "libalphaskia", "libalphaskia.dylib"),

        "iosSimulatorArm64" to NativeLibInfo("libalphaskia-iossimulator-arm64-shared", "libalphaskia", "libalphaskia.dylib"),
        "iosX64" to NativeLibInfo("libalphaskia-iossimulator-x64-shared", "libalphaskia", "libalphaskia.dylib"),

        "iosArm64" to NativeLibInfo("libalphaskia-ios-arm64-shared", "libalphaskia", "libalphaskia.dylib"),

        "linuxX64" to NativeLibInfo("libalphaskia-win-x64-shared", "libalphaskia", "libalphaskia.so"),
        "mingwX64" to NativeLibInfo("libalphaskia-win-x64-shared", "libalphaskia.dll", "libalphaskia.dll libalphaskia.dll.lib")
    )

    for(nativeTarget in nativeTargets) {
        nativeTarget.apply {
            val values = nativeLibLookup[name] ?: throw IllegalStateException("Unsupported native platform $name")
            val libDir = rootProject.projectDir.resolve("../../dist/${values.dir}")
            binaries {
                sharedLib {
                    baseName = "alphaskia"
                    linkerOpts += arrayOf("-L${libDir.canonicalPath.replace('\\', '/')}", "-l${values.lib}")
                }
            }
            compilations["main"].cinterops {
                val alphaSkia by creating {
                    includeDirs(rootProject.projectDir.resolve("../../wrapper/include"))
                    extraOpts("-libraryPath", libDir)
                    for(included in values.file.split(" ")) {
                        extraOpts("-staticLibrary", included)
                    }
                }
            }
        }
    }

//    iosX64()
//    iosArm64()
//    iosSimulatorArm64()
//
//    cocoapods {
//        summary = "Some description for the Shared Module"
//        homepage = "Link to the Shared Module homepage"
//        version = "1.0"
//        ios.deploymentTarget = "16.0"
//        framework {
//            baseName = "native"
//            isStatic = true
//        }
//    }

    applyDefaultHierarchyTemplate()

    sourceSets {
        all {
            languageSettings.optIn("kotlin.experimental.ExperimentalNativeApi")
            languageSettings.optIn("kotlinx.cinterop.ExperimentalForeignApi")
            languageSettings.optIn("kotlin.ExperimentalStdlibApi")
        }

        commonMain.dependencies {
            //put your multiplatform dependencies here
        }
        commonTest.dependencies {
            implementation(libs.kotlin.test)
        }

        val commonNative by creating {
            dependsOn(commonMain.get())

            kotlin.srcDir("src/commonNative/kotlin")
        }

        mingwMain.get().dependsOn(commonNative)
        macosMain.get().dependsOn(commonNative)
        iosMain.get().dependsOn(commonNative)
        linuxMain.get().dependsOn(commonNative)
    }
}

android {
    namespace = "net.alphatab.alphaskia.android"
    compileSdk = 34

    defaultConfig {
        minSdk = 26
        ndk {
            abiFilters += listOf("x86", "x86_64", "armeabi-v7a", "arm64-v8a")
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = false
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }


}