using UnityEngine;

namespace SnowSurfer
{
    public sealed class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private DifficultyManager difficultyManager;
        [SerializeField] private Transform[] groundTiles;
        [SerializeField] private Transform[] clouds;
        [SerializeField] private float groundTileHeight = 3.58f;
        [SerializeField] private float cloudResetPadding = 2f;
        [SerializeField] private float groundSpeedMultiplier = 0.42f;
        [SerializeField] private float cloudSpeedMultiplier = 0.08f;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (difficultyManager == null)
            {
                return;
            }

            float speed = difficultyManager.CurrentSpeed;
            ScrollGround(speed);
            ScrollClouds(speed);
        }

        private void ScrollGround(float speed)
        {
            if (groundTiles == null || groundTiles.Length == 0)
            {
                return;
            }

            float bottom = CameraBottom() - groundTileHeight;
            foreach (Transform tile in groundTiles)
            {
                tile.position += Vector3.down * (speed * groundSpeedMultiplier * Time.deltaTime);
                if (tile.position.y < bottom)
                {
                    tile.position += Vector3.up * groundTileHeight * 5f;
                }
            }
        }

        private void ScrollClouds(float speed)
        {
            if (clouds == null || clouds.Length == 0)
            {
                return;
            }

            float bottom = CameraBottom() - cloudResetPadding;
            float top = CameraTop() + cloudResetPadding;
            float halfWidth = CameraHalfWidth();
            foreach (Transform cloud in clouds)
            {
                cloud.position += new Vector3(Mathf.Sin(Time.time * 0.35f + cloud.position.y) * 0.08f, -speed * cloudSpeedMultiplier, 0f) * Time.deltaTime;
                if (cloud.position.y < bottom)
                {
                    cloud.position = new Vector3(Random.Range(-halfWidth, halfWidth - 2f), top + Random.Range(0f, 1.6f), cloud.position.z);
                }
            }
        }

        private float CameraBottom()
        {
            return mainCamera != null ? mainCamera.transform.position.y - mainCamera.orthographicSize : -5f;
        }

        private float CameraTop()
        {
            return mainCamera != null ? mainCamera.transform.position.y + mainCamera.orthographicSize : 5f;
        }

        private float CameraHalfWidth()
        {
            return mainCamera != null ? mainCamera.orthographicSize * mainCamera.aspect : 5f;
        }
    }
}
