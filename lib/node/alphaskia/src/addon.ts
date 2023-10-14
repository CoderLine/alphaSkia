
import { createRequire } from 'node:module';
import { findAddonPath } from './AlphaSkiaPlatform';

export interface AlphaSkiaDataHandle { }
export interface AlphaSkiaTypefaceHandle { }
export interface AlphaSkiaImageHandle { }
export interface AlphaSkiaCanvasHandle { }

/**
 * Lists all text alignments which can be used to draw text.
 */
export enum AlphaSkiaTextAlign {
    /**
     * The text is drawn left aligned to the provided position.
     */
    Left = 0,
    /**
     * The text is drawn centered to the provided position.
     */
    Center = 1,
    /**
     * The text is drawn right aligned to the provided position.
     */
    Right = 2
}

/**
 * Lists all vertical text baseline alignments which can be used to draw text.
 */
export enum AlphaSkiaTextBaseline {
    /**
     * The text is drawn using the alphabetic baseline.
     */
    Alphabetic = 0,
    /**
     * The text is top-aligned to the provided position.
     */
    Top = 1,
    /**
     * The test is middle-aligned (vertically centered) to the provided position.
     */
    Middle = 2,
    /**
     * The text is bottom-aligned to the provided position.
     */
    Bottom = 3
}

/**
 * Lists all known color type with which AlphaSkia creates images.
 */
export enum AlphaSkiaColorType {
    // NOTE: we internally use kN32_SkColorType which is either  kBGRA_8888_SkColorType or kRGBA_8888_SkColorType
    // hence we don't need to expose the other SkColorType values

    /**
     * pixel with 8 bits for red, green, blue, alpha; in 32-bit word
     */
    Rgba888 = 4,

    /**
     * pixel with 8 bits for blue, green, red, alpha; in 32-bit word
     */
    Bgra888 = 6
}

/**
 * @internal
 */
export interface AlphaSkiaNodeAddon {
    alphaskia_get_color_type(): AlphaSkiaColorType;

    alphaskia_data_new_copy(data: ArrayBuffer): AlphaSkiaDataHandle | undefined;
    alphaskia_data_get_data(data: AlphaSkiaDataHandle): ArrayBuffer;
    alphaskia_data_free(data: AlphaSkiaDataHandle): void;

    alphaskia_typeface_register(data: AlphaSkiaDataHandle): AlphaSkiaTypefaceHandle | undefined;
    alphaskia_typeface_free(type_face: AlphaSkiaTypefaceHandle): void;
    alphaskia_typeface_make_from_name(name: string, bold: boolean, italic: boolean): AlphaSkiaTypefaceHandle | undefined;

    alphaskia_image_get_width(image: AlphaSkiaImageHandle): number;
    alphaskia_image_get_height(image: AlphaSkiaImageHandle): number;
    alphaskia_image_read_pixels(image: AlphaSkiaImageHandle): ArrayBuffer | undefined;
    alphaskia_image_encode_png(image: AlphaSkiaImageHandle): ArrayBuffer;
    alphaskia_image_from_pixels(width: number, height: number, pixels: ArrayBuffer): AlphaSkiaImageHandle;
    alphaskia_image_decode(pixels: ArrayBuffer): AlphaSkiaImageHandle;
    alphaskia_image_free(image: AlphaSkiaImageHandle): void;

    alphaskia_canvas_new(): AlphaSkiaCanvasHandle | undefined;
    alphaskia_canvas_free(canvas: AlphaSkiaCanvasHandle): void;

    alphaskia_canvas_set_color(canvas: AlphaSkiaCanvasHandle, color: number): void;
    alphaskia_canvas_get_color(canvas: AlphaSkiaCanvasHandle): number;

    alphaskia_canvas_set_line_width(canvas: AlphaSkiaCanvasHandle, line_width: number): void;
    alphaskia_canvas_get_line_width(canvas: AlphaSkiaCanvasHandle): number;

    alphaskia_canvas_begin_render(canvas: AlphaSkiaCanvasHandle, width: number, height: number, render_scale: number): void;
    alphaskia_canvas_end_render(canvas: AlphaSkiaCanvasHandle): AlphaSkiaImageHandle | undefined;
    alphaskia_canvas_fill_rect(canvas: AlphaSkiaCanvasHandle, x: number, y: number, width: number, height: number): void;
    alphaskia_canvas_stroke_rect(canvas: AlphaSkiaCanvasHandle, x: number, y: number, width: number, height: number): void;
    alphaskia_canvas_begin_path(canvas: AlphaSkiaCanvasHandle): void;
    alphaskia_canvas_close_path(canvas: AlphaSkiaCanvasHandle): void;
    alphaskia_canvas_move_to(canvas: AlphaSkiaCanvasHandle, x: number, y: number): void;
    alphaskia_canvas_line_to(canvas: AlphaSkiaCanvasHandle, x: number, y: number): void;
    alphaskia_canvas_quadratic_curve_to(canvas: AlphaSkiaCanvasHandle, cpx: number, cpy: number, x: number, y: number): void;
    alphaskia_canvas_bezier_curve_to(canvas: AlphaSkiaCanvasHandle, cp1x: number, cp1y: number, cp2x: number, cp2y: number, x: number, y: number): void;
    alphaskia_canvas_fill_circle(canvas: AlphaSkiaCanvasHandle, x: number, y: number, radius: number): void;
    alphaskia_canvas_stroke_circle(canvas: AlphaSkiaCanvasHandle, x: number, y: number, radius: number): void;
    alphaskia_canvas_fill(canvas: AlphaSkiaCanvasHandle): void;
    alphaskia_canvas_stroke(canvas: AlphaSkiaCanvasHandle): void;
    alphaskia_canvas_draw_image(canvas: AlphaSkiaCanvasHandle, image: AlphaSkiaImageHandle, x: number, y: number, w: number, h: number): void;
    
    alphaskia_canvas_fill_text(canvas: AlphaSkiaCanvasHandle, text: string, typeface: AlphaSkiaTypefaceHandle, font_size: number, x: number, y: number, text_align: AlphaSkiaTextAlign, baseline: AlphaSkiaTextBaseline): void;
    alphaskia_canvas_measure_text(canvas: AlphaSkiaCanvasHandle, text: string, typeface: AlphaSkiaTypefaceHandle, font_size: number): number;
    alphaskia_canvas_begin_rotate(canvas: AlphaSkiaCanvasHandle, center_x: number, center_y: number, angle: number): void;
    alphaskia_canvas_end_rotate(canvas: AlphaSkiaCanvasHandle): void;
}

const require = createRequire(import.meta.url);
let addonInstance: AlphaSkiaNodeAddon | undefined = undefined

/**
 * @internal
 */
export function loadAddon() {
    if (!addonInstance) {
        const addonPath = findAddonPath();
        if (!addonPath) {
            throw new ReferenceError("alphaSkia native addon could not be found");
        }
        addonInstance = require(addonPath) as AlphaSkiaNodeAddon
    }
    return addonInstance;
};
