using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommNetConstellation.UI
{
    public class ColorPickerDialog : AbstractDialog
    {
        private Color userColor;
        private Callback<Color> returnFunction;

        // the solid texture which everything is compared against
        public Texture2D colorPicker;

        // the picker being displayed
        private Texture2D displayPicker;

        // the color that has been chosen
        public Color setColor =  Color.green;
        private Color lastSetColor = Color.white;

        public int textureWidth = 360;
        public int textureHeight = 120;

        private float saturationSlider = 0.0F;
        private Texture2D saturationTexture;

        private Texture2D styleTexture;

        public ColorPickerDialog(Color userColor, Callback<Color> returnFunction) : base("Color Picker",
                                                                                            0.5f, //x
                                                                                            0.5f, //y
                                                                                            180, //width
                                                                                            180, //height
                                                                                            new string[] { "showclosebutton" }) //arguments
        {
            this.userColor = userColor;
            this.returnFunction = returnFunction;

            // if a default color picker texture hasn't been assigned, make one dynamically
            if (!colorPicker)
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

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            DialogGUILabel newColorLabel = new DialogGUILabel("<b>New</b>", 40, 12);
            DialogGUILabel currentColorLabel = new DialogGUILabel("<b>Current</b>", 40, 12);
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { newColorLabel, new DialogGUIFlexibleSpace(), currentColorLabel }));

            DialogGUIImage newColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, setColor, styleTexture);
            DialogGUIImage currentColorImage = new DialogGUIImage(new Vector2(32, 32), Vector2.one, lastSetColor, styleTexture);
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { newColorImage, new DialogGUIFlexibleSpace(), currentColorImage }));


            DialogGUIBox box1 = new DialogGUIBox("", textureWidth + 60, textureHeight + 60);
            listComponments.Add(box1);

            /*
            DialogGUIButton updateButton = new DialogGUIButton("Pick?", pick, false);
            DialogGUISlider slider = new DialogGUISlider(a, -1f, 1f, false, 140, 24, b);
            
            setColor = lastSetColor + new Color(saturationSlider, saturationSlider, saturationSlider);
            GUI.Box(new Rect(positionLeft + textureWidth + 20, positionTop, 20, textureHeight), saturationTexture);

            if (GUI.Button(new Rect(positionLeft + textureWidth - 60, positionTop + textureHeight + 10, 60, 25), "Apply"))
            {
                setColor = styleTexture.GetPixel(0, 0);
            }

            // color display
            GUIStyle style = new GUIStyle();
            styleTexture.SetPixel(0, 0, setColor);
            styleTexture.Apply();

            style.normal.background = styleTexture;
            GUI.Box(new Rect(positionLeft + textureWidth + 10, positionTop + textureHeight + 10, 30, 30), new GUIContent(""), style);
            */
            return listComponments;
        }

        private float a()
        {
            throw new NotImplementedException();
        }

        private void b(float arg0)
        {
            throw new NotImplementedException();
        }

        protected override bool runIntenseInfo(object[] args)
        {
            return true;
        }

        private void pick()
        {
            int a = (int)Input.mousePosition.x;
            int b = Screen.height - (int)Input.mousePosition.y;

            setColor = displayPicker.GetPixel(a, b);
            lastSetColor = setColor;
        }
    }
}
