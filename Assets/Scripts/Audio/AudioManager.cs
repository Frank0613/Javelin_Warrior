using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // 取得 AudioManager；若當前場景沒有，就即時建立一個（Awake 會設定好一切）
    public static AudioManager Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("AudioManager (auto)");
            go.AddComponent<AudioManager>();
        }
        return Instance;
    }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public float MasterVolume => masterVolume;

    [Header("BGM")]
    public float bgmFadeDuration = 1f;

    private const string MasterVolKey = "MasterVolume";
    private const string MusicVolKey = "MusicVolume";
    private const string SfxVolKey = "SfxVolume";

    void Awake()
    {
        // 常駐單例：第一個存活，之後場景帶進來的重複物件自我銷毀
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        masterVolume = PlayerPrefs.GetFloat(MasterVolKey, masterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicVolKey, musicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolKey, sfxVolume);

        AudioListener.volume = masterVolume; // 全域音量：所有聲音都乘上這個值

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 每次載入新場景後，主動播放該場景 SceneBGM 指定的 BGM
    // （用「播放」而非「停止」；若該場景的 SceneBGM.Start 已先播了同一首，PlayMusic 的 guard 會略過，不會重播）
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var sceneBgm = FindAnyObjectByType<SceneBGM>();
        if (sceneBgm != null)
            PlayMusic(sceneBgm.bgm);
    }

    // ---- BGM ----
    // 播放 Inspector 設定好的 BgmCue（音檔 + 循環 + 延遲）
    public void PlayMusic(BgmCue cue)
    {
        if (cue == null || cue.clip == null || musicSource == null) return;
        if (musicSource.clip == cue.clip && musicSource.isPlaying) return;

        StopAllCoroutines();
        StartCoroutine(PlayMusicRoutine(cue));
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return; // 同一首正在播就不重來

        StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(clip, loop));
    }

    IEnumerator PlayMusicRoutine(BgmCue cue)
    {
        if (cue.delay > 0f)
            yield return new WaitForSeconds(cue.delay);
        yield return StartCoroutine(CrossfadeMusic(cue.clip, cue.loop));
    }

    public void StopMusic()
    {
        StopAllCoroutines();
        if (musicSource != null) musicSource.Stop();
    }

    // 淡出並停止 BGM（轉場時用）。duration 留空則用 bgmFadeDuration
    public void FadeOutMusic(float duration = -1f)
    {
        if (musicSource == null || !musicSource.isPlaying) return;
        StopAllCoroutines();
        StartCoroutine(FadeOutRoutine(duration < 0f ? bgmFadeDuration : duration));
    }

    IEnumerator FadeOutRoutine(float duration)
    {
        float startVol = musicSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        musicSource.volume = 0f;
        musicSource.Stop();
    }

    IEnumerator CrossfadeMusic(AudioClip clip, bool loop)
    {
        // 用 unscaledDeltaTime，暫停（timeScale = 0）時淡入淡出仍正常
        if (musicSource.isPlaying && bgmFadeDuration > 0f)
        {
            float startVol = musicSource.volume;
            float t = 0f;
            while (t < bgmFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeDuration);
                yield return null;
            }
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();

        float t2 = 0f;
        while (t2 < bgmFadeDuration)
        {
            t2 += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t2 / bgmFadeDuration);
            yield return null;
        }
        musicSource.volume = musicVolume;
    }

    // ---- SFX ----
    // 直接播放 Inspector 設定好的 SfxCue（音檔 + 音量 + 延遲）
    public void PlaySFX(SfxCue cue)
    {
        if (cue == null) return;
        PlaySFX(cue.clip, cue.volume, cue.delay);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float delay = 0f)
    {
        if (clip == null || sfxSource == null) return;

        if (delay > 0f)
            StartCoroutine(PlaySFXDelayed(clip, volumeScale, delay));
        else
            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    IEnumerator PlaySFXDelayed(AudioClip clip, float volumeScale, float delay)
    {
        yield return new WaitForSeconds(delay);
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // ---- Volume（之後接設定面板用）----
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = masterVolume;
        PlayerPrefs.SetFloat(MasterVolKey, masterVolume);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicSource != null) musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat(MusicVolKey, musicVolume);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolKey, sfxVolume);
    }
}
