using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetBody : CommNetBody
    {
        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
            CNCCommNetNetwork.Add(this.occluder);
        }

    }
}
