using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;

namespace CrossingLears.Editor
{
    [InitializeOnLoad]
    public static class CrossingLearsDefineBootstrap
    {
        internal const string DefineSymbol = "CROSSINGLEARS";
        internal const string PackageRoot = "Assets/Crossing Lears Core";

        static CrossingLearsDefineBootstrap()
        {
            EditorApplication.delayCall -= EnsureDefine;
            EditorApplication.delayCall += EnsureDefine;
            Events.registeredPackages -= OnRegisteredPackages;
            Events.registeredPackages += OnRegisteredPackages;
        }

        internal static void EnsureDefine()
        {
            SetDefineForAllNamedBuildTargets(true);
        }

        internal static void RemoveDefine()
        {
            SetDefineForAllNamedBuildTargets(false);
        }

        private static void OnRegisteredPackages(PackageRegistrationEventArgs eventArguments)
        {
            for (int i = 0; i < eventArguments.removed.Count; i++)
            {
                UnityEditor.PackageManager.PackageInfo removedPackage = eventArguments.removed[i];

                if (removedPackage.name == "com.crossinglears.core")
                {
                    RemoveDefine();
                    return;
                }
            }

            EnsureDefine();
        }

        private static void SetDefineForAllNamedBuildTargets(bool enabled)
        {
            PropertyInfo[] properties = typeof(NamedBuildTarget).GetProperties(BindingFlags.Public | BindingFlags.Static);
            HashSet<string> processedTargets = new HashSet<string>();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                if (property.PropertyType != typeof(NamedBuildTarget) || property.Name == nameof(NamedBuildTarget.Unknown))
                {
                    continue;
                }

                NamedBuildTarget namedBuildTarget = (NamedBuildTarget)property.GetValue(null);
                string targetName = namedBuildTarget.TargetName;

                if (string.IsNullOrEmpty(targetName) || !processedTargets.Add(targetName))
                {
                    continue;
                }

                string currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
                string updatedDefines = UpdateDefines(currentDefines, enabled);

                if (currentDefines == updatedDefines)
                {
                    continue;
                }

                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, updatedDefines);
            }
        }

        private static string UpdateDefines(string currentDefines, bool enabled)
        {
            string[] splitDefines = currentDefines.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> defines = new List<string>();

            for (int i = 0; i < splitDefines.Length; i++)
            {
                string define = splitDefines[i].Trim();

                if (string.IsNullOrEmpty(define) || define == DefineSymbol)
                {
                    continue;
                }

                defines.Add(define);
            }

            if (enabled)
            {
                defines.Add(DefineSymbol);
            }

            return string.Join(";", defines);
        }
    }

    public sealed class CrossingLearsDefineRemovalProcessor : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (assetPath == CrossingLearsDefineBootstrap.PackageRoot)
            {
                CrossingLearsDefineBootstrap.RemoveDefine();
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
