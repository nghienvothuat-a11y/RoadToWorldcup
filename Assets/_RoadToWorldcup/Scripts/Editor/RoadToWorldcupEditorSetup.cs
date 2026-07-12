#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoadToWorldcup.EditorTools
{
    [InitializeOnLoad]
    public static class RoadToWorldcupEditorSetup
    {
        private const string MobilePortraitLabel = "Mobile Portrait 9:16";

        static RoadToWorldcupEditorSetup()
        {
            EditorApplication.delayCall += ConfigureProjectForMobile;
        }

        [MenuItem("The King: Road to Champion/Set Mobile 9:16 Game View")]
        public static void ConfigureProjectForMobile()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            try
            {
                AddAndSelectGameViewSize(1080, 1920, MobilePortraitLabel);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("The King: Road to Champion could not auto-select the mobile Game View size. Select 1080x1920 or 9:16 manually if needed. " + exception.Message);
            }
        }

        private static void AddAndSelectGameViewSize(int width, int height, string label)
        {
            Assembly editorAssembly = typeof(EditorWindow).Assembly;
            Type gameViewType = editorAssembly.GetType("UnityEditor.GameView");
            EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
            Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
            Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            PropertyInfo instanceProperty = singletonType.GetProperty("instance");
            object sizesInstance = instanceProperty.GetValue(null, null);

            MethodInfo getGroup = sizesType.GetMethod("GetGroup");
            GameViewSizeGroupType groupTypeValue = GetGameViewSizeGroupType(gameView, gameViewType);
            object group = getGroup.Invoke(sizesInstance, new object[] { groupTypeValue });
            Type groupType = group.GetType();
            MethodInfo getTotalCount = groupType.GetMethod("GetTotalCount");
            MethodInfo getGameViewSize = groupType.GetMethod("GetGameViewSize");

            int selectedIndex = FindGameViewSizeIndex(group, getTotalCount, getGameViewSize, label, width, height);
            if (selectedIndex < 0)
            {
                Type gameViewSizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
                Type gameViewSizeTypeEnum = editorAssembly.GetType("UnityEditor.GameViewSizeType");
                object fixedResolution = Enum.Parse(gameViewSizeTypeEnum, "FixedResolution");
                ConstructorInfo constructor = gameViewSizeType.GetConstructor(new Type[]
                {
                    gameViewSizeTypeEnum,
                    typeof(int),
                    typeof(int),
                    typeof(string)
                });
                object newSize = constructor.Invoke(new object[] { fixedResolution, width, height, label });
                MethodInfo addCustomSize = groupType.GetMethod("AddCustomSize");
                addCustomSize.Invoke(group, new object[] { newSize });
                selectedIndex = FindGameViewSizeIndex(group, getTotalCount, getGameViewSize, label, width, height);
            }

            Debug.Log("The King: Road to Champion selected Game View size " + label + " in " + groupTypeValue + " at index " + selectedIndex + ".");
            SelectGameViewSize(gameView, gameViewType, selectedIndex);
        }

        private static GameViewSizeGroupType GetGameViewSizeGroupType(EditorWindow gameView, Type gameViewType)
        {
            PropertyInfo currentGroup = gameViewType.GetProperty("currentGameViewSizeGroupType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (currentGroup != null)
            {
                object value = currentGroup.GetValue(gameView, null);
                if (value is GameViewSizeGroupType)
                {
                    return (GameViewSizeGroupType)value;
                }
            }

            PropertyInfo selectedGroup = gameViewType.GetProperty("selectedSizeGroupType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (selectedGroup != null)
            {
                object value = selectedGroup.GetValue(gameView, null);
                if (value is GameViewSizeGroupType)
                {
                    return (GameViewSizeGroupType)value;
                }
            }

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                return GameViewSizeGroupType.iOS;
            }

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                return GameViewSizeGroupType.Android;
            }

            return GameViewSizeGroupType.Standalone;
        }

        private static int FindGameViewSizeIndex(object group, MethodInfo getTotalCount, MethodInfo getGameViewSize, string label, int width, int height)
        {
            int count = (int)getTotalCount.Invoke(group, null);
            for (int i = 0; i < count; i++)
            {
                object size = getGameViewSize.Invoke(group, new object[] { i });
                Type sizeType = size.GetType();
                string baseText = (string)sizeType.GetProperty("baseText").GetValue(size, null);
                int sizeWidth = (int)sizeType.GetProperty("width").GetValue(size, null);
                int sizeHeight = (int)sizeType.GetProperty("height").GetValue(size, null);

                if (baseText == label || (sizeWidth == width && sizeHeight == height))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void SelectGameViewSize(EditorWindow gameView, Type gameViewType, int selectedIndex)
        {
            if (selectedIndex < 0)
            {
                return;
            }

            MethodInfo sizeSelectionCallback = gameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (sizeSelectionCallback != null)
            {
                ParameterInfo[] parameters = sizeSelectionCallback.GetParameters();
                if (parameters.Length == 2)
                {
                    sizeSelectionCallback.Invoke(gameView, new object[] { selectedIndex, null });
                }
                else if (parameters.Length == 1)
                {
                    sizeSelectionCallback.Invoke(gameView, new object[] { selectedIndex });
                }
                else
                {
                    Debug.LogWarning("The King: Road to Champion found an unsupported GameView.SizeSelectionCallback signature.");
                }

                gameView.Repaint();
                return;
            }

            PropertyInfo selectedSizeIndex = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (selectedSizeIndex != null)
            {
                selectedSizeIndex.SetValue(gameView, selectedIndex, null);
                gameView.Repaint();
            }
        }
    }
}
#endif
