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
using MusicNoteLib;
using GAlib;
using System.Windows.Shapes;
using GAtune.UserControls;

namespace GAtune.Controllers
{
    public static class LayoutController
    {
        // The elements needed to display a note.
        internal struct NoteVisual
        {
            internal DurationCFugue Duration
            {
                get;
                set;
            }
            // A musical dot (used to extend the duration of the note).
            internal bool Dot
            {
                get;
                set;
            }
            // The note orientation: '+' for normal note image (stem up), '-' for lower note image (stem down).
            internal char Orientation
            {
                get;
                set;
            }
            // The Uri of a note image.
            internal Uri Output
            {
                get;
                set;
            }
        }

        // List of elements necessary for each note to be displayed.
        internal static List<NoteVisual> NoteVisuals = new List<NoteVisual>()
        {
            new NoteVisual()
            {
                Duration = DurationCFugue.s,
                Dot = false,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Note/16th-note.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.i,
                Dot = false,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Note/8th-note.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.q,
                Dot = false,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Note/4th-note.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.h,
                Dot = false,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Note/half-note.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.w,
                Dot = false,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Note/whole-note.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.s,
                Dot = false,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Note/lower-16th-note.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.i,
                Dot = false,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Note/lower-8th-note.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.q,
                Dot = false,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Note/lower-4th-note.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.h,
                Dot = false,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Note/lower-half-note.png", UriKind.RelativeOrAbsolute)
            },
              new NoteVisual()
            {
                Duration = DurationCFugue.w,
                Dot = false,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/whole-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.s,
                Dot = true,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/16th-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.i,
                Dot = true,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/8th-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.q,
                Dot = true,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/4th-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.h,
                Dot = true,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/half-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.w,
                Dot = true,
                Orientation = '+',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/whole-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.s,
                Dot = true,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/lower-16th-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.i,
                Dot = true,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/lower-8th-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.q,
                Dot = true,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/lower-4th-note-dotted.png", UriKind.RelativeOrAbsolute)
            },
            new NoteVisual()
            {
                Duration = DurationCFugue.h,
                Dot = true,
                Orientation = '-',
                Output = new Uri(@"/Resources/Score symbols/Dotted note/lower-half-note-dotted.png", UriKind.RelativeOrAbsolute)
            }          
        };        

        /// <summary>
        /// Returns an image resource that is used to represent a specific note duration.
        /// </summary>
        /// <param name="Duration"></param>
        /// <returns></returns>
        public static BitmapImage GetNote(DurationCFugue Duration, bool Dot, char Orientation)
        {
            var result = from imageSource in NoteVisuals
                         where (imageSource.Duration == Duration) && (imageSource.Dot == Dot) && 
                         (imageSource.Orientation == Orientation)
                         select imageSource.Output;
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = result.First<Uri>();
            bmp.EndInit();
            return bmp;
        }

        // The elements needed to display a rest.
        internal struct RestVisual
        {
            internal DurationCFugue Duration
            {
                get;
                set;
            }
            // A musical dot (used to extend the duration of the rest).
            internal bool Dot
            {
                get;
                set;
            }
            // The Uri of a rest image.
            internal Uri Output
            {
                get;
                set;
            }
        }

        // List of elements necessary for each rest to be displayed.
        internal static List<RestVisual> RestVisuals = new List<RestVisual>()
        {
             new RestVisual()
            {
                Duration = DurationCFugue.s,
                Dot = false,
                Output = new Uri(@"/Resources/Score symbols/Rest/13px-16th_rest.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.i,
                Dot = false,
                Output = new Uri(@"/Resources/Score symbols/Rest/10px-8th_rest.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.q,
                Dot = false,
                Output = new Uri(@"/Resources/Score symbols/Rest/8px-4th_plain_rest.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.h,
                Dot = false,
                Output = new Uri(@"/Resources/Score symbols/Rest/30px-half_rest.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.w,
                Dot = false,
                Output = new Uri(@"/Resources/Score symbols/Rest/30px-whole_rest.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.s,
                Dot = true,
                Output = new Uri(@"/Resources/Score symbols/Dotted rest/13px-16th_rest-dotted.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.i,
                Dot = true,
                Output = new Uri(@"/Resources/Score symbols/Dotted rest/10px-8th_rest-dotted.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.q,
                Dot = true,
                Output = new Uri(@"/Resources/Score symbols/Dotted rest/8px-4th_plain_rest-dotted.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.h,
                Dot = true,
                Output = new Uri(@"/Resources/Score symbols/Dotted rest/30px-half_rest-dotted.png", UriKind.RelativeOrAbsolute)
            },
             new RestVisual()
            {
                Duration = DurationCFugue.w,
                Dot = true,
                Output = new Uri(@"/Resources/Score symbols/Dotted rest/30px-whole_rest-dotted.png", UriKind.RelativeOrAbsolute)
            }
        };

        /// <summary>
        /// Returns an image resource that is used to represent a specific rest duration.
        /// </summary>
        /// <param name="Duration"></param>
        /// <param name="Dot"></param>
        /// <returns></returns>
        public static BitmapImage GetRest(DurationCFugue Duration, bool Dot)
        {
            var result = from imageSource in RestVisuals
                         where (imageSource.Duration == Duration) && (imageSource.Dot == Dot)
                         select imageSource.Output;
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = result.First<Uri>();
            bmp.EndInit();
            return bmp;
        }

        // The elements needed to display a key signature.
        internal struct KeySignatureVisual 
        {
            internal string Name
            {
                get;
                set;
            }
            internal string Clef
            {
                get;
                set;
            }
            // The Uri of a key signature image.
            internal Uri Output
            {
                get;
                set;
            }
        }

        // List of elements necessary for each key signature to be displayed.
        internal static List<KeySignatureVisual> KeySignatureVisuals = new List<KeySignatureVisual>()
        {
         new KeySignatureVisual()
            {
                Name = "Ab",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-A-flat-major_f-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "A",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-A-major_f-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Bb",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-B-flat-major_g-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "B",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-B-major_g-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Cb",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-C-flat-major_a-flat-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {               
                Name = "C",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-C-major_a-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Cs",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-C-sharp-major_a-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Db",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-D-flat-major_b-flat-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {               
                Name = "D",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-D-major_b-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Eb",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-E-flat-major_c-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "E",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-E-major_c-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
             new KeySignatureVisual()
            {                
                Name = "F",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-F-major_d-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Fs",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-F-sharp-major_d-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
             new KeySignatureVisual()
            {               
                Name = "Gb",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-G-flat-major_e-flat-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "G",
                Clef = "treble",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Treble-G-major_e-minor.png", UriKind.RelativeOrAbsolute)
            }, 
            new KeySignatureVisual()
            {
                Name = "Ab",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-A-flat-major_f-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "A",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-A-major_f-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Bb",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-B-flat-major_g-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "B",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-B-major_g-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Cb",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-C-flat-major_a-flat-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {               
                Name = "C",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-C-major_a-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Cs",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-C-sharp-major_a-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Db",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-D-flat-major_b-flat-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {               
                Name = "D",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-D-major_b-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Eb",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-E-flat-major_c-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "E",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-E-major_c-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
             new KeySignatureVisual()
            {                
                Name = "F",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-F-major_d-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "Fs",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-F-sharp-major_d-sharp-minor.png", UriKind.RelativeOrAbsolute)
            },
             new KeySignatureVisual()
            {               
                Name = "Gb",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-G-flat-major_e-flat-minor.png", UriKind.RelativeOrAbsolute)
            },
            new KeySignatureVisual()
            {                
                Name = "G",
                Clef = "bass",
                Output = new Uri(@"/Resources/Score symbols/Key signature/Bass-G-major_e-minor.png", UriKind.RelativeOrAbsolute)
            }
        };

        /// <summary>
        /// Returns an image resource that is used to represent a specific key signature.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Clef"></param>
        /// <returns></returns>
        public static BitmapImage GetKeySignature(string Name, string Clef)
        {
            var result = from imageSource in KeySignatureVisuals
                         where (imageSource.Name == Name) && (imageSource.Clef == Clef)
                         select imageSource.Output;
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = result.First<Uri>();
            bmp.EndInit();
            return bmp;
        }

        // The elements needed to display a musical element.
        internal struct ElementVisual
        {
            // Possible categories: accidental, flat, clef, others.
            internal string Category
            {
                get;
                set;
            }
            internal string Element
            {
                get;
                set;
            }
            // The Uri of a musical element.
            internal Uri Output
            {
                get;
                set;
            }
        }

        // List of elements necessary for each musical element to be displayed.
        internal static List<ElementVisual> ElementVisuals = new List<ElementVisual>()
        {
             new ElementVisual()
            {
                Category="accidental",
                Element = "flat",
                Output = new Uri(@"/Resources/Score symbols/Accidental/100px-flat.png", UriKind.RelativeOrAbsolute)
            },
            new ElementVisual()
            {
                Category="accidental",
                Element = "n",
                Output = new Uri(@"/Resources/Score symbols/Accidental/100px-natural.png", UriKind.RelativeOrAbsolute)
            },
            new ElementVisual()
            {
                Category="accidental",
                Element = "#",
                Output = new Uri(@"/Resources/Score symbols/Accidental/100px-sharp.png", UriKind.RelativeOrAbsolute)
            },
             new ElementVisual()
            {
                Category="clef",
                Element = "bass",
                Output = new Uri(@"/Resources/Score symbols/Clef/bass-clef.png", UriKind.RelativeOrAbsolute)
            },
            new ElementVisual()
            {
                Category="clef",
                Element = "treble",
                Output = new Uri(@"/Resources/Score symbols/Clef/treble-clef.png", UriKind.RelativeOrAbsolute)
            },
            
            new ElementVisual()
            {
                Category="others",
                Element="barline",
                Output = new Uri(@"/Resources/Score symbols/Others/bar-line.png", UriKind.RelativeOrAbsolute)
            },
            new ElementVisual()
            {
                Category="others",
                Element="metronome",
                Output = new Uri(@"/Resources/Score symbols/Others/Music-metronome.png", UriKind.RelativeOrAbsolute)
            }
        };

        /// <summary>
        /// Returns an image resource that is used to represent a specific musical element.
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="Element"></param>
        /// <returns></returns>
        public static BitmapImage GetElement(string Category, string Element)
        {
            var result = from imageSource in ElementVisuals
                         where (imageSource.Category == Category) && (imageSource.Element == Element)
                         select imageSource.Output;
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = result.First<Uri>();
            bmp.EndInit();
            return bmp;
        }

        public static string GetPlayerKeySignatureName(int index)
         {
             switch(index)
             {
                 case (int)KeySignatureAccidental.A:
                     return KeySignatureAccidental.A.ToString();
                 case (int)KeySignatureAccidental.Ab:
                     return KeySignatureAccidental.Ab.ToString();
                 case (int)KeySignatureAccidental.B:
                     return KeySignatureAccidental.B.ToString();
                 case (int)KeySignatureAccidental.Bb:
                     return KeySignatureAccidental.Bb.ToString();
                 case (int)KeySignatureAccidental.C:
                     return KeySignatureAccidental.C.ToString();
                 case (int)KeySignatureAccidental.Cb:
                     return KeySignatureAccidental.Cb.ToString();
                 case (int)KeySignatureAccidental.Cs:
                     return KeySignatureAccidental.Cs.ToString();
                 case (int)KeySignatureAccidental.D:
                     return KeySignatureAccidental.D.ToString();
                 case (int)KeySignatureAccidental.Db:
                     return KeySignatureAccidental.Db.ToString();
                 case (int)KeySignatureAccidental.E:
                     return KeySignatureAccidental.E.ToString();
                 case (int)KeySignatureAccidental.Eb:
                     return KeySignatureAccidental.Eb.ToString();
                 case (int)KeySignatureAccidental.F:
                     return KeySignatureAccidental.F.ToString();
                 case (int)KeySignatureAccidental.Fs:
                     return KeySignatureAccidental.Fs.ToString();
                 case (int)KeySignatureAccidental.G:
                     return KeySignatureAccidental.G.ToString();
                 case (int)KeySignatureAccidental.Gb:
                     return KeySignatureAccidental.Gb.ToString();
             }
             return null;
         }

        public static BitmapImage GetPlayerKeySignatureImage(int index)
        {
            switch (index)
            {
                case (int)KeySignatureAccidental.A:
                    return GetKeySignature(KeySignatureAccidental.A.ToString(), "treble");
                case (int)KeySignatureAccidental.Ab:
                    return GetKeySignature(KeySignatureAccidental.Ab.ToString(), "treble");
                case (int)KeySignatureAccidental.B:
                    return GetKeySignature(KeySignatureAccidental.B.ToString(), "treble");
                case (int)KeySignatureAccidental.Bb:
                    return GetKeySignature(KeySignatureAccidental.Bb.ToString(), "treble");
                case (int)KeySignatureAccidental.C:
                    return GetKeySignature(KeySignatureAccidental.C.ToString(), "treble");
                case (int)KeySignatureAccidental.Cb:
                    return GetKeySignature(KeySignatureAccidental.Cb.ToString(), "treble");
                case (int)KeySignatureAccidental.Cs:
                    return GetKeySignature(KeySignatureAccidental.Cs.ToString(), "treble");
                case (int)KeySignatureAccidental.D:
                    return GetKeySignature(KeySignatureAccidental.D.ToString(), "treble");
                case (int)KeySignatureAccidental.Db:
                    return GetKeySignature(KeySignatureAccidental.Db.ToString(), "treble");
                case (int)KeySignatureAccidental.E:
                    return GetKeySignature(KeySignatureAccidental.E.ToString(), "treble");
                case (int)KeySignatureAccidental.Eb:
                    return GetKeySignature(KeySignatureAccidental.Eb.ToString(), "treble");
                case (int)KeySignatureAccidental.F:
                    return GetKeySignature(KeySignatureAccidental.F.ToString(), "treble");
                case (int)KeySignatureAccidental.Fs:
                    return GetKeySignature(KeySignatureAccidental.Fs.ToString(), "treble");
                case (int)KeySignatureAccidental.G:
                    return GetKeySignature(KeySignatureAccidental.G.ToString(), "treble");
                case (int)KeySignatureAccidental.Gb:
                    return GetKeySignature(KeySignatureAccidental.Gb.ToString(), "treble");
            }
            return null;
        }

        /// <summary>
        /// Gets a bitmap from the specified Uniform Resource Identifier.
        /// </summary>
        /// <param name="uriPath"></param>
        /// <returns></returns>
        public static BitmapImage GetImage(string uriPath)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(uriPath, UriKind.Relative);
            bmp.EndInit();
            return bmp;
        }

        public static void DrawArc(DrawingContext drawingContext, Brush brush, Pen pen, Point start, Point end, Size radius)
        {
            // Setup the geometrical object.
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            geometry.Figures.Add(figure);
            figure.StartPoint = start;

            // Add the arc to the geometrical object.
            figure.Segments.Add(new ArcSegment(end, radius,
                0, false, SweepDirection.Clockwise, true));

            // Draw the arc.
            drawingContext.DrawGeometry(brush, pen, geometry);
        }
              
        public static Dictionary<string, double> ElementWidth = new Dictionary<string, double>()
        {
            {
                "Ns",
                13
            },
            {
                "Ni",
                13
            },
            {
                "Nq",
                13
            },
            {
                "Nh",
                13
            },
            {
                "Nw",
                12
            }
        };
       
        public static Dictionary<string, double> ElementHeight = new Dictionary<string, double>()
        {
            {
                "Ns",
                24.5
            },
            {
                "Ni",
                24.5
            },
            {
                "Nq",
                24.5
            },
            {
                "Nh",
                24.5
            },
            {
                "Nw",
                7
            },
            {
                "Rs",
                17
            },
            {
                "Ri",
                12
            },
            {
                "Rq",
                20
            },
            {
                "Rh",
                42
            },
            {
                "Rw",
                42
            },
            {
                "A#",
                15
            },
            {
                "An",
                15
            },
            {
                "T",
                20
            }
        };
        
        // Contains the top margin for the key signature corresponding to each part of the genome.
        internal static double[] KeySignatureTopMargin = { 22, 104 };
        internal static double[] KeySignatureTopMarginScore = { 21, 23 };

        private static double[] gridRowHeight = { 82, 85 };
        private static int elemPosFactor = 2;
        private static double[] lineTopMargin = { 7, 7 };
        
        public static int stepLedgerLine = 6;
        private static byte staffLineCount = 12;
        public static double accidentalOffset = 10;
        static double scoreOffset = stepLedgerLine * (staffLineCount - 1);

        // Necessary values to compute the rests top margins.
        public static double[] F6 = 
        {
            GetNotePos(Note.F, Octave.Octave6, Clef.treble, 0)  ,
            GetNotePos(Note.F, Octave.Octave6, Clef.treble, 1) ,
        };

        /// <summary>
        /// Returns the standard top value for an element (i.e. the actual value for Margin.Top).
        /// </summary>
        /// <param name="elHeight"></param>
        /// <param name="partIndex"></param>
        /// <returns></returns>
        private static double GetElementTop0(double elHeight, byte partIndex)
        {
            return ((gridRowHeight[partIndex] - elHeight) / 2);
        }

        /// <summary>
        /// Returns a standard top value for the ledger line/space used to compute specific top values.
        /// </summary>
        /// <param name="octave"></param>
        /// <param name="clef"></param>
        /// <param name="partIndex"></param>
        /// <returns></returns>
        private static double GetScoreLinePos(Octave octave, Clef clef, byte partIndex)
        {
            double result = scoreOffset;
            if (clef == Clef.treble)
            {
                result = result - stepLedgerLine;

                if (octave == Octave.Octave5)
                {
                }
                else if (octave == Octave.Octave6)
                    result = result - stepLedgerLine * 7 / 2;
                else if (octave == Octave.Octave7)
                    result = result - stepLedgerLine * 14 / 2;
            }
            else if (clef == Clef.bass)
            {
                if (octave == Octave.Octave3)
                {
                }
                else if (octave == Octave.Octave4)
                    result = result - stepLedgerLine * 7 / 2;
            }

            result += lineTopMargin[partIndex];

            return result;
        }

        /// <summary>
        /// Returns the top margin value for the ledger line/the space corresponding to the given note.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="octave"></param>
        /// <param name="clef"></param>
        /// <param name="partIndex"></param>
        /// <returns></returns>
        public static double GetNotePos(Note note, Octave octave, Clef clef, byte partIndex)
        {
            double result = GetScoreLinePos(octave, clef, partIndex); 
            result -= (byte)note * stepLedgerLine / 2;
            return result;
        }

        private static double GetElementTopMargin(double value, double Height, byte partIndex)
        {
            return (value - GetElementTop0(Height, partIndex)) * elemPosFactor;
        }

        public static double GetNoteTopMargin(double value, double noteHeight, char orientation, byte partIndex)
        {
            double res = GetElementTopMargin(value, noteHeight, partIndex);
            if (orientation == '+')
                res = res + (stepLedgerLine / 2 - noteHeight) * elemPosFactor;
            else
                res = res - (stepLedgerLine / 2) * elemPosFactor;
            return res;
        }

        public static double GetAccidentalTopMargin(double value, double accHeight, char orientation, byte partIndex)
        {
            double res = GetElementTopMargin(value, accHeight, partIndex);
            res = res - stepLedgerLine * elemPosFactor;
           
            return res;
        }

        public static double GetTieTopMargin(double value, double tieHeight, char orientation, byte partIndex)
        {
            double res = GetElementTopMargin(value, tieHeight, partIndex);

            if (orientation == '+')
                res = res + stepLedgerLine / 2;
            else
                res = res - tieHeight;

            return res;
        }

        public static double GetTieOffset(Note note, Octave octave, byte partIndex)
        {
            if (octave == Octave.Octave3 && (note == Note.C || note == Note.D))
                return -stepLedgerLine / (partIndex + 1);
            else
                return 0;
        }

        public static double GetRestTopMargin(double restHeight, DurationCFugue duration, byte partIndex)
        {
            double res = GetElementTopMargin(F6[partIndex], restHeight, partIndex);
            switch (duration)
            {
                case DurationCFugue.s:
                    res += stepLedgerLine / 2;
                    break;
                case DurationCFugue.i:
                    res += stepLedgerLine * elemPosFactor ;
                    break;
                case DurationCFugue.q:
                    res += stepLedgerLine / 2 * elemPosFactor;
                    break;
                case DurationCFugue.h:
                    res = res - stepLedgerLine * elemPosFactor + stepLedgerLine - 1;
                    break;
                case DurationCFugue.w:
                    res = res - stepLedgerLine * elemPosFactor - 1;
                    break;
            }
            return res;
        }

        /// <summary>
        /// Returns the note orientation (stem up/down) used to choose the normal note image or the lower note image.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="octave"></param>
        /// <returns></returns>
        public static char GetNoteOrientation(Note note, Octave octave)
        {
            if (octave == Octave.Octave3 || (octave == Octave.Octave4 && (note == Note.C || note == Note.D)) 
                || octave == Octave.Octave5)
                return '+';
            else 
                return '-';
        }

        /// <summary>
        /// Decides if a note supports a ledger line and returns its type.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="octave"></param>        
        /// <returns></returns>
        public static string HasLine(Note note, Octave octave)
        {
            string has = "";
 
                if ((octave == Octave.Octave3 && (note == Note.C || note == Note.E)) ||
                    (octave == Octave.Octave5 && note == Note.C) ||
                    (octave == Octave.Octave6 && note == Note.A ) || (octave == Octave.Octave7 && (note== Note.C ||
                    note == Note.E || note== Note.G || note==Note.B)))
                    has = "0";
                else if((octave == Octave.Octave3 && note==Note.D) )
                    has="-1";
                else if((octave == Octave.Octave6 && note == Note.B) || (octave == Octave.Octave7 && (note == Note.D ||
                    note==Note.F || note==Note.A)))
                    has="+1";
              
            return has;

        }

        /// <summary>
        /// Converts a string to an enum element of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        /// <summary>
        /// Helper to search up the VisualTree.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <returns></returns>
        public static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        } 
    }
}
