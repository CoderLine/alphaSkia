# AlphaSkia

AlphaSkia is a special cross platform [Skia](https://skia.org/) wrapper used in [AlphaTab](https://github.com/CoderLine/alphaTab) it aims to provide a HTML5 Canvas like API to alphaTab providing a consistent rendering experience across the alphaTab flavours like web, .net and Kotlin. Differences across operating systems might still apply.

A big **Thank you** to the following projects giving me good insights on how to build Skia in a cross platform fashion. These projects influenced heavily
how the pipelines of AlphaSkia are organized. 

* [SkiaSharp](https://github.com/mono/SkiaSharp/)
* [Skija](https://github.com/HumbleUI/Skija/)

## Supported platforms

We currently support following targets and platforms: 

* .net 
  * win-x86
  * win-x64
  * win-arm64
  * linux-x64
* Node.js 
  * win-x64
  * linux-x64

## Planned platforms

* .net
  * linux-arm
  * linux-arm64
  * osx
* Android (Kotlin)
  * android-arm
  * android-arm64
  * android-x64
  * android-x86
* iOS (Kotlin)
  * ios
  * iossimulator
* Node.js 
  * win-x86
  * win-arm64
  * linux-arm
  * linux-arm64
  * osx