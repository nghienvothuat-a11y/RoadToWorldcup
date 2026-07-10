#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RoadToWorldcup.EditorTools
{
    public static class IosTestFlightBuild
    {
        private const string OutputPath = "Builds/iOS/RoadtoWorldCup";

        public static void Build()
        {
            Export(OutputPath, BuildOptions.None);
        }

        public static void BuildSimulator()
        {
            PropertyInfo sdkVersion = typeof(PlayerSettings.iOS).GetProperty("sdkVersion", BindingFlags.Public | BindingFlags.Static);
            PropertyInfo simulatorArchitecture = typeof(PlayerSettings.iOS).GetProperty("simulatorSdkArchitecture", BindingFlags.Public | BindingFlags.Static);

            if (sdkVersion == null || simulatorArchitecture == null)
            {
                throw new Exception("This Unity version does not expose the iOS Simulator player settings.");
            }

            object previousSdkVersion = sdkVersion.GetValue(null, null);
            object previousArchitecture = simulatorArchitecture.GetValue(null, null);
            try
            {
                sdkVersion.SetValue(null, Enum.Parse(sdkVersion.PropertyType, "SimulatorSDK"), null);
                simulatorArchitecture.SetValue(null, Enum.Parse(simulatorArchitecture.PropertyType, "ARM64"), null);
                Export("Builds/iOSSimulator/RoadtoWorldCup", BuildOptions.Development);
            }
            finally
            {
                sdkVersion.SetValue(null, previousSdkVersion, null);
                simulatorArchitecture.SetValue(null, previousArchitecture, null);
            }
        }

        private static void Export(string outputPath, BuildOptions options)
        {
            ConfigureAppIcon();

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new Exception("No enabled scenes are configured for the iOS build.");
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string output = Path.Combine(projectRoot, outputPath);
            Directory.CreateDirectory(output);

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.iOS,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"iOS export failed: {report.summary.result}");
            }

            Console.WriteLine($"iOS Xcode project exported to {output}");
        }

        private static void ConfigureAppIcon()
        {
            const string iconPath = "Assets/_RoadToWorldcup/Art/Textures/RoadtoWorldCupAppIcon.png";
            Texture2D appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            if (appIcon == null)
            {
                throw new Exception($"iOS app icon was not found at {iconPath}.");
            }

            int iconCount = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.iOS).Length;
            PlayerSettings.SetIconsForTargetGroup(
                BuildTargetGroup.iOS,
                Enumerable.Repeat(appIcon, iconCount).ToArray());
        }
    }
}
#endif
