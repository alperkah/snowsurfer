using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SnowSurfer
{
    public sealed class FloatingText : MonoBehaviour
    {
        private static Canvas worldCanvas;
        private Text label;

        public static void Configure(Canvas canvas)
        {
            worldCanvas = canvas;
        }

        public static void Spawn(Vector3 worldPosition, string text)
        {
            if (worldCanvas == null || Camera.main == null)
            {
                return;
            }

            GameObject go = new GameObject("Floating Text");
            go.transform.SetParent(worldCanvas.transform, false);
            FloatingText floating = go.AddComponent<FloatingText>();
            floating.label = go.AddComponent<Text>();
            floating.label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            floating.label.fontSize = 32;
            floating.label.alignment = TextAnchor.MiddleCenter;
            floating.label.color = new Color(0.22f, 0.36f, 0.56f, 1f);
            floating.label.text = text;
            floating.label.raycastTarget = false;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, 52f);
            rect.position = Camera.main.WorldToScreenPoint(worldPosition);
            floating.StartCoroutine(floating.Routine(rect));
        }

        private IEnumerator Routine(RectTransform rect)
        {
            float elapsed = 0f;
            Color startColor = label.color;
            Vector3 startPosition = rect.position;
            while (elapsed < 0.8f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.8f;
                rect.position = startPosition + Vector3.up * (70f * t);
                rect.localScale = Vector3.one * Mathf.Lerp(0.9f, 1.08f, Mathf.Sin(t * Mathf.PI));
                label.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
