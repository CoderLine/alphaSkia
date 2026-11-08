# Classes and members the alphaSkia JNI library looks up by name must survive shrinking and keep their names.
-keepclasseswithmembernames,includedescriptorclasses class alphaTab.alphaSkia.** {
    native <methods>;
}
-keep class alphaTab.alphaSkia.AlphaSkiaNative {
    long handle;
}
-keep class alphaTab.alphaSkia.AlphaSkiaImage {
    <init>(long);
}
-keep class alphaTab.alphaSkia.AlphaSkiaTextMetrics {
    <init>(long);
}
