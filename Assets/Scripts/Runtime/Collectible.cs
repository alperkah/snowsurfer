using System.Collections;
using UnityEngine;

namespace SnowSurfer
{
    public sealed class Collectible : MonoBehaviour
    {
        [SerializeField] private float spinDegreesPerSecond = 120f;
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobFrequency = 4f;

        private Vector3 baseScale;
        private Vector3 spawnPosition;
        private PooledObject pooledObject;
        private Collider2D hitbox;
        private bool collected;

        private void Awake()
        {
            baseScale = transform.localScale;
            pooledObject = GetComponent<PooledObject>();
            hitbox = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            transform.localScale = baseScale;
            spawnPosition = transform.position;
            collected = false;
            if (hitbox != null)
            {
                hitbox.enabled = true;
            }
        }

        private void Update()
        {
            Transform visual = transform.childCount > 0 ? transform.GetChild(0) : transform;
            visual.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);
            visual.localPosition = new Vector3(visual.localPosition.x, Mathf.Sin(Time.time * bobFrequency + spawnPosition.x) * bobAmplitude, visual.localPosition.z);
        }

        public void Collect(ScoreManager scoreManager, AudioManager audioManager)
        {
            if (collected)
            {
                return;
            }

            collected = true;
            if (hitbox != null)
            {
                hitbox.enabled = false;
            }

            int gained = scoreManager.AddCollectible();
            audioManager.PlayCollect(scoreManager.Combo);
            StartCoroutine(CollectRoutine(gained));
        }

        private IEnumerator CollectRoutine(int gained)
        {
            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < 0.14f)
            {
                t += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(t / 0.14f * Mathf.PI) * 0.45f;
                transform.localScale = startScale * pulse;
                yield return null;
            }

            FloatingText.Spawn(transform.position + Vector3.up * 0.35f, "+" + gained);
            pooledObject.Release();
        }
    }
}
