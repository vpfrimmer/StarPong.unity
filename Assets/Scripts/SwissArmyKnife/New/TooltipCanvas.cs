using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;
using UnityEngine.EventSystems;
using TMPro;

namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                       <summary>
	///  TooltipCanvas is a canvas to display tooltips          										 </summary>
	///
	///=================================================================================================================
    public class TooltipCanvas : Singleton<TooltipCanvas> 
	{
		public GameObject _tooltipBubble;
        public TMP_Text _tooltipText;

        public TooltipObject CurrentObject { get; set; }
#pragma warning disable 414
		private List<GameObject> _subscribers = new List<GameObject>();
#pragma warning restore 414

		public void Subscribe( GameObject subscriber )
        {
            _subscribers.Add(subscriber);
        }

        public void Unsubscribe(GameObject subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        private void Awake()
        {
			_tooltipBubble.SetActive(false);
            CurrentObject = null;
        }

        public void OnCursorEnter( TooltipObject ttobject )
        {
            _tooltipBubble.SetActive(true);
            _tooltipText.text = ttobject._tooltip;
            CurrentObject = ttobject;


            float relativeMouseX = Input.mousePosition.x / Screen.width;
            if( Input.mousePosition.y < Screen.height /2 )
            {
                // Lower screen half: Tooltip above cursor 
                // (The pivot will be placed on the cursor position, it must be below bubble)
                _tooltipBubble.GetComponent<RectTransform>().pivot = new Vector2(relativeMouseX, -0.2f);
            }
            else
            {
                // Upper screen half: Tooltip below cursor
                // (The pivot will be placed on the cursor position, it must be above bubble)
                _tooltipBubble.GetComponent<RectTransform>().pivot = new Vector2(relativeMouseX, 1.8f);
            }

            // Inform subscribers
            foreach( GameObject subscriber in _subscribers )
            {
                subscriber.SendMessage("OnTooltipDisplayed");
            }
        }

        public void OnCursorExit( TooltipObject ttobject )
        {
            if( ttobject == CurrentObject )
            {
                CurrentObject = null;
               _tooltipBubble.SetActive(false);

               // Inform subscribers
               foreach (GameObject subscriber in _subscribers)
               {
                   subscriber.SendMessage("OnTooltipHidden");
               }
            }
        }

        public void LateUpdate()
        {
            if( _tooltipBubble.activeSelf )
            {
                _tooltipBubble.transform.position = Input.mousePosition;
            }
        }

	}
}

