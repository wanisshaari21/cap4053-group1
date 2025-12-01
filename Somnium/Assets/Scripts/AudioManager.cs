using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }
    private bool isMuted = false;


    [Header("Routing")]
    public AudioMixer masterMixer;
    public AudioSource musicSource;   // assign the AudioSource on this GameObject

    [Header("Default Music")]
    [Range(0f,1f)] public float musicVolume = 1.0f;   // start LOUD
    public AudioClip backgroundMusic;
    public float fadeTime = 0.75f;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource)
        {
            musicSource.loop         = true;
            musicSource.spatialBlend = 0f;   // 2D
            musicSource.mute         = false;
            musicSource.volume       = musicVolume;
        }

        // global safety
        AudioListener.volume = 1f;
    }

    void Start()
    {
        if (backgroundMusic && musicSource && !musicSource.isPlaying)
        {
            musicSource.clip   = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
            Debug.Log($"[AudioManager] Start playing at volume {musicSource.volume}");
        }
    }

    // ---------- SLIDER HOOK ----------

    public void SetVolumeFromSlider(float value)
    {
        // slider is 0–1
        musicVolume = Mathf.Clamp01(value);

        if (musicSource)
            musicSource.volume = musicVolume;

        // also scale global listener so EVERYTHING follows the slider
        AudioListener.volume = musicVolume;

        Debug.Log($"[AudioManager] Slider: {value:F2}, " +
                  $"musicSource.volume={musicSource?.volume}, " +
                  $"listener={AudioListener.volume}");
    }

    public void ToggleMute()
    {
        if (!musicSource) return;
        musicSource.mute = !musicSource.mute;
        AudioListener.volume = musicSource.mute ? 0f : musicVolume;
        Debug.Log("[AudioManager] Muted: " + musicSource.mute);
    }

    // ---------- Fade helpers (unchanged) ----------

    public void FadeTo(AudioClip newClip)
    {
        if (!musicSource) return;
        StartCoroutine(FadeRoutine(newClip));
    }

    IEnumerator FadeRoutine(AudioClip newClip)
    {
        float t = 0, start = musicSource.volume;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.clip = newClip;
        musicSource.Play();

        t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeTime);
            yield return null;
        }
    }
}
