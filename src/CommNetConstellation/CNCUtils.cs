using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using CommNetConstellation.CommNetLayer;

namespace CommNetConstellation
{
    public class CNCUtils
    {
        private static string TextureDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Name + "/Textures/";

        public static Texture2D loadImage(String fileName)
        {
            string str = TextureDirectory + fileName;
            if (GameDatabase.Instance.ExistsTexture(str))
                return GameDatabase.Instance.GetTexture(str, false);
            else
            {
                CNCLog.Error("Cannot find the texture '{0}': {1}", fileName, str);
                return Texture2D.blackTexture;
            }
        }

        public static List<CNCCommNetVessel> getCommNetVessels()
        {
            return getCommNetVessels(CNCSettings.Instance.PublicRadioFrequency); // don't hardcode (int radioFrequency = 0)
        }

        public static List<CNCCommNetVessel> getCommNetVessels(int targetRadioFrequency)
        {
            List<Vessel> vessels = FlightGlobals.Vessels;
            List<CNCCommNetVessel> commnetVessels = new List<CNCCommNetVessel>();

            for (int i=0; i<vessels.Count; i++)
            {
                Vessel thisVessel = vessels.ElementAt(i);
                if(thisVessel.Connection != null)
                {
                    CNCCommNetVessel cncVessel = (CNCCommNetVessel)thisVessel.Connection;
                    if(cncVessel.getRadioFrequency() == targetRadioFrequency)
                        commnetVessels.Add(cncVessel);
                }
            }

            return commnetVessels;
        }
    }
}
