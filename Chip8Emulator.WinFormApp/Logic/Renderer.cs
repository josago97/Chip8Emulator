namespace Chip8Emulator.WinFormApp.Logic;

internal class Renderer : IRenderer
{
    private static readonly Color BACKGROUND_COLOR = Color.Black;
    private static readonly Color FOREGROUND_COLOR = Color.White;
    private const int PIXEL_SIZE = 20;

    private Control _controlUI;
    private Graphics _graphics;
    private Brush _backgroundBrush;
    private Brush _foregroundBrush;

    public Renderer(Control controlUI)
    {
        _controlUI = controlUI;

        _backgroundBrush = new SolidBrush(BACKGROUND_COLOR);
        _foregroundBrush = new SolidBrush(FOREGROUND_COLOR);
    }

    public void SetSize(int width, int height)
    {
        _controlUI.ClientSize = new Size(width * PIXEL_SIZE, height * PIXEL_SIZE);
        _graphics?.Dispose();
        _graphics = _controlUI.CreateGraphics();
    }

    public void Clear()
    {
        _graphics.Clear(BACKGROUND_COLOR);
    }

    public void DrawSprite(int x, int y)
    {
        Draw(_foregroundBrush, x, y);
    }

    public void EraseSprite(int x, int y)
    {
        Draw(_backgroundBrush, x, y);
    }

    private void Draw(Brush brush, int x, int y)
    {
        Rectangle rectangle = new Rectangle(x * PIXEL_SIZE, y * PIXEL_SIZE, PIXEL_SIZE, PIXEL_SIZE);
        _graphics.FillRectangle(brush, rectangle);
    }
}
