using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicNoteLib
{  
        /// <summary>
        /// A MIDI Channel.
        /// </summary>
        public enum Channel : byte
        {
            Channel1 = 0,
            Channel2 = 1,
            Channel3 = 2,
            Channel4 = 3,
            Channel5 = 4,
            Channel6 = 5,
            Channel7 = 6,
            Channel8 = 7,
            Channel9 = 8,
            // Channel 10 is the dedicated percussion channel.
            Channel10 = 9,
            Channel11 = 10,
            Channel12 = 11,
            Channel13 = 12,
            Channel14 = 13,
            Channel15 = 14,
            Channel16 = 15
        };

        public class Channels
        {
        }
}
