using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

public static class Program
{
    public static byte[] WrapJpegs(List<byte[]> jpegs)
    {
        if (jpegs == null || !jpegs.Any() || jpegs.Any(j => j == null || j.Length == 0))
            throw new ArgumentException("Image data must not be null or empty");

        using (MemoryStream tiffStream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(tiffStream))
        {
            // TIFF header
            writer.Write(new byte[] { 0x49, 0x49 }); // Little-endian
            writer.Write((ushort)42); // TIFF magic number
            writer.Write((uint)8); // Offset to first IFD

            uint nextIFDOffset = 8;

            foreach (var jpegData in jpegs)
            {
                using (var imgStream = new MemoryStream(jpegData))
                using (var image = Image.FromStream(imgStream))
                {
                    var width = image.Width;
                    var height = image.Height;
                    var xres = (int)(image.HorizontalResolution > 0 ? image.HorizontalResolution : 72);
                    var yres = (int)(image.VerticalResolution > 0 ? image.VerticalResolution : 72);

                    // Calculate offsets
                    long ifdPosition = nextIFDOffset;
                    writer.BaseStream.Seek(ifdPosition, SeekOrigin.Begin);

                    // Write number of directory entries (14)
                    writer.Write((ushort)14);

                    // IFD entries
                    WriteTiffEntry(writer, 254, 4, 1, 0);          // NewSubfileType
                    WriteTiffEntry(writer, 256, 4, 1, (uint)width); // ImageWidth
                    WriteTiffEntry(writer, 257, 4, 1, (uint)height);// ImageLength
                    WriteTiffEntry(writer, 258, 3, 3, (uint)(ifdPosition + 2 + 14 * 12 + 4)); // BitsPerSample
                    WriteTiffEntry(writer, 259, 3, 1, 7);          // Compression (JPEG)
                    WriteTiffEntry(writer, 262, 3, 1, 6);          // PhotometricInterpretation
                    WriteTiffEntry(writer, 273, 4, 1, (uint)(ifdPosition + 2 + 14 * 12 + 4 + 22)); // StripOffsets
                    WriteTiffEntry(writer, 277, 3, 1, 3);          // SamplesPerPixel
                    WriteTiffEntry(writer, 278, 4, 1, (uint)height); // RowsPerStrip
                    WriteTiffEntry(writer, 279, 4, 1, (uint)jpegData.Length); // StripByteCounts
                    WriteTiffEntry(writer, 282, 5, 1, (uint)(ifdPosition + 2 + 14 * 12 + 4 + 6)); // XResolution
                    WriteTiffEntry(writer, 283, 5, 1, (uint)(ifdPosition + 2 + 14 * 12 + 4 + 14)); // YResolution
                    WriteTiffEntry(writer, 284, 3, 1, 1);          // PlanarConfiguration
                    WriteTiffEntry(writer, 296, 3, 1, 2);          // ResolutionUnit

                    // Next IFD offset
                    nextIFDOffset = (uint)(ifdPosition + 2 + 14 * 12 + 4 + 22 + jpegData.Length);
                    if (jpegs.IndexOf(jpegData) == jpegs.Count - 1) nextIFDOffset = 0;
                    writer.Write(nextIFDOffset);

                    // Write additional data
                    long dataPosition = ifdPosition + 2 + 14 * 12 + 4;
                    writer.BaseStream.Seek(dataPosition, SeekOrigin.Begin);

                    // BitsPerSample
                    writer.Write((ushort)8);
                    writer.Write((ushort)8);
                    writer.Write((ushort)8);

                    // XResolution
                    writer.Write((uint)xres);
                    writer.Write((uint)1);

                    // YResolution
                    writer.Write((uint)yres);
                    writer.Write((uint)1);

                    // JPEG data
                    writer.BaseStream.Seek(dataPosition + 22, SeekOrigin.Begin);
                    writer.Write(jpegData);
                }
            }

            return tiffStream.ToArray();
        }
    }

    private static void WriteTiffEntry(BinaryWriter writer, ushort tag, ushort type, uint count, uint value)
    {
        writer.Write(tag);
        writer.Write(type);
        writer.Write(count);
        writer.Write(value);
    }

    public static void Main(string[] args)
    {
        try
        {
            var inputDir = Path.Combine(Directory.GetCurrentDirectory(), "input");
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
            
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputDir);

            var jpegFiles = Directory.GetFiles(inputDir, "*.jpg");
            if (jpegFiles.Length == 0)
            {
                Console.WriteLine($"No JPEG files found in {inputDir}");
                Console.WriteLine("Place JPEG files in the 'input' directory");
                return;
            }

            var jpegDataList = jpegFiles.Select(File.ReadAllBytes).ToList();
            byte[] tiffBytes = WrapJpegs(jpegDataList);

            var outputPath = Path.Combine(outputDir, "output.tiff");
            File.WriteAllBytes(outputPath, tiffBytes);
            
            Console.WriteLine($"Created {outputPath}");
            Console.WriteLine($"Pages: {jpegFiles.Length}");
            Console.WriteLine($"Size: {tiffBytes.Length / 1024} KB");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}