using UnityEngine;

namespace SnowSurfer
{
    public sealed class DifficultyManager : MonoBehaviour
    {
        [SerializeField] private float baseSpeed = 3.8f;
        [SerializeField] private float maxSpeed = 8.6f;
        [SerializeField] private float speedRampPerSecond = 0.045f;
        [SerializeField] private float baseSpawnInterval = 1.25f;
        [SerializeField] private float minSpawnInterval = 0.58f;
        [SerializeField] private float collectibleSpawnChance = 0.58f;

        public float ElapsedTime { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float SpawnInterval { get; private set; }
        public float CollectibleSpawnChance => collectibleSpawnChance;
        public int DifficultyTier => Mathf.Clamp(Mathf.FloorToInt(ElapsedTime / 18f), 0, 5);

        private void Awake()
        {
            ResetRun();
        }

        public void ResetRun()
        {
            ElapsedTime = 0f;
            CurrentSpeed = baseSpeed;
            SpawnInterval = baseSpawnInterval;
        }

        public void Tick(float deltaTime)
        {
            ElapsedTime += deltaTime;
            CurrentSpeed = Mathf.Min(maxSpeed, baseSpeed + ElapsedTime * speedRampPerSecond + DifficultyTier * 0.26f);
            SpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - ElapsedTime * 0.012f - DifficultyTier * 0.055f);
        }
    }
}
