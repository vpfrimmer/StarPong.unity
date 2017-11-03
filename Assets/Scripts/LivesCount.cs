using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///=================================================================================================================	<summary>
/// Represents a lives display.                                                    					</summary>
///=================================================================================================================
public class LivesCount : MonoBehaviour {

    private Stack<Image> _livesObject = new Stack<Image>();

    private void Awake()
    {
        _livesObject = new Stack<Image>(GetComponentsInChildren<Image>());
    }

    public void RemoveLife()
    {
        Destroy(_livesObject.Peek().gameObject);
        _livesObject.Pop();
    }
}
