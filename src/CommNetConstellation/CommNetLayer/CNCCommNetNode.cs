using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetNode : CommNetNode
    {
        protected override void Start()
        {
            base.Start();
            if (CommNetNetwork.Initialized)
            {
                CNCCommNetNetwork.Add(this.comm);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!this.networkInitialised)
            {
                CNCCommNetNetwork.Remove(this.comm);
            }
        }

        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
            CNCCommNetNetwork.Add(this.comm);
        }

    }
}
