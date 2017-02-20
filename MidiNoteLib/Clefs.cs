using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicNoteLib
{
    /// <summary>
    /// Represents a musical symbol used to indicate the pitch of written notes.
    /// </summary>
    public enum Clef
    {
        /// <summary>
        /// When placed at the beginning of a section, the lines on the staff correspond bottom-up 
        /// to E5, G5, B5, D6, and F6 respectively.
        /// </summary>
        treble,
        /// <summary>
        /// When placed at the beginning of a section, the lines on the staff correspond bottom-up
        /// to G3, B3, D4, F4, and A4 respectively.
        /// </summary>
        bass,
        /// <summary>
        /// When placed at the beginning of a section, the lines on the staff correspond bottom-up
        /// to F3, A3, C4, E4, and G4 respectively.
        /// </summary>
        Alto,
        /// <summary>
        /// When placed at the beginning of a section, the lines and spaces of the staff are
        /// each assigned to a percussion instrument with no precise pitch.
        /// </summary>
        Neutral,
        /// <summary>
        /// Used for transposing instruments.
        /// </summary>
        /// <remarks>
        /// This clef is slightly undocumented.
        /// </remarks>
        Octave
    }

   class Clefs
    {

    }
}
