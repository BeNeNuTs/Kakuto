using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMenuComponent : MenuComponent
{
    private static readonly string K_MASTER_VOLUME = "MasterVolume";
    private static readonly string K_MUSIC_VOLUME = "MusicVolume";
    private static readonly string K_SFX_VOLUME = "SFXVolume";
    private static readonly string K_VOICE_VOLUME = "VoiceVolume";

    private static readonly float K_SLIDER_INCREMENT = 0.1f;

    private static Dictionary<string, float> m_CurrentVolumes = new Dictionary<string, float>()
    {
        { K_MASTER_VOLUME, 1f },
        { K_MUSIC_VOLUME, 1f },
        { K_SFX_VOLUME, 1f },
        { K_VOICE_VOLUME, 1f }
    };

#pragma warning disable 0649
    [SerializeField] private Button[] m_OptionButtons;
    [SerializeField] private Selectable m_DefaultSelectable;
    [SerializeField] private HighlightInfo[] m_AudioHighlightInfo;

    [SerializeField] private Slider m_MasterSlider;
    [SerializeField] private Slider m_MusicSlider;
    [SerializeField] private Slider m_SFXSlider;
    [SerializeField] private Slider m_VoiceSlider;
#pragma warning restore 0649

#if UNITY_EDITOR
    [MenuItem("Kakuto/Clear saved audio options")]
    static void ClearSavedAudioOptions()
    {
        PlayerPrefs.SetFloat(K_MASTER_VOLUME, 1f);
        PlayerPrefs.SetFloat(K_MUSIC_VOLUME, 1f);
        PlayerPrefs.SetFloat(K_SFX_VOLUME, 1f);
        PlayerPrefs.SetFloat(K_VOICE_VOLUME, 1f);
        KakutoDebug.Log("Audio options cleared!");
    }
#endif

    public static void LoadAudioOptions()
    {
        LoadAudioGroupVolume(K_MASTER_VOLUME);
        LoadAudioGroupVolume(K_MUSIC_VOLUME);
        LoadAudioGroupVolume(K_SFX_VOLUME);
        LoadAudioGroupVolume(K_VOICE_VOLUME);
    }

    private static void LoadAudioGroupVolume(string key)
    {
        float volume = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 1f;
        OnVolumeChanged_Internal(key, volume, false);
    }

    protected void OnEnable()
    {
        for (int i = 0; i < m_OptionButtons.Length; i++)
        {
            Navigation buttonNavigation = m_OptionButtons[i].navigation;
            buttonNavigation.selectOnDown = m_DefaultSelectable;
            m_OptionButtons[i].navigation = buttonNavigation;
        }
        UpdateHighlightedGameObject(m_AudioHighlightInfo);

        m_MasterSlider.value = m_CurrentVolumes[K_MASTER_VOLUME];
        m_MusicSlider.value = m_CurrentVolumes[K_MUSIC_VOLUME];
        m_SFXSlider.value = m_CurrentVolumes[K_SFX_VOLUME];
        m_VoiceSlider.value = m_CurrentVolumes[K_VOICE_VOLUME];
    }

    protected void OnDisable()
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
            else if (currentHighlight.m_SelectedGameObject == m_VoiceSlider.gameObject)
            {
                return m_VoiceSlider;
            }
        }

        return null;
    }

    public void OnMasterVolumeChanged(float value)
    {
        OnVolumeChanged_Internal(K_MASTER_VOLUME, value, true);
    }

    public void OnMusicVolumeChanged(float value)
    {
        OnVolumeChanged_Internal(K_MUSIC_VOLUME, value, true);
    }

    public void OnSFXVolumeChanged(float value)
    {
        OnVolumeChanged_Internal(K_SFX_VOLUME, value, true);
    }

    public void OnVoiceVolumeChanged(float value)
    {
        OnVolumeChanged_Internal(K_VOICE_VOLUME, value, true);
    }

    private static void OnVolumeChanged_Internal(string key, float value, bool saveVolume)
    {
        GameConfig.Instance.m_MainMixer.SetFloat(key, Mathf.Log10(value) * 30f);
        m_CurrentVolumes[key] = value;

        if (saveVolume)
            PlayerPrefs.SetFloat(key, value);
    }
}
