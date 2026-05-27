using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnEnable()
    {
        if (AudioManager.Instance != null && slider != null)
            slider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);
    }

    void OnValueChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
    }
}
