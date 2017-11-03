using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                       <summary>
	///  SmartCursor allows complex cursor handling                     									 </summary>
	///
	///=================================================================================================================
    public class SmartCursor : Singleton<SmartCursor> 
	{
		#region Declarations and simple properties

        // Due to a bug in Unity, we disable the custom cursors for now.
        // Cf. https://fogbugz.unity3d.com/default.asp?744374_ds5e9il7im29i558
        private bool DISABLED_UNTIL_UNITY_FIXES_CURSOR_BUG = true;

        public enum Priority { Low=0, High=1, Mouseover=2 };

        [System.Serializable]
        private struct CursorInfo
        {
            public string ID;
            public Texture2D _bitmap;
            public Vector2 _hotspot;
        }

        private CursorInfo[] _cursorInfosByPriority;

        [SerializeField]
        private CursorInfo[] _predefinedCursors;

        private CursorMode _actualCursorMode;

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
            _cursorInfosByPriority = new CursorInfo[System.Enum.GetNames(typeof(Priority)).Length];

            // Set cursor mode depending on platform
#if UNITY_EDITOR
            _actualCursorMode = CursorMode.Auto;
#elif UNITY_STANDALONE
            _actualCursorMode = CursorMode.Auto;
#elif UNITY_WEBGL
            _actualCursorMode = CursorMode.ForceSoftware;
#else
            _actualCursorMode = CursorMode.Auto;
#endif
		}


		#endregion
		#region Changing the cursor
		//=================================================================================================================
		//
		// C H A N G I N G   T H E   C U R S O R
		//
		//=================================================================================================================

		///-------------------------------------------------------                                                 <summary>
		/// To set the cursor of a certain priority                                                                  </summary>
		///-------------------------------------------------------
		public void SetCursor( string id, Priority priority = Priority.Low) 
		{
            if (DISABLED_UNTIL_UNITY_FIXES_CURSOR_BUG )
            {
                return;
            }

            if( id == "" || id == null )
            {
                ClearCursor(priority);
                return;
            }

            foreach( CursorInfo info in _predefinedCursors )
            {
                if( info.ID == id)
                {
                    SetCustomCursor(info._bitmap, info._hotspot, priority);
                    return;
                }
            }


		}

        ///-------------------------------------------------------                                                 <summary>
        /// To set the cursor of a certain priority                                                                  </summary>
        ///-------------------------------------------------------
        private void SetCustomCursor(Texture2D bitmap, Vector2 hotspot, Priority priority = Priority.Low)
        {
            // Update the array
            _cursorInfosByPriority[(int)priority]._bitmap = bitmap;
            _cursorInfosByPriority[(int)priority]._hotspot = hotspot;

            // Look the highest priority with non-null cursor
            int highestPriority = System.Enum.GetNames(typeof(Priority)).Length - 1;
            while (highestPriority > 0 && _cursorInfosByPriority[highestPriority]._bitmap == null)
            {
                // This cursor is null, check the next lower one
                highestPriority--;
            }

            // Apply
            Cursor.SetCursor(_cursorInfosByPriority[(int)highestPriority]._bitmap, _cursorInfosByPriority[(int)highestPriority]._hotspot, _actualCursorMode);


        }

        ///-------------------------------------------------------                                                 <summary>
        /// To clear the cursor of a certain priority                                                                  </summary>
        ///-------------------------------------------------------
        public void ClearCursor(Priority priority)
        {
            if (DISABLED_UNTIL_UNITY_FIXES_CURSOR_BUG)
            {
                return;
            }

            SetCustomCursor(null, Vector2.zero, priority);
        }


        private string DebugString()
        {
            string output = "";
            for( int i = 0; i < 3; i++ )
            {
                if( _cursorInfosByPriority[i]._bitmap == null )
                {
                    output += "[  ...   ]";
                }
                else
                {
                    output += "["+_cursorInfosByPriority[i]._bitmap.name +"]";
                }
            }
            return output;
        }

		#endregion
	}
}

