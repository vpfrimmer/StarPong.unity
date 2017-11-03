using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwissArmyKnife;

///=================================================================================================================	<summary>
/// Represents the ball and handles its collision and general behaviour                                					</summary>
///=================================================================================================================
public class Ball : Singleton<Ball> {

    [Tooltip("The added speed each time the ball hit a paddle.")]
    public float addedSpeed = 0.1f;
    [Tooltip("The initial ball speed;")]
    public float initSpeed = 8f;
    [Tooltip("The maximum ball speed;")]
    public float maxSpeed = 12f;

    /// <summary> Ball kinematic rigidbody </summary>
    [HideInInspector]
    public Rigidbody2D rigidbody2d;

    /// <summary> The ball trail renderer </summary>
    private TrailRenderer _tr;
    /// <summary> Original ball position </summary>
    private Vector2 _oriPos;
    /// <summary> Last player hitting the ball. -1 if none. </summary>
    private int _player = -1;
    public int Player { get { return _player; } }

    public override void AwakeSingleton()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();

        _oriPos = transform.position;
        _tr = GetComponent<TrailRenderer>();
    }

    /// <summary>
    /// Set the ball at start and launch it with a fixed velocity on the horizontal axis
    /// </summary>
    public void Launch()
    {
        _tr.enabled = true;
        transform.position = _oriPos;
        rigidbody2d.velocity = new Vector2(((Random.value > 0.5f) ? 1f : -1f) * initSpeed, 0);
    }

    /// <summary>
    /// Set the ball at its starting position and remove any velocity
    /// </summary>
    public void InitBall()
    {
        _tr.enabled = false;
        rigidbody2d.velocity = Vector2.zero;
        transform.position = _oriPos;
        _player = -1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case "Paddle":
                BounceOnPaddle(collision);
                break;

            case "Border":
                BounceOn(collision.contacts);
                break;

            case "Goal":
            default:
                // Nothing, go right through it
                break;
        }
    }

    /// <summary>
    /// Handle paddle collision
    /// </summary>
    private void BounceOnPaddle(Collision2D collision)
    {
        // Play a small pop sound
        GameManager.Instance.PlaySFX("SFX_Pop_Paddle");

        // Just throw the ball in the paddle center opposite direction to allow basic control over the ball trajectory
        Vector3 paddleCenter = collision.transform.position;
        rigidbody2d.velocity = -(paddleCenter - transform.position).normalized * Mathf.Min(maxSpeed, (rigidbody2d.velocity.magnitude + addedSpeed));

        // Keep the last touching player id
        Paddle paddle = collision.collider.GetComponent<Paddle>();
        _player = paddle.controller.player;
    }

    /// <summary>
    /// Reflect the ball depending on contact points
    /// </summary>
    /// <param name="contacts">Collision contact points</param>
    private void BounceOn(ContactPoint2D[] contacts)
    {
        Vector2 normalsSum = Vector2.zero;
        foreach(var c in contacts)
        {
            normalsSum += c.normal;
        }

        rigidbody2d.velocity = Vector2.Reflect(rigidbody2d.velocity, normalsSum);
    } 
}
