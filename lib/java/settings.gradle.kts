pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}
rootProject.name = "AlphaSkia"

include("main")
include("linux")
include("windows")
include("android")
include("macos")
