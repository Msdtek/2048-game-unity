using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public Slider masterSlider;
    public Toggle masterMuteToggle;
    public Slider musicSlider;
    public Toggle musicMuteToggle;

    const string PREF_MASTER_VOL = "masterVolume";
    const string PREF_MUSIC_VOL = "musicVolume";
    const string PREF_MASTER_MUTE = "masterMuted";
    const string PREF_MUSIC_MUTE = "musicMuted";

    private float _savedMaster = 1f;
    private float _savedMusic = 1f;
    private bool _isInitializing = false;

    void Awake()
    {
        _isInitializing = true;

        // Kayitlardan yükle
        _savedMaster = PlayerPrefs.GetFloat(PREF_MASTER_VOL, 1f);
        _savedMusic = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 1f);
        bool masterMuted = PlayerPrefs.GetInt(PREF_MASTER_MUTE, 0) == 1;
        bool musicMuted = PlayerPrefs.GetInt(PREF_MUSIC_MUTE, 0) == 1;

        // Olaylarý tetiklemeden UI deðerlerini ayarla
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(masterMuted ? 0f : _savedMaster);
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(musicMuted ? 0f : _savedMusic);

        // Toggle'larý olay tetiklemeden ayarla (Unity UI Toggle API)
        if (masterMuteToggle != null) masterMuteToggle.SetIsOnWithoutNotify(masterMuted);
        if (musicMuteToggle != null) musicMuteToggle.SetIsOnWithoutNotify(musicMuted);

        ApplyMasterVolume(masterMuted ? 0f : _savedMaster);
        ApplyMusicVolume(musicMuted ? 0f : _savedMusic);

        _isInitializing = false;
    }

    // UI'dan baðlanacaklar
    public void OnMasterVolumeChanged(float value)
    {
        if (_isInitializing) return;

        _savedMaster = value;
        PlayerPrefs.SetFloat(PREF_MASTER_VOL, value);
        PlayerPrefs.SetInt(PREF_MASTER_MUTE, value <= 0f ? 1 : 0);
        ApplyMasterVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (_isInitializing) return;

        _savedMusic = value;
        PlayerPrefs.SetFloat(PREF_MUSIC_VOL, value);
        PlayerPrefs.SetInt(PREF_MUSIC_MUTE, value <= 0f ? 1 : 0);
        ApplyMusicVolume(value);
    }

    public void OnMasterMuteToggle(bool muted)
    {
        if (_isInitializing) return;

        PlayerPrefs.SetInt(PREF_MASTER_MUTE, muted ? 1 : 0);
        if (masterSlider != null) masterSlider.interactable = !muted;
        ApplyMasterVolume(muted ? 0f : _savedMaster);

        // Eðer mute toggle kullanýcý tarafýndan deðiþtiyse slider'ý güncelle (notify olmasýn)
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(muted ? 0f : _savedMaster);
    }

    public void OnMusicMuteToggle(bool muted)
    {
        if (_isInitializing) return;

        PlayerPrefs.SetInt(PREF_MUSIC_MUTE, muted ? 1 : 0);
        if (musicSlider != null) musicSlider.interactable = !muted;
        ApplyMusicVolume(muted ? 0f : _savedMusic);

        if (musicSlider != null) musicSlider.SetValueWithoutNotify(muted ? 0f : _savedMusic);
    }

    private void ApplyMasterVolume(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
    }

    private void ApplyMusicVolume(float v)
    {
        if (BackgroundMusic.Instance != null)
        {
            BackgroundMusic.Instance.SetVolume(Mathf.Clamp01(v));
        }
    }

    // Ýsteðe baðlý: ayarlarý sýfýrla butonuna baðlayabilirsiniz
    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(PREF_MASTER_VOL);
        PlayerPrefs.DeleteKey(PREF_MUSIC_VOL);
        PlayerPrefs.DeleteKey(PREF_MASTER_MUTE);
        PlayerPrefs.DeleteKey(PREF_MUSIC_MUTE);
        Awake();
    }
}