using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MusicNoteLib;

namespace GAlib
{
    /// <summary>
    /// The population of genomes.
    /// </summary>
    public class Population
    { 
        private double mutationRate = 0.05;
        private List<Genome> genomes = new List<Genome>();
        public List<Genome> Genomes
        {
            get
            {
                return genomes;
            }
            set
            {
                genomes = value;
            }
        }
        
        /// <summary>
        /// Creates the initial population of the specified size.
        /// </summary>
        /// <param name="individCount"> The number of individuals in the population.</param>
        public void RandomPop(byte individCount)
        {
            Random random = new Random();
            genomes.Clear();
            for (int i = 0; i < individCount; i++)
            {               
                Genome gen = new Genome(random); 
                genomes.Add(gen); 
            }
        }

        /// <summary>
        /// Returns the specified list of genomes after their genotypes have been decoded.
        /// </summary>
        /// <param name="geno"> The list of genomes after a scale change.</param>
        /// <returns></returns>
        public static List<Genome> ScaleChangeGenomes(List<Genome> geno)
        {
            List<Genome> list = new List<Genome>();

            foreach (Genome x in geno)
            {
                // Need to use the constructor so that the genotypes get decoded.
                Genome g = new Genome(x);
                list.Add(g);
            }

            return list;
        }        

        /// <summary>
        /// Loads the information associated with the list of genomes contained in a field opened by the user.
        /// </summary>
        /// <param name="genomeList"></param>
        /// <param name="fieldParams"></param>
        /// <returns> The list of genomes from a user-opened field.</returns>
        public List<Genome> OpenFieldGenomes(List<List<string[]>> genomeList, List<string> fieldParams)
        {
            byte[] barIterate = { Parameters.barIterate[0], Parameters.barIterate[1] };
            byte[] octaves = { Parameters.octavesByteValues[0], Parameters.octavesByteValues[1] };
            byte ksIndex = Parameters.keySignatureIndex;
            DurationCFugue duration = Parameters.Duration;
            List<Genome> list = new List<Genome>();
            byte numParts = Parameters.NumParts;

            try
            {
                Parameters.keySignatureIndex = Convert.ToByte(Controller.StringToEnum<KeySignature>(fieldParams[0] + "Maj"));
                Parameters.octavesByteValues[0] = Convert.ToByte(Controller.StringToEnum<Octave>(fieldParams[2]));
                Parameters.octavesByteValues[1] = Convert.ToByte(Controller.StringToEnum<Octave>(fieldParams[3]));
                Parameters.barIterate[0] = Convert.ToByte(fieldParams[4]);
                Parameters.barIterate[1] = Convert.ToByte(fieldParams[5]);
                Parameters.Duration = Controller.StringToEnum<DurationCFugue>(fieldParams[6]);

                int k;
                foreach (List<string[]> x in genomeList)
                {
                    k = 0;
                    Genome g = new Genome();
                    for (int i = 0; i < numParts; i++)
                    {
                        g.rhythmChromosome.Add(x[0 + k]);
                        int[] array = new int[16];
                        for (int j = 0; j < x[1 + k].Length; j++)
                            array[j] = Convert.ToInt32(x[1 + k][j]);
                        g.pitchChromosome.Add(array);
                        byte[] array1 = new byte[16];
                        for (int j = 0; j < x[2 + k].Length; j++)
                            array1[j] = Convert.ToByte(x[2 + k][j]);
                        g.velocityChromosome.Add(array1);
                        k += 3;
                    }

                    g.GenotypeDecode();
                    list.Add(g);
                }
            }
            catch (Exception)
            {
                throw new Exception("The file you're trying to open is corrupted.");
            }

            Parameters.barIterate[0] = barIterate[0];
            Parameters.barIterate[1] = barIterate[1];
            Parameters.octavesByteValues[0] = octaves[0];
            Parameters.octavesByteValues[1] = octaves[1];
            Parameters.keySignatureIndex = ksIndex;
            Parameters.Duration = duration;

            return list;
        }

        /// <summary>
        /// Loads the information associated with the list of genomes contained in a score opened by the user.
        /// </summary>
        /// <param name="genomeList"></param>
        /// <param name="scoreParams"></param>
        /// <returns> The list of genomes from a user-opened score.</returns>
        public List<Genome> OpenScoreGenomes(List<List<string[]>> genomeList, List<string> scoreParams)
        {
            byte[] barIterate = { Parameters.barIterate[0], Parameters.barIterate[1] };
            byte[] octaves = { Parameters.octavesByteValues[0], Parameters.octavesByteValues[1] };
            byte keySignatureIndex = Parameters.keySignatureIndex;
            DurationCFugue duration = Parameters.Duration;

            List<Genome> list = new List<Genome>();

            int k, paramIndex = 1;
            int pmCount = 6;
            byte numParts = Parameters.NumParts;

            try
            {
                Parameters.keySignatureIndex = Convert.ToByte(Controller.StringToEnum<KeySignature>(scoreParams[0] + "Maj"));

                foreach (List<string[]> x in genomeList)
                {
                    Parameters.octavesByteValues[0] = Convert.ToByte(scoreParams[paramIndex + 1]);
                    Parameters.octavesByteValues[1] = Convert.ToByte(scoreParams[paramIndex + 2]);
                    Parameters.barIterate[0] = Convert.ToByte(scoreParams[paramIndex + 3]);
                    Parameters.barIterate[1] = Convert.ToByte(scoreParams[paramIndex + 4]);
                    Parameters.Duration = Controller.StringToEnum<DurationCFugue>(scoreParams[paramIndex + 5]);
                    paramIndex += pmCount;

                    k = 0;
                    Genome g = new Genome();
                    for (int i = 0; i < numParts; i++)
                    {
                        g.rhythmChromosome.Add(x[0 + k]);
                        int[] array = new int[16];
                        for (int j = 0; j < x[1 + k].Length; j++)
                            array[j] = Convert.ToInt32(x[1 + k][j]);
                        g.pitchChromosome.Add(array);
                        byte[] array1 = new byte[16];
                        for (int j = 0; j < x[2 + k].Length; j++)
                            array1[j] = Convert.ToByte(x[2 + k][j]);
                        g.velocityChromosome.Add(array1);
                        k += 3;
                    }

                    g.GenotypeDecode();
                    list.Add(g);
                }
            }
            catch (Exception)
            {
                throw new Exception("The file you're trying to open is corrupted.");
            }

            Parameters.barIterate[0] = barIterate[0];
            Parameters.barIterate[1] = barIterate[1];
            Parameters.octavesByteValues[0] = octaves[0];
            Parameters.octavesByteValues[1] = octaves[1];
            Parameters.keySignatureIndex = keySignatureIndex;
            Parameters.Duration = duration;

            return list;
        }

        /// <summary>
        /// Mutates the user-selected genomes.
        /// </summary>
        /// <param name="selected"> A list of the user-selected genomes.</param>
        public void MutateSelectedGenomes(List<byte> selected)
        {
            Random random = new Random();

            foreach (byte i in selected)
            {
                Genome genome = new Genome(Mutation(genomes[i], random));
                genomes[i] = genome;
            }
        }

        /// <summary>
        /// Updates the genomes in a field by decoding their genotypes.
        /// </summary>
        public void UpdateField()
        {
            foreach (Genome g in genomes)
            {
                for (int i = 0; i < g.Parts.Length; i++)
                    g.Parts[i] = "";
                g.GenotypeDecode();
            }
        }

        /// <summary>
        /// Applies single-parent crossover to the specified chromosome at the specified cut point.
        /// The crossover is applied to the chromosome substring starting at index 0 and of length chromLength.
        /// </summary>
        /// <param name="chrom"> The chromosome to apply crossover to.</param>
        /// <param name="cutPoint"></param>
        /// <param name="chromLength"> The length of the chromosome substring used in the crossover.</param>
        /// <param name="part"> The solo part of the chromosome.</param>
        private void ApplySingleParentCrossover(ref object[] chrom, byte cutPoint, byte chromLength, byte part)
        {
            int i = 0;

            object[] result = new object[chromLength];
            Array.Copy(chrom, result, chromLength);

            for (int j = cutPoint; j < chromLength; j++)
            {
                chrom[i] = result[j];
                i++;
            }
            for (int j = 0; j < cutPoint; j++)
            {
                chrom[i] = result[j];
                i++;
            }
        }

        /// <summary>
        /// Performs single-parent crossover on the specified genome.
        /// The crossover is applied to chromosome substrings starting at index 0 and of length genotypeIterate[part],
        /// so that the relevant section of the chromosome structure is modified.
        /// E.g. unitBeat=8 and barIterate=2: we want to use substrings of length 4 so that, after crossover,
        ///     duplicating these modified substrings results in a crossed-over, doubly-iterated genome.       
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="random"></param>
        /// <returns> The crossed-over genome.</returns>
        Genome Crossover(Genome gen, Random random)
        {            
            Genome g = new Genome(gen);
            byte genotypeIterate_0 = Parameters.genotypeIterate[0];
            byte genotypeIterate_1 = Parameters.genotypeIterate[1];
            byte maxUnitBeat = Parameters.maxUnitBeat;
            object[] chrom = new object[maxUnitBeat];

            byte lockChrom_0 = Parameters.lockChrom[0];
            byte lockChrom_1 = Parameters.lockChrom[1];
            byte cutPointPart1 = (byte)random.Next(1, genotypeIterate_0);
            byte cutPointPart2 = (byte)random.Next(1, genotypeIterate_1);            

            // If the rhythm chromosome is unlocked for both parts, apply crossover to it.
            if (lockChrom_0 != 1 && lockChrom_1 != 1)
            {
                g.rhythmChromosome[0].CopyTo(chrom, 0);                             
                ApplySingleParentCrossover(ref chrom, cutPointPart1, genotypeIterate_0, 0);               
                chrom.CopyTo(g.rhythmChromosome[0], 0);

                g.rhythmChromosome[1].CopyTo(chrom, 0);
                ApplySingleParentCrossover(ref chrom, cutPointPart2, genotypeIterate_1, 1);                
                chrom.CopyTo(g.rhythmChromosome[1], 0);                
            }
            // If the pitch chromosome for part 1 is unlocked, apply crossover to it.
            if (lockChrom_0 != 2)
            {
                chrom = new object[maxUnitBeat];
                g.pitchChromosome[0].CopyTo(chrom, 0);
                ApplySingleParentCrossover(ref chrom, cutPointPart1, genotypeIterate_0, 0);
                chrom.CopyTo(g.pitchChromosome[0], 0);               
            }
            // If the pitch chromosome for part 2 is unlocked, apply crossover to it.
            if (lockChrom_1 != 2)
            {
                g.pitchChromosome[1].CopyTo(chrom, 0);
                ApplySingleParentCrossover(ref chrom, cutPointPart1, genotypeIterate_0, 0);
                chrom.CopyTo(g.pitchChromosome[1], 0);                
            }
            // If the velocity chromosome for part 0 is unlocked, apply crossover to it.
            if (lockChrom_0 != 3)
            {
                g.velocityChromosome[0].CopyTo(chrom, 0);
                ApplySingleParentCrossover(ref chrom, cutPointPart1, genotypeIterate_0, 0);
                chrom.CopyTo(g.velocityChromosome[0], 0);                
            }
            // If the velocity chromosome for part 1 is unlocked, apply crossover to it.
            if (lockChrom_1 != 3)
            {
                g.velocityChromosome[1].CopyTo(chrom, 0);
                ApplySingleParentCrossover(ref chrom, cutPointPart1, genotypeIterate_0, 0);
                chrom.CopyTo(g.velocityChromosome[1], 0);                
            }

            return g;
        }

        /// <summary>
        /// Performs one-point crossover between 2 parents (both chromosomes have the same cut point).
        /// The crossover is applied to corresponding parent chromosome substrings, starting at index 0 and
        /// of length genotypeIterate[part], so that the structure is preserved.        
        /// E.g. unitBeat=8 and barIterate=2: we want to use corresponding substrings of length 4 so that, after crossover,
        ///     duplicating these modified substrings results in 2 crossed-over, doubly-iterated child genomes.
        /// </summary>
        /// <param name="genome1"></param>
        /// <param name="genome2"></param>
        /// <param name="random"></param>
        /// <returns> The crossed-over genomes.</returns>
        List<Genome> Crossover(Genome genome1, Genome genome2, Random random)
        {
            List<Genome> offspring = new List<Genome>();            

            Genome childGenome1 = new Genome();
            Genome childGenome2 = new Genome();

            // The value of genotypeIterate is unitBeat/barIterate.
            byte genotypeIterate_0 = Parameters.genotypeIterate[0];
            byte genotypeIterate_1 = Parameters.genotypeIterate[1];
            byte maxUnitBeat = Parameters.maxUnitBeat;
            
            string[] rhythm1_childGenome1 = new string[maxUnitBeat];
            string[] rhythm2_childGenome1 = new string[maxUnitBeat];
            string[] rhythm1_childGenome2 = new string[maxUnitBeat];
            string[] rhythm2_childGenome2 = new string[maxUnitBeat];
            int[] pitch1_childGenome1 = new int[maxUnitBeat];
            int[] pitch2_childGenome1 = new int[maxUnitBeat];
            int[] pitch1_childGenome2 = new int[maxUnitBeat];
            int[] pitch2_childGenome2 = new int[maxUnitBeat];
            byte[] velocity1_childGenome1 = new byte[maxUnitBeat];
            byte[] velocity2_childGenome1 = new byte[maxUnitBeat];
            byte[] velocity1_childGenome2 = new byte[maxUnitBeat];
            byte[] velocity2_childGenome2 = new byte[maxUnitBeat];

            // For each parent chromosome part, copy the chunk to be used in crossover into the corresponding child chromosome part.
            Array.Copy(genome1.rhythmChromosome[0], 0, rhythm1_childGenome1, 0, genotypeIterate_0);
            Array.Copy(genome1.rhythmChromosome[1], 0, rhythm2_childGenome1, 0, genotypeIterate_1);
            Array.Copy(genome2.rhythmChromosome[0], 0, rhythm1_childGenome2, 0, genotypeIterate_0);
            Array.Copy(genome2.rhythmChromosome[1], 0, rhythm2_childGenome2, 0, genotypeIterate_1);

            Array.Copy(genome1.pitchChromosome[0], 0, pitch1_childGenome1, 0, genotypeIterate_0);
            Array.Copy(genome1.pitchChromosome[1], 0, pitch2_childGenome1, 0, genotypeIterate_1);
            Array.Copy(genome2.pitchChromosome[0], 0, pitch1_childGenome2, 0, genotypeIterate_0);
            Array.Copy(genome2.pitchChromosome[1], 0, pitch2_childGenome2, 0, genotypeIterate_1);

            Array.Copy(genome1.velocityChromosome[0], 0, velocity1_childGenome1, 0, genotypeIterate_0);
            Array.Copy(genome1.velocityChromosome[1], 0, velocity2_childGenome1, 0, genotypeIterate_1);
            Array.Copy(genome2.velocityChromosome[0], 0, velocity1_childGenome2, 0, genotypeIterate_0);
            Array.Copy(genome2.velocityChromosome[1], 0, velocity2_childGenome2, 0, genotypeIterate_1);            
            
            byte cutPointPart1 = (byte)random.Next(1, genotypeIterate_0);
            byte cutPointPart2 = (byte)random.Next(1, genotypeIterate_1);
            // The length of the right chunk resulting from cutting, for each part.
            int copyLengthPart1 = genotypeIterate_0 - cutPointPart1;
            int copyLengthPart2 = genotypeIterate_1 - cutPointPart2;

            if (Parameters.lockChrom[0] != 1 && Parameters.lockChrom[1] != 1)
            {
                Array.Copy(genome2.rhythmChromosome[0], cutPointPart1, rhythm1_childGenome1, cutPointPart1, copyLengthPart1);
                Array.Copy(genome2.rhythmChromosome[1], cutPointPart2, rhythm2_childGenome1, cutPointPart2, copyLengthPart2);
                Array.Copy(genome1.rhythmChromosome[0], cutPointPart1, rhythm1_childGenome2, cutPointPart1, copyLengthPart1);
                Array.Copy(genome1.rhythmChromosome[1], cutPointPart2, rhythm2_childGenome2, cutPointPart2, copyLengthPart2);
            }
            if (Parameters.lockChrom[0] != 2)
            {
                Array.Copy(genome2.pitchChromosome[0], cutPointPart1, pitch1_childGenome1, cutPointPart1, copyLengthPart1);
                Array.Copy(genome1.pitchChromosome[0], cutPointPart1, pitch1_childGenome2, cutPointPart1, copyLengthPart1);    
            }
            if (Parameters.lockChrom[1] != 2)
            {
                Array.Copy(genome2.pitchChromosome[1], cutPointPart2, pitch2_childGenome1, cutPointPart2, copyLengthPart2);
                Array.Copy(genome1.pitchChromosome[1], cutPointPart2, pitch2_childGenome2, cutPointPart2, copyLengthPart2);
            }
            if (Parameters.lockChrom[0] != 3)
            {
                Array.Copy(genome2.velocityChromosome[0], cutPointPart1, velocity1_childGenome1, cutPointPart1, copyLengthPart1);
                Array.Copy(genome1.velocityChromosome[0], cutPointPart1, velocity1_childGenome2, cutPointPart1, copyLengthPart1);
            }
            if (Parameters.lockChrom[1] != 3)
            {
                Array.Copy(genome2.velocityChromosome[1], cutPointPart2, velocity2_childGenome1, cutPointPart2, copyLengthPart2);
                Array.Copy(genome1.velocityChromosome[1], cutPointPart2, velocity2_childGenome2, cutPointPart2, copyLengthPart2);
            }

            // The length of the chunk that did not participate in crossover, for each part.
            int restOfChromLengthPart1 = maxUnitBeat - genotypeIterate_0;
            int restOfChromLengthPart2 = maxUnitBeat - genotypeIterate_1;

            // For each parent chromosome part, copy the remaining chunk into the corresponding child chromosome part.
            Array.Copy(genome1.rhythmChromosome[0], genotypeIterate_0, rhythm1_childGenome1, genotypeIterate_0, restOfChromLengthPart1);
            Array.Copy(genome1.rhythmChromosome[1], genotypeIterate_1, rhythm2_childGenome1, genotypeIterate_1, restOfChromLengthPart2);
            Array.Copy(genome2.rhythmChromosome[0], genotypeIterate_0, rhythm1_childGenome2, genotypeIterate_0, restOfChromLengthPart1);
            Array.Copy(genome2.rhythmChromosome[1], genotypeIterate_1, rhythm2_childGenome2, genotypeIterate_1, restOfChromLengthPart2);

            Array.Copy(genome1.pitchChromosome[0], genotypeIterate_0, pitch1_childGenome1, genotypeIterate_0, restOfChromLengthPart1);
            Array.Copy(genome1.pitchChromosome[1], genotypeIterate_1, pitch2_childGenome1, genotypeIterate_1, restOfChromLengthPart2);
            Array.Copy(genome2.pitchChromosome[0], genotypeIterate_0, pitch1_childGenome2, genotypeIterate_0, restOfChromLengthPart1);
            Array.Copy(genome2.pitchChromosome[1], genotypeIterate_1, pitch2_childGenome2, genotypeIterate_1, restOfChromLengthPart2);

            Array.Copy(genome1.velocityChromosome[0], genotypeIterate_0, velocity1_childGenome1, genotypeIterate_0, restOfChromLengthPart1);
            Array.Copy(genome1.velocityChromosome[1], genotypeIterate_1, velocity2_childGenome1, genotypeIterate_1, restOfChromLengthPart2);
            Array.Copy(genome2.velocityChromosome[0], genotypeIterate_0, velocity1_childGenome2, genotypeIterate_0, restOfChromLengthPart1);
            Array.Copy(genome2.velocityChromosome[1], genotypeIterate_1, velocity2_childGenome2, genotypeIterate_1, restOfChromLengthPart2);

            // Add the crossed-over chromosomes to the child genomes.
            childGenome1.rhythmChromosome.Add(rhythm1_childGenome1);
            childGenome1.rhythmChromosome.Add(rhythm2_childGenome1);
            childGenome2.rhythmChromosome.Add(rhythm1_childGenome2);
            childGenome2.rhythmChromosome.Add(rhythm2_childGenome2);

            childGenome1.pitchChromosome.Add(pitch1_childGenome1);
            childGenome1.pitchChromosome.Add(pitch2_childGenome1);
            childGenome2.pitchChromosome.Add(pitch1_childGenome2);
            childGenome2.pitchChromosome.Add(pitch2_childGenome2);

            childGenome1.velocityChromosome.Add(velocity1_childGenome1);
            childGenome1.velocityChromosome.Add(velocity2_childGenome1);
            childGenome2.velocityChromosome.Add(velocity1_childGenome2);
            childGenome2.velocityChromosome.Add(velocity2_childGenome2);

            childGenome1.GenotypeDecode();
            childGenome2.GenotypeDecode();

            offspring.Add(childGenome1);
            offspring.Add(childGenome2);

            return offspring;
        }
        
        /// <summary>
        /// Applies swap mutation to the specified part of each type of chromosome of the specified genome.       
        /// </summary>
        /// <param name="genome"> The genome to apply swap mutation to.</param>
        /// <param name="swapPoint1"></param>
        /// <param name="swapPoint2"></param>
        /// <param name="part"> The solo part of the chromosome.</param>
        /// <param name="random"></param>        
        private void ApplySwapMutationForPart(ref Genome genome, int swapPoint1, int swapPoint2, byte part, Random random)
        {
            byte[] lockChrom = Parameters.lockChrom;            
            bool isRhythmUnlocked = (lockChrom[0] != 1 && lockChrom[1] != 1);
            bool isPitchUnlocked = (lockChrom[part] != 2);
            bool isVelocityUnlocked = (lockChrom[part] != 3);

            int aux;

            if (lockChrom[0] != 1 && lockChrom[1] != 1)
            {
                string geneToMutate = genome.rhythmChromosome[part][swapPoint1];
                genome.rhythmChromosome[part][swapPoint1] = genome.rhythmChromosome[part][swapPoint2];
                genome.rhythmChromosome[part][swapPoint2] = geneToMutate;
            }

            if (lockChrom[part] != 2)
            {
                aux = genome.pitchChromosome[part][swapPoint1];
                genome.pitchChromosome[part][swapPoint1] = genome.pitchChromosome[part][swapPoint2];
                genome.pitchChromosome[part][swapPoint2] = aux;
            }

            if (lockChrom[part] != 3)
            {
                aux = genome.velocityChromosome[part][swapPoint1];
                genome.velocityChromosome[part][swapPoint1] = genome.velocityChromosome[part][swapPoint2];
                genome.velocityChromosome[part][swapPoint2] = (byte)aux;
            }
        }

        /// <summary>
        /// Applies swap mutation in its limited case i.e. inverts the bits of the specified rhythm chromosome gene
        /// of the specified genome.
        /// </summary>
        /// <param name="genome"></param>
        /// <param name="rhythmGene"></param>
        private void ApplySwapMutationLimitingCase(ref Genome genome, int rhythmGene)
        {
            string geneToMutate = "";
            string[] genPart0 = genome.rhythmChromosome[0];
            int rhythmGeneLength = genPart0[rhythmGene].Length;
            for (int i = 0; i < rhythmGeneLength; i++)
                if (genPart0[rhythmGene][i] == '0')
                    geneToMutate += "1";
                else
                    geneToMutate += "0";

            genPart0[rhythmGene] = geneToMutate;
            genome.rhythmChromosome[0] = genPart0;
            genome.rhythmChromosome[1] = genPart0;
        }

        /// <summary>
        /// Performs swap mutation i.e. for 2 random genes:
        ///     - if different, the genes are swapped for each chromosome;
        ///     - if equal, the bits of the rhythm chromosome gene are inverted.
        /// The swap mutation is applied to the chromosome substrings starting at index 0 and of length genotypeIterate[part],
        /// so that the relevant section of the chromosome structure is modified.
        /// E.g. unitBeat=8 and barIterate=2: we want to use substrings of length 4 so that, after swap mutation,
        ///     duplicating these modified substrings results in a swap-mutated, doubly-iterated genome.
        /// </summary>
        /// <param name="genome"></param>
        /// <param name="random"></param>
        private void SwapMutation(Genome genome, Random random)
        {                        
            byte[] lockChrom = Parameters.lockChrom;
            byte[] genotypeIterate = Parameters.genotypeIterate;
            byte genotypeIterate_0 = genotypeIterate[0];
            byte genotypeIterate_1 = genotypeIterate[1];

            byte swapPoint1 = (byte)random.Next(0, genotypeIterate_0);
            byte swapPoint2 = (byte)random.Next(0, genotypeIterate_1);
            
            // If the swap genes are equal and the rhythm chromosome is not locked.
            if (swapPoint1 == swapPoint2 && lockChrom[0] != 1 && lockChrom[1] != 1)
            {
                ApplySwapMutationLimitingCase(ref genome, swapPoint1);
            }
            else
            {
                ApplySwapMutationForPart(ref genome, swapPoint1, swapPoint2, 0, random);
                ApplySwapMutationForPart(ref genome, swapPoint1, swapPoint2, 1, random);
            }            
        }

        /// <summary>
        /// Returns the mutated version of the bitstring coded gene parameter.
        /// The mutation inverts the gene bits.
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
       private string FlipFlopMutation(string gene, Random random)
        {
           string mutatedGene="";
           int geneLength = gene.Length;

           for (int i = 0; i < geneLength; i++)
           {
               if (random.NextDouble() <= mutationRate) ////change the operator and/or the mutationRate
                   if (gene[i] == '0')
                       mutatedGene += "1";
                   else
                       mutatedGene += "0";
               else
                   mutatedGene += gene[i].ToString();
           }

            return mutatedGene;
       }

       /// <summary>
       /// Returns the mutated version of the integer coded gene parameter.
       /// The mutation replaces the gene value with a random value in [0, supNotesInterval].
       /// </summary>
       /// <param name="gene"></param>
       /// <returns></returns>
       private int UniformMutation(int gene, int inf, int sup, Random random)
        {
            int mutatedGene = gene;            

            if (random.NextDouble() <= mutationRate)
                mutatedGene = random.Next(inf, sup);

           return mutatedGene;
        }

       /// <summary>
       /// Applies mutation to the specified part of each type of chromosome of the specified genome.
       /// The mutation is applied to the chromosome substrings starting at index 0 and of length chromLength.
       /// </summary>
       /// <param name="genome"> The genome to apply mutation to.</param>
       /// <param name="chromLength"> The length of the chromosome substring used in the mutation.</param>
       /// <param name="inf"> The low end of the valid range for mutated genes values.</param>
       /// <param name="sup"> The high end of the valid range for mutated genes values.</param>
       /// <param name="part"> The solo part of the chromosome.</param>
       /// <param name="random"></param>
       private void ApplyMutationForPart(ref Genome genome, byte chromLength, int inf, int sup, byte part, Random random)
       {
           byte[] lockChrom = Parameters.lockChrom;
           byte maxSteps = Parameters.maxNoOfStepsVelocity;
           bool isRhythmUnlocked = (lockChrom[0] != 1 && lockChrom[1] != 1);
           bool isPitchUnlocked = (lockChrom[part] != 2);
           bool isVelocityUnlocked = (lockChrom[part] != 3);

           for (int i = 0; i < chromLength; i++)
           {
               if (random.NextDouble() <= mutationRate)
               {
                   if (isRhythmUnlocked)
                       genome.rhythmChromosome[part][i] = FlipFlopMutation(genome.rhythmChromosome[part][i], random);
                   if (isPitchUnlocked)
                       genome.pitchChromosome[part][i] = UniformMutation(genome.pitchChromosome[part][i], inf, sup, random);
                   if (isVelocityUnlocked)
                       genome.velocityChromosome[part][i] = (byte)UniformMutation(genome.velocityChromosome[part][i], 0, maxSteps, random);
               }
           }
       }

        /// <summary>
        /// Performs mutation on a genome for each gene, according to the probability of mutation:
        ///     - for bit genes: flip-flop i.e. the bits of the gene are inverted;
        ///     - for int genes: uniform i.e. the value of the gene is replaced with a random value in [0, supNotesInterval].
        /// The mutation is applied to chromosome substrings starting at index 0 and of length genotypeIterate[part],
        /// so that the relevant section of the chromosome structure is modified.
        /// E.g. unitBeat=8 and barIterate=2: we want to use substrings of length 4 so that, after mutation,
        ///     duplicating these modified substrings results in a mutated, doubly-iterated genome.
        /// </summary>
        /// <param name="genome"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        Genome Mutation(Genome genome, Random random)
        {
            byte[] genotypeIterate = Parameters.genotypeIterate;
            byte genotypeIterate_0 = genotypeIterate[0];
            byte genotypeIterate_1 = genotypeIterate[1];
            
            // inf and sup define the valid range of values used to generate the random pitch gene, for each part.
            ApplyMutationForPart(ref genome, genotypeIterate_0, 0, Parameters.supNotesInterval, 0, random);
            ApplyMutationForPart(ref genome, genotypeIterate_1, -3, 4, 1, random);

            // Gives a better chance at avoiding the situation when the modified genotype corresponds to the same phenotype.
            // E.g. same rhythm and mutated pitch values corresponding to rest genes, thus being ignored.
            SwapMutation(genome, random);

            return genome;
        }

        /// <summary>
        /// Generates the next generation of genomes, breeded from the current population.
        /// Preserved genomes suffer no modifications across generations.
        /// Selected genomes (or parent genomes) breed offspring.
        /// Note: mutation is applied immediately after crowwover.
        /// </summary>
        /// <param name="currPop"></param>
        /// <param name="nonPreserved"></param>
        /// <param name="selected"></param>
        public void NextGeneration(List<Genome> currPop, List<byte> nonPreserved, List<byte> selected)
        {
            Random random = new Random();

            Genome gen;
            List<Genome> offspring = new List<Genome>() ;
            int parent1, parent2;
            int i;

            // If there are no selected individuals, we generate new random ones.
            if (selected.Count == 0)
            {
                foreach(byte b in nonPreserved) 
                {
                    Genome geno = new Genome(random);
                    genomes[b] = geno;
                }
            }
            // When there's only 1 selected parent, we apply single-parent crossover.            
            else if (selected.Count == 1)
            {
                Genome g = new Genome(Crossover(currPop[selected[0]], random));
                foreach (byte b in nonPreserved)
                {
                    Genome geno = new Genome(g);
                    Genome ge = new Genome(Mutation(geno, random));
                    genomes[b] = ge;
                }
            }
            // When there are at least 2 selected parents.
            else
            {
                i = nonPreserved[0];
                while (i < nonPreserved.Count)
                {
                    // Pick random parents for crossover.
                    parent1 = random.Next(0, selected.Count);
                    parent2 = random.Next(0, selected.Count);

                    // If we are to crossover the same parent, we apply single-parent crossover.
                    if (parent1 == parent2)
                    {
                        gen = Crossover(currPop[parent1], random);
                        Genome genome = new Genome(Mutation(gen, random));
                        genomes[nonPreserved[i]] = genome;
                        i++;
                        if (i == nonPreserved.Count)
                            break;
                        gen = null;
                    }
                    else
                    {
                        // We apply one-point crossover on the randomly picked parents.
                        offspring = Crossover(currPop[selected[parent1]], currPop[selected[parent2]], random);
                        Genome genomeA = new Genome(Mutation(offspring[0], random));
                        genomes[nonPreserved[i]] = genomeA;
                        i++;
                        if (i == nonPreserved.Count)
                            break;
                        Genome genomeB = new Genome(Mutation(offspring[1], random));
                        genomes[nonPreserved[i]] = genomeB;
                        i++;
                        if (i == nonPreserved.Count)
                            break;
                        offspring.Clear();
                    }
                }
            }           
        }
    }
}
