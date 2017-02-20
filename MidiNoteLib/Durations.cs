using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicNoteLib
{        
    public enum DurationCFugue : byte
    {
        s = 0,
        i = 1,
        q = 2,
        h = 3,
        w = 4
    };

    /// <summary>
    /// This class supports only 1/16, 1/8 and 1/4 unit beats.
    /// </summary>
    public class Durations
    {
        /// <summary>
        /// The durations for each beat.
        /// </summary>
        private static readonly string[] sixteenth = {"s", "i", "i.", "q", "ii.", "q.", "qi.", "h", "hs", "hi", 
                                             "hi.", "h.", "h.s", "h.i", "h.i.", "w"};
        private static readonly string[] eighth = { "i", "q", "q.", "h", "hi", "h.", "h.i", "w" };
        private static readonly string[] quarter = { "q", "h", "h.", "w" };

        public static readonly List<string[]> durations = new List<string[]>()
            {
                sixteenth,
                eighth,
                quarter
            };
       
        /// <summary>
        /// Returns a string representing the compound duration of a rest/note.
        /// </summary>
        /// <param name="durationIndex"></param>
        /// <param name="addDur"></param>
        /// <returns></returns>
        public static string FinalDuration(byte durationIndex, byte addDur)
        {
            return durations[durationIndex][addDur - 1];
        }        
    }
}
