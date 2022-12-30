using Chip8Emulator.WinFormApp.Logic;

namespace Chip8Emulator.WinFormApp.Views;

public partial class Gameplay : Form
{
    private const string GAMES_FOLDER_PATH = "Games/Chip8";
    private static readonly Dictionary<Keys, KeyCode> KEYCODE_MAP = new Dictionary<Keys, KeyCode>()
    {
        { Keys.D1, KeyCode.Key1 }, { Keys.D2, KeyCode.Key2 }, { Keys.D3, KeyCode.Key3 }, { Keys.D4, KeyCode.KeyC },
        { Keys.Q, KeyCode.Key4 }, { Keys.W, KeyCode.Key5 }, { Keys.E, KeyCode.Key6 }, { Keys.R, KeyCode.KeyD },
        { Keys.A, KeyCode.Key7 }, { Keys.S, KeyCode.Key8 }, { Keys.D, KeyCode.Key9 }, { Keys.F, KeyCode.KeyE },
        { Keys.Z, KeyCode.KeyA }, { Keys.X, KeyCode.Key0 }, { Keys.C, KeyCode.KeyB }, { Keys.V, KeyCode.KeyF }
    };

    private Chip8 _chip8;

    public Gameplay()
    {
        InitializeComponent();

        _chip8 = new Chip8()
        {
            Renderer = new Renderer(this),
            Buzzer = new Buzzer()
        };
    }

    private void OnShown(object sender, EventArgs e)
    {
        string romPath = null;
        string initialPath = Path.Combine(Directory.GetCurrentDirectory(), GAMES_FOLDER_PATH);
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.InitialDirectory = Path.GetFullPath(initialPath);

        while (string.IsNullOrEmpty(romPath))
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                romPath = dialog.FileName;
            }
        }

        byte[] rom = File.ReadAllBytes(romPath);
        _chip8.LoadProgram(rom);
        Task.Run(Start);
    }

    private void Start()
    {
        while (true)
        {
            _chip8.Tick();
            Thread.Sleep(1000 / Chip8.FPS / 4);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        KeyCode? keyCode = GetKeyCode(e.KeyCode);

        if (keyCode.HasValue) _chip8.InputKeyDown(keyCode.Value);
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        KeyCode? keyCode = GetKeyCode(e.KeyCode);

        if (keyCode.HasValue) _chip8.InputKeyUp(keyCode.Value);
    }

    private KeyCode? GetKeyCode(Keys key)
    {
        KeyCode? result = null;

        if (KEYCODE_MAP.TryGetValue(key, out KeyCode aux))
        {
            result = aux;
        }

        return result;
    }
}
