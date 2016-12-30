using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommNetConstellation.UI
{
    public class ColorPickerDialog : AbstractDialog
    {
        private Callback<Color> callbackForChosenColor;

        private Texture2D displayPicker;
        private int displayTextureWidth = 250;
        private int displayTextureHeight = 250;
        //private DialogGUIImage colorPickerImage;
        private DialogGUISprite colorPickerImage;

        private static int dialogWidth = 250 + 10 + 5 + 10;
        private static int dialogHeight = 300;

        private Color chosenColor;
        private Texture2D chosenColorTexture;

        private Color currentColor;
        private Texture2D currentColorTexture;

        private float hueSlider = 0f;
        private int sliderHeight = 5;
        private Texture2D hueTexture;

        public ColorPickerDialog(Color userColor, Callback<Color> callbackForChosenColor) : base("Color Picker",
                                                                                                0.5f, //x
                                                                                                0.5f, //y
                                                                                                dialogWidth, //width
                                                                                                dialogHeight, //height
                                                                                                new string[] { "hideclosebutton", "nodragging" }) //arguments
        {
            this.currentColor = userColor;
            this.chosenColor = userColor;
            this.callbackForChosenColor = callbackForChosenColor;

            renderColorPicker();
            renderHueSliderTexture();
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            DialogGUILabel newColorLabel = new DialogGUILabel("<b>  New</b>", 40, 12);
            DialogGUIImage newColorImage = new DialogGUIImage(new Vector2(30, 24), Vector2.zero, chosenColor, chosenColorTexture); 
            DialogGUILabel currentColorLabel = new DialogGUILabel("<b>Current  </b>", 45, 12);
            DialogGUIImage currentColorImage = new DialogGUIImage(new Vector2(30, 24), Vector2.zero, currentColor, currentColorTexture);
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUISpace(40), newColorImage, newColorLabel, new DialogGUISpace(dialogWidth - 80 - 145), currentColorLabel, currentColorImage, new DialogGUISpace(40) }));

            //colorPickerImage = new DialogGUIImage(new Vector2(displayTextureWidth, displayTextureHeight), Vector2.zero, Color.white, displayPicker);
            colorPickerImage = new DialogGUISprite(new Vector2(displayTextureWidth, displayTextureHeight), new Vector2(0, 0), Color.white, Sprite.Create(displayPicker, new Rect(0, 0, displayTextureWidth, displayTextureHeight), new Vector2(0, 0)));
            DialogGUIImage hueSliderImage = new DialogGUIImage(new Vector2(displayTextureWidth, sliderHeight * 2), Vector2.zero, Color.white, hueTexture);
            DialogGUISlider hueSlider = new DialogGUISlider(() => this.hueSlider, 0f, 1f, false, displayTextureWidth, sliderHeight, setHueValue);
            listComponments.Add(new DialogGUIVerticalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { colorPickerImage, new DialogGUISpace(5f), hueSliderImage, hueSlider }));

            DialogGUIButton applyButton = new DialogGUIButton("Apply", applyClick);
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), applyButton, new DialogGUIFlexibleSpace() }));
            return listComponments;
        }

        protected override bool runIntenseInfo(object[] args)
        {
            return true;
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

        private void renderHueSliderTexture()
        {
            hueTexture = new Texture2D(displayTextureWidth, sliderHeight * 2, TextureFormat.ARGB32, false);
            for (int x = 0; x < hueTexture.width; x++)
            {
                for (int y = 0; y < hueTexture.height; y++)
                {
                    float h = (x / (hueTexture.width* 1.0f)) * 1f;
                    hueTexture.SetPixel(x, y, new ColorHSV(h, 1f, 1f).ToColor());
                }
            }
            hueTexture.Apply();
        }

        private void setHueValue(float newValue)
        {
            this.hueSlider = newValue;
            renderColorPicker();
            //colorPickerImage.image = displayPicker;
            colorPickerImage.sprite = Sprite.Create(displayPicker, new Rect(0, 0, displayTextureWidth, displayTextureHeight), new Vector2(0, 0));
            //colorPickerImage.Dirty = true;
        }

        private void applyClick() // TODO: to be replaced by abstract dialog's close button's callback & text
        {
            callbackForChosenColor(chosenColor);
            this.dismiss();
        }
    }
}
