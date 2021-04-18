using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioSubGameManager : SubGameManagerBase
{
    private readonly List<AttackSFX> m_AttackSFX;
    private readonly List<AnimSFX> m_AnimSFX;
    private readonly UISFX m_UISFX;
    private readonly AudioMixerGroup m_SFXMixerGroup;

    private GameObject m_Player1SFXHandler;
    private AudioSource m_Player1AttackSFXAudioSource;
    private Dictionary<EAnimSFXType, AudioSource> m_Player1AnimSFXAudioSources = new Dictionary<EAnimSFXType, AudioSource>();

    private GameObject m_Player2SFXHandler;
    private AudioSource m_Player2AttackSFXAudioSource;
    private Dictionary<EAnimSFXType, AudioSource> m_Player2AnimSFXAudioSources = new Dictionary<EAnimSFXType, AudioSource>();

    private GameObject m_UISFXHandler;
    private Dictionary<EUISFXType, AudioSource> m_UIAudioSources = new Dictionary<EUISFXType, AudioSource>();

    // Except projectile audio sources
    private List<AudioSource> m_PausableSFXAudioSources = new List<AudioSource>();

    public AudioSubGameManager()
    {
        m_AttackSFX = AttackConfig.Instance.m_AttackSFX;
        m_AnimSFX = AttackConfig.Instance.m_AnimSFX;
        m_UISFX = UIConfig.Instance.m_UISFX;
        m_SFXMixerGroup = GameConfig.Instance.m_SFXMixerGroup;

        GamePauseMenuComponent.IsInPauseChanged += IsInPauseChanged;
    }

    public override void Init()
    {
        base.Init();
        InitAllSFX();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        GamePauseMenuComponent.IsInPauseChanged -= IsInPauseChanged;
    }

    void InitAllSFX()
    {
        CreateHandler("Player1SFXHandler", ref m_Player1SFXHandler);
        CreateHandler("Player1SFXHandler", ref m_Player2SFXHandler);
        CreateHandler("UISFXHandler", ref m_UISFXHandler);

        InitSFXAudioSource(ref m_Player1SFXHandler, ref m_Player1AttackSFXAudioSource, true);
        InitSFXAudioSource(ref m_Player2SFXHandler, ref m_Player2AttackSFXAudioSource, true);

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

    void InitSFXAudioSource(ref GameObject handler, ref AudioSource SFXAudioSource, bool pausableAudioSource)
    {
        SFXAudioSource = CreateAudioSource(ref handler, m_SFXMixerGroup, pausableAudioSource);
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
            PlayAttackSFX(playerIndex, attackSFXType);
        }
    }

    public void PlayAttackSFX(int playerIndex, EAttackSFXType attackSFXType)
    {
        AudioSource sourceToPlay = (playerIndex == 0) ? m_Player1AttackSFXAudioSource : m_Player2AttackSFXAudioSource;
        AudioEntry[] attackSFXList = m_AttackSFX[(int)attackSFXType].m_SFXList;
        AudioEntry attackSFXToPlay = attackSFXList[Random.Range(0, attackSFXList.Length)];
        sourceToPlay.clip = attackSFXToPlay.m_Clip;
        sourceToPlay.volume = attackSFXToPlay.m_Volume;
        sourceToPlay.Play();
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
}
