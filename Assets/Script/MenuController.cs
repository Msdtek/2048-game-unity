using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    public string gameSceneName = "GameScene"; // Build Settings'teki oyun sahne adý
    public string mainMenuSceneName = "MainMenu";

    [Header("Settings")]
    public GameObject settingsPanel;        // Eðer ayný sahnede bir panel kullanacaksan buraya ata
    public string settingsSceneName = "";   // Veya ayrý bir ayarlar sahnesi adý gir

    [Header("UI")]
    public TextMeshProUGUI bestScoreText; // Ana menüde gösterilecek en iyi skor metni

    void Start()
    {
        // Ana menü açýldýðýnda best skoru göster
        if (bestScoreText != null)
        {
            bestScoreText.text = "BEST: " + ScoreManager.GetBestScore().ToString();
        }
    }

    public void StartGame()
    {
        if (!IsSceneInBuild(gameSceneName))
        {
            Debug.LogError($"Scene '{gameSceneName}' build profilinde yok. File -> Build Settings -> \"Scenes In Build\" bölümüne sahneyi ekleyin.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // NOT: StartGameAsync hâlâ mevcut ama Settings butonu için OpenSettings kullan
    public void StartGameAsync()
    {
        if (!IsSceneInBuild(gameSceneName))
        {
            Debug.LogError($"Scene '{gameSceneName}' build profilinde yok. File -> Build Settings -> \"Scenes In Build\" bölümüne sahneyi ekleyin.");
            return;
        }

        StartCoroutine(LoadAsync(gameSceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        if (op == null)
        {
            Debug.LogError($"LoadSceneAsync returned null for scene '{sceneName}'. Sahnenin Build Settings'e eklendiðinden emin olun.");
            yield break;
        }

        while (!op.isDone)
        {
            // op.progress (0..0.9) ve op.allowSceneActivation kontrolü ile loading UI gösterilebilir
            yield return null;
        }
    }

    // --- Yeni: Ayarlar aç / kapat ---
    public void OpenSettings()
    {
        // Öncelik: settingsPanel varsa onu aç
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            return;
        }

        // Yoksa ayrý sahneyi yükle (Build Settings kontrolü)
        if (!string.IsNullOrEmpty(settingsSceneName))
        {
            if (!IsSceneInBuild(settingsSceneName))
            {
                Debug.LogError($"Scene '{settingsSceneName}' build profilinde yok. File -> Build Settings -> \"Scenes In Build\" bölümüne sahneyi ekleyin.");
                return;
            }

            SceneManager.LoadScene(settingsSceneName);
            return;
        }

        Debug.LogWarning("OpenSettings çaðrýldý ama ne settingsPanel atanmýþ ne de settingsSceneName verilmiþ.");
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            return;
        }

        // Eðer settings ayrý sahne ise geri dönmek istediðinde MainMenu sahnesini yükleyebilirsin
        if (!string.IsNullOrEmpty(mainMenuSceneName) && IsSceneInBuild(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }
    }

    public void BackToMenu()
    {
        if (!IsSceneInBuild(mainMenuSceneName))
        {
            Debug.LogError($"Scene '{mainMenuSceneName}' build profilinde yok.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Build Settings içinde sahne var mý diye kontrol eder (dosya adýndan karþýlaþtýrma)
    private bool IsSceneInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(path)) continue;
            if (Path.GetFileNameWithoutExtension(path) == sceneName) return true;
        }
        return false;
    }
}