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
