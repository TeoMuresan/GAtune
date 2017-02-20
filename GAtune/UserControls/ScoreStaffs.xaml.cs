using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GAlib;
using MusicNoteLib;
using GAtune.Controllers;

namespace GAtune.UserControls
{
    /// <summary>
    /// Interaction logic for ScoreStaffs.xaml.
    /// </summary>
    public partial class ScoreStaffs : UserControl
    {
        public const double thisWidth = 368;
        public const double thisHeight = 170;
        public static double unitElementWidth = thisWidth / Parameters.unitBeat;
        public double leftMargin = 2;
        // The space between 2 succesive elements to be placed on the staff.
        private double layoutInterval;
        public const int defaultNumberElem = 14;
        public bool clicked = false;
        public Point startPoint;        

        public ScoreStaffs()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Extracts the accidental from the specified note for each part.       
        /// </summary>
        /// <param name="notePart0"></param>
        /// <returns>  If the note is clean, returns an empty string.</returns>
        private string[] GetAccidentals(string notePart0, string notePart1)
        {
            string[] acc = { string.Empty, string.Empty };

            if (notePart0.Length == 2)
                acc[0] = notePart0[1].ToString();

            if (notePart1.Length == 2)
                acc[1] = notePart1[1].ToString();

            return acc;
        }

        /// <summary>
        /// Extracts the actual octave given the specified note string for each part.
        /// </summary>
        /// <param name="noteWordPart0"> "[actual_octave]|[note]|[final_duration]" for part 1.</param>
        /// <param name="noteWordPart1"></param>
        /// <returns></returns>
        private Octave[] GetActualOctaves(string noteWordPart0, string noteWordPart1)
        {
            Octave[] actOctave = new Octave[2];
            int firstSeparatorIndex;
            string actOctString;

            firstSeparatorIndex = noteWordPart0.IndexOf('|');
            actOctString = noteWordPart0.Substring(0, firstSeparatorIndex);
            actOctave[0] = LayoutController.StringToEnum<Octave>(actOctString);

            firstSeparatorIndex = noteWordPart1.IndexOf('|');
            actOctString = noteWordPart1.Substring(0, firstSeparatorIndex);
            actOctave[1] = LayoutController.StringToEnum<Octave>(actOctString);

            return actOctave;
        }        

        /// <summary>
        /// Extracts the actual octave given the specified note string for each part.
        /// </summary>
        /// <param name="noteWordPart0"> "[actual_octave]|[note]|[final_duration]" for part 1.</param>
        /// <param name="noteWordPart1"></param>
        /// <returns></returns>
        private string[] GetNotes(string noteWordPart0, string noteWordPart1)
        {
            string[] note = new string[2];
            int firstSeparatorIndex, lastSeparatorIndex;            

            firstSeparatorIndex = noteWordPart0.IndexOf('|');
            lastSeparatorIndex = noteWordPart0.LastIndexOf('|');
            note[0] = noteWordPart0.Substring(firstSeparatorIndex + 1, lastSeparatorIndex - firstSeparatorIndex - 1);

            firstSeparatorIndex = noteWordPart1.IndexOf('|');
            lastSeparatorIndex = noteWordPart1.LastIndexOf('|');
            note[1] = noteWordPart1.Substring(firstSeparatorIndex + 1, lastSeparatorIndex - firstSeparatorIndex - 1);

            return note;
        }

        /// <summary>
        /// Extracts the clean note (i.e. without accidentals) given the specified note and actual octave for each part.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="actualOctave"></param>
        /// <returns></returns>
        private Note[] GetCleanNotes(string[] note, Octave[] actualOctave)
        {
            Note[] cleanNote = new Note[2];

            if (note[0].StartsWith("B") && actualOctave[0] == Parameters.Octaves[0] + 1)
                cleanNote[0] = Note.C;
            else
                cleanNote[0] = LayoutController.StringToEnum<Note>(note[0][0].ToString());

            if (note[1].StartsWith("B") && actualOctave[1] == Parameters.Octaves[1] + 1)
                cleanNote[1] = Note.C;
            else
                cleanNote[1] = LayoutController.StringToEnum<Note>(note[1][0].ToString());

            return cleanNote;
        }

        /// <summary>
        /// Extracts the note orientation given the specified clean note and actual octave for each part.
        /// </summary>
        /// <param name="cleanNote"></param>
        /// <param name="actualOctave"></param>
        /// <returns></returns>
        private char[] GetOrientations(Note[] cleanNote, Octave[] actualOctave)
        {
            char[] orientation = new char[2];

            orientation[0] = LayoutController.GetNoteOrientation(cleanNote[0], actualOctave[0]);
            orientation[1] = LayoutController.GetNoteOrientation(cleanNote[1], actualOctave[1]);

            return orientation;
        }

        /// <summary>
        /// Extracts the CFugue (as data type) duration given the specified CFugue duration string for each note in tie.
        /// </summary>
        /// <param name="cFugueDuration"></param>
        /// <returns></returns>
        private DurationCFugue[] GetCFugueDurationsInTie(string cFugueDurationNote1, string cFugueDurationNote2)
        {
            DurationCFugue[] cFugueDurationResult = new DurationCFugue[2];

            cFugueDurationResult[0] = LayoutController.StringToEnum<DurationCFugue>(cFugueDurationNote1);
            cFugueDurationResult[1] = LayoutController.StringToEnum<DurationCFugue>(cFugueDurationNote2);

            return cFugueDurationResult;
        }

        /// <summary>
        /// Detects the presence of a dot for each note in tie.
        /// </summary>
        /// <param name="cFugueDuration"> The duration of the first note in tie, followed by the duration of the last note in tie.</param>
        /// <param name="durLastNoteInTieIndex"> The position in cFugueDuration of the duration of the last note in tie.</param>
        /// <returns></returns>
        private bool[] GetDotsInTie(string cFugueDuration, out byte durLastNoteInTieIndex)
        {
            bool[] dot = new bool[2];
            int dotCount = cFugueDuration.Length - cFugueDuration.Replace(".", "").Length;

            if (dotCount == 0)
            {
                dot[0] = dot[1] = false;
                durLastNoteInTieIndex = 1;
            }
            else if (dotCount == 1)
            {
                if (cFugueDuration[1] == '.')
                {
                    dot[0] = true;
                    dot[1] = false;
                    durLastNoteInTieIndex = 2;
                }
                else
                {
                    dot[0] = false;
                    dot[1] = true;
                    durLastNoteInTieIndex = 1;
                }
            }
            else
            {
                dot[0] = dot[1] = true;
                durLastNoteInTieIndex = 2;
            }

            return dot;
        }

        /// <summary>
        /// Detects the presence of a dot given the specified CFugue duration of one note.
        /// </summary>
        /// <param name="cFugueDuration"></param>
        /// <returns></returns>
        private bool GetDot(ref string cFugueDuration)
        {
            bool dot = false;
            if (cFugueDuration.Contains('.'))
            {
                dot = true;
                cFugueDuration = cFugueDuration.Remove(1);
            }
            return dot;
        }

        private void DrawNoteForEachPart(string noteWordPart0, string noteWordPart1, bool hasDot, string cFugueFinalDuration)
        {
            Octave[] actualOctave = GetActualOctaves(noteWordPart0, noteWordPart1);
            string[] note = GetNotes(noteWordPart0, noteWordPart1);
            // A clean note is a note without accidentals.
            Note[] cleanNote = GetCleanNotes(note, actualOctave);                   
            char[] orientation = GetOrientations(cleanNote, actualOctave);            
            string[] hasAcc = GetAccidentals(note[0], note[1]);
            DurationCFugue cFugueDuration = LayoutController.StringToEnum<DurationCFugue>(cFugueFinalDuration);

            // First part score.                                 
            DrawNote(cleanNote[0], cFugueDuration, hasDot, actualOctave, orientation[0], hasAcc[0], false, 0);
            // Second part score.          
            DrawNote(cleanNote[1], cFugueDuration, hasDot, actualOctave, orientation[1], hasAcc[1], false, 1);                        
        }

        private void DrawTiedNotesForEachPart(string noteWordPart0, string noteWordPart1, bool[] hasDot, DurationCFugue[] cFugueDuration)
        {
            Octave[] actualOctave = GetActualOctaves(noteWordPart0, noteWordPart1);
            string[] note = GetNotes(noteWordPart0, noteWordPart1);
            // A clean note is a note without accidentals.
            Note[] cleanNote = GetCleanNotes(note, actualOctave);
            char[] orientation = GetOrientations(cleanNote, actualOctave);
            string[] hasAcc = GetAccidentals(note[0], note[1]);

            // First note in the tie.
            DrawNote(cleanNote[0], cFugueDuration[0], hasDot[0], actualOctave, orientation[0], hasAcc[0], false, 0);
            DrawNote(cleanNote[1], cFugueDuration[0], hasDot[0], actualOctave, orientation[1], hasAcc[1], false, 1);

            leftMargin += layoutInterval + unitElementWidth;

            // Last note in the tie.
            DrawNote(cleanNote[0], cFugueDuration[1], hasDot[1], actualOctave, orientation[0], hasAcc[0], true, 0);
            DrawNote(cleanNote[1], cFugueDuration[1], hasDot[1], actualOctave, orientation[1], hasAcc[1], true, 1);
        }

        private void DrawRestForEachPart(string cFugueDuration, bool hasDot)
        {
            DurationCFugue cFugueFinalDur = LayoutController.StringToEnum<DurationCFugue>(cFugueDuration);
            // First part score.
            DrawRest(cFugueFinalDur, hasDot, 0);
            // Second part score.
            DrawRest(cFugueFinalDur, hasDot, 1);
        }

        private void DrawTiedRestsForEachPart(DurationCFugue[] cFugueDuration, bool[] hasDot)
        {
            // First rest in the tie.
            DrawRest(cFugueDuration[0], hasDot[0], 0);
            DrawRest(cFugueDuration[0], hasDot[0], 1);
            leftMargin += layoutInterval + unitElementWidth;
            // Second rest in the tie.
            DrawRest(cFugueDuration[1], hasDot[1], 0);
            DrawRest(cFugueDuration[1], hasDot[1], 1);
        }

        /// <summary>
        /// Fills the score staff template of THIS individual with the corresponding genome information.
        /// </summary>
        /// <param name="genome"></param>
        public void setContent(Genome genome)
        {
            try
            {                                              
                // The final duration (e.g. q. is 1/4+(1/4)/2 or 6*1/16):
                // wrt unitBeat (e.g. unitBeat=16: q. is 6);
                byte unitBeatFinalDur;
                // in CFugue notation (e.g. q.).
                string cFugueFinalDur;                
                byte durLastNoteInTieIndex;
                double diff;
                layoutInterval = 0;
                string currentWordPart0, currentWordPart1;

                string[] wordsPart0 = genome.Parts[2].Split(' ');
                string[] wordsPart1 = genome.Parts[3].Split(' ');                

                for (int j = 0; j < wordsPart0.Length - 1; j++)
                {
                    currentWordPart0 = wordsPart0[j];
                    currentWordPart1 = wordsPart1[j];
                    unitBeatFinalDur = Convert.ToByte(currentWordPart0.Substring(currentWordPart0.LastIndexOf('|') + 1));
                    cFugueFinalDur = Durations.FinalDuration(Parameters.durationIndex, unitBeatFinalDur);                                     

                    // If there is one element to place on the score.
                    if (cFugueFinalDur.Length == 1 || (cFugueFinalDur.Length == 2 && cFugueFinalDur.Contains('.')))
                    {
                        diff = ((unitBeatFinalDur - 1) * unitElementWidth) / 2;
                        leftMargin += diff;

                        bool dot = GetDot(ref cFugueFinalDur);                        

                        // If it's a rest.
                        if (currentWordPart0[0] == 'R')
                        {
                            DrawRestForEachPart(cFugueFinalDur, dot);                            
                        }
                        // If it's a note.
                        else
                        {
                            DrawNoteForEachPart(currentWordPart0, currentWordPart1, dot, cFugueFinalDur);                                                        
                        }

                        leftMargin += unitElementWidth + diff + 1;
                    }

                    // If there are two elements (they're gonna be connected by a tie) to place on the score.
                    else
                    {
                        double[] interval = GetLayoutIntervals(unitBeatFinalDur);                                                
                        diff = interval[0];
                        layoutInterval = interval[1];
                        leftMargin += diff;
                        
                        bool[] dot = GetDotsInTie(cFugueFinalDur, out durLastNoteInTieIndex);                        
                        DurationCFugue[] cFugueFinalDuration = GetCFugueDurationsInTie(cFugueFinalDur[0].ToString(), cFugueFinalDur[durLastNoteInTieIndex].ToString());                        

                        // If the two elements are rests.
                        if (currentWordPart0[0] == 'R')
                        {
                            DrawTiedRestsForEachPart(cFugueFinalDuration, dot);                            
                        }
                        // If the two elements are notes.
                        else
                        {
                            DrawTiedNotesForEachPart(currentWordPart0, currentWordPart1, dot, cFugueFinalDuration);                            
                        }

                        leftMargin += unitElementWidth + diff + 1;
                    }
                }

                // Reset leftMargin to default value when done.
                leftMargin = 2;
            }
            catch (Exception)
            {
            }
        }

        private void DrawNote(Note note, DurationCFugue duration, bool dot, Octave[] actOctave, char orientation, 
            string hasAcc, bool hasTie, byte partIndex)
        {
            double height = LayoutController.ElementHeight["N" + duration];
            Octave actualOctave = actOctave[partIndex];
            double notePos = LayoutController.GetNotePos(note, actualOctave, Parameters.Clefs[partIndex], partIndex);
            double top = LayoutController.GetNoteTopMargin(notePos, height, orientation, partIndex);
            double noteLeftMargin = leftMargin;

            if (hasAcc != "")
            {
                DrawAccidental(notePos, hasAcc, orientation, partIndex);
                noteLeftMargin += LayoutController.accidentalOffset;
            }
            if (hasTie)
            {               
                double notePosOffset = notePos + LayoutController.GetTieOffset(note, actualOctave, partIndex);
                DrawTie(layoutInterval  + 2 * unitElementWidth, noteLeftMargin - layoutInterval - unitElementWidth , notePosOffset, orientation, partIndex);
            }

            Image image = new Image();            
            image.HorizontalAlignment = HorizontalAlignment.Left;
            image.Source = LayoutController.GetNote(duration, dot, orientation);
            image.Height = height;
            image.Margin = new Thickness(noteLeftMargin, top, 0, 0);

            staffGrid.Children.Add(image);
            Grid.SetRow(image, partIndex);

            string has = LayoutController.HasLine(note, actualOctave);
            if (has != "")
            {
                DrawExtraLedgerLines(notePos, note, actualOctave, duration, noteLeftMargin, Convert.ToInt32(has), partIndex);
            }
        }

        private void DrawLine(double x1, double x2, double y, byte partIndex)
        {
            Line l = new Line();
            l.Style = (Style)staffGrid.Resources["lineStyle"];
            l.X1 = x1;
            l.X2 = x2;
            l.Y1 = l.Y2 = y;
            staffGrid.Children.Add(l);
            Grid.SetRow(l, partIndex);
        }

        /// <summary>
        /// Draws extra ledger line(s) when the specified note falls above/below the staff.
        /// </summary>
        /// <param name="notePos"></param>
        /// <param name="note"></param>
        /// <param name="octave"></param>
        /// <param name="duration"></param>
        /// <param name="noteLeft"></param>
        /// <param name="partIndex"></param>
        private void DrawExtraLedgerLines(double notePos, Note note, Octave octave, DurationCFugue duration,
            double noteLeftMargin,int verticalOffset, byte partIndex)
        {
            double x1 = noteLeftMargin - 4;
            double x2 = noteLeftMargin + LayoutController.ElementWidth["N" + duration];
            double y = notePos + verticalOffset * LayoutController.stepLedgerLine / 2;

            DrawLine(x1, x2, y, partIndex);            

            Clef clef = Parameters.Clefs[partIndex];

            // Now add the extra lines.
            if (octave == Octave.Octave3 && note == Note.C)
            {
                y = LayoutController.GetNotePos(Note.E, Octave.Octave3, clef, partIndex);
                DrawLine(x1, x2, y, partIndex);               
            }
            else if (octave == Octave.Octave7)
            {
                if (note == Note.C || note == Note.D)
                {
                    y = LayoutController.GetNotePos(Note.A, Octave.Octave6, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);                   
                }
                else if (note == Note.E || note == Note.F)
                {
                    y = LayoutController.GetNotePos(Note.A, Octave.Octave6, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);
                    y = LayoutController.GetNotePos(Note.C, Octave.Octave7, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);                    
                }
                else if (note == Note.G || note == Note.A)
                {
                    y = LayoutController.GetNotePos(Note.A, Octave.Octave6, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);
                    y = LayoutController.GetNotePos(Note.C, Octave.Octave7, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);
                    y = LayoutController.GetNotePos(Note.E, Octave.Octave7, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);                                       
                }
                else
                {
                    y = LayoutController.GetNotePos(Note.A, Octave.Octave6, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);
                    y = LayoutController.GetNotePos(Note.C, Octave.Octave7, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);
                    y = LayoutController.GetNotePos(Note.E, Octave.Octave7, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex);
                    y = LayoutController.GetNotePos(Note.G, Octave.Octave7, clef, partIndex);
                    DrawLine(x1, x2, y, partIndex); 
                }
            }
        }

        private void DrawAccidental (double notePos, string hasAcc, char orientation, byte partIndex)
        {
            Image imageAcc = new Image();            
            imageAcc.HorizontalAlignment = HorizontalAlignment.Left;
            imageAcc.Source = LayoutController.GetElement("accidental", hasAcc);
            imageAcc.Height = LayoutController.ElementHeight["A" + hasAcc];
            double topAcc = LayoutController.GetAccidentalTopMargin(notePos, imageAcc.Height, orientation, partIndex);

            imageAcc.Margin = new Thickness(leftMargin, topAcc, 0, 0);

            staffGrid.Children.Add(imageAcc);
            Grid.SetRow(imageAcc, partIndex);
        }

        private void DrawRest(DurationCFugue duration, bool dot, byte partIndex)
        {
            Image image = new Image();            
            image.HorizontalAlignment = HorizontalAlignment.Left;
            image.Source = LayoutController.GetRest(duration, dot);
            image.Height=LayoutController.ElementHeight["R" + duration];
            double top = LayoutController.GetRestTopMargin(image.Height, duration, partIndex);
            image.Margin = new Thickness(leftMargin, top, 0, 0);

            staffGrid.Children.Add(image);
            Grid.SetRow(image, partIndex);
        }

        private void DrawTie(double width, double left, double notePos, char orientation, byte partIndex)
        {
            Image image = new Image();
            image.Width = width;
            double height = LayoutController.ElementHeight["T"];
            image.Height = height;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            double top = LayoutController.GetTieTopMargin(notePos, height, orientation, partIndex);
            image.Margin = new Thickness(left, top, 0, 0);

            DrawingVisual drawingVisual = new DrawingVisual();
            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            // Concave arc.
            if(orientation == '-')
                LayoutController.DrawArc(drawingContext, null, new Pen(Brushes.Black, 2),
                    new Point(0, height / 6), new Point(width, height / 6), new Size(width, height));
            // Convex arc.
            else
                LayoutController.DrawArc(drawingContext, null, new Pen(Brushes.Black, 2),
                     new Point(width, 0), new Point(0, 0), new Size(width, height));
            // Persist the drawing content.
            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)width + 50, (int)height, 120, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            image.Source = bmp;

            staffGrid.Children.Add(image);
            Grid.SetRow(image, partIndex);
        }

        /// <summary>
        /// Adds a tag which specifies the index of the current score staff within the SCORE window.
        /// </summary>
        /// <param name="barIndex"></param>
        public void AddIndexTag(int barIndex)
        {
            Label l = new Label();
            l.Content = barIndex;
            l.Style=(Style)staffGrid.Resources["indexTag"];
            staffGrid.Children.Add(l);
        }

        public void AddTempoInfo(string tempo)
        {
            Image i = new Image();
            Label l = new Label();
            i.Style = (Style)staffGrid.Resources["metronomeImage"];
            l.Style = (Style)staffGrid.Resources["tempoLabel"];
            l.Content = tempo;
            staffGrid.Children.Add(i);
            staffGrid.Children.Add(l);
        }

        /// <summary>
        /// Computes width intervals to arrange two notes over a specific finalDur*unitElementWidth interval.
        /// </summary>
        /// <param name="finalDur"></param>
        /// <returns></returns>
        private double[] GetLayoutIntervals(byte finalDur)
        {
            double[] intervals = new double[2];
            byte remainder = (byte)((finalDur - 2) % 3);
            byte quotient = (byte)((finalDur - 2) / 3);
            if (remainder == 0)
                intervals[0] = intervals[1] = quotient;
            else
            {
                intervals[0] = quotient + 0.5;
                intervals[1] = quotient;
                if (remainder == 2)
                    intervals[1]++;
            }
            intervals[0] *= unitElementWidth;
            intervals[1] *= unitElementWidth;

            return intervals;
        }        
    }
}
