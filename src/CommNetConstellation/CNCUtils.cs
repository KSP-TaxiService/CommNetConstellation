using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

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
    }
}
