using UnityEngine.SceneManagement;

namespace RoadToWorldcup
{
    public static class SceneLoader
    {
        public const string BootstrapSceneName = "Bootstrap";
        public const string MainMenuSceneName = "MainMenu";
        public const string GameplaySceneName = "Gameplay";

        private static bool gameplayRequested;

        public static bool ConsumeGameplayRequest()
        {
            if (!gameplayRequested)
            {
                return false;
            }

            gameplayRequested = false;
            return true;
        }

        public static void LoadMainMenu()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }

        public static void LoadGameplay()
        {
            gameplayRequested = true;
            SceneManager.LoadScene(GameplaySceneName);
        }

        public static bool IsMainMenu(string sceneName)
        {
            return sceneName == MainMenuSceneName;
        }

        public static bool IsGameplay(string sceneName)
        {
            return sceneName == GameplaySceneName;
        }

        public static bool IsBootstrap(string sceneName)
        {
            return sceneName == BootstrapSceneName;
        }
    }
}
