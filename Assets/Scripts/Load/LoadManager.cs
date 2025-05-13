using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Load
{
    [DoNotDestroySingleton]
    public class LoadManager : Singleton<LoadManager>
    {
        public static event Action<Scene> OnSceneLoaded;
        public static event Action<float> OnSceneUnloaded; // float => buildIndex

        public static bool IsLoading;

        public const int MainMenuSceneIndex = 0;
        public const int ElevatorSceneIndex = 1;

        public static int UILevelIndexPressed = 2;

        public static int ActiveLevelBuildIndex
        {
            get
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.buildIndex > ElevatorSceneIndex) return scene.buildIndex;
                }

                return -1;
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            OnSceneLoaded += scene => Debug.Log($"Loaded scene {scene.buildIndex}: {scene.name}");
            OnSceneUnloaded += buildIndex => Debug.Log($"Unloaded scene {buildIndex}");
        }

        public static void LoadScene(int buildIndex, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            Instance.StartCoroutine(Instance.LoadSceneCoroutine(buildIndex, mode));
        }

        private IEnumerator LoadSceneCoroutine(int buildIndex, LoadSceneMode loadMode = LoadSceneMode.Additive)
        {
            if (!IsValidBuildIndex(buildIndex)) yield break;
            IsLoading = true;
            Debug.Log($"Loading scene {buildIndex}");
            if (loadMode == LoadSceneMode.Single && TransitionUI.Instance != null)
                yield return TransitionUI.FadeTransition(false);
            
            yield return SceneManager.LoadSceneAsync(buildIndex, loadMode);
            Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
            IsLoading = false;
            OnSceneLoaded?.Invoke(scene);
            
            Debug.Log($"Loaded scene {buildIndex}");
            if (loadMode == LoadSceneMode.Single && TransitionUI.Instance != null)
                yield return TransitionUI.FadeTransition(true);
        }

        public static void UnloadScene(int buildIndex)
        {
            Instance.StartCoroutine(Instance.UnloadSceneCoroutine(buildIndex));
        }

        private IEnumerator UnloadSceneCoroutine(int buildIndex)
        {
            if (!IsValidBuildIndex(buildIndex)) yield break;
            Debug.Log($"Unloading scene {buildIndex}");
            yield return SceneManager.UnloadSceneAsync(buildIndex);
            OnSceneUnloaded?.Invoke(buildIndex);
            
            Debug.Log($"Unloaded scene {buildIndex}");
        }

        private static bool IsValidBuildIndex(int buildIndex)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            return !string.IsNullOrEmpty(path);
        }
    }
}