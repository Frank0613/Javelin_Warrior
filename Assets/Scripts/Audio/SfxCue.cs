using UnityEngine;

// 一段音效的設定：音檔 + 音量 + 延遲，全部可在 Inspector 調整
[System.Serializable]
public class SfxCue
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("觸發後等幾秒才播放")]
    public float delay = 0f;
}
