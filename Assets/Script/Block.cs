using UnityEngine;
using TMPro;
using System.Collections;

public class Block : MonoBehaviour
{
    public int Value { get; private set; }
    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Shadow")]
    [Tooltip("Gölgeyi devre dýþý býrakmak için kapatýn")]
    [SerializeField] private bool enableShadow = false;
    [SerializeField] private float shadowAlpha = 0.45f;
    [SerializeField] private Vector2 shadowOffset = new Vector2(0.08f, -0.08f);
    [SerializeField] private float shadowScale = 1.05f;
    [SerializeField] private int shadowOrderOffset = -1;

    private SpriteRenderer _shadowRenderer;

    public void Init(int newValue, Color color)
    {
        Value = newValue;
        if (valueText != null) valueText.text = Value.ToString();

        if (spriteRenderer != null)
        {
            color.a = 1f;
            spriteRenderer.sprite = spriteRenderer.sprite; // sprite korunur
            spriteRenderer.color = color;
        }

        RefreshShadow();
    }

    public void Init(int newValue, Sprite sprite)
    {
        Value = newValue;
        if (valueText != null) valueText.text = Value.ToString();

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
        }

        RefreshShadow();
    }

    // Smooth move (ease)
    public IEnumerator MoveToTarget(Vector3 target, float speed)
    {
        if (this == null) yield break;

        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, target);
        if (distance <= 0.001f)
        {
            transform.position = target;
            yield break;
        }

        float duration = Mathf.Max(0.01f, distance / Mathf.Max(0.0001f, speed));
        float elapsed = 0f;

        while (this != null && elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, duration > 0f ? elapsed / duration : 1f);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        if (this != null)
        {
            transform.position = target;
        }
    }

    // Yeni: Basit ve tok düþme animasyonu (gölge yok / bounce yok)
    public IEnumerator FallToTargetSimple(Vector3 target, float duration = 0.28f, float squashAmount = 0.14f)
    {
        if (this == null) yield break;

        Vector3 start = transform.position;
        float elapsed = 0f;
        float d = Mathf.Max(0.01f, duration);

        // Reset scale baþlangýç
        Vector3 originalScale = transform.localScale;

        // Ease-in (hýzlanarak düþüþ)
        while (this != null && elapsed < d)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / d);
            float ease = t * t * t; // ease-in cubic for natural acceleration

            transform.position = Vector3.Lerp(start, target, ease);

            // Hafif squash sadece iniþin son kýsmýnda
            float squashT = Mathf.Clamp01((t - 0.7f) / 0.3f); // squash sadece son %30'da görünür
            float sx = 1f + squashAmount * squashT;
            float sy = 1f - squashAmount * squashT;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(sx, sy, 1f), 0.6f);

            yield return null;
        }

        if (this == null) yield break;

        // Tam hedefe yerleþtir ve scale'i eski haline döndür
        transform.position = target;
        transform.localScale = originalScale;
    }

    // Pop / squash-stretch animasyonu
    public IEnumerator PopAnimation(float scaleMultiplier = 1.2f, float totalDuration = 0.1f)
    {
        if (this == null) yield break;
        Transform t = transform;
        Vector3 original = t.localScale;
        if (totalDuration <= 0f)
        {
            t.localScale = original;
            yield break;
        }

        float half = totalDuration * 0.5f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, elapsed / half));
            t.localScale = Vector3.Lerp(original, original * scaleMultiplier, k);
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, elapsed / half));
            t.localScale = Vector3.Lerp(original * scaleMultiplier, original, k);
            yield return null;
        }

        t.localScale = original;
    }

    // Shadow güncelleme / oluþturma — gölge devre dýþýysa hiç oluþturmaz
    private void RefreshShadow()
    {
        if (!enableShadow)
        {
            if (_shadowRenderer != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(_shadowRenderer.gameObject);
                else
#endif
                    Destroy(_shadowRenderer.gameObject);
                _shadowRenderer = null;
            }
            return;
        }

        if (spriteRenderer == null) return;
        Sprite sprite = spriteRenderer.sprite;
        if (sprite == null)
        {
            if (_shadowRenderer != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(_shadowRenderer.gameObject);
                else
#endif
                    Destroy(_shadowRenderer.gameObject);
                _shadowRenderer = null;
            }
            return;
        }

        if (_shadowRenderer == null)
        {
            GameObject g = new GameObject("Shadow");
            g.transform.SetParent(transform, false);
            _shadowRenderer = g.AddComponent<SpriteRenderer>();
            _shadowRenderer.transform.localRotation = Quaternion.identity;
            _shadowRenderer.transform.localScale = Vector3.one;
        }

        _shadowRenderer.sprite = sprite;
        Color sc = Color.black;
        sc.a = shadowAlpha;
        _shadowRenderer.color = sc;

        if (spriteRenderer != null)
        {
            _shadowRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            _shadowRenderer.sortingOrder = spriteRenderer.sortingOrder + shadowOrderOffset;
        }

        _shadowRenderer.transform.localPosition = new Vector3(shadowOffset.x, shadowOffset.y, 0f);
        _shadowRenderer.transform.localScale = new Vector3(shadowScale, shadowScale, 1f);
        _shadowRenderer.maskInteraction = SpriteMaskInteraction.None;
    }
}