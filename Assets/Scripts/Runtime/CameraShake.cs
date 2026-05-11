using System.Collections;
using UnityEngine;

namespace SnowSurfer
{
    public sealed class CameraShake : MonoBehaviour
    {
        private Vector3 basePosition;
        private Coroutine shakeRoutine;

        private void Awake()
        {
            basePosition = transform.localPosition;
        }

        public void Shake(float duration, float magnitude)
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float fade = 1f - elapsed / duration;
                Vector2 offset = Random.insideUnitCircle * (magnitude * fade);
                transform.localPosition = basePosition + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            transform.localPosition = basePosition;
            shakeRoutine = null;
        }
    }
}
