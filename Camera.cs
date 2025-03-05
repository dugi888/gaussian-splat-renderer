using OpenTK.Mathematics;

namespace GaussianSplatRenderer;

public class Camera
{
    private Vector3 Position { get; set; }
    private Vector3 Target { get; set; } = Vector3.Zero;
    public float Azimuth { get; set; }
    public float Elevation { get; set; }
    public float FieldOfView { get; set; } = MathHelper.PiOver4;
    private float Radius { get; set; } // Distance from the camera to the target

    public Camera()
    {
        // Set initial position (e.g., x=10)
        Position = new Vector3(10, 0, 0);

        // Calculate the initial distance (radius) from the camera to the target
        Radius = Vector3.Distance(Position, Target);

        // Calculate initial azimuth and elevation based on the initial position
        UpdateAnglesFromPosition();
    }

    public Matrix4 GetViewProjectionMatrix(float aspectRatio)
    {
        Vector3 forward = Vector3.Normalize(Target - Position);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, forward));

        // view matrix
        Matrix4 view = Matrix4.LookAt(Position, Target, up);

        // projection matrix
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, 0.1f, 100.0f);

        return view * projection;
    }

    private void UpdateAnglesFromPosition()
    {
        Vector3 direction = Vector3.Normalize(Position - Target);

        Azimuth = MathF.Atan2(direction.Z, direction.X);
        Elevation = MathF.Asin(direction.Y);
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