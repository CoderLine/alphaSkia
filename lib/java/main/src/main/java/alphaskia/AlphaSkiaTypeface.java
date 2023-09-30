package alphaskia;

public class AlphaSkiaTypeface extends AlphaSkiaNative {
    private final AlphaSkiaData data;

    private AlphaSkiaTypeface(long handle, AlphaSkiaData data) {
        super(handle);
        this.data = data;
    }

    @Override
    public void close() {
        AlphaSkiaData data = this.data;
        if (data != null) {
            data.close();
        }
        release(handle);
        handle = 0;
    }

    private native void release(long handle);

    public static AlphaSkiaTypeface register(byte[] data) {
        var nativeData = new AlphaSkiaData(data);
        var typeface = register(nativeData.handle);
        if (typeface == 0) {
            nativeData.close();
            return null;
        }
        return new AlphaSkiaTypeface(typeface, nativeData);
    }

    private static native long register(long handle);

    public static AlphaSkiaTypeface create(String name, boolean bold, boolean italic) {
        var typeface = makeFromName(name, bold, italic);
        if (typeface == 0) {
            return null;
        }

        return new AlphaSkiaTypeface(typeface, null);
    }

    private static native long makeFromName(String name, boolean bold, boolean italic);
}
