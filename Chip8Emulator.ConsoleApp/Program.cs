namespace Chip8Emulator.ConsoleApp;

public class Program
{
    private const string GAMES_FOLDER = "Games";
    private static readonly Dictionary<ConsoleKey, KeyCode> KEYCODE_MAP = new Dictionary<ConsoleKey, KeyCode>()
    {
        { ConsoleKey.D1, KeyCode.Key1 }, { ConsoleKey.D2, KeyCode.Key2 }, { ConsoleKey.D3, KeyCode.Key3 }, { ConsoleKey.D4, KeyCode.KeyC },
        { ConsoleKey.Q, KeyCode.Key4 }, { ConsoleKey.W, KeyCode.Key5 }, { ConsoleKey.E, KeyCode.Key6 }, { ConsoleKey.R, KeyCode.KeyD },
        { ConsoleKey.A, KeyCode.Key7 }, { ConsoleKey.S, KeyCode.Key8 }, { ConsoleKey.D, KeyCode.Key9 }, { ConsoleKey.F, KeyCode.KeyE },
        { ConsoleKey.Z, KeyCode.KeyA }, { ConsoleKey.X, KeyCode.Key0 }, { ConsoleKey.C, KeyCode.KeyB }, { ConsoleKey.V, KeyCode.KeyF }
    };

    private static List<KeyCode> _ConsoleKeyPressed = new List<KeyCode>();

    static void Main(string[] args)
    {
        StartGame("Games/PONG");
    }

    private static void ChooseGame()
    {
        Directory.GetFiles(GAMES_FOLDER);
    }

    private static void StartGame(string gamePath)
    {
        Chip8 chip8 = new Chip8()
        {
            Renderer = new ConsoleRenderer(),
            Buzzer = new ConsoleBuzzer(),
        };

        chip8.LoadProgram(File.ReadAllBytes(gamePath));

        while (true)
        {
            ManageKeyboard(chip8);

            chip8.Tick();
            Thread.Sleep(1000 / Chip8.FPS / 5);
        }
    }

    private static void ManageKeyboard(Chip8 chip8)
    {
        while (Console.KeyAvailable)
        {
            ConsoleKey consoleKey = Console.ReadKey(true).Key;

            if (KEYCODE_MAP.TryGetValue(consoleKey, out KeyCode key))
            {
                if (_ConsoleKeyPressed.Contains(key))
                {
                    _ConsoleKeyPressed.Remove(key);
                    chip8.InputKeyUp(key);
                }
                else
                {
                    _ConsoleKeyPressed.Add(key);
                    chip8.InputKeyDown(key);
                }
            }
        }
    }
}