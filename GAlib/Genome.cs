using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicNoteLib;


namespace GAlib
{
    /// <summary>
    /// A genome represents an individual.
    /// Its genotype represents the encoded individual. The genotype consists of chromosomes, which encode
    ///     the necessary information of the individual.
    /// Its phenotype represents the decoded individual.
    /// Our genome comprises 3 chromosome types: rhythm, pitch and velocity.
    /// E.g. in the genotype, a gene in the pitch chromosome is 4 (encodes the 5th note in a 12-semitone scale);
    ///      in the phenotype, that gene is 88 (the MIDI value of the note).
    /// </summary>
    public class Genome
    {
        // The following chromosomes have a fixed length (the maximum unitBeat i.e. 16) and
        // a fixed structure (16 random-generated values initially, values resulting from crossover and mutation subsequently).
        // They are used when changing the unitBeat parameter (from a lower to a higher value) and
        // when changing the barIterate parameter (from a higher to a lower value).
        public List<string[]> rhythmChromosome
        {
            get;
            set;
        }

        public List<int[]> pitchChromosome
        {
            get;
            set;
        }

        public List<byte[]> velocityChromosome
        {
            get;
            set;
        }

        // The following chromosomes have a variable length (i.e. the current value of unitBeat) and
        // a variable structure (depending on the current value of barIterate).
        // They are used when decoding the genotype.
        public List<string[]> currentRhythmChromosome
        {
            get;
            set;
        }

        public List<int[]> currentPitchChromosome
        {
            get;
            set;
        }

        public List<byte[]> currentVelocityChromosome
        {
            get;
            set;
        }

        private string[] parts = new string[Parameters.NumParts * 2];
        public string[] Parts
        {
            get
            {
                return parts;
            }
            set
            {
                parts = value;
            }
        }        

        /// <summary>
        /// Creates a genome representing a bar which consists of a whole rest.
        /// </summary>
        /// <param name="empty"></param>
        public Genome(int empty)
        {
            this.rhythmChromosome = Chromosome.WholeRestRhythmChrom();
            this.pitchChromosome = Chromosome.WholeRestPitchChrom();
            this.velocityChromosome = Chromosome.WholeRestVelocityChrom();

            GenotypeDecode();
        }

        public bool IsEmpty()
        {
            if (!this.parts[0].Contains('['))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Initializes a genome.
        /// </summary>
        public Genome()
        {
            this.rhythmChromosome = new List<string[]>();
            this.pitchChromosome = new List<int[]>();
            this.velocityChromosome = new List<byte[]>();
        }

        /// <summary>
        /// Creates a random genome with the 3 types of chromosomes.
        /// </summary>
        public Genome(Random random)
        {
            Chromosome chrom = new Chromosome(random);

            this.rhythmChromosome = chrom.RhythmChrom();
            this.pitchChromosome = chrom.PitchChrom();
            this.velocityChromosome = chrom.VelocityChrom();

            GenotypeDecode();
        }

        /// <summary>
        /// Creates a genome that is a replica of the given genome.
        /// </summary>
        /// <param name="genome"></param>
        public Genome(Genome genome)
        {
            byte maxUnitBeat = Parameters.maxUnitBeat;

            this.rhythmChromosome = new List<string[]>();
            this.pitchChromosome = new List<int[]>();
            this.velocityChromosome = new List<byte[]>();
            
            string[] rhythm1_resGen1 = new string[maxUnitBeat];
            string[] rhythm2_resGen1 = new string[maxUnitBeat];

            int[] pitch1_resGen1 = new int[maxUnitBeat];
            int[] pitch2_resGen1 = new int[maxUnitBeat];

            byte[] velocity1_resGen1 = new byte[maxUnitBeat];
            byte[] velocity2_resGen1 = new byte[maxUnitBeat];

            Array.Copy(genome.rhythmChromosome[0], 0, rhythm1_resGen1, 0, maxUnitBeat);
            Array.Copy(genome.rhythmChromosome[1], 0, rhythm2_resGen1, 0, maxUnitBeat);

            Array.Copy(genome.pitchChromosome[0], 0, pitch1_resGen1, 0, maxUnitBeat);
            Array.Copy(genome.pitchChromosome[1], 0, pitch2_resGen1, 0, maxUnitBeat);

            Array.Copy(genome.velocityChromosome[0], 0, velocity1_resGen1, 0, maxUnitBeat);
            Array.Copy(genome.velocityChromosome[1], 0, velocity2_resGen1, 0, maxUnitBeat);

            this.rhythmChromosome.Add(rhythm1_resGen1);
            this.rhythmChromosome.Add(rhythm2_resGen1);

            this.pitchChromosome.Add(pitch1_resGen1);
            this.pitchChromosome.Add(pitch2_resGen1);

            this.velocityChromosome.Add(velocity1_resGen1);
            this.velocityChromosome.Add(velocity2_resGen1);

            GenotypeDecode();
        }

        /// <summary>
        /// c: continuation of the previous note or rest;
        /// r: rest;
        /// p: play note.
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        private char RhythmDecode(string gene)
        {
            char res;
            if (gene[0] == '1')
                res = 'c';
            else if (gene.Substring(0, 3).Equals("011") == true)
                res = 'r';
            else
                res = 'p';
            return res;
        }

        /// <summary>
        /// Returns the Midi value of a note for the first part (solo), calculated depending on 
        /// the gene pitch, the octave, the scale and the notes interval.
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        private byte PitchSolo1Decode(int gene, byte fP)
        {
            byte res;
            byte supMidiNote = Parameters.supMidiNote;
            res = (byte)(fP + gene);            
            if (res >= supMidiNote)
                res = (byte)(fP + res % (supMidiNote - fP));
                
            return res;
        }

        /// <summary>
        /// Returns the Midi value of a note for the second part (solo), calculated depending on 
        /// the gene pitch, the octaves, the scale and the notes interval.        
        /// </summary>
        /// <param name="gene"></param>
        /// <param name="geneMidi"> The Midi value for the note in the first part.</param>
        /// <returns></returns>
        private byte PitchSolo2Decode(int gene, byte geneMidi, byte fP)
        {
            int res;
            byte geneMapped;
            if (Parameters.octavesByteValues[0] >= Parameters.octavesByteValues[1])
                geneMapped = (byte)(geneMidi - 12 * (Parameters.octavesByteValues[0] - Parameters.octavesByteValues[1]));
            else
                geneMapped = (byte)(geneMidi + 12 * (Parameters.octavesByteValues[1] - Parameters.octavesByteValues[0]));

            res = geneMapped;
            byte var= (byte)Math.Abs(gene);
            if (var == 1)           // major third (4 half steps)
                res += 4 * gene / var;
            else if (var == 2)       // perfect fourth (5 half steps)
                res += 5 * gene / var;
            else if (var == 3)        // perfect fifth (7 half steps)
                res += 7 * gene / var;

            if (res <= Parameters.infMidiNote)  // if the note is below the minimum value
                res = res + 12;
            if (res >= Parameters.supMidiNote)   // if the note is above the maximum value
                res = (byte)(fP + res % (Parameters.supMidiNote - fP));            
            if (res - fP > 11)
                res = res - 12;
            if (res < fP)
                res = res + 12;

            return (byte)res;
        }

        /// <summary>
        /// Returns the Midi value of the velocity correspondent to the gene parameter.
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        private byte VelocityDecode(byte gene)
        {
            return (byte)(Parameters.infVelocity + (gene * Parameters.stepVelocity));
        }
        
        /// <summary>
        /// Returns the octave for the given note.
        /// E.g. GbMaj in octave 6 starts with G6 but Cn is in octave 7 (i.e. GbMaj spans 2 octaves).
        /// </summary>
        /// <param name="noteIndex"></param>
        /// <param name="partIndex"></param>
        /// <returns></returns>
        private byte ActualOctave(int noteIndex, byte partIndex)
        {
            byte result = Parameters.octavesByteValues[partIndex];
            if (noteIndex >= Scales.changeOctave[Parameters.keySignatureIndex])
                result++;
            return result;
        }

        /// <summary>
        /// Initializes the chromosomes based on the bar iteration parameter value.
        /// </summary>
        /// <param name="rhythmSoloPart"></param>
        /// <param name="pitchSoloPart"></param>
        /// <param name="velocitySoloPart"></param>
        /// <param name="part"></param>
        private void UpdateCurrentChroms(byte part)
        {            
            byte barIterate = Parameters.barIterate[part];
            byte genotypeIterate = Parameters.genotypeIterate[part];
            int k = 0;
            for (int i = 0; i < barIterate; i++)
            {
                for (int j = 0; j < genotypeIterate; j++)
                {
                    currentRhythmChromosome[part][k] = rhythmChromosome[part][j];
                    currentPitchChromosome[part][k] = pitchChromosome[part][j];
                    currentVelocityChromosome[part][k] = velocityChromosome[part][j];
                    k++;
                }
            }
        }

        /// <summary>
        /// Updates the parts[] vector based on the current rhythm gene being a rest.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="rhythmGene"></param>
        /// <param name="rhythmSoloPart0"></param>
        private void ProcessRestGene(ref int i, char rhythmGene)
        {
            byte addDur = 0;
            string temps;
            byte maxIterate = Parameters.maxIterate;
            byte genotypeIterate_0 = Parameters.genotypeIterate[0];
            string[] rhythmSoloPart0 = currentRhythmChromosome[0];

            parts[0] += "R";
            parts[1] += "R";
            parts[2] += "R|";
            parts[3] += "R|";

            do
            {
                addDur++;
                i++;

                if (i == maxIterate || i % genotypeIterate_0 == 0)
                    break;
                else
                    rhythmGene = RhythmDecode(rhythmSoloPart0[i]);
            }
            while ((rhythmGene == 'r' || rhythmGene == 'c'));

            temps = Durations.FinalDuration(Parameters.durationIndex, addDur);
            parts[0] += temps + " ";
            parts[1] += temps + " ";
            parts[2] += addDur + " ";
            parts[3] += addDur + " ";            
        }

        /// <summary>
        /// Updates the parts[] vector based on the current rhythm gene being a note.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="rhythmGene"></param>
        /// <param name="pitchSoloPart0"></param>
        /// <param name="pitchSoloPart1"></param>
        /// <param name="velocitySoloPart0"></param>
        /// <param name="velocitySoloPart1"></param>
        /// <param name="rhythmSoloPart0"></param>
        private void ProcessPitchGene(ref int i, char rhythmGene)
        {
            byte note1, note2;
            byte velocity1, velocity2;
            byte addDur = 1, fP;           
            int j;
            byte genotypeIterate_0 = Parameters.genotypeIterate[0];
            byte maxIterate = Parameters.maxIterate;
            byte keySignatureIndex = Parameters.keySignatureIndex;
            byte[] octaves = Parameters.octavesByteValues;
            string duration = Durations.FinalDuration(Parameters.durationIndex, addDur);

            int[] pitchSoloPart0 = currentPitchChromosome[0];
            int[] pitchSoloPart1 = currentPitchChromosome[1];
            byte[] velocitySoloPart0 = currentVelocityChromosome[0];
            byte[] velocitySoloPart1 = currentVelocityChromosome[1];
            string[] rhythmSoloPart0 = currentRhythmChromosome[0];

            do
            {
                fP = Scales.FirstPitch(octaves[0], keySignatureIndex);
                note1 = PitchSolo1Decode(pitchSoloPart0[i], fP);
                velocity1 = VelocityDecode(velocitySoloPart0[i]);
                parts[0] += "[" + note1.ToString() + "]" + duration + "V" + velocity1.ToString() + " ";
                j = note1 - fP;
                parts[2] += ActualOctave(j, 0).ToString() +  //(byte)Parameters.octaves[0] +  
                    "|" + Scales.scales[keySignatureIndex][j] + "|" + addDur + " ";

                fP = Scales.FirstPitch(octaves[1], keySignatureIndex);
                note2 = PitchSolo2Decode(pitchSoloPart1[i], note1, fP);
                velocity2 = VelocityDecode(velocitySoloPart1[i]);
                parts[1] += "[" + note2.ToString() + "]" + duration + "V" + velocity2.ToString() + " ";
                j = note2 - fP;
                parts[3] += ActualOctave(j, 1).ToString() +  //(byte)Parameters.octaves[0] +  
                    "|" + Scales.scales[keySignatureIndex][j] + "|" + addDur + " ";

                i++;

                if (i == maxIterate || i % genotypeIterate_0 == 0)
                    break;
                else
                    rhythmGene = RhythmDecode(rhythmSoloPart0[i]);
            }
            while (rhythmGene == 'p');           
        }

        /// <summary>
        /// Updates the parts[] vector based on the current rhythm gene being a continuation.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="rhythmGene"></param>
        /// <param name="velocitySoloPart0"></param>
        /// <param name="velocitySoloPart1"></param>
        /// <param name="rhythmSoloPart0"></param>
        private void ProcessContinuationGene(ref int i, char rhythmGene)
        {
            byte velocity1, velocity2;
            byte addDur = 1;
            string duration;
            byte maxIterate = Parameters.maxIterate;
            byte genotypeIterate_0 = Parameters.genotypeIterate[0];

            string[] rhythmSoloPart0 = currentRhythmChromosome[0];

            parts[0] = parts[0].Remove(parts[0].LastIndexOf(']') + 1);
            parts[1] = parts[1].Remove(parts[1].LastIndexOf(']') + 1);
            parts[2] = parts[2].Remove(parts[2].LastIndexOf('|') + 1);
            parts[3] = parts[3].Remove(parts[3].LastIndexOf('|') + 1);

            velocity1 = VelocityDecode(currentVelocityChromosome[0][i]);
            velocity2 = VelocityDecode(currentVelocityChromosome[1][i]);            

            do
            {
                addDur++;
                i++;

                if (i == maxIterate || i % genotypeIterate_0 == 0)
                    break;
                else
                    rhythmGene = RhythmDecode(rhythmSoloPart0[i]);
            }
            while (rhythmGene == 'c');

            duration = Durations.FinalDuration(Parameters.durationIndex, addDur);
            parts[0] += duration + "V" + velocity1.ToString() + " ";
            parts[1] += duration + "V" + velocity2.ToString() + " ";
            parts[2] += addDur + " ";
            parts[3] += addDur + " ";            
        }

        /// <summary>
        /// Updates the genome when the bar iteration and/or beat parameters are modified.
        /// </summary>
        private void UpdateGenomeIfNeeded()
        {
            byte maxIterate = Parameters.maxIterate;
            byte unitBeat = Parameters.unitBeat;

            // Bar iteration =:
            //  - 1: random structure;
            //  - 2: AA (2 duplicate sections);
            //  - 4: AAAA (4 duplicate sections).

            int parts0Length = parts[0].Length;
            int parts1Length = parts[1].Length;
            int parts2Length = parts[2].Length;
            int parts3Length = parts[3].Length;

            int i = maxIterate, j;
            while (i < unitBeat)
            {
                for (j = 0; j < parts0Length; j++)
                    parts[0] += parts[0][j];
                for (j = 0; j < parts2Length; j++)
                    parts[2] += parts[2][j];

                for (j = 0; j < parts1Length; j++)
                    parts[1] += parts[1][j];
                for (j = 0; j < parts3Length; j++)
                    parts[3] += parts[3][j];

                i += maxIterate;
            }
        }

        /// <summary>
        /// Creates the numParts*2 elements of the vector parts[] for THIS genome:
        ///  - the first numParts elements represent CFugue musically meaningful strings
        ///         (the notes are represented by their MIDI values);
        ///  - the last numParts elements represent meaningful strings for representing the musical score.
        /// </summary>
        /// <param name="genome"></param>
        /// <returns></returns> 
        public void GenotypeDecode()
        {
            currentRhythmChromosome = new List<string[]>();
            currentPitchChromosome = new List<int[]>();
            currentVelocityChromosome = new List<byte[]>();

            char rhythm1;
            int i = 0;

            byte unitBeat = Parameters.unitBeat;
            // The rhythm is the same for both parts.
            currentRhythmChromosome.Add(new string[unitBeat]);
            // Need a dummy array to pass to the initialization method.
            currentRhythmChromosome.Add(new string[unitBeat]);
            currentPitchChromosome.Add(new int[unitBeat]);
            currentPitchChromosome.Add(new int[unitBeat]);
            currentVelocityChromosome.Add(new byte[unitBeat]);
            currentVelocityChromosome.Add(new byte[unitBeat]);

            byte genotypeIterate_0 = Parameters.genotypeIterate[0];
            byte maxIterate = Parameters.maxIterate;

            UpdateCurrentChroms(0);
            UpdateCurrentChroms(1);

            do
            {
                rhythm1 = RhythmDecode(currentRhythmChromosome[0][i]);

                if (i % genotypeIterate_0 == 0 && rhythm1 == 'c')
                {
                    rhythm1 = 'p';
                }

                if (rhythm1 == 'r')
                {
                    ProcessRestGene(ref i, rhythm1);
                }
                else if (rhythm1 == 'p')
                {
                    ProcessPitchGene(ref i, rhythm1);
                }
                else
                {
                    ProcessContinuationGene(ref i, rhythm1);
                }
            }
            while (i < maxIterate);

            UpdateGenomeIfNeeded();
        }

        /// <summary>
        /// Constructs a string that can be used to playback the bar corresponding to a genome. 
        /// </summary>
        /// <param name="tempo"></param>
        /// <returns></returns>
        public string AssemblePlayGenome(string tempo)
        {
            string playParts = "";
            playParts += "T" + tempo + " ";
            byte numParts = Parameters.NumParts;
            byte[] channels = Parameters.channels;

            for (int i = 0; i < numParts; i++)
            {
                playParts += "V" + channels[i].ToString() + " ";
                playParts += parts[i];
            }

            return playParts;
        }
    }
}
