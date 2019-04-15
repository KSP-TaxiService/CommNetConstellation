using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

namespace CommNetConstellation.UI
{
    /// <summary>
    /// Self-contained color picker, built on KSP's new DialogGUI components
    /// </summary>
    public class ColorPickerDialog : AbstractDialog
    {
        private Callback<Color> callbackForChosenColor;

        private int displayTextureWidth = 250;
        private int displayTextureHeight = 250;
        private DialogGUIImage colorPickerImage;
        private Texture2D colorPickerTexture;
        private DialogGUITextInput colorHexInput;

        private static int dialogWidth = 250 + 10;
        private static int dialogHeight = 300;

        private Color currentColor;
        private Color chosenColor;
        private DialogGUIImage newColorImage;

        private float hueValue = 0f;
        private int sliderHeight = 5;

        private bool buttonPressing = false;

        public ColorPickerDialog(Color userColor, Callback<Color> callbackForChosenColor) : base("colorpicker",
                                                                                                "Color Picker",
                                                                                                0.5f, //x
                                                                                                0.5f, //y
                                                                                                dialogWidth, //width
                                                                                                dialogHeight, //height
                                                                                                new DialogOptions[] { DialogOptions.NonDraggable })
        {
            this.dismissButtonText = "Apply";
            this.currentColor = userColor;
            this.chosenColor = userColor;
            this.callbackForChosenColor = callbackForChosenColor;

            this.colorPickerTexture = new Texture2D(displayTextureWidth, displayTextureHeight, TextureFormat.ARGB32, false);
            renderColorPicker(this.colorPickerTexture, hueValue);
        }

        /// <summary>
        /// Detect the cursor and react to it accordingly
        /// </summary>
        protected override void OnUpdate()
        {
            Vector2 cursor = Input.mousePosition;
            Vector3 pickerCenter = Camera.current.WorldToScreenPoint(colorPickerImage.uiItem.transform.position);

            if (!EventSystem.current.IsPointerOverGameObject())
                buttonPressing = false;

            if (pickerCenter.x- displayTextureWidth/2 <= cursor.x && cursor.x <= pickerCenter.x+displayTextureWidth/2 &&
                pickerCenter.y - displayTextureHeight / 2 <= cursor.y && cursor.y <= pickerCenter.y + displayTextureHeight / 2)
            {
                if(!buttonPressing && Input.GetMouseButtonDown(0)) // user pressing button
                {
                    buttonPressing = true;
                }
                else if(buttonPressing && Input.GetMouseButtonUp(0)) // user releasing button
                {
                    buttonPressing = false;
                }

                if (buttonPressing)
                {
                    int localX = (int)(cursor.x - (pickerCenter.x - displayTextureWidth/2));
                    int localY = (int)(cursor.y - (pickerCenter.y - displayTextureHeight/2));

                    renderColorPicker(colorPickerTexture, hueValue); // wipe out cursor data
                    chosenColor = colorPickerTexture.GetPixel(localX, localY);
                    newColorImage.uiItem.GetComponent<RawImage>().color = chosenColor;

                    colorPickerImage.uiItem.GetComponent<RawImage>().texture = drawCursorOn(colorPickerTexture, localX, localY);
                }
            }
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> listComponments = new List<DialogGUIBase>();

            DialogGUILabel newColorLabel = new DialogGUILabel("<b>  New</b>", 40, 12);
            newColorImage = new DialogGUIImage(new Vector2(30, 24), Vector2.zero, chosenColor, UIUtils.createAndColorize(30, 24, Color.white)); 
            DialogGUILabel currentColorLabel = new DialogGUILabel("<b>Current  </b>", 45, 12);
            DialogGUIImage currentColorImage = new DialogGUIImage(new Vector2(30, 24), Vector2.zero, currentColor, UIUtils.createAndColorize(30, 24, Color.white));
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUISpace(40), newColorImage, newColorLabel, new DialogGUISpace(dialogWidth - 80 - 145), currentColorLabel, currentColorImage, new DialogGUISpace(40) }));

            colorPickerImage = new DialogGUIImage(new Vector2(displayTextureWidth, displayTextureHeight), Vector2.zero, Color.white, colorPickerTexture);
            DialogGUIImage hueSliderImage = new DialogGUIImage(new Vector2(displayTextureWidth, sliderHeight * 2), Vector2.zero, Color.white, renderHueSliderTexture());
            DialogGUISlider hueSlider = new DialogGUISlider(() => hueValue, 0f, 1f, false, displayTextureWidth, sliderHeight, setHueValue);
            listComponments.Add(new DialogGUIVerticalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { colorPickerImage, new DialogGUISpace(5f), hueSliderImage, hueSlider }));

            DialogGUILabel hexColorLabel = new DialogGUILabel("<b>Or hex color <size=15>#</size></b>", true, false);
            colorHexInput = new DialogGUITextInput("", false, 6, setColorHexString, 75, 24);
            DialogGUIButton hexGoButton = new DialogGUIButton("Parse", delegate { this.readColorHexString(); }, 40, 24, false);
            listComponments.Add(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(0,0,0,5), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUISpace(5), hexColorLabel, colorHexInput, new DialogGUISpace(3), hexGoButton, new DialogGUISpace(5) }));

            return listComponments;
        }

        /// <summary>
        /// Deallocate the textures
        /// </summary>
        protected override void OnPreDismiss()
        {
            UnityEngine.GameObject.DestroyImmediate(colorPickerTexture, true);
            callbackForChosenColor(chosenColor);
        }

        /// <summary>
        /// Paint the colorful texture based on the given hue
        /// </summary>
        private void renderColorPicker(Texture2D thisTexture, float hueValue)
        {
            for (int x = 0; x < displayTextureWidth; x++)
            {
                for (int y = 0; y < displayTextureHeight; y++)
                {
                    float h = hueValue;
                    float v = (y / (displayTextureHeight * 1.0f)) * 1f;
                    float s = (x / (displayTextureWidth * 1.0f)) * 1f;
                    thisTexture.SetPixel(x, y, new ColorHSV(h, s, v).ToColor());
                }
            }
            thisTexture.Apply();
        }

        /// <summary>
        /// Just a slider of hue values
        /// </summary>
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

        /// <summary>
        /// For the hue slider to call when a player chooses
        /// </summary>
        private void setHueValue(float newValue)
        {
            this.hueValue = newValue;
            renderColorPicker(colorPickerTexture, newValue);
            colorPickerImage.uiItem.GetComponent<RawImage>().texture = colorPickerTexture;
        }

        /// <summary>
        /// For the color hex field to call upon new input
        /// </summary>
        private string setColorHexString(string newHexString)
        {
            //do nothing
            return newHexString;
        }

        /// <summary>
        /// Action to parse hex string and update chosen color
        /// </summary>
        private void readColorHexString()
        {
            try
            {
                string hexStr = "#"+colorHexInput.uiItem.GetComponent<TMP_InputField>().text.Trim();

                if (hexStr.Length < 7) //nice try
                {
                    throw new Exception("Hex string must be in format RRGGBB");
                }

                if (!ColorUtility.TryParseHtmlString(hexStr, out chosenColor))
                {
                    throw new Exception("Unable to parse hex string");
                }
                else//all ok!
                {
                    newColorImage.uiItem.GetComponent<RawImage>().color = chosenColor;
                }
            }
            catch(Exception e)
            {
                ScreenMessage msg = new ScreenMessage("<color=red>"+e.Message+"</color>", CNCSettings.ScreenMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                ScreenMessages.PostScreenMessage(msg);
            }
        }

        /// <summary>
        /// Paint the cursor mark on the given texture like the Photoshop's color picker
        /// </summary>
        private Texture2D drawCursorOn(Texture2D thisTexture, int x, int y)
        {
            Color cursorColor = Color.white;

            if (x - 2 >= 0) // left arm
                thisTexture.SetPixel(x-2, y, cursorColor);
            if (x - 3 >= 0)
                thisTexture.SetPixel(x-3, y, cursorColor);

            if (x +2  <= thisTexture.width) // right arm
                thisTexture.SetPixel(x + 2, y, cursorColor);
            if (x + 3 <= thisTexture.width)
                thisTexture.SetPixel(x + 3, y, cursorColor);

            if (y - 2 >= 0) // legs
                thisTexture.SetPixel(x, y - 2, cursorColor);
            if (y - 3 >= 0)
                thisTexture.SetPixel(x, y - 3, cursorColor);

            if (y + 2 <= thisTexture.height) // head
                thisTexture.SetPixel(x, y + 2, cursorColor);
            if (y + 3 <= thisTexture.height)
                thisTexture.SetPixel(x, y + 3, cursorColor);

            thisTexture.Apply();
            return thisTexture;
        }
    }
}
