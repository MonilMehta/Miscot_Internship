using ImageMagick;
using System;
using System.IO;
using System.Linq;

namespace TiffConverter
{
    public class PdfConverter
    {
        public static void ConvertToPdf(string inputDirectory, string outputPath, int quality = 25)
        {
            if (string.IsNullOrEmpty(inputDirectory) || string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentNullException("Input directory and output path cannot be null or empty");
            }

            // Ensure output directory exists
            string outputDirectory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Get sorted image files
            var imagePaths = Directory.GetFiles(inputDirectory)
                .Where(file => IsImageFile(file))
                .OrderBy(f => f)
                .ToArray();

            if (imagePaths.Length == 0)
            {
                Console.WriteLine("No images found in the input directory.");
                return;
            }

            using (var images = new MagickImageCollection())
            {
                foreach (var path in imagePaths)
                {
                    var image = new MagickImage(path)
                    {
                        Quality = (uint)quality,  // Cast to uint is required
                        ColorSpace = ColorSpace.sRGB,
                        Format = MagickFormat.Jpeg
                    };

                    // Configure JPEG compression using SetDefine directly
                    image.Settings.SetDefine(MagickFormat.Jpeg, "optimize-coding", "true");
                    image.Settings.SetDefine(MagickFormat.Jpeg, "quantize-colors", "255");
                    image.Settings.Compression = CompressionMethod.JPEG;

                    images.Add(image);
                }

                images.Write(outputPath, MagickFormat.Pdf);
            }

            // Verify output
            FileInfo pdfFileInfo = new FileInfo(outputPath);
            if (pdfFileInfo.Exists)
            {
                Console.WriteLine($"PDF generated successfully: {outputPath}");
                Console.WriteLine($"PDF Size: {pdfFileInfo.Length / 1024.0:F2} KB");
            }
            else
            {
                Console.WriteLine("Failed to generate the PDF.");
            }
        }

        private static bool IsImageFile(string filePath)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
            string extension = Path.GetExtension(filePath).ToLower();
            return allowedExtensions.Contains(extension);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            PdfConverter.ConvertToPdf(
                @".\input\",              // Input directory
                @".\output\result.pdf",   // Output path
                quality: 15               // Quality setting (25-35 recommended)
            );
        }
    }
}