using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpiderChaseAudio : MonoBehaviour
{
    [Header("Refs")]
    public SpiderBrain brain;       

    [Header("Chase Audio")]
    public AudioClip chaseLoop;      // spider chase sound
    [Range(0f, 1f)]
    public float volume = 0.8f;
    public bool use3DSound = false;  // true = positional audio, false = flat 2D

    private AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();

        // basic source setup
        _src.playOnAwake = false;
        _src.loop = true;
        _src.volume = volume;
        _src.spatialBlend = use3DSound ? 1f : 0f;   // 0 = 2D, 1 = 3D

        if (chaseLoop)
            _src.clip = chaseLoop;

        if (!brain)
            brain = GetComponent<SpiderBrain>();    
    }

    void Update()
    {
        if (!brain || !_src || !chaseLoop)
            return;

        bool shouldPlay = (brain.state == EnemyState.Chase);

        if (shouldPlay && !_src.isPlaying)
        {
            _src.Play();
        }
        else if (!shouldPlay && _src.isPlaying)
        {
            _src.Stop();
        }
    }
}
