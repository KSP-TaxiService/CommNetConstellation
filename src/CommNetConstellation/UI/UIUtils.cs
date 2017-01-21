using System.IO;
using System.Reflection;
using UnityEngine;

namespace CommNetConstellation.UI
{
    public class UIUtils
    {
        private static string TextureDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Name + "/Textures/";

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

        //http://answers.unity3d.com/questions/1102232/how-to-get-the-color-code-in-rgb-hex-from-rgba-uni.html
        public static string colorToHex(Color thisColor) { return string.Format("#{0:X2}{1:X2}{2:X2}", toByte(thisColor.r), toByte(thisColor.g), toByte(thisColor.b)); }
        private static byte toByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

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

        public static void colorize(Texture2D thisTexture, Color thisColor)
        {
            for (int y = 0; y < thisTexture.height; y++)
            {
                for (int x = 0; x < thisTexture.width; x++)
                {
                    thisTexture.SetPixel(x, y, thisColor);
                }
            }

            thisTexture.Apply();
        }

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
    }
}
