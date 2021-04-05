using System;
using UnityEngine;

public enum EUISFXType
{
    Pause,
    Back,
    Confirm,
    Navigation,
    Slider,
    Toggle
}

[Serializable]
public class UISFX
{
    public AudioEntry m_PauseSFX;
    public AudioEntry m_BackSFX;
    public AudioEntry m_ConfirmSFX;
    public AudioEntry m_NavigationSFX;
    public AudioEntry m_SliderSFX;
    public AudioEntry m_ToggleSFX;

    public AudioEntry GetSFX(EUISFXType UISFXType)
    {
        switch (UISFXType)
        {
            case EUISFXType.Pause:
                return m_PauseSFX;
            case EUISFXType.Back:
                return m_BackSFX;
            case EUISFXType.Confirm:
                return m_ConfirmSFX;
            case EUISFXType.Navigation:
                return m_NavigationSFX;
            case EUISFXType.Slider:
                return m_SliderSFX;
            case EUISFXType.Toggle:
                return m_ToggleSFX;
            default:
                return null;
        }
    }
}

