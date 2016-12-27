using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using CommNetConstellation.CommNetLayer;
using CommNet;

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

        public static List<CNCCommNetVessel> getCommNetVessels(short targetRadioFrequency = -1)
        {
            List<Vessel> vessels = FlightGlobals.Vessels;
            List<CNCCommNetVessel> commnetVessels = new List<CNCCommNetVessel>();

            for (int i=0; i<vessels.Count; i++)
            {
                Vessel thisVessel = vessels.ElementAt(i);
                if(thisVessel.Connection != null)
                {
                    CNCCommNetVessel cncVessel = (CNCCommNetVessel)thisVessel.Connection;
                    if(cncVessel.getRadioFrequency() == targetRadioFrequency || targetRadioFrequency == -1)
                        commnetVessels.Add(cncVessel);
                }
            }

            return commnetVessels;
        }

        public static Vessel findCorrespondingVessel(CommNode commNodeRef)
        {
            List<Vessel> allVessels = FlightGlobals.Vessels;
            IEqualityComparer<CommNode> comparer = commNodeRef.Comparer;

            //brute-force search temporarily until I find a \omega(n) method
            for (int i = 0; i < allVessels.Count(); i++)
            {
                Vessel thisVessel = allVessels.ElementAt(i);
                if (thisVessel.connection != null)
                {
                    if (comparer.Equals(commNodeRef, thisVessel.connection.Comm))
                        return thisVessel;
                }
            }

            //not found
            return null;
        }

        //http://answers.unity3d.com/questions/1102232/how-to-get-the-color-code-in-rgb-hex-from-rgba-uni.html
        public static string colorToHex(Color thisColor) { return string.Format("#{0:X2}{1:X2}{2:X2}", toByte(thisColor.r), toByte(thisColor.g), toByte(thisColor.b)); }
        private static byte toByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}
