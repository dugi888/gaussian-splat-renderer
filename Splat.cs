using OpenTK.Mathematics;


namespace GaussianSplatRenderer;

public class Splat
{
    public Vector3 Position { get; set; }
    public Color4 Color { get; set; }
    public Vector3 Scale { get; set; }
    public Quaternion Rotation { get; set; }
}