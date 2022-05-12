using System.IO;

namespace Qoi.Csharp
{
    public class Encoder
    {
        public static byte[] Encode(byte[] input, int width, int height, Channels channels, ColorSpace colorSpace)
        {
            using (var memStream = new MemoryStream())
            {
                using (var binWriter = new BinaryWriter(memStream))
                {
                    binWriter.Write((byte)'q');
                    binWriter.Write((byte)'o');
                    binWriter.Write((byte)'i');
                    binWriter.Write((byte)'f');
                    WriteBigEndian(binWriter, width);
                    WriteBigEndian(binWriter, height);
                    binWriter.Write((byte)channels);
                    binWriter.Write((byte)colorSpace);
                }
                return memStream.ToArray();
            }
        }

        private static void WriteBigEndian(BinaryWriter binWriter, int value)
        {
            binWriter.Write((byte)((value >> 030) & 0xFF));
            binWriter.Write((byte)((value >> 020) & 0xFF));
            binWriter.Write((byte)((value >> 010) & 0xFF));
            binWriter.Write((byte)((value >> 000) & 0xFF));
        }
    }
}
