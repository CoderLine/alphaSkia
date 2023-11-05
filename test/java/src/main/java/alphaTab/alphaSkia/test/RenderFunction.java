package alphaTab.alphaSkia.test;

import alphaTab.alphaSkia.AlphaSkiaCanvas;
import alphaTab.alphaSkia.AlphaSkiaImage;

public interface RenderFunction {
    AlphaSkiaImage render(AlphaSkiaCanvas canvas);
}
