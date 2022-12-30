using System.Diagnostics;

namespace Chip8Emulator.Test;

public class Program
{
    private const string TEST_FILENAME = "Resources/test.ch8";

    static void Main(string[] args)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Chip8 chip8 = new Chip8() 
        { 
            Buzzer = new ConsoleBuzzer(),
            Renderer = new ConsoleRenderer()
        };

        chip8.LoadProgram(File.ReadAllBytes(TEST_FILENAME));

        Console.WriteLine(stopwatch.Elapsed.ToString());

        while (true)
        {
            chip8.Tick();
            Thread.Sleep(1000 / Chip8.FPS);
        }
    }
}