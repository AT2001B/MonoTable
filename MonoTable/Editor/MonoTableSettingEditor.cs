using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MonoTable;
namespace MonoTableEditor
{
    public class MonoTableSettingProvider : SettingsProvider
    {
        private MonoTableSetting settings;

        // 样式定义
        private readonly GUIContent pathLabel = new GUIContent("生成路径位置", "goTable的类生成路径");
        private readonly GUIContent namespaceLabel = new GUIContent("默认命名空间", "table初始化时赋值");

        public MonoTableSettingProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // 加载配置
            settings = MonoTableSetting.Instance;
        }

        public override void OnGUI(string searchContext)
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox("配置文件不存在！请创建新配置。", MessageType.Warning);
                EditorGUILayout.Space();

                // 创建配置按钮
                if (GUILayout.Button("创建默认配置", GUILayout.Height(30)))
                {
                    CreateNewSettings();
                    LoadSettings(); // 重新加载配置
                }
                return;
            }

            EditorGUI.BeginChangeCheck();

            // 绘制设置界面
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("MonoTable设置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 字符串设置
            settings.GeneratePath = EditorGUILayout.TextField(pathLabel, settings.GeneratePath);
            settings.DefaultNameSpace = EditorGUILayout.TextField(namespaceLabel, settings.DefaultNameSpace);


            // 额外设置示例
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("高级设置", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField("配置文件路径", AssetDatabase.GetAssetPath(settings));
            }

            // 保存修改
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
        private void LoadSettings()
        {
            // 先通过AssetDatabase查找现有配置
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:MonoTableSetting");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<MonoTableSetting>(path);
            }
            else
            {
                settings = null;
            }
#endif
        }
        private void CreateNewSettings()
        {
#if UNITY_EDITOR
            // 确保目录存在
            string directory = Path.GetDirectoryName(settingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // 创建新的配置资源
            settings = ScriptableObject.CreateInstance<MonoTableSetting>();

            // 设置默认值

            // 保存资源到指定路径
            AssetDatabase.CreateAsset(settings, settingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("创建成功", $"配置文件已创建：\n{settingsPath}", "确定");
#endif
        }

        // 注册到Project Settings
        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider()
        {
            var provider = new MonoTableSettingProvider("Project/MonoTable Settings", SettingsScope.Project);

            // 添加关键词支持搜索
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<MonoTableSettingProvider>();

            return provider;
        }
    }
}