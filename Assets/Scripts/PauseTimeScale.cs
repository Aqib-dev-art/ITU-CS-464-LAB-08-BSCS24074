using UnityEngine;

/// Optional: pause the game on the Pause screen. Add to the Pause panel.
/// Lab 08: plays panel open/close sounds and intentionally keeps music audible
/// but gameplay SFX are silenced via Time.timeScale = 0.
public class PauseTimeScale : MonoBehaviour
{
    void OnEnable()
    {
        Time.timeScale = 0f;
        UIAudioPlayer.I?.PlayPanelOpen();   // Lab 08: pause panel opens
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        UIAudioPlayer.I?.PlayPanelClose();  // Lab 08: pause panel closes / game resumes
    }
}
