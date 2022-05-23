namespace Qoi.Csharp
{
    public class Image
    {
        public Image(byte[] bytes, uint width, uint height, Channels channels, ColorSpace colorSpace)
        {
            Bytes = bytes;
            Width = width;
            Height = height;
            Channels = channels;
            ColorSpace = colorSpace;
        }

        public byte[] Bytes { get; }
        public uint Width { get; }
        public uint Height { get; }
        public Channels Channels { get; }
        public ColorSpace ColorSpace { get; }
    }
}
