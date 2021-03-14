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

    public AudioSubGameManager()
    {
        m_AttackSFX = AttackConfig.Instance.m_AttackSFX;
        m_AnimSFX = AttackConfig.Instance.m_AnimSFX;
        m_SFXMixerGroup = GameConfig.Instance.m_SFXMixerGroup;
    }

    public override void Init()
    {
        base.Init();
        InitAllSFX();
    }

    void InitAllSFX()
    {
        CreateHandler(1, ref m_Player1SFXHandler);
        CreateHandler(2, ref m_Player2SFXHandler);

        InitAttackSFXAudioSources(ref m_Player1SFXHandler, ref m_Player1AttackSFXAudioSources);
        InitAttackSFXAudioSources(ref m_Player2SFXHandler, ref m_Player2AttackSFXAudioSources);

        foreach (EAnimSFXType sfxType in Enum.GetValues(typeof(EAnimSFXType)))
        {
            AudioClip clip = m_AnimSFX[(int)sfxType].m_SFX;

            AudioSource p1AnimSFXTypeAudioSource = CreateAudioSource(ref m_Player1SFXHandler, m_SFXMixerGroup, clip);
            m_Player1AnimSFXAudioSources.Add(sfxType, p1AnimSFXTypeAudioSource);

            AudioSource p2AnimSFXTypeAudioSource = CreateAudioSource(ref m_Player2SFXHandler, m_SFXMixerGroup, clip);
            m_Player2AnimSFXAudioSources.Add(sfxType, p2AnimSFXTypeAudioSource);
        }
    }

    void CreateHandler(int playerIndex, ref GameObject handler)
    {
        handler = new GameObject("Player" + playerIndex + "SFXHandler");
        GameObject.DontDestroyOnLoad(handler);
    }

    void InitAttackSFXAudioSources(ref GameObject handler, ref Dictionary<EAttackSFXType, AudioSource> attackSFXAudioSources)
    {
        AudioSource whiffAudioSource = CreateAudioSource(ref handler, m_SFXMixerGroup);
        attackSFXAudioSources.Add(EAttackSFXType.Whiff_LP, whiffAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Whiff_LK, whiffAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Whiff_HP, whiffAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Whiff_HK, whiffAudioSource);

        AudioSource hitAudioSource = CreateAudioSource(ref handler, m_SFXMixerGroup);
        attackSFXAudioSources.Add(EAttackSFXType.Hit_Light, hitAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Hit_Heavy, hitAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Hit_Throw, hitAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Hit_Special, hitAudioSource);

        AudioSource hitBlockedAudioSource = CreateAudioSource(ref handler, m_SFXMixerGroup);
        attackSFXAudioSources.Add(EAttackSFXType.Blocked_Hit, hitBlockedAudioSource);
    }

    AudioSource CreateAudioSource(ref GameObject handler, AudioMixerGroup mixerGroup, AudioClip defaultClip = null)
    {
        AudioSource newAudioSource = handler.AddComponent<AudioSource>();
        newAudioSource.clip = defaultClip;
        newAudioSource.outputAudioMixerGroup = mixerGroup;
        newAudioSource.playOnAwake = false;

        return newAudioSource;
    }

    public void PlayAttackSFX(int playerIndex, EAttackSFXType attackSFXType)
    {
        AudioSource sourceToPlay = (playerIndex == 0) ? m_Player1AttackSFXAudioSources[attackSFXType] : m_Player2AttackSFXAudioSources[attackSFXType];
        AudioClip[] attackSFXList = m_AttackSFX[(int)attackSFXType].m_SFXList;
        AudioClip attackSFXToPlay = attackSFXList[Random.Range(0, attackSFXList.Length)];
        sourceToPlay.PlayOneShot(attackSFXToPlay);
    }

    public void PlayAnimSFX(int playerIndex, EAnimSFXType animSFXType)
    {
        AudioSource sourceToPlay = (playerIndex == 0) ? m_Player1AnimSFXAudioSources[animSFXType] : m_Player2AnimSFXAudioSources[animSFXType];
        sourceToPlay.Play();
    }
}
