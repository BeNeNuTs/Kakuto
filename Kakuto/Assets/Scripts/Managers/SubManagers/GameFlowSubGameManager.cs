﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowSubGameManager : SubGameManagerBase
{
    LoadingScreenComponent m_LoadingScreenComponent;

    bool m_IsLoading;
    string m_PreviousScene;
    string m_SceneToLoad;

    public static Action<bool, string, string> OnLoadingScene;

    public override void Init()
    {
        base.Init();
        InitLoadingScreen();
    }

    public void LoadOptions()
    {
        // Need to load audio options here in order to affect AudioMixer (Init is too early)
        AudioMenuComponent.LoadAudioOptions();
        TrainingOptionsMenuComponent.LoadTrainingOptions();
    }

    private void InitLoadingScreen()
    {
        GameObject loadingScreenGO = GameObject.Instantiate(GameConfig.Instance.m_LoadingScreenPrefab, Vector3.zero, Quaternion.identity);
        m_LoadingScreenComponent = loadingScreenGO.GetComponent<LoadingScreenComponent>();
        GameObject.DontDestroyOnLoad(loadingScreenGO);
    }

    public void LoadScene(string sceneName, bool useFastLoading)
    {
        m_LoadingScreenComponent.StartLoading(useFastLoading);
        m_LoadingScreenComponent.m_OnLoadingScreenReady += OnLoadingScreenReady;

        m_IsLoading = true;
        m_PreviousScene = SceneManager.GetActiveScene().name;
        m_SceneToLoad = sceneName;
        OnLoadingScene?.Invoke(true, m_PreviousScene, m_SceneToLoad);
        GamePauseMenuComponent.IsInPause = false;
    }

    private void OnLoadingScreenReady()
    {
        m_LoadingScreenComponent.m_OnLoadingScreenReady -= OnLoadingScreenReady;
        GameManager.Instance.StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
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

        OnLoadingScene?.Invoke(false, m_PreviousScene, m_SceneToLoad);
        m_IsLoading = false;
        m_SceneToLoad = string.Empty;
    }

    public string GetLoadingScene()
    {
        return m_SceneToLoad;
    }

    public bool IsLoading()
    {
        return m_IsLoading;
    }
}
