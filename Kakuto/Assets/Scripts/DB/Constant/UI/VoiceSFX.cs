using System;

public enum EVoiceSFXType
{
    GameIntro,
    KO,
    DoubleKO,
    Perfect,
    TimeOver,
    Round1,
    Round2,
    FinalRound,
    Fight,
    P1Wins,
    P2Wins
}

[Serializable]
public class VoiceSFX
{
    public AudioEntry m_GameIntroSFX;
    public AudioEntry m_KOSFX;
    public AudioEntry m_DoubleKOSFX;
    public AudioEntry m_PerfectSFX;
    public AudioEntry m_TimeOverSFX;
    public AudioEntry m_Round1SFX;
    public AudioEntry m_Round2SFX;
    public AudioEntry m_FinalRoundSFX;
    public AudioEntry m_FightSFX;
    public AudioEntry m_P1WinsSFX;
    public AudioEntry m_P2WinsSFX;

    public AudioEntry GetSFX(EVoiceSFXType voiceSFXType)
    {
        switch (voiceSFXType)
        {
            case EVoiceSFXType.GameIntro:
                return m_GameIntroSFX;
            case EVoiceSFXType.KO:
                return m_KOSFX;
            case EVoiceSFXType.DoubleKO:
                return m_DoubleKOSFX;
            case EVoiceSFXType.Perfect:
                return m_PerfectSFX;
            case EVoiceSFXType.TimeOver:
                return m_TimeOverSFX;
            case EVoiceSFXType.Round1:
                return m_Round1SFX;
            case EVoiceSFXType.Round2:
                return m_Round2SFX;
            case EVoiceSFXType.FinalRound:
                return m_FinalRoundSFX;
            case EVoiceSFXType.Fight:
                return m_FightSFX;
            case EVoiceSFXType.P1Wins:
                return m_P1WinsSFX;
            case EVoiceSFXType.P2Wins:
                return m_P2WinsSFX;
            default:
                return null;
        }
    }
}

