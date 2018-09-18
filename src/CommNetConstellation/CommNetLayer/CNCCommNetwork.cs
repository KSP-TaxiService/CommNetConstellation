using CommNet;
using CommNetConstellation.UI;
using System;

namespace CommNetConstellation.CommNetLayer
{
    /// <summary>
    /// Extend the functionality of the KSP's CommNetwork (secondary model in the Model–view–controller sense)
    /// </summary>
    public class CNCCommNetwork : CommNetwork
    {
        private const int REFRESH_TICKS = 50;
        private int mTick = 0, mTickIndex = 0;

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
            try
            {
                //stop links between ground stations
                if (a.isHome && b.isHome)
                {
                    this.Disconnect(a, b, true);
                }
                else
                {
                    //each CommNode has at least some frequencies?
                    if (GameUtils.firstCommonElement(CNCCommNetScenario.Instance.getFrequencies(a), CNCCommNetScenario.Instance.getFrequencies(b)) >= 0)
                    {
                        return base.SetNodeConnection(a, b);
                    }
                    else
                    {
                        this.Disconnect(a, b, true);
                    }
                }
            }
            catch (Exception e) // either CommNode could be a kerbal on EVA
            {
                //CNCLog.Verbose("Error thrown when checking CommNodes '{0}' & '{1}' - {2}", a.name, b.name, e.Message);
                this.Disconnect(a, b, true);
            }

            return false;
        }

        protected override void UpdateNetwork()
        {
            /*
            UpdateNetwork is part of a Unity physical frame, which happens few times per time second (4?)
            This optimisation is to spread the full workload of connection check to few frames, instead of every frame, as RemoteTech does
            Directly copied from RemoteTech codebase
            */
            var count = this.nodes.Count;
            if (count == 0) { return; }

            var baseline = (count / REFRESH_TICKS);
            var takeCount = baseline + (((mTick++ % REFRESH_TICKS) < (count - baseline * REFRESH_TICKS)) ? 1 : 0);

            for (int i = mTickIndex ; i < mTickIndex + takeCount; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    this.SetNodeConnection(this.nodes[i], this[j]);
                }
            }

            mTickIndex += takeCount;
            mTickIndex = mTickIndex % this.nodes.Count;
        }
    }
}
