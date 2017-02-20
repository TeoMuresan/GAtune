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
using System.IO;
using System.ComponentModel;
using GAlib;
using MusicNoteLib;
using WPF.MDI;
using GAtune.Controllers;
using GAtune.UserControls;

namespace GAtune
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        public static byte fieldCount = 0;
        public static byte scoreCount = 0;        
        private string originalTitle;
        public const string fieldIdentifier = "Field";
        public const string scoreIdentifier = "Score";        
        public static Button playInProgress = null;        
        private static bool isCancelled = false;                
        private static readonly byte[,] partParamsLayoutIndexes = { { 7, 8, 9, 10, 11 }, { 13, 14, 15, 16, 17 } };

        public static List<string> mdiChildIdentifier = new List<string>();
        public static Population pop = new Population();
        // The initial windows are Part Options and Player.
        public const byte initialChildCount = 2;
        // The index of the last focused field/score window.
        public static int lastFocusedIndex = initialChildCount - 1;
        public static List<string> savedFieldsAndScores = new List<string>();
        public static List<int> savedIndexes = new List<int>();
        public static List<string> openedValues = new List<string>();        

        void WindowContainer_MdiChildTitleChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild activeMdiChild = WindowContainer.ActiveMdiChild;
                if (activeMdiChild != null && activeMdiChild.WindowState == WindowState.Maximized)
                    Title = originalTitle + " - [" + activeMdiChild.Title + "]";
                else
                    Title = originalTitle;
            }
            catch (Exception)
            {
            }
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                originalTitle = Title;
                WindowContainer.MdiChildTitleChanged += WindowContainer_MdiChildTitleChanged;

            }
            catch (Exception)
            {
            } 
        }

        private MdiChild FieldWindowLayout()
        {
            MdiChild child = new MdiChild();
            child.Name = fieldIdentifier;
            child.Title = "Field " + fieldCount + "." + 1;
            child.ToolTip = child.Title;
            child.Position = new Point(0, 0);
            child.Width = double.NaN;
            child.Height = double.NaN;
            child.Background = Brushes.White;
            return child;
        }

        /// <summary>
        /// Removes the FIELD/SCORE window, previously saved, specified by the given MdiChild index.
        /// </summary>
        /// <param name="mdiChildIndex"></param>
        private void RemoveSavedItemWindow(int mdiChildIndex, int savedIndex)
        {
            mdiChildIdentifier.RemoveAt(mdiChildIndex);
            savedFieldsAndScores.RemoveAt(savedIndex);
            savedIndexes.RemoveAt(savedIndex);
        }

        private void AddFieldWindowClosingEventHandler(MdiChild child, FieldWindow field)
        {
            child.Closing += delegate(object sender, RoutedEventArgs e)
            {
                lastFocusedIndex = initialChildCount - 1;
                int mdiChildIndex = mdiChildIdentifier.IndexOf(child.Title.Substring(child.Title.IndexOf(" ") + 1).Replace('.', '_'));
                int index = WindowContainer.Children.IndexOf(child);
                int savedIndex = savedIndexes.IndexOf(index);
                // If the FIELD window that's closing has been saved before.
                if (savedIndex != -1)
                {
                    string name = savedFieldsAndScores[savedIndex];
                    if (name.Contains('|'))
                    {
                        child.Focus();
                        MessageBoxResult result = MessageBox.Show("Do you want to save changes to " +
                            name.Substring(0, name.Length - 1) + "?",
                            "GAtune", MessageBoxButton.YesNoCancel);
                        if (result == MessageBoxResult.Yes)
                        {
                            name = name.Substring(0, name.Length - 1);
                            WriteToFieldSavedFile(name, field);
                            RemoveSavedItemWindow(mdiChildIndex, savedIndex);
                            fieldCount--;
                        }
                        else if (result == MessageBoxResult.Cancel)
                        {
                            ClosingEventArgs eventArgs = (ClosingEventArgs)e;
                            eventArgs.Cancel = true;
                            isCancelled = true;
                            lastFocusedIndex = index;
                        }
                        else // result = NO.
                        {
                            RemoveSavedItemWindow(mdiChildIndex, savedIndex);
                            fieldCount--;
                        }
                    }
                    else
                    {
                        RemoveSavedItemWindow(mdiChildIndex, savedIndex);
                        fieldCount--;
                    }
                }
                else
                {
                    if (mdiChildIndex != -1)
                        mdiChildIdentifier.RemoveAt(mdiChildIndex);
                }
            };
        }

        private MdiChild CreateFieldWindow(FieldWindow field)
        {
            MdiChild child = FieldWindowLayout();
            child.Content = field;

            child.GotFocus += delegate
            {
                MainWindow.lastFocusedIndex = WindowContainer.Children.IndexOf(child);
            };
            child.LostFocus += field.ChildWindow_LostFocus;            
            AddFieldWindowClosingEventHandler(child, field);

            return child;
        }

        private void InitializeOpenedFieldWindowParams(ref FieldWindow field)
        {            
            field.KeySignatureAccidental = openedValues[0];
            field.Tempo = openedValues[1];
            field.Octave0 = LayoutController.StringToEnum<Octave>(openedValues[2]);
            field.Octave1 = LayoutController.StringToEnum<Octave>(openedValues[3]);            
            field.BarIteration0 = Convert.ToByte(openedValues[4]);
            field.BarIteration1 = Convert.ToByte(openedValues[5]);
            field.Duration = LayoutController.StringToEnum<DurationCFugue>(openedValues[6]);
        }

        private void NewField_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                if (WindowContainer.Children.Count == initialChildCount)
                    fieldCount = 1;
                else
                    fieldCount++;

                FieldWindow field;
                // If the new field is a brand new one.
                if (mainNewField.Tag == null)
                {
                    pop.RandomPop(Parameters.PopSize);
                    field = new FieldWindow(pop.Genomes);                    
                }
                    // If the new field has been previously saved and it's opened by the user now.
                else
                {
                    byte beat = Parameters.GetBeat(openedValues[6]);                                        
                    ScoreStaffs.unitElementWidth = ScoreStaffs.thisWidth / beat;
                    
                    pop.Genomes = (List<Genome>)mainNewField.Tag;
                    field = new FieldWindow(pop.Genomes);
                    InitializeOpenedFieldWindowParams(ref field);

                    openedValues.Clear();
                    mainNewField.Tag = null;                    
                }

                mdiChildIdentifier.Add(fieldCount + "_1");

                // Generation count for each new field yielded on this button click.
                field.Tag = 1;
                ((Button)field.fieldGrid.FindName("thisButton")).Name = "thisButton_" + MainWindow.fieldCount + "_" + 1;

                field.ResetPreservedGenomes();

                MdiChild child = CreateFieldWindow(field);                
                WindowContainer.Children.Add(child);
            }
            catch (Exception)
            {
            }
        }

        private MdiChild ScoreWindowLayout(byte scoreCount)
        {
            MdiChild child = new MdiChild();
            child.Name = scoreIdentifier;//"Score" + scoreCount;
            child.Position = new Point(0, 0);
            child.Width = double.NaN;
            child.Height = double.NaN;
            child.Background = Brushes.White;
            child.Resizable = false;
            return child;
        }

        private void AddScoreWindowGotFocusEventHandler(MdiChild child)
        {
            child.GotFocus += delegate
            {
                MainWindow.lastFocusedIndex = WindowContainer.Children.IndexOf(child);
            };
        }

        private void AddScoreWindowLostFocusEventHandler(MdiChild child, ScoreWindow sw)
        {
            child.LostFocus += delegate
            {
                if (sw.playbackThread.IsBusy)
                    playInProgress = sw.Pause;
            };
        }

        private void AddScoreWindowClosingEventHandler(MdiChild child, ScoreWindow sw)
        {
            child.Closing += delegate(object sender, RoutedEventArgs e)
            {
                if (sw.playbackThread.IsBusy)
                    sw.playbackThread.CancelAsync();
                lastFocusedIndex = initialChildCount - 1;
                int mdiChildIndex = mdiChildIdentifier.IndexOf(sw.Tag.ToString());
                int index = WindowContainer.Children.IndexOf(child);
                int savedIndex = savedIndexes.IndexOf(index);
                // If the SCORE window that's closing has been saved before.
                if (savedIndex != -1)
                {
                    string name = savedFieldsAndScores[savedIndex];
                    if (name.Contains('|'))
                    {
                        child.Focus();
                        MessageBoxResult result = MessageBox.Show("Do you want to save changes to " +
                            name.Substring(0, name.Length - 1) + "?",
                            "GAtune", MessageBoxButton.YesNoCancel);
                        if (result == MessageBoxResult.Yes)
                        {
                            name = name.Substring(0, name.Length - 1);
                            WriteToScoreSavedFile(name, sw);
                            RemoveSavedItemWindow(mdiChildIndex, savedIndex);
                            scoreCount--;
                        }
                        else if (result == MessageBoxResult.Cancel)
                        {
                            ClosingEventArgs eventArgs = (ClosingEventArgs)e;
                            eventArgs.Cancel = true;
                            isCancelled = true;
                            lastFocusedIndex = index;
                        }
                        else // result = NO.
                        {
                            RemoveSavedItemWindow(mdiChildIndex, savedIndex);
                            scoreCount--;
                        }
                    }
                    else
                    {
                        RemoveSavedItemWindow(mdiChildIndex, savedIndex);
                        scoreCount--;
                    }
                }
                else
                {
                    if (mdiChildIndex != -1)
                        mdiChildIdentifier.RemoveAt(mdiChildIndex);
                }
            };
        }

        private void NewScore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ScoreWindow sw = new ScoreWindow();

                scoreCount++;
                mdiChildIdentifier.Add(scoreCount.ToString());
                sw.Tag = scoreCount;

                MdiChild child = ScoreWindowLayout(scoreCount);
                WindowContainer.Children.Add(child);

                // If the new score is a brand new one.
                if (mainNewScore.Tag == null)
                {
                    child.Title = "Untitled";                    
                }
                // If the new score has been previously saved and it's opened by the user now.
                else
                {
                    int filePathIndex = openedValues.Count - 1;
                    string filePath = openedValues[filePathIndex];
                    child.Title = System.IO.Path.GetFileNameWithoutExtension(filePath);

                    if (mainNewScore.Tag.ToString() != "0")
                    {
                        openedValues.RemoveAt(filePathIndex);
                        sw.OpenScore((List<Genome>)mainNewScore.Tag, openedValues);
                    }                                       
                    
                    openedValues.Clear();
                    mainNewScore.Tag = null;
                }
                child.ToolTip = child.Title;
                child.Content = sw;

                AddScoreWindowGotFocusEventHandler(child);
                AddScoreWindowLostFocusEventHandler(child, sw);
                AddScoreWindowClosingEventHandler(child, sw);
            }
            catch (Exception)
            {
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;
                int index = WindowContainer.Children.IndexOf(mc);
                // If the currently focused window is not Part Options or Player.
                if (index >= initialChildCount)
                {
                    int savedIndex = savedIndexes.IndexOf(index);
                    // If the focused window is a field window, reset it.
                    if (mc.Name == fieldIdentifier)
                    {
                        FieldWindow fw = (FieldWindow)mc.Content;
                        fw.ResetPopulation();
                    }
                    else
                    {
                        ScoreWindow sw = (ScoreWindow)mc.Content;
                        sw.ClearAll.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }

                }
            }
            catch (Exception)
            {
            }
        }

        private Microsoft.Win32.SaveFileDialog CreateSaveFileDialog(string itemName)
        {
            Microsoft.Win32.SaveFileDialog saveAsDialog = new Microsoft.Win32.SaveFileDialog();
            saveAsDialog.CheckPathExists = true;
            if (itemName == fieldIdentifier)
            {
                saveAsDialog.DefaultExt = ".gene";
                saveAsDialog.Filter = "Gene files (.gene)|*.gene";
                saveAsDialog.FileName = "*.gene";
            }
            else
            {
                saveAsDialog.DefaultExt = ".score";
                saveAsDialog.Filter = "Score files (.score)|*.score";
                saveAsDialog.FileName = "*.score";
            }            

            return saveAsDialog;
        }

        /// <summary>
        /// Saves the contents of the specified window for the first time.
        /// </summary>
        /// <param name="mc"></param>
        /// <param name="index"> The index of the item to be saved.</param>
        private void SaveItemForFirstTime(MdiChild mc, int index, string filePath)
        {
            savedFieldsAndScores.Add(filePath);
            savedIndexes.Add(index);

            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            fileStream.Close();

            string savedName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            // If the last focused window is a field.
            if (mc.Name == fieldIdentifier)
            {
                FieldWindow fw = (FieldWindow)mc.Content;
                WriteToFieldSavedFile(filePath, fw);
            }
            else
            {
                ScoreWindow sw = (ScoreWindow)mc.Content;
                WriteToScoreSavedFile(filePath, sw);
                mc.Title = savedName;
            }
            mc.ToolTip = savedName;
        }

        /// <summary>
        /// Saves the contents of the specified window, given it's not the first save.
        /// </summary>
        /// <param name="mc"></param>
        private void SavePrevSavedItem(MdiChild mc, int savedIndex)
        {
            string itemName = savedFieldsAndScores[savedIndex];
            // If there have been modifications since the last save.
            if (itemName.Contains('|'))
            {
                savedFieldsAndScores[savedIndex] = itemName.Remove(itemName.Length - 1);
                itemName = savedFieldsAndScores[savedIndex];
                // If the last focused window is a field, update its part bar iteration accordingly.
                if (mc.Name == fieldIdentifier)
                {
                    FieldWindow fw = (FieldWindow)mc.Content;
                    WriteToFieldSavedFile(itemName, fw);
                }
                else
                {
                    ScoreWindow sw = (ScoreWindow)mc.Content;
                    WriteToScoreSavedFile(itemName, sw);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;
                int index = WindowContainer.Children.IndexOf(mc);
                // If the currently focused window is not Part Options or Player.
                if (index >= initialChildCount)
                {
                    int savedIndex = savedIndexes.IndexOf(index);
                    // If this is the first time the field/score is saved.
                    if (savedIndex == -1)
                    {
                        Microsoft.Win32.SaveFileDialog saveAsDialog = CreateSaveFileDialog(mc.Name);

                        if (saveAsDialog.ShowDialog() == true)
                        {
                            SaveItemForFirstTime(mc, index, saveAsDialog.FileName);
                        }
                    }
                    // If the field/score has been saved before.
                    else
                    {
                        SavePrevSavedItem(mc, savedIndex);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                MdiChild mc = WindowContainer.ActiveMdiChild;
                int index = WindowContainer.Children.IndexOf(mc);
                // If the currently focused window is not Part Options or Player.
                if (index >= initialChildCount)
                {
                    int savedIndex = savedIndexes.IndexOf(index);

                    Microsoft.Win32.SaveFileDialog saveAsDialog = CreateSaveFileDialog(mc.Name);
                    string path = string.Empty;
                    bool isItemModified = false;
                    // If the item has been previously saved.
                    if (savedIndex != -1)
                    {
                        path = savedFieldsAndScores[savedIndex];
                        // If there have been modifications since the last save, remove the mark to make the path valid.
                        if (path.Contains('|'))
                        {
                            isItemModified = true;
                            path = path.Remove(path.Length - 1);
                            savedFieldsAndScores[savedIndex] = path;
                        }
                        saveAsDialog.FileName = System.IO.Path.GetFileName(path);
                    }

                    if (saveAsDialog.ShowDialog() == true)
                    {
                        SaveItemForFirstTime(mc, index, saveAsDialog.FileName);
                    }
                    // If the user cancelled.
                    else
                    {
                        // If the item has been previously saved and there are currently unsaved modifications, mark the item.
                        if (isItemModified)
                        {
                            path += "|";
                            savedFieldsAndScores[savedIndex] = path;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void WriteToFieldSavedFile(string name, FieldWindow fw)
        {
            using (TextWriter writer = new StreamWriter(name))
            {
                writer.WriteLine(fw.KeySignatureAccidental);
                writer.WriteLine(fw.Tempo);
                writer.WriteLine(fw.Octave0);
                writer.WriteLine(fw.Octave1);
                writer.WriteLine(fw.BarIteration0);
                writer.WriteLine(fw.BarIteration1);
                writer.WriteLine(fw.Duration);

                List<Genome> fieldIndviduals = fw.individuals;
                byte numParts = Parameters.NumParts;
                foreach (Genome x in fieldIndviduals)
                    for (int j = 0; j < numParts; j++)
                    {
                        foreach (string s in x.rhythmChromosome[j])
                            writer.Write(s + " ");
                        writer.WriteLine();
                        foreach (int i in x.pitchChromosome[j])
                            writer.Write(i + " ");
                        writer.WriteLine();
                        foreach (byte b in x.velocityChromosome[j])
                            writer.Write(b + " ");
                        writer.WriteLine();
                    }
            }
        }

        public static void WriteToScoreSavedFile(string name, ScoreWindow sw)
        {            
            GeneticSong song = sw.geneticSong;
            if (song != null)
            {
                using (TextWriter writer = new StreamWriter(name))
                {
                    writer.WriteLine(sw.keySignature);

                    int temposCount = song.tempos.Count;
                    byte numParts = Parameters.NumParts;
                    List<Genome> songGenomes = song.songGenomes;
                    for (int i = 0; i < temposCount; i++)
                    {
                        writer.WriteLine(song.tempos[i]);
                        writer.WriteLine(song.songOctaves[i][0]);
                        writer.WriteLine(song.songOctaves[i][1]);
                        writer.WriteLine(song.songBarIters[i][0]);
                        writer.WriteLine(song.songBarIters[i][1]);
                        writer.WriteLine(song.songDurations[i]);

                        for (int j = 0; j < numParts; j++)
                        {
                            Genome genome = songGenomes[i];
                            foreach (string s in genome.rhythmChromosome[j])
                                writer.Write(s + " ");
                            writer.WriteLine();
                            foreach (int k in genome.pitchChromosome[j])
                                writer.Write(k + " ");
                            writer.WriteLine();
                            foreach (byte b in genome.velocityChromosome[j])
                                writer.Write(b + " ");
                            writer.WriteLine();
                        }
                    }
                }
            }            
        }

        /// <summary>
        /// Counts the number of lines in the specified file.
        /// </summary>
        /// <param name="filePath"> The file path.</param>
        /// <returns> The number of lines in the file.</returns>
        private int CountLinesInFile(string filePath)
        {
            int count = 0;
            using (StreamReader r = new StreamReader(filePath))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    count++;
                }
            }
            return count;
        }

        private void ReadFromFieldSavedFile(string name)
        {
            List<List<string[]>> openedParts = new List<List<string[]>>();
            int lineCount = CountLinesInFile(name);
            int numPartsTimes3 = Parameters.NumParts * 3;
            int pmCount = 7;
            int validLineCount = pmCount + numPartsTimes3 * Parameters.PopSize;
            
            if (lineCount != validLineCount)
            {
                throw new Exception("The file you're trying to open is missing data.");
            }
            else
            {
                using (TextReader reader = new StreamReader(name))
                {
                    for (int j = 0; j < pmCount; j++)
                    {
                        openedValues.Add(reader.ReadLine());
                    }

                    for (int i = pmCount; i < lineCount; i += numPartsTimes3)
                    {
                        List<string[]> list = new List<string[]>();
                        for (int j = 0; j < numPartsTimes3; j++)
                        {
                            list.Add(reader.ReadLine().TrimEnd().Split(' '));
                        }

                        openedParts.Add(list);
                    }
                }

                try
                {                    
                    mainNewField.Tag = pop.OpenFieldGenomes(openedParts, openedValues);
                    mainNewField.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        private void ReadFromScoreSavedFile(string name)
        {
            List<List<string[]>> openedParts = new List<List<string[]>>();
            int lineCount = CountLinesInFile(name);
            int numPartsTimes3 = Parameters.NumParts * 3;
            // The key signature appears once, the rest of the params appear for each bar.
            int pmCount = 6;
            int barInfoLineCount = pmCount + numPartsTimes3;

            // If the saved score has no bars.
            if (lineCount == 0)
            {
                mainNewScore.Tag = 0;
            }
            else
            {
                // If there's potentially incomplete info for a bar.
                if (lineCount == 1 || (lineCount - 1) % barInfoLineCount != 0)
                {
                    throw new Exception("The file you're trying to open is missing data.");
                }
                else
                {
                    using (TextReader reader = new StreamReader(name))
                    {
                        // Read the key signature parameter.
                        openedValues.Add(reader.ReadLine());                    

                        for (int i = 1; i < lineCount; i += barInfoLineCount)
                        {
                            for (int j = 0; j < pmCount; j++)
                            {
                                openedValues.Add(reader.ReadLine());
                            }                            

                            List<string[]> list = new List<string[]>();
                            for (int j = 0; j < numPartsTimes3; j++)
                                list.Add(reader.ReadLine().TrimEnd().Split(' '));

                            openedParts.Add(list);
                        }
                    }

                    try
                    {
                        mainNewScore.Tag = pop.OpenScoreGenomes(openedParts, openedValues);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }

            openedValues.Add(name);
            mainNewScore.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
                openDialog.DefaultExt = ".gene";
                openDialog.CheckPathExists = true;
                openDialog.Filter = "Gene files (.gene)|*.gene|Score files (.score)|*.score";

                if (openDialog.ShowDialog() == true)
                {
                    string name = openDialog.FileName;

                    if (name.Contains("gene"))
                        ReadFromFieldSavedFile(name);
                    else
                        ReadFromScoreSavedFile(name);
                    
                    savedFieldsAndScores.Add(name);
                    int index = WindowContainer.Children.Count - 1;
                    savedIndexes.Add(index);
                }
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void CloseAllWindows_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isCancelled = false;
                // If there's at least one other window opened, besides the Part Options and Player windows.
                if (WindowContainer.Children.Count != initialChildCount)
                {
                    lastFocusedIndex = initialChildCount - 1;

                    for (int i = WindowContainer.Children.Count - 1; i >= initialChildCount && !isCancelled; i--)
                    {
                        MdiChild mc = (MdiChild)WindowContainer.Children[i];
                        int savedIndex = savedIndexes.IndexOf(i);
                        // If the window is supposed to be saved.
                        if (savedIndex != -1)
                        {
                            // If the window contents have been modified since the last change, prompt the user.
                            if (savedFieldsAndScores[savedIndex].Contains('|'))
                                mc.OnClosing();
                        }
                        else
                            mc.Close();
                    }

                    if (!isCancelled)
                    {
                        fieldCount = 0;
                        scoreCount = 0;
                        mdiChildIdentifier.Clear();
                        savedFieldsAndScores.Clear();
                        savedIndexes.Clear();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;
                string name = mc.Name;
                // Part Options and Player can't be closed, only hidden.
                if (name == fieldIdentifier || name == scoreIdentifier)
                {
                    mc.OnClosing();
                }
            }
            catch (Exception)
            {
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NextThis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;
                int index = WindowContainer.Children.IndexOf(mc);
                // If the currently focused window is a field.
                if (mc.Name == fieldIdentifier)              
                {
                    FieldWindow fw = (FieldWindow)mc.Content;
                    fw.thisButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));                    
                }
            }
            catch (Exception)
            {
            }
        }

        private void NextNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;                
                // If the currently focused window is a field.
                if (mc.Name == fieldIdentifier)               
                {                    
                    FieldWindow fw = (FieldWindow)mc.Content;
                    fw.newButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
            catch (Exception)
            {
            }
        }

        private void MutateIt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;
                int index = WindowContainer.Children.IndexOf(mc);
                // If the currently focused window is a field.
                if (mc.Name == fieldIdentifier)
                {                    
                    FieldWindow fw = (FieldWindow)mc.Content;
                    fw.MutateSelectedGenomes();                              
                }
            }
            catch (Exception)
            {
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;
                int index = WindowContainer.Children.IndexOf(mc);
                // If the currently focused window is not Part Options or Player.
                if (index >= initialChildCount)
                {                    
                    if (mc.Name == fieldIdentifier)
                    {
                        FieldWindow fw = (FieldWindow)mc.Content;
                        fw.SelectAllField();
                    }
                    else
                    {
                        ScoreWindow sw = (ScoreWindow)mc.Content;
                        sw.SelectAllScore();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void partOptionsWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lastFocusedIndex >= initialChildCount)
                {
                    MdiChild mc = WindowContainer.Children[lastFocusedIndex];
                    // If the last focused window is a field, set Part Options' params to this field's params' values.
                    if (mc.Name == fieldIdentifier)
                    {
                        FieldWindow fw = (FieldWindow)mc.Content;
                        byte octave0 = (byte)((byte)fw.Octave0 - 3);
                        byte octave1 = (byte)((byte)fw.Octave1 - 3);
                        DurationCFugue duration = fw.Duration;
                        byte barIter0 = fw.BarIteration0;
                        byte barIter1 = fw.BarIteration1;

                        Border b;
                        b = (Border)partOptionsGrid.Children[partParamsLayoutIndexes[0, 0]];
                        ((ComboBox)b.Child).SelectedIndex = octave0;

                        b = (Border)partOptionsGrid.Children[partParamsLayoutIndexes[1, 0]];
                        ((ComboBox)b.Child).SelectedIndex = octave1;

                        b = (Border)partOptionsGrid.Children[partParamsLayoutIndexes[0, 1]];
                        ((ComboBox)b.Child).SelectedIndex = (byte)duration;

                        b = (Border)partOptionsGrid.Children[partParamsLayoutIndexes[1, 1]];

                        b = (Border)partOptionsGrid.Children[partParamsLayoutIndexes[0, 2]];
                        ((ComboBox)b.Child).SelectedIndex = (int)barIter0 / 2;

                        b = (Border)partOptionsGrid.Children[partParamsLayoutIndexes[1, 2]];
                        ((ComboBox)b.Child).SelectedIndex = (int)barIter1 / 2;                        
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void PartOptions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                partOptionsWindow.Position = new Point(1000, 0);
                partOptionsWindow.Visibility = Visibility.Visible;
                partOptionsWindow.Focus();
            }
            catch (Exception)
            {
            }
        }

        private void OctaveComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cb = (ComboBox)sender;
                int partIndex = Convert.ToInt32(cb.Tag);
                int index = (int)cb.SelectedIndex;                
                Octave partOctave = LayoutController.StringToEnum<Octave>("Octave" + (index + 3));

                MdiChild mc = WindowContainer.Children[lastFocusedIndex];
                // If the last focused window is a field, update its part octave accordingly.
                if (mc.Name == fieldIdentifier)
                {
                    FieldWindow fw = (FieldWindow)mc.Content;
                    if (partIndex == 0)
                        fw.Octave0 = partOctave;
                    else
                        fw.Octave1 = partOctave;
                    fw.UpdateFieldOctave((byte)partIndex);
                }
                // Else, set the part octave for the future fields. 
                else
                {                   
                    Parameters.Octaves[partIndex] = partOctave;
                    Parameters.octavesByteValues[partIndex] = (byte)partOctave;
                    Parameters.Clefs[partIndex] = Scales.GetClef(partOctave);
                }                                                
            }
            catch (Exception)
            {
            }
        }

        private void UnitBeatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cb = (ComboBox)sender;
                int partIndex = Convert.ToInt32(cb.Tag);
                int index = (int)cb.SelectedIndex;

                Border b = (Border)partOptionsGrid.Children[partParamsLayoutIndexes[1 - partIndex, 1]];
                ((ComboBox)b.Child).SelectedIndex = (byte)index;

                MdiChild mc = WindowContainer.Children[lastFocusedIndex];
                // If the last focused window is a field, update its unit beat accordingly.
                if (mc.Name == fieldIdentifier)
                {
                    FieldWindow fw = (FieldWindow)mc.Content;
                    fw.Duration = (DurationCFugue)Enum.GetValues(typeof(DurationCFugue)).GetValue(index);
                    fw.UpdatePopulation();
                }
                // Else, set the unit beat for the future fields. 
                else
                {
                    Parameters.Duration = (DurationCFugue)Enum.GetValues(typeof(DurationCFugue)).GetValue(index);
                    ScoreStaffs.unitElementWidth = ScoreStaffs.thisWidth / Parameters.unitBeat;
                }
            }
            catch (Exception)
            {
            }
        }

        private void BarIterationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cb = (ComboBox)sender;
                int partIndex = Convert.ToInt32(cb.Tag);
                int index = (int)cb.SelectedIndex;

                MdiChild mc = WindowContainer.Children[lastFocusedIndex];
                // If the last focused window is a field, update its part bar iteration accordingly.
                if (mc.Name == fieldIdentifier)
                {
                    FieldWindow fw = (FieldWindow)mc.Content;
                    byte barIteration = (byte)Math.Pow(2, index);
                    if (partIndex == 0)
                        fw.BarIteration0 = barIteration;
                    else
                        fw.BarIteration1 = barIteration;

                    fw.UpdatePopulation();
                }
                // Else, set the part bar iteration for the future fields. 
                else
                {
                    Parameters.barIterate[partIndex] = (byte)Math.Pow(2, index);
                    Parameters.Duration = Parameters.Duration;
                }                
            }
            catch (Exception)
            {
            }
        }
        
        private void lockComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cb = (ComboBox)sender;
                int partIndex = Convert.ToInt32(cb.Tag);
                byte index = (byte)cb.SelectedIndex;
                Parameters.lockChrom[partIndex] = index;
            }
            catch (Exception)
            {
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button b = (Button)sender;
                int partIndex = Convert.ToInt32(b.Tag);

                if (partIndex == 0)
                {
                    part1OctaveComboBox.SelectedIndex = 2;
                    part1UnitBeatComboBox.SelectedIndex = 0;
                    part2UnitBeatComboBox.SelectedIndex = 0;
                    part1BarIterationComboBox.SelectedIndex = 0;
                    part1LockComboBox.SelectedIndex = 0;
                }
                else
                {
                    part2OctaveComboBox.SelectedIndex = 2;
                    part1UnitBeatComboBox.SelectedIndex = 0;
                    part2UnitBeatComboBox.SelectedIndex = 0;
                    part2BarIterationComboBox.SelectedIndex = 0;
                    part2LockComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {
            }
        }        

        private void playerWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lastFocusedIndex >= initialChildCount)
                {
                    MdiChild mc = WindowContainer.Children[lastFocusedIndex];
                    // If the last focused window is a field, set Player's params to this field's params' values.                                        
                    if (mc.Name == fieldIdentifier)
                    {
                        FieldWindow fw = (FieldWindow)mc.Content;
                        tempoValueLabel.Content = fw.Tempo;
                        scaleValueLabel.Content = fw.KeySignatureAccidental;
                        renderedImage.Source = LayoutController.GetPlayerKeySignatureImage(
                            (int)LayoutController.StringToEnum<KeySignatureAccidental>(fw.KeySignatureAccidental));
                    }
                    else
                    {
                        ScoreWindow sw = (ScoreWindow)mc.Content;
                        scaleValueLabel.Content = sw.keySignature;
                        renderedImage.Source = LayoutController.GetPlayerKeySignatureImage(
                            (int)LayoutController.StringToEnum<KeySignatureAccidental>(sw.keySignature));
                    }
                }                
            }
            catch (Exception)
            {
            }
        }

        private void Player_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                playerWindow.Position = new Point(1000, 175);
                playerWindow.Visibility = Visibility.Visible;
                playerWindow.Focus();
            }
            catch (Exception)
            {
            }
        }
        
        private void ApplyTempo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte currTempo = Parameters.Tempo;
                byte result;
                if (Byte.TryParse(tempoTextBox.Text, out result))
                {
                    Parameters.Tempo = result;
                    if (Parameters.Tempo != currTempo)
                    {
                        string newTempo = Parameters.Tempo.ToString();
                        tempoValueLabel.Content = newTempo;

                        // If there's at least one field/score window active.
                        if (lastFocusedIndex >= initialChildCount)
                        {
                            MdiChild mc = WindowContainer.Children[lastFocusedIndex];

                            // If the last focused window is a field, update its tempo accordingly.
                            if (mc.Name == fieldIdentifier)
                            {
                                FieldWindow fw = (FieldWindow)mc.Content;
                                fw.Tempo = newTempo;
                                int ind = WindowContainer.Children.IndexOf(mc);
                                int savedIndex = savedIndexes.IndexOf(ind);
                                MarkSaved(savedIndex);
                            }
                            else
                            {
                                // If the tempo is to be applied to a score window, set the tempo param to its old value
                                // so that the tempo for the future fields is not affected by this change.
                                Parameters.Tempo = currTempo;
                                if (playInProgress == null)
                                {
                                    ScoreWindow sw = (ScoreWindow)mc.Content;
                                    sw.ApplyTempo(newTempo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }        

        private void tempoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                byte value = (byte)e.NewValue;
                if (!tempoTextBox.IsFocused)
                    tempoTextBox.Text = value.ToString();
            }
            catch (Exception)
            {
            }
        }

        private void TempoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (tempoSlider != null && tempoTextBox.Text != "")
                    tempoSlider.Value = byte.Parse(tempoTextBox.Text);
            }
            catch (Exception)
            {
            }
        }

        private void ApplyScale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                KeySignature currKeySignature = Parameters.KeySignature;
                string scaleText = scaleTextBox.Text;
                KeySignature newKeySignature = LayoutController.StringToEnum<KeySignature>(scaleText + "Maj");
                if (currKeySignature != newKeySignature)
                {
                    Parameters.KeySignature = newKeySignature;
                    scaleValueLabel.Content = scaleText;
                    renderedImage.Source = LayoutController.GetPlayerKeySignatureImage((int)scaleSlider.Value);

                    // If there's at least one field/score window active.
                    if (lastFocusedIndex >= initialChildCount)
                    {
                        MdiChild mc = WindowContainer.Children[lastFocusedIndex];

                        // If the last focused window is a field, update its scale accordingly.
                        if (mc.Name == fieldIdentifier)
                        {
                            FieldWindow fw = (FieldWindow)mc.Content;
                            fw.KeySignatureAccidental = scaleText;
                            fw.ScaleChange();

                            int ind = WindowContainer.Children.IndexOf(mc);
                            int savedIndex = savedIndexes.IndexOf(ind);
                            MarkSaved(savedIndex);
                        }
                        else
                        {
                            // If the key signature is to be applied to a score window, set the key signature param to
                            // its old value so that the key signature for the future fields is not affected by this change.
                            Parameters.KeySignature = currKeySignature;
                            if (playInProgress == null)
                            {
                                ScoreWindow sw = (ScoreWindow)mc.Content;
                                sw.keySignature = scaleText;
                                sw.ApplyKeySignature();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }        

        private void scaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int value = (int)e.NewValue;
                scaleTextBox.Text = LayoutController.GetPlayerKeySignatureName(value);
            }
            catch (Exception)
            {
            }
        }        

        private void Shrink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;
                if (mc.Name == scoreIdentifier)
                {
                    ScoreWindow sw = (ScoreWindow)mc.Content;
                    sw.Shrink.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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
                MdiChild mc = WindowContainer.ActiveMdiChild;
                if (mc.Name == scoreIdentifier)
                {
                    ScoreWindow sw = (ScoreWindow)mc.Content;
                    sw.Expand.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
            catch (Exception)
            {
            }
        }

        private void ExportMIDI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MdiChild mc = WindowContainer.ActiveMdiChild;
                if (mc.Name == scoreIdentifier)
                {
                    if (playInProgress == null)
                    {
                        Microsoft.Win32.SaveFileDialog saveMIDIDialog = new Microsoft.Win32.SaveFileDialog();
                        saveMIDIDialog.CheckPathExists = true;
                        saveMIDIDialog.DefaultExt = ".midi";
                        saveMIDIDialog.Filter = "MIDI files (.midi)|*.midi";
                        saveMIDIDialog.FileName = "*.midi";

                        if (saveMIDIDialog.ShowDialog() == true)
                        {
                            ScoreWindow sw = (ScoreWindow)mc.Content;
                            MusicNoteLib.MusicNoteLib.SaveAsMidiFile(sw.geneticSong.GetSong(), saveMIDIDialog.FileName);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void GAtuneManual_Click(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start("\\GAtune manual.pdf");
            MessageBoxResult result = MessageBox.Show("Sorry, the manual is not available.",
                    "GAtune", MessageBoxButton.OK);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.N))
                mainNewField.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            else if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift) && (e.Key == Key.N))
                mainNewScore.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.O))
                mainOpen.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.S))
                mainSave.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            else if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift) && (e.Key == Key.S))
                mainSaveAs.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.M))
                mainExportMIDI.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.R))
                mainReset.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.A))
                mainSelectAll.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            else if ((Keyboard.Modifiers == ModifierKeys.Alt) && (e.Key == Key.F4))
                mainExit.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        /// <summary>
        /// Marks the field/window given by the specified name, if it's been saved/opened and then modified.
        /// </summary>
        /// <param name="name"></param>
        public static void MarkSaved(string name)
        {
            int nameIndex = mdiChildIdentifier.IndexOf(name);
            MdiChild mc = (MdiChild)windowContainer.Children[nameIndex + initialChildCount];
            int index = windowContainer.Children.IndexOf(mc);
            int savedIndex = savedIndexes.IndexOf(index);

            if (savedIndex != -1 && !savedFieldsAndScores[savedIndex].Contains('|'))
            {
                savedFieldsAndScores[savedIndex] += "|";
            }
        }

        /// <summary>
        /// Marks the field/window given by the specified index, if it's been saved/opened and then modified.
        /// </summary>
        /// <param name="savedIndex"></param>
        private static void MarkSaved(int savedIndex)
        {
            if (savedIndex != -1 && !savedFieldsAndScores[savedIndex].Contains('|'))
            {
                savedFieldsAndScores[savedIndex] += "|";
            }
        }

        private static MdiContainer windowContainer = new MdiContainer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainCloseAllWindows.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));                        
            if (isCancelled)
            {
                e.Cancel = true;
            }
        }
    }
}