using CommNetConstellation.CommNetLayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommNetConstellation
{
    public class Constellation
    {
        [Persistent] public short frequency;
        [Persistent] public string name;
        [Persistent] public Color color;

        public Constellation()
        {
            //empty constructor for ConfigNode.LoadObjectFromConfig()
        }

        public Constellation(short frequency, string name, Color color)
        {
            this.frequency = frequency;
            this.name = name;
            this.color = color;
        }

        public static Constellation find(List<Constellation> constellations, int givenFreq)
        {
            if (constellations == null)
                return null;

            return constellations.Find(i => i.frequency == givenFreq);
        }

        public static Color getColor(List<Constellation> constellations, int givenFreq)
        {
            Constellation possibleMatch = find(constellations, givenFreq);
            if (possibleMatch == null)
                return new Color(0.65f, 0.65f, 0.65f, 1f); // fallback color
            else
                return possibleMatch.color;
        }

        public static bool isFrequencyValid(int givenFreq)
        {
            if (givenFreq < 0 || givenFreq > short.MaxValue)
                return false;

            return true;
        }

        public static int countVesselsOf(Constellation thisConstellation)
        {
            List<CNCCommNetVessel> allVessels = CNCUtils.getCommNetVessels();
            return allVessels.Sum(i => (i.getRadioFrequency() == thisConstellation.frequency) ? 1 : 0);
        }

        
    }
}
