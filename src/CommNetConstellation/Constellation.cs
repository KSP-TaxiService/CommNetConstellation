using CommNetConstellation.CommNetLayer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommNetConstellation
{
    /// <summary>
    /// Data structure to be saved to the persistent.sfs
    /// </summary>
    public class Constellation: IComparable<Constellation>
    {
        [Persistent] public short frequency;
        [Persistent] public string name;
        [Persistent] public Color color;

        /// <summary>
        /// Empty constructor for ConfigNode.LoadObjectFromConfig()
        /// </summary>
        public Constellation() { }

        /// <summary>
        /// Parameter constructor for other uses
        /// </summary>
        public Constellation(short frequency, string name, Color color)
        {
            this.frequency = frequency;
            this.name = name;
            this.color = color;
        }

        /// <summary>
        /// Retrieve the constellation color assoicated with the frequency
        /// </summary>
        public static Color getColor(int givenFreq)
        {
            Constellation possibleMatch = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == givenFreq);
            if (possibleMatch == null)
                return CNCSettings.Instance.DefaultPublicColor; // fallback color
            else
                return possibleMatch.color;
        }

        /// <summary>
        /// Retrieve the constellation name assoicated with the frequency
        /// </summary>
        public static string getName(int givenFreq)
        {
            Constellation possibleMatch = CNCCommNetScenario.Instance.constellations.Find(x => x.frequency == givenFreq);
            if (possibleMatch == null)
                return getName(CNCSettings.Instance.PublicRadioFrequency); // fallback constellation
            else
                return possibleMatch.name;
        }

        /// <summary>
        /// Sanitize the user-origin frequency prior to the commit
        /// </summary>
        public static bool isFrequencyValid(int givenFreq)
        {
            if (givenFreq < 0 || givenFreq > short.MaxValue)
                return false;

            return true;
        }

        /// <summary>
        /// Count the number of existing vessels of the given constellation
        /// </summary>
        public static int countVessels(Constellation thisConstellation)
        {
            int count = 0;
            List<CNCCommNetVessel> vessels = CNCCommNetScenario.Instance.getCommNetVessels();
            for (int i=0; i< vessels.Count; i++)
            {
                if (vessels[i].getFrequencies().Contains(thisConstellation.frequency))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Allow to be sorted easily
        /// </summary>
        public int CompareTo(Constellation other)
        {
            return this.frequency - other.frequency;
        }

        public static bool NonLinqAny(List<Constellation> constellations, short givenFrequency)
        {
            for (int i = 0; i < constellations.Count; i++)
            {
                if (constellations[i].frequency == givenFrequency)
                    return true;
            }
            return false;
        }

        public static List<short> NonLinqIntersect(List<short> aFreqs, List<short> bFreqs)
        {
            List<short> commonFreqs = new List<short>();

            if (aFreqs.Count == 0 || bFreqs.Count == 0)
                return commonFreqs;

            aFreqs.Sort();
            bFreqs.Sort();

            int aIndex = 0;
            int bIndex = 0;
            while (aIndex < aFreqs.Count && bIndex < bFreqs.Count)
            {
                if(aFreqs[aIndex] < bFreqs[bIndex])
                {
                    aIndex++;
                }
                else if (aFreqs[aIndex] > bFreqs[bIndex])
                {
                    bIndex++;
                }
                else if(aFreqs[aIndex] == bFreqs[bIndex] && !commonFreqs.Contains(aFreqs[aIndex]))
                {
                    commonFreqs.Add(aFreqs[aIndex]);
                    aIndex++;
                    bIndex++;
                }
            }

            return commonFreqs;
        }

        public static double NonLinqSum(List<double> list)
        {
            double sum = 0.0;
            for (int i = 0; i < list.Count; i++)
                sum += list[i];
            return sum;
        }

        public static double NonLinqMax(List<double> list)
        {
            double max = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                if (max < list[i])
                    max = list[i];
            }
            return max;
        }
    }
}
