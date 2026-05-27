using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class HomeUI : MonoBehaviour
{
    [Header("Scene")]
    public string startSceneName = "Opening Scene";

    [Header("Canvas")]
    public GameObject settingsCanvas;
    public GameObject creditsCanvas;

    [Header("Fade")]
    public FadeUI fadeUI;

    void Start()
    {
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        if (creditsCanvas != null) creditsCanvas.SetActive(false);
    }

    // 開始
    public void StartGame()
    {
        Time.timeScale = 1f;

        if (fadeUI != null)
            StartCoroutine(FadeThenLoad());
        else
            SceneManager.LoadScene(startSceneName);
    }

    private IEnumerator FadeThenLoad()
    {
        fadeUI.FadeOut();
        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutMusic(fadeUI.fadeDuration);
        yield return new WaitForSeconds(fadeUI.fadeDuration);
        SceneManager.LoadScene(startSceneName);
    }

    // 設定
    public void OpenSettings() => Open(settingsCanvas);
    public void CloseSettings() => Close(settingsCanvas);

    // 製作團隊
    public void OpenCredits() => Open(creditsCanvas);
    public void CloseCredits() => Close(creditsCanvas);

    // 退出
    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void Open(GameObject canvas)
    {
        if (canvas == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        canvas.SetActive(true);
    }

    private void Close(GameObject canvas)
    {
        if (canvas == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        canvas.SetActive(false);
    }
}
