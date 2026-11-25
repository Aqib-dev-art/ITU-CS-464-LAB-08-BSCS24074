using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages the Audio Mixer volume controls.
/// Converts slider values (0-1 linear) ↔ dB using logarithmic mapping.
/// Saves/loads all group volumes via PlayerPrefs so settings persist after restart.
/// </summary>
public class AudioMixerController : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("Exposed Parameter Names (must match Mixer)")]
    public string paramMaster = "MasterVol";
    public string paramMusic  = "MusicVol";
    public string paramSFX    = "SFXVol";
    public string paramUI     = "UIVol";

    // ── PlayerPrefs keys ──────────────────────────────────────────────────────
    const string KEY_MASTER = "vol_master";
    const string KEY_MUSIC  = "vol_music";
    const string KEY_SFX    = "vol_sfx";
    const string KEY_UI     = "vol_ui";

    // Minimum dB floor to avoid -infinity when slider = 0
    const float MIN_DB = -80f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake() => LoadAll();

    void OnApplicationQuit() => PlayerPrefs.Save();

    // ── Public: called by UI sliders OnValueChanged ───────────────────────────

    public void SetMaster(float linear) => SetGroup(paramMaster, KEY_MASTER, linear);
    public void SetMusic (float linear) => SetGroup(paramMusic,  KEY_MUSIC,  linear);
    public void SetSFX   (float linear) => SetGroup(paramSFX,    KEY_SFX,    linear);
    public void SetUI    (float linear) => SetGroup(paramUI,     KEY_UI,     linear);

    // ── Conversion helpers (public so sliders can initialise their own value) ──

    /// <summary>Convert a 0–1 linear slider to decibels (logarithmic).</summary>
    public static float LinearToDb(float linear)
    {
        // Clamp to a tiny positive value to avoid log(0) = -infinity
        linear = Mathf.Max(linear, 0.0001f);
        return Mathf.Max(Mathf.Log10(linear) * 20f, MIN_DB);
    }

    /// <summary>Convert decibels back to a 0–1 linear slider value.</summary>
    public static float DbToLinear(float db)
    {
        if (db <= MIN_DB) return 0f;
        return Mathf.Pow(10f, db / 20f);
    }

    // ── Initialise slider values from saved prefs ─────────────────────────────

    /// <summary>
    /// Returns the saved linear (0-1) value for a group so a Slider can
    /// initialise itself without calling SetGroup (which would re-save).
    /// </summary>
    public float GetSavedLinear(string key, float defaultLinear = 1f)
        => PlayerPrefs.GetFloat(key, defaultLinear);

    public float GetSavedMaster() => GetSavedLinear(KEY_MASTER);
    public float GetSavedMusic()  => GetSavedLinear(KEY_MUSIC);
    public float GetSavedSFX()    => GetSavedLinear(KEY_SFX);
    public float GetSavedUI()     => GetSavedLinear(KEY_UI);

    // ── Internal ──────────────────────────────────────────────────────────────

    void SetGroup(string param, string prefKey, float linear)
    {
        float db = LinearToDb(linear);
        mixer.SetFloat(param, db);
        PlayerPrefs.SetFloat(prefKey, linear);  // store linear for easy slider reload
    }

    void LoadAll()
    {
        ApplyGroup(paramMaster, PlayerPrefs.GetFloat(KEY_MASTER, 1f));
        ApplyGroup(paramMusic,  PlayerPrefs.GetFloat(KEY_MUSIC,  1f));
        ApplyGroup(paramSFX,    PlayerPrefs.GetFloat(KEY_SFX,    1f));
        ApplyGroup(paramUI,     PlayerPrefs.GetFloat(KEY_UI,     1f));
    }

    void ApplyGroup(string param, float linear)
    {
        if (!mixer) return;
        mixer.SetFloat(param, LinearToDb(linear));
    }
}
