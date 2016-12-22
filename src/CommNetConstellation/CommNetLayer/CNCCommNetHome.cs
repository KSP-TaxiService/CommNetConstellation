using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetHome : CommNetHome
    {
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
