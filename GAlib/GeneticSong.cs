using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicNoteLib;

namespace GAlib
{
    /// <summary>
    /// A succession of genomes, which represent musical bars, composing a song.
    /// </summary>
   public class GeneticSong
    {
       string song = "";
       string part1 = "V0 ";
       string part2 = "V1 ";
       public List<int> tempos = new List<int>();
       public List<byte[]> songOctaves = new List<byte[]>();
       public List<DurationCFugue> songDurations = new List<DurationCFugue>();
       public List<byte[]> songBarIters = new List<byte[]>();

       public List<Genome> songGenomes = new List<Genome>();

       public string GetSong()
       {
          return song + part1 + part2;
       }

        public GeneticSong(string keySignature)
        {
            song = "K[" + keySignature + "Maj" + "] ";
            songGenomes = Population.ScaleChangeGenomes(songGenomes);
        }

        private void AddToParts(string tempo, Genome gen)
        {
            part1 += "T" + tempo + " " + gen.Parts[0];
            part2 += "T" + tempo + " " + gen.Parts[1];    
        }

        public void AddGenome(string tempo, Genome gen, byte[] octaves, DurationCFugue durCF, byte[] barIter, int pos)
        {
            // If the genome represents a non-empty bar, it is to be appended to the song.
            if (pos == -1)
            {
                tempos.Add(Convert.ToInt32(tempo));
                songOctaves.Add(octaves);
                songDurations.Add(durCF);
                songBarIters.Add(barIter);
                songGenomes.Add(gen);
                AddToParts(tempo, gen);
            }
                // Else, it is to be inserted in its right place.
            else
            {
                tempos.Insert(pos, Convert.ToInt32(tempo));
                songOctaves.Insert(pos, octaves);
                songDurations.Insert(pos, durCF);
                songBarIters.Insert(pos, barIter);
                songGenomes.Insert(pos, gen);
                part1 = "V0 ";
                part2 = "V1 ";
                for (int i = 0; i < songGenomes.Count; i++)
                    AddToParts(tempos[i].ToString(), songGenomes[i]);
            }            
        }
              
        public void DeleteGenome(List<int> delIndex)
        {           
            delIndex.Sort();

            for (int i = delIndex.Count - 1; i >= 0; i--)
            {
                songGenomes.RemoveAt(delIndex[i]);
                tempos.RemoveAt(delIndex[i]);
                songOctaves.RemoveAt(delIndex[i]);
                songDurations.RemoveAt(delIndex[i]);
                songBarIters.RemoveAt(delIndex[i]);
            }

            part1 = "V0 ";
            part2 = "V1 ";
            for (int i = 0; i < songGenomes.Count; i++)
                AddToParts(tempos[i].ToString(), songGenomes[i]);
        }

        private string SwapPart(int index1, int index2, int partIndex)
        {
            string partAux, part = "", partIndex1, partIndex2, restPart;
            int ind,  count=0, i, j;

            if(partIndex == 0)
                partAux = part1;
            else
                partAux = part2;

            for (i = 0; i < partAux.Length && count <= index1; i++)
            {
                if (partAux[i] == 'T')
                    count++;
            }
            index1 = i - 1;
            for (j = i; j < partAux.Length && count <= index2; j++)
            {
                if (partAux[j] == 'T')
                    count++;
            }
            index2 = j - 1;

            part = partAux.Substring(0, index1);
            partIndex1 = partAux.Substring(index1 + 1);
            partIndex2 = partAux.Substring(index2 + 1);

            ind = partIndex1.IndexOf('T');
            partAux = partIndex1.Substring(ind, index2 - (ind + index1 + 1));
            partIndex1 = "T" + partIndex1.Substring(0, ind);
            ind = partIndex2.IndexOf('T');
            if (ind != -1)
            {
                restPart = partIndex2.Substring(ind);
                partIndex2 = "T" + partIndex2.Substring(0, ind);
            }
            else
            {
                restPart = "";
                partIndex2 = "T" + partIndex2;
            }
            
            return part + partIndex2 + partAux + partIndex1 + restPart;
        }

        public void SwapGenomes(int index1, int index2)
        {
            int aux;
            DurationCFugue durAux;
            byte[] byteAux;

            if (index1 > index2)
            {
                aux = index1;
                index1 = index2;
                index2 = aux;
            }

            Genome gen1 = songGenomes[index1];
            Genome gen2 = songGenomes[index2];

            songGenomes.Remove(gen1);
            songGenomes.Remove(gen2);

            aux = tempos[index1];
            tempos[index1]=tempos[index2];
            tempos[index2] = aux;

            durAux = songDurations[index1];
            songDurations[index1] = songDurations[index2];
            songDurations[index2] = durAux;

            byteAux = songBarIters[index1];
            songBarIters[index1] = songBarIters[index2];
            songBarIters[index2] = byteAux;

            songGenomes.Insert(index1, gen2);
            songGenomes.Insert(index2, gen1);

            part1 = SwapPart(index1, index2, 0);
            part2 = SwapPart(index1, index2, 1); 
        }

        public void ClearGenome()
        {
            song = "";
            songGenomes.Clear();
        }

        public string TruncateSong(string partialSong, int index)
        {
            string part1Local, part2Local;
            part1Local = partialSong.Substring(partialSong.IndexOf("V0 "));
            string ks = partialSong.Substring(0, partialSong.Length - part1Local.Length);
            part2Local = part1Local.Substring(part1Local.IndexOf("V1 "));
            part1Local = part1Local.Substring(0, part1Local.Length - part2Local.Length);
            part1Local = part1Local.Substring(3);
            part2Local = part2Local.Substring(3);

            // Remove the first index bars from the genetic song.
            for (int i = 0; i < index; i++)
            {
                part1Local = part1Local.Substring(1);
                part1Local = part1Local.Substring(part1Local.IndexOf('T'));
                part2Local = part2Local.Substring(1);
                part2Local = part2Local.Substring(part2Local.IndexOf('T'));
            }
            return ks + "V0 " + part1Local + "V1 " + part2Local;
        }

        private string ModifyTempo(List<int> indexes, string newTempo, int partIndex)
        {
            string part, temp = "";
            int count = 0, l, i=-1;
            if (partIndex == 0)
                part = part1;
            else
                part = part2;
            temp = part;
            foreach (int ind in indexes)
            {
                while (i < part.Length && count <= ind)
                {
                    i++;
                    if (part[i] == 'T')
                        count++;
                }

                l = part.Substring(i, part.Substring(i).IndexOf(' ')).Length;
                part = part.Remove(i, l).Insert(i, "T" + newTempo);
                i  += newTempo.Length + 1;
            }
            return part;
        }

        public void ApplyTempo(List<int> indexes, string newTempo)
        {
            indexes.Sort();
            int t = Convert.ToInt32(newTempo);
            foreach (int ind in indexes)
                tempos[ind] = t;

            part1 = ModifyTempo(indexes, newTempo, 0);
            part2 = ModifyTempo(indexes, newTempo, 1);
        }

        public void ApplyKeySignature(string newKS)
        {
            song = "K[" + newKS + "Maj" + "] ";
            songGenomes=Population.ScaleChangeGenomes(songGenomes);
            part1 = "V0 ";
            part2 = "V1 ";
            for (int i = 0; i < songGenomes.Count; i++)
                AddToParts(tempos[i].ToString(), songGenomes[i]);
        }

        public bool IsEmpty(int index)
        {
            if (index != -1)
            {
                return songGenomes[index].IsEmpty();
            }
            return false;
        }

        public void FillEmptyBar(string tempo, Genome gen, byte[] octaves, DurationCFugue durCF, byte[] barIter, int pos)
        {
            tempos[pos] = Convert.ToInt32(tempo);
            songOctaves[pos] = octaves;
            songDurations[pos] = durCF;
            songBarIters[pos] = barIter;
            songGenomes[pos] = gen;
            part1 = "V0 ";
            part2 = "V1 ";
            for (int i = 0; i < songGenomes.Count; i++)
                AddToParts(tempos[i].ToString(), songGenomes[i]);
        }
    }

}
