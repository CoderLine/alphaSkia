import { AlphaSkiaImage } from './AlphaSkiaImage';
import { AlphaSkiaNative } from './AlphaSkiaNative';
import { AlphaSkiaTypeface } from './AlphaSkiaTypeface';
import { AlphaSkiaTextBaseline, AlphaSkiaCanvasHandle, AlphaSkiaColorType, AlphaSkiaTextAlign, loadAddon } from './addon';

export class AlphaSkiaCanvas extends AlphaSkiaNative<AlphaSkiaCanvasHandle> {
    static #colorType: AlphaSkiaColorType | undefined;

    public static get colorType(): AlphaSkiaColorType {
        if (!AlphaSkiaCanvas.#colorType) {
            AlphaSkiaCanvas.#colorType = loadAddon().alphaskia_get_color_type();
        }
        return AlphaSkiaCanvas.#colorType;
    }

    public get color(): number {
        this.checkDisposed();
        return loadAddon().alphaskia_canvas_get_color(this.handle!);
    }

    public set color(color: number) {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_set_color(this.handle!, color);
    }

    public static rgbaToColor(r: number, g: number, b: number, a: number) {
        return ((a & 0xFF) << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | ((b & 0xFF) << 0);
    }

    public get lineWidth(): number {
        this.checkDisposed();
        return loadAddon().alphaskia_canvas_get_line_width(this.handle!);
    }

    public set lineWidth(lineWidth: number) {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_set_line_width(this.handle!, lineWidth);
    }

    /**
     * @internal
     */
    public constructor() {
        super(loadAddon().alphaskia_canvas_new()!, loadAddon().alphaskia_canvas_free);
        loadAddon().alphaskia_canvas_begin_render(this.handle!, 100, 100, 1);
    }

    public beginRender(width: number, height: number, renderScale: number = 1): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_begin_render(this.handle!, width, height, renderScale);
    }

    public drawImage(image: AlphaSkiaImage, x: number, y: number, w: number, h: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_draw_image(this.handle!, image.handle!, x, y, w, h);
    }

    public endRender(): AlphaSkiaImage | undefined {
        this.checkDisposed();
        const image = loadAddon().alphaskia_canvas_end_render(this.handle!);
        if (!image) {
            return undefined;
        }
        return new AlphaSkiaImage(image);
    }

    public fillRect(x: number, y: number, width: number, height: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_fill_rect(this.handle!, x, y, width, height);
    }

    public strokeRect(x: number, y: number, width: number, height: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_stroke_rect(this.handle!, x, y, width, height);
    }

    public beginPath(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_begin_path(this.handle!);
    }

    public closePath(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_close_path(this.handle!);
    }

    public moveTo(x: number, y: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_move_to(this.handle!, x, y);
    }

    public lineTo(x: number, y: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_line_to(this.handle!, x, y);
    }

    public quadraticCurveTo(cpx: number, cpy: number, x: number, y: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_quadratic_curve_to(this.handle!, cpx, cpy, x, y);
    }

    public bezierCurveTo(cp1x: number, cp1y: number, cp2x: number, cp2y: number, x: number, y: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_bezier_curve_to(this.handle!, cp1x, cp1y, cp2x, cp2y, x, y);
    }

    public fillCircle(x: number, y: number, radius: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_fill_circle(this.handle!, x, y, radius);
    }

    public strokeCircle(x: number, y: number, radius: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_stroke_circle(this.handle!, x, y, radius);
    }

    public fill(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_fill(this.handle!);
    }

    public stroke(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_stroke(this.handle!);
    }

    public fillText(text: string, typeface: AlphaSkiaTypeface, fontSize: number, x: number, y: number, textAlign: AlphaSkiaTextAlign, baseline: AlphaSkiaTextBaseline): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_fill_text(this.handle!, text, typeface.handle!, fontSize, x, y, textAlign as number, baseline as number);
    }

    public measureText(text: string, typeface: AlphaSkiaTypeface, fontSize: number): number {
        this.checkDisposed();
        return loadAddon().alphaskia_canvas_measure_text(this.handle!, text, typeface.handle!, fontSize);
    }

    public beginRotate(centerX: number, centerY: number, angle: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_begin_rotate(this.handle!, centerX, centerY, angle);
    }

    public endRotate(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_end_rotate(this.handle!);
    }
}