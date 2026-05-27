using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    public BgmCue bgm;

    void Start()
    {
        AudioManager.Ensure().PlayMusic(bgm);
    }
}
