@file:Suppress("FunctionName", "LocalVariableName")

package alphaTab.alphaSkia

import kotlinx.cinterop.CPointed
import kotlinx.cinterop.readBytes
import kotlinx.cinterop.refTo
import kotlinx.cinterop.toCPointer
import kotlinx.cinterop.toKStringFromUtf8
import kotlinx.cinterop.toLong
import kotlinx.cinterop.wcstr

actual class NativeMethods {
    actual companion object {
        actual val requiresNativeLibLoading: Boolean = false

        actual fun alphaskiaGetColorType(): Int {
            return alphaTab.alphaSkia.cinterop.alphaskia_get_color_type()
        }


        actual fun alphaskiaDataNewCopy(data: ByteArray): alphaskia_data_t {
            return alphaTab.alphaSkia.cinterop.alphaskia_data_new_copy(
                data.asUByteArray().refTo(0),
                data.size.toULong()
            ).toLong()
        }


        actual fun alphaskiaDataGetData(data: alphaskia_data_t): ByteArray {
            val nativeData = data.toCPointer<CPointed>();
            val ptr = alphaTab.alphaSkia.cinterop.alphaskia_data_get_data(nativeData)
            val length = alphaTab.alphaSkia.cinterop.alphaskia_data_get_length(nativeData)
            return ptr!!.readBytes(length.toInt())
        }


        actual fun alphaskiaDataFree(data: alphaskia_data_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_data_free(data.toCPointer())
        }


        actual fun alphaskiaTypefaceRegister(data: alphaskia_data_t): alphaskia_typeface_t {
            return alphaTab.alphaSkia.cinterop.alphaskia_typeface_register(data.toCPointer())
                .toLong()
        }


        actual fun alphaskiaTypefaceFree(typeface: alphaskia_typeface_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_typeface_free(typeface.toCPointer())
        }


        actual fun alphaskiaTypefaceMakeFromName(
            family_name: String,
            bold: Boolean,
            italic: Boolean
        ): alphaskia_typeface_t {
            return alphaTab.alphaSkia.cinterop.alphaskia_typeface_make_from_name(
                family_name,
                if (bold) 1.toUByte() else 0.toUByte(),
                if (italic) 1.toUByte() else 0.toUByte()
            ).toLong()
        }


        actual fun alphaskiaTypefaceGetFamilyName(typeface: alphaskia_typeface_t): String {
            val str =
                alphaTab.alphaSkia.cinterop.alphaskia_typeface_get_family_name(typeface.toCPointer())
                    ?: return ""
            val strBytes = alphaTab.alphaSkia.cinterop.alphaskia_string_get_utf8(str)!!
            alphaTab.alphaSkia.cinterop.alphaskia_string_free(str)
            return strBytes.toKStringFromUtf8()
        }


        actual fun alphaskiaTypefaceIsBold(typeface: alphaskia_typeface_t): Boolean {
            return alphaTab.alphaSkia.cinterop.alphaskia_typeface_is_bold(typeface.toCPointer()) != 0.toUByte()
        }


        actual fun alphaskiaTypefaceIsItalic(typeface: alphaskia_typeface_t): Boolean {
            return alphaTab.alphaSkia.cinterop.alphaskia_typeface_is_italic(typeface.toCPointer()) != 0.toUByte()
        }


        actual fun alphaskiaImageGetWidth(image: alphaskia_image_t): Int {
            return alphaTab.alphaSkia.cinterop.alphaskia_image_get_width(image.toCPointer())
        }


        actual fun alphaskiaImageGetHeight(image: alphaskia_image_t): Int {
            return alphaTab.alphaSkia.cinterop.alphaskia_image_get_height(image.toCPointer())
        }


        actual fun alphaskiaImageReadPixels(image: alphaskia_image_t): ByteArray? {
            val rowBytes = alphaskiaImageGetWidth(image) * 4
            val arrayBuffer = UByteArray(rowBytes * alphaskiaImageGetHeight(image))

            if (alphaTab.alphaSkia.cinterop.alphaskia_image_read_pixels(
                    image.toCPointer(),
                    arrayBuffer.refTo(0),
                    rowBytes.toULong()
                ) == 0.toUByte()
            ) {
                return null
            }

            return arrayBuffer.asByteArray()
        }


        actual fun alphaskiaImageEncodePng(image: alphaskia_image_t): ByteArray? {
            val data = alphaTab.alphaSkia.cinterop.alphaskia_image_encode_png(image.toCPointer())
                ?: return null
            val raw = alphaTab.alphaSkia.cinterop.alphaskia_data_get_data(data)!!
            val bytes =
                raw.readBytes(alphaTab.alphaSkia.cinterop.alphaskia_data_get_length(data).toInt())
            alphaTab.alphaSkia.cinterop.alphaskia_data_free(data)
            return bytes
        }


        actual fun alphaskiaImageDecode(data: ByteArray): alphaskia_image_t {
            return alphaTab.alphaSkia.cinterop.alphaskia_image_decode(
                data.asUByteArray().refTo(0),
                data.size.toULong()
            ).toLong()
        }


        actual fun alphaskiaImageFromPixels(
            width: Int,
            height: Int,
            pixels: ByteArray
        ): alphaskia_image_t {
            return alphaTab.alphaSkia.cinterop.alphaskia_image_from_pixels(
                width,
                height,
                pixels.asUByteArray().refTo(0)
            ).toLong()
        }


        actual fun alphaskiaImageFree(image: alphaskia_image_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_image_free(image.toCPointer())
        }


        actual fun alphaskiaCanvasNew(): alphaskia_canvas_t {
            return alphaTab.alphaSkia.cinterop.alphaskia_canvas_new().toLong()
        }


        actual fun alphaskiaCanvasFree(canvas: alphaskia_canvas_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_free(canvas.toCPointer())
        }


        actual fun alphaskiaCanvasSetColor(canvas: alphaskia_canvas_t, color: Int) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_set_color(
                canvas.toCPointer(),
                color.toUInt()
            )
        }


        actual fun alphaskiaCanvasGetColor(canvas: alphaskia_canvas_t): Int {
            return alphaTab.alphaSkia.cinterop.alphaskia_canvas_get_color(canvas.toCPointer())
                .toInt()
        }


        actual fun alphaskiaCanvasSetLineWidth(
            canvas: alphaskia_canvas_t,
            line_width: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_set_line_width(
                canvas.toCPointer(),
                line_width
            )
        }


        actual fun alphaskiaCanvasGetLineWidth(canvas: alphaskia_canvas_t): Float {
            return alphaTab.alphaSkia.cinterop.alphaskia_canvas_get_line_width(canvas.toCPointer())
        }


        actual fun alphaskiaCanvasBeginRender(
            canvas: alphaskia_canvas_t,
            width: Int,
            height: Int,
            render_scale: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_begin_render(
                canvas.toCPointer(),
                width,
                height,
                render_scale
            )
        }


        actual fun alphaskiaCanvasEndRender(canvas: alphaskia_canvas_t): alphaskia_image_t {
            return alphaTab.alphaSkia.cinterop.alphaskia_canvas_end_render(canvas.toCPointer())
                .toLong()
        }


        actual fun alphaskiaCanvasFillRect(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            width: Float,
            height: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_fill_rect(
                canvas.toCPointer(),
                x,
                y,
                width,
                height
            )
        }


        actual fun alphaskiaCanvasStrokeRect(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            width: Float,
            height: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_stroke_rect(
                canvas.toCPointer(),
                x,
                y,
                width,
                height
            )
        }


        actual fun alphaskiaCanvasBeginPath(canvas: alphaskia_canvas_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_begin_path(
                canvas.toCPointer()
            )
        }


        actual fun alphaskiaCanvasClosePath(canvas: alphaskia_canvas_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_close_path(
                canvas.toCPointer()
            )
        }


        actual fun alphaskiaCanvasMoveTo(canvas: alphaskia_canvas_t, x: Float, y: Float) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_move_to(
                canvas.toCPointer(),
                x, y
            )
        }


        actual fun alphaskiaCanvasLineTo(canvas: alphaskia_canvas_t, x: Float, y: Float) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_line_to(
                canvas.toCPointer(),
                x, y
            )
        }


        actual fun alphaskiaCanvasQuadraticCurveTo(
            canvas: alphaskia_canvas_t,
            cpx: Float,
            cpy: Float,
            x: Float,
            y: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_quadratic_curve_to(
                canvas.toCPointer(),
                cpx, cpy, x, y
            )
        }


        actual fun alphaskiaCanvasBezierCurveTo(
            canvas: alphaskia_canvas_t,
            cp1x: Float,
            cp1y: Float,
            cp2x: Float,
            cp2y: Float,
            x: Float,
            y: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_bezier_curve_to(
                canvas.toCPointer(),
                cp1x, cp1y, cp2x, cp2y, x, y
            )
        }


        actual fun alphaskiaCanvasFillCircle(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            radius: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_fill_circle(
                canvas.toCPointer(),
                x, y, radius
            )
        }


        actual fun alphaskiaCanvasStrokeCircle(
            canvas: alphaskia_canvas_t,
            x: Float,
            y: Float,
            radius: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_stroke_circle(
                canvas.toCPointer(),
                x, y, radius
            )
        }


        actual fun alphaskiaCanvasFill(canvas: alphaskia_canvas_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_fill(canvas.toCPointer())
        }

        actual fun alphaskiaCanvasStroke(canvas: alphaskia_canvas_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_stroke(canvas.toCPointer())
        }


        actual fun alphaskiaCanvasDrawImage(
            canvas: alphaskia_canvas_t,
            image: alphaskia_image_t,
            x: Float,
            y: Float,

            w: Float,
            h: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_draw_image(
                canvas.toCPointer(),
                image.toCPointer(),
                x,
                y,
                w,
                h
            )
        }


        actual fun alphaskiaCanvasFillText(
            canvas: alphaskia_canvas_t,
            text: String,
            typeface: alphaskia_typeface_t,
            font_size: Float,
            x: Float,
            y: Float,
            text_align: alphaskia_text_align_t,
            baseline: alphaskia_text_baseline_t
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_fill_text(
                canvas.toCPointer(),
                text.wcstr,
                text.length,
                typeface.toCPointer(),
                font_size,
                x, y,
                text_align.toUInt(),
                baseline.toUInt()
            )
        }


        actual fun alphaskiaCanvasMeasureText(
            canvas: alphaskia_canvas_t,
            text: String,
            typeface: alphaskia_typeface_t, font_size: Float
        ): Float {
            return alphaTab.alphaSkia.cinterop.alphaskia_canvas_measure_text(
                canvas.toCPointer(),
                text.wcstr,
                text.length,
                typeface.toCPointer(),
                font_size
            )
        }


        actual fun alphaskiaCanvasBeginRotate(
            canvas: alphaskia_canvas_t,
            center_x: Float,
            center_y: Float,
            angle: Float
        ) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_begin_rotate(
                canvas.toCPointer(),
                center_x, center_y, angle
            )
        }


        actual fun alphaskiaCanvasEndRotate(canvas: alphaskia_canvas_t) {
            alphaTab.alphaSkia.cinterop.alphaskia_canvas_end_rotate(
                canvas.toCPointer(),
            )
        }
    }
}