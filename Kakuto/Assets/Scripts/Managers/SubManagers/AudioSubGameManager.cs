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

    public override void OnPlayerRegistered(GameObject player)
    {
        PlayerAttackComponent attackComponent = player.GetComponent<PlayerAttackComponent>();
        attackComponent.OnCurrentAttackStateChanged += OnPlayerAttackStateChanged;
    }

    public override void OnPlayerUnregistered(GameObject player)
    {
        PlayerAttackComponent attackComponent = player.GetComponent<PlayerAttackComponent>();
        attackComponent.OnCurrentAttackStateChanged -= OnPlayerAttackStateChanged;
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
        AudioSource attackAudioSource = CreateAudioSource(ref handler, m_SFXMixerGroup);
        attackSFXAudioSources.Add(EAttackSFXType.Whiff_LP, attackAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Whiff_LK, attackAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Whiff_HP, attackAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Whiff_HK, attackAudioSource);

        attackSFXAudioSources.Add(EAttackSFXType.Hit_Light, attackAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Hit_Heavy, attackAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Hit_Throw, attackAudioSource);
        attackSFXAudioSources.Add(EAttackSFXType.Hit_Special, attackAudioSource);

        attackSFXAudioSources.Add(EAttackSFXType.Blocked_Hit, attackAudioSource);
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
        sourceToPlay.clip = attackSFXToPlay;
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

    private void OnPlayerAttackStateChanged(PlayerBaseAttackLogic playerAttackLogic, EAttackState attackState)
    {
        if (attackState == EAttackState.Startup)
        {
            switch (playerAttackLogic.GetAttack().m_AnimationAttackName)
            {
                case EAnimationAttackName.CrouchLP:
                case EAnimationAttackName.StandLP:
                case EAnimationAttackName.JumpLP:
                    PlayAttackSFX(playerAttackLogic.GetPlayerIndex(), EAttackSFXType.Whiff_LP);
                    break;

                case EAnimationAttackName.CrouchLK:
                case EAnimationAttackName.StandLK:
                case EAnimationAttackName.JumpLK:
                    PlayAttackSFX(playerAttackLogic.GetPlayerIndex(), EAttackSFXType.Whiff_LK);
                    break;

                case EAnimationAttackName.CrouchHP:
                case EAnimationAttackName.StandHP:
                case EAnimationAttackName.JumpHP:
                //Special08 = Overhead
                case EAnimationAttackName.Special08:
                    PlayAttackSFX(playerAttackLogic.GetPlayerIndex(), EAttackSFXType.Whiff_HP);
                    break;

                case EAnimationAttackName.CrouchHK:
                case EAnimationAttackName.StandHK:
                case EAnimationAttackName.JumpHK:
                    PlayAttackSFX(playerAttackLogic.GetPlayerIndex(), EAttackSFXType.Whiff_HK);
                    break;
            }
        }
    }
}
