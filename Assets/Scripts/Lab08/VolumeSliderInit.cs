using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a Settings volume Slider.
/// Loads the saved value on Awake and wires OnValueChanged to AudioMixerController.
/// </summary>
public enum VolumeGroup { Master, Music, SFX, UI }

public class VolumeSliderInit : MonoBehaviour
{
    public VolumeGroup group;
    public AudioMixerController mixerController;

    Slider _slider;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        if (!_slider || !mixerController) return;

        // Load saved value and set slider without triggering OnValueChanged
        float saved = group switch
        {
            VolumeGroup.Master => mixerController.GetSavedMaster(),
            VolumeGroup.Music  => mixerController.GetSavedMusic(),
            VolumeGroup.SFX    => mixerController.GetSavedSFX(),
            VolumeGroup.UI     => mixerController.GetSavedUI(),
            _                  => 1f
        };
        _slider.SetValueWithoutNotify(saved);

        // Wire up change callback
        _slider.onValueChanged.AddListener(OnChanged);
    }

    void OnChanged(float value)
    {
        switch (group)
        {
            case VolumeGroup.Master: mixerController.SetMaster(value); break;
            case VolumeGroup.Music:  mixerController.SetMusic(value);  break;
            case VolumeGroup.SFX:    mixerController.SetSFX(value);    break;
            case VolumeGroup.UI:     mixerController.SetUI(value);     break;
        }
    }
}
