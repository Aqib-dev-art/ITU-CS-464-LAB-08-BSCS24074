# ITU-CS-464-LAB-08-BSCS24074
**Unity 6.3 LTS (6000.3.24f1) · URP · Mobile UI + Audio + C# Events**

> Roll Number: BSCS24074  
> Built on top of Lab 07 — all 10 screens remain functional.

video link : https://drive.google.com/file/d/1vVEvBZfL-Q39jG8zl57CbKbJaGeVJx_J/view?usp=sharing
---

## ⚙️ Setup Steps

1. Clone or download the repository.
2. Open **Unity Hub → Open Project** and select the root folder.
3. Wait for the project to reimport (first open takes ~2–3 minutes).
4. Open `Assets/Scenes/SampleScene.unity` — this is the main scene with all 10 UI panels.
5. **Generate Procedural Audio**: in the Unity menu bar, go to **Tools → Lab08 → Generate Procedural Audio Clips**. This creates all WAV files inside `Assets/Audio/`.
6. In the Hierarchy, select the **AudioManagers** GameObject and assign the generated clips to `UIAudioPlayer`, `MusicManager`, and `SoundEventListener` slots.
7. Press **Play** — the Title screen loads immediately with menu music.

> **Note on the Audio Mixer**: `Assets/Audio/Mixer/GameMixer.mixer` is included. Open it in Unity's Audio Mixer window to verify the Master → Music/SFX/UI hierarchy. The exposed parameters (`MasterVol`, `MusicVol`, `SFXVol`, `UIVol`) are already referenced by `AudioMixerController.cs`.

---

## 🔄 Loading Flow

```
Level Select → [GoTo "Loading"] → Loading Panel (AsyncSceneLoader)
    ├── SceneManager.LoadSceneAsync("GameplayScene", Additive)
    ├── Displays real progress (0–90%) + minimum 1.2s display time
    ├── At 100%, triggers MusicManager.PlayGameplay() (music crossfade)
    └── Calls ScreenManager.GoTo("Gameplay") to enter gameplay
```

The player **cannot** enter gameplay until the async operation allows scene activation (`operation.allowSceneActivation = true`). A simulated floor time of 1.2 s ensures the loading bar is always visible.

---

## 🎚️ Audio Mixer Groups

| Group | Routes | Exposed Param |
|-------|--------|---------------|
| **Master** | All audio | `MasterVol` |
| **Music** | `MusicManager` A/B sources | `MusicVol` |
| **SFX** | `SoundEventListener` AudioSource | `SFXVol` |
| **UI** | `UIAudioPlayer` AudioSource | `UIVol` |

---

## 🔊 Volume Mapping (Log–dB)

Slider value `v ∈ [0, 1]` → dB using:

```
db = max(log10(max(v, 0.0001)) × 20, -80)
```

Going back:

```
linear = 10^(db / 20)
```

Mute floor: `−80 dB` (not `−∞`) avoids `log(0)` errors. Settings are saved with `PlayerPrefs` and loaded in `Awake()` on every play session:

```csharp
// Save
PlayerPrefs.SetFloat("vol_music", sliderValue);

// Load & apply
float saved = PlayerPrefs.GetFloat("vol_music", 1f);
mixer.SetFloat("MusicVol", Mathf.Max(Mathf.Log10(Mathf.Max(saved, 0.0001f)) * 20f, -80f));
```

---

## 🎵 Music Transitions

| Trigger | Action |
|---------|--------|
| Game starts (Title/MainMenu) | `MusicManager.PlayMenu()` |
| Level → Loading → Gameplay | `MusicManager.PlayGameplay()` via `AsyncSceneLoader` |
| OnGameWon / OnGameLost | `MusicManager.PlayResults()` via `SoundEventListener` |
| Results/Credits → MainMenu | `MusicManager.PlayMenu()` via `ScreenManager` |

**Crossfade**: Two `AudioSource` components (A/B ping-pong) crossfade over **1.5 seconds** using a coroutine. While paused (`Time.timeScale = 0`), `ignoreListenerPause` is `false` on music sources so music stops when the game is paused — this is intentional.

---

## 📡 C# Event System

### Publisher: `GameEventBus.cs`

```csharp
// Declaration (static event — code-driven, not UnityEvent)
public static event System.Action<int> OnDamaged;
public static event System.Action      OnDied;
public static event System.Action<int> OnCollected;
public static event System.Action      OnGameWon;
public static event System.Action      OnGameLost;

// Safe invocation (publisher side — in PlayerController.cs)
GameEventBus.RaiseDamaged(1);   // ?.Invoke() pattern
GameEventBus.RaiseCollected(10);
GameEventBus.RaiseGameWon();
```

### Subscriber Pattern

```csharp
// Subscribe in OnEnable, unsubscribe in OnDisable
void OnEnable()  { GameEventBus.OnDamaged += HandleDamaged; }
void OnDisable() { GameEventBus.OnDamaged -= HandleDamaged; }
```

### Three Independent Listeners

| Class | Subscribes To | Response |
|-------|--------------|----------|
| `HUDListener.cs` | `OnCollected`, `OnDamaged`, `OnHealed`, `OnGameWon`, `OnGameLost` | Updates score TMP_Text and HP Slider |
| `SoundEventListener.cs` | `OnCollected`, `OnDamaged`, `OnDied`, `OnGameWon`, `OnGameLost` | Plays matching SFX via `AudioSource.PlayOneShot` |
| `ScreenFlashListener.cs` | `OnDamaged`, `OnCollected`, `OnGameWon`, `OnGameLost` | Coroutine color flash on full-screen overlay Image |

---

## 🔇 Audio Behaviour While Paused

| Source | `ignoreListenerPause` | Behaviour during pause |
|--------|----------------------|------------------------|
| Music (MusicManager) | `false` | **Stops** when paused — game silence = dramatic pause feel |
| SFX (SoundEventListener) | `false` | Stops with `timeScale = 0` — no events fire anyway |
| UI (UIAudioPlayer) | uses `PlayOneShot` | Plays normally — panel open/close sounds are heard |

---

## 🗂️ New Scripts Summary

```
Assets/Scripts/
├── ScreenManager.cs         [MODIFIED] — added UI audio + music triggers
├── ButtonFeedback.cs        [MODIFIED] — added hover/press sounds
├── PauseTimeScale.cs        [MODIFIED] — added pause/resume sounds
└── Lab08/
    ├── GameEventBus.cs           Static C# event hub (publisher)
    ├── PlayerController.cs       Gameplay controller (publisher)
    ├── HUDListener.cs            Listener #1: HUD updates
    ├── SoundEventListener.cs     Listener #2: Gameplay SFX
    ├── ScreenFlashListener.cs    Listener #3: Screen flash
    ├── UIAudioPlayer.cs          Singleton UI audio manager
    ├── MusicManager.cs           Singleton music crossfader
    ├── AsyncSceneLoader.cs       Async loading + progress bar
    ├── AudioMixerController.cs   Volume controls + PlayerPrefs
    ├── VolumeSliderInit.cs       Slider ↔ AudioMixer wiring
    └── Editor/
        └── ProceduralAudioGenerator.cs  Generates all WAV clips
```

---

## 🎵 Asset Credits

All audio in this project is **procedurally generated** in C# at editor time using sine-wave synthesis and white noise. No third-party audio files were used. The generator code is at `Assets/Scripts/Lab08/Editor/ProceduralAudioGenerator.cs`.

The synthesis method produces:
- **UI clicks/hovers**: exponentially-decaying sine waves
- **SFX collect**: ascending frequency glide ("ding")
- **SFX damage**: noise burst (white noise × decay envelope)
- **SFX die**: descending frequency sweep
- **SFX win/lose**: multi-note arpeggios (C Major / descending minor)
- **Music loops**: sustained chord drones with amplitude modulation (4-second looping WAV)

**No copyright issues** — all audio is original, synthesised code output.

Fonts: LiberationSans (included with Unity TMP) — OFL licence (see `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`).

---

## 📋 Unity Version

**Unity 6.3 LTS (6000.3.24f1)** · Universal Render Pipeline 17.3.0

---

