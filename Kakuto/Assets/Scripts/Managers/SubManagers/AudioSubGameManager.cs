using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioSubGameManager : SubGameManagerBase
{
    private readonly List<AttackSFX> m_AttackSFX;
    private readonly List<AnimSFX> m_AnimSFX;
    private readonly AudioMixerGroup m_SFXMixerGroup;

    private GameObject m_Player1SFXHandler;
    private Dictionary<EAttackSFXType, AudioSource> m_Player1AttackSFXAudioSources = new Dictionary<EAttackSFXType, AudioSource>();
    private Dictionary<EAnimSFXType, AudioSource> m_Player1AnimSFXAudioSources = new Dictionary<EAnimSFXType, AudioSource>();

    private GameObject m_Player2SFXHandler;
    private Dictionary<EAttackSFXType, AudioSource> m_Player2AttackSFXAudioSources = new Dictionary<EAttackSFXType, AudioSource>();
    private Dictionary<EAnimSFXType, AudioSource> m_Player2AnimSFXAudioSources = new Dictionary<EAnimSFXType, AudioSource>();

    private List<AudioSource> m_AllSFXAudioSources = new List<AudioSource>();

    public AudioSubGameManager()
    {
        m_AttackSFX = AttackConfig.Instance.m_AttackSFX;
        m_AnimSFX = AttackConfig.Instance.m_AnimSFX;
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
        CreateHandler(1, ref m_Player1SFXHandler);
        CreateHandler(2, ref m_Player2SFXHandler);

        InitAttackSFXAudioSources(ref m_Player1SFXHandler, ref m_Player1AttackSFXAudioSources);
        InitAttackSFXAudioSources(ref m_Player2SFXHandler, ref m_Player2AttackSFXAudioSources);

        foreach (EAnimSFXType sfxType in Enum.GetValues(typeof(EAnimSFXType)))
        {
            AudioEntry sfxEntry = m_AnimSFX[(int)sfxType].m_SFX;
            if (sfxEntry.m_Clip != null)
            {
                AudioSource p1AnimSFXTypeAudioSource = CreateAudioSource(ref m_Player1SFXHandler, m_SFXMixerGroup, sfxEntry.m_Clip);
                p1AnimSFXTypeAudioSource.volume = sfxEntry.m_Volume;

                AudioSource p2AnimSFXTypeAudioSource = CreateAudioSource(ref m_Player2SFXHandler, m_SFXMixerGroup, sfxEntry.m_Clip);
                p2AnimSFXTypeAudioSource.volume = sfxEntry.m_Volume;

                m_Player1AnimSFXAudioSources.Add(sfxType, p1AnimSFXTypeAudioSource);
                m_Player2AnimSFXAudioSources.Add(sfxType, p2AnimSFXTypeAudioSource);
            }
        }
    }

    void CreateHandler(int playerIndex, ref GameObject handler)
    {
        handler = new GameObject("Player" + playerIndex + "SFXHandler");
        GameObject.DontDestroyOnLoad(handler);
    }

    void InitAttackSFXAudioSources(ref GameObject handler, ref Dictionary<EAttackSFXType, AudioSource> attackSFXAudioSources)
    {
        AudioSource attackAudioSource = CreateAudioSource(ref handler, m_SFXMixerGroup);
        foreach (EAttackSFXType sfxType in Enum.GetValues(typeof(EAttackSFXType)))
        {
            attackSFXAudioSources.Add(sfxType, attackAudioSource);
        }
    }

    AudioSource CreateAudioSource(ref GameObject handler, AudioMixerGroup mixerGroup, AudioClip defaultClip = null)
    {
        AudioSource newAudioSource = handler.AddComponent<AudioSource>();
        newAudioSource.clip = defaultClip;
        newAudioSource.outputAudioMixerGroup = mixerGroup;
        newAudioSource.playOnAwake = false;

        m_AllSFXAudioSources.Add(newAudioSource);

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
        AudioSource sourceToPlay = (playerIndex == 0) ? m_Player1AttackSFXAudioSources[attackSFXType] : m_Player2AttackSFXAudioSources[attackSFXType];
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
            for (int i = 0; i < m_AllSFXAudioSources.Count; i++)
            {
                m_AllSFXAudioSources[i].Pause();
            }
        }
        else
        {
            for (int i = 0; i < m_AllSFXAudioSources.Count; i++)
            {
                m_AllSFXAudioSources[i].UnPause();
            }
        }
    }
}
