using UnityEngine;
using System.Collections;

// relies on: http://forum.unity3d.com/threads/12031-create-random-colors?p=84625&viewfull=1#post84625

namespace CommNetConstellation
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class ColorPicker : MonoBehaviour
    {

        public bool useDefinedPosition = true;
        public int positionLeft = 300;
        public int positionTop = 300;

        // the solid texture which everything is compared against
        public Texture2D colorPicker = null;

        // the picker being displayed
        private Texture2D displayPicker;

        // the color that has been chosen
        public Color setColor;
        private Color lastSetColor;

        public bool useDefinedSize = false;
        public int textureWidth = 360;
        public int textureHeight = 120;

        private float saturationSlider = 0.0F;
        private Texture2D saturationTexture;

        private Texture2D styleTexture;

        public bool showPicker = true;

        void Awake()
        {
            if (!useDefinedPosition)
            {
                positionLeft = (Screen.width / 2) - (textureWidth / 2);
                positionTop = (Screen.height / 2) - (textureHeight / 2);
            }

            // if a default color picker texture hasn't been assigned, make one dynamically
            if (colorPicker == null)
            {
                colorPicker = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
                ColorHSV hsvColor;
                for (int i = 0; i < textureWidth; i++)
                {
                    for (int j = 0; j < textureHeight; j++)
                    {
                        hsvColor = new ColorHSV((float)i, (1.0f / j) * textureHeight, 1.0f);
                        colorPicker.SetPixel(i, j, hsvColor.ToColor());
                    }
                }
            }
            colorPicker.Apply();
            displayPicker = colorPicker;

            if (!useDefinedSize)
            {
                textureWidth = colorPicker.width;
                textureHeight = colorPicker.height;
            }

            float v = 0.0F;
            float diff = 1.0f / textureHeight;
            saturationTexture = new Texture2D(20, textureHeight);
            for (int i = 0; i < saturationTexture.width; i++)
            {
                for (int j = 0; j < saturationTexture.height; j++)
                {
                    saturationTexture.SetPixel(i, j, new Color(v, v, v));
                    v += diff;
                }
                v = 0.0F;
            }
            saturationTexture.Apply();

            // small color picker box texture
            styleTexture = new Texture2D(1, 1);
            styleTexture.SetPixel(0, 0, setColor);
        }

        void OnGUI()
        {
            if (!showPicker) return;

            GUI.Box(new Rect(positionLeft - 3, positionTop - 3, textureWidth + 60, textureHeight + 60), "");

            if (GUI.RepeatButton(new Rect(positionLeft, positionTop, textureWidth, textureHeight), displayPicker))
            {
                int a = (int)Input.mousePosition.x;
                int b = Screen.height - (int)Input.mousePosition.y;

                setColor = displayPicker.GetPixel(a - positionLeft, -(b - positionTop));
                lastSetColor = setColor;
            }

            saturationSlider = GUI.VerticalSlider(new Rect(positionLeft + textureWidth + 3, positionTop, 10, textureHeight), saturationSlider, 1, -1);
            setColor = lastSetColor + new Color(saturationSlider, saturationSlider, saturationSlider);
            GUI.Box(new Rect(positionLeft + textureWidth + 20, positionTop, 20, textureHeight), saturationTexture);

            if (GUI.Button(new Rect(positionLeft + textureWidth - 60, positionTop + textureHeight + 10, 60, 25), "Apply"))
            {
                setColor = styleTexture.GetPixel(0, 0);

                // hide picker
                //showPicker = false;
            }

            // color display
            GUIStyle style = new GUIStyle();
            styleTexture.SetPixel(0, 0, setColor);
            styleTexture.Apply();

            style.normal.background = styleTexture;
            GUI.Box(new Rect(positionLeft + textureWidth + 10, positionTop + textureHeight + 10, 30, 30), new GUIContent(""), style);
        }
    }
}
