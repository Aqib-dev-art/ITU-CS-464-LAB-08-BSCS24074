using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility: generates procedural CC0 audio clips for Lab 08.
/// Run from: Tools > Lab08 > Generate Procedural Audio
/// All clips are synthesised in code — no third-party files needed.
/// </summary>
public class ProceduralAudioGenerator : UnityEditor.Editor
{
    const int SampleRate = 44100;
    const string OutputPath = "Assets/Audio";

    [MenuItem("Tools/Lab08/Generate Procedural Audio Clips")]
    static void GenerateAll()
    {
        Directory.CreateDirectory(Application.dataPath + "/Audio/SFX");
        Directory.CreateDirectory(Application.dataPath + "/Audio/Music");

        SaveClip(MakeClick(),       "SFX/ui_click");
        SaveClip(MakeHover(),       "SFX/ui_hover");
        SaveClip(MakeBack(),        "SFX/ui_back");
        SaveClip(MakePanelOpen(),   "SFX/ui_panel_open");
        SaveClip(MakePanelClose(),  "SFX/ui_panel_close");
        SaveClip(MakeCollect(),     "SFX/sfx_collect");
        SaveClip(MakeDamage(),      "SFX/sfx_damage");
        SaveClip(MakeDie(),         "SFX/sfx_die");
        SaveClip(MakeWin(),         "SFX/sfx_win");
        SaveClip(MakeLose(),        "SFX/sfx_lose");
        SaveClip(MakeMenuMusic(),   "Music/music_menu");
        SaveClip(MakeGameMusic(),   "Music/music_gameplay");
        SaveClip(MakeResultMusic(), "Music/music_results");

        AssetDatabase.Refresh();
        Debug.Log("[Lab08] Procedural audio clips generated in Assets/Audio/");
    }

    // ─────────────────────── UI SFX ───────────────────────────────────────────

    static AudioClip MakeClick()
    {
        int len = SampleRate / 10; // 0.1s
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Exp(-t * 30f);
            s[i] = env * Mathf.Sin(2f * Mathf.PI * 1000f * t) * 0.6f;
        }
        return Build("ui_click", s, len);
    }

    static AudioClip MakeHover()
    {
        int len = SampleRate / 20; // 0.05s
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Exp(-t * 50f);
            s[i] = env * Mathf.Sin(2f * Mathf.PI * 1200f * t) * 0.3f;
        }
        return Build("ui_hover", s, len);
    }

    static AudioClip MakeBack()
    {
        int len = SampleRate / 8;
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Exp(-t * 15f);
            float freq = Mathf.Lerp(800f, 400f, t * 8f); // descending
            s[i] = env * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f;
        }
        return Build("ui_back", s, len);
    }

    static AudioClip MakePanelOpen()
    {
        int len = SampleRate / 5;
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Exp(-t * 10f);
            float freq = Mathf.Lerp(400f, 900f, t * 5f); // ascending sweep
            s[i] = env * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f;
        }
        return Build("ui_panel_open", s, len);
    }

    static AudioClip MakePanelClose()
    {
        int len = SampleRate / 5;
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Exp(-t * 10f);
            float freq = Mathf.Lerp(900f, 400f, t * 5f); // descending sweep
            s[i] = env * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f;
        }
        return Build("ui_panel_close", s, len);
    }

    // ─────────────────────── Gameplay SFX ────────────────────────────────────

    static AudioClip MakeCollect()
    {
        int len = SampleRate / 4;
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Exp(-t * 12f);
            float freq = 880f + 440f * Mathf.Clamp01(t * 8f); // ding up
            s[i] = env * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f;
        }
        return Build("sfx_collect", s, len);
    }

    static AudioClip MakeDamage()
    {
        int len = SampleRate / 4;
        float[] s = new float[len];
        System.Random rng = new System.Random(42);
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Exp(-t * 15f);
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            s[i] = env * noise * 0.5f; // noise burst = impact
        }
        return Build("sfx_damage", s, len);
    }

    static AudioClip MakeDie()
    {
        int len = SampleRate / 2;
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Exp(-t * 5f);
            float freq = Mathf.Lerp(500f, 100f, t * 2f); // descending low tone
            s[i] = env * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f;
        }
        return Build("sfx_die", s, len);
    }

    static AudioClip MakeWin()
    {
        int len = SampleRate;
        float[] s = new float[len];
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5-E5-G5-C6 fanfare
        int noteLen = len / notes.Length;
        for (int n = 0; n < notes.Length; n++)
        {
            for (int i = 0; i < noteLen; i++)
            {
                float t = (float)i / SampleRate;
                float env = (1f - (float)i / noteLen) * 0.6f;
                s[n * noteLen + i] = env * Mathf.Sin(2f * Mathf.PI * notes[n] * t);
            }
        }
        return Build("sfx_win", s, len);
    }

    static AudioClip MakeLose()
    {
        int len = SampleRate;
        float[] s = new float[len];
        float[] notes = { 392f, 349.23f, 329.63f, 261.63f }; // G4-F4-E4-C4 descending
        int noteLen = len / notes.Length;
        for (int n = 0; n < notes.Length; n++)
        {
            for (int i = 0; i < noteLen; i++)
            {
                float t = (float)i / SampleRate;
                float env = (1f - (float)i / noteLen) * 0.5f;
                s[n * noteLen + i] = env * Mathf.Sin(2f * Mathf.PI * notes[n] * t);
            }
        }
        return Build("sfx_lose", s, len);
    }

    // ─────────────────────── Music (looping procedural) ───────────────────────

    static AudioClip MakeMenuMusic()
    {
        int len = SampleRate * 4; // 4 second loop
        float[] s = new float[len];
        float[] chord = { 261.63f, 329.63f, 392f }; // C major
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float beat = Mathf.Sin(2f * Mathf.PI * 2f * t); // 2 Hz tremolo
            float env = 0.15f + 0.05f * beat;
            foreach (float f in chord)
                s[i] += env * Mathf.Sin(2f * Mathf.PI * f * t) / chord.Length;
        }
        return Build("music_menu", s, len);
    }

    static AudioClip MakeGameMusic()
    {
        int len = SampleRate * 4;
        float[] s = new float[len];
        float[] chord = { 349.23f, 440f, 523.25f }; // F major (brighter)
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float beat = Mathf.Abs(Mathf.Sin(2f * Mathf.PI * 4f * t)); // 4 Hz pulse
            float env = 0.1f + 0.1f * beat;
            foreach (float f in chord)
                s[i] += env * Mathf.Sin(2f * Mathf.PI * f * t) / chord.Length;
        }
        return Build("music_gameplay", s, len);
    }

    static AudioClip MakeResultMusic()
    {
        int len = SampleRate * 4;
        float[] s = new float[len];
        float[] chord = { 523.25f, 659.25f, 783.99f }; // C5 major triumphant
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float env = 0.12f;
            foreach (float f in chord)
                s[i] += env * Mathf.Sin(2f * Mathf.PI * f * t) / chord.Length;
        }
        return Build("music_results", s, len);
    }

    // ─────────────────────── Helpers ─────────────────────────────────────────

    static AudioClip Build(string name, float[] samples, int length)
    {
        var clip = AudioClip.Create(name, length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static void SaveClip(AudioClip clip, string relPath)
    {
        string fullPath = Application.dataPath + "/Audio/" + relPath + ".wav";
        WavUtility.Save(fullPath, clip);
        Object.DestroyImmediate(clip);
    }
}

/// <summary>Minimal WAV file writer (PCM 16-bit mono).</summary>
public static class WavUtility
{
    public static void Save(string path, AudioClip clip)
    {
        float[] data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);

        using var fs = new FileStream(path, FileMode.Create);
        using var bw = new System.IO.BinaryWriter(fs);

        int sampleRate    = clip.frequency;
        int channels      = clip.channels;
        int sampleCount   = data.Length;
        int byteRate      = sampleRate * channels * 2;
        int dataChunkSize = sampleCount * 2;

        // RIFF header
        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + dataChunkSize);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        // fmt chunk
        bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);          // chunk size
        bw.Write((short)1);    // PCM
        bw.Write((short)channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write((short)(channels * 2));
        bw.Write((short)16);   // bits per sample
        // data chunk
        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(dataChunkSize);
        foreach (float f in data)
        {
            short sample = (short)Mathf.Clamp(Mathf.RoundToInt(f * 32767f), -32768, 32767);
            bw.Write(sample);
        }
    }
}
