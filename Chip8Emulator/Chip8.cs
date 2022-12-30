using System;

namespace Chip8Emulator;

/// <summary>
/// Documentation:
/// http://devernay.free.fr/hacks/chip8/C8TECH10.HTM
/// </summary>
public class Chip8
{
    public const int FPS = 60;
    private const byte STACK_SIZE = 16;
    private const int RAM_SIZE = 4096;
    private const byte V_SIZE = 16;
    private const short PROGRAM_START = 0x200;
    private const byte SPRITE_LONG = 5;
    private const int DISPLAY_WIDTH = 64;
    private const int DISPLAY_HEIGHT = 32;

    private bool _isBuzzerEnabled;
    private byte[,] _display;

    //Display
    public virtual int DisplayWidth => DISPLAY_WIDTH;
    public virtual int DisplayHeight => DISPLAY_HEIGHT;

    public IRenderer Renderer { get; set; }
    public IBuzzer Buzzer { get; set; }
    private Random Random { get; init; }
    private byte[] Keyboard { get; init; }
    private short ProgramCounter { get; set; }

    // Stack
    private short StackPointer { get; set; }
    private short[] Stack { get; init; }

    // RAM
    private byte[] Ram { get; init; }

    //Registers
    private byte[] V { get; init; }
    private short I { get; set; }

    // Timers
    private byte Delay { get; set; }
    private byte Sound { get; set; }

    public Chip8()
    {
        _isBuzzerEnabled = false;
        _display = new byte[DISPLAY_WIDTH, DISPLAY_HEIGHT];
        Ram = new byte[RAM_SIZE];
        Stack = new short[STACK_SIZE];
        V = new byte[V_SIZE];
        Keyboard = new byte[Enum.GetValues<KeyCode>().Length];
        Random = new Random();

        Ram.Insert(0, GetCharacterSprites());
    }

    private byte[] GetCharacterSprites()
    {
        return new byte[] {
            // 0
            0xF0, 0x90, 0x90, 0x90, 0xF0,
            // 1
            0x20, 0x60, 0x20, 0x20, 0x70,
            // 2
            0xF0, 0x10, 0xF0, 0x80, 0xF0,
            // 3
            0xF0, 0x10, 0xF0, 0x10, 0xF0,
            // 4
            0x90, 0x90, 0xF0, 0x10, 0x10,
            // 5
            0xF0, 0x80, 0xF0, 0x10, 0xF0,
            // 6
            0xF0, 0x80, 0xF0, 0x90, 0xF0,
            // 7
            0xF0, 0x10, 0x20, 0x40, 0x40,
            // 8
            0xF0, 0x90, 0xF0, 0x90, 0xF0,
            // 9
            0xF0, 0x90, 0xF0, 0x10, 0xF0,
            // A
            0xF0, 0x90, 0xF0, 0x90, 0x90,
            // B
            0xE0, 0x90, 0xE0, 0x90, 0xE0,
            // C
            0xF0, 0x80, 0x80, 0x80, 0xF0,
            // D
            0xE0, 0x90, 0x90, 0x90, 0xE0,
            // E
            0xF0, 0x80, 0xF0, 0x80, 0xF0,
            // F
            0xF0, 0x80, 0xF0, 0x80, 0x80
        };
    }

    public void LoadProgram(byte[] program)
    {
        Ram.Insert(PROGRAM_START, program);
        Restart();
    }

    public void Restart()
    {
        for (int i = 0; i < V.Length; i++)
        {
            V[i] = 0;
            Stack[i] = 0;
        }

        I = 0;

        Sound = 0;
        Delay = 0;

        ProgramCounter = PROGRAM_START;
        StackPointer = 0;
        Renderer.SetSize(DisplayWidth, DisplayHeight);
        ClearDisplay();
        Buzzer?.StopPlaySound();        
    }

    public void InputKeyDown(KeyCode keyCode)
    {
        Keyboard[(int)keyCode] = 1;
    }

    public void InputKeyUp(KeyCode keyCode)
    {
        Keyboard[(int)keyCode] = 0;
    }

    public void Tick()
    {
        // Fetch instruction
        byte up = Ram[ProgramCounter];
        byte down = Ram[ProgramCounter + 1];
        short opCode = (short)(up << 8 | down);
        ExecuteInstruction(opCode);

        // Advance program counter
        ProgramCounter += 2;
        if (ProgramCounter >= Ram.Length) ProgramCounter = PROGRAM_START;

        if (Delay > 0) Delay--;
        if (Sound > 0)
        {
            if (!_isBuzzerEnabled)
            {
                _isBuzzerEnabled = true;
                Buzzer?.StartPlaySound();
            }

            Sound--;
        }
        else if (_isBuzzerEnabled)
        {
            _isBuzzerEnabled = false;
            Buzzer?.StopPlaySound();
        }
    }

    private void ExecuteInstruction(short opCode)
    {
        // Decode instruction
        byte codeIndex = (byte)((opCode & 0xF000) >> 12);

        switch (codeIndex)
        {
            case 0:
                ExecuteOpCode0(opCode);
                break;

            case 1:
                ExecuteOpCode1(opCode);
                break;

            case 2:
                ExecuteOpCode2(opCode);
                break;

            case 3:
                ExecuteOpCode3(opCode);
                break;

            case 4:
                ExecuteOpCode4(opCode);
                break;

            case 5:
                ExecuteOpCode5(opCode);
                break;

            case 6:
                ExecuteOpCode6(opCode);
                break;

            case 7:
                ExecuteOpCode7(opCode);
                break;

            case 8:
                ExecuteOpCode8(opCode);
                break;

            case 9:
                ExecuteOpCode9(opCode);
                break;

            case 0xA:
                ExecuteOpCodeA(opCode);
                break;

            case 0xB:
                ExecuteOpCodeB(opCode);
                break;

            case 0xC:
                ExecuteOpCodeC(opCode);
                break;

            case 0xD:
                ExecuteOpCodeD(opCode);
                break;

            case 0xE:
                ExecuteOpCodeE(opCode);
                break;

            case 0xF:
                ExecuteOpCodeF(opCode);
                break;
        }
    }

    protected virtual void ExecuteOpCode0(short opCode)
    {
        switch (opCode)
        {
            case 0x00E0:
                // 00E0 - CLS
                // Clear the display.
                ClearDisplay();
                break;

            case 0x00EE:
                // 00EE - RET
                // Return from a subroutine.
                // The interpreter sets the program counter to the address at the top of the stack,
                // then subtracts 1 from the stack pointer.
                ProgramCounter = Stack[StackPointer];
                StackPointer--;
                break;

            default:
                // 0nnn - SYS addr
                // Jump to a machine code routine at nnn.
                // This instruction is only used on the old computers on which Chip - 8 was originally
                // implemented. It is ignored by modern interpreters.
                ExecuteOpCode1(opCode);
                break;
        }
    }

    private void ExecuteOpCode1(short opCode)
    {
        // 1nnn - JP addr
        // Jump to location nnn.
        // The interpreter sets the program counter to nnn.
        short nnn = GetNNN(opCode);
        Jump(nnn);
    }

    private void ExecuteOpCode2(short opCode)
    {
        // 2nnn - CALL addr
        // Call subroutine at nnn.
        // The interpreter increments the stack pointer,
        StackPointer++;

        // then puts the current PC on the top of the stack. The PC is then set to nnn.
        Stack[StackPointer] = ProgramCounter;

        ExecuteOpCode1(opCode);
    }

    private void ExecuteOpCode3(short opCode)
    {
        // 3xkk - SE Vx, byte
        // Skip next instruction if Vx = kk.
        // The interpreter compares register Vx to kk, and if they are equal,
        // increments the program counter by 2.
        (int x, int kk) = GetXKK(opCode);

        if (V[x] == kk) ProgramCounter += 2;
    }

    private void ExecuteOpCode4(short opCode)
    {
        // 4xkk - SNE Vx, byte
        // Skip next instruction if Vx != kk.
        // The interpreter compares register Vx to kk, and if they are not equal,
        // increments the program counter by 2.
        (int x, int kk) = GetXKK(opCode);

        if (V[x] != kk) ProgramCounter += 2;
    }

    private void ExecuteOpCode5(short opCode)
    {
        // 5xy0 - SE Vx, Vy
        // Skip next instruction if Vx = Vy.
        // The interpreter compares register Vx to register Vy, and if they are equal,
        // increments the program counter by 2.
        (int x, int y) = GetXY(opCode);

        if (V[x] == V[y]) ProgramCounter += 2;
    }

    private void ExecuteOpCode6(short opCode)
    {
        // 6xkk - LD Vx, byte
        // Set Vx = kk.
        // The interpreter puts the value kk into register Vx.
        (int x, int kk) = GetXKK(opCode);

        V[x] = (byte)kk;
    }

    private void ExecuteOpCode7(short opCode)
    {
        // 7xkk - ADD Vx, byte
        // Set Vx = Vx + kk.
        // Adds the value kk to the value of register Vx, then stores the result in Vx.

        (int x, int kk) = GetXKK(opCode);

        V[x] += (byte)kk;
    }

    private void ExecuteOpCode8(short opCode)
    {
        int opCode8Index = opCode & 0xF;
        (int x, int y) = GetXY(opCode);

        switch (opCode8Index)
        {
            case 0:
                ExecuteOpCode80(x, y);
                break;

            case 1:
                ExecuteOpCode81(x, y);
                break;

            case 2:
                ExecuteOpCode82(x, y);
                break;

            case 3:
                ExecuteOpCode83(x, y);
                break;

            case 4:
                ExecuteOpCode84(x, y);
                break;

            case 5:
                ExecuteOpCode85(x, y);
                break;

            case 6:
                ExecuteOpCode86(x);
                break;

            case 7:
                ExecuteOpCode87(x, y);
                break;

            case 0xE:
                ExecuteOpCode8E(x);
                break;
        }
    }

    private void ExecuteOpCode80(int x, int y)
    {
        // 8xy0 - LD Vx, Vy
        // Set Vx = Vy.
        // Stores the value of register Vy in register Vx.

        V[x] = V[y];
    }

    private void ExecuteOpCode81(int x, int y)
    {
        // 8xy1 - OR Vx, Vy
        // Set Vx = Vx OR Vy.
        // Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx.
        // A bitwise OR compares the corrseponding bits from two values,
        // and if either bit is 1, then the same bit in the result is also 1. Otherwise, it is 0.

        V[x] = (byte)(V[x] | V[y]);
    }

    private void ExecuteOpCode82(int x, int y)
    {
        // 8xy2 - AND Vx, Vy
        // Set Vx = Vx AND Vy.
        // Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
        // A bitwise AND compares the corrseponding bits from two values, and if both bits are 1,
        // then the same bit in the result is also 1. Otherwise, it is 0.

        V[x] = (byte)(V[x] & V[y]);
    }

    private void ExecuteOpCode83(int x, int y)
    {
        // 8xy3 - XOR Vx, Vy
        // Set Vx = Vx XOR Vy.
        // Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx.
        // An exclusive OR compares the corrseponding bits from two values,
        // and if the bits are not both the same, then the corresponding bit in the result is set to 1. Otherwise, it is 0.

        V[x] = (byte)(V[x] ^ V[y]);
    }

    private void ExecuteOpCode84(int x, int y)
    {
        // 8xy4 - ADD Vx, Vy
        // Set Vx = Vx + Vy, set VF = carry.
        // The values of Vx and Vy are added together. If the result is greater than 8 bits (i.e., > 255,)
        // VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored in Vx.

        int sum = V[x] + V[y];

        V[0xF] = (byte)((sum > 0xFF) ? 1 : 0);

        V[x] = (byte)(sum & 0xFF);
    }

    private void ExecuteOpCode85(int x, int y)
    {
        // 8xy5 - SUB Vx, Vy
        // Set Vx = Vx - Vy, set VF = NOT borrow.
        // If Vx > Vy, then VF is set to 1, otherwise 0.
        // Then Vy is subtracted from Vx, and the results stored in Vx.

        V[0xF] = (byte)(V[x] > V[y] ? 1 : 0);

        V[x] = (byte)((V[x] - V[y]) & 0xFF);
    }

    private void ExecuteOpCode86(int x)
    {
        // 8xy6 - SHR Vx {, Vy}
        // Set Vx = Vx SHR 1.
        // If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0.
        // Then Vx is divided by 2.

        V[0xF] = (byte)(V[x] & 0x1);

        V[x] = (byte)(V[x] >> 1); // Equals to divide by 2
    }

    private void ExecuteOpCode87(int x, int y)
    {
        // 8xy7 - SUBN Vx, Vy
        // Set Vx = Vy - Vx, set VF = NOT borrow.
        // If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy,
        // and the results stored in Vx.

        V[0xF] = (byte)(V[y] > V[x] ? 1 : 0);

        V[x] = (byte)((V[y] - V[x]) & 0xFF);
    }

    private void ExecuteOpCode8E(int x)
    {
        // 8xyE - SHL Vx {, Vy}
        // Set Vx = Vx SHL 1.
        // If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0.
        // Then Vx is multiplied by 2.

        V[0xF] = (byte)((V[x] >> 7) & 0x1);

        V[x] = (byte)(V[x] << 1); // Equals to multiply by 2
    }

    private void ExecuteOpCode9(short opCode)
    {
        // 9xy0 - SNE Vx, Vy
        // Skip next instruction if Vx != Vy.
        // The values of Vx and Vy are compared, and if they are not equal,
        // the program counter is increased by 2.

        (int x, int y) = GetXY(opCode);

        if (V[x] != V[y]) ProgramCounter += 2;
    }

    private void ExecuteOpCodeA(short opCode)
    {
        // Annn - LD I, addr
        // Set I = nnn.
        // The value of register I is set to nnn.

        short nnn = GetNNN(opCode);
        I = nnn;
    }

    private void ExecuteOpCodeB(short opCode)
    {
        // Bnnn - JP V0, addr
        // Jump to location nnn + V0.
        // The program counter is set to nnn plus the value of V0.

        short nnn = GetNNN(opCode);
        byte v0 = V[0];

        Jump((short)(nnn + v0));
    }

    private void ExecuteOpCodeC(short opCode)
    {
        // Cxkk - RND Vx, byte
        // Set Vx = random byte AND kk.
        // The interpreter generates a random number from 0 to 255,
        // which is then ANDed with the value kk. The results are stored in Vx.
        // See instruction 8xy2 for more information on AND.

        (int x, int kk) = GetXKK(opCode);

        int random = Random.Next(256);

        V[x] = (byte)(random & kk);
    }

    protected virtual void ExecuteOpCodeD(short opCode)
    {
        // Dxyn - DRW Vx, Vy, nibble
        // Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
        // The interpreter reads n bytes from memory, starting at the address stored in I.
        // These bytes are then displayed as sprites on screen at coordinates (Vx, Vy).
        // Sprites are XORed onto the existing screen. If this causes any pixels to be erased,
        // VF is set to 1, otherwise it is set to 0. If the sprite is positioned so part of it is outside
        // the coordinates of the display, it wraps around to the opposite side of the screen.
        // See instruction 8xy3 for more information on XOR.

        (int x, int y, int n) = GetXYN(opCode);

        byte[] spriteBytes = new byte[n];

        for (int i = 0; i < n; i++)
        {
            spriteBytes[i] = Ram[I + i];
        }

        int collision = 0;

        for (int i = 0; i < spriteBytes.Length; i++)
        {
            int xPos = V[x];
            int yPos = V[y] + i;

            if (xPos < 0) xPos += _display.GetLength(0);
            else if(xPos >= _display.GetLength(0)) xPos -= _display.GetLength(0);

            if (yPos < 0) yPos += _display.GetLength(1);
            else if (yPos >= _display.GetLength(1)) yPos -= _display.GetLength(1);

            collision |= DrawSprite(spriteBytes[i], xPos, yPos);
        }

        V[0xF] = (byte)collision;
    }

    private void ExecuteOpCodeE(short opCode)
    {
        int subECode = opCode & 0xFF;
        int x = GetX(opCode);

        switch (subECode)
        {
            case 0x9E:
                ExecuteOpCodeE9E(x);
                break;
            case 0xA1:
                ExecuteOpCodeEA1(x);
                break;
        }
    }

    private void ExecuteOpCodeE9E(int x)
    {
        // Ex9E - SKP Vx
        // Skip next instruction if key with the value of Vx is pressed.
        // Checks the keyboard, and if the key corresponding to the value of Vx is currently
        // in the down position, PC is increased by 2.

        if (Keyboard[V[x]] != 0) ProgramCounter += 2;
    }

    private void ExecuteOpCodeEA1(int x)
    {
        // ExA1 - SKNP Vx
        // Skip next instruction if key with the value of Vx is not pressed.
        // Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position,
        // PC is increased by 2.

        if (Keyboard[V[x]] == 0) ProgramCounter += 2;
    }

    protected virtual void ExecuteOpCodeF(short opCode)
    {
        int subFCode = opCode & 0xFF;
        int x = GetX(opCode);

        switch (subFCode)
        {
            case 0x07:
                ExecuteOpCodeF07(x);
                break;

            case 0x0A:
                ExecuteOpCodeF0A(x);
                break;

            case 0x15:
                ExecuteOpCodeF15(x);
                break;

            case 0x18:
                ExecuteOpCodeF18(x);
                break;

            case 0x1E:
                ExecuteOpCodeF1E(x);
                break;

            case 0x29:
                ExecuteOpCodeF29(x);
                break;

            case 0x33:
                ExecuteOpCodeF33(x);
                break;

            case 0x55:
                ExecuteOpCodeF55(x);
                break;

            case 0x65:
                ExecuteOpCodeF65(x);
                break;
        }
    }

    private void ExecuteOpCodeF07(int x)
    {
        // Fx07 - LD Vx, DT
        // Set Vx = delay timer value.
        // The value of DT is placed into Vx.

        V[x] = Delay;
    }

    private void ExecuteOpCodeF0A(int x)
    {
        // Fx0A - LD Vx, K
        // Wait for a key press, store the value of the key in Vx.
        // All execution stops until a key is pressed, then the value of that key is stored in Vx.

        bool keyPressed = false;
        byte count = 0;

        while (!keyPressed && count < Keyboard.Length)
        {
            bool isPressed = Keyboard[count] != 0;

            if (isPressed)
            {
                V[x] = count;
                keyPressed = true;
            }

            count++;
        }

        if (!keyPressed) ProgramCounter -= 2;
    }

    private void ExecuteOpCodeF15(int x)
    {
        // Fx15 - LD DT, Vx
        // Set delay timer = Vx.
        // DT is set equal to the value of Vx.

        Delay = V[x];
    }

    private void ExecuteOpCodeF18(int x)
    {
        // Fx18 - LD ST, Vx
        // Set sound timer = Vx.
        // ST is set equal to the value of Vx.

        Sound = V[x];
    }

    private void ExecuteOpCodeF1E(int x)
    {
        // Fx1E - ADD I, Vx
        // Set I = I + Vx.
        // The values of I and Vx are added, and the results are stored in I.

        I += V[x];
    }

    private void ExecuteOpCodeF29(int x)
    {
        // Fx29 - LD F, Vx
        // Set I = location of sprite for digit Vx.
        // The value of I is set to the location for the hexadecimal sprite corresponding to the value of Vx.

        I = (short)(V[x] * SPRITE_LONG);
    }

    private void ExecuteOpCodeF33(int x)
    {
        // Fx33 - LD B, Vx
        // Store BCD representation of Vx in memory locations I, I+1, and I+2.
        // The interpreter takes the decimal value of Vx, and places the hundreds digit in memory
        // at location in I, the tens digit at location I+1, and the ones digit at location I+2.

        int num = V[x];

        int hundreds = num / 100;
        num -= hundreds * 100;
        int tens = num / 10;
        num -= tens * 10;
        int ones = num;

        Ram[I + 0] = (byte)hundreds;
        Ram[I + 1] = (byte)tens;
        Ram[I + 2] = (byte)ones;
    }

    private void ExecuteOpCodeF55(int x)
    {
        // Fx55 - LD [I], Vx
        // Store registers V0 through Vx in memory starting at location I.
        // The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.

        for (int i = 0; i <= x; i++)
        {
            Ram[I + i] = V[i];
        }
    }

    private void ExecuteOpCodeF65(int x)
    {
        // Fx65 - LD Vx, [I]
        // Read registers V0 through Vx from memory starting at location I.
        // The interpreter reads values from memory starting at location I into registers V0 through Vx.

        for (int i = 0; i <= x; i++)
        {
            V[i] = Ram[I + i];
        }
    }

    #region Utils

    private void Jump(short programCounter)
    {
        ProgramCounter = programCounter;

        // NOTE: after every instruction, the program counter is increased, so
        //  this is to avoid problems.
        ProgramCounter -= 2;
    }

    protected short GetNNN(short opCode)
    {
        return (short)(opCode & 0xFFF);
    }

    protected (int, int) GetXKK(short opCode)
    {
        int x = (opCode & 0xF00) >> 8;
        int kk = opCode & 0xFF;

        return (x, kk);
    }

    protected int GetX(short opCode)
    {
        return (opCode & 0xF00) >> 8;
    }

    protected (int, int) GetXY(short opCode)
    {
        int x = GetX(opCode);
        int y = (opCode & 0xF0) >> 4;

        return (x, y);
    }

    protected (int, int, int) GetXYN(short opCode)
    {
        (int x, int y) = GetXY(opCode);
        int n = opCode & 0xF;

        return (x, y, n);
    }

    private void ClearDisplay()
    {
        Renderer?.Clear();

        for (int x = 0; x < _display.GetLength(0); x++)
            for (int y = 0; y < _display.GetLength(1); y++)
                _display[x, y] = 0;
    }


    private int DrawSprite(byte sprite, int x, int y)
    {
        int hasSomeCollision = 0;

        for (int i = 0; i < 8; i++)
        {
            if ((sprite & (0x80 >> i)) != 0)
            {
                int newX = (x + i) % _display.GetLength(0);
                int collisionData = _display[newX, y];

                if (collisionData == 0)
                {
                    _display[newX, y] = 1;
                    Renderer?.DrawSprite(newX, y);
                }
                else
                {
                    hasSomeCollision = 1;
                    _display[newX, y] = 0;
                    Renderer?.EraseSprite(newX, y); //Remove sprite
                }
            }
        }

        return hasSomeCollision;
    }

    #endregion
}
