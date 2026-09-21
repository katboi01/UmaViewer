using System.Runtime.InteropServices;

// On WebGL, File.WriteAllBytes only reaches the sandboxed MEMFS, so saved files never leave the
// tab. Save() routes bytes to the browser download flow instead. No-op elsewhere; callers keep
// their normal File.WriteAllBytes path when Active is false.
public static class WebDownload
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void UmaSaveFile(string name, byte[] data, int length);

    public static bool Active => true;

    public static void Save(string fileName, byte[] data)
    {
        if (data == null || data.Length == 0) return;
        UmaSaveFile(fileName, data, data.Length);
    }
#else
    public static bool Active => false;
    public static void Save(string fileName, byte[] data) { }
#endif
}
