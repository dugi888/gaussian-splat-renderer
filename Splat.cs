using OpenTK.Mathematics;


namespace GaussianSplatRenderer;

public class Splat
{
    public Vector3 Position { get; set; }
    public ColorByte Color { get; set; }
    public Vector3 Scale { get; set; }
    public Quaternion Rotation { get; set; }
}

public struct ColorByte
{
    public byte R, G, B, A;

    public ColorByte(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}