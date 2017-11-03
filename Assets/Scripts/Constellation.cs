using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///=================================================================================================================	<summary>
/// Represents a Constellation and handles its stars                                                 					</summary>
///=================================================================================================================
public class Constellation : MonoBehaviour {

    #region variables

    [Tooltip("Display name of the constellation")]
    public string latinName;

    /// <summary> Stars representing the constellation </summary>
    private Star[] _stars;

    #endregion

    private void Awake()
    {
        _stars = GetComponentsInChildren<Star>();
        Hide();
    }

    /// <summary>
    /// Show the constellation by activating its stars
    /// </summary>
    public void Show()
    {
        foreach (var s in _stars)
        {
            s.gameObject.SetActive(true);
            s.Initialize();
        }
    }

    /// <summary>
    /// Hide the constellation by deactivating its stars
    /// </summary>
    public void Hide()
    {
        foreach (var s in _stars)
        {
            s.gameObject.SetActive(false);
        }
    }
}
