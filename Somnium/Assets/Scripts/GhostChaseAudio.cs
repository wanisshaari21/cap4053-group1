using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GhostChaseAudio : MonoBehaviour
{
    [Header("Refs")]
    public GhostBrain brain;         
    [Header("Chase Audio")]
    public AudioClip chaseLoop;       
    [Range(0f, 1f)]
    public float volume = 0.8f;
    public bool use3DSound = false;   

    private AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();

        // Basic audio source setup
        _src.playOnAwake = false;
        _src.loop = true;
        _src.volume = volume;
        _src.spatialBlend = use3DSound ? 1f : 0f;   // 0 = 2D, 1 = 3D

        if (chaseLoop)
            _src.clip = chaseLoop;

        if (!brain)
            brain = GetComponent<GhostBrain>();     
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
