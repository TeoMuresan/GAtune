using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicNoteLib
{
    /// <summary>
    /// Contains, for each key signature, the encodings of the 12 notes for its corresponding scale.
    /// Also contains, for each key signature, the position of the natural C which is one octave above.    
    /// A standard octave is considered to start on C and end on B.
    /// </summary>
    public class Scales
    {
        private static readonly string[] CbMajScale = { "C", "Cn", "D", "Dn", "E", "F", "Fn", "G", "Gn", "A", "An", "B" };
        private static readonly string[] GbMajScale = { "G", "Gn", "A", "An", "B", "C", "Cn", "D", "Dn", "E", "En", "F" };
        private static readonly string[] DbMajScale = { "D", "Dn", "E", "En", "F", "G", "Gn", "A", "An", "B", "Bn", "C" };
        private static readonly string[] AbMajScale = { "A", "An", "B", "Bn", "C", "D", "Dn", "E", "En", "F", "F#", "G" };
        private static readonly string[] EbMajScale = { "E", "En", "F", "F#", "G", "A", "An", "B", "Bn", "C", "C#", "D" };
        private static readonly string[] BbMajScale = { "B", "Bn", "C", "C#", "D", "E", "En", "F", "F#", "G", "G#", "A" };
        private static readonly string[] FMajScale = { "F", "F#", "G", "G#", "A", "B", "Bn", "C", "C#", "D", "D#", "E" };
        private static readonly string[] CMajScale = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private static readonly string[] GMajScale = { "G", "G#", "A", "A#", "B", "C", "C#", "D", "D#", "E", "E#", "F" };
        private static readonly string[] DMajScale = { "D", "D#", "E", "E#", "F", "G", "G#", "A", "A#", "B", "B#", "C" };
        private static readonly string[] AMajScale = { "A", "A#", "B", "B#", "C", "D", "D#", "E", "E#", "F", "F#", "G" };
        private static readonly string[] EMajScale = { "E", "E#", "F", "F#", "G", "A", "A#", "B", "B#", "C", "C#", "D" };
        private static readonly string[] BMajScale = { "B", "B#", "C", "C#", "D", "E", "E#", "F", "F#", "G", "G#", "A" };
        private static readonly string[] FsMajScale = { "F", "F#", "G", "G#", "A", "B", "B#", "C", "C#", "D", "D#", "E" };
        private static readonly string[] CsMajScale = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        /// <summary>
        /// The position of the natural C one octave above.
        /// E.g. GbMaj in octave 5: C is Cb and Cn is natural C; the position of natural C in octave 6 is 6;
        ///      FMaj in octave 5: C is natural C; the position of natural C in octave 6 is 7;
        ///      DMaj in octave 3: C is C# and B# is natural C; the position of natural C in octave 4 is 10.
        /// </summary>
        public static readonly byte[] changeOctave = { 1, 6, 11, 4, 9, 2, 7, 12, 5, 10, 3, 8, 1, 6, 11 };

        public static readonly List<string[]> scales = new List<string[]>()
            {
                CbMajScale,
                GbMajScale,
                DbMajScale,
                AbMajScale,
                EbMajScale,
                BbMajScale,
                FMajScale,
                CMajScale,
                GMajScale,
                DMajScale,
                AMajScale,
                EMajScale,
                BMajScale,
                FsMajScale,
                CsMajScale
            };

        /// <summary>
        /// Returns the MIDI value of the first pitch in the scale.
        /// </summary>
        /// <param name="octave"></param>
        /// <param name="scaleIndex"></param>
        /// <returns></returns>
        public static byte FirstPitch(byte octave, byte scaleIndex)
        {
            return (byte)(12 * (octave + 1) - changeOctave[scaleIndex]);
        }

        /// <summary>
        /// Returns the appropriate clef, based on octave.
        /// </summary>
        /// <param name="octave"></param>
        /// <returns></returns>
        public static Clef GetClef(Octave octave)
        {
            Clef clef;
            if ((byte)octave <= (byte)Octave.Octave4)
                clef = Clef.bass;
            else
                clef = Clef.treble;
            return clef;
        }
    }
}
