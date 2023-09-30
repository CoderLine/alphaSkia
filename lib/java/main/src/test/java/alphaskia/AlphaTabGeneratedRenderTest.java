package alphaskia;

public class AlphaTabGeneratedRenderTest extends MusicSheetRenderTest
{
    @Override
    protected int[][] getPartPositions() {
        return new int[][] {
                { 0, 0 },
                { 0, 164 },
                { 0, 202 },
                { 0, 208 },
                { 587, 455 }        };
    }
    @Override
    protected int getTotalWidth() { return 1300; }
    @Override
    protected int getTotalHeight() { return 519; }
    @Override
    protected RenderFunction[] getAllParts() { return new RenderFunction[] { this::drawMusicSheetPart1, this::drawMusicSheetPart2, this::drawMusicSheetPart3, this::drawMusicSheetPart4, this::drawMusicSheetPart5 }; }


    private AlphaSkiaImage drawMusicSheetPart1(AlphaSkiaCanvas canvas)
    {
        canvas.beginRender((int)1300, (int)164);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("PT Serif", false, false));
        setFontSize((float)32);
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.fillText("Title", getTypeface(), getFontSize(), (float)650, (float)40, getTextAlign(), getTextBaseline());
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("PT Serif", false, false));
        setFontSize((float)20);
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.fillText("Subtitle", getTypeface(), getFontSize(), (float)650, (float)72, getTextAlign(), getTextBaseline());
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("PT Serif", false, false));
        setFontSize((float)20);
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.fillText("Artist", getTypeface(), getFontSize(), (float)650, (float)92, getTextAlign(), getTextBaseline());
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("PT Serif", false, false));
        setFontSize((float)20);
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.fillText("Album", getTypeface(), getFontSize(), (float)650, (float)112, getTextAlign(), getTextBaseline());
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("PT Serif", false, false));
        setFontSize((float)15);
        setTextAlign(AlphaSkiaTextAlign.RIGHT);
        canvas.fillText("Music by Music Writer", getTypeface(), getFontSize(), (float)1260, (float)132, getTextAlign(), getTextBaseline());
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("PT Serif", false, false));
        setFontSize((float)15);
        setTextAlign(AlphaSkiaTextAlign.LEFT);
        canvas.fillText("Words by Words Writer", getTypeface(), getFontSize(), (float)40, (float)132, getTextAlign(), getTextBaseline());
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        return canvas.endRender();
    }

    private AlphaSkiaImage drawMusicSheetPart2(AlphaSkiaCanvas canvas)
    {
        canvas.beginRender((int)1300, (int)38);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("PT Serif", false, true));
        setFontSize((float)12);
        setTextAlign(AlphaSkiaTextAlign.LEFT);
        canvas.fillText("Guitar Standard Tuning", getTypeface(), getFontSize(), (float)40, (float)3, getTextAlign(), getTextBaseline());
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        return canvas.endRender();
    }

    private AlphaSkiaImage drawMusicSheetPart3(AlphaSkiaCanvas canvas)
    {
        canvas.beginRender((int)1300, (int)6);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTextAlign(AlphaSkiaTextAlign.CENTER);
        return canvas.endRender();
    }

    private AlphaSkiaImage drawMusicSheetPart4(AlphaSkiaCanvas canvas)
    {
        canvas.beginRender((int)1300, (int)247);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTextAlign(AlphaSkiaTextAlign.LEFT);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("PT Serif", true, false));
        setFontSize((float)14);
        canvas.fillText("\ue1d5", getMusicTypeface(), (float)(getMusicFontSize() * 0.75), (float)167, (float)40, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC);
        canvas.fillText("= 120", getTypeface(), getFontSize(), (float)179.5, (float)27, getTextAlign(), getTextBaseline());
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)165, (byte)165, (byte)165, (byte)255));
        canvas.fillRect((float)102, (float)79, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)88, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)97, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)106, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)115, (float)116, (float)1.04);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.fillText("\ue050", getMusicTypeface(), (float)(getMusicFontSize() * 1), (float)104, (float)106.52000000000001, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC);
        canvas.fillText("\ue084", getMusicTypeface(), (float)(getMusicFontSize() * 1), (float)137, (float)88, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC);
        canvas.fillText("\ue084", getMusicTypeface(), (float)(getMusicFontSize() * 1), (float)137, (float)106, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)200, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("Roboto", false, false));
        setFontSize((float)11);
        canvas.fillText("1", getTypeface(), getFontSize(), (float)151, (float)67.75, getTextAlign(), getTextBaseline());
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.fillRect((float)210, (float)79.1, (float)1, (float)36.000000000000014);
        canvas.fillRect((float)214, (float)79.1, (float)4, (float)36.000000000000014);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)100));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)100));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)100));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)165, (byte)165, (byte)165, (byte)255));
        canvas.fillRect((float)102, (float)158, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)169, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)180, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)191, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)202, (float)116, (float)1.04);
        canvas.fillRect((float)102, (float)213, (float)116, (float)1.04);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.fillText("\ue06d", getMusicTypeface(), (float)(getMusicFontSize() * 0.9230769230769231), (float)107, (float)185.79999999999998, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        canvas.fillRect((float)210, (float)158.29999999999998, (float)1, (float)55);
        canvas.fillRect((float)214, (float)158.29999999999998, (float)4, (float)55);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)34, (byte)34, (byte)17, (byte)255));
        canvas.beginPath();
        canvas.moveTo((float)102, (float)79.1);
        canvas.lineTo((float)102, (float)213.3);
        canvas.stroke();
        setTypeface(getTypeface("PT Serif", false, true));
        setFontSize((float)12);
        canvas.fillText("Track 1", getTypeface(), getFontSize(), (float)50, (float)79.1, getTextAlign(), getTextBaseline());
        canvas.fillRect((float)96, (float)67.1, (float)3, (float)158.20000000000002);
        canvas.beginPath();
        canvas.moveTo((float)96, (float)67.1);
        canvas.bezierCurveTo((float)96, (float)67.1, (float)96, (float)67.1, (float)108, (float)64.1);
        canvas.bezierCurveTo((float)102, (float)70.1, (float)96, (float)70.1, (float)96, (float)70.1);
        canvas.closePath();
        canvas.fill();
        canvas.beginPath();
        canvas.moveTo((float)96, (float)225.3);
        canvas.bezierCurveTo((float)96, (float)225.3, (float)102, (float)225.3, (float)108, (float)228.3);
        canvas.bezierCurveTo((float)102, (float)222.3, (float)96, (float)222.3, (float)96, (float)222.3);
        canvas.closePath();
        canvas.fill();
        return canvas.endRender();
    }

    private AlphaSkiaImage drawMusicSheetPart5(AlphaSkiaCanvas canvas)
    {
        canvas.beginRender((int)124, (int)24);
        canvas.setColor(AlphaSkiaCanvas.rgbaToColor((byte)0, (byte)0, (byte)0, (byte)255));
        setTypeface(getTypeface("Roboto", true, false));
        setFontSize((float)12);
        setTextAlign(AlphaSkiaTextAlign.LEFT);
        canvas.fillText("rendered by alphaTab", getTypeface(), getFontSize(), (float)0, (float)12, getTextAlign(), getTextBaseline());
        return canvas.endRender();
    }
}