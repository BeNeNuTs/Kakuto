using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMenuComponent : MenuComponent
{
    private static readonly string K_MASTER_VOLUME = "MasterVolume";
    private static readonly string K_MUSIC_VOLUME = "MusicVolume";
    private static readonly string K_SFX_VOLUME = "SFXVolume";

    private static readonly float K_SLIDER_INCREMENT = 0.1f;

#pragma warning disable 0649
    [SerializeField] private Button[] m_OptionButtons;
    [SerializeField] private Selectable m_DefaultSelectable;
    [SerializeField] private HighlightInfo[] m_AudioHighlightInfo;

    [SerializeField] private Slider m_MasterSlider;
    [SerializeField] private Slider m_MusicSlider;
    [SerializeField] private Slider m_SFXSlider;

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

    public void OnDisable()
    {
        for (int i = 0; i < m_OptionButtons.Length; i++)
        {
            Navigation buttonNavigation = m_OptionButtons[i].navigation;
            if (buttonNavigation.selectOnDown == m_DefaultSelectable)
            {
                buttonNavigation.selectOnDown = null;
                m_OptionButtons[i].navigation = buttonNavigation;
            }
        }
    }

    protected void Update()
    {
        UpdateHighlightedGameObject(m_AudioHighlightInfo);
        CheckPlayerDpadDirectionWithDelay(UpdateDpadSlider);
    }

    private void UpdateDpadSlider(EPlayer player, float horizontalRawAxis, float verticalRawAxis)
    {
        if (horizontalRawAxis != 0f)
        {
            Slider highlightedSlider = GetHighlightedSlider();
            if (highlightedSlider != null)
            {
                if (horizontalRawAxis > 0f)
                    highlightedSlider.value += K_SLIDER_INCREMENT;
                else
                    highlightedSlider.value -= K_SLIDER_INCREMENT;
            }
        }
    }

    private Slider GetHighlightedSlider()
    {
        HighlightInfo currentHighlight = GetCurrentHighlight();
        if (currentHighlight != null)
        {
            if(currentHighlight.m_SelectedGameObject == m_MasterSlider.gameObject)
            {
                return m_MasterSlider;
            }
            else if (currentHighlight.m_SelectedGameObject == m_MusicSlider.gameObject)
            {
                return m_MusicSlider;
            }
            else if (currentHighlight.m_SelectedGameObject == m_SFXSlider.gameObject)
            {
                return m_SFXSlider;
            }
        }

        return null;
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
