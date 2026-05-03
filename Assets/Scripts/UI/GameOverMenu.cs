using UnityEngine;

public class GameOverMenu : UIMenuBase
{
    [Header("References")]
    public PauseMenu pauseMenu;

    [Header("Settings")]
    public float showDelay = 2f;

    public void TriggerGameOver()
    {
        if (pauseMenu != null)
            pauseMenu.OnGameOver();

        Invoke(nameof(Show), showDelay);
    }
}