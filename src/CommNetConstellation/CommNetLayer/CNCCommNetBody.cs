using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommNetConstellation.CommNetLayer
{
    public class CNCCommNetBody : CommNetBody
    {
        public void copyOf(CommNetBody stockBody)
        {
            this.body = stockBody.GetComponentInChildren<CelestialBody>();
            this.occluder = stockBody.GetComponentInChildren<Occluder>(); // maybe too early as it is null at beginning
        }
    }
}
