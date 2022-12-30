using System.Collections.Generic;

namespace Chip8Emulator;

internal static class Extensions
{
    internal static void Insert<T>(this T[] source, int startIndex, IList<T> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            source[startIndex + i] = items[i];
        }
    }
}
