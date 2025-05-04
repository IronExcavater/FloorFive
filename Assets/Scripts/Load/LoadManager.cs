using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Load
{
    [DoNotDestroySingleton]
    public class LoadManager : Singleton<LoadManager>
    {
        [Header("Game Data")]
        public static Data GameData = new();
        public string gameDataPath = "/save.json";

        public static event Action OnDataSaved;
        public static event Action OnDataLoaded;
        
        public static event Action<Scene> OnSceneLoaded;
        public static event Action<float> OnSceneUnloaded; // float => buildIndex

        public static int ActiveLevelBuildIndex
        {
            get
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    // Assumed that elevator scene has buildIndex of 0
                    if (scene.buildIndex != 0) return scene.buildIndex;
                }

                return -1;
            }
        }

        public class Data
        {
            public float MasterVolume;
            public float MusicVolume;
            public float SfxVolume;
            public bool VSync;
            public FullScreenMode FullScreenMode = FullScreenMode.FullScreenWindow;
        }

        protected override void Awake()
        {
            base.Awake();
            LoadData();
            
            OnSceneLoaded += scene => Debug.Log($"Loaded scene {scene.buildIndex}: {scene.name}");
            OnSceneUnloaded += buildIndex => Debug.Log($"Unloaded scene {buildIndex}");
        }

        public static void SaveData()
        {
            string json = JsonUtility.ToJson(GameData, true);
            File.WriteAllText(Application.persistentDataPath + Instance.gameDataPath, json);
            OnDataSaved?.Invoke();
        }

        private static void LoadData()
        {
            string path = Application.persistentDataPath + Instance.gameDataPath;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                GameData = JsonUtility.FromJson<Data>(json);
                OnDataLoaded?.Invoke();
            }
        }

        public static IEnumerator LoadScene(int buildIndex, LoadSceneMode loadMode = LoadSceneMode.Additive)
        {
            if (!IsValidBuildIndex(buildIndex)) yield break;
            Debug.Log($"Loading scene {buildIndex}");
            yield return SceneManager.LoadSceneAsync(buildIndex, loadMode);
            Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
            OnSceneLoaded?.Invoke(scene);
        }

        public static IEnumerator UnloadScene(int buildIndex)
        {
            if (!IsValidBuildIndex(buildIndex)) yield break;
            Debug.Log($"Unloading scene {buildIndex}");
            yield return SceneManager.UnloadSceneAsync(buildIndex);
            OnSceneUnloaded?.Invoke(buildIndex);
        }

        private static bool IsValidBuildIndex(int buildIndex)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            return !string.IsNullOrEmpty(path);
        }
    }
}