using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SliderField : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueText;

    [System.Serializable]
    public class FloatEvent : UnityEngine.Events.UnityEvent<float> { }

    public FloatEvent onValueChanged;

    private void Awake()
    {
        if (slider != null)
            slider.onValueChanged.AddListener(InvokeEvent);
    }

    private void InvokeEvent(float value)
    {
        // Actualiza texto
        if (valueText != null)
            valueText.text = (value * 100f).ToString("0.0") + "%";

        // Llama al evento del inspector
        onValueChanged?.Invoke(value);
    }
}
