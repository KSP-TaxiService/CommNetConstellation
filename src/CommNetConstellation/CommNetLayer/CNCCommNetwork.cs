using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Extend the functionality of the KSP's CommNetwork (secondary model in the Model–view–controller sense)
    /// </summary>
    public class CNCCommNetwork : CommNetwork
    {
        private short publicFreq = CNCSettings.Instance.PublicRadioFrequency;

        //IEqualityComparer<CommNode> comparer = commNode.Comparer; // a combination of third-party mods somehow  affects CommNode's IEqualityComparer on two objects
        //return commVessels.Find(x => comparer.Equals(commNode, x.Comm)).Vessel;
        /// <summary>
        /// Check if two CommNodes are the exact same object
        /// </summary>
        public static bool AreSame(CommNode a, CommNode b)
        {
            return a.precisePosition == b.precisePosition;
        }

        /// <summary>
        /// Edit the connectivity between two potential nodes
        /// </summary>
        protected override bool SetNodeConnection(CommNode a, CommNode b)
        {
            //stop links between ground stations
            if (a.isHome && b.isHome)
            {
                this.Disconnect(a, b, true);
                return false;
            }

            List<short> aFreqs, bFreqs;

            //each CommNode has at least some frequencies?
            try
            {
                aFreqs = CNCCommNetScenario.Instance.getFrequencies(a);
                bFreqs = CNCCommNetScenario.Instance.getFrequencies(b);
            }
            catch (NullReferenceException e) // either CommNode could be a kerbal on EVA
            {
                this.Disconnect(a, b, true);
                return false;
            }

            //share same frequency?
            IEnumerable<short> commonFreqs = aFreqs.Intersect(bFreqs);
            if (commonFreqs.Count() == 0) // no common element in two arrays
            {
                this.Disconnect(a, b, true);
                return false;
            }

            return base.SetNodeConnection(a, b);
        }
    }
}
