using CommNet;
using System;
using System.Collections.Generic;

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
            if (a == null || b == null)
            {
                return false;
            }

            return a.precisePosition == b.precisePosition;
        }

        /// <summary>
        /// Edit the connectivity between two potential nodes
        /// </summary>
        protected override bool SetNodeConnection(CommNode a, CommNode b)
        {
            List<short> aFreqs, bFreqs;

            try
            {
                //stop links between ground stations
                if (a.isHome && b.isHome)
                {
                    this.Disconnect(a, b, true);
                    return false;
                }

                //each CommNode has at least some frequencies?
                aFreqs = CNCCommNetScenario.Instance.getFrequencies(a);
                bFreqs = CNCCommNetScenario.Instance.getFrequencies(b);

                //share same frequency?
                for (int i = 0; i < aFreqs.Count; i++)
                {
                    if (bFreqs.Contains(aFreqs[i])) // yes, it does
                    {
                        return base.SetNodeConnection(a, b);
                    }
                }

                this.Disconnect(a, b, true);
                return false;
            }
            catch (Exception e) // either CommNode could be a kerbal on EVA
            {
                //CNCLog.Verbose("Error thrown when checking CommNodes '{0}' & '{1}' - {2}", a.name, b.name, e.Message);
                this.Disconnect(a, b, true);
                return false;
            }
        }
    }
}
