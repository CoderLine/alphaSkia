@file:Suppress("FunctionName", "LocalVariableName")

package alphaTab.alphaSkia

actual class NativeMethods {
    actual companion object {
        @JvmStatic
        actual val requiresNativeLibLoading: Boolean = true

        @JvmStatic
        actual external fun alphaskiaGetColorType(): Int

        @JvmStatic
        actual external fun alphaskiaDataNewCopy(data: ByteArray): alphaskia_data_t

        @JvmStatic
        actual external fun alphaskiaDataGetData(data: alphaskia_data_t): ByteArray

        @JvmStatic
        actual external fun alphaskiaDataFree(data: alphaskia_data_t)

        @JvmStatic
        actual external fun alphaskiaTypefaceRegister(data: alphaskia_data_t): alphaskia_typeface_t

        @JvmStatic
        actual external fun alphaskiaTypefaceFree(typeface: alphaskia_typeface_t)

        @JvmStatic
        actual external fun alphaskiaTypefaceMakeFromName(
            family_name: String,
            bold: Boolean,
            italic: Boolean
        ): alphaskia_typeface_t

        @JvmStatic
        actual external fun alphaskiaTypefaceGetFamilyName(typeface: alphaskia_typeface_t): String

        @JvmStatic
        actual external fun alphaskiaTypefaceIsBold(typeface: alphaskia_typeface_t): Boolean

        @JvmStatic
        actual external fun alphaskiaTypefaceIsItalic(typeface: alphaskia_typeface_t): Boolean

        @JvmStatic
        actual external fun alphaskiaImageGetWidth(image:  alphaskia_image_t): Int

        @JvmStatic
        actual external fun alphaskiaImageGetHeight(image: alphaskia_image_t): Int

        @JvmStatic
        actual external fun alphaskiaImageReadPixels(image: alphaskia_image_t): ByteArray?

        @JvmStatic
        actual external fun alphaskiaImageEncodePng(image: alphaskia_image_t): ByteArray?

        @JvmStatic
        actual external fun alphaskiaImageDecode(data: ByteArray): alphaskia_image_t

        @JvmStatic
        actual external fun alphaskiaImageFromPixels(
            width: Int,
            height: Int,
            pixels: ByteArray
        ): alphaskia_image_t

        @JvmStatic
        actual external fun alphaskiaImageFree(image: alphaskia_image_t)

        @JvmStatic
        actual external fun alphaskiaCanvasNew(): alphaskia_canvas_t

        @JvmStatic
        actual external fun alphaskiaCanvasFree(canvas: alphaskia_canvas_t)

        @JvmStatic
        actual external fun alphaskiaCanvasSetColor(canvas: alphaskia_canvas_t, color: Int)

        @JvmStatic
        actual external fun alphaskiaCanvasGetColor(canvas: alphaskia_canvas_t): Int

        @JvmStatic
        actual external fun alphaskiaCanvasSetLineWidth(canvas: alphaskia_canvas_t, line_width: Float)

        @JvmStatic
        actual external fun alphaskiaCanvasGetLineWidth(canvas: alphaskia_canvas_t): Float

        @JvmStatic
        actual external fun alphaskiaCanvasBeginRender(
            canvas: alphaskia_canvas_t,
            width: Int,
            height: Int,
            render_scale: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasEndRender(canvas: alphaskia_canvas_t): alphaskia_image_t

        @JvmStatic
        actual external fun alphaskiaCanvasFillRect(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            width: Float,
            height: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasStrokeRect(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            width: Float,
            height: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasBeginPath(canvas: alphaskia_canvas_t)

        @JvmStatic
        actual external fun alphaskiaCanvasClosePath(canvas: alphaskia_canvas_t)

        @JvmStatic
        actual external fun alphaskiaCanvasMoveTo(canvas: alphaskia_canvas_t, x: Float, y: Float)

        @JvmStatic
        actual external fun alphaskiaCanvasLineTo(canvas: alphaskia_canvas_t, x: Float, y: Float)

        @JvmStatic
        actual external fun alphaskiaCanvasQuadraticCurveTo(
            canvas: alphaskia_canvas_t,
            cpx: Float,
            cpy: Float,
            x: Float,
            y: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasBezierCurveTo(
            canvas: alphaskia_canvas_t,
            cp1x: Float,
            cp1y: Float,
            cp2x: Float,
            cp2y: Float,
            x: Float,
            y: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasFillCircle(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            radius: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasStrokeCircle(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            radius: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasFill(canvas: alphaskia_canvas_t)

        @JvmStatic
        actual external fun alphaskiaCanvasStroke(canvas: alphaskia_canvas_t)

        @JvmStatic
        actual external fun alphaskiaCanvasDrawImage(
            canvas: alphaskia_canvas_t,
            image: alphaskia_image_t,
            x: Float,
            y: Float,

            w: Float,
            h: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasFillText(
            canvas: alphaskia_canvas_t,
            text: String,
            typeface: alphaskia_typeface_t,
            font_size: Float,
            x: Float,
            y: Float,
            text_align: alphaskia_text_align_t,
            baseline: alphaskia_text_baseline_t
        )

        @JvmStatic
        actual external fun alphaskiaCanvasMeasureText(
            canvas: alphaskia_canvas_t,
            text: String,
            typeface: alphaskia_typeface_t, font_size: Float
        ): Float

        @JvmStatic
        actual external fun alphaskiaCanvasBeginRotate(
            canvas: alphaskia_canvas_t,
            center_x: Float,
            center_y: Float,
            angle: Float
        )

        @JvmStatic
        actual external fun alphaskiaCanvasEndRotate(canvas: alphaskia_canvas_t)
    }
}