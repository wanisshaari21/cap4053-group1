using UnityEngine;
using UnityEngine.Events;

public class PuzzleTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Seconds for the countdown.")]
    public float durationSeconds = 30f;

    [Tooltip("Start the timer automatically when the puzzle opens/enables.")]
    public bool autoStartOnEnable = true;

    [Tooltip("Use unscaled time (keeps counting if Time.timeScale == 0).")]
    public bool useUnscaledTime = false;

    [Header("UI (optional)")]
    [Tooltip("TextMeshPro label for time. Leave null if not using TMP.")]
    public TMPro.TMP_Text tmpLabel;
    [Tooltip("Legacy UI Text label. Leave null if not using it.")]
    public UnityEngine.UI.Text uiText;

    [Header("Events")]
    [Tooltip("Called when time hits 0 (only once). Wire your close/reset logic here.")]
    public UnityEvent onExpired;

    [Tooltip("Called whenever the timer starts.")]
    public UnityEvent onStarted;
    [Tooltip("Called if the timer is stopped manually before expiring.")]
    public UnityEvent onStopped;

    public bool IsRunning { get; private set; }
    public float Remaining { get; private set; }

    float _endTime;       // world time when the timer ends
    bool _firedExpired;  // to ensure onExpired only fires once

    void OnEnable()
    {
        _firedExpired = false;
        if (autoStartOnEnable) StartTimer();
        else UpdateLabel(durationSeconds);
    }

    void OnDisable()
    {
        // Stop visual updates when object is hidden/disabled
        IsRunning = false;
    }

    void Update()
    {
        if (!IsRunning) return;

        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        Remaining = Mathf.Max(0f, _endTime - now);
        UpdateLabel(Remaining);

        if (Remaining <= 0f && !_firedExpired)
        {
            _firedExpired = true;
            IsRunning = false;
            onExpired?.Invoke();
        }
    }

    // ---- Public API ----
    public void StartTimer()
    {
        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        _endTime = now + durationSeconds;
        Remaining = durationSeconds;
        _firedExpired = false;
        IsRunning = true;
        UpdateLabel(Remaining);
        onStarted?.Invoke();
    }

    public void StartTimer(float seconds)
    {
        durationSeconds = seconds;
        StartTimer();
    }

    public void StopTimer()
    {
        if (!IsRunning) return;
        IsRunning = false;
        onStopped?.Invoke();
    }

    public void ResetTimer()
    {
        _firedExpired = false;
        Remaining = durationSeconds;
        UpdateLabel(Remaining);
    }

    // ---- Helpers ----
    void UpdateLabel(float t)
    {
        if (tmpLabel == null && uiText == null) return;

        int sec = Mathf.CeilToInt(t);
        int m = sec / 60;
        int s = sec % 60;
        string text = $"{m:0}:{s:00}";

        if (tmpLabel != null) tmpLabel.text = text;
        if (uiText != null) uiText.text = text;
    }
}
