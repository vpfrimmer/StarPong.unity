using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                       <summary>
    ///  ColorPickerGroup is a hub script for a set of color picker textures, color boxes etc.					 </summary>
	///
	///=================================================================================================================
	public class ColorPickerGroup : MonoBehaviour 
	{
        [SerializeField]
        [Tooltip("The current color")]
        private Color _color;

        public Color color 
        { 
            get
            {
                return _color;
            }
            set 
            {

                _color = value;
                if (_subscribers != null )
                {
                    foreach (GameObject subscriber in _subscribers)
                    {
                        subscriber.SendMessage("OnColorChanged", _color, SendMessageOptions.DontRequireReceiver);
                    }
                }

            }
        }

        /// <summary>
        /// The objects that want to be informed when the color changes
        /// </summary>
        private List<GameObject> _subscribers;


        ///-------------------------------------------------------                                                  <summary>
        /// Called by subscribers, usually in their Start() method                                                    </summary>
        ///-------------------------------------------------------
        public void CallMeIfColorChanges( GameObject subscriber )
        {
            if( _subscribers == null )
            {
                _subscribers = new List<GameObject>();
            }
            _subscribers.Add(subscriber);
        }


	}
}

