pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}
rootProject.name = "alphaSkia"

include(":mpp")
include(":linux")
include(":windows")
include(":macos")


project(":mpp").name = "alphaSkia"
project(":linux").name = "alphaSkia-jvm-linux"
project(":windows").name = "alphaSkia-jvm-windows"
project(":macos").name = "alphaSkia-jvm-macos"