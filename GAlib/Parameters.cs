using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicNoteLib;

namespace GAlib
{
    public static class Parameters
    {
        private static byte tempo = 120;
        public static byte Tempo
        {
            get
            {
                return tempo;
            }
            set
            {
                if (value < 20)
                {
                    tempo = 20;
                }
                else if (value > 180)
                {
                    tempo = 180;
                }
                else
                {
                    tempo = value;
                }
            }
        }

        internal const byte infVelocity = 40;
        internal const byte supVelocity = 120;
        internal const byte stepVelocity = 5;
        internal const byte maxNoOfStepsVelocity = (supVelocity - infVelocity) / stepVelocity;

        internal const byte supNotesInterval = 12;
        internal const byte infMidiNote = 35;
        internal const byte supMidiNote = 96;

        private const byte popSize = 9;
        public static byte PopSize
        {
            get
            {
                return popSize;
            }
            set
            {               
            }
        }

        // Number of parts (aka solos) per bar.
        private const byte numParts = 2;
        public static byte NumParts
        {
            get
            {
                return numParts;
            }
            set
            {
            }
        }

        // The following arrays have 2 elements because numParts is set to 2.
        
        public static Clef[] Clefs = { Clef.treble, Clef.treble };

        public static byte[] octavesByteValues = { (byte)Octave.Octave5, (byte)Octave.Octave5 };
        private static Octave[] octaves = { Octave.Octave5, Octave.Octave5 };
        public static Octave[] Octaves
        {
            get
            {
                return octaves;
            }
            set
            {
                octaves = value;
            }           
        }

        internal static KeySignature keySignature = KeySignature.CMaj;
        public static byte keySignatureIndex = (byte)KeySignature.CMaj;
        public static string keySignatureAccidental = KeySignatureAccidental.C.ToString();
        public static KeySignature KeySignature
        {
            get
            {
                return keySignature;
            }
            set
            {
                keySignature = value;
                keySignatureIndex = (byte)keySignature;
                int index = keySignatureIndex;
                if (0 <= keySignatureIndex && keySignatureIndex <= 6)
                    index += 8;
                else
                    index -= 7;
                keySignatureAccidental = Enum.GetValues(typeof(KeySignatureAccidental)).Cast<KeySignatureAccidental>().ElementAt(index).ToString();
            }
        }

        internal static byte[] channels = { (byte)Channel.Channel1, (byte)Channel.Channel2 };        

        internal static DurationCFugue duration = MusicNoteLib.DurationCFugue.s;
        public static byte durationIndex = 0;
        // This variable specifies the number of genes per chromosome.
        // Music-wise, it specifies what fraction of a bar's duration we assign to a note/rest.
        // In other words, it specifies the value of the shortest duration in a bar and is used to interpret a chromosome's genes.  
        public static byte unitBeat = 16;
        public const byte maxUnitBeat = 16;

        // Array to hold the values selected by the user in the dropdowns for the lock parameter.
        public static byte[] lockChrom = { 0, 0 };
        // Array to hold the values selected by the user in the dropdowns for the bar iteration parameter.
        public static byte[] barIterate = { 1, 1 };
        public static byte[] genotypeIterate = { (byte)(unitBeat / barIterate[0]), (byte)(unitBeat / barIterate[1]) };
        public static byte maxIterate = Math.Max(genotypeIterate[0], genotypeIterate[1]);
        public static byte minIndex = 1;

        public static DurationCFugue Duration
        {
            get
            {                
                return duration;
            }
            set
            {
                duration = value;

                durationIndex = (byte)duration;
                switch (duration)
                {
                    case DurationCFugue.s:
                        unitBeat = 16;
                        break;
                    case DurationCFugue.i:
                        unitBeat = 8;
                        break;
                    case DurationCFugue.q:
                        unitBeat = 4;
                        break;
                    default:
                        unitBeat = 16;
                        break;
                }

                genotypeIterate[0] = (byte)(unitBeat / barIterate[0]);
                genotypeIterate[1] = (byte)(unitBeat / barIterate[1]);
                if (genotypeIterate[0] >= genotypeIterate[1])
                {
                    maxIterate = genotypeIterate[0];
                    minIndex = 1;
                }
                else
                {
                    maxIterate = genotypeIterate[1];
                    minIndex = 0;
                }
            }
        }

        /// <summary>
        /// Gets the unit beat based on the specified duration.
        /// </summary>
        /// <param name="cFugueDurationString"> "s", "i" or "q".</param>
        /// <returns></returns>
        public static byte GetBeat(string cFugueDurationString)
        {
            byte beat;
            switch (cFugueDurationString)
            {
                case "s":
                    beat = 16;
                    break;
                case "i":
                    beat = 8;
                    break;
                case "q":
                    beat = 4;
                    break;
                default:
                    beat = 16;
                    break;
            }
            return beat;
        }
    }
}

