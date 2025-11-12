using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyBrain))]
public class EnemyChaseAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip chaseLoop;          // looping “danger” bed
    public AudioClip chaseStartStinger;  // optional one-shot when chase begins
    public AudioClip chaseEndStinger;    // optional one-shot when chase ends

    [Header("Settings")]
    [Range(0f, 1f)] public float loopVolume = 0.6f;
    public float fadeTime = 0.35f;       // crossfade speed

    [Header("Routing")]
    public AudioSource loopSource;       // 2D AudioSource for the loop (created below)
    public AudioSource sfxSource;        // 2D AudioSource for one-shots (optional)

    private EnemyBrain brain;
    private EnemyState lastState;

    void Awake()
    {
        brain = GetComponent<EnemyBrain>();

        // Ensure sources exist
        if (!loopSource) loopSource = gameObject.AddComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();

        // Configure sources
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 0f;     // 2D so it’s always audible
        loopSource.volume = 0f;           // start silent

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;      // 2D one-shots
    }

    void Start()
    {
        lastState = brain.state;
        if (chaseLoop) loopSource.clip = chaseLoop;
    }

    void Update()
    {
        var state = brain.state;
        if (state == lastState) return;

        // Transition handling
        if (lastState != EnemyState.Chase && state == EnemyState.Chase)
        {
            // Entered Chase
            if (chaseStartStinger) sfxSource.PlayOneShot(chaseStartStinger);
            if (chaseLoop)
            {
                if (!loopSource.isPlaying) loopSource.Play();
                StopAllCoroutines();
                StartCoroutine(Fade(loopSource, target: loopVolume));
            }
        }
        else if (lastState == EnemyState.Chase && state != EnemyState.Chase)
        {
            // Exited Chase (to Search/Patrol)
            if (chaseEndStinger) sfxSource.PlayOneShot(chaseEndStinger);
            StopAllCoroutines();
            StartCoroutine(FadeAndMaybeStop(loopSource, target: 0f));
        }

        lastState = state;
    }

    IEnumerator Fade(AudioSource src, float target)
    {
        float start = src.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        src.volume = target;
    }

    IEnumerator FadeAndMaybeStop(AudioSource src, float target)
    {
        yield return Fade(src, target);
        if (Mathf.Approximately(target, 0f)) src.Stop();
    }
}
