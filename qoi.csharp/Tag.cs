namespace Qoi.Csharp
{
    public static class Tag
    {
        public const byte RGB = 0b11111110;
        public const byte RGBA = 0b11111111;
        public const byte MASK = 0b11_000000;
        public const byte INDEX = 0b00_000000;
        public const byte DIFF = 0b01_000000;
        public const byte LUMA = 0b10_000000;
        public const byte RUN = 0b11_000000;
    }
}
