using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SwissArmyKnife;

///=================================================================================================================	<summary>
/// Main game manager. Handles game states. Should be unique. 					</summary>
///=================================================================================================================
public class GameManager : SingletonPersistent<GameManager> {

    #region variables
    /// <summary> Do we need an AI ? </summary>
    public bool needsAi = false;

    /// <summary> Audio component </summary>
    private AudioSource _audio;
    
    [SerializeField]
    private AudioClip[] _positiveSounds = new AudioClip[0];
    [SerializeField]
    private AudioClip[] _negativeSounds = new AudioClip[0];
    #endregion

    public override void AwakeSingleton()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Handles Android back button
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
        }
    }

    /// <summary>
    /// Play a random positive sound
    /// </summary>
    public void PlayPositive()
    {
        PlayAudio(_positiveSounds[Random.Range(0, _positiveSounds.Length)]);
    }

    /// <summary>
    /// Play a random negative sound
    /// </summary>
    public void PlayNegative()
    {
        PlayAudio(_negativeSounds[Random.Range(0, _negativeSounds.Length)]);
    }

    /// <summary>
    /// Play given sfx sound
    /// </summary>
    /// <param name="soundName">SFX name, located in Resources/SFX</param>
    public void PlaySFX(string soundName)
    {
        AudioClip clip = Resources.Load<AudioClip>("SFX/" + soundName);
        PlayAudio(clip);
    }

    /// <summary>
    /// Play audio clip
    /// </summary>
    /// <param name="clip"></param>
    private void PlayAudio(AudioClip clip)
    {
        _audio.PlayOneShot(clip, 0.5f);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    /// <summary>
    /// Launch game scene
    /// </summary>
    /// <param name="_ai">Are we playing solo ?</param>
    public void OnStartButtonPressed(bool _ai)
    {
        needsAi = _ai;
        SceneManager.LoadScene("Game");
    }
    
    /// <summary>
    /// Callback for game start
    /// </summary>
    private void OnTimerEnded()
    {
        GameCanvas.OnTextEnded -= OnTimerEnded;
        Ball.Instance.Launch();
    }

    /// <summary>
    /// Callback for game end
    /// </summary>
    private void EndCallback()
    {
        GameCanvas.OnTextEnded -= EndCallback;
        SceneManager.LoadScene("Menu");
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        // Callback for game scene setup
        if(scene.name == "Game")
        {
            InitGame();
        }
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void InitGame()
    {
        Ball.Instance.InitBall();
        ConstellationManager.Instance.ShowRandomConstellation();
        GameCanvas.OnTextEnded += OnTimerEnded;
        GameCanvas.Instance.ShowText(new string[] { "3", "2", "1", "GO" });
    }

    /// <summary>
    /// End a game and go back to menu
    /// </summary>
    /// <param name="loser"></param>
    public void StopGame(PaddleController loser)
    {
        GameCanvas.OnTextEnded += EndCallback;
        GameCanvas.Instance.ShowText(new string[] { "Player " + loser.player + " lose !" }, 3f);
    }
}
