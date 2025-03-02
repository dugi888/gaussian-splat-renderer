using OpenTK.Mathematics;

namespace GaussianSplatRenderer;

public class Camera
{
    public Vector3 Position { get; set; } = new Vector3(0, 0, 10);
    public Vector3 Target { get; set; } = Vector3.Zero;
    public float Azimuth { get; set; } = 0.0f;
    public float Elevation { get; set; } = 0.0f;
    public float FieldOfView { get; set; } = MathHelper.PiOver4;

    public Matrix4 GetViewProjectionMatrix(float aspectRatio)
    {
        Matrix4 view = Matrix4.LookAt(Position, Target, Vector3.UnitY);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, 0.1f, 100.0f);
        return view * projection;
    }

    public void UpdateViewMatrix()
    {
        float x = 5 * MathF.Cos(Elevation) * MathF.Cos(Azimuth);
        float y = 5 * MathF.Sin(Elevation);
        float z = 5 * MathF.Cos(Elevation) * MathF.Sin(Azimuth);
        Position = new Vector3(x, y, z);
    }
}
