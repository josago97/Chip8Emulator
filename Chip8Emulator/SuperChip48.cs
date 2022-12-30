using System;

namespace Chip8Emulator;

/// <summary>
/// Documentation:
/// http://johnearnest.github.io/Octo/docs/SuperChip.html
/// </summary>
internal class SuperChip48 : Chip8
{
    public event Action Exited;


    protected override void ExecuteOpCode0(short opCode)
    {
        short opCode0Index = (short)(opCode >> 4);

        if (opCode0Index == 0x00C)
        {
            int n = GetN(opCode);
        }
        else
        {
            switch (opCode0Index)
            {
                case 0x00FB:
                    break;

                case 0x00FC:
                    break;

                case 0x00FD:
                    Exited?.Invoke();
                    break;

                case 0x00FE:
                    break;

                case 0x00FF:
                    break;

                default:
                    base.ExecuteOpCode0(opCode);
                    break;
            }
        }
    }

    protected override void ExecuteOpCodeD(short opCode)
    {
        short opCodeDIndex = (short)(opCode & 0xF);

        if (opCodeDIndex == 0)
        {

        }
        else
        {
            base.ExecuteOpCodeD(opCode);
        }
    }

    protected override void ExecuteOpCodeF(short opCode)
    {
        short opCodeFIndex = (short)(opCode & 0xFF);

        base.ExecuteOpCodeF(opCode);
    }

    #region Utils

    protected int GetN(short opCode)
    {
        return opCode & 0xF;
    }

    #endregion
}
