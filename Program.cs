namespace GaussianSplatRenderer;

class Program
{
    public static void Main(string[] args)
    {
        using var window = new SplatWindow();
        window.Run();
    }
}