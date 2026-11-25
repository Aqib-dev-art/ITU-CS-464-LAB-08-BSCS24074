using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LISTENER #1 — HUD Updater.
/// Subscribes to GameEventBus events and refreshes the in-game HUD.
/// Subscribe in OnEnable; unsubscribe in OnDisable (safe pattern).
/// </summary>
public class HUDListener : MonoBehaviour
{
    [Header("HUD References")]
    public TMP_Text scoreText;
    public TMP_Text hpText;
    public Slider   hpBar;
    public TMP_Text statusText;   // shows "WIN!" or "GAME OVER"

    int _score;
    int _hp = 5;
    int _maxHP = 5;

    // ── Subscribe / Unsubscribe ───────────────────────────────────────────────
    void OnEnable()
    {
        GameEventBus.OnCollected   += HandleCollected;
        GameEventBus.OnDamaged     += HandleDamaged;
        GameEventBus.OnHealed      += HandleHealed;
        GameEventBus.OnGameWon     += HandleWon;
        GameEventBus.OnGameLost    += HandleLost;
        GameEventBus.OnGameStarted += HandleStart;
        ResetDisplay();
    }

    void OnDisable()
    {
        GameEventBus.OnCollected   -= HandleCollected;
        GameEventBus.OnDamaged     -= HandleDamaged;
        GameEventBus.OnHealed      -= HandleHealed;
        GameEventBus.OnGameWon     -= HandleWon;
        GameEventBus.OnGameLost    -= HandleLost;
        GameEventBus.OnGameStarted -= HandleStart;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    void HandleStart()
    {
        _score = 0;
        _hp    = _maxHP;
        ResetDisplay();
    }

    void HandleCollected(int delta)
    {
        _score += delta;
        if (scoreText) scoreText.text = "Score: " + _score;
    }

    void HandleDamaged(int amount)
    {
        _hp = Mathf.Max(_hp - amount, 0);
        RefreshHP();
    }

    void HandleHealed(int amount)
    {
        _hp = Mathf.Min(_hp + amount, _maxHP);
        RefreshHP();
    }

    void HandleWon()
    {
        if (statusText) { statusText.gameObject.SetActive(true); statusText.text = "YOU WIN! 🏆"; }
    }

    void HandleLost()
    {
        if (statusText) { statusText.gameObject.SetActive(true); statusText.text = "GAME OVER"; }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void ResetDisplay()
    {
        if (scoreText)  scoreText.text  = "Score: 0";
        if (hpText)     hpText.text     = "HP: " + _maxHP;
        if (hpBar)      hpBar.value     = 1f;
        if (statusText) statusText.gameObject.SetActive(false);
    }

    void RefreshHP()
    {
        if (hpText) hpText.text  = "HP: " + _hp;
        if (hpBar)  hpBar.value  = (float)_hp / _maxHP;
    }
}
