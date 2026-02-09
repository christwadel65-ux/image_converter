
using System.Collections.Generic;
using System.IO;

namespace ImageConvertResize.WPF
{
    internal static class IcoHelper
    {
        public static void WriteIcoFromPngBlobs(Dictionary<int, byte[]> pngBySize, Stream output)
        {
            var sizes = new List<int>(pngBySize.Keys); sizes.Sort(); int count = sizes.Count;
            using var bw = new BinaryWriter(output);
            bw.Write((ushort)0); // reserved
            bw.Write((ushort)1); // type icon
            bw.Write((ushort)count);
            long entriesPos = bw.BaseStream.Position;
            for (int i = 0; i < count; i++) bw.Write(new byte[16]);
            var offsets = new List<int>(count); var sizesBytes = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                int sz = sizes[i]; byte[] blob = pngBySize[sz]; int offset = (int)bw.BaseStream.Position; bw.Write(blob); offsets.Add(offset); sizesBytes.Add(blob.Length);
            }
            bw.BaseStream.Position = entriesPos;
            for (int i = 0; i < count; i++)
            {
                int sz = sizes[i]; int widthByte = sz == 256 ? 0 : System.Math.Min(sz, 255); int heightByte = sz == 256 ? 0 : System.Math.Min(sz, 255);
                bw.Write((byte)widthByte);
                bw.Write((byte)heightByte);
                bw.Write((byte)0); // ColorCount
                bw.Write((byte)0); // Reserved
                bw.Write((ushort)1); // Planes
                bw.Write((ushort)32); // BitCount
                bw.Write(sizesBytes[i]);
                bw.Write(offsets[i]);
            }
        }
    }
}
