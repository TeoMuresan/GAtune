using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicNoteLib
{
    /// <summary>
    /// An octave as implemented in CFugue (i.e. starting from 0 and not -1, which makes C5 the middle C).
    /// </summary>
    public enum Octave : byte
    {
        Octave0 = 0,
        Octave1 = 1,
        Octave2 = 2,
        Octave3 = 3,
        Octave4 = 4,
        Octave5 = 5,
        Octave6 = 6,
        Octave7 = 7,
        Octave8 = 8,
        Octave9 = 9,
        Octave10 = 10
    }

    public class Octaves
    {
    }
}
