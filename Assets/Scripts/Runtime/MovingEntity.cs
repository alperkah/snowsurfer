using UnityEngine;

namespace SnowSurfer
{
    public sealed class MovingEntity : MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private float offscreenPadding = 1.4f;

        private GameManager gameManager;
        private DifficultyManager difficultyManager;
        private PooledObject pooledObject;
        private Camera mainCamera;

        private void Awake()
        {
            pooledObject = GetComponent<PooledObject>();
            mainCamera = Camera.main;
        }

        public void Initialize(GameManager manager, DifficultyManager difficulty)
        {
            gameManager = manager;
            difficultyManager = difficulty;
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (gameManager == null || difficultyManager == null || !gameManager.IsPlaying)
            {
                return;
            }

            transform.position += Vector3.down * (difficultyManager.CurrentSpeed * speedMultiplier * Time.deltaTime);

            float bottom = mainCamera != null ? mainCamera.transform.position.y - mainCamera.orthographicSize - offscreenPadding : -7f;
            if (transform.position.y < bottom)
            {
                pooledObject.Release();
            }
        }
    }
}
