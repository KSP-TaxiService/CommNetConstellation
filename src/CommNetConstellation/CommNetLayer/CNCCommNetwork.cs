using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;

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
            List<short> aFreqs, bFreqs;
            bool aMembershipFlag, bMembershipFlag;

            try
            {
                aFreqs = CNCCommNetScenario.Instance.getFrequencies(a);
                bFreqs = CNCCommNetScenario.Instance.getFrequencies(b);

                aMembershipFlag = (a.isHome) ? true : ((CNCCommNetVessel)CNCCommNetScenario.Instance.findCorrespondingVessel(a).Connection).getMembershipFlag();
                bMembershipFlag = (b.isHome) ? true : ((CNCCommNetVessel)CNCCommNetScenario.Instance.findCorrespondingVessel(b).Connection).getMembershipFlag();
            }
            catch (NullReferenceException e) // either CommNode could be a kerbal on EVA
            {
                this.Disconnect(a, b, true);
                return false;
            }

            //TODO: get rid of membership once vessel-frequency list is implemented

            if (!aMembershipFlag && !bMembershipFlag && 
                !aFreqs.Contains(publicFreq) && !bFreqs.Contains(publicFreq) &&
                aFreqs.Intersect(bFreqs).Count()==0)
            {
                this.Disconnect(a, b, true);
                return false;
            }

            if (!aMembershipFlag)
            {
                aFreqs.Add(publicFreq);
            }
            if (!bMembershipFlag)
            {
                bFreqs.Add(publicFreq);
            }

            int numCommonElements = aFreqs.Intersect(bFreqs).Count();
            if (numCommonElements == 0) // no common element in two arrays
            {
                this.Disconnect(a, b, true);
                return false;
            }

            return base.SetNodeConnection(a, b);
        }
    }
}
