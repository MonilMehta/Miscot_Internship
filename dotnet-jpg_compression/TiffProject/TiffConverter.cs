using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tiff;
using System;
using System.Collections.Generic;
using System.IO;

namespace TiffUtility
{
    public static class TiffConverter
    {
        /// <summary>
        /// Wraps a list of JPEG images into a multi-page TIFF container.
        /// </summary>
        /// <param name="jpegs">List of JPEG image data as byte arrays</param>
        /// <returns>Byte array containing the multi-page TIFF data</returns>
        public static byte[] WrapJpegs(List<byte[]> jpegs)
        {
            if (jpegs == null || jpegs.Count == 0 || jpegs.FindIndex(b => b.Length == 0) > -1)
                throw new ArgumentNullException("Image data must not be null or empty");

            List<Image> images = new List<Image>();
            MemoryStream outputStream = new MemoryStream();

            try
            {
                // Load all JPEG images from byte arrays
                foreach (byte[] jpegData in jpegs)
                {
                    using (MemoryStream ms = new MemoryStream(jpegData))
                    {
                        images.Add(Image.Load(ms));
                    }
                }

                // Configure TIFF encoder with settings similar to original implementation
                var tiffEncoder = new TiffEncoder
                {
                    // Using Deflate compression for good balance of compression and compatibility
                    Compression = SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Deflate,
                    // Enabling big endian to match original implementation
                    BitsPerPixel = TiffBitsPerPixel.Bit24,
                };

                // Encode all images to TIFF
                var rgba32Images = images.ConvertAll(image => image.CloneAs<SixLabors.ImageSharp.PixelFormats.Rgba32>()).ToArray();
                foreach (var image in rgba32Images)
                {
                    tiffEncoder.Encode(image, outputStream);
                }

                return outputStream.ToArray();
            }
            finally
            {
                // Clean up resources
                foreach (var image in images)
                {
                    image?.Dispose();
                }
            }
        }
    }
}