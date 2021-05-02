using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AudioSubGameManager : SubGameManagerBase
{
    public class MusicInfo
    {
        public MusicSettings m_MusicSettings;
        public AudioEntry m_MusicEntry;
        public AudioSource m_MusicSource;

        public MusicInfo(MusicSettings settings, AudioSource source)
        {
            m_MusicSettings = settings;
            m_MusicEntry = settings.m_MusicEntry;
            m_MusicSource = source;
        }
    }

    private readonly List<AttackSFX> m_AttackSFX;
    private readonly HeavyHitGruntSFX m_HeavyHitGruntSFX;
    private readonly List<AnimSFX> m_AnimSFX;
    private readonly UISFX m_UISFX;
    private readonly VoiceSFX m_VoiceSFX;
    private readonly AudioMixerGroup m_MusicMixerGroup;
    private readonly AudioMixerGroup m_SFXMixerGroup;
    private readonly AudioMixerGroup m_VoiceMixerGroup;

    private GameObject m_MusicHandler;
    private Dictionary<string, MusicInfo> m_MusicAudioSources = new Dictionary<string, MusicInfo>();

    private GameObject m_Player1SFXHandler;
    private AudioSource m_Player1WhiffSFXAudioSource;
    private AudioSource m_Player1AttackSFXAudioSource;
    private AudioSource m_Player1HeavyHitGruntSFXAudioSource;
    private Dictionary<EAnimSFXType, AudioSource> m_Player1AnimSFXAudioSources = new Dictionary<EAnimSFXType, AudioSource>();

    private GameObject m_Player2SFXHandler;
    private AudioSource m_Player2WhiffSFXAudioSource;
    private AudioSource m_Player2AttackSFXAudioSource;
    private AudioSource m_Player2HeavyHitGruntSFXAudioSource;
    private Dictionary<EAnimSFXType, AudioSource> m_Player2AnimSFXAudioSources = new Dictionary<EAnimSFXType, AudioSource>();

    private GameObject m_UISFXHandler;
    private Dictionary<EUISFXType, AudioSource> m_UIAudioSources = new Dictionary<EUISFXType, AudioSource>();

    private GameObject m_VoiceSFXHandler;
    private AudioSource m_VoiceSFXAudioSource;

    // Except projectile audio sources
    private List<AudioSource> m_PausableSFXAudioSources = new List<AudioSource>();

    public AudioSubGameManager()
    {
        m_AttackSFX = AttackConfig.Instance.m_AttackSFX;
        m_HeavyHitGruntSFX = AttackConfig.Instance.m_HeavyHitGruntSFX;
        m_AnimSFX = AttackConfig.Instance.m_AnimSFX;

        m_UISFX = UIConfig.Instance.m_UISFX;
        m_VoiceSFX = UIConfig.Instance.m_VoiceSFX;

        m_MusicMixerGroup = GameConfig.Instance.m_MusicMixerGroup;
        m_SFXMixerGroup = GameConfig.Instance.m_SFXMixerGroup;
        m_VoiceMixerGroup = GameConfig.Instance.m_VoiceMixerGroup;

        GamePauseMenuComponent.IsInPauseChanged += IsInPauseChanged;
        GameFlowSubGameManager.OnLoadingScene += OnLoadingScene;
    }

    public override void Init()
    {
        base.Init();
        InitAllAudio();
        StartMusic();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        GamePauseMenuComponent.IsInPauseChanged -= IsInPauseChanged;
        GameFlowSubGameManager.OnLoadingScene -= OnLoadingScene;
    }

    void InitAllAudio()
    {
        CreateHandler("MusicHandler", ref m_MusicHandler);
        CreateHandler("Player1SFXHandler", ref m_Player1SFXHandler);
        CreateHandler("Player2SFXHandler", ref m_Player2SFXHandler);
        CreateHandler("UISFXHandler", ref m_UISFXHandler);
        CreateHandler("VoiceSFXHandler", ref m_VoiceSFXHandler);

        InitSFXAudioSource(ref m_Player1SFXHandler, ref m_Player1WhiffSFXAudioSource, m_SFXMixerGroup, true);
        InitSFXAudioSource(ref m_Player1SFXHandler, ref m_Player1AttackSFXAudioSource, m_SFXMixerGroup, true);
        InitSFXAudioSource(ref m_Player1SFXHandler, ref m_Player1HeavyHitGruntSFXAudioSource, m_SFXMixerGroup, true);

        InitSFXAudioSource(ref m_Player2SFXHandler, ref m_Player2WhiffSFXAudioSource, m_SFXMixerGroup, true);
        InitSFXAudioSource(ref m_Player2SFXHandler, ref m_Player2AttackSFXAudioSource, m_SFXMixerGroup, true);
        InitSFXAudioSource(ref m_Player2SFXHandler, ref m_Player2HeavyHitGruntSFXAudioSource, m_SFXMixerGroup, true);

        InitSFXAudioSource(ref m_VoiceSFXHandler, ref m_VoiceSFXAudioSource, m_VoiceMixerGroup, true);

        List<SceneSettings> allSceneSettings = ScenesConfig.GetAllSceneSettings();
        for(int i = 0; i < allSceneSettings.Count; i++)
        {
            AudioEntry musicEntry = allSceneSettings[i].m_MusicSettings.m_MusicEntry;
            if (musicEntry.m_Clip != null)
            {
                AudioSource musicAudioSource = CreateAudioSource(ref m_MusicHandler, m_MusicMixerGroup, false, musicEntry.m_Clip);
                musicAudioSource.volume = musicEntry.m_Volume;
                m_MusicAudioSources.Add(allSceneSettings[i].m_Scene, new MusicInfo(allSceneSettings[i].m_MusicSettings, musicAudioSource));
            }
        }

        foreach (EUISFXType sfxType in Enum.GetValues(typeof(EUISFXType)))
        {
            AudioEntry sfxEntry = m_UISFX.GetSFX(sfxType);
            if (sfxEntry.m_Clip != null)
            {
                AudioSource SFXTypeAudioSource = CreateAudioSource(ref m_UISFXHandler, m_SFXMixerGroup, false, sfxEntry.m_Clip);
                SFXTypeAudioSource.volume = sfxEntry.m_Volume;
                m_UIAudioSources.Add(sfxType, SFXTypeAudioSource);
            }
        }

        foreach (EAnimSFXType sfxType in Enum.GetValues(typeof(EAnimSFXType)))
        {
            AudioEntry sfxEntry = m_AnimSFX[(int)sfxType].m_SFX;
            if (sfxEntry.m_Clip != null)
            {
                AudioSource p1AnimSFXTypeAudioSource = CreateAudioSource(ref m_Player1SFXHandler, m_SFXMixerGroup, true, sfxEntry.m_Clip);
                p1AnimSFXTypeAudioSource.volume = sfxEntry.m_Volume;

                AudioSource p2AnimSFXTypeAudioSource = CreateAudioSource(ref m_Player2SFXHandler, m_SFXMixerGroup, true, sfxEntry.m_Clip);
                p2AnimSFXTypeAudioSource.volume = sfxEntry.m_Volume;

                m_Player1AnimSFXAudioSources.Add(sfxType, p1AnimSFXTypeAudioSource);
                m_Player2AnimSFXAudioSources.Add(sfxType, p2AnimSFXTypeAudioSource);
            }
        }
    }

    void CreateHandler(string name, ref GameObject handler)
    {
        handler = new GameObject(name);
        GameObject.DontDestroyOnLoad(handler);
    }

    void InitSFXAudioSource(ref GameObject handler, ref AudioSource SFXAudioSource, AudioMixerGroup mixerGroup, bool pausableAudioSource)
    {
        SFXAudioSource = CreateAudioSource(ref handler, mixerGroup, pausableAudioSource);
    }

    AudioSource CreateAudioSource(ref GameObject handler, AudioMixerGroup mixerGroup, bool pausableAudioSource, AudioClip defaultClip = null)
    {
        AudioSource newAudioSource = handler.AddComponent<AudioSource>();
        newAudioSource.clip = defaultClip;
        newAudioSource.outputAudioMixerGroup = mixerGroup;
        newAudioSource.playOnAwake = false;

        if(pausableAudioSource)
            m_PausableSFXAudioSources.Add(newAudioSource);

        return newAudioSource;
    }

    public void PlayWhiffSFX(int playerIndex, EWhiffSFXType whiffSFXType)
    {
        if(whiffSFXType != EWhiffSFXType.None)
        {
            EAttackSFXType attackSFXType = ConvertWhiffToAttackSFXType(whiffSFXType);
            AudioSource sourceToPlay = (playerIndex == 0) ? m_Player1WhiffSFXAudioSource : m_Player2WhiffSFXAudioSource;
            AudioEntry[] attackSFXList = m_AttackSFX[(int)attackSFXType].m_SFXList;
            AudioEntry attackSFXToPlay = attackSFXList[Random.Range(0, attackSFXList.Length)];
            sourceToPlay.clip = attackSFXToPlay.m_Clip;
            sourceToPlay.volume = attackSFXToPlay.m_Volume;
            sourceToPlay.Play();
        }
    }

    public void PlayHitSFX(int playerVictimIndex, EAttackSFXType attackSFXType, bool isProjectileAttack)
    {
        AudioSource sourceToPlay = (playerVictimIndex == 0) ? m_Player1AttackSFXAudioSource : m_Player2AttackSFXAudioSource;
        AudioEntry[] attackSFXList = m_AttackSFX[(int)attackSFXType].m_SFXList;
        AudioEntry attackSFXToPlay = attackSFXList[Random.Range(0, attackSFXList.Length)];
        sourceToPlay.clip = attackSFXToPlay.m_Clip;
        sourceToPlay.volume = attackSFXToPlay.m_Volume;
        sourceToPlay.Play();

        // If projectile attack, stop whiff only on victim, else on both players
        if(isProjectileAttack)
        {
            AudioSource whiffSourceToStop = (playerVictimIndex == 0) ? m_Player1WhiffSFXAudioSource : m_Player2WhiffSFXAudioSource;
            whiffSourceToStop.Stop();
        }
        else
        {
            m_Player1WhiffSFXAudioSource.Stop();
            m_Player2WhiffSFXAudioSource.Stop();
        }

        if (attackSFXType == EAttackSFXType.Hit_Heavy)
        {
            float random = Random.Range(0f, 1f);
            if(random <= m_HeavyHitGruntSFX.m_GruntProbability)
            {
                AudioSource gruntSourceToPlay = (playerVictimIndex == 0) ? m_Player1HeavyHitGruntSFXAudioSource : m_Player2HeavyHitGruntSFXAudioSource;
                AudioEntry gruntSFXToPlay = m_HeavyHitGruntSFX.m_GruntSFX[Random.Range(0, m_HeavyHitGruntSFX.m_GruntSFX.Length)];
                gruntSourceToPlay.clip = gruntSFXToPlay.m_Clip;
                gruntSourceToPlay.volume = gruntSFXToPlay.m_Volume;
                gruntSourceToPlay.Play();
            }
        }
    }

    public void PlayAnimSFX(int playerIndex, EAnimSFXType animSFXType)
    {
        AudioSource sourceToPlay = (playerIndex == 0) ? m_Player1AnimSFXAudioSources[animSFXType] : m_Player2AnimSFXAudioSources[animSFXType];
        sourceToPlay.Play();
    }

    public void StopAnimSFX(int playerIndex, EAnimSFXType animSFXType)
    {
        AudioSource sourceToPlay = (playerIndex == 0) ? m_Player1AnimSFXAudioSources[animSFXType] : m_Player2AnimSFXAudioSources[animSFXType];
        sourceToPlay.Stop();
    }

    public void PlayUISFX(EUISFXType UISFXType)
    {
        m_UIAudioSources[UISFXType].Play();
    }

    public void PlayVoiceSFX(EVoiceSFXType voiceSFXType)
    {
        AudioEntry voiceEntry = m_VoiceSFX.GetSFX(voiceSFXType);
        m_VoiceSFXAudioSource.clip = voiceEntry.m_Clip;
        m_VoiceSFXAudioSource.volume = voiceEntry.m_Volume;
        m_VoiceSFXAudioSource.Play();
    }

    private EAttackSFXType ConvertWhiffToAttackSFXType(EWhiffSFXType whiffSFXType)
    {
        switch (whiffSFXType)
        {
            case EWhiffSFXType.Whiff_LP:
                return EAttackSFXType.Whiff_LP;
            case EWhiffSFXType.Whiff_LK:
                return EAttackSFXType.Whiff_LK;
            case EWhiffSFXType.Whiff_HP:
                return EAttackSFXType.Whiff_HP;
            case EWhiffSFXType.Whiff_HK:
                return EAttackSFXType.Whiff_HK;
            case EWhiffSFXType.Whiff_Parry:
                return EAttackSFXType.Whiff_Parry;
            case EWhiffSFXType.None:
            default:
                return EAttackSFXType.Whiff_LP;
        }
    }

    private void IsInPauseChanged(bool isInPause)
    {
        if(isInPause)
        {
            for (int i = 0; i < m_PausableSFXAudioSources.Count; i++)
            {
                m_PausableSFXAudioSources[i].Pause();
            }
        }
        else
        {
            for (int i = 0; i < m_PausableSFXAudioSources.Count; i++)
            {
                m_PausableSFXAudioSources[i].UnPause();
            }
        }
    }

    private void OnLoadingScene(bool isLoading, string previousScene, string newScene)
    {
        if(!isLoading && previousScene != newScene)
        {
            if(m_MusicAudioSources.TryGetValue(previousScene, out MusicInfo previousMusic) &&
                m_MusicAudioSources.TryGetValue(newScene, out MusicInfo newMusic))
            {
                if(previousMusic.m_MusicEntry.m_Clip != newMusic.m_MusicEntry.m_Clip)
                {
                    GameManager.Instance.StartCoroutine(StopMusic(previousMusic));
                    GameManager.Instance.StartCoroutine(StartMusic(newMusic, false));
                }
            }               
        }
    }

    private void StartMusic()
    {
        if (m_MusicAudioSources.TryGetValue(SceneManager.GetActiveScene().name, out MusicInfo startMusic))
        {
            GameManager.Instance.StartCoroutine(StartMusic(startMusic, true));
        }
    }

    private IEnumerator StopMusic(MusicInfo musicInfo)
    {
        yield return new WaitForSeconds(musicInfo.m_MusicSettings.m_TimeBeforeDecreasingMusicVolume);

        float startingTime = Time.unscaledTime;

        float initialVolume = musicInfo.m_MusicSource.volume;
        float finalVolume = 0f;
        float currentTime = 0.0f;
        float duration = musicInfo.m_MusicSettings.m_TimeToDecreaseMusicVolume;
        while (initialVolume > 0f)
        {
            musicInfo.m_MusicSource.volume = Mathf.Lerp(initialVolume, finalVolume, currentTime);
            currentTime += Time.deltaTime / duration;
            yield return null;
        }

        musicInfo.m_MusicSource.Stop();
    }

    private IEnumerator StartMusic(MusicInfo musicInfo, bool skipDelay)
    {
        if(!skipDelay)
            yield return new WaitForSeconds(musicInfo.m_MusicSettings.m_TimeBeforeIncreasingMusicVolume);

        AudioSource musicSource = musicInfo.m_MusicSource;
        musicSource.volume = 0f;
        musicSource.Play();

        float startingTime = Time.unscaledTime;

        float initialVolume = 0f;
        float finalVolume = musicInfo.m_MusicEntry.m_Volume;
        float currentTime = 0.0f;
        float duration = musicInfo.m_MusicSettings.m_TimeToIncreaseMusicVolume;
        while (initialVolume < finalVolume)
        {
            musicSource.volume = Mathf.Lerp(initialVolume, finalVolume, currentTime);
            currentTime += Time.deltaTime / duration;
            yield return null;
        }
    }
}
