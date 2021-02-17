using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadingScreenComponent : MonoBehaviour
{
    private static readonly string K_ANIM_USEFASTLOADING_BOOL = "UseFastLoading";
    private static readonly string K_ANIM_ONLOADINGSTART_TRIGGER = "OnLoadingStart";
    private static readonly string K_ANIM_ONLOADINGEND_TRIGGER = "OnLoadingEnd";

    public Animator m_Animator;
    public GraphicRaycaster m_GraphicRaycaster;

    public bool m_IsLoadingScreenReady;
    public Action m_OnLoadingScreenReady;

    public void StartLoading(bool useFastLoading)
    {
        m_Animator.SetBool(K_ANIM_USEFASTLOADING_BOOL, useFastLoading);
        m_Animator.SetTrigger(K_ANIM_ONLOADINGSTART_TRIGGER);
        EventSystem.current.enabled = false;
        m_GraphicRaycaster.enabled = true;
        m_IsLoadingScreenReady = false;
    }

    public void OnLoadingScreenReady()
    {
        m_OnLoadingScreenReady?.Invoke();
        m_IsLoadingScreenReady = true;
    }

    public void EndLoading()
    {
        m_Animator.SetTrigger(K_ANIM_ONLOADINGEND_TRIGGER);
        EventSystem.current.enabled = true;
        m_GraphicRaycaster.enabled = false;
    }
}
