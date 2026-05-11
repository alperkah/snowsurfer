using System;
using UnityEngine;

namespace SnowSurfer
{
    public sealed class ScoreManager : MonoBehaviour
    {
        private const string HighScoreKey = "SnowSurfer.HighScore";

        [SerializeField] private float pointsPerSecond = 12f;
        [SerializeField] private float comboTimeout = 2.2f;
        [SerializeField] private int collectibleBaseScore = 60;
        [SerializeField] private int nearMissScore = 25;

        private float scoreFloat;
        private float comboTimer;

        public int Score => Mathf.FloorToInt(scoreFloat);
        public int HighScore { get; private set; }
        public int Combo { get; private set; }
        public int GiftsCollected { get; private set; }
        public int NearMisses { get; private set; }
        public float SurvivedTime { get; private set; }
        public bool IsNewBest { get; private set; }

        public event Action ScoreChanged;

        private void Awake()
        {
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        public void ResetRun()
        {
            scoreFloat = 0f;
            comboTimer = 0f;
            Combo = 0;
            GiftsCollected = 0;
            NearMisses = 0;
            SurvivedTime = 0f;
            IsNewBest = false;
            ScoreChanged?.Invoke();
        }

        public void Tick(float deltaTime, float speed)
        {
            SurvivedTime += deltaTime;
            scoreFloat += pointsPerSecond * deltaTime * Mathf.Lerp(1f, 1.8f, Mathf.InverseLerp(3.8f, 8.6f, speed));

            if (Combo > 0)
            {
                comboTimer -= deltaTime;
                if (comboTimer <= 0f)
                {
                    Combo = 0;
                    ScoreChanged?.Invoke();
                }
            }

            ScoreChanged?.Invoke();
        }

        public int AddCollectible()
        {
            GiftsCollected++;
            Combo++;
            comboTimer = comboTimeout;
            int comboBonus = Mathf.Min(Combo - 1, 10) * 12;
            int gained = collectibleBaseScore + comboBonus;
            scoreFloat += gained;
            ScoreChanged?.Invoke();
            return gained;
        }

        public int AddNearMiss()
        {
            NearMisses++;
            scoreFloat += nearMissScore;
            ScoreChanged?.Invoke();
            return nearMissScore;
        }

        public void AddMissionBonus(int amount)
        {
            scoreFloat += amount;
            ScoreChanged?.Invoke();
        }

        public bool CommitHighScore()
        {
            IsNewBest = Score > HighScore;
            if (IsNewBest)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }

            return IsNewBest;
        }
    }
}
