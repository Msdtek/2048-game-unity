using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText; // Canvas içindeki skor metni
    private int _currentScore = 0;

    const string PREF_KEY_BEST_SCORE = "BestScore";

    [Header("Popup")]
    public GameObject floatingTextPrefab; // Opsiyonel: hazýr prefab (TextMeshProUGUI içermeli). Yoksa runtime oluþturulacak.
    public Color popupColor = new Color(1f, 0.9f, 0.5f, 1f);
    public float popupDuration = 0.9f;
    public float popupRiseDistance = 60f; // UI biriminde (anchoredPosition)
    public float popupFontSizeMultiplier = 1.0f;

    [Header("Score Pulse")]
    public float pulseScale = 1.15f;   // ne kadar büyüsün
    public float pulseDuration = 0.25f; // toplam süre

    // Pulse coroutine referansý — çakýþmalarý önlemek için
    Coroutine _pulseCoroutine;

    // YAZININ ORÝJÝNAL BOYUTUNU TUTACAÐIMIZ DEÐÝÞKEN (EKLENDÝ)
    private Vector3 _baseScale;

    void Awake()
    {
        if (scoreText != null)
        {
            // Yazýnýn en baþtaki orijinal boyutunu kaydediyoruz (EKLENDÝ)
            _baseScale = scoreText.rectTransform.localScale;

            scoreText.text = "SCORE: " + _currentScore.ToString();
        }
    }

    public void AddScore(int amount)
    {
        if (scoreText != null)
        {
            _currentScore += amount;
            scoreText.text = "SCORE: " + _currentScore.ToString();

            // Best score kontrolü ve kaydý
            int best = GetBestScore();
            if (_currentScore > best)
            {
                PlayerPrefs.SetInt(PREF_KEY_BEST_SCORE, _currentScore);
                PlayerPrefs.Save();
            }

            // Önceki pulse'u güvenli þekilde durdur
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
            _pulseCoroutine = StartCoroutine(PulseCoroutine());

            // Floating popup baþlat
            StartCoroutine(SpawnFloatingText(amount));
        }
        else
        {
            _currentScore += amount;
            int best = GetBestScore();
            if (_currentScore > best)
            {
                PlayerPrefs.SetInt(PREF_KEY_BEST_SCORE, _currentScore);
                PlayerPrefs.Save();
            }
        }
    }

    public static int GetBestScore()
    {
        return PlayerPrefs.GetInt(PREF_KEY_BEST_SCORE, 0);
    }

    private IEnumerator PulseCoroutine()
    {
        RectTransform rt = scoreText.rectTransform;

        // ANLIK BOYUTU DEÐÝL, EN BAÞTA KAYDETTÝÐÝMÝZ SABÝT BOYUTU KULLANIYORUZ (DEÐÝÞTÝRÝLDÝ)
        Vector3 original = _baseScale;

        rt.localScale = original;
        Vector3 target = original * pulseScale;

        float half = pulseDuration * 0.5f;
        float t = 0f;

        // Büyü
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, t / half));
            rt.localScale = Vector3.Lerp(original, target, k);
            yield return null;
        }

        // Küçül
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, t / half));
            rt.localScale = Vector3.Lerp(target, original, k);
            yield return null;
        }

        rt.localScale = original;
        _pulseCoroutine = null;
    }

    private IEnumerator SpawnFloatingText(int amount)
    {
        if (scoreText == null) yield break;

        // Create or instantiate floating text UI element
        TextMeshProUGUI floating = null;
        GameObject go = null;

        if (floatingTextPrefab != null)
        {
            go = Instantiate(floatingTextPrefab, scoreText.rectTransform.parent);
            go.transform.SetAsLastSibling();
            floating = go.GetComponent<TextMeshProUGUI>();
            if (floating == null)
            {
                // Eðer prefab TextMeshProUGUI içermiyorsa, bir TMP component ekle
                floating = go.AddComponent<TextMeshProUGUI>();
            }
        }
        else
        {
            go = new GameObject("FloatingScore");
            go.transform.SetParent(scoreText.rectTransform.parent, false);
            floating = go.AddComponent<TextMeshProUGUI>();
            // Basit default düzenlemeler
            floating.raycastTarget = false;
            floating.fontStyle = FontStyles.Bold;
            floating.alignment = TextAlignmentOptions.Center;
        }

        // Metin ve stil
        floating.text = "+" + amount.ToString();
        floating.color = popupColor;
        floating.fontSize = scoreText.fontSize * popupFontSizeMultiplier;
        floating.alpha = 1f;

        RectTransform floatingRt = floating.rectTransform;
        // Baþlangýç pozisyonu: scoreText'in üstü
        Vector2 startPos = scoreText.rectTransform.anchoredPosition;
        // Hafif sað/sol kaydýrma rastgeleleþtir
        startPos += new Vector2(Random.Range(-6f, 6f), 12f);
        // Clamp: parent sýnýrlarý içinde kalmasýný saðla
        ClampToParent(floatingRt, ref startPos);
        floatingRt.anchoredPosition = startPos;

        float elapsed = 0f;
        Vector2 endPos = startPos + new Vector2(0f, popupRiseDistance);
        // End pozisyonunu de clamp et (parent sýnýrlarýný aþmasýn)
        ClampToParent(floatingRt, ref endPos);

        // Ease-out hareket ve fade out
        while (elapsed < popupDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / popupDuration);
            float ease = 1f - Mathf.Pow(1f - p, 3f); // ease-out cubic
            floatingRt.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
            float a = Mathf.Lerp(1f, 0f, p);
            Color c = floating.color;
            c.a = a;
            floating.color = c;
            yield return null;
        }

        // Temizle
        if (go != null)
        {
            Destroy(go);
        }
    }

    // Basit clamp: child'ýn anchoredPosition'ýný parent rect sýnýrlarý içinde tutmaya çalýþýr.
    private void ClampToParent(RectTransform child, ref Vector2 anchoredPos)
    {
        var parent = child.parent as RectTransform;
        if (parent == null) return;

        // parent Rect köþeleri (local-space)
        Rect parentRect = parent.rect;

        // child pivot ve size kullanarak görünür aralýðý hesapla
        Vector2 childSize = child.sizeDelta;
        Vector2 min = parentRect.min + (childSize * child.pivot);
        Vector2 max = parentRect.max - (childSize * (Vector2.one - child.pivot));

        anchoredPos.x = Mathf.Clamp(anchoredPos.x, min.x, max.x);
        anchoredPos.y = Mathf.Clamp(anchoredPos.y, min.y, max.y);
    }
}