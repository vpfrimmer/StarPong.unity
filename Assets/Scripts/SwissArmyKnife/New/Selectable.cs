using UnityEngine;
using System.Collections;

///=================================================================================================================
///                                                                                                       <summary>
///  Selector and Selectable are classes to implement selection of objects
///  (items, tools, camera modes etc.), supervised by a Selector.
///  Example: PenguinSelector : Selector<PenguinSelector,Penguin> and Penguin : Selectable<PenguinSelector,Penguin>
///                                                                                                         </summary>
///=================================================================================================================
namespace SwissArmyKnife
{

	public class Selectable<M,S> : MonoBehaviour
        where M : Selector<M, S>
        where S : Selectable<M,S>
	{
		#region Declarations and simple properties

        /// <summary> The suffic in the inspector if selected </summary>
        private const string SELECTED_SUFFIX = " [SELECTED]";

        /// <summary> Says if the item is selected. </summary>
		private bool _isSelected;

        /// <summary> Read-Only property for _isSelected. </summary>
		public bool IsSelected 
		{ 
			get 
			{
                return _isSelected; 
			} 

            set 
			{
                if (_isSelected == value) return;

                _isSelected = value; 
                if( _isSelected )
                {
                    if (!name.EndsWith(SELECTED_SUFFIX))
                    {
                        name = name + SELECTED_SUFFIX;
                    }
                }
                else
                {
                    if (name.EndsWith(SELECTED_SUFFIX))
                    {
                        name = name.Substring(0, name.Length - SELECTED_SUFFIX.Length);
                    }
                }
			} 
		}

		#endregion


		#region Selection
		//=================================================================================================================
		//
		// S E L E C T I O N
		//
		//=================================================================================================================


		///-------------------------------------------------------
        /// Select()
		/// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
		/// Called by UI buttons etc. to select the item                                                            </summary>
		///-------------------------------------------------------
        public void Select() 
		{
            if (!IsSelected)
            {
                Singleton<M>.Instance.OnSelectedExternally( this as S );
                IsSelected = true;

                if (Singleton<M>.Instance.ShouldSendMessage())
                {
                    Singleton<M>.Instance._recursionCounter++;
                    OnSelected();
                    Singleton<M>.Instance._recursionCounter--;
                }


            }
		}

        ///-------------------------------------------------------
        /// Unselect()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Called by UI buttons etc. to unselect the tool                                                          </summary>
        ///-------------------------------------------------------
        public void Unselect()
        {

            if (IsSelected)
            {
                Singleton<M>.Instance.OnUnselectedExternally(this as S);
                IsSelected = false;

                if (Singleton<M>.Instance.ShouldSendMessage())
                {
                    Singleton<M>.Instance._recursionCounter++;
                    OnUnselected();
                    Singleton<M>.Instance._recursionCounter--;
                }



            }
        }

        #endregion
        #region Selection by Selector
        //=================================================================================================================
        //
        // S E L E C T   B Y    M A N A G E R
        //
        //=================================================================================================================

        ///-------------------------------------------------------
        /// SelectBySelector()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Called by tool Selector on default tool if another 
        /// tool is manually unselected.                                                   </summary>
        ///-------------------------------------------------------
        public void SelectBySelector()
        {
            if (!IsSelected)
            {
                IsSelected = true;
                 if (Singleton<M>.Instance.ShouldSendMessage())
                 {
                     Singleton<M>.Instance._recursionCounter++;
                     OnSelected();
                     Singleton<M>.Instance._recursionCounter--;
                 }
            }
           
        }

        ///-------------------------------------------------------
        /// SelectBySelector()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Called by tool Selector if another tool is selected.                                                     </summary>
        ///-------------------------------------------------------
        public void UnselectBySelector()
        {
            if (IsSelected)
            {
                IsSelected = false;
                if (Singleton<M>.Instance.ShouldSendMessage())
                {
                    Singleton<M>.Instance._recursionCounter++;
                    OnUnselected();
                    Singleton<M>.Instance._recursionCounter--;
                }
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
        /// OnSelected()
		/// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
		/// Called by Selectable script when selected.
        /// Use this to activate tool-specific stuff.                                                             </summary>
		///-------------------------------------------------------
        protected virtual void OnSelected()
        {}

        ///-------------------------------------------------------
        /// OnUnselected()
        /// - - - - - - - - - - - - - - - - - - - - - - - - - - -                                                   <summary>
        /// Called by Selectable script when unselected. 
        /// Use this to deactivate tool-specific stuff.                                                               </summary>
        ///-------------------------------------------------------
        protected virtual void OnUnselected()
        {}

		#endregion
	}

}

