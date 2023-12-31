/* DO NOT EDIT THIS FILE - it is machine generated */
#include <jni.h>
/* Header for class alphaTab_alphaSkia_AlphaSkiaTypeface */

#ifndef _Included_alphaTab_alphaSkia_AlphaSkiaTypeface
#define _Included_alphaTab_alphaSkia_AlphaSkiaTypeface
#ifdef __cplusplus
extern "C" {
#endif
/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaTypeface
 * Method:    isBold
 * Signature: ()Z
 */
JNIEXPORT jboolean JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_isBold
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaTypeface
 * Method:    isItalic
 * Signature: ()Z
 */
JNIEXPORT jboolean JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_isItalic
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaTypeface
 * Method:    loadFamilyName
 * Signature: (J)Ljava/lang/String;
 */
JNIEXPORT jstring JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_loadFamilyName
  (JNIEnv *, jclass, jlong);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaTypeface
 * Method:    release
 * Signature: (J)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_release
  (JNIEnv *, jobject, jlong);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaTypeface
 * Method:    register
 * Signature: (J)J
 */
JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_register
  (JNIEnv *, jclass, jlong);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaTypeface
 * Method:    makeFromName
 * Signature: (Ljava/lang/String;ZZ)J
 */
JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_makeFromName
  (JNIEnv *, jclass, jstring, jboolean, jboolean);

#ifdef __cplusplus
}
#endif
#endif
