using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace SwissArmyKnife
{
    ///=================================================================================================================
    ///                                                                                                       <summary>
    ///  ColorPickerTexture is a generic script for 1D and 2D gradients for a color picker.       			 </summary>
    ///
    ///=================================================================================================================
    [RequireComponent(typeof(RawImage))]
    public class ColorPickerTexture : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        #region Declarations and simple properties

        public enum ColorProperty { Red, Green, Blue, Hue, Saturation, Value, Alpha, Constant };

        [SerializeField]
        [Tooltip("What corresponds to the x direction?")]
        private ColorProperty _xProperty = ColorProperty.Saturation;

        [SerializeField]
        [Tooltip("Check to flip x direction")]
        private bool _flipX = false;

        [SerializeField]
        [Tooltip("What corresponds to the y direction?")]
        private ColorProperty _yProperty = ColorProperty.Value;

        [SerializeField]
        [Tooltip("Check to flip y direction")]
        private bool _flipY = false;

        [SerializeField]
        [Tooltip("Shall we adapt, for example, the saturation of a hue gradient to the color saturation? (Caution, setting this to true might impact performance as the texture has to be recomputed each time we change the color.)")]
        private bool _adaptGradientToOtherValues = false;

        [SerializeField]
        [Tooltip("The pickable color script")]
        private ColorPickerGroup _colorPickerGroup;

        [SerializeField]
        [Tooltip("The cursor on the texture")]
        private RectTransform _cursor;

        // The color used to complete the color when _adaptGradientToOtherValues == false 
        private Color _referenceColor;

        private bool _isScreenSpaceOverlay = true;
        private Texture2D _texture;

        #endregion
        #region Initialization
        //=================================================================================================================
        //
        // I N I T I A L I Z A T I O N 
        //
        //=================================================================================================================


        ///-------------------------------------------------------                                                  <summary>
        /// Use this for initialization                                                                             </summary>
        ///-------------------------------------------------------
        void Start()
        {
            // Determine _referenceColor
            _referenceColor = DetermineReferenceColor(_xProperty, _yProperty);

            // We want to be informed when the color changes.
            _colorPickerGroup.CallMeIfColorChanges(gameObject);

            CreateTexture();
            PositionCursor();

            // Find my canvas
            Transform isThisMyCanvas = transform.parent;
            while (isThisMyCanvas.GetComponent<Canvas>() == null)
            {
                isThisMyCanvas = isThisMyCanvas.parent;
            }

            // Check if we are in world space cancvas
            Canvas currentCanvas = isThisMyCanvas.GetComponent<Canvas>();
            _isScreenSpaceOverlay = (currentCanvas.renderMode == RenderMode.ScreenSpaceOverlay);
        }

        ///-------------------------------------------------------                                                  <summary>
        /// Determine the color used to "complete" picked color                                                    </summary>
        ///-------------------------------------------------------
        private Color DetermineReferenceColor(ColorProperty property1, ColorProperty property2)
        {
            switch (property1)
            {
                case ColorProperty.Red:
                case ColorProperty.Green:
                case ColorProperty.Blue:
                    return Color.black;

                case ColorProperty.Hue:
                case ColorProperty.Value:
                case ColorProperty.Saturation:
                    return Color.red;

                case ColorProperty.Constant:
                    if (property2 == ColorProperty.Constant )
                    {
                        Debug.LogError("Double constant property is not allowed");
                        return Color.magenta;
                    }
                    return DetermineReferenceColor( property2, property1 );

                default:
                    Debug.LogError("Unhandled property " + _xProperty);
                    return Color.magenta;
            }
        }

        #endregion
        #region Texture creation
        //=================================================================================================================
        //
        // T E X T U R E   C R E A T I O N 
        //
        //=================================================================================================================

        ///-------------------------------------------------------                                                  <summary>
        /// Create the texture                                                                                  </summary>
        ///-------------------------------------------------------
        void CreateTexture()
        {
            int width = (int)GetComponent<RectTransform>().rect.width;
            int height = (int)GetComponent<RectTransform>().rect.height;

            if (width < 0 || height < 0)
            {
                height = width = 100;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.wrapMode = TextureWrapMode.Clamp;
            _texture = texture;
            UpdateTexture();
        }

        ///-------------------------------------------------------                                                  <summary>
        /// UpdateTheTexture                                                                               </summary>
        ///-------------------------------------------------------
        void UpdateTexture()
        {
            if (_texture == null)
            {
                CreateTexture();
            }
            Texture2D texture = _texture;
            int width = texture.width;
            int height = texture.height;
            Color[] colors;

            if (_xProperty == ColorProperty.Constant)
            {
                // Constant color in x direction
                if (_yProperty == ColorProperty.Constant)
                {
                    // ---- Constant color in x and y direction  (color field) ----

                    // Fill an array with the color
                    colors = new Color[width * height];
                    for (int i = 0; i < width * height; i++)
                    {
                        colors[i] = _adaptGradientToOtherValues ? _colorPickerGroup.color : _referenceColor;
                    }

                    // Copy the array into the texture
                    texture.SetPixels(colors);

                }
                else
                {
                    //  ---- Color varies in y direction, constant in x direction  (vertical gradient) ----

                    colors = new Color[width];
                    for (int row = 0; row < height; row++)
                    {
                        // Compute color
                        float yValue = (float)row / height;
                        Color color = ComputeColor(_yProperty, yValue, _flipY, _adaptGradientToOtherValues);

                        // Fill row with color
                        for (int column = 0; column < width; column++)
                        {
                            colors[column] = color;
                        }
                        texture.SetPixels(0, row, width, 1, colors);
                    }
                }
            }
            else
            {
                // Color varies in x direction
                if (_yProperty == ColorProperty.Constant)
                {
                    //  ---- Color varies in x direction, constant in y direction (horizontal gradient)  ----

                    colors = new Color[height];
                    for (int column = 0; column < width; column++)
                    {
                        // Compute color
                        float xValue = (float)column / width;
                        Color color = ComputeColor(_xProperty, xValue, _flipX, _adaptGradientToOtherValues);

                        // Fill row with color
                        for (int row = 0; row < height; row++)
                        {
                            colors[row] = color;
                        }
                        texture.SetPixels(column, 0, 1, height, colors);
                    }
                }
                else
                {
                    //  ---- Color varies in both directions (color picking square)  ----
                    colors = new Color[width];
                    for (int row = 0; row < height; row++)
                    {
                        for (int column = 0; column < width; column++)
                        {
                            // Compute color
                            float xValue = (float)column / width;
                            float yValue = (float)row / height;
                            Color color = ComputeColor(_xProperty, xValue, _flipX, _yProperty, yValue, _flipY, _adaptGradientToOtherValues);

                            colors[column] = color;
                        }
                        texture.SetPixels(0, row, width, 1, colors);
                    }
                }
            }

            // Upload changes to GPU
            texture.Apply();

            // Assign texture
            GetComponent<RawImage>().texture = texture;
        }

        #endregion
        #region Cursor
        //=================================================================================================================
        //
        // C U R S O R
        //
        //=================================================================================================================


        ///-------------------------------------------------------                                                  <summary>
        /// Gets the value of colort picker's color depending on one property. For cursor position.                 </summary>
        ///-------------------------------------------------------
        private void PositionCursor()
        {
            // Anchor cursor in center
            _cursor.anchorMin = Vector2.one * 0.5f;
            _cursor.anchorMax = Vector2.one * 0.5f;

            // Compute width and height
            float width = GetComponent<RectTransform>().rect.width;
            float height = GetComponent<RectTransform>().rect.height;

            // Set anchored position (relative to center)
            Vector2 anchoredPosition = _cursor.GetComponent<RectTransform>().anchoredPosition;
            anchoredPosition.x = width * (GetValueForProperty(_xProperty, _flipX) - 0.5f);
            anchoredPosition.y = height * (GetValueForProperty(_yProperty, _flipY) - 0.5f);
            _cursor.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        }

        #endregion
        #region Auxiliary methods
        //=================================================================================================================
        //
        // A U X I L I A R Y   M E T H O D S
        //
        //=================================================================================================================


        ///-------------------------------------------------------                                                  <summary>
        /// Gets the value of colort picker's color depending on one property. For cursor position.                 </summary>
        ///-------------------------------------------------------
        private float GetValueForProperty(ColorProperty property, bool flip)
        {
            float value = 0.0f;
            Color colorRGB = _colorPickerGroup.color;
            ColorHSV colorHSV = (ColorHSV)colorRGB;

            switch (property)
            {
                case ColorProperty.Red: value = colorRGB.r; break;
                case ColorProperty.Green: value = colorRGB.g; break;
                case ColorProperty.Blue: value = colorRGB.b; break;
                case ColorProperty.Hue: value = colorHSV.h; break;
                case ColorProperty.Saturation: value = colorHSV.s; break;
                case ColorProperty.Value: value = colorHSV.v; break;
                case ColorProperty.Alpha: value = colorRGB.a; break;
                case ColorProperty.Constant: value = 0.5f; break;
            }

            if (flip)
            {
                value = 1.0f - value;
            }

            return value;
        }



        ///-------------------------------------------------------                                                  <summary>
        ///Computes the color depending on one property                                                             </summary>
        ///-------------------------------------------------------
        private Color ComputeColor(ColorProperty property, float value, bool flip, bool otherValuesFromPickableColor)
        {
            // Flip value
            if (flip)
            {
                value = 1.0f - value;
            }

            Color colorRGB = otherValuesFromPickableColor ? _colorPickerGroup.color : Color.black;
            ColorHSV colorHSV = otherValuesFromPickableColor ? (ColorHSV)colorRGB : (ColorHSV)Color.red; // In order to display full saturation color gradient

            switch (property)
            {
                case ColorProperty.Red:
                    return new Color(value, colorRGB.g, colorRGB.b, colorRGB.a);

                case ColorProperty.Green:
                    return new Color(colorRGB.r, value, colorRGB.b, colorRGB.a);

                case ColorProperty.Blue:
                    return new Color(colorRGB.r, colorRGB.g, value, colorRGB.a);

                case ColorProperty.Alpha:
                    return new Color(colorRGB.r, colorRGB.g, colorRGB.b, value);

                case ColorProperty.Hue:
                    return (Color)(new ColorHSV(value, colorHSV.s, colorHSV.v, colorHSV.a));

                case ColorProperty.Saturation:
                    return (Color)(new ColorHSV(colorHSV.h, value, colorHSV.v, colorHSV.a));

                case ColorProperty.Value:
                    return (Color)(new ColorHSV(colorHSV.h, colorHSV.s, value, colorHSV.a));

                default:
                    Debug.LogError("Unhandled property " + property);
                    return Color.magenta;
            }
        }

        ///-------------------------------------------------------                                                  <summary>
        ///Computes the color depending on two properties                                                             </summary>
        ///-------------------------------------------------------
        private Color ComputeColor(ColorProperty property1, float value1, bool flip1, ColorProperty property2, float value2, bool flip2, bool otherValuesFromPickableColor)
        {
            if (!ArePropertiesConsistent(property1, property2))
            {
                return Color.magenta;
            }

            // Flip values
            if (flip1)
            {
                value1 = 1.0f - value1;
            }
            if (flip2)
            {
                value2 = 1.0f - value2;
            }

            Color colorRGB = otherValuesFromPickableColor ? _colorPickerGroup.color : _referenceColor;
            ColorHSV colorHSV = (ColorHSV)colorRGB;

            switch (property1)
            {
                case ColorProperty.Red:
                    switch (property2)
                    {
                        case ColorProperty.Green:
                            return new Color(value1, value2, colorRGB.b, colorRGB.a);

                        case ColorProperty.Blue:
                            return new Color(value1, colorRGB.g, value2, colorRGB.a);

                        default: // Shouldn't happen
                            return Color.magenta;
                    }

                case ColorProperty.Green:
                    switch (property2)
                    {
                        case ColorProperty.Red:
                            return new Color(value2, value1, colorRGB.b, colorRGB.a);

                        case ColorProperty.Blue:
                            return new Color(colorRGB.r, value1, value2, colorRGB.a);

                        default: // Shouldn't happen
                            return Color.magenta;
                    }

                case ColorProperty.Blue:
                    switch (property2)
                    {
                        case ColorProperty.Red:
                            return new Color(value2, colorRGB.g, value1, colorRGB.a);

                        case ColorProperty.Green:
                            return new Color(colorRGB.r, value2, value1, colorRGB.a);

                        default: // Shouldn't happen
                            return Color.magenta;
                    }

                case ColorProperty.Hue:
                    switch (property2)
                    {
                        case ColorProperty.Saturation:
                            return (Color)new ColorHSV(value1, value2, colorHSV.v, colorHSV.a);

                        case ColorProperty.Value:
                            return (Color)new ColorHSV(value1, colorHSV.s, value2, colorHSV.a);

                        default: // Shouldn't happen
                            return Color.magenta;
                    }

                case ColorProperty.Saturation:
                    switch (property2)
                    {
                        case ColorProperty.Hue:
                            return (Color)new ColorHSV(value2, value1, colorHSV.v, colorHSV.a);

                        case ColorProperty.Value:
                            return (Color)new ColorHSV(colorHSV.h, value1, value2, colorHSV.a);

                        default: // Shouldn't happen
                            return Color.magenta;
                    }

                case ColorProperty.Value:
                    switch (property2)
                    {
                        case ColorProperty.Hue:
                            return (Color)new ColorHSV(value2, colorHSV.s, value1, colorHSV.a);

                        case ColorProperty.Saturation:
                            return (Color)new ColorHSV(colorHSV.h, value2, value1, colorHSV.a);

                        default: // Shouldn't happen
                            return Color.magenta;
                    }

                default:
                    Debug.LogError("Unhandled property " + property1);
                    return Color.magenta;
            }
        }

        ///-------------------------------------------------------                                                  <summary>
        /// Tells if you can combine two properties                                                             </summary>
        ///-------------------------------------------------------
        private bool ArePropertiesConsistent(ColorProperty property1, ColorProperty property2)
        {
            if (property1 == property2)
            {
                Debug.LogWarning("Can't combine two identical properties " + property1);
                return false;
            }

            if (property1 == ColorProperty.Alpha || property2 == ColorProperty.Alpha)
            {
                Debug.LogWarning("Can't combine Alpha with other property");
                return false;
            }

            switch (property1)
            {
                case ColorProperty.Red:
                case ColorProperty.Green:
                case ColorProperty.Blue:
                    switch (property2)
                    {
                        case ColorProperty.Hue:
                        case ColorProperty.Saturation:
                        case ColorProperty.Value:
                            Debug.LogWarning("Can't combine RGB property " + property1 + "with HSV property " + property2);
                            return false;
                    }
                    break;

                case ColorProperty.Hue:
                case ColorProperty.Saturation:
                case ColorProperty.Value:
                    switch (property2)
                    {
                        case ColorProperty.Red:
                        case ColorProperty.Green:
                        case ColorProperty.Blue:
                            Debug.LogWarning("Can't combine HSV property " + property1 + "with RGB property " + property2);
                            return false;
                    }
                    break;
            }

            return true;
        }

        #endregion
        #region Events
        //=================================================================================================================
        //
        // E V E N T S
        //
        //=================================================================================================================

        ///-------------------------------------------------------                                                  <summary>
        /// Called when someone changed the color                                                                        </summary>
        ///-------------------------------------------------------
        void OnColorChanged(Color color)
        {
            if (_adaptGradientToOtherValues)
            {
                UpdateTexture();
            }
            PositionCursor();
        }

        ///-------------------------------------------------------                                                  <summary>
        /// Called when someone draged the slider                                                                       </summary>
        ///-------------------------------------------------------
        public void OnDrag(PointerEventData eventData)
        {
            HandleClickOrDrag(eventData);

        }

        ///-------------------------------------------------------                                                  <summary>
        /// Called when someone clicked down onto the etxture                                                            </summary>
        ///-------------------------------------------------------
        public void OnPointerDown(PointerEventData eventData)
        {
            HandleClickOrDrag(eventData);
        }

        ///-------------------------------------------------------                                                  <summary>
        /// CDoes the work for OnDrag and   OnPointerDown                                                          </summary>
        ///-------------------------------------------------------
        private void HandleClickOrDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, _isScreenSpaceOverlay ? null : Camera.main, out localPoint);

            // Compute width and height
            float width = GetComponent<RectTransform>().rect.width;
            float height = GetComponent<RectTransform>().rect.height;

            // Normalize local point
            localPoint.x = Mathf.Clamp(localPoint.x / width + 0.5f, 0.01f, 0.99f);
            localPoint.y = Mathf.Clamp(localPoint.y / height + 0.5f, 0.01f, 0.99f);

            // Compute color
            ComputeColorFromLocalPoint(localPoint);
        }

        ///-------------------------------------------------------                                                     <summary>
        /// Updates the color from a position                                                                       </summary>
        ///-------------------------------------------------------
        private void ComputeColorFromLocalPoint(Vector2 localPoint)
        {

            if (_xProperty == ColorProperty.Constant)
            {
                // Constant color in x direction
                if (_yProperty == ColorProperty.Constant)
                {
                    // ---- Constant color in x and y direction : ignore ----
                    //NOP
                }
                else
                {
                    //  ---- Color varies in y direction, constant in x direction  (vertical gradient) ----

                    // Compute color
                    _colorPickerGroup.color = ComputeColor(_yProperty, localPoint.y, _flipY, true);
                }
            }
            else
            {
                // Color varies in x direction
                if (_yProperty == ColorProperty.Constant)
                {
                    //  ---- Color varies in x direction, constant in y direction (horizontal gradient)  ----

                    // Compute color
                    _colorPickerGroup.color = ComputeColor(_xProperty, localPoint.x, _flipX, true);
                }
                else
                {
                    //  ---- Color varies in both directions (color picking square)  ----
                    _colorPickerGroup.color = ComputeColor(_xProperty, localPoint.x, _flipX, _yProperty, localPoint.y, _flipY, true);
                }
            }

        }
        #endregion
    }
}

