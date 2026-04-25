csharp Assets\Script\BackgroundMusic.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Audio")]
    public AudioClip musicClip;
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool loop = true;
    public bool playOnStart = true;

    private AudioSource _source;
    private Coroutine _fadeCoroutine;

    void Awake()
    {
        // Singleton: sahne deðiþse de bir tane kalsýn
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = loop;
        _source.spatialBlend = 0f; // 2D müzik
        _source.clip = musicClip;
        _source.volume = Mathf.Clamp01(volume);
    }

    void Start()
    {
        if (playOnStart && _source.clip != null)
            _source.Play();
    }

    // Basit kontrol API
    public void Play()
    {
        if (_source.clip == null) return;
        if (!_source.isPlaying) _source.Play();
    }

    public void Stop()
    {
        if (_source.isPlaying) _source.Stop();
    }

    public void Pause()
    {
        if (_source.isPlaying) _source.Pause();
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        _source.volume = volume;
    }

    // Fade yardýmcýlarý (koroutine kullan)
    public void FadeIn(float duration)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeCoroutine(0f, volume, duration, true));
    }

    public void FadeOut(float duration)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeCoroutine(_source.volume, 0f, duration, false));
    }

    private IEnumerator FadeCoroutine(float from, float to, float duration, bool ensurePlay)
    {
        if (ensurePlay && !_source.isPlaying) _source.Play();
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _source.volume = Mathf.Lerp(from, to, duration > 0f ? t / duration : 1f);
            yield return null;
        }
        _source.volume = to;
        if (to <= 0f) _source.Stop();
        _fadeCoroutine = null;
    }
}
}