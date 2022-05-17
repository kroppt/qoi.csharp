namespace Qoi.Csharp
{
    public class Image
    {
        public Image(byte[] bytes, Channels channels, ColorSpace colorSpace)
        {
            Bytes = bytes;
            Channels = channels;
            ColorSpace = colorSpace;
        }

        public byte[] Bytes { get; }
        public Channels Channels { get; }
        public ColorSpace ColorSpace { get; }
    }
}
