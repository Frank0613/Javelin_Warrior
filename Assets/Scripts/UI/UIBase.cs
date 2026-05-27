using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public abstract class UIBase : MonoBehaviour
{
    [Header("UI")]
    public GameObject ui;

    [Header("Scene")]
    public string menuSceneName = "Menu";

    [Header("Fade")]
    public FadeUI fadeUI;

    protected virtual void Start()
    {
        ui.SetActive(false);
    }

    protected void Show()
    {
        EventSystem.current.SetSelectedGameObject(null);
        ui.SetActive(true);
        Time.timeScale = 0f;
    }

    protected void Hide()
    {
        EventSystem.current.SetSelectedGameObject(null);
        ui.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Time.timeScale = 1f;

        if (fadeUI != null)
            StartCoroutine(FadeThenQuit());
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();
            SceneManager.LoadScene(menuSceneName);
        }
    }

    private IEnumerator FadeThenQuit()
    {
        fadeUI.FadeOut();
        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutMusic(fadeUI.fadeDuration);
        yield return new WaitForSeconds(fadeUI.fadeDuration);
        SceneManager.LoadScene(menuSceneName);
    }
}