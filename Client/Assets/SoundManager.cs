using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Audio;      // (옵션) 믹서 연결용

public class SoundManager : GenericSingleton<SoundManager>
{
    [Header("오디오 믹서")]
    [SerializeField] private AudioMixerGroup bgmMixer;
    [SerializeField] private AudioMixerGroup sfxMixer;

    [SerializeField, LabelText("BGM 리스트")] private List<AudioClip> bgmList;
    [SerializeField, LabelText("SFX 리스트")] private List<AudioClip> sfxList;
    
    [Header("Pool Settings")]
    [SerializeField] private int sfxPoolSize = 10;
    
    private AudioSource bgmSource;
    private Coroutine bgmFadeCo;
    
    private readonly Queue<AudioSource> sfxPool = new();
    private readonly List<AudioSource> activeSfx = new();
    
    void Awake()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = bgmMixer;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        
        for (int i = 0; i < sfxPoolSize; i++)
            sfxPool.Enqueue(CreateSfxSource());
    }

    private AudioSource CreateSfxSource()
    {
        var src = new GameObject("SFX").AddComponent<AudioSource>();
        src.transform.parent = transform;
        src.outputAudioMixerGroup = sfxMixer;
        src.playOnAwake = false;
        return src;
    }
    
    public void PlayBGM(int clipIndex, float volume = 1f, float fadeTime = 0.5f)
    {
        if (bgmFadeCo != null) StopCoroutine(bgmFadeCo);
        bgmFadeCo = StartCoroutine(FadeBgmRoutine(bgmList[clipIndex], volume, fadeTime));
    }

    public void StopBGM(float fadeTime = 0.5f)
    {
        if (bgmFadeCo != null) StopCoroutine(bgmFadeCo);
        bgmFadeCo = StartCoroutine(FadeBgmRoutine(null, 0f, fadeTime));
    }

    private IEnumerator FadeBgmRoutine(AudioClip nextClip, float targetVol, float time)
    {
        float startVol = bgmSource.volume;
        float t = 0f;

        // 페이드 아웃
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / time);
            yield return null;
        }

        if (nextClip != null)
        {
            bgmSource.clip = nextClip;
            bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }

        // 페이드 인
        t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVol, t / time);
            yield return null;
        }
        bgmSource.volume = targetVol;
    }
    
    public void PlaySFX(int sfxIndex, float volume = 1f, float pitchRandom = 0.05f)
    {
        if (sfxList[sfxIndex] == null) return;

        AudioSource src = sfxPool.Count > 0 ? sfxPool.Dequeue() : CreateSfxSource();
        src.clip = sfxList[sfxIndex];
        src.volume = volume;
        src.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
        src.transform.position = Vector3.zero;
        src.Play();

        activeSfx.Add(src);
        StartCoroutine(ReleaseWhenDone(src));
    }

    private System.Collections.IEnumerator ReleaseWhenDone(AudioSource src)
    {
        yield return new WaitUntil(() => !src.isPlaying);
        activeSfx.Remove(src);
        sfxPool.Enqueue(src);
    }
    
    public void SetMasterVolume(float linear) =>
        AudioListener.volume = Mathf.Clamp01(linear);

    public void SetBgmVolume(float linear) =>
        bgmMixer?.audioMixer.SetFloat("BGMVol", LinearToDb(linear));

    public void SetSfxVolume(float linear) =>
        sfxMixer?.audioMixer.SetFloat("SFXVol", LinearToDb(linear));

    private float LinearToDb(float linear) => Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
}