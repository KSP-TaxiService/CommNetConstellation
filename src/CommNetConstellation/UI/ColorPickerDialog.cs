using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommNetConstellation.UI
{
    public class ColorPickerDialog : AbstractDialog
    {
        private Callback<Color> callbackForChosenColor;

        private int displayTextureWidth = 250;
        private int displayTextureHeight = 250;
        private DialogGUIImage colorPickerImage;
        private Texture2D colorPickerTexture;

        private static int dialogWidth = 250 + 10 + 5 + 10;
        private static int dialogHeight = 300;

        private Color chosenColor;
        private Texture2D chosenColorTexture;
        private DialogGUIImage newColorImage;

        private Color currentColor;
        private Texture2D currentColorTexture;

        private float hueValue = 0f;
        private int sliderHeight = 5;

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

            this.chosenColorTexture = UIUtils.createAndColorize(30, 24, chosenColor);
            this.currentColorTexture = UIUtils.createAndColorize(30, 24, currentColor);
        }

        protected override void OnUpdate()
        {
            //TODO: Failure. Can't handle the scenario of user desiring leaving chosen color while tracking cursor within img. switch to button and press event
            if(detectCursorWithinColorPicker(Input.mousePosition))
            {
                //assumed the color picker is non-draggable
                int pickerCenterX = Screen.width / 2;
                int pickerCenterY = Screen.height / 2 + 139;

                chosenColor = colorPickerTexture.GetPixel((int)Input.mousePosition.x - pickerCenterX, (int)Input.mousePosition.y - pickerCenterY);
                chosenColorTexture = UIUtils.createAndColorize(30, 24, chosenColor);
                newColorImage.uiItem.GetComponent<RawImage>().texture = chosenColorTexture;
            }
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            DialogGUILabel newColorLabel = new DialogGUILabel("<b>  New</b>", 40, 12);
            newColorImage = new DialogGUIImage(new Vector2(30, 24), Vector2.zero, chosenColor, chosenColorTexture); 
            DialogGUILabel currentColorLabel = new DialogGUILabel("<b>Current  </b>", 45, 12);
            DialogGUIImage currentColorImage = new DialogGUIImage(new Vector2(30, 24), Vector2.zero, currentColor, currentColorTexture);
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUISpace(40), newColorImage, newColorLabel, new DialogGUISpace(dialogWidth - 80 - 145), currentColorLabel, currentColorImage, new DialogGUISpace(40) }));

            colorPickerImage = new DialogGUIImage(new Vector2(displayTextureWidth, displayTextureHeight), Vector2.zero, Color.white, (colorPickerTexture = renderColorPicker(hueValue)));
            DialogGUIImage hueSliderImage = new DialogGUIImage(new Vector2(displayTextureWidth, sliderHeight * 2), Vector2.zero, Color.white, renderHueSliderTexture());
            DialogGUISlider hueSlider = new DialogGUISlider(() => hueValue, 0f, 1f, false, displayTextureWidth, sliderHeight, setHueValue);
            listComponments.Add(new DialogGUIVerticalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { colorPickerImage, new DialogGUISpace(5f), hueSliderImage, hueSlider }));

            DialogGUIButton applyButton = new DialogGUIButton("Apply", applyClick);
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), applyButton, new DialogGUIFlexibleSpace() }));
            return listComponments;
        }

        protected override bool runIntenseInfo(object[] args)
        {
            return true;
        }

        private Texture2D renderColorPicker(float hueValue)
        {
            Texture2D colorPicker = new Texture2D(displayTextureWidth, displayTextureHeight, TextureFormat.ARGB32, false);
            for (int x = 0; x < displayTextureWidth; x++)
            {
                for (int y = 0; y < displayTextureHeight; y++)
                {
                    float h = hueValue;
                    float v = (y / (displayTextureHeight * 1.0f)) * 1f;
                    float s = (x / (displayTextureWidth * 1.0f)) * 1f;
                    colorPicker.SetPixel(x, y, new ColorHSV(h, s, v).ToColor());
                }
            }

            colorPicker.Apply();
            return colorPicker;
        }

        private Texture2D renderHueSliderTexture()
        {
            Texture2D hueTexture = new Texture2D(displayTextureWidth, sliderHeight * 2, TextureFormat.ARGB32, false);
            for (int x = 0; x < hueTexture.width; x++)
            {
                for (int y = 0; y < hueTexture.height; y++)
                {
                    float h = (x / (hueTexture.width* 1.0f)) * 1f;
                    hueTexture.SetPixel(x, y, new ColorHSV(h, 1f, 1f).ToColor());
                }
            }
            hueTexture.Apply();
            return hueTexture;
        }

        private void setHueValue(float newValue)
        {
            this.hueValue = newValue;
            colorPickerTexture = renderColorPicker(newValue);
            colorPickerImage.uiItem.GetComponent<RawImage>().texture = colorPickerTexture;

            UIUtils.colorizeFull(chosenColorTexture, new ColorHSV(newValue, 1f, 1f).ToColor());
            newColorImage.uiItem.GetComponent<RawImage>().texture = chosenColorTexture;
        }

        private void applyClick() // TODO: to be replaced by abstract dialog's close button's callback & text
        {
            callbackForChosenColor(chosenColor);
            this.dismiss();
        }

        //TODO: Need better detection and assume the dialog is draggable
        private bool detectCursorWithinColorPicker(Vector3 cursorPosition)
        {
            int x = (int)cursorPosition.x;
            int y = (int)cursorPosition.y;
            bool withinX = false;
            bool withinY = false;

            //assumed the color picker is non-draggable
            int pickerCenterX = Screen.width / 2;
            int pickerCenterY = Screen.height / 2 + 139;

            //CNCLog.Debug(cursorPosition.x + " " + cursorPosition.y);

            if (pickerCenterX - displayTextureWidth / 2 <= x && x <= pickerCenterX + displayTextureWidth / 2)
                withinX = true;

            if (pickerCenterY - displayTextureHeight / 2 <= y && y <= pickerCenterY + displayTextureHeight / 2)
                withinY = true;

            return withinX && withinY;
        }
    }
}
