using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                       <summary>
	///  ColorHSV is a sibling of the Unity struct Color - only it's a HSV color instead of a RGB color.	  	 </summary>
	///
	///=================================================================================================================
    public struct ColorHSV 
	{
		#region Simple properties

        /// <summary> The hue. </summary>
        public float h { get; set; }

        /// <summary> The saturation. </summary>
        public float s { get; set; }

        /// <summary> The value. </summary>
        public float v { get; set; }

        /// <summary> The alpha. </summary>
        public float a { get; set; }

		#endregion
		#region Constructors
		//=================================================================================================================
		//
		// C O N S T R U C T O R S
		//
		//=================================================================================================================

        public ColorHSV(float h, float s, float v)
        {
            this.h = h;
            this.s = s;
            this.v = v;
            this.a = 1.0f;
        }

        public ColorHSV(float h, float s, float v, float a)
        {
            this.h = h;
            this.s = s;
            this.v = v;
            this.a = a;
        }

        public ColorHSV(Color color)
        {
            h = s = v= a = 0.0f;
            RGB_to_HSV(color);
        }

        #endregion
		#region Assignment and cast
		//=================================================================================================================
		//
		// A S S I G N M E N T   &   C A S T
		//
		//=================================================================================================================

		///-------------------------------------------------------                                                 <summary>
		/// RGB to HSV                                                                          </summary>
		///-------------------------------------------------------
        public static explicit operator ColorHSV(Color color)
        {
            return new ColorHSV(color);
        }

        ///-------------------------------------------------------                                                 <summary>
        /// HSV to RGB                                                                          </summary>
        ///-------------------------------------------------------
        public static explicit operator Color(ColorHSV colorHSV)
        {
            return colorHSV.HSV_to_RGB();
        }

        ///-------------------------------------------------------                                                 <summary>
        /// HSV to string                                                                          </summary>
        ///-------------------------------------------------------
        public static explicit operator string(ColorHSV colorHSV)
        {
            return "HSVA(" + colorHSV.h + ", " + colorHSV.s + ", " + colorHSV.v + ", " + colorHSV.a+")";
        }


        #endregion
        #region Equality
        //=================================================================================================================
        //
        // E Q U A L I T Y 
        //
        //=================================================================================================================

        ///-------------------------------------------------------                                                 <summary>
        /// Equals                                                                        </summary>
        ///-------------------------------------------------------
        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to ColorHSV return false.
            ColorHSV colorHSV = (ColorHSV)obj;
            if ((System.Object)colorHSV == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.h == colorHSV.h 
                && this.s == colorHSV.s 
                && this.v == colorHSV.v 
                && this.a == colorHSV.a );
        }

        ///-------------------------------------------------------                                                 <summary>
        /// ==                                                                        </summary>
        ///-------------------------------------------------------
        public static bool operator ==(ColorHSV color1, ColorHSV color2)
        {
            return (color1.h == color2.h
                && color1.s == color2.s
                && color1.v == color2.v
                && color1.a == color2.a);
        }

        ///-------------------------------------------------------                                                 <summary>
        /// !=                                                                        </summary>
        ///-------------------------------------------------------
        public static bool operator !=(ColorHSV color1, ColorHSV color2)
        {
            return !(color1 == color2);
        }

        ///-------------------------------------------------------                                                 <summary>
        /// Hash code                                                                        </summary>
        ///-------------------------------------------------------
        public override int GetHashCode()
        {
            return h.GetHashCode() + 11 * s.GetHashCode() + 17 * v.GetHashCode() + 23 * a.GetHashCode();
        }

		#endregion
		#region RGB-HSV Conversion
		//=================================================================================================================
		//
		// R G B - H S V   C O N V E R S I O N
		//
		//=================================================================================================================

		///-------------------------------------------------------                                                 <summary>
		/// Conversion from RGB to HSV                                                                            </summary>
		///-------------------------------------------------------
		void RGB_to_HSV( Color colorRGB ) 
		{
            // Get r, g and b
            float r = colorRGB.r;
            float g = colorRGB.g;
            float b = colorRGB.b;

            // Compute auxiliary values
            float min, max, delta;
            min = Mathf.Min(r, g, b);
            max = Mathf.Max(r, g, b);
            delta = max - min;

            // Value is max
            this.v = max;

            // Copy alpha
            this.a = colorRGB.a;

            // Is everythng grey or black ?
            if ( delta < 0.00001f)
            {
                this.s = 0.0f;
                this.h = 0.0f; // Actually, undefined
                return;
            }

            // Saturation is delta / max
            this.s = delta / max;

            // Compute hue
            if( max == r )
            {
                // Red is dominant - between yellow and magenta
                this.h = 0.166666f * (g - b) / delta;
            }
            else if (max == g)
            {
                // Green is dominant - between cyan and yellow
                this.h = 0.33333f + 0.166666f * (b - r) / delta;
            }
            else
            {
                // Blue is dominant - between magenta and cyan
                this.h = 0.66666f + 0.166666f * (r - g) / delta;
            }

            // Hue mod 1
            while (this.h < 0)
            {
                this.h += 1.0f;
            }

            // We are done here.
		}

        ///-------------------------------------------------------                                                 <summary>
		/// Conversion from RGB to HSV      
        /// Cf. http://www.cs.rit.edu/~ncs/color/t_convert.html                                                     </summary>
		///-------------------------------------------------------
        Color HSV_to_RGB()
        {
            float r = 0.0f;
            float g = 0.0f;
            float b = 0.0f;

            // Is everythng grey or black ?
            if( s == 0.0f )
            {
                return new Color( v, v, v, this.a);
            }

            // Auxiliary values
            float sector = h * 6.0f;
            int sectorIndex = Mathf.FloorToInt(sector);
            float f = sector - sectorIndex;
            float p = v * (1.0f - s);
            float q = v * (1.0f - s * f);
            float t = v * (1.0f - s * (1.0f - f));

            // Formula depends on sector index
            switch( sectorIndex )
            {
                // Red to yellow
                case 0:     r = v; g = t; b = p; break;

                // Yellow to green
                case 1:     r = q; g = v; b = p; break;

                // Green to cyan
                case 2:     r = p; g = v; b = t; break;

                // Cyan to blue
                case 3:     r = p; g = q; b = v; break;

                // Blue to magenta
                case 4:     r = t; g = p; b = v; break;

                // Magenta to red
                // case 5
                default:    r = v; g = p; b = q; break;
            }


            // We are done here.
            return new Color(r, g, b, this.a);
        }


		#endregion
	}
}

