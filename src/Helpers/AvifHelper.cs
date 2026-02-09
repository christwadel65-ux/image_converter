using System;
using System.IO;
using ImageMagick;
using SixLabors.ImageSharp.PixelFormats;
using SixImageSharp = SixLabors.ImageSharp;

namespace ImageConvertResize.WPF
{
    internal static class AvifHelper
    {
        /// <summary>
        /// Charge une image AVIF et la convertit en Image<Rgba32> pour ImageSharp
        /// </summary>
        public static SixImageSharp.Image<Rgba32> LoadAvif(string path)
        {
            using var magickImage = new MagickImage(path);
            
            // Convertir en RGBA pour compatibilité avec ImageSharp
            magickImage.Format = MagickFormat.Rgba;
            
            // Obtenir les pixels bruts
            var width = (int)magickImage.Width;
            var height = (int)magickImage.Height;
            var pixels = magickImage.GetPixelsUnsafe();
            var bytes = pixels.ToByteArray(PixelMapping.RGBA);
            
            // Créer l'image ImageSharp
            var image = SixImageSharp.Image.LoadPixelData<Rgba32>(bytes, width, height);
            return image;
        }

        /// <summary>
        /// Sauvegarde une Image<Rgba32> au format AVIF
        /// </summary>
        public static void SaveAvif(SixImageSharp.Image<Rgba32> image, Stream stream, int quality)
        {
            // Convertir ImageSharp vers ImageMagick
            var width = (uint)image.Width;
            var height = (uint)image.Height;
            
            // Extraire les pixels de ImageSharp
            var pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);
            
            // Créer l'image ImageMagick
            using var magickImage = new MagickImage(pixels, new MagickReadSettings
            {
                Width = width,
                Height = height,
                Format = MagickFormat.Rgba
            });
            
            // Configurer l'encodage AVIF
            magickImage.Format = MagickFormat.Avif;
            magickImage.Quality = (uint)quality;
            
            // Sauvegarder dans le stream
            magickImage.Write(stream, MagickFormat.Avif);
        }

        /// <summary>
        /// Vérifie si le fichier est un AVIF valide
        /// </summary>
        public static bool IsAvif(string path)
        {
            try
            {
                var info = new MagickImageInfo(path);
                return info.Format == MagickFormat.Avif;
            }
            catch
            {
                return false;
            }
        }
    }
}
