using System;
using System.Runtime.InteropServices;

namespace MapAssist.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Inventory
    {
        // Used to see if the current player is the correct player
        // Should not be 0 for local player (expansion character)
        // should not be 1 for local player (non-expansion character)
        [FieldOffset(0x70)] public int pUnk1Exp;
        [FieldOffset(0x30)] public int pUnk1NonExp;
    }
}
