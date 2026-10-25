using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource buttonClickAudio;
    [SerializeField] private AudioSource introAudioSource;
    [SerializeField] private AudioClip introAudioClip;

    [Header("UI Text Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gameTimerText;
    [SerializeField] private TextMeshProUGUI ghostScaredTimerText;

    [Header("Round Start Countdown Elements")]
    [SerializeField] private GameObject blockingImage;     
    [SerializeField] private TextMeshProUGUI countdownText; 

    [Header("Lives UI Settings")]
    [SerializeField] private GameObject[] lifeIcons;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Over / Win Settings")]
    [SerializeField] private GameObject gameOverBlockingImage;
    [SerializeField] private TextMeshProUGUI gameOverText;

    private int currentScore = 0;
    private float elapsedTime = 0f;
    public bool IsGameActive { get; private set; } = false;

    void Start()
    {
        UpdateScoreDisplay();
        
        if (ghostScaredTimerText != null)
        {
            ghostScaredTimerText.gameObject.SetActive(false); 
        }

        StartCoroutine(RunRoundStartCountdown());
    }

    void Update()
    {
        if (IsGameActive)
        {
            elapsedTime += Time.deltaTime;
            UpdateGameTimerDisplay(elapsedTime);
        }
    }

    public IEnumerator RunRoundStartCountdown()
    {
        IsGameActive = false;
        GhostController.GlobalCanMove = false; // Ensure ghosts are frozen at start
        
        if (blockingImage != null) blockingImage.SetActive(true);
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        if (introAudioSource != null && introAudioClip != null)
        {
            introAudioSource.PlayOneShot(introAudioClip);
        }

        string[] countdownSteps = { "3", "2", "1", "GO!" };
        foreach (string step in countdownSteps)
        {
            if (countdownText != null) countdownText.text = step;
            yield return new WaitForSeconds(1f);
        }

        if (introAudioSource != null && introAudioSource.isPlaying)
        {
            introAudioSource.Stop();
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (blockingImage != null) blockingImage.SetActive(false);

        GhostController.GlobalCanMove = true;
        IsGameActive = true;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString("D6");
        }
    }

    private void UpdateGameTimerDisplay(float timeInSeconds)
    {
        if (gameTimerText == null) return;

        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);

        gameTimerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void UpdateScaredTimer(float timeRemaining)
    {
        if (ghostScaredTimerText == null) return;

        if (timeRemaining > 0)
        {
            ghostScaredTimerText.gameObject.SetActive(true);
            ghostScaredTimerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
        else
        {
            ghostScaredTimerText.gameObject.SetActive(false);
        }
    }

    public void UpdateLivesDisplay(int livesRemaining)
    {
        if (lifeIcons == null) return;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                lifeIcons[i].SetActive(i < livesRemaining);
            }
        }
    }

    public void TriggerWinOrGameOver(bool isWin)
    {
        IsGameActive = false;
        GhostController.GlobalCanMove = false;

        if (gameOverBlockingImage != null) gameOverBlockingImage.SetActive(true);
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = isWin ? "LEVEL CLEAR!" : "GAME OVER";
        }

        SaveScoreAndStats();

        StartCoroutine(EndGameSequenceRoutine());
    }

    private void SaveScoreAndStats()
    {
        int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        float savedBestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);

        if (currentScore > savedHighScore || (currentScore == savedHighScore && elapsedTime < savedBestTime))
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.SetFloat("BestTime", elapsedTime);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator EndGameSequenceRoutine()
    {
        yield return new WaitForSeconds(3f);
        ReturnToStartScene();
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToStartScene()
    {
        PlayClickSoundAndLoad("StartScene"); 
    }



    private void PlayClickSoundAndLoad(string sceneName)
    {
        if (buttonClickAudio != null)
        {
            buttonClickAudio.Play();
        }
        
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"Scene '{sceneName}' is not added to Build Settings yet!");
        }
    }


}