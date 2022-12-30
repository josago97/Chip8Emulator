namespace Chip8Emulator.Test;

public class ConsoleRenderer : IRenderer
{

    public void SetSize(int width, int height)
    {
        Console.SetWindowSize(width, height);
    }

    public void Clear()
    {
        Console.Clear();
    }

    public void DrawSprite(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        Console.Write('*');
    }

    public void EraseSprite(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        Console.Write(' ');
    }
}
