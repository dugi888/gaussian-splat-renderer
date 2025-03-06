using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GaussianSplatRenderer;

public class SplatWindow : GameWindow
{
    private readonly Camera _camera;
    private readonly List<Splat> _splats;
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private Shader _shader;
    private float _lastMouseX;
    private float _lastMouseY;
    private float _scalingParameter = 1.0f; 

    public SplatWindow() : base(
        GameWindowSettings.Default,
        new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "Gaussian Splatting with OpenTK"
        })
    {
        _camera = new Camera();
        _splats = LoadSplatsFromFile("../../../data/nike.splat");
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        InitializeOpenGl();
        SortSplatsByZ(); // Sort splats after loading
    }

    private void InitializeOpenGl()
    {
        GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f); //  background white
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ProgramPointSize); 

        // Compile shaders
        _shader = new Shader("../../../shaders/shader.vert", "../../../shaders/shader.frag");

        // Upload splat data to GPU
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        float[] splatData = GetSplatData();
        GL.BufferData(BufferTarget.ArrayBuffer, splatData.Length * sizeof(float), splatData, BufferUsageHint.StaticDraw);

        // Define vertex attributes
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 32, 0); // Position
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 32, 24); // Color
        GL.EnableVertexAttribArray(1);
    }

    private float[] GetSplatData()
    {
        var data = new List<float>();
        foreach (var splat in _splats)
        {
            data.Add(splat.Position.X);
            data.Add(splat.Position.Y);
            data.Add(splat.Position.Z);
            // Convert byte to float
            data.Add(splat.Color.R / 255f); 
            data.Add(splat.Color.G / 255f); 
            data.Add(splat.Color.B / 255f); 
            data.Add(splat.Color.A / 255f); 
        }
        return data.ToArray();
    }

    

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        SortSplatsByZ(); // Sort splats before rendering

        _shader.Use();
        _shader.SetMatrix4("viewProjection", _camera.GetViewProjectionMatrix(ClientSize.X / (float)ClientSize.Y));

        // Console.WriteLine("SCALING (OnRenderFrame): " + _scalingParameter);

        _shader.SetFloat("scalingParameter", _scalingParameter);
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(PrimitiveType.Points, 0, _splats.Count);

        SwapBuffers();
    }
    private void SortSplatsByZ()
    {
        _splats.Sort((splat1, splat2) => splat1.Position.Z.CompareTo(splat2.Position.Z));
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        var mouseState = MouseState;
        if (mouseState.IsButtonDown(MouseButton.Left))
        {
            float deltaX = mouseState.X - _lastMouseX;
            float deltaY = mouseState.Y - _lastMouseY;
            _camera.Azimuth += deltaX * 0.01f;
            _camera.Elevation = Math.Clamp(_camera.Elevation - deltaY * 0.01f, -MathF.PI, MathF.PI);
            _camera.UpdateViewMatrix();
        }
        _lastMouseX = mouseState.X;
        _lastMouseY = mouseState.Y;
    }
    

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _camera.FieldOfView -= e.OffsetY * .1f;
        _camera.FieldOfView = Math.Clamp(_camera.FieldOfView, 0.1f, 1f); // MathHelper.Pi, TODO: In case of infinite zoom-in it always enlarges but FOV have limit of 0.1f
         // Console.WriteLine(_camera.FieldOfView);
        // TODO: What does mean:  Update the program to render the splats as ----view-aligned---- squares with side length 2s/z pixels. From task 2?         
        _scalingParameter += e.OffsetY * 1.1f; // TODO: Set good parameter
        
        _scalingParameter = Math.Max(_scalingParameter, 1.0f); // Prevent vanishing 
    }

    private List<Splat> LoadSplatsFromFile(string path)
    {
        var splats = new List<Splat>();
        byte[] data = File.ReadAllBytes(path);

        for (int i = 0; i < data.Length; i += 32)
        {
            Vector3 position = new Vector3(
                BitConverter.ToSingle(data, i),
                BitConverter.ToSingle(data, i + 4),
                BitConverter.ToSingle(data, i + 8)
            );

            Vector3 scale = new Vector3(
                BitConverter.ToSingle(data, i + 12),
                BitConverter.ToSingle(data, i + 16),
                BitConverter.ToSingle(data, i + 20)
            );

            Color4 color = new Color4(
                data[i + 24],
                data[i + 25],
                data[i + 26],
                data[i + 27]
            );

            Quaternion rotation = new Quaternion(
                (data[i + 28] - 128) / 128f,
                (data[i + 29] - 128) / 128f,
                (data[i + 30] - 128) / 128f,
                (data[i + 31] - 128) / 128f
            );

            splats.Add(new Splat { Position = position, Scale = scale, Color = color, Rotation = rotation });
        }
        return splats;
    }
}