using UnityEngine;

namespace SnowSurfer
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioManager : MonoBehaviour
    {
        private const string MutedKey = "SnowSurfer.AudioMuted";

        [SerializeField] private float volume = 0.45f;

        private AudioSource source;
        private AudioClip collectClip;
        private AudioClip crashClip;
        private AudioClip startClip;
        private AudioClip gameOverClip;
        private AudioClip comboClip;

        public bool IsMuted { get; private set; }

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            IsMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            BuildClips();
        }

        public void ToggleMuted()
        {
            IsMuted = !IsMuted;
            PlayerPrefs.SetInt(MutedKey, IsMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void PlayCollect(int combo)
        {
            Play(collectClip, Mathf.Clamp01(volume + combo * 0.025f), 1f + Mathf.Min(combo, 8) * 0.055f);
            if (combo > 2)
            {
                PlayCombo(combo);
            }
        }

        public void PlayCrash()
        {
            Play(crashClip, volume * 1.25f, Random.Range(0.92f, 1.03f));
        }

        public void PlayStart()
        {
            Play(startClip, volume, 1f);
        }

        public void PlayGameOver()
        {
            Play(gameOverClip, volume, 1f);
        }

        public void PlayCombo(int combo)
        {
            Play(comboClip, Mathf.Clamp01(volume * 0.65f + combo * 0.025f), 1f + Mathf.Min(combo, 10) * 0.04f);
        }

        private void Play(AudioClip clip, float clipVolume, float pitch)
        {
            if (IsMuted || clip == null)
            {
                return;
            }

            source.pitch = pitch;
            source.PlayOneShot(clip, clipVolume);
        }

        private void BuildClips()
        {
            collectClip = CreateTone("collect_ding", 0.16f, 880f, 1420f, Wave.Sine, 0.55f);
            comboClip = CreateTone("combo_ping", 0.12f, 1200f, 1880f, Wave.Triangle, 0.38f);
            startClip = CreateTone("start_rise", 0.28f, 420f, 760f, Wave.Sine, 0.42f);
            gameOverClip = CreateTone("game_over_fall", 0.34f, 360f, 140f, Wave.Sine, 0.5f);
            crashClip = CreateNoiseThud("crash_thud", 0.22f);
        }

        private enum Wave
        {
            Sine,
            Triangle
        }

        private static AudioClip CreateTone(string name, float duration, float startFrequency, float endFrequency, Wave wave, float amplitude)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)(sampleCount - 1);
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                phase += frequency / sampleRate;
                float raw = wave == Wave.Sine
                    ? Mathf.Sin(phase * Mathf.PI * 2f)
                    : 2f * Mathf.Abs(2f * (phase - Mathf.Floor(phase + 0.5f))) - 1f;
                float envelope = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * (1f - t * 0.35f);
                samples[i] = raw * envelope * amplitude;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateNoiseThud(string name, float duration)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            float[] samples = new float[sampleCount];
            float last = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)(sampleCount - 1);
                float noise = Random.Range(-1f, 1f);
                last = Mathf.Lerp(last, noise, 0.12f);
                float bass = Mathf.Sin((70f + 25f * (1f - t)) * Mathf.PI * 2f * i / sampleRate);
                float envelope = Mathf.Pow(1f - t, 2.8f);
                samples[i] = (last * 0.45f + bass * 0.55f) * envelope * 0.75f;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
