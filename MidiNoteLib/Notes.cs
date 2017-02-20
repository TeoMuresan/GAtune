using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicNoteLib
{
    /// <summary>
    /// Represents MIDI note labels.
    /// </summary>
    /// <remarks> The values are used to compute the MIDI code of the label when combined with an octave and/or accidental.</remarks>
    public enum Note : byte
    {
        C = 0,
        D = 1,
        E = 2,
        F = 3,
        G = 4,
        A = 5,
        B = 6,
    }

    class Notes
    {
    }
}
