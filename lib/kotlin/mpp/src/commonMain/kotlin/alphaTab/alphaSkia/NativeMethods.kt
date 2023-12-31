@file:Suppress("FunctionName", "LocalVariableName")

package alphaTab.alphaSkia

import kotlin.jvm.JvmStatic


typealias alphaskia_data_t = Long
typealias alphaskia_typeface_t = Long
typealias alphaskia_image_t = Long
typealias alphaskia_canvas_t = Long
typealias alphaskia_text_align_t = Int
typealias alphaskia_text_baseline_t = Int

expect class NativeMethods {
    companion object {
        @JvmStatic
        val requiresNativeLibLoading: Boolean

        @JvmStatic
        fun alphaskiaGetColorType(): Int

        @JvmStatic
        fun alphaskiaDataNewCopy(data: ByteArray): alphaskia_data_t

        @JvmStatic
        fun alphaskiaDataGetData(data: alphaskia_data_t): ByteArray

        @JvmStatic
        fun alphaskiaDataFree(data: alphaskia_data_t)

        @JvmStatic
        fun alphaskiaTypefaceRegister(data: alphaskia_data_t): alphaskia_typeface_t

        @JvmStatic
        fun alphaskiaTypefaceFree(typeface: alphaskia_typeface_t)

        @JvmStatic
        fun alphaskiaTypefaceMakeFromName(
            family_name: String,
            bold: Boolean,
            italic: Boolean
        ): alphaskia_typeface_t

        @JvmStatic
        fun alphaskiaTypefaceGetFamilyName(typeface: alphaskia_typeface_t): String

        @JvmStatic
        fun alphaskiaTypefaceIsBold(typeface: alphaskia_typeface_t): Boolean

        @JvmStatic
        fun alphaskiaTypefaceIsItalic(typeface: alphaskia_typeface_t): Boolean

        @JvmStatic
        fun alphaskiaImageGetWidth(image: alphaskia_image_t): Int

        @JvmStatic
        fun alphaskiaImageGetHeight(image: alphaskia_image_t): Int

        @JvmStatic
        fun alphaskiaImageReadPixels(image: alphaskia_image_t): ByteArray?

        @JvmStatic
        fun alphaskiaImageEncodePng(image: alphaskia_image_t): ByteArray?

        @JvmStatic
        fun alphaskiaImageDecode(data: ByteArray): alphaskia_image_t

        @JvmStatic
        fun alphaskiaImageFromPixels(
            width: Int,
            height: Int,
            pixels: ByteArray
        ): alphaskia_image_t

        @JvmStatic
        fun alphaskiaImageFree(image: alphaskia_image_t)

        @JvmStatic
        fun alphaskiaCanvasNew(): alphaskia_canvas_t

        @JvmStatic
        fun alphaskiaCanvasFree(canvas: alphaskia_canvas_t)

        @JvmStatic
        fun alphaskiaCanvasSetColor(canvas: alphaskia_canvas_t, color: Int)

        @JvmStatic
        fun alphaskiaCanvasGetColor(canvas: alphaskia_canvas_t): Int

        @JvmStatic
        fun alphaskiaCanvasSetLineWidth(canvas: alphaskia_canvas_t, line_width: Float)

        @JvmStatic
        fun alphaskiaCanvasGetLineWidth(canvas: alphaskia_canvas_t): Float

        @JvmStatic
        fun alphaskiaCanvasBeginRender(
            canvas: alphaskia_canvas_t,
            width: Int,
            height: Int,
            render_scale: Float
        )

        @JvmStatic
        fun alphaskiaCanvasEndRender(canvas: alphaskia_canvas_t): alphaskia_image_t

        @JvmStatic
        fun alphaskiaCanvasFillRect(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            width: Float,
            height: Float
        )

        @JvmStatic
        fun alphaskiaCanvasStrokeRect(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            width: Float,
            height: Float
        )

        @JvmStatic
        fun alphaskiaCanvasBeginPath(canvas: alphaskia_canvas_t)

        @JvmStatic
        fun alphaskiaCanvasClosePath(canvas: alphaskia_canvas_t)

        @JvmStatic
        fun alphaskiaCanvasMoveTo(canvas: alphaskia_canvas_t, x: Float, y: Float)

        @JvmStatic
        fun alphaskiaCanvasLineTo(canvas: alphaskia_canvas_t, x: Float, y: Float)

        @JvmStatic
        fun alphaskiaCanvasQuadraticCurveTo(
            canvas: alphaskia_canvas_t,
            cpx: Float,
            cpy: Float,
            x: Float,
            y: Float
        )

        @JvmStatic
        fun alphaskiaCanvasBezierCurveTo(
            canvas: alphaskia_canvas_t,
            cp1x: Float,
            cp1y: Float,
            cp2x: Float,
            cp2y: Float,
            x: Float,
            y: Float
        )

        @JvmStatic
        fun alphaskiaCanvasFillCircle(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            radius: Float
        )

        @JvmStatic
        fun alphaskiaCanvasStrokeCircle(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            radius: Float
        )

        @JvmStatic
        fun alphaskiaCanvasFill(canvas: alphaskia_canvas_t)

        @JvmStatic
        fun alphaskiaCanvasStroke(canvas: alphaskia_canvas_t)

        @JvmStatic
        fun alphaskiaCanvasDrawImage(
            canvas: alphaskia_canvas_t,
            image: alphaskia_image_t,
            x: Float,
            y: Float,

            w: Float,
            h: Float
        )

        @JvmStatic
        fun alphaskiaCanvasFillText(
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
        fun alphaskiaCanvasMeasureText(
            canvas: alphaskia_canvas_t,
            text: String,
            typeface: alphaskia_typeface_t, font_size: Float
        ): Float

        @JvmStatic
        fun alphaskiaCanvasBeginRotate(
            canvas: alphaskia_canvas_t,
            center_x: Float,
            center_y: Float,
            angle: Float
        )

        @JvmStatic
        fun alphaskiaCanvasEndRotate(canvas: alphaskia_canvas_t)
    }
}