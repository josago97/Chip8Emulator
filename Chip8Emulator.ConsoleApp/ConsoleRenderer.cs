namespace Chip8Emulator.ConsoleApp;

class ConsoleRenderer : IRenderer
{
    private const string SPRITE = "**";
    private const string ERASE_SPRITE = "  ";

    public void SetSize(int width, int height)
    {
        Console.SetWindowSize(width * SPRITE.Length, height);
    }

    public void Clear()
    {
        Console.Clear();
    }

    public void DrawSprite(int x, int y)
    {
        Print(SPRITE, x, y);
    }

    public void EraseSprite(int x, int y)
    {
        Print(ERASE_SPRITE, x, y);
    }

    private void Print(string input, int x, int y)
    {
        Console.SetCursorPosition(x * input.Length, y);
        Console.Write(input);
    }
}
