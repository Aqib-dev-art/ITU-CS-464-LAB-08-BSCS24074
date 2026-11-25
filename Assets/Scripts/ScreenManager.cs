using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Put this on an empty GameObject called "UIManager".
/// Each screen = a panel with a CanvasGroup. Register them in the list.
/// Lab 08: wired to UIAudioPlayer for panel open/close and navigation sounds.
public class ScreenManager : MonoBehaviour
{
    [System.Serializable]
    public class ScreenEntry
    {
        public string id;          // e.g. "Title", "MainMenu"
        public CanvasGroup group;  // the panel's CanvasGroup
    }

    public List<ScreenEntry> screens;
    public string startScreen = "Title";
    public float duration = 0.35f;
    public float slideDistance = 120f;

    // ── Music trigger screens ─────────────────────────────────────────────────
    [Header("Music Transitions (optional)")]
    public string menuMusicScreen    = "MainMenu";   // triggers menu music
    public string resultsMusicScreen = "Results";    // triggers results music

    readonly Dictionary<string, CanvasGroup> map = new Dictionary<string, CanvasGroup>();
    readonly Stack<string> history = new Stack<string>();
    string current;
    bool busy;

    void Awake()
    {
        foreach (var s in screens)
        {
            map[s.id] = s.group;
            SetState(s.group, false);
        }
    }

    void Start()
    {
        current = startScreen;
        SetState(map[current], true);
        // Start menu music
        MusicManager.I?.PlayMenu();
    }

    // ---- Call these from Button OnClick ----

    /// Go forward to a screen (remembers where you came from).
    public void GoTo(string id)
    {
        if (busy || id == current) return;
        UIAudioPlayer.I?.PlayButtonClick();      // Lab 08: navigation sound
        history.Push(current);
        StartCoroutine(Transition(current, id, +1));
    }

    /// Go back to the previous screen.
    public void Back()
    {
        if (busy || history.Count == 0) return;
        UIAudioPlayer.I?.PlayButtonBack();       // Lab 08: back sound
        StartCoroutine(Transition(current, history.Pop(), -1));
    }

    /// Jump to a screen and clear history (e.g. Results -> Main Menu).
    public void GoToRoot(string id)
    {
        if (busy) return;
        UIAudioPlayer.I?.PlayButtonBack();       // Lab 08: back/cancel sound
        history.Clear();
        StartCoroutine(Transition(current, id, -1));
    }

    // ---- Internals ----

    IEnumerator Transition(string fromId, string toId, int dir)
    {
        busy = true;
        CanvasGroup from = map[fromId], to = map[toId];
        RectTransform fr = from.transform as RectTransform;
        RectTransform tr = to.transform as RectTransform;

        UIAudioPlayer.I?.PlayPanelOpen();        // Lab 08: panel opening sound

        from.interactable = false;
        to.gameObject.SetActive(true);
        to.alpha = 0; to.interactable = false; to.blocksRaycasts = true;
        tr.anchoredPosition = new Vector2(slideDistance * dir, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float k = Mathf.Clamp01(t);

            // incoming: slides in, decelerates (EaseOutCubic) -> feels responsive
            float eIn = 1f - Mathf.Pow(1f - k, 3f);
            to.alpha = eIn;
            tr.anchoredPosition = new Vector2(Mathf.Lerp(slideDistance * dir, 0, eIn), 0);

            // outgoing: fades & drifts away, accelerates (EaseInCubic) -> gets out of the way
            float eOut = k * k * k;
            from.alpha = 1f - eOut;
            fr.anchoredPosition = new Vector2(Mathf.Lerp(0, -slideDistance * dir * 0.5f, eOut), 0);
            yield return null;
        }

        SetState(from, false);
        fr.anchoredPosition = Vector2.zero;
        SetState(to, true);
        current = toId;
        busy = false;

        // Lab 08: trigger music transitions on specific screen arrivals
        TriggerMusicForScreen(toId);
    }

    void TriggerMusicForScreen(string id)
    {
        if (id == menuMusicScreen)    MusicManager.I?.PlayMenu();
        if (id == resultsMusicScreen) MusicManager.I?.PlayResults();
    }

    static void SetState(CanvasGroup g, bool on)
    {
        g.gameObject.SetActive(on);
        g.alpha = on ? 1 : 0;
        g.interactable = on;
        g.blocksRaycasts = on;
    }
}
