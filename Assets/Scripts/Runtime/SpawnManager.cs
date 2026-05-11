using System.Collections.Generic;
using UnityEngine;

namespace SnowSurfer
{
    public sealed class SpawnManager : MonoBehaviour
    {
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private GameObject[] obstaclePrefabs;
        [SerializeField] private GameObject[] collectiblePrefabs;
        [SerializeField] private float spawnYExtra = 1.35f;
        [SerializeField] private float lanePadding = 0.9f;
        [SerializeField] private float minimumSameLaneGap = 1.15f;

        private readonly List<float> recentLanes = new List<float>(4);
        private GameManager gameManager;
        private DifficultyManager difficultyManager;
        private ScoreManager scoreManager;
        private PlayerController player;
        private Camera mainCamera;
        private float nextSpawnTime;
        private float nextCollectibleTime;

        public void Initialize(GameManager manager, DifficultyManager difficulty, ScoreManager score, PlayerController playerController)
        {
            gameManager = manager;
            difficultyManager = difficulty;
            scoreManager = score;
            player = playerController;
            mainCamera = Camera.main;

            WarmPools();
        }

        private void Update()
        {
            if (gameManager == null || !gameManager.IsPlaying)
            {
                return;
            }

            if (Time.time >= nextSpawnTime)
            {
                SpawnObstaclePattern();
                nextSpawnTime = Time.time + difficultyManager.SpawnInterval;
            }

            if (Time.time >= nextCollectibleTime)
            {
                TrySpawnCollectible();
                nextCollectibleTime = Time.time + Random.Range(0.85f, 1.55f);
            }
        }

        public void ResetRun()
        {
            mainCamera = Camera.main;
            recentLanes.Clear();
            nextSpawnTime = Time.time + 0.45f;
            nextCollectibleTime = Time.time + 0.8f;
            if (objectPool != null)
            {
                objectPool.ReturnAllActive();
            }
        }

        public void StopSpawning()
        {
            recentLanes.Clear();
        }

        private void SpawnObstaclePattern()
        {
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            {
                return;
            }

            int count = difficultyManager.DifficultyTier >= 3 && Random.value < 0.28f ? 2 : 1;
            List<float> lanes = ChooseFairLanes(count);
            foreach (float lane in lanes)
            {
                GameObject prefab = obstaclePrefabs[Random.Range(0, Mathf.Min(obstaclePrefabs.Length, 3 + difficultyManager.DifficultyTier))];
                GameObject instance = objectPool.Get(prefab, new Vector3(lane, SpawnY(), 0f), Quaternion.identity);
                InitializeSpawned(instance);
            }
        }

        private void TrySpawnCollectible()
        {
            if (collectiblePrefabs == null || collectiblePrefabs.Length == 0 || Random.value > difficultyManager.CollectibleSpawnChance)
            {
                return;
            }

            float lane = ChooseLaneAvoidingRecent();
            GameObject prefab = collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)];
            GameObject instance = objectPool.Get(prefab, new Vector3(lane, SpawnY() + Random.Range(0.25f, 0.8f), 0f), Quaternion.identity);
            InitializeSpawned(instance);
        }

        private void InitializeSpawned(GameObject instance)
        {
            if (instance.TryGetComponent(out MovingEntity mover))
            {
                mover.Initialize(gameManager, difficultyManager);
            }

            if (instance.TryGetComponent(out Obstacle obstacle))
            {
                obstacle.Initialize(player, scoreManager, gameManager.Audio);
            }
        }

        private List<float> ChooseFairLanes(int count)
        {
            List<float> lanes = new List<float>(count);
            for (int i = 0; i < count; i++)
            {
                float lane = ChooseLaneAvoidingRecent();
                int guard = 0;
                while (lanes.Exists(existing => Mathf.Abs(existing - lane) < minimumSameLaneGap) && guard < 8)
                {
                    lane = ChooseLaneAvoidingRecent();
                    guard++;
                }

                lanes.Add(lane);
                RememberLane(lane);
            }

            return lanes;
        }

        private float ChooseLaneAvoidingRecent()
        {
            Bounds bounds = CameraBounds();
            float minX = bounds.min.x + lanePadding;
            float maxX = bounds.max.x - lanePadding;
            float lane = Random.Range(minX, maxX);
            int guard = 0;
            while (recentLanes.Exists(existing => Mathf.Abs(existing - lane) < minimumSameLaneGap) && guard < 8)
            {
                lane = Random.Range(minX, maxX);
                guard++;
            }

            return lane;
        }

        private void RememberLane(float lane)
        {
            recentLanes.Add(lane);
            if (recentLanes.Count > 3)
            {
                recentLanes.RemoveAt(0);
            }
        }

        private float SpawnY()
        {
            return CameraBounds().max.y + spawnYExtra;
        }

        private Bounds CameraBounds()
        {
            if (mainCamera == null)
            {
                return new Bounds(Vector3.zero, new Vector3(8f, 10f, 0f));
            }

            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;
            return new Bounds(mainCamera.transform.position, new Vector3(halfWidth * 2f, halfHeight * 2f, 0f));
        }

        private void WarmPools()
        {
            if (objectPool == null)
            {
                return;
            }

            foreach (GameObject prefab in obstaclePrefabs)
            {
                objectPool.Warm(prefab, 4);
            }

            foreach (GameObject prefab in collectiblePrefabs)
            {
                objectPool.Warm(prefab, 6);
            }
        }
    }
}
