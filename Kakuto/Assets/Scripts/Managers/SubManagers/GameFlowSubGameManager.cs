﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowSubGameManager : SubGameManagerBase
{
    LoadingScreenComponent m_LoadingScreenComponent;

    bool m_IsLoading;
    string m_SceneToLoad;

    public override void Init()
    {
        base.Init();
        InitLoadingScreen();
    }

    private void InitLoadingScreen()
    {
        GameObject loadingScreenGO = GameObject.Instantiate(GameConfig.Instance.m_LoadingScreenPrefab, Vector3.zero, Quaternion.identity);
        m_LoadingScreenComponent = loadingScreenGO.GetComponent<LoadingScreenComponent>();
        GameObject.DontDestroyOnLoad(loadingScreenGO);
    }

    public void LoadScene(string sceneName)
    {
        m_LoadingScreenComponent.StartLoading();
        m_LoadingScreenComponent.m_OnLoadingScreenReady += OnLoadingScreenReady;

        m_SceneToLoad = sceneName;
        m_IsLoading = true;
        GamePauseMenuComponent.m_IsInPause = false;
    }

    private void OnLoadingScreenReady()
    {
        m_LoadingScreenComponent.m_OnLoadingScreenReady -= OnLoadingScreenReady;
        GameManager.Instance.StartCoroutine(LoadAsyncScene(m_SceneToLoad));
    }

    IEnumerator LoadAsyncScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(m_SceneToLoad);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        EndOfLoading();
    }

    private void EndOfLoading()
    {
        m_LoadingScreenComponent.EndLoading();
        m_IsLoading = false;
    }

    public bool IsLoading()
    {
        return m_IsLoading;
    }
}
