using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///=================================================================================================================	<summary>
/// Just a simple button script. Used to call GameManager Instance.                                                     </summary>
///=================================================================================================================
public class StartButton : MonoBehaviour {

	public void OnStartButtonPressed(bool b)
    {
        GameManager.Instance.OnStartButtonPressed(b);
    }
}
