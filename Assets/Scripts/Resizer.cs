using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///=================================================================================================================	<summary>
/// Should be put in an object in need of being scaled                                                					</summary>
///=================================================================================================================
public class Resizer : MonoBehaviour {

    [Tooltip("Should we base scaling on width only ?")]
    public bool widthOnly = true;

    private void Awake()
    {
        CameraManager.Instance.Subscribe(this);
    }

    /// <summary>
    /// Callback for the resize event
    /// </summary>
    private void OnResizeCallback(Vector2 scaleFactor)
    {
        transform.localScale = new Vector3(scaleFactor.x, widthOnly ? scaleFactor.x : scaleFactor.y, 1);
    }
}
