using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///=================================================================================================================	<summary>
/// Represents a player goal 					</summary>
///=================================================================================================================
public class Goal : MonoBehaviour {

    [Tooltip("Player owning this goal")]
    public PaddleController owner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Ball"))
        {
            owner.OnLoseGoal();
        }
    }
}
