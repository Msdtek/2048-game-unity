using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
class LocalizationEntry
{
    public string key;
    public string value;
}

[Serializable]
class LocalizationData
{
    public LocalizationEntry[] entries;
}

public class LocalizationManager : MonoBehaviour
{
    const string PREF_KEY_LANG = "language";
    public static LocalizationManager Instance { get; private set; }

    public string defaultLanguage = "en";
    Dictionary<string, string> _table = new Dictionary<string, string>();
    public string CurrentLanguage { get; private set; }

    public event Action OnLanguageChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string lang = PlayerPrefs.GetString(PREF_KEY_LANG, defaultLanguage);
        LoadLanguage(lang);
    }

    public void LoadLanguage(string langCode)
    {
        if (string.IsNullOrWhiteSpace(langCode)) langCode = defaultLanguage;
        TextAsset ta = Resources.Load<TextAsset>($"Localization/{langCode}");
        if (ta == null)
        {
            Debug.LogWarning($"Localization file not found: Resources/Localization/{langCode}.json - falling back to {defaultLanguage}");
            ta = Resources.Load<TextAsset>($"Localization/{defaultLanguage}");
            if (ta == null) { _table.Clear(); CurrentLanguage = defaultLanguage; OnLanguageChanged?.Invoke(); return; }
            langCode = defaultLanguage;
        }

        var data = JsonUtility.FromJson<LocalizationData>(ta.text);
        _table.Clear();
        if (data?.entries != null)
        {
            foreach (var e in data.entries)
            {
                if (string.IsNullOrEmpty(e?.key)) continue;
                _table[e.key] = e.value ?? string.Empty;
            }
        }

        CurrentLanguage = langCode;
        PlayerPrefs.SetString(PREF_KEY_LANG, langCode);
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke();
    }

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        return _table.TryGetValue(key, out var v) ? v : key;
    }

    // Helper to switch language at runtime
    public void SetLanguage(string langCode) => LoadLanguage(langCode);
}