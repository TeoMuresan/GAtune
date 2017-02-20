using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

/// http://cfugue.sourceforge.net/docs/html/page_examples.html#secPInvokeUsage

namespace GAtune.MusicNoteLib
{
    public class MusicNoteLib
    {
        [DllImport("MusicNoteDlld.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PlayMusicString(string szMusicNotes);

        [DllImport("MusicNoteDlld.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SaveAsMidiFile([MarshalAs(UnmanagedType.LPStr)] String szMusicNotes, [MarshalAs(UnmanagedType.LPStr)] String szOutputFilePath);
        
        /*[DllImport("MusicNoteDlld.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PlayAsync([MarshalAs(UnmanagedType.LPStr)] String szMusicNotes, 
            Int32 szMIDIOutPortID, UInt32 szMIDITimerResMS);

        [DllImport("MusicNoteDlld.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsPlaying();

        [DllImport("MusicNoteDlld.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopPlay();*/
        
        ///
        /// Modified versions of the CFugue functions PlayAsync, IsPlaying and StopPlay, respectively. 
        ///
        [DllImport("MusicNoteDlld.Dll",  CallingConvention = CallingConvention.Cdecl)]
        public static extern float myPlayAsync(float t, [MarshalAs(UnmanagedType.LPStr)] String szMusicNotes);

        [DllImport("MusicNoteDlld.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool myIsPlaying();

        [DllImport("MusicNoteDlld.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void myStopPlay();
    }
}
