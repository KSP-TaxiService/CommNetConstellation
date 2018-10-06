using CommNetConstellation.CommNetLayer;
using CommNetConstellation.UI;
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
        [Persistent] public bool visibility = true;

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
            this.visibility = true;
        }

        /// <summary>
        /// Retrieve the constellation color assoicated with the frequency
        /// </summary>
        public static Color getColor(int givenFreq)
        {
            Constellation possibleMatch = find(givenFreq);
            if (possibleMatch == null)
                return Color.clear; // fallback color
            else
                return possibleMatch.color;
        }

        /// <summary>
        /// Retrieve the constellation name assoicated with the frequency
        /// </summary>
        public static string getName(int givenFreq)
        {
            Constellation possibleMatch = find(givenFreq);
            if (possibleMatch == null)
                return "Not-Found"; // fallback name
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
        /// Retrieve the constellation assoicated with the frequency
        /// </summary>
        public static Constellation find(int givenFreq)
        {
            for(int i=0; i<CNCCommNetScenario.Instance.constellations.Count; i++)
            {
                if(CNCCommNetScenario.Instance.constellations[i].frequency == givenFreq)
                {
                    return CNCCommNetScenario.Instance.constellations[i];
                }
            }
            return null;
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
                if (GameUtils.firstCommonElement(vessels[i].getFrequencyArray(), new short[] { thisConstellation.frequency }) >= 0)
                {
                    count++;
                }
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
    }
}
