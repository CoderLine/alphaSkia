import { AlphaSkiaImage } from './AlphaSkiaImage';
import { AlphaSkiaNative } from './AlphaSkiaNative';
import { AlphaSkiaTextStyle } from './AlphaSkiaTextStyle';
import { AlphaSkiaTextBaseline, AlphaSkiaCanvasHandle, AlphaSkiaColorType, AlphaSkiaTextAlign, loadAddon } from './addon';

/**
 * A Skia based canvas for rendering images.
 */
export class AlphaSkiaCanvas extends AlphaSkiaNative<AlphaSkiaCanvasHandle> {
    static #colorType: AlphaSkiaColorType | undefined;

    /**
     * Gets the {@link AlphaSkiaColorType} used when generating images.
     * AlphaSkia tries to use the native color tpe of the system but this can vary based on OS.
     * @return The {@link AlphaSkiaColorType} used when generating images
     */
    public static get colorType(): AlphaSkiaColorType {
        if (!AlphaSkiaCanvas.#colorType) {
            AlphaSkiaCanvas.#colorType = loadAddon().alphaskia_get_color_type();
        }
        return AlphaSkiaCanvas.#colorType;
    }

    /**
     * Switches the rendering to use the operating system font manager and font rendering. 
     * This results in a platform specific display of any rendered texts and allows using of any
     * fonts installed on the system.
     */
    public static switchToOperatingSystemFonts(): void {
        loadAddon().alphaskia_switch_to_operating_system_fonts();
    }

    /**
     * Switches the rendering to use the FreeType font manager and font rendering. 
     * This results in a platform independent display of any rendered texts achieving consistent rendering. 
     * Operating system fonts cannot be used in this mode.
     */
    public static switchToFreeTypeFonts(): void {
        loadAddon().alphaskia_switch_to_freetype_fonts();
    }
    
    /**
     * Gets the color to use for drawing operations in the native canvas.
     * See also {@link rgbaToColor}
     * @return The color to use for drawing operations in the native canvas
     */
    public get color(): number {
        this.checkDisposed();
        return loadAddon().alphaskia_canvas_get_color(this.handle!);
    }

    /**
     * Sets the color to use for drawing operations in the native canvas.
     * See also {@link rgbaToColor}
     * @param color The color to use for drawing operations in the native canvas.
     */
    public set color(color: number) {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_set_color(this.handle!, color);
    }

    /**
     * Encodes the given color components into the right format.
     *
     * @param r The red component of the color.
     * @param g The green component of the color.
     * @param b The blue component of the color.
     * @param a The alpha channel of the color.
     * @return The encoded color.
     */
    public static rgbaToColor(r: number, g: number, b: number, a: number): number {
        return ((a & 0xFF) << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | ((b & 0xFF) << 0);
    }

    /**
     * Gets the line width to use when drawing strokes and lines.
     * @return The line width to use when drawing strokes and lines.
     */
    public get lineWidth(): number {
        this.checkDisposed();
        return loadAddon().alphaskia_canvas_get_line_width(this.handle!);
    }

    /**
     * Sets the line width to use when drawing strokes and lines.
     * @param lineWidth The line width to use when drawing strokes and lines
     */
    public set lineWidth(lineWidth: number) {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_set_line_width(this.handle!, lineWidth);
    }

    /**
     * Initializes a new empty canvas to use for drawing.
     */
    public constructor() {
        super(loadAddon().alphaskia_canvas_new()!, loadAddon().alphaskia_canvas_free);
        loadAddon().alphaskia_canvas_begin_render(this.handle!, 100, 100, 1);
    }

    /**
     * Starts a new rendering session in the canvas.
     *
     * @param width  The width of the image to produce.
     * @param height The height of the image to produce.
     * @param scaleFactor The scale factor for the image (e.g. for high DPI rendering with keeping coordinates).
     */
    public beginRender(width: number, height: number, renderScale: number = 1): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_begin_render(this.handle!, width, height, renderScale);
    }

    /**
     * Draws the given image into the canvas.
     *
     * @param image The image to draw.
     * @param x     The X-coordinate at which to draw the image.
     * @param y     The Y-coordinate at which to draw the image.
     * @param w     The target width to which the image should be scaled.
     * @param h     The target height to which the image should be scaled.
     */
    public drawImage(image: AlphaSkiaImage, x: number, y: number, w: number, h: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_draw_image(this.handle!, image.handle!, x, y, w, h);
    }

    /**
     * Ends the rendering session and provides the rendered result.
     *
     * @return The rendered result or {@code undefined} if something went wrong.
     */
    public endRender(): AlphaSkiaImage | undefined {
        this.checkDisposed();
        const image = loadAddon().alphaskia_canvas_end_render(this.handle!);
        if (!image) {
            return undefined;
        }
        return new AlphaSkiaImage(image);
    }

    /**
     * Fills the given rectangle with the current color.
     *
     * @param x      The X-coordinate of the rectangle
     * @param y      The Y-coordinate of the rectangle
     * @param width  The width of the rectangle
     * @param height The height of the rectangle
     */
    public fillRect(x: number, y: number, width: number, height: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_fill_rect(this.handle!, x, y, width, height);
    }

    /**
     * Strokes the given rectangle with the current color.
     *
     * @param x      The X-coordinate of the rectangle
     * @param y      The Y-coordinate of the rectangle
     * @param width  The width of the rectangle
     * @param height The height of the rectangle
     */
    public strokeRect(x: number, y: number, width: number, height: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_stroke_rect(this.handle!, x, y, width, height);
    }

    /**
     * Begins a new dynamic path for rendering.
     */
    public beginPath(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_begin_path(this.handle!);
    }

    /**
     * Closes the started path by connecting the last point and the first point.
     */
    public closePath(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_close_path(this.handle!);
    }

    /**
     * Moves the current position within the current path to the given position.
     *
     * @param x The X-position where to continue the path.
     * @param y The Y-position where to continue the path.
     */
    public moveTo(x: number, y: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_move_to(this.handle!, x, y);
    }

    /**
     * Draws a line from the current path position to the given position.
     *
     * @param x The X-position to which to draw a line.
     * @param y The Y-position to which to draw a line.
     */
    public lineTo(x: number, y: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_line_to(this.handle!, x, y);
    }

    /**
     * Draws a quadratic curve from the current path position to the given position.
     *
     * @param cpx The X-position of the control point.
     * @param cpy The Y-position of the control point.
     * @param x   The X-position of the curve end.
     * @param y   The Y-position of the curve end.
     */
    public quadraticCurveTo(cpx: number, cpy: number, x: number, y: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_quadratic_curve_to(this.handle!, cpx, cpy, x, y);
    }

    /**
     * Draws a Bézier curve from the current path position to the given position.
     *
     * @param cp1X The X-position of the first control point.
     * @param cp1Y The Y-position of the first control point.
     * @param cp2X The X-position of the second control point.
     * @param cp2Y The Y-position of the second control point.
     * @param x    The X-position of the curve end.
     * @param y    The Y-position of the curve end.
     */
    public bezierCurveTo(cp1x: number, cp1y: number, cp2x: number, cp2y: number, x: number, y: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_bezier_curve_to(this.handle!, cp1x, cp1y, cp2x, cp2y, x, y);
    }

    /**
     * Fills a circle with the current color.
     *
     * @param x      The X-position of the circle center.
     * @param y      The Y-position of the circle center.
     * @param radius The circle radius.
     */
    public fillCircle(x: number, y: number, radius: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_fill_circle(this.handle!, x, y, radius);
    }

    /**
     * Strokes a circle with the current color and line width.
     *
     * @param x      The X-position of the circle center.
     * @param y      The Y-position of the circle center.
     * @param radius The circle radius.
     */
    public strokeCircle(x: number, y: number, radius: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_stroke_circle(this.handle!, x, y, radius);
    }

    /**
     * Fills the current path with the current color.
     */
    public fill(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_fill(this.handle!);
    }

    /**
     * Strokes the current path with the current color and line width.
     */
    public stroke(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_stroke(this.handle!);
    }

    /**
     * Fills a text with the current color and provided details.
     *
     * @param text      The text to draw.
     * @param textStyle  The typeface to use for drawing the text.
     * @param fontSize  The font size to use when drawing the text.
     * @param x         The X-position where to draw the text to.
     * @param y         The Y-position where to draw the text to.
     * @param textAlign How to align the text at the given position horizontally.
     * @param baseline  How to align the text at the given position vertically.
     */
    public fillText(text: string, textStyle: AlphaSkiaTextStyle, fontSize: number, x: number, y: number, textAlign: AlphaSkiaTextAlign, baseline: AlphaSkiaTextBaseline): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_fill_text(this.handle!, text, textStyle.handle!, fontSize, x, y, textAlign as number, baseline as number);
    }

    /**
     * Measures the given text.
     *
     * @param text     The text to measure.
     * @param typeface The typeface to use for drawing the text.
     * @param fontSize The font size to use when drawing the text.
     * @return The horizontal width of the text when it would be drawn.
     */
    public measureText(text: string, textStyle: AlphaSkiaTextStyle, fontSize: number): number {
        this.checkDisposed();
        return loadAddon().alphaskia_canvas_measure_text(this.handle!, text, textStyle.handle!, fontSize);
    }

    /**
     * Rotates the canvas allowing angled drawing. .
     *
     * @param centerX The X-position of the center around which to rotate.
     * @param centerY The Y-position of the center around which to rotate.
     * @param angle   The angle in degrees to rotate.
     */
    public beginRotate(centerX: number, centerY: number, angle: number): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_begin_rotate(this.handle!, centerX, centerY, angle);
    }

    /**
     * Restores the previous rotation state after {@link #beginRotate} was called.
     */
    public endRotate(): void {
        this.checkDisposed();
        loadAddon().alphaskia_canvas_end_rotate(this.handle!);
    }
}