using OpenTK.Mathematics;

namespace GaussianSplatRenderer;

public class Camera
{
    public Vector3 Position { get; private set; }
    public Vector3 Target { get; private set; } = Vector3.Zero;
    public float Azimuth { get; set; }
    public float Elevation { get; set; }
    public float FieldOfView { get; set; } = MathHelper.PiOver4;
    public float Radius { get; set; } // Distance from the camera to the target

    public Camera()
    {
        // Set initial position (e.g., x=10)
        Radius = 10.0f;
        UpdateViewMatrix();
    }

    public Matrix4 GetViewProjectionMatrix(float aspectRatio)
    {
        Vector3 forward = Vector3.Normalize(Target - Position);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        Vector3 up = Vector3.Cross(forward, right);

        // view matrix
        Matrix4 view = Matrix4.LookAt(Position, Target, up);

        // projection matrix
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, 0.1f, 100.0f);

        return view * projection;
    }

    public void UpdateViewMatrix()
    {
        // Clamp elevation to avoid flipping at the poles
        Elevation = Math.Clamp(Elevation, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

        // Calculate the new position based on azimuth and elevation
        float x = Target.X + Radius * MathF.Cos(Elevation) * MathF.Cos(Azimuth);
        float y = Target.Y + Radius * MathF.Sin(Elevation);
        float z = Target.Z + Radius * MathF.Cos(Elevation) * MathF.Sin(Azimuth);

        Position = new Vector3(x, y, z);
    }
}