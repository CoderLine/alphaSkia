package net.alphatab.alphaskia.test;

import net.alphatab.alphaskia.AlphaSkiaCanvas;
import net.alphatab.alphaskia.AlphaSkiaImage;

public interface RenderFunction {
    AlphaSkiaImage render(AlphaSkiaCanvas canvas);
}
