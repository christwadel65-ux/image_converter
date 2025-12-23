
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageConvertResize.WPF
{
    internal static class ImageHelper
    {
        private static readonly HashSet<string> exts = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".tiff", ".tif", ".gif"
        };

        public static IEnumerable<string> GetInputFiles(string inputPath, bool recursive)
        {
            if (File.Exists(inputPath))
            {
                var e = Path.GetExtension(inputPath);
                if (exts.Contains(e))
                    yield return inputPath;
                yield break;
            }
            if (Directory.Exists(inputPath))
            {
                var opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var p in Directory.EnumerateFiles(inputPath, "*.*", opt))
                {
                    var e = Path.GetExtension(p);
                    if (exts.Contains(e))
                        yield return p;
                }
            }
        }

        public static (int width, int height) ComputeTargetSize(int w, int h, int maxW, int maxH)
        {
            if (w <= 0 || h <= 0 || maxW <= 0 || maxH <= 0)
                throw new ArgumentException("Les dimensions doivent être positives");
            if (w <= maxW && h <= maxH) return (w, h);
            double ratioW = (double)maxW / w; double ratioH = (double)maxH / h; double ratio = Math.Min(ratioW, ratioH);
            int newW = Math.Max(1, (int)Math.Round(w * ratio)); int newH = Math.Max(1, (int)Math.Round(h * ratio)); return (newW, newH);
        }

        public static void AutoOrient(SixLabors.ImageSharp.Image img)
        {
            try
            {
                var exif = img.Metadata.ExifProfile;
                if (exif == null) return;
                var orientationTag = exif.TryGetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Orientation, out var orientationValue);
                if (!orientationTag || orientationValue == null) return;
                switch ((ushort)orientationValue.Value)
                {
                    case 2: img.Mutate(x => x.Flip(FlipMode.Horizontal)); break;
                    case 3: img.Mutate(x => x.Rotate(RotateMode.Rotate180)); break;
                    case 4: img.Mutate(x => x.Flip(FlipMode.Vertical)); break;
                    case 5: img.Mutate(x => x.Rotate(RotateMode.Rotate90).Flip(FlipMode.Horizontal)); break;
                    case 6: img.Mutate(x => x.Rotate(RotateMode.Rotate90)); break;
                    case 7: img.Mutate(x => x.Rotate(RotateMode.Rotate270).Flip(FlipMode.Horizontal)); break;
                    case 8: img.Mutate(x => x.Rotate(RotateMode.Rotate270)); break;
                    default: break;
                }
                exif!.RemoveValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Orientation);
                exif.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Orientation, (ushort)1);
            }
            catch { }
        }

        public static IImageEncoder GetEncoder(string format, int quality)
        {
            switch (format.ToLowerInvariant())
            {
                case "jpg": case "jpeg": return new JpegEncoder { Quality = quality };
                case "png": return new PngEncoder { CompressionLevel = PngCompressionLevel.Level6 };
                case "webp": return new WebpEncoder { Quality = quality, FileFormat = WebpFileFormatType.Lossy };
                case "bmp": return new BmpEncoder();
                case "tiff": return new TiffEncoder();
                case "ico": throw new ArgumentException("ICO n'est pas un format raster standard. Utilisez IcoHelper.");
                default: throw new ArgumentException($"Format de sortie inconnu: {format}");
            }
        }

        public static SixLabors.ImageSharp.Image LoadImage(string path)
        {
            return SixLabors.ImageSharp.Image.Load(path);
        }
    }
}
