using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///=================================================================================================================
///                                                                                                       <summary>
///  Selector and Selectable are classes to implement selection of objects
///  (items, tools, camera modes etc.), supervised by a Selector.
///  Example: PenguinSelector : Selector<PenguinSelector,Penguin> and Penguin : Selectable<PenguinSelector,Penguin>.
///  See header of Selector.cs for an example.
///  </summary>
///
/// Example for implementation :
/// 
/// public class PenguinSelector : Selector<PenguinSelector,Penguin> 
///	{
///	   public GameObject _selectiuonMarker;
///	    
///     void Start()
///     {
///        _selectiuonMarker.SetActive( false );
///        
///        // IMPORTANT:
///        base.Start(); 
///     }
///    
///     protected override void OnItemSelected( Penguin penguin )
///     { 
///         _selectiuonMarker.transform.position = penguin.transform.position;
///         _selectiuonMarker.SetActive( true );
///     }
///     
///    protected override void OnItemUnselected( Penguin penguin )
///     { 
///         _selectiuonMarker.SetActive( false );
///     }
///	}
/// 
/// public class Penguin : Selectable<PenguinSelector,Penguin> 
///	{
///    // Nothing to implement here 
///	}
/// 
/// public class PenguinFlappy : Penguin 
///	{
///    public override void OnSelected()
///    {
///        // Implement flapping wings here    
///    }
///	}
/// 
/// /// public class PenguinJumper : Penguin 
///	{
///    public override void OnSelected()
///    {
///        // Implement jumping up and down here    
///    }
///	}
///=================================================================================================================
namespace SwissArmyKnife
{
    public class Selector<M, S> : Singleton<M> 
        where M : Selector<M, S>  // e.g. PenguinSelector
        where S : Selectable<M,S> // e.g. Penguin
	{
        #region Declarations and simple properties
        [SerializeField]
        [Tooltip("To set the item selected by default.")]
        private S _selectedByDefault;

        /// <summary>
        /// The currently selected item
        /// </summary>
        private S _selectedItem;
        public S Selected { get { return _selectedItem; } }

        /// <summary>  Set to true if you don't want to call virtual methods like OnSelected </summary>
        public bool IgnoreMessages { get; set; }

        public int _recursionCounter = 0;

        public struct Subscriber
        {
            public GameObject _receiver;
            public string _message;
            public Subscriber( GameObject receiver, string message )
            {
                _receiver = receiver;
                _message = message;
            }
        }
        private List<Subscriber> _subscribers = new List<Subscriber>();



        #endregion
        #region Initialization
        //=================================================================================================================
        //
        // I N I T I A L I Z A T I O N 
        //
        //=================================================================================================================

        ///-------------------------------------------------------
        /// Start()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Use this for initialization                                                                             </summary>
        ///-------------------------------------------------------
        protected void Start()
        {
            // Select default tool if any
            if (_selectedByDefault != null)
            {
                SelectedItem = _selectedByDefault;
                _selectedByDefault.SelectBySelector();
            }
        }

        #endregion
        #region Default selected
        //=================================================================================================================
        //
        // S E L E C T E D   B Y    D E F A U L T 
        //
        //=================================================================================================================

        ///-------------------------------------------------------
        /// ChangeDefaultSelectedTo()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// To access the item selected by default.  
        /// The set part changes selects the new default if the old default was selected.                           </summary>
        ///-------------------------------------------------------
        public S SelectedByDefault
        {
            get
            {
                return _selectedByDefault;
            }

            set
            {
                S oldValue = _selectedByDefault;

                _selectedByDefault = value;

                if (_selectedItem == oldValue)
                {
                    if( value != null )
                    {
                        value.Select();
                    }
                    else if (_selectedItem != null )
                    {
                        _selectedItem.Unselect();
                    }
                    SelectedItem = value;
                }

            }
        }



        #endregion
        #region Selection Management
        //=================================================================================================================
        //
        // S E L E C T I O N    M A N A G E M E N T
        //
        //=================================================================================================================

        ///-------------------------------------------------------                                           <summary>
        /// Private property, informs automatically subscribers                     </summary>
        ///-------------------------------------------------------
        private S SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                if( _selectedItem != value )
                {
                    _selectedItem = value;

                    if (_recursionCounter < 100)
                    {
                        _recursionCounter++;
                        InformSubscribersAbout(_selectedItem);
                        _recursionCounter--;
                    }

                }
            }
        }


        ///-------------------------------------------------------
        /// OnSelectedExternally()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Sent by Selectable script when the item is selected by UI or third party script                      </summary>
        ///-------------------------------------------------------
        public void OnSelectedExternally(S item)
        {
            // Unselect currently selected tool
            if (_selectedItem != null && _selectedItem != item)
            {
                _selectedItem.UnselectBySelector();
            }

            SelectedItem = item;

            if (ShouldSendMessage())
            {
                _recursionCounter++;
                OnItemSelected(item);
                _recursionCounter--;
            }


        }

        ///-------------------------------------------------------
        /// OnUnselectedExternally()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Sent by Selector script when the item is unselected by UI or third party script                     </summary>
        ///-------------------------------------------------------
        public void OnUnselectedExternally(S item)
        {
            OnItemUnselected(item);
            if (_selectedItem == item )
            {
                _selectedItem = _selectedByDefault;
                if( _selectedByDefault != null )
                {
                    _selectedByDefault.SelectBySelector();

                    if (ShouldSendMessage())
                    {
                        _recursionCounter++;
                        OnItemSelected(_selectedByDefault);
                        _recursionCounter--;
                    }
                }
            }
        }

        ///-------------------------------------------------------
        /// UnselectCurrent()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Called by whoever wants to unselect selected item (if any)                              </summary>
        ///-------------------------------------------------------
        public void UnselectCurrent()
        {
            if (_selectedItem != null && _selectedItem != _selectedByDefault )
            {
                _selectedItem.Unselect();
            }
        }


        #endregion
        #region Virtual selection methods
        //=================================================================================================================
        //
        // V I R T U A L  S E L E C T I O N    M E T H O D S 
        //
        //=================================================================================================================

        ///-------------------------------------------------------
        /// OnItemSelected()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Called when item selected.
        /// Use this to activate tool-specific stuff.                                                             </summary>
        ///-------------------------------------------------------
        protected virtual void OnItemSelected( S item )
        { }

        ///-------------------------------------------------------
        /// OnItemUnselected()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Called when item unselected. 
        /// Use this to deactivate tool-specific stuff.                                                               </summary>
        ///-------------------------------------------------------
        protected virtual void OnItemUnselected( S item )
        { }

        #endregion

        #region Virtual selection methods
        //=================================================================================================================
        //
        // V I R T U A L  S E L E C T I O N    M E T H O D S 
        //
        //=================================================================================================================

        ///-------------------------------------------------------                                               <summary>                                             
        /// Called by whoever wants to receive a message when an item is selected                                </summary>
        ///-------------------------------------------------------
        public void Subscribe(GameObject receiver, string message)
        {
            _subscribers.Add(new Subscriber(receiver, message));
        }

        ///-------------------------------------------------------                                               <summary>                                             
        /// Called when item (possibly null) is selected.                                                            </summary>
        ///-------------------------------------------------------
        public void InformSubscribersAbout( S newlySelectedItem )
        {
            for( int i = 0; i < _subscribers.Count; i++ )
            {
                Subscriber subscriber = _subscribers[i];
                if( subscriber._receiver == null )
                {
                    // Must have been deleted
                    _subscribers.RemoveAt(i--);
                }
                else
                {
                    // Send message
                    subscriber._receiver.SendMessage(subscriber._message, newlySelectedItem);
                }
            }
        }

        #endregion
        #region Infinite recursion prevention
        //=================================================================================================================
        //
        // I N F I N I T E   R E C U R S I O N    P R E V E N T I O N
        //
        //=================================================================================================================

        ///-------------------------------------------------------                                                  <summary>
        /// Says if we should send a message like OnSelected, OnItemSelected etc.                      </summary>
        ///-------------------------------------------------------
        public bool ShouldSendMessage()
        {
            if (IgnoreMessages)
            {
                Debug.Log("Ignore messages == true");
                return false;
            }

            if( _recursionCounter > 100 )
            {
                Debug.LogWarning("Recursion counter = " + _recursionCounter + "! (IgnoreMessages = " + IgnoreMessages + ")");
                return false;
            }

            return true;

        }
        #endregion
    }
}

