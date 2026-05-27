using UnityEngine;

// 一首 BGM 的設定：音檔 + 循環 + 延遲，全部可在 Inspector 調整
[System.Serializable]
public class BgmCue
{
    public AudioClip clip;
    public bool loop = true;
    [Tooltip("觸發後等幾秒才播放")]
    public float delay = 0f;
}
