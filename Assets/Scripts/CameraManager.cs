using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwissArmyKnife;

///=================================================================================================================	<summary>
/// Handles game camera behaviour, scaling and background stars generation                                      		</summary>
///=================================================================================================================
public class CameraManager : Singleton<CameraManager> {

    [Tooltip("Root and background sprite. Used to scale everything.")]
    public SpriteRenderer backgroundSprite;
    [Tooltip("How many background stars should be spawned.")]
    public int starCount = 100;

    /// <summary> Computed screen height in game units </summary>
    private float _worldScreenHeight;
    /// <summary> Computed screen width in game units </summary>
    private float _worldScreenWidth;
    /// <summary> Scale factor used to resize objects </summary>
    private Vector2 _scaleFactor;
    /// <summary> Objects to warn in case of resize </summary>
    private List<MonoBehaviour> _resizeSubscribers = new List<MonoBehaviour>();
    
    private void Awake ()
    {
        // Screen size shouldn't change during game, so only scale it once.
        AdaptRootToViewport();
    }

    private void Start()
    {
        // Generate a random shiny background
        GenerateStars();

        // Warn subscribers about new scaling
        SubscribersCallback();
    }

    /// <summary>
    /// Used to subscribe to resize callback
    /// </summary>
    public void Subscribe(MonoBehaviour sub)
    {
        _resizeSubscribers.Add(sub);
    }

    /// <summary>
    /// Used to unsubscribe to resize callback
    /// </summary>
    public void Unsubscribe(MonoBehaviour sub)
    {
        _resizeSubscribers.Remove(sub);
    }

    /// <summary>
    /// Warn subscribers about resize
    /// </summary>
    private void SubscribersCallback()
    {
        for (int i = 0; i < _resizeSubscribers.Count; i++)
        {
            if (_resizeSubscribers[i] != null)
            {
                _resizeSubscribers[i].SendMessage("OnResizeCallback", _scaleFactor, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                // Must be an old deleted object
                _resizeSubscribers.RemoveAt(i--);
            }
        }
    }

#if UNITY_EDITOR
    // Used for debug purposes
    private void Update()
    {
        AdaptRootToViewport();
    }
#endif

    /// <summary>
    /// Scale the background object to fit viewport
    /// </summary>
    private void AdaptRootToViewport()
    {
        /* We basically define real world screen height and width
         * depending on the root background's sprite bounds
         * and then apply a corresponding localScale.
         * 
         * Values are stored for other resize effects as well as
         * star generation.                                     */

        SpriteRenderer sr = backgroundSprite;
        if (sr == null) return;

        backgroundSprite.transform.localScale = new Vector3(1, 1, 1);

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        _worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        _worldScreenWidth = _worldScreenHeight / (float)Screen.height * (float)Screen.width;

        Vector3 newScale = sr.transform.localScale;
        newScale.x = _worldScreenWidth / width;
        newScale.y = _worldScreenHeight / height;
        sr.transform.localScale = newScale;

        _scaleFactor = newScale;
    }

    /// <summary>
    /// Put stars loaded from Resources in the background
    /// </summary>
    private void GenerateStars()
    {
        // Get all sprite stored insided given Stars' resources folder
        Sprite[] _stars = Resources.LoadAll<Sprite>("Stars");

        // Instantiate starCount stars at random places around the screen
        // Higher resolutions will have "less" stars.
        //TODO : ^That could be improved.
        for (int i = 0; i < starCount; i++)
        {
            GameObject go = new GameObject("star" + i.ToString());
            go.transform.SetParent(Camera.main.transform, false);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _stars[Random.Range(0, _stars.Length)];
            sr.sortingOrder = 0;

            go.transform.position = new Vector3(Random.Range(-_worldScreenWidth/2, _worldScreenWidth/2), Random.Range(-_worldScreenHeight/2, _worldScreenHeight/2), 0);
        }
    }
}
