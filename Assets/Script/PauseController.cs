using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel; // Inspector'da ata (Panel kapalý baþlasýn)
    public Button resumeButton;   // Opsiyonel: Resume butonunu atayabilirsiniz
    public Button menuButton;     // Opsiyonel: Menu butonunu atayabilirsiniz

    [Header("Behavior")]
    public KeyCode toggleKey = KeyCode.Escape;
    public bool pauseTime = true; // true -> Time.timeScale = 0 ile durdur

    bool _isPaused;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        _isPaused = false;

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (menuButton != null) menuButton.onClick.AddListener(ReturnToMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;

        // UI
        if (pausePanel != null) pausePanel.SetActive(true);

        // Oyun zamaný durdur
        if (pauseTime) Time.timeScale = 0f;

        // Input'u durdur: InputHandler'ý devre dýþý býrak
        var ih = Object.FindFirstObjectByType<InputHandler>();
        if (ih != null)
        {
            ih.enabled = false;
        }

        // (Opsiyonel) oyun seslerini de durdurmak istersen:
        // AudioListener.pause = true;
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseTime) Time.timeScale = 1f;

        var ih = Object.FindFirstObjectByType<InputHandler>();
        if (ih != null) ih.enabled = true;

        // AudioListener.pause = false; // eðer pause sýrasýnda sesi durdurduysan geri aç
    }

    public void ReturnToMenu()
    {
        // Önce normal oyun durumuna al
        if (pauseTime) Time.timeScale = 1f;
        var ih = Object.FindFirstObjectByType<InputHandler>();
        if (ih != null) ih.enabled = true;

        // MenuController.BackToMenu kullan
        var mc = Object.FindFirstObjectByType<MenuController>();
        if (mc != null)
        {
            mc.BackToMenu();
            return;
        }

        // Eðer MenuController yoksa doðrudan sahne ismi ile yükle (varsayýlan "MainMenu")
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}