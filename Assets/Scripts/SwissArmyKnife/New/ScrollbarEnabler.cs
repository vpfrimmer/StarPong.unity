using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                       <summary>
	///  ScrollbarEnabler is a little script that enables / disables a scrollbar depending on whether or not it's needed.
    ///  Attach this to the content object.
    ///  Found in http://answers.unity3d.com/questions/891715/how-to-auto-enabledisable-scrollbar-in-new-ui-46.html </summary>
	///
	///=================================================================================================================
	public class ScrollbarEnabler : MonoBehaviour 
	{
         [SerializeField]
         Scrollbar _scrollbar;
         [SerializeField]
         ScrollRect _scrollrect;

         private bool _enableScrollbar = false;
 
         void Update()
         {
             if (_enableScrollbar != _scrollbar.gameObject.activeSelf)
             {
                 _scrollbar.gameObject.SetActive(_enableScrollbar);

                 if (_enableScrollbar )
                 {
                     _scrollrect.velocity = new Vector2( 0.0f, -1000.0f);
                 }
             }

         }
 
         void OnRectTransformDimensionsChange()
         {
             _enableScrollbar = _scrollrect.GetComponent<RectTransform>().rect.height < GetComponent<RectTransform>().rect.height;
         }
     }
}

