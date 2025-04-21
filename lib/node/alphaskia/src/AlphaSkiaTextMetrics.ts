import { AlphaSkiaNative } from './AlphaSkiaNative';
import { AlphaSkiaTextMetricsHandle, loadAddon } from './addon';

/**
 * Represents a typeface to draw text.
 */
export class AlphaSkiaTextMetrics extends AlphaSkiaNative<AlphaSkiaTextMetricsHandle> {


    /**
     * Returns the width of a segment of inline text in pixels. It takes into account the current font of the context.
     */
    public get width(): number {
        return loadAddon().alphaskia_text_metrics_get_width(this.handle!);
    }

    /**
     * Distance parallel to the baseline from the alignment point given by the textAlign parameter to the left side of the bounding rectangle of the given text, in pixels; positive numbers indicating a distance going left from the given alignment point.
     */
    public get actualBoundingBoxLeft(): number {
        return loadAddon().alphaskia_text_metrics_get_actual_bounding_box_left(this.handle!);
    }

    /**
     * Returns the distance from the alignment point given by the textAlign parameter to the right side of the bounding rectangle of the given text, in pixels. The distance is measured parallel to the baseline.
     */
    public get actualBoundingBoxRight(): number {
        return loadAddon().alphaskia_text_metrics_get_actual_bounding_box_right(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the highest bounding rectangle of all the fonts used to render the text, in pixels.
     */
    public get fontBoundingBoxAscent(): number {
        return loadAddon().alphaskia_text_metrics_get_font_bounding_box_ascent(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the bounding rectangle of all the fonts used to render the text, in pixels.
     */
    public get fontBoundingBoxDescent(): number {
        return loadAddon().alphaskia_text_metrics_get_font_bounding_box_descent(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the bounding rectangle used to render the text, in pixels.
     */
    public get actualBoundingBoxAscent(): number {
        return loadAddon().alphaskia_text_metrics_get_actual_bounding_box_ascent(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the bounding rectangle used to render the text, in pixels.
     */
    public get actualBoundingBoxDescent(): number {
        return loadAddon().alphaskia_text_metrics_get_actual_bounding_box_descent(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the em square in the line box, in pixels.
     */
    public get emHeightAscent(): number {
        return loadAddon().alphaskia_text_metrics_get_em_height_ascent(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the em square in the line box, in pixels.
     */
    public get emHeightDescent(): number {
        return loadAddon().alphaskia_text_metrics_get_em_height_descent(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the hanging baseline of the line box, in pixels.
     */
    public get hangingBaseline(): number {
        return loadAddon().alphaskia_text_metrics_get_hanging_baseline(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the alphabetic baseline of the line box, in pixels.
     */
    public get alphabeticBaseline(): number {
        return loadAddon().alphaskia_text_metrics_get_alphabetic_baseline(this.handle!);
    }

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the ideographic baseline of the line box, in CSS pixels.
     */
    public get ideographicBaseline(): number {
        return loadAddon().alphaskia_text_metrics_get_ideographic_baseline(this.handle!);
    }

    /**
     * @internal
     */
    public constructor(handle: AlphaSkiaTextMetricsHandle) {
        super(handle, loadAddon().alphaskia_text_metrics_free);
    }
}