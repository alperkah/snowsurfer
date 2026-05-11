using UnityEngine;

namespace SnowSurfer
{
    public sealed class Obstacle : MonoBehaviour
    {
        [SerializeField] private float nearMissXDistance = 0.8f;
        [SerializeField] private float nearMissYWindow = 0.45f;

        private PlayerController player;
        private ScoreManager scoreManager;
        private AudioManager audioManager;
        private bool nearMissAwarded;

        public void Initialize(PlayerController playerController, ScoreManager score, AudioManager audio)
        {
            player = playerController;
            scoreManager = score;
            audioManager = audio;
        }

        private void OnEnable()
        {
            nearMissAwarded = false;
        }

        private void Update()
        {
            if (nearMissAwarded || player == null || scoreManager == null || !GameManager.Instance.IsPlaying)
            {
                return;
            }

            Vector3 delta = transform.position - player.transform.position;
            if (delta.y < 0f && Mathf.Abs(delta.y) < nearMissYWindow && Mathf.Abs(delta.x) < nearMissXDistance)
            {
                nearMissAwarded = true;
                scoreManager.AddNearMiss();
                audioManager.PlayCombo(Mathf.Clamp(scoreManager.Combo + 1, 1, 8));
            }
        }
    }
}
