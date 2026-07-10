using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace RoadToWorldcup
{
    public sealed class RuntimeSceneBootstrap : MonoBehaviour
    {
        private static RuntimeSceneBootstrap instance;
        private string lastSceneHandled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (instance != null)
            {
                return;
            }

            GameObject root = new GameObject("RoadToWorldcup_RuntimeBootstrap");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<RuntimeSceneBootstrap>();
            StadiumAudio.EnsureInstalled(root);
            SceneManager.sceneLoaded += instance.OnSceneLoaded;

            ConfigureMobileRuntime();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HandleInitialScene()
        {
            if (instance != null)
            {
                instance.HandleScene(SceneManager.GetActiveScene());
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(HandleSceneNextFrame(scene));
        }

        private IEnumerator HandleSceneNextFrame(Scene scene)
        {
            yield return null;
            HandleScene(scene);
        }

        private void HandleScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            string sceneName = scene.name;
            EnsureEventSystem();

            if (SceneLoader.IsBootstrap(sceneName))
            {
                StadiumAudio.StopBackground();
                if (lastSceneHandled == sceneName)
                {
                    return;
                }

                lastSceneHandled = sceneName;
                GameSession.LoadProgress();
                SceneLoader.LoadMainMenu();
                return;
            }

            lastSceneHandled = sceneName;

            if (SceneLoader.IsMainMenu(sceneName))
            {
                StadiumAudio.SetMainMenuAmbience();
                EnsureSceneController<GeneratedMenuController>("Generated_MainMenu_Controller");
                return;
            }

            if (SceneLoader.IsGameplay(sceneName))
            {
                StadiumAudio.SetGameplayAmbience();
                if (Application.isEditor && !SceneLoader.ConsumeGameplayRequest())
                {
                    SceneLoader.LoadMainMenu();
                    return;
                }

                EnsureSceneController<RoadToWorldcupGame>("Generated_Gameplay_Controller");
            }
        }

        private static void EnsureSceneController<T>(string objectName) where T : Component
        {
            if (FindSceneObject<T>() != null)
            {
                return;
            }

            GameObject controller = new GameObject(objectName);
            controller.AddComponent<T>();
        }

        private static void EnsureEventSystem()
        {
            if (FindSceneObject<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static T FindSceneObject<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        private static void ConfigureMobileRuntime()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.pixelLightCount = 1;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            if (Application.isEditor)
            {
                Screen.SetResolution(1080, 1920, false);
            }
        }
    }
}
