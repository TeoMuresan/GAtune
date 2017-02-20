using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicNoteLib
{
    /// <summary>
    /// Represents a common list of key signatures.
    /// </summary>
    public enum KeySignature : byte
    {
        CbMaj = 0,
        GbMaj = 1,
        DbMaj = 2,
        AbMaj = 3,
        EbMaj = 4,
        BbMaj = 5,
        FMaj = 6,
        CMaj = 7,
        GMaj = 8,
        DMaj = 9,
        AMaj = 10,
        EMaj = 11,
        BMaj = 12,
        FsMaj = 13,
        CsMaj = 14
    };

    /// <summary>
    /// Specifies the number of accidentals for each key signature.
    /// The positive numbers represent the number of sharps.
    /// The negative numbers represent the number of flats.
    /// E.g. Cb has 7 b, C has no accidentals and G has 1 #. 
    /// </summary>
    public enum KeySignatureAccidental : int
    {
        Cb = -7,
        Gb = -6,
        Db = -5,
        Ab = -4,
        Eb = -3,
        Bb = -2,
        F = -1,
        C = 0,
        G = 1,
        D = 2,
        A = 3,
        E = 4,
        B = 5,
        Fs = 6,
        Cs = 7
    };

    /// <summary>
    /// A key signature is a series of sharp or flat symbols placed on the staff, designating
    /// notes that are to be consistently played one semitone higher or lower than the equivalent
    /// natural notes.
    /// </summary>
    public class KeySignatures
    {
        public static string GetKeyFromKeySignature(KeySignature ks)
        {
            string key = ks.ToString();
            key = key.Replace("Maj", string.Empty);
            return key;
        }
    }
}
