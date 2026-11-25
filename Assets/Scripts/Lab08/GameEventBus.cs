using System;

/// <summary>
/// Static C# event hub for core gameplay events.
/// Publishers call Raise*(); listeners subscribe with += in OnEnable and -= in OnDisable.
/// All invocations use ?.Invoke() so null-checks are safe.
/// </summary>
public static class GameEventBus
{
    // ── Events ──────────────────────────────────────────────────────────────
    public static event Action<int> OnDamaged;    // param: damage amount
    public static event Action<int> OnHealed;     // param: heal amount
    public static event Action      OnDied;
    public static event Action<int> OnCollected;  // param: score delta
    public static event Action      OnGameWon;
    public static event Action      OnGameLost;
    public static event Action      OnGameStarted;

    // ── Safe invoke helpers (publishers call these) ─────────────────────────
    public static void RaiseDamaged(int v)   => OnDamaged?.Invoke(v);
    public static void RaiseHealed(int v)    => OnHealed?.Invoke(v);
    public static void RaiseDied()           => OnDied?.Invoke();
    public static void RaiseCollected(int v) => OnCollected?.Invoke(v);
    public static void RaiseGameWon()        => OnGameWon?.Invoke();
    public static void RaiseGameLost()       => OnGameLost?.Invoke();
    public static void RaiseGameStarted()    => OnGameStarted?.Invoke();

    /// <summary>
    /// Clear all subscriptions — call when unloading a gameplay scene so stale
    /// listeners from destroyed objects cannot fire.
    /// </summary>
    public static void ClearAll()
    {
        OnDamaged    = null;
        OnHealed     = null;
        OnDied       = null;
        OnCollected  = null;
        OnGameWon    = null;
        OnGameLost   = null;
        OnGameStarted = null;
    }
}
