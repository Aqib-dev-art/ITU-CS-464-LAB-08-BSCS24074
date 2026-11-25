using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Singleton that manages looping background music with crossfade between states.
/// Uses two AudioSources (A/B) for seamless cross-fading.
/// Routes through the Music mixer group.
/// Persists across scenes with DontDestroyOnLoad.
/// </summary>
public class MusicManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static MusicManager I { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Mixer Group")]
    public AudioMixerGroup musicMixerGroup;

    [Header("Music Clips")]
    public AudioClip clipMenu;
    public AudioClip clipGameplay;
    public AudioClip clipResults;

    [Header("Crossfade")]
    public float crossfadeDuration = 1.5f;

    // ── Private ───────────────────────────────────────────────────────────────
    AudioSource _srcA;
    AudioSource _srcB;
    AudioSource _active;   // currently playing full-volume source
    Coroutine   _fadeRoutine;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        _srcA = CreateSource();
        _srcB = CreateSource();
        _active = _srcA;
    }

    AudioSource CreateSource()
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.loop = true;
        s.playOnAwake = false;
        s.volume = 0f;
        // Music continues through Unity pause (time scale 0) but obeys the mixer
        s.ignoreListenerPause = false;  // paused music stops on pause — intentional
        if (musicMixerGroup) s.outputAudioMixerGroup = musicMixerGroup;
        return s;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void PlayMenu()      => CrossfadeTo(clipMenu);
    public void PlayGameplay()  => CrossfadeTo(clipGameplay);
    public void PlayResults()   => CrossfadeTo(clipResults);

    // ── Crossfade ─────────────────────────────────────────────────────────────

    void CrossfadeTo(AudioClip clip)
    {
        if (!clip) return;
        // If same track already playing, skip
        if (_active.clip == clip && _active.isPlaying) return;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(DoFade(clip));
    }

    IEnumerator DoFade(AudioClip newClip)
    {
        // Pick the inactive source as the incoming track
        AudioSource incoming = (_active == _srcA) ? _srcB : _srcA;
        incoming.clip = newClip;
        incoming.volume = 0f;
        incoming.Play();

        float t = 0f;
        float startVol = _active.volume;

        while (t < crossfadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float ratio = Mathf.Clamp01(t / crossfadeDuration);
            _active.volume  = Mathf.Lerp(startVol, 0f, ratio);
            incoming.volume = Mathf.Lerp(0f, 1f,       ratio);
            yield return null;
        }

        _active.Stop();
        _active.volume = 0f;
        incoming.volume = 1f;
        _active = incoming;
    }

    public void StopAll()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _srcA.Stop();
        _srcB.Stop();
    }
}
