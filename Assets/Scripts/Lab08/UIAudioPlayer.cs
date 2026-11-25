using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Reusable, singleton UI audio player.
/// Routes all UI sounds through one AudioSource → UI mixer group.
/// Use UIAudioPlayer.I.Play(clip) or UIAudioPlayer.I.Play(SoundID) from any script.
/// Avoids placing an AudioSource on every button.
/// </summary>
public class UIAudioPlayer : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
    public static UIAudioPlayer I { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Mixer")]
    public AudioMixerGroup uiMixerGroup;

    [Header("UI Sound Clips")]
    public AudioClip clipButtonClick;
    public AudioClip clipButtonHover;
    public AudioClip clipButtonBack;
    public AudioClip clipPanelOpen;
    public AudioClip clipPanelClose;
    public AudioClip clipSuccess;
    public AudioClip clipError;

    [Header("Pitch Variation")]
    [Range(0f, 0.15f)]
    public float pitchVariation = 0.05f;   // ±5 % on repeated clicks

    // ── Private ───────────────────────────────────────────────────────────────
    AudioSource _src;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop        = false;
        if (uiMixerGroup) _src.outputAudioMixerGroup = uiMixerGroup;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Play(AudioClip clip, bool randomPitch = false)
    {
        if (!clip) return;
        float pitch = randomPitch
            ? 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation)
            : 1f;
        _src.pitch = pitch;
        _src.PlayOneShot(clip);  // overlapping short effects
    }

    public void PlayButtonClick()  => Play(clipButtonClick,  randomPitch: true);
    public void PlayButtonHover()  => Play(clipButtonHover,  randomPitch: false);
    public void PlayButtonBack()   => Play(clipButtonBack,   randomPitch: false);
    public void PlayPanelOpen()    => Play(clipPanelOpen,    randomPitch: false);
    public void PlayPanelClose()   => Play(clipPanelClose,   randomPitch: false);
    public void PlaySuccess()      => Play(clipSuccess,      randomPitch: false);
    public void PlayError()        => Play(clipError,        randomPitch: false);
}
