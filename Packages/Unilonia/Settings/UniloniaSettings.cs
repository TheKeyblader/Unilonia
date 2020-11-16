using TypeReferences;
using UnityEditor;
using UnityEngine;
using AvaloniaApplication = Avalonia.Application;
using Unilonia.Selectors;
using UnityEditor.Compilation;

namespace Unilonia.Settings
{
    public class UniloniaSettings : ScriptableObject
    {
        public const string SettingsPath = "Assets/Unilonia.Settings.asset";

        [InspectorName("Application Type")]
        [CustomInherits(typeof(AvaloniaApplication))]
        public TypeReference applicationType;

        [InspectorName("Use deferred rendering")]
        public bool useDeferredRendering = true;

        public static UniloniaSettings Load()
        {
#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<UniloniaSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<UniloniaSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
#else
            return Resources.Load<UniloniaSettings>(SettingsPath);
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (AvaloniaApplication.Current != null && AvaloniaApplication.Current.GetType() != applicationType.Type)
                CompilationPipeline.RequestScriptCompilation();
        }
#endif
    }
}
