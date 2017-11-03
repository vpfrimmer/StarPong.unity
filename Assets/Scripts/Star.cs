using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///=================================================================================================================	<summary>
/// Represents a star, handles its (de)activation and links drawing                                    					</summary>
///=================================================================================================================
public class Star : MonoBehaviour {

    #region variables

    [HideInInspector]
    public bool isActivated = false;

    public Star[] neighbors = new Star[0];

    private Animator _animator;
    private List<GameObject> _links = new List<GameObject>();
    

    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
	
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!isActivated && other.transform.CompareTag("Ball") && Ball.Instance.Player != -1)
        {
            GameManager.Instance.PlayPositive();
            _animator.SetTrigger("Touched");
            isActivated = true;
            DrawLinks();
        }
    }

    private void DrawLinks()
    {
        foreach(Star s in neighbors)
        {
            if(s != null && s.isActivated)
            {
                StartCoroutine(DrawLink_Routine(s.transform.position));
            }
        }
    }

    public void Initialize()
    {
        foreach(var o in _links)
        {
            Destroy(o);
        }

        _links.Clear();
        isActivated = false;

        if(isActivated)
        {
            _animator.SetTrigger("Init");
        }
    }

    private IEnumerator DrawLink_Routine(Vector3 target)
    {
        GameObject link = Instantiate(ConstellationManager.Instance.linkPrefab);
        link.transform.SetParent(transform, false);
        LineRenderer lr = link.GetComponent<LineRenderer>();
        Vector3 midPos = Vector3.Lerp(transform.position, target, 0.5f);

        float duration = 1.5f;

        float t = 0f;
        while(t < duration)
        {
            lr.SetPosition(0, Vector3.Lerp(midPos, target, t / duration));
            lr.SetPosition(1, Vector3.Lerp(midPos, transform.position, t / duration));

            yield return null;
            t += Time.deltaTime;
        }

        lr.SetPosition(0, target);
        lr.SetPosition(1, transform.position);

        _links.Add(link);
    }
}
