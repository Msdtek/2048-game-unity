using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    public string localizationKey;
    TextMeshProUGUI _tmp;

    void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        UpdateText();
        if (LocalizationManager.Instance != null) LocalizationManager.Instance.OnLanguageChanged += UpdateText;
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null) LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    public void UpdateText()
    {
        if (_tmp == null) return;
        if (LocalizationManager.Instance == null) { _tmp.text = localizationKey; return; }
        _tmp.text = LocalizationManager.Instance.Get(localizationKey);
    }
}