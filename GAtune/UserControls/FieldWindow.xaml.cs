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
using GAtune.Controllers;
using MusicNoteLib;
using WPF.MDI;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;

namespace GAtune.UserControls
{
    /// <summary>
    /// Interaction logic for FieldWindow.xaml.
    /// </summary>
    public partial class FieldWindow : UserControl
    {
        // The number of individuals per row.
        public const byte individPerRow = 3;
        private const byte tempoPos = 7;
        static byte fieldGridChildrenCount;
        // Specifies if all bars are selected.
        private bool selectAllVar = false;
        private bool lostFocus = false;
        private double barBeat = ScoreStaffs.thisWidth / 5;
        // -1: stop playing music; 0: start playing music.
        private int currPlayingStatus;
        // Specifies if an individual is locked i.e. immune to the application of genetic operators.
        private bool lockTag;
        public List<Genome> individuals = new List<Genome>();

        // A list of indexes of immune genomes.
        private static List<byte> preserved = new List<byte>();
        private static List<byte> borderTags = new List<byte>();
        private static List<byte> keySignatureTags = new List<byte>();
                
        /// <summary>
        /// Dependency property for key signature: it is updated for this field in sync with the change of the key
        /// signature in the PLAYER window.
        /// </summary>
        public string KeySignatureAccidental
        {
            get 
            {
                try
                {
                    return (string)this.Dispatcher.Invoke(DispatcherPriority.Normal,
                       (DispatcherOperationCallback)delegate { return GetValue(KeySignatureProperty); },
                       KeySignatureProperty);
                }
                catch
                {
                    return (string)KeySignatureProperty.DefaultMetadata.DefaultValue;
                }
            }
            set 
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   (SendOrPostCallback)delegate { SetValue(KeySignatureProperty, value); },
                   value);
            }
        }
        public static readonly DependencyProperty KeySignatureProperty = DependencyProperty.Register("KeySignature", typeof(string),
            typeof(FieldWindow), new PropertyMetadata(Parameters.keySignatureAccidental, KeySignaturePropertyChanged));

        private void ChangeKeySignature(string Name)
        {
            int i = 0;
            while (i < keySignatureTags.Count)
            {
                ((Image)fieldGrid.Children[keySignatureTags[i++]]).Source = LayoutController.GetKeySignature(Name, Parameters.Clefs[0].ToString());
                ((Image)fieldGrid.Children[keySignatureTags[i++]]).Source = LayoutController.GetKeySignature(Name, Parameters.Clefs[1].ToString());
            }
        }

        private static void KeySignaturePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) 
        {
            FieldWindow fieldWindow = source as FieldWindow; 
            string newValue = e.NewValue as string;
            Parameters.KeySignature = LayoutController.StringToEnum<KeySignature>(newValue + "Maj");
            fieldWindow.ChangeKeySignature(newValue);
        }

        /// <summary>
        /// Dependency property for tempo: it is updated for this field in sync with the change of the tempo
        /// in the PLAYER window.
        /// </summary>
        public string Tempo
        {
            get 
            {
                try
                {
                    return (string)this.Dispatcher.Invoke(DispatcherPriority.Normal,
                       (DispatcherOperationCallback)delegate { return GetValue(TempoProperty); },
                       TempoProperty);
                }
                catch
                {

                    return (string)TempoProperty.DefaultMetadata.DefaultValue;
                }
            }
            set 
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   (SendOrPostCallback)delegate { SetValue(TempoProperty, value); },
                   value);
            }
        }

        public static readonly DependencyProperty TempoProperty = DependencyProperty.Register("Tempo", typeof(string),
            typeof(FieldWindow), new PropertyMetadata(Parameters.Tempo.ToString(), TempoPropertyChanged));

        private void UpdateTempoLabel(string tempo)
        {
            ((Label)fieldGrid.Children[tempoPos]).Content = tempo;
        }

        private static void TempoPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            FieldWindow fieldWindow = source as FieldWindow;
            string newValue = e.NewValue as string;
            Parameters.Tempo = Convert.ToByte(newValue);
            fieldWindow.UpdateTempoLabel(newValue);
        }

        private Octave octave0 = Parameters.Octaves[0];
        public Octave Octave0
        {
            get
            {
                return octave0;
            }
            set
            {
                octave0 = value;
                if (MainWindow.openedValues.Count == 0)
                {
                    Parameters.Octaves[0] = octave0;
                    Parameters.octavesByteValues[0] = (byte)octave0;
                    Parameters.Clefs[0] = Scales.GetClef(octave0);
                }
            }
        }

        private Octave octave1 = Parameters.Octaves[1];
        public Octave Octave1
        {
            get
            {
                return octave1;
            }
            set
            {
                octave1 = value;
                if (MainWindow.openedValues.Count == 0)
                {
                    Parameters.Octaves[1] = octave1;
                    Parameters.octavesByteValues[1] = (byte)octave1;
                    Parameters.Clefs[1] = Scales.GetClef(octave1);
                }
            }
        }

        private DurationCFugue duration = Parameters.Duration;
        public DurationCFugue Duration
        {
            get
            {
                return duration;
            }
            set
            {
                duration = value;

                if (MainWindow.openedValues.Count == 0)
                {
                    Parameters.Duration = value;
                    ScoreStaffs.unitElementWidth = ScoreStaffs.thisWidth / Parameters.unitBeat;
                }                
            }
        }

        private byte barIteration0 = Parameters.barIterate[0];
        public byte BarIteration0
        {
            get
            {
                return barIteration0;
            }
            set
            {
                barIteration0 = value;
                if (MainWindow.openedValues.Count == 0)
                {
                    Parameters.barIterate[0] = barIteration0;
                    // The Duration property updates vars that depend on the barIterate param.
                    Parameters.Duration = Parameters.Duration;
                }
            }
        }

        private byte barIteration1 = Parameters.barIterate[1];
        public byte BarIteration1
        {
            get
            {
                return barIteration1;
            }
            set
            {
                barIteration1 = value;
                if (MainWindow.openedValues.Count == 0)
                {
                    Parameters.barIterate[1] = barIteration1;
                    // The Duration property updates vars that depend on the barIterate param.
                    Parameters.Duration = Parameters.Duration;
                }
            }
        }

        private BackgroundWorker[] playbackThreads = new BackgroundWorker[Parameters.PopSize];
        private int currPlayingIndex = -1;
        public Button currSpeakerButton;        

        MdiContainer windowContainer = new MdiContainer();

        private void FieldWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application curApp = Application.Current;
                Window main = curApp.MainWindow;
                DockPanel dp = (DockPanel)main.FindName("mainDockPanel");
                windowContainer = (MdiContainer)dp.FindName("WindowContainer");
            }
            catch (Exception)
            {
            }
        }

        private void InitializeIndividuals(List<Genome> Genomes)
        {
            foreach (Genome gen in Genomes)
            {
                Genome g = gen;
                individuals.Add(g);
            }
        }

        public FieldWindow(List<Genome> Genomes)
        {
            try
            {
                InitializeComponent();
                for (int i = 0; i < playbackThreads.Length; i++)
                {
                    playbackThreads[i] = new BackgroundWorker();
                    playbackThreads[i].WorkerSupportsCancellation = true;
                }
                InitializeIndividuals(Genomes);
                fieldGridChildrenCount = (byte)fieldGrid.Children.Count;
                CreateFieldTemplate();
                SetFieldContent();

                Tempo = Parameters.Tempo.ToString();
                UpdateTempoLabel(Tempo);
                KeySignatureAccidental = Parameters.keySignatureAccidental;
            }
            catch (Exception)
            {
            }
        }

        private void AddPlaybackThreadDoWorkEventHandler(Border border, int index)
        {
            playbackThreads[index].DoWork += delegate(object s, DoWorkEventArgs args)
            {
                double left = barBeat;
                border.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
        new Action(
          delegate()
          {
              // Add metronome.
              ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[index] + 1];
              Rectangle rect = new Rectangle();
              rect.Width = 3;
              rect.Height = ScoreStaffs.thisHeight;
              rect.Fill = Brushes.Blue;
              rect.Margin = new Thickness(left, 0, 0, 0);
              rect.HorizontalAlignment = HorizontalAlignment.Left;
              Grid.SetRowSpan(rect, 2);
              ss.staffGrid.Children.Add(rect);
          }
      ));

                MusicNoteLib.MusicNoteLib.myPlayAsync(0, individuals[index].AssemblePlayGenome(Tempo));

                int time = 0;
                int tempo = 670 * 120 / Convert.ToInt32(Tempo);
                tempo -= tempo % 10;

                while (MusicNoteLib.MusicNoteLib.myIsPlaying())
                {
                    if (playbackThreads[index].CancellationPending)
                    {
                        args.Cancel = true;
                        break;
                    }
                    time += 10;
                    Thread.Sleep(10);

                    // Update metronome position in currently playing bar in FIELD window.
                    if (time % tempo == 0)
                    {
                        left += barBeat;
                        if (time / tempo != 4)
                            border.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                  delegate()
                  {
                      ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[index] + 1];
                      ((Rectangle)ss.staffGrid.Children[ss.staffGrid.Children.Count - 1]).Margin = new Thickness(left, 0, 0, 0);
                  }));
                    }
                }

                while (time / tempo != 4)
                {
                    if (playbackThreads[index].CancellationPending)
                    {
                        args.Cancel = true;
                        break;
                    }
                    time += 10;
                    Thread.Sleep(10);

                    // Update metronome position in currently playing bar in FIELD window.
                    if (time % tempo == 0)
                    {
                        left += barBeat;
                        if (time / tempo != 4)
                            border.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                  delegate()
                  {
                      ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[index] + 1];
                      ((Rectangle)ss.staffGrid.Children[ss.staffGrid.Children.Count - 1]).Margin = new Thickness(left, 0, 0, 0);
                  }));
                    }
                }

                Thread.Sleep(50);
                MusicNoteLib.MusicNoteLib.myStopPlay();
            };
        }

        private void AddPlaybackThreadRunWorkerCompletedEventHandler(int index)
        {
            playbackThreads[index].RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                if (!args.Cancelled)
                    currPlayingStatus = 0;

                ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[index] + 1];
                ss.staffGrid.Children.RemoveAt(ss.staffGrid.Children.Count - 1);
            };
        }

        private void SetCurrPlayingVarsForPlayMode(int index, Object sender)
        {
            currPlayingStatus = -1;
            currPlayingIndex = index;
            currSpeakerButton = (Button)sender;
            playbackThreads[index].RunWorkerAsync();
        }

        private Button SpeakerButtonLayout(int index)
        {
            Button speakerButton = new Button();
            speakerButton.Name = "speakerButton" + index;
            Image speakerImage = new Image();
            speakerButton.Tag = 0;
            speakerImage.Source = LayoutController.GetImage(@"/Resources/App icons/field-speaker.gif");
            speakerButton.Content = speakerImage;
            speakerButton.Width = 20;
            speakerButton.Height = 20;
            speakerButton.HorizontalAlignment = HorizontalAlignment.Left;
            speakerButton.Margin = new Thickness(3, 0, 0, 0);
            return speakerButton;
        }

        private Button SpeakerButtonSetup(int index)
        {
            Button speakerButton = SpeakerButtonLayout(index);

            speakerButton.Click += delegate(object sender, RoutedEventArgs e)
            {
                if (MainWindow.playInProgress != null)
                {
                    MainWindow.playInProgress.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Thread.Sleep(500);
                    MainWindow.playInProgress = null;
                }

                if ((int)speakerButton.Tag == 0)
                {
                    speakerButton.Tag = (int)Parameters.unitBeat;

                    if (currPlayingIndex != -1)
                    {
                        playbackThreads[currPlayingIndex].CancelAsync();
                        Thread.Sleep(100);
                    }

                    SetCurrPlayingVarsForPlayMode(index, sender);
                }
                else
                {
                    if (lostFocus == true)
                    {
                        currPlayingStatus = -1;
                        lostFocus = false;
                    }

                    if (currPlayingStatus == 0)
                    {
                        SetCurrPlayingVarsForPlayMode(index, sender);
                    }
                    else
                    {
                        speakerButton.Tag = 0;
                        currPlayingIndex = -1;
                        currSpeakerButton = null;
                        playbackThreads[index].CancelAsync();
                    }
                }
            };

            return speakerButton;
        }

        private CheckBox SelectCheckBoxSetup(int index)
        {
            CheckBox selectCheckBox = new CheckBox();
            selectCheckBox.Name = "selectCheckBox" + index;
            selectCheckBox.HorizontalAlignment = HorizontalAlignment.Left;
            selectCheckBox.VerticalAlignment = VerticalAlignment.Center;
            selectCheckBox.Margin = new Thickness(52, 0, 0, 0);

            selectCheckBox.Checked += delegate
            {
                ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[index] + 1];
                if (lockTag == true)
                    ss.borderStaff.BorderBrush = Brushes.Magenta;
                else
                    ss.borderStaff.BorderBrush = Brushes.Red;
            };
            selectCheckBox.Unchecked += delegate
            {
                ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[index] + 1];
                if (lockTag == false)
                    ss.borderStaff.BorderBrush = Brushes.Black;
                else
                    ss.borderStaff.BorderBrush = Brushes.Blue;
            };

            return selectCheckBox;
        }

        private Button LockButtonLayout(int index, out Image theLock)
        {
            Button lockButton = new Button();
            lockButton.Name = "lockButton" + index;
            theLock = new Image();
            lockButton.Tag = 0;
            theLock.Source = LayoutController.GetImage("/Resources/App icons/un_lock.png");
            lockButton.Content = theLock;
            lockButton.Width = 20;
            lockButton.Height = 20;
            lockButton.HorizontalAlignment = HorizontalAlignment.Left;
            lockButton.Margin = new Thickness(26, 0, 0, 0);
            return lockButton;
        }

        private Button LockButtonSetup(int index, CheckBox selectCheckBox)
        {
            Image theLock;
            Button lockButton = LockButtonLayout(index, out theLock);
            lockButton.Click += delegate
            {
                ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[index] + 1];
                if ((int)lockButton.Tag == 0)  // If it's unlocked.
                {
                    theLock.Source = LayoutController.GetImage("/Resources/App icons/lock.png");
                    if (selectCheckBox.IsChecked == true)
                        ss.borderStaff.BorderBrush = Brushes.Magenta;
                    else
                        ss.borderStaff.BorderBrush = Brushes.Blue;
                    lockButton.Tag = 1;
                    lockTag = true;
                }
                else  // If it's locked.
                {
                    theLock.Source = LayoutController.GetImage("/Resources/App icons/un_lock.png");
                    if (selectCheckBox.IsChecked == true)
                        ss.borderStaff.BorderBrush = Brushes.Red;
                    else
                        ss.borderStaff.BorderBrush = Brushes.Black;
                    lockButton.Tag = 0;
                    lockTag = false;
                }
            };

            return lockButton;
        }

        private Label SelectLabelLayout(int index)
        {
            Label selectLabel = new Label();
            selectLabel.Content = "Select it";
            selectLabel.HorizontalAlignment = HorizontalAlignment.Left;
            selectLabel.Margin = new Thickness(70, 0, 0, 0);
            selectLabel.FontSize = 14;
            return selectLabel;
        }

        /// <summary>
        /// Creates the visual aspect of the individual in the FIELD window.
        /// </summary>
        /// <param name="index"> The index of the individual in the field.</param>
        /// <returns></returns>
        private Border CreateIndividualGrid(int index)
        {
            Border border = new Border();

            border.Name = "border" + index.ToString();
            border.BorderBrush = Brushes.Black;
            border.Background = Brushes.Gainsboro;

            Grid grid = new Grid();
            grid.Name = "grid" + index.ToString();

            currPlayingStatus = -1;
            lockTag = false;

            AddPlaybackThreadDoWorkEventHandler(border, index);
            AddPlaybackThreadRunWorkerCompletedEventHandler(index);

            Button speakerButton = SpeakerButtonSetup(index);
            CheckBox selectCheckBox = SelectCheckBoxSetup(index);
            Button lockButton = LockButtonSetup(index, selectCheckBox);
            Label selectLabel = SelectLabelLayout(index);

            grid.Children.Add(speakerButton);
            grid.Children.Add(lockButton);
            grid.Children.Add(selectCheckBox);
            grid.Children.Add(selectLabel);

            border.Child = grid;

            return border;
        }

        private void CreateIndividualTemplateInField(ref int index, ref byte cellValue, int j, int k, string borderStaffIndex)
        {
            Border g = CreateIndividualGrid(index);
            g.Tag = cellValue;
            borderTags.Add(cellValue);
            fieldGrid.Children.Add(g);

            Grid.SetRow(g, k - 1);
            Grid.SetColumn(g, j + 1);

            cellValue++;

            // The staff for the individual.
            ScoreStaffs obj = new ScoreStaffs();
            obj.Name = "scoreStaffs" + index;
            obj.borderStaff.Name += borderStaffIndex;
            if (j != 0)
                obj.Margin = new Thickness(1, 0, 0, 0);
            fieldGrid.Children.Add(obj);

            Grid.SetRow(obj, k);
            Grid.SetColumn(obj, j + 1);

            cellValue++;
        }

        /// <summary>
        /// Gets the formatted image of the key signature corresponding to the index-th part.
        /// NOTE: The first part has index 0.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Clef"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Image DrawKeySignature(string Name, Clef Clef, byte index)
        {
            Image image = new Image();
            image.Source = LayoutController.GetKeySignature(Name, Clef.ToString());
            image.VerticalAlignment = VerticalAlignment.Top;
            image.Width = 60;
            image.Margin = new Thickness(0, LayoutController.KeySignatureTopMargin[index], 0, 0);

            return image;
        }

        /// <summary>
        /// Sets the initial content for a population's individuals in the FIELD window.
        /// </summary>
        private void CreateFieldTemplate()
        {
            byte popSize = Parameters.PopSize;
            string keySignatureAccidental = Parameters.keySignatureAccidental;
            Clef[] clefs = Parameters.Clefs;

            // index's value is set to the quotient of popSize / individPerRow.
            byte index = (byte)((popSize / individPerRow) * individPerRow);
            int i = 0, j = 0, k = 0;
            byte cellValue = fieldGridChildrenCount;

            // Create field individuals in all full rows of the FIELD window.
            while (i < index)
            {
                k = 2 * (i / individPerRow + 1);

                Image keySignature1 = DrawKeySignature(keySignatureAccidental, clefs[0], 0);
                Image keySignature2 = DrawKeySignature(keySignatureAccidental, clefs[1], 1);

                fieldGrid.Children.Add(keySignature1);
                fieldGrid.Children.Add(keySignature2);
                keySignatureTags.Add(cellValue++);
                keySignatureTags.Add(cellValue++);

                Grid.SetRow(keySignature1, k);
                Grid.SetColumn(keySignature1, 0);
                Grid.SetRow(keySignature2, k);
                Grid.SetColumn(keySignature2, 0);

                // Create a full row of field individuals.
                for (j = 0; j < individPerRow; j++, i++)
                {
                    CreateIndividualTemplateInField(ref i, ref cellValue, j, k, i.ToString());
                }
            }

            // Create the rest of the field individuals, whose count is the remainder of popSize / individPerRow.
            k = 2 * (i / individPerRow + 1);
            for (j = 0; i < popSize; i++, j++)
            {
                CreateIndividualTemplateInField(ref i, ref cellValue, j, k, string.Empty);
            }
        }
 
        /// <summary>
        /// Gets a list of the selected genomes for genetic modifications.
        /// </summary>
        /// <returns></returns>
        private List<byte> GetSelectedGenomes()
        {
            List<byte> selected = new List<byte>();

            Border b = new Border();
            Grid g = new Grid();
            CheckBox cb = new CheckBox();
            byte popSize = Parameters.PopSize;
            for (byte i = 0; i < popSize; i++)
            {
                b = (Border)fieldGrid.Children[borderTags[i]];
                g = (Grid)b.Child;
                cb = (CheckBox)g.Children[2];
                if (cb.IsChecked == true)
                    selected.Add(i);
            }
            return selected;
        }

        /// <summary>
        /// Triggers the click event to select the preserved genomes.
        /// </summary>
        public void ResetPreservedGenomes()
        {
            Border b = new Border();
            Grid g = new Grid();
            Button button = new Button();
            byte popSize = Parameters.PopSize;
            for (byte i = 0; i < popSize; i++)
            {
                b = (Border)fieldGrid.Children[borderTags[i]];
                g = (Grid)b.Child;
                button = (Button)g.Children[1];
                if ((int)button.Tag == 1)
                    button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        /// <summary>
        /// Gets a list of the locked genomes (not affected by genetic operators).
        /// </summary>
        /// <returns></returns>
        private List<byte> GetNonPreservedGenomes()
        {
            List<byte> nonPreserved = new List<byte>();
            preserved.Clear();
            Border b = new Border();
            Grid g = new Grid();
            Button button = new Button();
            byte popSize = Parameters.PopSize;
            for (byte i = 0; i < popSize; i++)
            {
                b = (Border)fieldGrid.Children[borderTags[i]];
                g = (Grid)b.Child;
                button = (Button)g.Children[1];
                if ((int)button.Tag == 0)
                    nonPreserved.Add(i);
                else
                    preserved.Add(i);
            }
            return nonPreserved;
        }

        /// <summary>
        /// Updates the template of each field individual with the corresponding updated genome information.
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="genomes"></param>
        private void UpdateFieldContent(List<byte> selected, List<Genome> genomes)
        {
            Border b = new Border();
            Grid g = new Grid();
            CheckBox cb = new CheckBox();
            byte popSize = Parameters.PopSize;
            int defaultNumberElem = ScoreStaffs.defaultNumberElem;
            for (byte i = 0; i < popSize; i++)
            {
                ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[i] + 1];
                Grid grid = (Grid)ss.borderStaff.Child;
                grid.Children.RemoveRange(defaultNumberElem, grid.Children.Count - defaultNumberElem);
                ss.setContent(genomes[i]);

                // The selections from a generation are not persisted to the next one.
                // But the selected genomes which are mutated by the user, remain selected after the mutation.
                if (!isManuallyMutated)
                    if (selected.IndexOf(i) != -1)
                    {
                        b = (Border)fieldGrid.Children[borderTags[i]];
                        g = (Grid)b.Child;
                        cb = (CheckBox)g.Children[2];
                        cb.IsChecked = false;
                        cb.RaiseEvent(new RoutedEventArgs(CheckBox.UncheckedEvent));
                    }
            }
            isManuallyMutated = false;
        }

        /// <summary>
        /// Fills the template of each field individual with the corresponding genome information.
        /// </summary>
        private void SetFieldContent()
        {
            Border b = new Border();
            Grid g = new Grid();
            Button button = new Button();
            byte popSize = Parameters.PopSize;
            for (byte i = 0; i < popSize; i++)
            {
                ScoreStaffs ss = (ScoreStaffs)fieldGrid.Children[borderTags[i] + 1];
                ss.setContent(individuals[i]);

                // If the individual is supposed to be preserved.
                if (preserved.IndexOf(i) != -1)
                {
                    b = (Border)fieldGrid.Children[borderTags[i]];
                    g = (Grid)b.Child;
                    button = (Button)g.Children[1];
                    // Trigger a click event on the locking-genome button.
                    button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }

        public void ScaleChange()
        {
            List<byte> selected = GetSelectedGenomes();
            List<Genome> changed = Population.ScaleChangeGenomes(individuals);
            UpdateFieldContent(selected, changed);
            individuals.Clear();
            InitializeIndividuals(changed);
        }

        private string MarkSavedField()
        {
            string name = thisButton.Name;
            name = name.Substring(name.IndexOf('_') + 1);
            MainWindow.MarkSaved(name);
            return name;
        }

        /// <summary>
        /// Produces a generation of fresh random individuals.
        /// </summary>
        public void ResetPopulation()
        {
            List<byte> selected = new List<byte>();
            List<byte> nonPreserved = GetNonPreservedGenomes();
            MainWindow.pop.NextGeneration(individuals, nonPreserved, selected);
            UpdateFieldContent(selected, MainWindow.pop.Genomes);
            individuals.Clear();
            InitializeIndividuals(MainWindow.pop.Genomes);

            MarkSavedField();
        }

        // Is set to "true" when the user mutates selected genomes.
        private bool isManuallyMutated = false;

        /// <summary>
        /// Performs manual mutation on the selected genomes.
        /// </summary>
        public void MutateSelectedGenomes()
        {
            List<byte> selected = GetSelectedGenomes();
            MainWindow.pop.MutateSelectedGenomes(selected);
            isManuallyMutated = true;
            UpdateFieldContent(selected, MainWindow.pop.Genomes);
            individuals.Clear();
            InitializeIndividuals(MainWindow.pop.Genomes);

            MarkSavedField();
        }

        /// <summary>
        /// Selects all the individuals in the current field.
        /// </summary>
        public void SelectAllField()
        {
            Border b = new Border();
            Grid g = new Grid();
            CheckBox cb = new CheckBox();
            byte popSize = Parameters.PopSize;
            if (selectAllVar == false)
            {
                // If not all genomes are selected, select them all.
                for (byte i = 0; i < popSize; i++)
                {
                    b = (Border)fieldGrid.Children[borderTags[i]];
                    g = (Grid)b.Child;
                    cb = (CheckBox)g.Children[2];
                    if (cb.IsChecked == false)
                    {
                        cb.IsChecked = true;
                        cb.RaiseEvent(new RoutedEventArgs(CheckBox.CheckedEvent));
                    }
                }
                selectAllVar = true;
            }
            else
            {
                // If all genomes are selected, deselect them all.
                for (byte i = 0; i < popSize; i++)
                {
                    b = (Border)fieldGrid.Children[borderTags[i]];
                    g = (Grid)b.Child;
                    cb = (CheckBox)g.Children[2];
                    cb.IsChecked = false;
                    cb.RaiseEvent(new RoutedEventArgs(CheckBox.UncheckedEvent));
                }
                selectAllVar = false;
            }
        }

        /// <summary>
        /// Used to update the field every time parameters are modified.
        /// </summary>
        public void UpdatePopulation()
        {
            List<byte> selected = GetSelectedGenomes();
            MainWindow.pop.UpdateField();
            UpdateFieldContent(selected, MainWindow.pop.Genomes);
            individuals.Clear();
            InitializeIndividuals(MainWindow.pop.Genomes);
            MarkSavedField();
        }

        public void UpdateFieldOctave(byte partIndex)
        {
            UpdatePopulation();

            int i = partIndex;
            while (i < keySignatureTags.Count)
            {
                ((Image)fieldGrid.Children[keySignatureTags[i]]).Source = LayoutController.GetKeySignature(KeySignatureAccidental, Parameters.Clefs[partIndex].ToString());
                i += 2;
            }

            MarkSavedField();
        }

        /// <summary>
        /// Produces the next generation of individuals in the same field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void thisButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ChangeKeySignature(Parameters.keySignatureAccidental);
                this.UpdateTempoLabel(Parameters.Tempo.ToString());
                Tempo = Parameters.Tempo.ToString();

                List<byte> selected = GetSelectedGenomes();
                List<byte> nonPreserved = GetNonPreservedGenomes();
                MainWindow.pop.NextGeneration(individuals, nonPreserved, selected);
                UpdateFieldContent(selected, MainWindow.pop.Genomes);
                this.Tag = (int)this.Tag + 1;

                individuals.Clear();
                InitializeIndividuals(MainWindow.pop.Genomes);

                Button thisButton = (Button)sender;
                string name = MarkSavedField();                
                int nameIndex = MainWindow.mdiChildIdentifier.IndexOf(name);
                MdiChild child = (MdiChild)windowContainer.Children[nameIndex + MainWindow.initialChildCount];

                child.Title = child.Title.Remove(child.Title.IndexOf('.') + 1);
                string fieldCount = child.Title.Substring(child.Title.IndexOf(' ') + 1);
                fieldCount = fieldCount.Remove(fieldCount.Length - 1);
                child.Title += this.Tag;

                child.ToolTip = child.Title;

                MainWindow.mdiChildIdentifier.RemoveAt(nameIndex);
                MainWindow.mdiChildIdentifier.Insert(nameIndex, fieldCount + "_" + this.Tag);
                thisButton.Name = "thisButton_" + fieldCount + "_" + this.Tag;                
            }
            catch (Exception)
            {
            }
        }

        private void AddFieldWindowGotFocusEventHandler(MdiChild child)
        {
            child.GotFocus += delegate
            {
                MainWindow.lastFocusedIndex = windowContainer.Children.IndexOf(child);
            };
        }

        private void AddFieldWindowClosingEventHandler(MdiChild child)
        {
            child.Closing += delegate(object sende, RoutedEventArgs ee)
            {
                MainWindow.lastFocusedIndex = MainWindow.initialChildCount - 1;
                int Index = MainWindow.mdiChildIdentifier.IndexOf(child.Title.Substring(child.Title.IndexOf(" ") + 1).Replace('.', '_'));
                int index = windowContainer.Children.IndexOf(child);
                int saved = MainWindow.savedIndexes.IndexOf(index);
                if (saved != -1)
                {
                    string name = MainWindow.savedFieldsAndScores[saved];
                    if (name.Contains('|'))
                    {
                        MessageBoxResult result = MessageBox.Show("Do you want to save changes to " + name + "?",
                            "GAtune", MessageBoxButton.YesNoCancel);
                        if (result == MessageBoxResult.Yes)
                        {
                            MainWindow.WriteToFieldSavedFile(name, (FieldWindow)child.Content);
                            MainWindow.mdiChildIdentifier.RemoveAt(Index);
                            MainWindow.savedFieldsAndScores.RemoveAt(MainWindow.savedIndexes.IndexOf(index));
                            MainWindow.savedIndexes.RemoveAt(MainWindow.savedIndexes.IndexOf(index));
                        }
                        else if (result == MessageBoxResult.Cancel)
                        {
                            ClosingEventArgs eventArgs = (ClosingEventArgs)ee;
                            eventArgs.Cancel = true;
                        }
                        else //result == NO
                        {
                            MainWindow.mdiChildIdentifier.RemoveAt(Index);
                            MainWindow.savedFieldsAndScores.RemoveAt(MainWindow.savedIndexes.IndexOf(index));
                            MainWindow.savedIndexes.RemoveAt(MainWindow.savedIndexes.IndexOf(index));
                        }
                    }
                }
                else
                    MainWindow.mdiChildIdentifier.RemoveAt(Index);
            };
        }

        /// <summary>
        /// Produces the next generation of individuals in a new field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void newButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currSpeakerButton != null)
                    currSpeakerButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                List<byte> selected = GetSelectedGenomes();
                List<byte> nonPreserved = GetNonPreservedGenomes();
                MainWindow.pop.NextGeneration(individuals, nonPreserved, selected);

                FieldWindow fw = new FieldWindow(MainWindow.pop.Genomes);
                fw.Tag = (int)this.Tag + 1;
                ((Button)fw.fieldGrid.FindName("thisButton")).Name = "thisButton_" + MainWindow.fieldCount + "_" + fw.Tag;

                MdiChild child = new MdiChild();
                child.Name = MainWindow.fieldIdentifier;
                child.Title = "Field " + MainWindow.fieldCount + "." + fw.Tag;
                MainWindow.mdiChildIdentifier.Add(MainWindow.fieldCount + "_" + fw.Tag);
                child.Width = double.NaN;
                child.Height = double.NaN;
                child.Background = Brushes.White;
                child.Content = fw;
                child.ToolTip = child.Title;
                windowContainer.Children.Add(child);

                AddFieldWindowGotFocusEventHandler(child);
                AddFieldWindowClosingEventHandler(child);                
            }
            catch (Exception)
            {
            }
        }

        public void ChildWindow_LostFocus(object sender, RoutedEventArgs e)
        {
            if (currSpeakerButton != null)
            {
                lostFocus = true;
                currSpeakerButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }       

        Point startPoint;
        private void fieldGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position.
            startPoint = e.GetPosition(null);
        }

        private void fieldGrid_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position.
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;

                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    // Get the dragged ListViewItem.
                    Grid grid = (Grid)sender;
                    ScoreStaffs ss = LayoutController.FindAncestor<ScoreStaffs>((DependencyObject)e.OriginalSource);

                    // Initialize the drag & drop operation.
                    if (ss != null)
                    {
                        int index = Convert.ToInt32(ss.Name.Substring(ss.Name.LastIndexOf("s") + 1));
                        Genome gen = individuals[index];
                        DataObject dragData = new DataObject();
                        dragData.SetData("Object", gen);
                        dragData.SetData("String", KeySignatureAccidental + "|" + Tempo);
                        Octave[] octaves = { octave0, octave1 };
                        dragData.SetData("MusicNoteLib.Octave[]", octaves);
                        dragData.SetData("MusicNoteLib.DurationCFugue", Duration);
                        byte[] barIter = { barIteration0, barIteration1 };
                        dragData.SetData("byte[]", barIter);

                        DragDrop.DoDragDrop(ss, dragData, DragDropEffects.Copy);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void mutateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MutateSelectedGenomes();
            }
            catch (Exception)
            {
            }
        }           
    }
}
