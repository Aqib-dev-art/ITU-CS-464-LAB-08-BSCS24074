using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimal tap-to-collect gameplay controller (publisher).
/// Spawns coins and enemies; raises C# events on the GameEventBus.
/// Lives on the Gameplay panel GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("HUD References")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public Slider   hpBar;

    [Header("Spawn Targets (parent transforms for coins/enemies)")]
    public RectTransform coinSpawnArea;
    public RectTransform enemySpawnArea;

    [Header("Prefab-like setup (use simple UI Images)")]
    public GameObject coinPrefab;    // small UI Image acting as coin
    public GameObject enemyPrefab;   // small UI Image acting as enemy

    [Header("Game Settings")]
    public int   maxHP          = 5;
    public float coinInterval   = 2f;
    public float enemyInterval  = 5f;
    public float gameDuration   = 30f;   // seconds to win

    // ── State ─────────────────────────────────────────────────────────────────
    int   _hp;
    int   _score;
    float _timeLeft;
    bool  _alive;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void OnEnable()
    {
        // Subscribe to GameEventBus events that PlayerController needs to react to
        // (none for now — it only publishes, but pattern is established)
        ResetGame();
    }

    void OnDisable()
    {
        _alive = false;
        StopAllCoroutines();
    }

    void Update()
    {
        if (!_alive) return;

        _timeLeft -= Time.deltaTime;
        if (timerText) timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();

        if (_timeLeft <= 0f)
        {
            _alive = false;
            GameEventBus.RaiseGameWon();
        }
    }

    // ── Game Reset ────────────────────────────────────────────────────────────
    void ResetGame()
    {
        _hp       = maxHP;
        _score    = 0;
        _timeLeft = gameDuration;
        _alive    = true;

        UpdateHUD();
        StopAllCoroutines();
        StartCoroutine(SpawnCoins());
        StartCoroutine(SpawnEnemies());
    }

    // ── HUD Helpers ───────────────────────────────────────────────────────────
    void UpdateHUD()
    {
        if (scoreText) scoreText.text = "Score: " + _score;
        if (hpBar)     hpBar.value    = (float)_hp / maxHP;
    }

    // ── Coin Spawner ──────────────────────────────────────────────────────────
    IEnumerator SpawnCoins()
    {
        while (_alive)
        {
            yield return new WaitForSeconds(coinInterval);
            if (!_alive) yield break;
            if (coinPrefab && coinSpawnArea)
            {
                var go = Instantiate(coinPrefab, coinSpawnArea);
                go.GetComponent<RectTransform>().anchoredPosition = RandomPoint(coinSpawnArea);
                var btn = go.GetComponent<UnityEngine.UI.Button>();
                if (!btn) btn = go.AddComponent<UnityEngine.UI.Button>();
                btn.onClick.AddListener(() => CollectCoin(go));
            }
        }
    }

    void CollectCoin(GameObject coin)
    {
        if (!_alive) return;
        Destroy(coin);
        _score += 10;
        UpdateHUD();
        GameEventBus.RaiseCollected(10);  // ← C# event raised here
    }

    // ── Enemy Spawner ─────────────────────────────────────────────────────────
    IEnumerator SpawnEnemies()
    {
        while (_alive)
        {
            yield return new WaitForSeconds(enemyInterval);
            if (!_alive) yield break;
            if (enemyPrefab && enemySpawnArea)
            {
                var go = Instantiate(enemyPrefab, enemySpawnArea);
                go.GetComponent<RectTransform>().anchoredPosition = RandomPoint(enemySpawnArea);
                var btn = go.GetComponent<UnityEngine.UI.Button>();
                if (!btn) btn = go.AddComponent<UnityEngine.UI.Button>();
                btn.onClick.AddListener(() => EnemyHit(go));
            }
        }
    }

    void EnemyHit(GameObject enemy)
    {
        if (!_alive) return;
        Destroy(enemy);
        _hp--;
        UpdateHUD();
        GameEventBus.RaiseDamaged(1);   // ← C# event raised here

        if (_hp <= 0)
        {
            _alive = false;
            GameEventBus.RaiseDied();     // ← C# event raised here
            GameEventBus.RaiseGameLost(); // ← C# event raised here
        }
    }

    // ── Utility ───────────────────────────────────────────────────────────────
    Vector2 RandomPoint(RectTransform rt)
    {
        float hw = rt.rect.width  * 0.4f;
        float hh = rt.rect.height * 0.4f;
        return new Vector2(Random.Range(-hw, hw), Random.Range(-hh, hh));
    }
}
