/* DO NOT EDIT THIS FILE - it is machine generated */
#include <jni.h>
/* Header for class alphaTab_alphaSkia_AlphaSkiaCanvas */

#ifndef _Included_alphaTab_alphaSkia_AlphaSkiaCanvas
#define _Included_alphaTab_alphaSkia_AlphaSkiaCanvas
#ifdef __cplusplus
extern "C" {
#endif
/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    getColor
 * Signature: ()I
 */
JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_getColor
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    setColor
 * Signature: (I)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_setColor
  (JNIEnv *, jobject, jint);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    getLineWidth
 * Signature: ()F
 */
JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_getLineWidth
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    setLineWidth
 * Signature: (F)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_setLineWidth
  (JNIEnv *, jobject, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    close
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_close
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    beginRender
 * Signature: (IIF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginRender
  (JNIEnv *, jobject, jint, jint, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    drawImage
 * Signature: (LalphaTab/alphaSkia/AlphaSkiaImage;FFFF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_drawImage
  (JNIEnv *, jobject, jobject, jfloat, jfloat, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    endRender
 * Signature: ()LalphaTab/alphaSkia/AlphaSkiaImage;
 */
JNIEXPORT jobject JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_endRender
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    fillRect
 * Signature: (FFFF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fillRect
  (JNIEnv *, jobject, jfloat, jfloat, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    strokeRect
 * Signature: (FFFF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_strokeRect
  (JNIEnv *, jobject, jfloat, jfloat, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    beginPath
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginPath
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    closePath
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_closePath
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    moveTo
 * Signature: (FF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_moveTo
  (JNIEnv *, jobject, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    lineTo
 * Signature: (FF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_lineTo
  (JNIEnv *, jobject, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    quadraticCurveTo
 * Signature: (FFFF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_quadraticCurveTo
  (JNIEnv *, jobject, jfloat, jfloat, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    bezierCurveTo
 * Signature: (FFFFFF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_bezierCurveTo
  (JNIEnv *, jobject, jfloat, jfloat, jfloat, jfloat, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    fillCircle
 * Signature: (FFF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fillCircle
  (JNIEnv *, jobject, jfloat, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    strokeCircle
 * Signature: (FFF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_strokeCircle
  (JNIEnv *, jobject, jfloat, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    fill
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fill
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    stroke
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_stroke
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    fillText
 * Signature: (Ljava/lang/String;LalphaTab/alphaSkia/AlphaSkiaTextStyle;FFFLalphaTab/alphaSkia/AlphaSkiaTextAlign;LalphaTab/alphaSkia/AlphaSkiaTextBaseline;)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fillText
  (JNIEnv *, jobject, jstring, jobject, jfloat, jfloat, jfloat, jobject, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    measureText
 * Signature: (Ljava/lang/String;LalphaTab/alphaSkia/AlphaSkiaTextStyle;FLalphaTab/alphaSkia/AlphaSkiaTextAlign;LalphaTab/alphaSkia/AlphaSkiaTextBaseline;)LalphaTab/alphaSkia/AlphaSkiaTextMetrics;
 */
JNIEXPORT jobject JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_measureText
  (JNIEnv *, jobject, jstring, jobject, jfloat, jobject, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    beginRotate
 * Signature: (FFF)V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginRotate
  (JNIEnv *, jobject, jfloat, jfloat, jfloat);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    endRotate
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_endRotate
  (JNIEnv *, jobject);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    alphaskiaCanvasAllocate
 * Signature: ()J
 */
JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_alphaskiaCanvasAllocate
  (JNIEnv *, jclass);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    alphaskiaColorType
 * Signature: ()I
 */
JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_alphaskiaColorType
  (JNIEnv *, jclass);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    switchToFreeTypeFonts
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_switchToFreeTypeFonts
  (JNIEnv *, jclass);

/*
 * Class:     alphaTab_alphaSkia_AlphaSkiaCanvas
 * Method:    switchToOperatingSystemFonts
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_switchToOperatingSystemFonts
  (JNIEnv *, jclass);

#ifdef __cplusplus
}
#endif
#endif
