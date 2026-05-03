using UnityEngine;

public class PauseMenu : UIMenuBase
{
    [Header("Input")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;

    private bool isPaused = false;
    private bool gameOver = false;

    void Update()
    {
        if (gameOver) return;

        if (OVRInput.GetDown(OVRInput.Button.One, controllerHand)
            || Input.GetKeyDown(KeyCode.P))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        isPaused = !isPaused;
        if (isPaused) Show();
        else Hide();
    }

    public void Close() => Toggle();

    public void OnGameOver()
    {
        gameOver = true;
        if (isPaused) Toggle();
    }
}