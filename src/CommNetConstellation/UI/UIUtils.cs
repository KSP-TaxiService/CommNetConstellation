using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Swiss Army knife for interface use cases
    /// </summary>
    public class UIUtils
    {
        private static string _TextureDirectory = ""; // one-time calculation for performance reason
        private static string TextureDirectory
        {
            get
            {
                if (_TextureDirectory.Length <= 0)
                {
                    _TextureDirectory = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name.Equals("CommNetConstellation")).url.Replace("Plugins", "Textures") + "/";
                }
                return _TextureDirectory;
            }
        }

        /// <summary>
        /// Read KSP's GameDatabase for the desired texture
        /// </summary>
        public static Texture2D loadImage(string fileName)
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

        /// <summary>
        /// Convert the color to hex string (#RRGGBB)
        /// </summary>
        //http://answers.unity3d.com/questions/1102232/how-to-get-the-color-code-in-rgb-hex-from-rgba-uni.html
        public static string colorToHex(Color thisColor) { return string.Format("#{0:X2}{1:X2}{2:X2}", toByte(thisColor.r), toByte(thisColor.g), toByte(thisColor.b)); }
        private static byte toByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        /// <summary>
        /// Create new texture and fill up with given color
        /// </summary>
        //https://forum.unity3d.com/threads/best-easiest-way-to-change-color-of-certain-pixels-in-a-single-sprite.223030/
        public static Texture2D createAndColorize(int width, int height, Color thisColor)
        {
            Texture2D newTexture = new Texture2D(width, height);
            newTexture.filterMode = FilterMode.Point;
            newTexture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    newTexture.SetPixel(x, y, thisColor);
                }
            }

            newTexture.Apply();
            return newTexture;
        }

        /// <summary>
        /// Fill the existing texture with the given color
        /// </summary>
        public static void colorize(Texture2D existingTexture, Color thisColor)
        {
            for (int y = 0; y < existingTexture.height; y++)
            {
                for (int x = 0; x < existingTexture.width; x++)
                {
                    existingTexture.SetPixel(x, y, thisColor);
                }
            }

            existingTexture.Apply();
        }

        /// <summary>
        /// Overlay two base and topmost textures to create a new texture
        /// </summary>
        public static Texture2D createAndOverlay(Texture2D baseTexture, Texture2D frontTexture)
        {
            Texture2D newTexture = new Texture2D(baseTexture.width, baseTexture.height);
            newTexture.filterMode = FilterMode.Point;
            newTexture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    if (frontTexture.GetPixel(x, y).a <= 0f) // transparent
                        newTexture.SetPixel(x, y, baseTexture.GetPixel(x, y));
                    else
                        newTexture.SetPixel(x, y, frontTexture.GetPixel(x, y));
                }
            }

            newTexture.Apply();
            return newTexture;
        }

        /// <summary>
        /// Build a button style for the different states
        /// </summary>
        public static UIStyle createImageButtonStyle(Texture2D iconTexture)
        {
            Texture2D activeButtonTx = createAndOverlay(loadImage("activeButtonBg"), iconTexture);
            Texture2D disableButtonTx = createAndOverlay(loadImage("disableButtonBg"), iconTexture);
            Texture2D hoverButtonTx = createAndOverlay(loadImage("hoverButtonBg"), iconTexture);
            Texture2D pressButtonTx = createAndOverlay(loadImage("pressButtonBg"), iconTexture);

            UIStyle buttonStyle = new UIStyle();
            float width = iconTexture.width;
            float height = iconTexture.height;

            buttonStyle.normal = new UIStyleState();
            buttonStyle.normal.background = Sprite.Create(activeButtonTx, new Rect(0, 0, width, height), Vector2.zero);
            buttonStyle.normal.textColor = Color.green;

            buttonStyle.highlight = new UIStyleState();
            buttonStyle.highlight.background = Sprite.Create(hoverButtonTx, new Rect(0, 0, width, height), Vector2.zero);
            buttonStyle.highlight.textColor = Color.green;

            buttonStyle.active = new UIStyleState();
            buttonStyle.active.background = Sprite.Create(pressButtonTx, new Rect(0, 0, width, height), Vector2.zero);
            buttonStyle.active.textColor = Color.green;

            buttonStyle.disabled = new UIStyleState();
            buttonStyle.disabled.background = Sprite.Create(disableButtonTx, new Rect(0, 0, width, height), Vector2.zero);
            buttonStyle.disabled.textColor = Color.green;

            return buttonStyle;
        }

        /// <summary>
        /// Cursor detection within the given window
        /// </summary>
        public static bool ContainsMouse(Rect window)
        {
            return window.Contains(new Vector2(Input.mousePosition.x,
                Screen.height - Input.mousePosition.y));
        }

        /// <summary>
        /// Easy method to convert list to string 
        /// </summary>
        public static string Concatenate<T>(IEnumerable<T> source, string delimiter)
        {
            var s = new StringBuilder();
            bool first = true;
            foreach (T t in source)
            {
                if (first)
                    first = false;
                else
                    s.Append(delimiter);
                s.Append(t);
            }
            return s.ToString();
        }
    }
}
