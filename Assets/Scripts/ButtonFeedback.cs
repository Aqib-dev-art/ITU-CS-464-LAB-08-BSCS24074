using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// Add to every Button: shrinks on press, springs back with overshoot (EaseOutBack).
/// Lab 08: also plays hover and press sounds through UIAudioPlayer.
public class ButtonFeedback : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
    public float pressedScale = 0.92f;
    Coroutine routine;

    // ── Pointer events ────────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData e)
    {
        UIAudioPlayer.I?.PlayButtonHover();      // Lab 08: hover sound
    }

    public void OnPointerDown(PointerEventData e)
    {
        UIAudioPlayer.I?.PlayButtonClick();      // Lab 08: press sound
        ScaleTo(pressedScale, 0.08f, false);
    }

    public void OnPointerUp(PointerEventData e)   { ScaleTo(1f, 0.25f, true); }
    public void OnPointerExit(PointerEventData e) { ScaleTo(1f, 0.15f, false); }

    void OnDisable() { transform.localScale = Vector3.one; }

    void ScaleTo(float target, float time, bool back)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run(target, time, back));
    }

    IEnumerator Run(float target, float time, bool back)
    {
        float start = transform.localScale.x, t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / time;
            float k = Mathf.Clamp01(t);
            float e = back ? EaseOutBack(k) : 1f - (1f - k) * (1f - k); // EaseOutQuad
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(start, target, e);
            yield return null;
        }
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
