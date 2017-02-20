using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAlib
{
    /// <summary>
    /// Chromosomes consist of genes, which encode characters of the genome.
    /// We need 3 characters: rhythm, pitch and velocity.
    /// </summary>
    class Chromosome
    {
        Random random;        

        public Chromosome(Random random)
        {
            this.random = random;
        }

        /// <summary>
        /// Creates a random Rhythm chromosome for 2 parts.
        /// </summary>
        /// <returns></returns>
        public List<string[]> RhythmChrom()
        {
            byte maxUnitBeat = Parameters.maxUnitBeat;
            List<string[]> rhythmList = new List<string[]>();
            string[] rhythmPart1 = new string[maxUnitBeat];
            string[] rhythmPart2 = new string[maxUnitBeat];
            string bytes1;
            string bytes2;

            for (int i = 0; i < maxUnitBeat; i++)
            {
                bytes1 = "";
                bytes2 = "";
                for (int j = 0; j < 4; j++)
                {
                    bytes1 += random.Next(0, 2);
                    bytes2 += random.Next(0, 2);
                }
                rhythmPart1[i] = bytes1;
                rhythmPart2[i] = bytes2;
            }

            rhythmList.Add(rhythmPart1);
            rhythmList.Add(rhythmPart2);

            return rhythmList;
        }

        /// <summary>
        /// Creates a random Pitch chromosome for 2 parts: the basic melody of the second part is
        /// based on the first by adding/subtracting between 4 and 7 half steps (aka semitones). //????and picking the octave
        /// </summary>
        /// <returns></returns>
        public List<int[]> PitchChrom()
        {
            byte maxUnitBeat = Parameters.maxUnitBeat;
            List<int[]> pitchList = new List<int[]>();
            int[] pitchSoloPart1 = new int[maxUnitBeat];
            int[] pitchSoloPart2 = new int[maxUnitBeat];

            for (int i = 0; i < maxUnitBeat; i++)
            {
                pitchSoloPart1[i] = random.Next(0, Parameters.supNotesInterval);
                pitchSoloPart2[i] = random.Next(-3, 4);
            }

            pitchList.Add(pitchSoloPart1);
            pitchList.Add(pitchSoloPart2);

            return pitchList;
        }

        /// <summary>
        /// Creates a random Velocity chromosome for 2 parts: the interval [infVelocity, supVelocity] is
        /// divided by a step of stepVelocity, resulting in a set of 17 possible values.
        /// </summary>
        /// <returns></returns>
        public List<byte[]> VelocityChrom()
        {
            byte maxUnitBeat = Parameters.maxUnitBeat;
            List<byte[]> velocityList = new List<byte[]>();
            byte[] velocitySoloPart1 = new byte[maxUnitBeat];
            byte[] velocitySoloPart2 = new byte[maxUnitBeat];
            byte numDiv = (byte)((Parameters.supVelocity - Parameters.infVelocity) / Parameters.stepVelocity);

            for (int i = 0; i < maxUnitBeat; i++)
            {
                velocitySoloPart1[i] = (byte)random.Next(0, numDiv);
                velocitySoloPart2[i] = (byte)random.Next(0, numDiv);
            }

            velocityList.Add(velocitySoloPart1);
            velocityList.Add(velocitySoloPart2);

            return velocityList;
        }

        /// <summary>
        /// Creates a Rhythm chromosome for 2 parts, each of which consists of a whole rest.
        /// </summary>
        /// <returns></returns>
        public static List<string[]> WholeRestRhythmChrom()
        {
            byte maxUnitBeat = Parameters.maxUnitBeat;
            List<string[]> rhythmList = new List<string[]>();
            string[] rhythmPart1 = new string[maxUnitBeat];
            string[] rhythmPart2 = new string[maxUnitBeat];

            // Set the first gene to a rest.
            rhythmPart1[0] = "0110";
            rhythmPart2[0] = "0110";

            // Set the rest of the genes to continue the rest, thus yielding a whole rest.
            for (int i = 1; i < maxUnitBeat; i++)
            {
                rhythmPart1[i] = "1000";
                rhythmPart2[i] = "1000";
            }

            rhythmList.Add(rhythmPart1);
            rhythmList.Add(rhythmPart2);

            return rhythmList;
        }

        /// <summary>
        /// Creates a Pitch chromosome for 2 parts.
        /// To be used in combination with <see cref="WholeRestRhythmChrom()"/>.
        /// </summary>
        /// <returns></returns>
        public static List<int[]> WholeRestPitchChrom()
        {
            byte maxUnitBeat = Parameters.maxUnitBeat;
            List<int[]> pitchList = new List<int[]>();
            int[] pitchSoloPart1 = new int[maxUnitBeat];
            int[] pitchSoloPart2 = new int[maxUnitBeat];

            // The pitch genes are ignored since the bar contains no notes.
            for (int i = 0; i < maxUnitBeat; i++)
            {
                pitchSoloPart1[i] = 0;
                pitchSoloPart2[i] = 0;
            }

            pitchList.Add(pitchSoloPart1);
            pitchList.Add(pitchSoloPart2);

            return pitchList;
        }

        /// <summary>
        /// Creates a Velocity chromosome for 2 parts.
        /// To be used in combination with <see cref="WholeRestRhythmChrom()"/>.
        /// </summary>
        /// <returns></returns>
        public static List<byte[]> WholeRestVelocityChrom()
        {
            byte maxUnitBeat = Parameters.maxUnitBeat;
            List<byte[]> velocityList = new List<byte[]>();
            byte[] velocitySoloPart1 = new byte[maxUnitBeat];
            byte[] velocitySoloPart2 = new byte[maxUnitBeat];            

            // The velocity genes are ignored since the bar contains no notes.
            for (int i = 0; i < maxUnitBeat; i++)
            {
                velocitySoloPart1[i] = Parameters.infVelocity;
                velocitySoloPart2[i] = Parameters.infVelocity;
            }

            velocityList.Add(velocitySoloPart1);
            velocityList.Add(velocitySoloPart2);

            return velocityList;
        }
    }
}
