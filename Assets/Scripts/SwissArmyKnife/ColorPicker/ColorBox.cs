using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;


namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                       <summary>
	///  ColorBox is a white Image that is colored to display a PickableColor 								 </summary>
	///
	///=================================================================================================================
    [RequireComponent(typeof(Image))]
	public class ColorBox : MonoBehaviour 
	{

        [SerializeField]
        [Tooltip("The color picker group script")]
        private ColorPickerGroup _colorPickerGroup;

		///-------------------------------------------------------                                                  <summary>
		/// Use this for initialization                                                                             </summary>
		///-------------------------------------------------------
		void Start () 
		{
            GetComponent<Image>().color = _colorPickerGroup.color;

            // We want to be informed when the color changes.
            _colorPickerGroup.CallMeIfColorChanges(gameObject);
		}
        ///-------------------------------------------------------                                                  <summary>
        /// Called when someone changed the color                                                                        </summary>
        ///-------------------------------------------------------
        public void OnColorChanged( Color color )
        {
            if( enabled )
            {
                GetComponent<Image>().color = color;
            }
        }

        ///-------------------------------------------------------                                                  <summary>
        /// Called to set the initial color (only needed in some cases, for example
        /// when managing multiple color boxes corresponding to "color slots").                                   </summary>
        ///-------------------------------------------------------
        public void InitColor(Color color)
        {
           GetComponent<Image>().color = color;
        }



    }
}

