using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadingScreenComponent : MonoBehaviour
{
    public Animator m_Animator;
    public GraphicRaycaster m_GraphicRaycaster;

    public bool m_IsLoadingScreenReady;
    public Action m_OnLoadingScreenReady;


    public void StartLoading(bool useFastLoading)
    {
        m_Animator.SetBool("UseFastLoading", useFastLoading);
        m_Animator.SetTrigger("OnLoadingStart");
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
        m_Animator.SetTrigger("OnLoadingEnd");
        EventSystem.current.enabled = true;
        m_GraphicRaycaster.enabled = false;
    }
}
