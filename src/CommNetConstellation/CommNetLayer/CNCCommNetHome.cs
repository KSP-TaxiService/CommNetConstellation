using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetHome : CommNetHome
    {
        public void copyOf(CommNetHome stockHome)
        {
            this.nodeName = stockHome.nodeName;
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;
            this.comm = stockHome.GetComponentInChildren<CommNode>(); // maybe too early as it is null at beginning
            this.body = stockHome.GetComponentInChildren<CelestialBody>(); // maybe too early as it is null at beginning
        }

        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
            if (this.comm != null)
            {
                CNCCommNetNetwork.Add(this.comm);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.comm != null)
            {
                CNCCommNetNetwork.Remove(this.comm);
            }
        }

        protected override void CreateNode()
        {
            base.CreateNode();
            if (HighLogic.CurrentGame != null && !HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations && !this.isKSC)
            {
                if (this.comm != null)
                {
                    CNCCommNetNetwork.Remove(this.comm);
                    this.comm = null;
                }
                return;
            }
        }
    }
}
