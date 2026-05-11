using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SnowSurfer
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private SpawnManager spawnManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private DifficultyManager difficultyManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private MissionManager missionManager;

        private GameState previousState;

        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; } = GameState.Start;
        public bool IsPlaying => State == GameState.Playing;
        public ScoreManager Score => scoreManager;
        public DifficultyManager Difficulty => difficultyManager;
        public AudioManager Audio => audioManager;

        private void Awake()
        {
            Instance = this;
            uiManager.Initialize(this, scoreManager, difficultyManager, audioManager, missionManager);
            player.Initialize(this, scoreManager, audioManager);
            spawnManager.Initialize(this, difficultyManager, scoreManager, player);
            missionManager.Initialize(scoreManager);
        }

        private void Start()
        {
            ResetToStart();
        }

        private void Update()
        {
            if (State == GameState.Playing)
            {
                difficultyManager.Tick(Time.deltaTime);
                scoreManager.Tick(Time.deltaTime, difficultyManager.CurrentSpeed);
                missionManager.Tick();
            }

            if ((State == GameState.Start || State == GameState.GameOver) && WasStartPressed())
            {
                StartRun();
            }

            if (State == GameState.Playing && WasPausePressed())
            {
                TogglePause();
            }
        }

        public void StartRun()
        {
            Time.timeScale = 1f;
            State = GameState.Playing;
            difficultyManager.ResetRun();
            scoreManager.ResetRun();
            missionManager.ResetRun();
            player.ResetPlayer();
            spawnManager.ResetRun();
            uiManager.ShowPlaying();
            audioManager.PlayStart();
        }

        public void GameOver()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            State = GameState.GameOver;
            scoreManager.CommitHighScore();
            spawnManager.StopSpawning();
            player.PlayCrash();
            cameraShake.Shake(0.25f, 0.18f);
            audioManager.PlayGameOver();
            uiManager.ShowGameOver();
        }

        public void TogglePause()
        {
            if (State == GameState.Paused)
            {
                State = previousState == GameState.Playing ? GameState.Playing : GameState.Start;
                Time.timeScale = 1f;
                uiManager.SetPaused(false);
                return;
            }

            if (State != GameState.Playing)
            {
                return;
            }

            previousState = State;
            State = GameState.Paused;
            Time.timeScale = 0f;
            uiManager.SetPaused(true);
        }

        public void ResetToStart()
        {
            Time.timeScale = 1f;
            State = GameState.Start;
            difficultyManager.ResetRun();
            scoreManager.ResetRun();
            missionManager.ResetRun();
            player.ResetPlayer();
            spawnManager.ResetRun();
            spawnManager.StopSpawning();
            uiManager.ShowStart();
        }

        private static bool WasStartPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0) || Input.touchCount > 0;
#endif
        }

        private static bool WasPausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
        }
    }
}
