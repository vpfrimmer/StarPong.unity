using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                       <summary>
	///  CursorOnMouseover allows to specify a cursor to appear on mouseover of an UI element 			 </summary>
	///
	///=================================================================================================================
	public class CursorOnMouseover : MonoBehaviour 
	{
        public string _cursorID;

        //TODO: Better cursor handling with stack

        void Awake()
        {
            // Get EventTrigger component, add one if there is none
            EventTrigger eventTrigger =  GetComponentInParent<EventTrigger>();
            if( eventTrigger == null )
            {
                eventTrigger = gameObject.AddComponent<EventTrigger>();
            }

            // Add PointerEnter event
            EventTrigger.Entry entryPointerEnter = new EventTrigger.Entry();
            entryPointerEnter.eventID = EventTriggerType.PointerEnter;
            entryPointerEnter.callback.AddListener( delegate { OnCursorEnter(); } );
            eventTrigger.triggers.Add(entryPointerEnter);

            // Add PointerExit event
            EventTrigger.Entry entryPointerExit = new EventTrigger.Entry();
            entryPointerExit.eventID = EventTriggerType.PointerExit;
            entryPointerExit.callback.AddListener( delegate { OnCursorExit(); } );
            eventTrigger.triggers.Add(entryPointerExit);

        }

        void OnCursorEnter()
        {
            SmartCursor.Instance.SetCursor(_cursorID, SmartCursor.Priority.Mouseover);
        }

        void OnCursorExit()
        {
            SmartCursor.Instance.ClearCursor(SmartCursor.Priority.Mouseover);
        }
	}
}

