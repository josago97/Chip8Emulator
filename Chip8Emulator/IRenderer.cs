namespace Chip8Emulator;

public interface IRenderer
{
    void SetSize(int width, int height);
    void Clear();
    void DrawSprite(int x, int y);
    void EraseSprite(int x, int y);
}
