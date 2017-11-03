using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;
using UnityEngine.EventSystems;


namespace SwissArmyKnife
{
    public enum WhenToShutUp {  WhenSeen, WhenClicked, WhenReleased, WhenToldSo, Never };

	///=================================================================================================================
	///                                                                                                       <summary>
	///  TooltipObject can be attached to any UI element, it tells the TooltipCanvas to display a tooltip
	///  when the cursor is over the element.   
	///  Note that there is no need to populate the event trigger component, this script does it all by itself. </summary>
	///
	///=================================================================================================================
	public class TooltipObject : MonoBehaviour
    {
        #region Declarations and simple properties

        [TextArea(3, 10)]
        public string _tooltip = "";
        public WhenToShutUp _whenShallIShutUp = WhenToShutUp.Never;

#pragma warning disable 414
		private bool _shutUp = false;
        private bool _isTargeted = false;
#pragma warning restore 414

		public TooltipObject[] _siblings; // In case multiple TooltipObjects shall shut up at the same time

        public bool _isScriptDriven = false;
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
        void Awake()
        {
			if ( !_isScriptDriven )
             {
                 // Set up the event trigger
                 EventTrigger trigger = GetComponent<EventTrigger>();
                 if (trigger == null)
                 {
                     trigger = gameObject.AddComponent<EventTrigger>();
                 }


                 // Add "PointerEnter" event
                 EventTrigger.Entry entry = new EventTrigger.Entry();
                 entry.eventID = EventTriggerType.PointerEnter;
                 entry.callback.AddListener((eventData) => { OnMousePointerEnter(); });
                 trigger.triggers.Add(entry);

                 // Add "PointerExit" event
                 entry = new EventTrigger.Entry();
                 entry.eventID = EventTriggerType.PointerExit;
                 entry.callback.AddListener((eventData) => { OnMousePointerExit(); });
                 trigger.triggers.Add(entry);

                 // Depending on _whenShallIShutUp, add "shut up" event. Or don't.
                 EventTriggerType? triggerType = null;
                 switch (_whenShallIShutUp)
                 {
                     case WhenToShutUp.WhenSeen:
                         triggerType = EventTriggerType.PointerExit;
                         break;

                     case WhenToShutUp.WhenClicked:
                         triggerType = EventTriggerType.PointerDown;
                         break;

                     case WhenToShutUp.WhenReleased:
                         triggerType = EventTriggerType.PointerUp;
                         break;

                     case WhenToShutUp.WhenToldSo:
                         // Nothing to do here, another object might hhook to my OnShutUp method.
                         //var toolbarButton = GetComponent<Decocloud.Layout.ToolbarButton>();
                         //toolbarButton._modes[0]._whenActivated.AddListener( delegate { OhShutUp(); });
                         break;
                 }

                 if (triggerType != null)
                 {
                     // Add the event
                     entry = new EventTrigger.Entry();
                     entry.eventID = (EventTriggerType)triggerType;
                     entry.callback.AddListener((eventData) => { OhShutUp(); });
                     trigger.triggers.Add(entry);
                 }
             }
        }


        #endregion
        #region Events
        //=================================================================================================================
        //
        // E V E N T S
        //
        //=================================================================================================================

        ///-------------------------------------------------------                                                 <summary>
        /// Called when pointer enters                                                                           </summary>
        ///-------------------------------------------------------
        public void OnMousePointerEnter()
        {
            if (_shutUp) return;
            
            _isTargeted = true;
            
            TooltipCanvas.Instance.OnCursorEnter(this);
        }

        ///-------------------------------------------------------                                                 <summary>
        /// Called when pointer leaves                                                                           </summary>
        ///-------------------------------------------------------
        public void OnMousePointerExit()
        {
             _isTargeted = false;
             
             if (_shutUp) return;
             TooltipCanvas.Instance.OnCursorExit(this);
        }

        ///-------------------------------------------------------                                                 <summary>
        /// Called to make the tooltip disappear                                                                          </summary>
        ///-------------------------------------------------------
        public void OhShutUp()
        {
           // Are we already shutting up ?
           if (_shutUp) return;

           // Set the "shut up" flag
           _shutUp = true;

           // Shut up siblings. Example: The wind roses which appear on two different screens
           foreach (TooltipObject sibling in _siblings )
           {
               sibling._shutUp = true;
           }

           // Make tooltip disappear
           if (_isTargeted )
           {
               TooltipCanvas.Instance.OnCursorExit(this);
           }

        }


        ///-------------------------------------------------------                                                 <summary>
        /// Called by engine when object is hidden. Added to avoid
        /// the tooltip staying around in this case.                                                            </summary>
        ///-------------------------------------------------------
        void OnDisable()
        {
            if (_isTargeted)
            {
                TooltipCanvas.Instance.OnCursorExit(this);
            }
        }


        #endregion
    }
}

