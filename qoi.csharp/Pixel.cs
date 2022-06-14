using System;

namespace Qoi.Csharp
{
    internal struct Pixel : IEquatable<Pixel>
    {
        public Pixel(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public bool Equals(Pixel other)
        {
            return R == other.R
                && G == other.G
                && B == other.B
                && A == other.A;
        }
    }
}
