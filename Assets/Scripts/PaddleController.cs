using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///=================================================================================================================	<summary>
/// Represents a player and handle controls                                                          					</summary>
///=================================================================================================================
public class PaddleController : MonoBehaviour {

    #region variables
    [Tooltip("Player paddle object.")]
    public Transform paddle;
    [Tooltip("The player id.")]
    public int player;
    [Tooltip("The associated life counter")]
    public LivesCount lifeCount;

    /// <summary> Lives remaining </summary>
    public int remainingLives = 5;
    
    /// <summary> Is this paddle an IA ? </summary>
    [SerializeField]
    private bool _isAI = false;
    public bool IsAI
    {
        set
        {
            _isAI = value;
            if (!value)
            {
                StopAllCoroutines();
            }
            else
            {
                StartCoroutine(AI_Routine());
            }
        }
    }

    // Reaction of the AI, may be tweaked to change difficulty
    private const float AI_SPEED = 8.5f;

    #endregion

    private void Awake()
    {
        // If we're in solo mode
        if(GameManager.Instance != null)
        {
            IsAI = _isAI || (GameManager.Instance.needsAi && player == 2);
        }
    }
    
    void Update () {

        if (paddle == null)
        {
            return;
        }

        // Handle controls
        if(!_isAI)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (Input.GetMouseButton(0))
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 touchPos = new Vector2(wp.x, wp.y);
                if (GetComponent<Collider2D>() == Physics2D.OverlapPoint(touchPos))
                {
                    Vector2 padNewPos = paddle.position;
                    padNewPos = new Vector2(padNewPos.x, wp.y);
                    paddle.position = padNewPos;
                }
            }
#endif

#if UNITY_ANDROID || UNITY_EDITOR
            foreach (Touch t in Input.touches)
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(t.position);
                Vector2 touchPos = new Vector2(wp.x, wp.y);
                if (GetComponent<Collider2D>() == Physics2D.OverlapPoint(touchPos))
                {
                    Vector2 padNewPos = paddle.position;
                    padNewPos = new Vector2(padNewPos.x, wp.y);
                    paddle.position = padNewPos;
                }
            }
#endif
        }
    }

    /// <summary>
    /// Called when opponent scores
    /// </summary>
    public void OnLoseGoal()
    {
        lifeCount.RemoveLife();

        // We lose
        if (--remainingLives == 0)
        {
            GameManager.Instance.StopGame(this);
        }
        // Life goes on
        else
        {
            GameManager.Instance.InitGame();
        }
    }

    /// <summary>
    /// Simple AI routine.
    /// </summary>
    private IEnumerator AI_Routine()
    {
        while (true)
        {
            Vector2 desiredPos = paddle.transform.position;
            desiredPos.y = Ball.Instance.transform.position.y;

            paddle.transform.position = Vector2.Lerp(paddle.transform.position, desiredPos, Time.deltaTime * AI_SPEED);

            yield return null;
        }
    }
}