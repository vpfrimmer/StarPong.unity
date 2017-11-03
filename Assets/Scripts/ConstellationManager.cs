using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwissArmyKnife;

///=================================================================================================================	<summary>
/// Handle constellation display and states                                                          					</summary>
///=================================================================================================================
public class ConstellationManager : Singleton<ConstellationManager> {

    #region variables

    [Tooltip("Link between two stars prefab")]
    public GameObject linkPrefab;
    [Tooltip("Every playable constellations")]
    public Constellation[] constellations;

    /// <summary> Currently shown constellation </summary>
    private Constellation _shownConstellation = null;

    #endregion

    private void Awake()
    {
        foreach(Constellation c in constellations)
        {
            c.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Activate a random playable constellation
    /// </summary>
    public void ShowRandomConstellation()
    {
        // Hide last constellation in case there's any
        if(_shownConstellation != null)
        {
            _shownConstellation.Hide();
        }

        // Show a new constellation
        _shownConstellation = constellations[Random.Range(0, constellations.Length)];
        _shownConstellation.Show();

        // Display its name for education purposes
        GameCanvas.Instance.ShowTitle(_shownConstellation.latinName);
    }
}
