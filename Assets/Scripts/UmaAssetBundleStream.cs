using System.IO;

public class UmaAssetBundleStream : FileStream
{
    private const int headerSize = 256;
    private readonly byte[] baseKeys = Config.Instance.ABKey;
    private readonly byte[] keys;

    public UmaAssetBundleStream(string filename, byte[] keys) : base(filename, FileMode.Open, FileAccess.Read)
    {
        this.keys = keys;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        long StartPosition = Position;
        int res = base.Read(buffer, offset, count);

        for (long i = (StartPosition < headerSize ? headerSize - StartPosition : 0); i < count; i++)
        {
            buffer[i] ^= keys[(StartPosition + i) % (baseKeys.Length * 8)];
        }

        return res;
    }
}
