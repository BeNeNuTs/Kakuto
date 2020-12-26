using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenComponent : MonoBehaviour
{
    public Animator m_Animator;
    public GraphicRaycaster m_GraphicRaycaster;

    public bool m_IsLoadingScreenReady;
    public Action m_OnLoadingScreenReady;


    public void StartLoading()
    {
        m_Animator.SetTrigger("OnLoadingStart");
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
        m_GraphicRaycaster.enabled = false;
    }
}
