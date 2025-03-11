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
    private Matrix4 _cachedViewProjection;

    
    // For FPS calculation
    private int _frameCount;
    private double _elapsedTime;
    private int _fps;

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
        CenterObject(_splats); // Center the object
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        InitializeOpenGl();
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }
    private void CenterObject(List<Splat> splats) // Centering the object
    {
        // Calculate the centroid of the object
        Vector3 centroid = Vector3.Zero;
        foreach (var splat in splats)
        {
            centroid += splat.Position;
        }
        centroid /= splats.Count;

        // Translate each vertex to center the object
        for (int i = 0; i < splats.Count; i++)
        {
            splats[i].Position -= centroid;
        }
    }

    private void InitializeOpenGl()
    {
        GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f); // background white
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ProgramPointSize); 

        _shader = new Shader("../../../shaders/shader.vert", "../../../shaders/shader.frag");

        // Upload splat data to GPU
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _splats.Count * 32, IntPtr.Zero, BufferUsageHint.StaticDraw);

        for (int i = 0; i < _splats.Count; i++)
        {
            var splat = _splats[i];
            // Position data (12 bytes)
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(i * 32), 12, 
                new float[] { splat.Position.X, splat.Position.Y, splat.Position.Z });
        
            // Color data (4 bytes) - original byte values
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(i * 32 + 24), 4, 
                new byte[] { splat.Color.R, splat.Color.G, splat.Color.B, splat.Color.A });
        }

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
    
        // Position attribute
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 32, 0);
        GL.EnableVertexAttribArray(0);
    
        // Color attribute - normalized bytes
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 32, 24);
        GL.EnableVertexAttribArray(1);
    }

    

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
    
        // FPS calculation
        _frameCount++;
        _elapsedTime += args.Time;
        if (_elapsedTime >= 1.0) // Update FPS every second
        {
            _fps = _frameCount;
            _frameCount = 0;
            _elapsedTime = 0;
            Console.WriteLine($"FPS: {_fps}");
        }

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Sort splats by screen-space depth
        SortSplatsByDepth();

        _shader.Use();
        _shader.SetMatrix4("viewProjection", _camera.GetViewProjectionMatrix(ClientSize.X / (float)ClientSize.Y));
        _shader.SetFloat("scalingParameter", _scalingParameter);
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(PrimitiveType.Points, 0, _splats.Count);

        SwapBuffers();
    }


private void SortSplatsByDepth()
{
    if (_cachedViewProjection != _camera.GetViewProjectionMatrix(ClientSize.X / (float)ClientSize.Y))
    {
        _cachedViewProjection = _camera.GetViewProjectionMatrix(ClientSize.X / (float)ClientSize.Y);
    }

    // Use the cached view-projection matrix
    Matrix4 viewProjection = _cachedViewProjection;

    // Rest of the code remains the same
    var depthArray = new (int Index, float Depth)[_splats.Count];

    Parallel.For(0, _splats.Count, i =>
    {
        Vector4 pos = viewProjection * new Vector4(_splats[i].Position, 1.0f);
        depthArray[i] = (i, pos.Z);
    });

    Array.Sort(depthArray, (a, b) => b.Depth.CompareTo(a.Depth));

    var tempList = _splats.ToList();
    for (int i = 0; i < depthArray.Length; i++)
    {
        _splats[i] = tempList[depthArray[i].Index];
    }
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
        
       // Console.WriteLine(_camera.Radius);
        _scalingParameter = 20.0f / _camera.Radius; // Random scaling factor

        //Console.WriteLine($"Camera Position: {_camera.Position.X}, {_camera.Position.Y}, {_camera.Position.Z}");

    }
    

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        
        _camera.Radius -= e.OffsetY * 0.5f; // Zoom level
        _camera.Radius = Math.Clamp(_camera.Radius, 0.1f, 100.0f); // Randomly selected values for min and max

        // Update the camera position based on the new radius
        _camera.UpdateViewMatrix();
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

            ColorByte color = new ColorByte(
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
