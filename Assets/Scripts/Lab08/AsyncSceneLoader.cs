using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles asynchronous scene loading for the Loading screen.
/// Place on the Loading panel object (or a persistent manager).
/// Connect LoadingBar (Slider) and LoadingText (TMP_Text) in the Inspector.
/// OnEnable starts loading; on completion it switches to the gameplay screen.
/// </summary>
public class AsyncSceneLoader : MonoBehaviour
{
    [Header("Scene to Load")]
    public string gameplaySceneName = "GameplayScene";

    [Header("UI References")]
    public Slider     loadingBar;
    public TMP_Text   loadingText;

    [Header("Screen Manager")]
    public ScreenManager screenManager;
    public string gameplayScreenID = "Gameplay";

    // Minimum simulated load time so the bar is always visible
    [Header("Feel")]
    [Range(0.5f, 3f)]
    public float minimumLoadSeconds = 1.2f;

    void OnEnable()
    {
        // Reset bar to 0 on every entry
        if (loadingBar)  { loadingBar.value = 0f; }
        if (loadingText) { loadingText.text  = "0%"; }
        StartCoroutine(LoadAsync());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator LoadAsync()
    {
        // Trigger crossfade to gameplay music BEFORE the load completes
        // so the transition feels intentional
        MusicManager.I?.PlayGameplay();

        AsyncOperation op = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);
        if (op == null)
        {
            // Scene not in build — simulate progress for single-scene mode
            yield return SimulateLoad();
            FinishLoad();
            yield break;
        }

        // Block automatic scene activation until we're ready
        op.allowSceneActivation = false;

        float elapsed = 0f;

        while (!op.isDone)
        {
            elapsed += Time.unscaledDeltaTime;

            // Unity reports 0–0.9 during load, then jumps to 1.0 after activation
            float realProgress  = Mathf.Clamp01(op.progress / 0.9f);
            float timeProgress  = Mathf.Clamp01(elapsed / minimumLoadSeconds);
            float displayedProgress = Mathf.Min(realProgress, timeProgress);

            SetProgress(displayedProgress);

            // Allow activation only when both real and simulated progress are done
            if (op.progress >= 0.9f && elapsed >= minimumLoadSeconds)
            {
                SetProgress(1f);
                yield return new WaitForSecondsRealtime(0.3f); // brief pause at 100%
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        FinishLoad();
    }

    IEnumerator SimulateLoad()
    {
        float elapsed = 0f;
        while (elapsed < minimumLoadSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            SetProgress(Mathf.Clamp01(elapsed / minimumLoadSeconds));
            yield return null;
        }
        SetProgress(1f);
        yield return new WaitForSecondsRealtime(0.3f);
    }

    void SetProgress(float t)
    {
        if (loadingBar)  loadingBar.value  = t;
        if (loadingText) loadingText.text  = Mathf.RoundToInt(t * 100f) + "%";
    }

    void FinishLoad()
    {
        GameEventBus.RaiseGameStarted();
        // Navigate to the in-scene Gameplay panel
        if (screenManager) screenManager.GoTo(gameplayScreenID);
    }
}
