namespace Helpers;

public static class FileOperationsHelpers
{
    public static byte[] LoadImageAsBytes(string imagePath)
    {
        Console.WriteLine("===============================");
        Console.WriteLine(imagePath);
        Console.WriteLine("===============================");
        using var image = File.OpenRead(imagePath);
        using var memoryStream = new MemoryStream();
        image.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}