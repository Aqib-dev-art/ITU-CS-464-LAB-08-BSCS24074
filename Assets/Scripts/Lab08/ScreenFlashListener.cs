using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LISTENER #3 — Screen Flash Listener.
/// Subscribes to GameEventBus events and briefly tints a full-screen Image:
///   • OnDamaged  → red flash
///   • OnCollected → gold flash
///   • OnGameWon  → green flash
///   • OnGameLost → dark flash
/// </summary>
public class ScreenFlashListener : MonoBehaviour
{
    [Header("Full-screen overlay Image (alpha starts at 0)")]
    public Image flashOverlay;

    [Header("Flash Colors")]
    public Color damageColor   = new Color(1f, 0f,   0f,   0.45f);
    public Color collectColor  = new Color(1f, 0.85f, 0f,  0.35f);
    public Color winColor      = new Color(0f, 1f,   0.3f, 0.35f);
    public Color loseColor     = new Color(0f, 0f,   0f,   0.6f);

    [Header("Timing")]
    public float flashInTime  = 0.08f;
    public float flashOutTime = 0.35f;

    Coroutine _flashRoutine;

    // ── Subscribe / Unsubscribe ───────────────────────────────────────────────
    void OnEnable()
    {
        GameEventBus.OnDamaged   += _ => Flash(damageColor);
        GameEventBus.OnCollected += _ => Flash(collectColor);
        GameEventBus.OnGameWon   += () => Flash(winColor);
        GameEventBus.OnGameLost  += () => Flash(loseColor);
    }

    void OnDisable()
    {
        GameEventBus.OnDamaged   -= _ => Flash(damageColor);
        GameEventBus.OnCollected -= _ => Flash(collectColor);
        GameEventBus.OnGameWon   -= () => Flash(winColor);
        GameEventBus.OnGameLost  -= () => Flash(loseColor);

        // Ensure overlay is hidden when disabled
        if (flashOverlay) flashOverlay.color = Color.clear;
    }

    // ── Flash ─────────────────────────────────────────────────────────────────

    void Flash(Color color)
    {
        if (!flashOverlay) return;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(DoFlash(color));
    }

    IEnumerator DoFlash(Color target)
    {
        // Fade in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / flashInTime;
            flashOverlay.color = Color.Lerp(Color.clear, target, Mathf.Clamp01(t));
            yield return null;
        }
        flashOverlay.color = target;

        // Fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / flashOutTime;
            flashOverlay.color = Color.Lerp(target, Color.clear, Mathf.Clamp01(t));
            yield return null;
        }
        flashOverlay.color = Color.clear;
    }
}
