using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MonoTable {
    public class MonoTableSetting : ScriptableObject
    {
        [Header("基础设置")]
        public string DefaultNameSpace = "";
        public string GeneratePath = "";
        public bool featureToggle = false;

        private static MonoTableSetting _instance;
        public static MonoTableSetting Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    // 编辑器下通过AssetDatabase查找
                    string[] guids = AssetDatabase.FindAssets("t:MonoTableSetting");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _instance = AssetDatabase.LoadAssetAtPath<MonoTableSetting>(path);
                    }
#else
                // 运行时通过Addressables或其他方式加载（可选）
                //_instance = Resources.Load<CustomProjectSettings>("CustomProjectSettings");
#endif

                    // 如果没找到则创建新实例
                    if (_instance == null)
                    {
                        _instance = CreateInstance<MonoTableSetting>();
#if UNITY_EDITOR
                        // 编辑器下自动保存到指定目录
                        string savePath = "Assets/Settings/CustomProjectSettings.asset";
                        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                        {
                            AssetDatabase.CreateFolder("Assets", "Settings");
                        }
                        AssetDatabase.CreateAsset(_instance, savePath);
                        AssetDatabase.SaveAssets();
#endif
                    }
                }
                return _instance;
            }
        }
    }
}