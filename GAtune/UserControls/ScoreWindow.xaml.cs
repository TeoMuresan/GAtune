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
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using MusicNoteLib;
using WPF.MDI;
using GAlib;
using GAtune.Controllers;

namespace GAtune.UserControls
{
    /// <summary>
    /// Interaction logic for ScoreWindow.xaml
    /// </summary>
    public partial class ScoreWindow : UserControl
    {
        public int barCount = 0;
        public string keySignature = KeySignatures.GetKeyFromKeySignature(KeySignature.CMaj);
        private Clef[] Clefs = new Clef[2];
        private string Tempo;
        public GeneticSong geneticSong;
        private string playingSong = "";
        // The index of the bar from where the melody should resume playing.
        private int currPlayingIndex = 0;
        private bool stop = false;
        private bool pause = false;
        // Specifies if selection mode 1 is currently active.
        private bool isSelectionMode1 = false;
        private bool addBarOnPause = false;
        private bool modifyScore = false;
        private bool clickedAll = false;
        // The index of the bar from where the melody should start playing:
        // - 0, if no bar is selected in mode 1;
        // - selectIndex, if there's a bar selected in mode 1.
        private int startIndex = 0;
        // The index of the bar currently selected in mode 1.
        private int selectIndex = -1;
        // Basically, this is currPlayingIndex for the partial genetic song.
        private int currPlayingIndexPartialGeneticSong = 0;
        private bool addEmptyBar = false;
        private int countDeleted = 0;

        #region Elements needed for layout
        private const int individPerRow = FieldWindow.individPerRow;
        private int individPerPage = FieldWindow.individPerRow * 2;
        private double pauseOffset;
        private double verticalOffset;
        private double startOffset;
        private const double ksPanelWidth = 55;
        private const double barWidth = 780;
        private const double barHeight = 338;
        // The time signature is 4/4, so there are 4 beats per measure which divide it in 5 parts.
        private const double barBeat = barWidth / 5;
        // The margins that set the position of the metronome in the SCORE window.
        private double leftMargin;
        private double topMargin;
        #endregion

        private List<ScoreStaffs> scores = new List<ScoreStaffs>();
        private List<WrapPanel> keySignatures = new List<WrapPanel>();
        public BackgroundWorker playbackThread = new BackgroundWorker();
        private MdiContainer windowContainer = new MdiContainer();
        private DockPanel dp = new DockPanel();

        private void ScoreWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application curApp = Application.Current;
                Window main = curApp.MainWindow;
                dp = (DockPanel)main.FindName("mainDockPanel");
                windowContainer = (MdiContainer)dp.FindName("WindowContainer");
            }
            catch (Exception)
            {
            }
        }        

        /// <summary>
        /// Decides if there is a primary selected bar; if so, the song starts playing from it. 
        /// </summary>
        /// <returns></returns>
        private void RefreshValues(int newPrimBar)
        {
            try
            {
                if (selectIndex != newPrimBar)
                {
                    selectIndex = newPrimBar;
                    startIndex = selectIndex;
                    startOffset = verticalOffset;
                    playingSong = geneticSong.TruncateSong(geneticSong.GetSong(), startIndex);
                }
                else
                {
                    selectIndex = -1;
                    startIndex = 0;
                    startOffset = 0;
                    playingSong = geneticSong.GetSong();
                }

                currPlayingIndex = startIndex;
            }
            catch (Exception)
            {
            }
        }

        private void scoreWrapPanelDispatcherInvokeUpdateScrollViewer(double offset)
        {
            scoreWrapPanel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        scoreScrollViewer.UpdateLayout();
                        scoreScrollViewer.ScrollToVerticalOffset(offset);
                    }
            ));
        }

        private void scoreWrapPanelDispatcherInvokeAddMetronome(double left, double top)
        {
            scoreWrapPanel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        // The metronome layout object.
                        Rectangle rect = new Rectangle();
                        rect.Width = 3;
                        rect.Height = ScoreStaffs.thisHeight;
                        rect.Fill = Brushes.Blue;
                        rect.Margin = new Thickness(left, top, 0, 0);

                        scoreWrapPanel.Children.Add(rect);
                    }
            ));
        }

        private void scoreWrapPanelDispatcherInvokeScrollDown()
        {
            scoreWrapPanel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                        {
                            scoreScrollViewer.PageDown();
                        }
            ));
        }

        private void scoreWrapPanelDispatcherInvokeUpdateMetronome(double left, double top)
        {
            scoreWrapPanel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        ((Rectangle)scoreWrapPanel.Children[scoreWrapPanel.Children.Count - 1]).Margin = new Thickness(left, top, 0, 0);
                    }
            ));
        }

        private double CalculateInitialLeftMargin(int remainder)
        {
            double left = 0;
            int tempLeft = (currPlayingIndex + 1) % individPerRow;

            if (remainder == 0)
            {
                tempLeft = currPlayingIndex % individPerRow;
                left = -((individPerRow - tempLeft) * barWidth - barBeat);
            }
            else if (remainder == 1)
            {
                if (tempLeft == 0)
                    left += barWidth / 2 + barBeat / 2;
                else if (tempLeft == 1)
                    left = -(1 * barWidth - barBeat);
                else
                    left += barBeat / 2;
            }
            else
            {
                if (tempLeft == 0)
                    left += barBeat / 2;
                else if (tempLeft == 1)
                    left = -(2 * barWidth - barBeat);
                else
                    left = -(1 * barWidth - barBeat);
            }

            return left;
        }

        private double CalculateInitialTopMargin()
        {
            double top = 0;
            int tempTop = (int)(Math.Ceiling(((double)barCount / individPerRow)) - Math.Ceiling(((double)(currPlayingIndex + 1) / individPerRow)));
            top = -(tempTop * barHeight);
            return top;
        }

        private int CalculateTempo(int currentIndex)
        {
            int tempo = 670 * 120 / geneticSong.tempos[currentIndex];
            tempo -= tempo % 10;
            return tempo;
        }

        /// <summary>
        /// Updates margins to resume playing from where it was cancelled.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="currentIndex"> The index of the bar where the playing was cancelled.</param>
        private void UpdateMarginsToResumePlaying(ref double left, double top, int currentIndex)
        {
            currPlayingIndex = currentIndex;
            if (left >= 0)
                left = left - (left % (barWidth / 2)) + barBeat / 2;
            else
                left = left - (barWidth + (left % barWidth)) + barBeat;
            leftMargin = left;
            topMargin = top;
        }

        /// <summary>
        /// Updates margins when the metronome jumps to the next bar and that bar lies on the next row.
        /// </summary>
        /// <param name="remainder"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        private void UpdateMarginsNextRow(int remainder, ref double left, ref double top)
        {
            double temp = individPerRow - remainder;
            if (left >= 0)
                left += barBeat / 2;
            else
                left += barBeat;
            if (remainder != 0)
                left = left - (temp * barWidth / 2 + remainder * barWidth) + barBeat;
            else
                left = left - (temp * barWidth + remainder * barWidth / 2) + barBeat;
            top += barHeight;
        }

        /// <summary>
        /// Updates margins when the metronome jumps to the next bar and that bar lies on the same row.
        /// </summary>
        /// <param name="left"></param>
        private void UpdateMarginsSameRow(ref double left)
        {
            if (left >= 0)
                left += barBeat;
            else
            {
                left += barBeat;
                if (left == 0)
                    left += barBeat / 2;
                else
                    left += barBeat;
            }
        }        

        /// <summary>
        /// Updates margins when the metronome advances within the bar.        
        /// </summary>
        /// <param name="left"></param>
        private void UpdateMarginsWithinBar(ref double left)
        {
            if (left >= 0)
                left += barBeat / 2;
            else
                left += barBeat;
        }

        private void UpdateMetronomePosition(ref double left, ref double top, ref int currentIndex, ref int time, ref int tempo,
            int remainder)
        {
            // If the rectangle is still within the boundaries of the current bar.
            if (time / tempo != 4)
            {
                UpdateMarginsWithinBar(ref left);
            }
            // If I jump to the next bar.
            else
            {
                currentIndex++;
                if (currentIndex < barCount)
                {
                    // If I jump to the next row of bars.
                    if (currentIndex % individPerRow == 0)
                    {
                        UpdateMarginsNextRow(remainder, ref left, ref top);
                        scoreWrapPanelDispatcherInvokeScrollDown();
                    }
                    // If I remain on the same row of bars.
                    else
                    {
                        UpdateMarginsSameRow(ref left);
                    }

                    time = 0;
                    tempo = CalculateTempo(currentIndex);
                }
            }
            scoreWrapPanelDispatcherInvokeUpdateMetronome(left, top);
        }

        private void AddPlaybackThreadDoWorkEventHandler()
        {
            currPlayingIndexPartialGeneticSong = 0;

            playbackThread.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                double left = 0;
                double top = 0;                
                int remainder = barCount % individPerRow;

                // Start from the mode-1-selected bar, if there is one selected; else start from the beginning.
                if (currPlayingIndex == startIndex)
                {
                    playingSong = geneticSong.TruncateSong(geneticSong.GetSong(), startIndex);
                    left = CalculateInitialLeftMargin(remainder);
                    top = CalculateInitialTopMargin();
                    scoreWrapPanelDispatcherInvokeUpdateScrollViewer(startOffset);
                }
                // Start from the bar where the song has been paused.
                else
                {
                    left = leftMargin;
                    top = topMargin;
                    scoreWrapPanelDispatcherInvokeUpdateScrollViewer(pauseOffset);
                }

                scoreWrapPanelDispatcherInvokeAddMetronome(left, top);

                MusicNoteLib.MusicNoteLib.myPlayAsync(0, playingSong);
                int time = 0;
                int currIndex = currPlayingIndex;                
                int tempo = CalculateTempo(currIndex);

                while (MusicNoteLib.MusicNoteLib.myIsPlaying())
                {
                    if (playbackThread.CancellationPending)
                    {
                        if (stop == false)
                        {
                            currPlayingIndexPartialGeneticSong = currIndex - currPlayingIndex;
                            UpdateMarginsToResumePlaying(ref left, top, currIndex);
                        }
                        else
                        {
                            stop = false;
                        }
                        args.Cancel = true;
                        break;
                    }
                    time += 10;
                    Thread.Sleep(10);

                    // If I need to change the rectangle's position.
                    if (time % tempo == 0)
                    {
                        UpdateMetronomePosition(ref left, ref top, ref currIndex, ref time, ref tempo, remainder);
                    }
                }
                MusicNoteLib.MusicNoteLib.myStopPlay();
            };
        }

        private void AddPlaybackThreadRunWorkerCompletedEventHandler()
        {
            playbackThread.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                if (args.Cancelled)
                {
                    if (pause == true)
                    {
                        playingSong = geneticSong.TruncateSong(playingSong, currPlayingIndexPartialGeneticSong);                        
                    }
                }
                else
                {
                    currPlayingIndex = startIndex;
                    pause = false;
                    addBarOnPause = false;
                }
                if (MainWindow.playInProgress != null)
                    MainWindow.playInProgress = null;
                scoreWrapPanel.Children.RemoveAt(scoreWrapPanel.Children.Count - 1);

                Thread.Sleep(250);
            };
        }

        public ScoreWindow()
        {
            try
            {
                InitializeComponent();
                playbackThread.WorkerSupportsCancellation = true;
                AddPlaybackThreadDoWorkEventHandler();
                AddPlaybackThreadRunWorkerCompletedEventHandler();                
            }
            catch (Exception)
            {
            }
        }

        private void scoreScrollViewer_DragEnter(object sender, DragEventArgs e)
        {
            if (sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Starts a new row of bars with the specified key signature.
        /// </summary>
        /// <param name="keySignature"></param>
        private void StartNewRow(string keySign)
        {
            WrapPanel p = new WrapPanel();
            p.Width = ksPanelWidth;
            p.Height = ScoreStaffs.thisHeight;
            Image keySignature1 = FieldWindow.DrawKeySignature(keySign, Clefs[0], 0);
            Image keySignature2 = FieldWindow.DrawKeySignature(keySign, Clefs[1], 1);
            keySignature1.Margin = new Thickness(0, LayoutController.KeySignatureTopMarginScore[0], 0, 0);
            keySignature2.Margin = new Thickness(0, LayoutController.KeySignatureTopMarginScore[1], 0, 0);
            p.Children.Add(keySignature1);
            p.Children.Add(keySignature2);

            keySignatures.Add(p);
            scoreWrapPanel.Children.Add(p);
        }

        private void AddScoreStaffsMouseLeftButtonUpEventHandler(ScoreStaffs ss)
        {
            ss.MouseLeftButtonUp += delegate
            {
                if (isSelectionMode1 == false)
                {
                    if (ss.clicked == false)
                    {
                        ss.clicked = true;
                        ss.borderStaff.Background = Brushes.Beige;
                    }
                    else
                    {
                        ss.clicked = false;
                        ss.borderStaff.Background = Brushes.White;
                    }
                }
                else
                {
                    int newPrimBar = scores.IndexOf(ss);
                    if (selectIndex != newPrimBar)
                    {
                        if (selectIndex != -1)   // If there is a mode-1-selected bar.
                        {
                            ScoreStaffs oldPrimBar = (ScoreStaffs)scores[selectIndex];
                            oldPrimBar.borderStaff.BorderThickness = new Thickness(0);
                        }
                        ss.borderStaff.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        ss.borderStaff.BorderThickness = new Thickness(0);
                    }
                    RefreshValues(newPrimBar);
                }
            };                        
        }

        private void AddScoreStaffsDragEnterEventHandler(ScoreStaffs ss)
        {
            ss.DragEnter += delegate(object senderS, DragEventArgs eS)
            {
                if (senderS == eS.Source)
                {
                    eS.Effects = DragDropEffects.None;
                }
            };
        }

        private void AddScoreStaffsDropEventHandler(ScoreStaffs ss)
        {
            ss.Drop += delegate(object senderSS, DragEventArgs eSS)
            {                
                Genome g = (Genome)eSS.Data.GetData("Object");
                if (g == null)
                {
                    int indexAlt = Convert.ToInt32(eSS.Data.GetData("int"));
                    ScoreStaffs ssAlt = (ScoreStaffs)scoreWrapPanel.Children[indexAlt];
                    int index = scoreWrapPanel.Children.IndexOf(ss);
                    if (index != indexAlt)
                    {
                        Label lAlt = (Label)ssAlt.staffGrid.Children[ssAlt.staffGrid.Children.Count - 1];
                        Label l = (Label)ss.staffGrid.Children[ss.staffGrid.Children.Count - 1];
                        int indexSc = Convert.ToInt32(l.Content) - 1;
                        int indexAltSc = Convert.ToInt32(lAlt.Content) - 1;

                        if (selectIndex != -1)
                        {
                            if (indexSc == startIndex)
                            {
                                ss.borderStaff.BorderThickness = new Thickness(0);
                                ssAlt.borderStaff.BorderThickness = new Thickness(2);
                            }
                            if (indexAltSc == startIndex)
                            {
                                ss.borderStaff.BorderThickness = new Thickness(2);
                                ssAlt.borderStaff.BorderThickness = new Thickness(0);
                            }
                        }

                        string aux = lAlt.Content.ToString();
                        lAlt.Content = l.Content;
                        l.Content = aux;

                        scoreWrapPanel.Children.Remove(ss);
                        scoreWrapPanel.Children.Remove(ssAlt);
                        scores.Remove(ss);
                        scores.Remove(ssAlt);

                        if (index < indexAlt)
                        {
                            scoreWrapPanel.Children.Insert(index, ssAlt);
                            scoreWrapPanel.Children.Insert(indexAlt, ss);
                            scores.Insert(indexSc, ssAlt);
                            scores.Insert(indexAltSc, ss);
                        }
                        else
                        {
                            scoreWrapPanel.Children.Insert(indexAlt, ss);
                            scoreWrapPanel.Children.Insert(index, ssAlt);
                            scores.Insert(indexAltSc, ss);
                            scores.Insert(indexSc, ssAlt);
                        }
                        geneticSong.SwapGenomes(indexSc, indexAltSc);

                        MarkSavedScore(true);
                    }
                }
            };
        }

        private void AddBlankBar(ScoreStaffs ss, int pos)
        {
            if (selectIndex != -1)
            {
                for (int i = selectIndex + 1; i < scores.Count; i++)
                {
                    Label l = (Label)scores[i].staffGrid.Children[scores[i].staffGrid.Children.Count - 1];
                    int n = Convert.ToInt32(l.Content);
                    l.Content = n + 1;
                }
            }

            scores.Insert(pos, ss);
            int newBarCount = scores.Count;

            int k = (int)Math.Ceiling((double)newBarCount / individPerRow);
            if (newBarCount <= individPerRow)
                k = 1;

            keySignatures.RemoveRange(k, keySignatures.Count - k);

            scoreWrapPanel.Children.Clear();

            k = 0;
            int count = 0;
            while (k < scores.Count)
            {
                scoreWrapPanel.Children.Add(keySignatures[count++]);
                for (int j = 0; j < individPerRow && k < scores.Count; j++)
                {
                    scoreWrapPanel.Children.Add(scores[k]);
                    k++;
                }
            }
        }

        /// <summary>
        /// Gets the bar position used to insert the bar into the song.
        /// </summary>
        /// <returns></returns>
        private int GetBarPosition()
        {
            int barPosition;

            if (!addEmptyBar)
            {
                barPosition = -1;
            }                        
            else
            {
                // If the empty bar to be added is the first one in the score.
                barPosition = 0;                
                if (barCount > 0)
                {
                    // If there isn't a mode-1-selected bar, insert the empty bar at the end of the score.
                    barPosition = barCount;
                    // Else, insert the empty bar right after the mode-1-selected bar.
                    if (selectIndex != -1)
                    {
                        barPosition = selectIndex + 1;
                    }
                }
            }

            return barPosition;
        }

        /// <summary>
        /// Create a ScoreStaffs object for a bar.
        /// </summary>
        /// <param name="genome"></param>
        /// <param name="tempo"></param>
        /// <param name="barPosition"></param>
        /// <returns></returns>
        private ScoreStaffs CreateScoreStaffsBar(Genome genome, string tempo, int barPosition)
        {
            // The staffs for the new bar.
            ScoreStaffs ss = new ScoreStaffs();
            ss.Background = Brushes.White;
            ss.setContent(genome);
            ss.AddTempoInfo(tempo);
            if (addEmptyBar == false)
                ss.AddIndexTag(barCount + 1);
            else
                ss.AddIndexTag(barPosition + 1);
            ss.borderStaff.BorderThickness = new Thickness(0);
            ss.borderStaff.BorderBrush = Brushes.Blue;

            ss.AllowDrop = true;
            AddScoreStaffsMouseLeftButtonUpEventHandler(ss);
            AddScoreStaffsDragEnterEventHandler(ss);
            AddScoreStaffsDropEventHandler(ss);

            return ss;
        }

        private void AddBar(Genome genome, string ksa, Octave[] octaves, DurationCFugue durCF, byte[] barIter, string tempo)
        {            
            if (barCount == 0)
            {
                keySignature = ksa;
                Clefs[0] = Scales.GetClef(octaves[0]);
                Clefs[1] = Scales.GetClef(octaves[1]);
                Tempo = "";
                geneticSong = new GeneticSong(keySignature);
            }

            if (keySignature == ksa)
            {                
                byte[] oct = { (byte)octaves[0], (byte)octaves[1] };
                int barPosition = GetBarPosition();                             
                geneticSong.AddGenome(tempo, genome, oct, durCF, barIter, barPosition);

                if (pause == true)
                    addBarOnPause = true;

                // If the new bar is the first one in the next row.
                if (barCount % individPerRow == 0)
                {
                    StartNewRow(ksa);
                }

                ScoreStaffs ss = CreateScoreStaffsBar(genome, tempo, barPosition);           

                if (addEmptyBar == false)
                {
                    scores.Add(ss);
                    scoreWrapPanel.Children.Add(ss);                    
                }
                else
                {
                    AddBlankBar(ss, barPosition);
                }

                barCount++;
                addEmptyBar = false;
                Tempo = tempo;                
            }
            else
                MessageBox.Show("Key signatures must match!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void OpenScore(List<Genome> genomes, List<string> scoreParams)
        {
            keySignature = scoreParams[0];
            byte beat;
            int pmCount = 6;
            // The first element in <see cref="scoreParams"/> is the key signature.
            int index = 1;
            Octave[] oct = new Octave[2];
            byte[] barIter = new byte[2];
            string tempo, duration;
            DurationCFugue cFugueDuration;
            double realUnit = ScoreStaffs.unitElementWidth;
            int genomesCount = genomes.Count;

            try
            {
                for (int i = 0; i < genomesCount; i++, index += pmCount)
                {
                    tempo = scoreParams[index];
                    oct[0] = LayoutController.StringToEnum<Octave>("Octave" + scoreParams[index + 1]);
                    oct[1] = LayoutController.StringToEnum<Octave>("Octave" + scoreParams[index + 2]);
                    barIter[0] = Convert.ToByte(scoreParams[index + 3]);
                    barIter[1] = Convert.ToByte(scoreParams[index + 4]);

                    duration = scoreParams[index + 5];
                    cFugueDuration = Controller.StringToEnum<DurationCFugue>(duration);
                    if (duration == "s")
                        beat = 16;
                    else if (duration == "i")
                        beat = 8;
                    else if (duration == "q")
                        beat = 4;
                    else
                        beat = 16;

                    ScoreStaffs.unitElementWidth = ScoreStaffs.thisWidth / beat;
                    AddBar(genomes[i], keySignature, oct, cFugueDuration, barIter, tempo);
                    ScoreStaffs.unitElementWidth = realUnit;
                }
            }
            catch (Exception)
            {
                throw new Exception("The file you're trying to open is corrupted.");
            }
        }

        private void scoreScrollViewer_Drop(object sender, DragEventArgs e)
        {
            try
            {
                Genome gen = e.Data.GetData("Object") as Genome;
                if (gen != null && !playbackThread.IsBusy)
                {                    
                    string str = e.Data.GetData("String") as string;
                    string tempo = str.Substring(str.IndexOf("|") + 1);
                    string ksa = str.Remove(str.IndexOf("|"));
                    Octave[] octaves = e.Data.GetData("MusicNoteLib.Octave[]") as Octave[];

                    DurationCFugue durCF = (DurationCFugue)(e.Data.GetData("MusicNoteLib.DurationCFugue"));
                    byte[] barIter = e.Data.GetData("byte[]") as byte[];

                    // If there's an empty bar currently mode-1-selected, replace it with the bar that's being dropped.
                    if (geneticSong != null && selectIndex!=-1 && geneticSong.IsEmpty(selectIndex) == true)
                        FillEmptyBar(gen, ksa, octaves, durCF, barIter, tempo);
                    // Else, append to the song the bar that's being dropped.
                    else
                        AddBar(gen, ksa, octaves, durCF, barIter, tempo);

                    MarkSavedScore(true);
                }
            }
            catch (Exception)
            {
            }
        }

        private void FillEmptyBar(Genome gen, string keysignature, Octave[] octaves, DurationCFugue durCF, byte[] barIter, string tempo)
        {
            ScoreStaffs ss; 

            int ind = scoreWrapPanel.Children.IndexOf(scores[startIndex]);
            ss = ((ScoreStaffs)scoreWrapPanel.Children[ind]); 
            ss.staffGrid.Children.RemoveRange(ss.staffGrid.Children.Count - 7 , 7);
            ss.setContent(gen);
            ss.AddTempoInfo(tempo);
            ss.AddIndexTag(startIndex + 1);
            
            byte[] oct = { (byte)octaves[0], (byte)octaves[1] };
            geneticSong.FillEmptyBar(tempo, gen, oct, durCF, barIter, startIndex);         
        }

        private void DeleteMode2SelectedBar()
        {
            List<int> delIndex = new List<int>();

            int count = 1;
            barCount = scores.Count;
            for (int i = 0; i < barCount; i++)
            {
                if (scores[i].clicked == false)
                {
                    Label l = (Label)scores[i].staffGrid.Children[scores[i].staffGrid.Children.Count - 1];
                    l.Content = count.ToString();
                    count++;
                }
                else
                {
                    delIndex.Add(i);
                    if (i < selectIndex)
                        countDeleted++;
                }
            }

            if (delIndex.Contains(startIndex))
                RefreshValues(-1);

            delIndex.Sort();
            for (int i = delIndex.Count - 1; i >= 0; i--)
                scores.RemoveAt(delIndex[i]);

            barCount = scores.Count;

            int k = (int)Math.Ceiling((double)barCount / individPerRow);
            if (barCount <= individPerRow)
                k = 1;

            keySignatures.RemoveRange(k, keySignatures.Count - k);
            scoreWrapPanel.Children.Clear();

            k = 0;
            count = 0;
            while (k < barCount)
            {
                scoreWrapPanel.Children.Add(keySignatures[count++]);
                for (int j = 0; j < individPerRow && k < barCount; j++)
                {
                    scoreWrapPanel.Children.Add(scores[k]);
                    k++;
                }
            }

            geneticSong.DeleteGenome(delIndex);            

            if (pause == true)
                modifyScore = true;
            else
                modifyScore = false;

            MarkSavedScore(modifyScore);
        }

        private void scoreScrollViewer_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.Home))
                {
                    scoreScrollViewer.ScrollToHome();
                }
                else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.End))
                {
                    scoreScrollViewer.ScrollToEnd();
                }
                else if (e.Key == Key.Delete && !playbackThread.IsBusy)
                {
                    DeleteMode2SelectedBar();
                }
            }
            catch (Exception)
            {
            }
        }

        Point startPoint;
        private void scoreScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position.
            startPoint = e.GetPosition(null);
        }

        Point mousePos;
        private void scoreScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position.
                mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;

                if (!playbackThread.IsBusy)
                {

                    if (e.LeftButton == MouseButtonState.Pressed &&
                        (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                    {
                        ScrollViewer sv = (ScrollViewer)sender;
                        ScoreStaffs ss = LayoutController.FindAncestor<ScoreStaffs>((DependencyObject)e.OriginalSource);
                        if (ss != null)
                        {
                            int index = scoreWrapPanel.Children.IndexOf(ss);
                            DataObject dragData = new DataObject("int", index);
                            DragDrop.DoDragDrop(ss, dragData, DragDropEffects.Move);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!playbackThread.IsBusy && scores.Count != 0)
                {
                    if (MainWindow.playInProgress != null)
                    {
                        MainWindow.playInProgress.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Thread.Sleep(500);
                        MainWindow.playInProgress = null;
                    }

                    if (pause == true)
                    {
                        if (addBarOnPause == true)
                        {
                            selectIndex = -1;
                            RefreshValues(startIndex);
                            addBarOnPause = false;
                        }
                        else
                        {
                            if (modifyScore == true)
                            {
                                playingSong = geneticSong.TruncateSong(geneticSong.GetSong(), currPlayingIndex);
                                modifyScore = false;
                            }
                        }
                        pause = false;
                    }
                    else
                    {
                        if (selectIndex == -1)
                        {
                            RefreshValues(selectIndex);
                        }
                        else
                        {
                            if (countDeleted != 0)
                            {
                                RefreshValues(selectIndex - countDeleted);
                                countDeleted = 0;
                            }
                        }
                    }
                    playbackThread.RunWorkerAsync();
                }
            }
            catch (Exception)
            {
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (playbackThread.IsBusy)
                {
                    stop = true;
                    playbackThread.CancelAsync();
                }
                currPlayingIndex = startIndex;
                playingSong = "";
                addBarOnPause = false;
            }
            catch (Exception)
            {
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (playbackThread.IsBusy)
                {
                    pauseOffset = verticalOffset;
                    pause = true;
                    playbackThread.CancelAsync();
                }
            }
            catch (Exception)
            {
            }
        }

        private void MarkSavedScore(bool isModifyScore)
        {
            string name = this.Tag.ToString();
            MainWindow.MarkSaved(name);
            modifyScore = isModifyScore;
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!playbackThread.IsBusy)
                {
                    currPlayingIndex = 0;
                    playingSong = "";
                    startIndex = 0;
                    selectIndex = -1;
                    addBarOnPause = false;
                    barCount = 0;
                    scores.Clear();
                    keySignatures.Clear();                    
                    scoreWrapPanel.Children.Clear();

                    MarkSavedScore(modifyScore);                    
                }
            }
            catch (Exception)
            {
            }
        }

        private void scoreScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            verticalOffset = e.VerticalOffset;
        }

        private void Shrink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (scoreScrollViewer.Height != ScoreStaffs.thisHeight)
                {
                    scoreScrollViewer.Height -= ScoreStaffs.thisHeight;
                    individPerPage -= individPerRow;
                }
            }
            catch (Exception)
            {
            }
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (scoreScrollViewer.Height != individPerRow * ScoreStaffs.thisHeight)
                {
                    scoreScrollViewer.Height += ScoreStaffs.thisHeight;
                    individPerPage += individPerRow;
                }
            }
            catch (Exception)
            {
            }
        }

        public void SelectAllScore()
        {
            if (isSelectionMode1 == false)
            {
                if (clickedAll == false)
                {
                    foreach (ScoreStaffs ss in scores)
                    {
                        ss.clicked = true;
                        ss.borderStaff.Background = Brushes.Beige;
                    }
                    clickedAll = true;
                }
                else
                {
                    foreach (ScoreStaffs ss in scores)
                    {
                        ss.clicked = false;
                        ss.borderStaff.Background = Brushes.White;
                    }
                    clickedAll = false;
                }
            }
        }
        
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isSelectionMode1 == false)
                {
                    isSelectionMode1 = true;
                    selectImage.Source = LayoutController.GetImage("/Resources/App icons/scbtpalm.png");
                    Select.ToolTip = "Selection I";
                }
                else
                {
                    isSelectionMode1 = false;
                    selectImage.Source = LayoutController.GetImage("/Resources/App icons/scbtarrow.png");
                    Select.ToolTip = "Selection II";
                }
            }
            catch (Exception)
            {
            }
        }

        private List<int> CountMode2Selected()
        {
            List<int> list = new List<int>();
            int scoresCount = scores.Count;
            for (int i = 0; i < scoresCount; i++)
                if (scores[i].clicked == true)
                    list.Add(i);
            return list;
        }

        public void ApplyTempo(string newTempo)
        {
            List<int> list = CountMode2Selected();
            if (list.Count != 0)
            {
                geneticSong.ApplyTempo(list, newTempo);
                foreach (int ind in list)
                    ((Label)scores[ind].staffGrid.Children[scores[ind].staffGrid.Children.Count - 2]).Content = newTempo;

                MarkSavedScore(true);
            }
            else
            {
                MessageBox.Show("There are no selected bars!");
            }
        }

        public void ApplyKeySignature()
        {
            KeySignature temp = Parameters.KeySignature;
            Parameters.KeySignature = LayoutController.StringToEnum<KeySignature>(keySignature+"Maj");
            geneticSong.ApplyKeySignature(keySignature);
            Parameters.KeySignature = temp;
            int defaultNumberElem = ScoreStaffs.defaultNumberElem;

            for (int i = 0; i < scores.Count;i++ )
            {
                ScoreStaffs ss = scores[i];
                Grid grid = (Grid)ss.borderStaff.Child;
                grid.Children.RemoveRange(defaultNumberElem, grid.Children.Count - defaultNumberElem);
                ss.setContent(geneticSong.songGenomes[i]);
                ss.AddTempoInfo(geneticSong.tempos[i].ToString());
                ss.AddIndexTag(i+1);
            }

            foreach (WrapPanel wp in keySignatures)
            {
                ((Image)wp.Children[0]).Source = LayoutController.GetKeySignature(keySignature, Clefs[0].ToString());
                ((Image)wp.Children[1]).Source = LayoutController.GetKeySignature(keySignature, Clefs[1].ToString());
            }

            MarkSavedScore(true);
        }

        private void bottomRectangle_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (verticalOffset < scoreScrollViewer.Height * Math.Ceiling((double)individPerPage / individPerRow))
                {
                    Thread.Sleep(20);
                    scoreScrollViewer.UpdateLayout();
                    verticalOffset += 10;
                    scoreScrollViewer.ScrollToVerticalOffset(verticalOffset);
                }
            }
            catch (Exception)
            {
            }
        }

        private void topRectangle_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (verticalOffset > 0)
                {
                    Thread.Sleep(20);
                    scoreScrollViewer.UpdateLayout();
                    verticalOffset -= 10;
                    scoreScrollViewer.ScrollToVerticalOffset(verticalOffset);
                }
            }
            catch (Exception)
            {
            }
        }

        private void AddEmptyBar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!playbackThread.IsBusy)
                {
                    addEmptyBar = true;
                    Genome gen = new Genome(0);
                    Octave[] octaves = { Octave.Octave5, Octave.Octave5 };
                    DurationCFugue dur = DurationCFugue.s;
                    byte[] iter = { 1, 1 };
                    string tempo = "120";
                    AddBar(gen, keySignature, octaves, dur, iter, tempo);

                    MarkSavedScore(true);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}