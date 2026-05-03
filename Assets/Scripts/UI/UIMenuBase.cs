using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class UIMenuBase : MonoBehaviour
{
    [Header("UI")]
    public GameObject ui;

    [Header("Scene")]
    public string menuSceneName = "Menu";

    protected virtual void Start()
    {
        ui.SetActive(false);
    }

    protected void Show()
    {
        ui.SetActive(true);
        Time.timeScale = 0f;
    }

    protected void Hide()
    {
        ui.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}