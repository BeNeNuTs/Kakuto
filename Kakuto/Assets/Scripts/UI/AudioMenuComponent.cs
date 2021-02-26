using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMenuComponent : MenuComponent
{
    private static readonly string K_MASTER_VOLUME = "MasterVolume";
    private static readonly string K_MUSIC_VOLUME = "MusicVolume";
    private static readonly string K_SFX_VOLUME = "SFXVolume";

#pragma warning disable 0649
    [SerializeField] private Button[] m_OptionButtons;
    [SerializeField] private Selectable m_DefaultSelectable;
    [SerializeField] private HighlightInfo[] m_AudioHighlightInfo;

    [SerializeField] private AudioMixer m_MainMixer;
#pragma warning restore 0649

    public void OnEnable()
    {
        for (int i = 0; i < m_OptionButtons.Length; i++)
        {
            Navigation buttonNavigation = m_OptionButtons[i].navigation;
            buttonNavigation.selectOnDown = m_DefaultSelectable;
            m_OptionButtons[i].navigation = buttonNavigation;
        }
        UpdateHighlightedGameObject(m_AudioHighlightInfo);
    }

    protected void Update()
    {
        UpdateHighlightedGameObject(m_AudioHighlightInfo);
    }

    public void OnMasterVolumeChanged(float value)
    {
        OnVolumeChanged(K_MASTER_VOLUME, value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        OnVolumeChanged(K_MUSIC_VOLUME, value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        OnVolumeChanged(K_SFX_VOLUME, value);
    }

    private void OnVolumeChanged(string key, float value)
    {
        m_MainMixer.SetFloat(key, Mathf.Log10(value) * 20f);
    }
}
