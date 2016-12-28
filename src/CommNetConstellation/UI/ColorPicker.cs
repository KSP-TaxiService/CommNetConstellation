using UnityEngine;

// credit to Brian Jones (https://github.com/boj)
// obtained from https://gist.github.com/boj/1181465 (warning: the original author's codes were written hurriedly, resulting in obvious bugs)
// license - not found; I think it is released to public domain
namespace CommNetConstellation
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class ColorPicker : MonoBehaviour
    {
        public bool showPicker = true;

        private Texture2D displayPicker;
        public int displayTextureWidth = 360;
        public int displayTextureHeight = 360;

        public int positionLeft;
        public int positionTop;

        public Color chosenColor;
        private Texture2D chosenColorTexture;

        private float hueSlider = 0f;
        private float prevHueSlider = 0f;
        private Texture2D hueTexture;
        
        protected void Awake()
        {
            positionLeft = (Screen.width / 2) - (displayTextureWidth / 2);
            positionTop = (Screen.height / 2) - (displayTextureHeight / 2);

            renderColorPicker();

            hueTexture = new Texture2D(10, displayTextureHeight, TextureFormat.ARGB32, false);
            for (int x = 0; x < hueTexture.width; x++)
            {
                for (int y = 0; y < hueTexture.height; y++)
                {
                    float h = (y / (hueTexture.height*1.0f)) * 1f;
                    hueTexture.SetPixel(x, y, new ColorHSV(h, 1f, 1f).ToColor());
                }
            }
            hueTexture.Apply();

            // small color picker box texture
            chosenColorTexture = new Texture2D(1, 1);
            chosenColorTexture.SetPixel(0, 0, chosenColor);
        }

        private void renderColorPicker()
        {
            Texture2D colorPicker = new Texture2D(displayTextureWidth, displayTextureHeight, TextureFormat.ARGB32, false);
            for (int x = 0; x < displayTextureWidth; x++)
            {
                for (int y = 0; y < displayTextureHeight; y++)
                {
                    float h = hueSlider;
                    float v = (y / (displayTextureHeight * 1.0f)) * 1f;
                    float s = (x / (displayTextureWidth * 1.0f)) * 1f;
                    colorPicker.SetPixel(x, y, new ColorHSV(h, s, v).ToColor());
                }
            }

            colorPicker.Apply();
            displayPicker = colorPicker;
        }

        protected void OnGUI()
        {
            if (!showPicker) return;

            GUI.Box(new Rect(positionLeft - 3, positionTop - 3, displayTextureWidth + 60, displayTextureHeight + 60), "");

            if (hueSlider != prevHueSlider) // new Hue value
            {
                prevHueSlider = hueSlider;
                renderColorPicker();
            }

            if (GUI.RepeatButton(new Rect(positionLeft, positionTop, displayTextureWidth, displayTextureHeight), displayPicker))
            {
                int a = (int)Input.mousePosition.x;
                int b = Screen.height - (int)Input.mousePosition.y;

                chosenColor = displayPicker.GetPixel(a - positionLeft, -(b - positionTop));
            }

            hueSlider = GUI.VerticalSlider(new Rect(positionLeft + displayTextureWidth + 3, positionTop, 10, displayTextureHeight), hueSlider, 1, 0);
            GUI.Box(new Rect(positionLeft + displayTextureWidth + 20, positionTop, 20, displayTextureHeight), hueTexture);

            if (GUI.Button(new Rect(positionLeft + displayTextureWidth - 60, positionTop + displayTextureHeight + 10, 60, 25), "Apply"))
            {
                chosenColor = chosenColorTexture.GetPixel(0, 0);
                showPicker = false;
            }

            // box for chosen color
            GUIStyle style = new GUIStyle();
            chosenColorTexture.SetPixel(0, 0, chosenColor);
            chosenColorTexture.Apply();
            style.normal.background = chosenColorTexture;
            GUI.Box(new Rect(positionLeft + displayTextureWidth + 10, positionTop + displayTextureHeight + 10, 30, 30), new GUIContent(""), style);
        }
    }
}
