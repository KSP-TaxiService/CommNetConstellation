using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

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

        private bool buttonPressing = false;

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
            Vector2 b = Input.mousePosition;
            Vector3 a = Camera.current.WorldToScreenPoint(colorPickerImage.uiItem.transform.position);
            //CNCLog.Debug(EventSystem.current.IsPointerOverGameObject()+", "+ Input.GetMouseButtonDown(0) + ", " + Input.GetMouseButtonUp(0)+" "+a.x+" "+a.y);
            //CNCLog.Debug(a.x + " " + a.y+" "+b.x+" "+b.y);
            

            if (a.x- displayTextureWidth/2 <= b.x && b.x <= a.x+displayTextureWidth/2 &&
                a.y - displayTextureHeight / 2 <= b.y && b.y <= a.y + displayTextureHeight / 2)
            {
                //CNCLog.Debug(EventSystem.current.IsPointerOverGameObject() + ", " + Input.GetMouseButtonDown(0) + ", " + Input.GetMouseButtonUp(0));
                if(!buttonPressing && Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject()) // user pressing button
                {
                    buttonPressing = true;
                }
                else if(buttonPressing && Input.GetMouseButtonUp(0) && EventSystem.current.IsPointerOverGameObject()) // user releasing button
                {
                    buttonPressing = false;
                }

                if (buttonPressing)
                {
                    int localX = (int)(b.x - a.x);
                    int localY = (int)(b.y - a.y); ;

                    chosenColor = colorPickerTexture.GetPixel(localX, localY);
                    chosenColorTexture = UIUtils.createAndColorize(30, 24, chosenColor);
                    newColorImage.uiItem.GetComponent<RawImage>().texture = chosenColorTexture;
                }
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
        }

        private void applyClick() // TODO: to be replaced by abstract dialog's close button's callback & text
        {
            callbackForChosenColor(chosenColor);
            this.dismiss();
        }
    }
}
