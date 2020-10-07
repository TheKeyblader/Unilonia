using TypeReferences;
using UnityEditor;
using UnityEngine;
using AvaloniaApplication = Avalonia.Application;
using Unilonia.Selectors;

namespace Unilonia.Settings
{
    public class UniloniaSettings : ScriptableObject
    {
        public const string SettingsPath = "Assets/Unilonia.Settings.asset";

        [InspectorName("Application Type")]
        [CustomInherits(typeof(AvaloniaApplication))]
        public TypeReference applicationType;

        public static UniloniaSettings Load()
        {
#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<UniloniaSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<UniloniaSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
#else
            return Resources.Load<UniloniaSettings>(SettingsPath);
#endif
        }
    }
}
