using UnityEngine;
using UnityEngine.UI;

namespace SnowSurfer
{
    public sealed class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pauseOverlay;

        [Header("Start")]
        [SerializeField] private Text startHighScoreText;

        [Header("HUD")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text missionText;
        [SerializeField] private Text speedText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button muteButton;
        [SerializeField] private Text muteButtonText;

        [Header("Game Over")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalHighScoreText;
        [SerializeField] private Text newBestText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Button restartButton;

        private GameManager gameManager;
        private ScoreManager scoreManager;
        private DifficultyManager difficultyManager;
        private AudioManager audioManager;
        private MissionManager missionManager;
        private int lastScore = -1;
        private int lastCombo = -1;
        private string lastMission = string.Empty;

        public void Initialize(GameManager game, ScoreManager score, DifficultyManager difficulty, AudioManager audio, MissionManager missions)
        {
            gameManager = game;
            scoreManager = score;
            difficultyManager = difficulty;
            audioManager = audio;
            missionManager = missions;

            restartButton.onClick.AddListener(gameManager.StartRun);
            pauseButton.onClick.AddListener(gameManager.TogglePause);
            muteButton.onClick.AddListener(ToggleMute);
            FloatingText.Configure(startPanel.GetComponentInParent<Canvas>());
            UpdateMuteButton();
        }

        private void Update()
        {
            if (gameManager == null || scoreManager == null || gameManager.State != GameState.Playing)
            {
                return;
            }

            UpdateHud();
        }

        public void ShowStart()
        {
            startPanel.SetActive(true);
            hudPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            pauseOverlay.SetActive(false);
            startHighScoreText.text = "Best " + scoreManager.HighScore;
        }

        public void ShowPlaying()
        {
            startPanel.SetActive(false);
            hudPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            pauseOverlay.SetActive(false);
            lastScore = -1;
            lastCombo = -1;
            lastMission = string.Empty;
            UpdateHud();
        }

        public void ShowGameOver()
        {
            startPanel.SetActive(false);
            hudPanel.SetActive(false);
            gameOverPanel.SetActive(true);
            pauseOverlay.SetActive(false);

            finalScoreText.text = "Score " + scoreManager.Score;
            finalHighScoreText.text = "Best " + scoreManager.HighScore;
            newBestText.gameObject.SetActive(scoreManager.IsNewBest);
            summaryText.text = "Gifts " + scoreManager.GiftsCollected + "   Near Miss " + scoreManager.NearMisses + "   Time " + Mathf.FloorToInt(scoreManager.SurvivedTime) + "s";
        }

        public void SetPaused(bool paused)
        {
            pauseOverlay.SetActive(paused);
        }

        private void UpdateHud()
        {
            if (lastScore != scoreManager.Score)
            {
                scoreText.text = scoreManager.Score.ToString("000000");
                highScoreText.text = "Best " + scoreManager.HighScore;
                lastScore = scoreManager.Score;
            }

            if (lastCombo != scoreManager.Combo)
            {
                comboText.text = scoreManager.Combo > 1 ? "Combo x" + scoreManager.Combo : "Combo -";
                lastCombo = scoreManager.Combo;
            }

            if (missionManager != null && lastMission != missionManager.CurrentMessage)
            {
                missionText.text = missionManager.CurrentMessage;
                lastMission = missionManager.CurrentMessage;
            }

            speedText.text = "Speed " + difficultyManager.CurrentSpeed.ToString("0.0");
        }

        private void ToggleMute()
        {
            audioManager.ToggleMuted();
            UpdateMuteButton();
        }

        private void UpdateMuteButton()
        {
            muteButtonText.text = audioManager.IsMuted ? "SFX Off" : "SFX On";
        }
    }
}
