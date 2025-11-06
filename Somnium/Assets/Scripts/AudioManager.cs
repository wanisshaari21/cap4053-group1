using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Routing")]
    public AudioMixer masterMixer;    
    public AudioSource musicSource;   // assign the AudioSource on this GameObject

    [Header("Default Music")]
    public AudioClip backgroundMusic;
    [Range(0f,1f)] public float musicVolume = 0.5f;
    public float fadeTime = 0.75f;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource)
        {
            musicSource.loop = true;
            musicSource.spatialBlend = 0f; // 2D
            musicSource.volume = musicVolume;
        }
    }

    void Start()
    {
        if (backgroundMusic && musicSource && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    // Optional helpers for later
    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (musicSource) musicSource.volume = musicVolume;
    }

    public void FadeTo(AudioClip newClip)
    {
        if (!musicSource) return;
        StartCoroutine(FadeRoutine(newClip));
    }

    IEnumerator FadeRoutine(AudioClip newClip)
    {
        float t=0, start=musicSource.volume;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, 0f, t/fadeTime);
            yield return null;
        }
        musicSource.clip = newClip;
        musicSource.Play();

        t=0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t/fadeTime);
            yield return null;
        }
    }
}
