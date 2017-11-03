using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///=================================================================================================================	<summary>
/// Represents a gravity well attracting the ball                                                    					</summary>
///=================================================================================================================
public class GravityWell : MonoBehaviour {

    #region variables

    /// <summary> Collider used for attraction </summary>
    private CircleCollider2D _collider;
    /// <summary> How much time has the ball been captured here ? </summary>
    private float _timeCaptured = 0f;

    [Tooltip("Attraction force")]
    public float attraction = 1.0f;

    // Added to attraction depending on ball speed.
    // At max speed, EXTRA_ATTRACTION is added.
    private const float EXTRA_ATTRACTION = 5.0f;
    
    #endregion

    // When star appears..
    private void OnEnable()
    {
        // Let's shut down our colliders right now..
        _collider = GetComponent<CircleCollider2D>();
        _collider.enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);

        // .. and give a bit of time for the ball to go away
        Invoke("EnableColliders", 1.5f);
    }

    /// <summary>
    /// Activate colliders
    /// </summary>
    private void EnableColliders()
    {
        _collider.enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.transform.CompareTag("Ball"))
        {
            Rigidbody2D br = Ball.Instance.rigidbody2d;
            
            float speed = br.velocity.magnitude;

            // In case the ball isn't fast enough to go away on its own, let's give it a small boost
            if (_timeCaptured > 2.5f)
            {
                speed += Ball.Instance.addedSpeed;
                _timeCaptured = 0f;
            }

            float speedFactor = speed / Ball.Instance.maxSpeed;

            // Simple math here, we just add a direct velocity going to the well to the ball
            // But the ball speed remains the same.
            br.velocity = (br.velocity + ((Vector2)transform.position - br.position) * (attraction + (EXTRA_ATTRACTION * speedFactor)) * Time.deltaTime).normalized * speed;
            
            _timeCaptured += Time.deltaTime;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.CompareTag("Ball"))
        {
            // Just reinit timer on ball exit.
            _timeCaptured = 0f;
        }
    }
}
