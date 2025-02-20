using System;
using System.IO;

namespace Tiff
{
    static class TiffConverter
    {
        public static byte[] CreateTiffFromJpeg(byte[] jpegData, uint width, uint height)
        {
            if (jpegData == null || jpegData.Length == 0)
                throw new ArgumentNullException(nameof(jpegData), "JPEG data cannot be null or empty.");

            using (MemoryStream tiffData = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(tiffData))
            {
                uint offsetToIFD = 8; // Offset to the first Image File Directory (IFD)
                ushort entryCount = 12; // Number of IFD entries

                // TIFF Header
                writer.Write((ushort)0x4949); // Little-endian byte order
                writer.Write((ushort)42);    // TIFF magic number
                writer.Write(offsetToIFD);   // Offset to IFD

                // Calculate offsets
                uint offsetToStrip = (uint)(offsetToIFD + 2 + (entryCount * 12) + 4 + 12); // After IFD entries and values
                uint offsetToXRes = offsetToStrip + (uint)jpegData.Length;
                uint offsetToYRes = offsetToXRes + 8;

                // Write IFD
                writer.Seek((int)offsetToIFD, SeekOrigin.Begin);
                writer.Write(entryCount);

                WriteIFDEntry(writer, 256, 4, 1, width);                 // ImageWidth
                WriteIFDEntry(writer, 257, 4, 1, height);                // ImageLength
                WriteIFDEntry(writer, 258, 3, 3, offsetToYRes + 8);      // BitsPerSample (3 values for RGB)
                WriteIFDEntry(writer, 259, 3, 1, 7);                     // Compression (7 = JPEG)
                WriteIFDEntry(writer, 262, 3, 1, 6);                     // PhotometricInterpretation (YCbCr)
                WriteIFDEntry(writer, 273, 4, 1, offsetToStrip);         // StripOffsets
                WriteIFDEntry(writer, 277, 3, 1, 3);                     // SamplesPerPixel
                WriteIFDEntry(writer, 278, 4, 1, height);                // RowsPerStrip
                WriteIFDEntry(writer, 279, 4, 1, (uint)jpegData.Length); // StripByteCounts
                WriteIFDEntry(writer, 282, 5, 1, offsetToXRes);          // XResolution
                WriteIFDEntry(writer, 283, 5, 1, offsetToYRes);          // YResolution
                WriteIFDEntry(writer, 296, 3, 1, 2);                     // ResolutionUnit (2 = inches)

                writer.Write((uint)0); // Next IFD offset (no more IFDs)

                // Write BitsPerSample values (8 bits for each of R, G, B)
                writer.Write((ushort)8);
                writer.Write((ushort)8);
                writer.Write((ushort)8);

                // Write XResolution and YResolution
                writer.Write((uint)72); // 72 dpi
                writer.Write((uint)1);  // Fraction denominator
                writer.Write((uint)72); // 72 dpi
                writer.Write((uint)1);  // Fraction denominator

                // Write JPEG data
                writer.Seek((int)offsetToStrip, SeekOrigin.Begin);
                writer.Write(jpegData);

                return tiffData.ToArray();
            }
        }

        private static void WriteIFDEntry(BinaryWriter writer, ushort tag, ushort type, uint count, uint value)
        {
            writer.Write(tag);   // Tag
            writer.Write(type);  // Field type
            writer.Write(count); // Number of values
            writer.Write(value); // Value or offset
        }

        static void Main(string[] args)
        {
            try
            {
                // Load a JPEG file
                byte[] jpegData = File.ReadAllBytes("image1.jpg");

                // Create a TIFF file from the JPEG
                byte[] tiffData = CreateTiffFromJpeg(jpegData, 800, 600); // Replace with actual width/height

                // Save the TIFF file
                File.WriteAllBytes("output.tiff", tiffData);

                Console.WriteLine("TIFF file created successfully: output.tiff");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
