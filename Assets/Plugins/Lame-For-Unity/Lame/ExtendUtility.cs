using System.IO;

public static class ExtendUtility
{
    // Only useful before .NET 4
    public static void CopyTo(this Stream input, Stream output)
    {
        byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
        int bytesRead;

        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, bytesRead);
        }
    }
}
