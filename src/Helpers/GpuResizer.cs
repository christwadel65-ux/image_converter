
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageConvertResize.WPF
{
    internal static class GpuResizer
    {
        // Désactivé temporairement - utilise CPU à la place
        public static bool IsAvailable => false;

        public static Image<Rgba32> Resize(Image<Rgba32> src, int targetW, int targetH)
        {
            throw new NotImplementedException("GPU resizing not available - use CPU fallback");
        }

        public static byte[] ResizePadToPng(Image<Rgba32> src, int size)
        {
            throw new NotImplementedException("GPU resizing not available - use CPU fallback");
        }
    }
}
