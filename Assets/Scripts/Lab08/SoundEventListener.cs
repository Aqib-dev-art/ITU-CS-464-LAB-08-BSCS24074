using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// LISTENER #2 — Sound Effects Listener.
/// Subscribes to GameEventBus events and plays matching gameplay SFX.
/// Uses PlayOneShot so sounds overlap correctly.
/// Routes through the SFX mixer group.
/// </summary>
public class SoundEventListener : MonoBehaviour
{
    [Header("SFX Clips")]
    public AudioClip clipCollect;
    public AudioClip clipDamage;
    public AudioClip clipDie;
    public AudioClip clipWin;
    public AudioClip clipLose;

    [Header("Mixer Group")]
    public AudioMixerGroup sfxMixerGroup;

    [Header("Pitch Variation on Collect")]
    [Range(0f, 0.15f)]
    public float pitchVariation = 0.08f;

    AudioSource _src;

    void Awake()
    {
        _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = false;
        if (sfxMixerGroup) _src.outputAudioMixerGroup = sfxMixerGroup;
    }

    // ── Subscribe / Unsubscribe ───────────────────────────────────────────────
    void OnEnable()
    {
        GameEventBus.OnCollected += HandleCollected;
        GameEventBus.OnDamaged   += HandleDamaged;
        GameEventBus.OnDied      += HandleDied;
        GameEventBus.OnGameWon   += HandleWon;
        GameEventBus.OnGameLost  += HandleLost;
    }

    void OnDisable()
    {
        GameEventBus.OnCollected -= HandleCollected;
        GameEventBus.OnDamaged   -= HandleDamaged;
        GameEventBus.OnDied      -= HandleDied;
        GameEventBus.OnGameWon   -= HandleWon;
        GameEventBus.OnGameLost  -= HandleLost;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    void HandleCollected(int _)
    {
        // Small pitch variation makes rapid collection sound lively
        float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        _src.pitch = pitch;
        _src.PlayOneShot(clipCollect);
    }

    void HandleDamaged(int _)
    {
        _src.pitch = 1f;
        _src.PlayOneShot(clipDamage);
    }

    void HandleDied()
    {
        _src.pitch = 1f;
        _src.PlayOneShot(clipDie);
    }

    void HandleWon()
    {
        _src.pitch = 1f;
        _src.PlayOneShot(clipWin);
        // Crossfade music to results track
        MusicManager.I?.PlayResults();
    }

    void HandleLost()
    {
        _src.pitch = 1f;
        _src.PlayOneShot(clipLose);
        // Crossfade music to results track
        MusicManager.I?.PlayResults();
    }
}
