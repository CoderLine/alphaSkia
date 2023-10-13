import bindings from 'bindings';

export interface AlphaSkiaDataHandle { }
export interface AlphaSkiaTypefaceHandle { }
export interface AlphaSkiaImageHandle { }
export interface AlphaSkiaCanvasHandle { }

export enum AlphaSkiaTextAlign {
    Left = 0,
    Center = 1,
    Right = 2
}

export enum AlphaSkiaBaseline {
    Alphabetic = 0,
    Top = 1,
    Middle = 2,
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

    alphaskia_canvas_fill_text(canvas: AlphaSkiaCanvasHandle, text: string, typeface: AlphaSkiaTypefaceHandle, font_size: number, x: number, y: number, text_align: AlphaSkiaTextAlign, baseline: AlphaSkiaBaseline): void;
    alphaskia_canvas_measure_text(canvas: AlphaSkiaCanvasHandle, text: string, typeface: AlphaSkiaTypefaceHandle, font_size: number): number;
    alphaskia_canvas_begin_rotate(canvas: AlphaSkiaCanvasHandle, center_x: number, center_y: number, angle: number): void;
    alphaskia_canvas_end_rotate(canvas: AlphaSkiaCanvasHandle): void;
}

const platformMap: any = {
    "win32": "win",
    "darwin": "macos"
};
const platform = platformMap[process.platform] || process.platform;

export const Addon = bindings({
    bindings: 'libalphaskianode.node',
    try: [
        ['module_root', 'build', `${platform}-${process.arch}`, 'bindings']
    ]
}) as AlphaSkiaNodeAddon;
