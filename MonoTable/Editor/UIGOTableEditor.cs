using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace MonoTable {
    [CustomEditor(typeof(UIGOTable))]
    public class UIGOTableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            UIGOTable table = (UIGOTable)this.serializedObject.targetObject;
            var nameSpaceProperty = serializedObject.FindProperty("nameSpace");
            var nameSpaceValue = EditorGUILayout.TextField(
                new GUIContent("Namespace", "设置自定义命名空间"),
                table.GetSharpNameSpace()
            );
            var classNameProperty = serializedObject.FindProperty("className");
            var classNameValue = EditorGUILayout.TextField(
                new GUIContent("ClassName", "设置自定义类名"),
                table.GetClassName()
            );
            if (GUI.changed)
            {
                nameSpaceProperty.stringValue = nameSpaceValue;
                classNameProperty.stringValue = classNameValue;
                serializedObject.ApplyModifiedProperties();
            }


            EditorGUILayout.LabelField("GoDic");
            EditorGUI.BeginDisabledGroup(true);


            foreach (var data in table.GoDataDic)
            {
                EditorGUILayout.ObjectField(data.Key, data.Value, typeof(GameObject), true);
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("刷新"))
            {
                string name = table.name;
                Dictionary<string, GameObject> dic = new Dictionary<string, GameObject>();
                FindChild(table.transform, dic);
                table.GoDataDic = dic;

                EditorUtility.SetDirty(table.gameObject);
                AssetDatabase.Refresh();

            }
            if (GUILayout.Button("生成cs文件"))
            {
                string scriptsName = table.GetClassName();
                string sharpNameSpace = table.GetSharpNameSpace();
                bool haveNameSpace = !string.IsNullOrEmpty(sharpNameSpace);
                var prefabPath = table.GetComponentResourcePath();
                var rootPath = GetGameObjectRootPath(table.gameObject);
                //根据go生成文件
                string realPath = Path.Combine(Application.dataPath, MonoTableSetting.Instance.GeneratePath);
                string unityPath = Path.Combine("Assets", MonoTableSetting.Instance.GeneratePath);
                if (string.IsNullOrEmpty(MonoTableSetting.Instance.GeneratePath))
                {
                    realPath = Application.dataPath;
                    unityPath = "Assets";
                }
                StringBuilder sb = new StringBuilder();
                int indent = 0;
                sb.AppendLineWithIndent(indent, "using MonoTable;");

                if (haveNameSpace)
                {
                    sb.AppendLineWithIndent(indent, $"namespace {sharpNameSpace}");
                    sb.AppendLineWithIndent(indent++, "{");
                }


                sb.AppendLineWithIndent(indent, $"public partial class {scriptsName} : ITable");
                sb.AppendLineWithIndent(indent++, "{");

                sb.AppendLineWithIndent(indent, $"public static string _AssetPath {{ get => \"{prefabPath}\";  set => throw new System.NotSupportedException(\"该属性禁止赋值\"); }}\r\n");
                sb.AppendLineWithIndent(indent, $"public static string _RootPath {{ get => \"{rootPath}\";  set => throw new System.NotSupportedException(\"该属性禁止赋值\"); }}\r\n");

                foreach (var data in table.GoDataDic)
                {
                    sb.AppendLineWithIndent(indent, $"private {Pattern.GetTypeByName(data.Value.name)} {data.Key};\r\n");
                }

                sb.AppendLineWithIndent(indent++, "public  void BindTable(UIGOTable goTable){\r\n");

                foreach (var data in table.GoDataDic)
                {
                    Type monoType = Pattern.GetTypeByName(data.Value.name);
                    if (monoType == typeof(GameObject))
                    {
                        sb.AppendLineWithIndent(indent, $"{data.Key} = goTable.GetGameObjectByKey(\"{data.Key}\");\r\n");
                    }
                    else
                    {
                        sb.AppendLineWithIndent(indent, $"{data.Key} = goTable.GetGameObjectByKey(\"{data.Key}\").GetComponent<{monoType}>();\r\n");
                    }
                }
                sb.AppendLineWithIndent(--indent, "}");

                sb.AppendLineWithIndent(--indent, "}");
                if (haveNameSpace)
                {

                    sb.AppendLineWithIndent(--indent, "}");
                }

                if (!Directory.Exists(realPath))
                {
                    Directory.CreateDirectory(realPath);
                }
                if (File.Exists(realPath + "/" + scriptsName + ".cs"))
                {
                    bool needPopWorn = true;
                    string scriptsPath = Path.Combine(unityPath, scriptsName + ".cs");
                    var MonoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptsPath);
                    var behaviour = MonoScript.GetClass();
                    if (typeof(ITable).IsAssignableFrom(behaviour))
                    {
                        var assetPathfiled = behaviour.GetProperty("_AssetPath", BindingFlags.Static | BindingFlags.Public);
                        var rootPathfiled = behaviour.GetProperty("_RootPath", BindingFlags.Static | BindingFlags.Public);
                        if (assetPathfiled.GetValue(null) is string oldAssetPath && oldAssetPath == prefabPath &&
                            rootPathfiled.GetValue(null) is string oldRootPath && rootPath == oldRootPath)
                        {
                            needPopWorn = false;
                        }
                    }

                    if (needPopWorn)
                    {
                        if (!EditorUtility.DisplayDialog("危险操作确认", scriptsPath + "目录下已有同名脚本，是否覆盖？", "确认覆盖", "取消"))
                        {
                            return;
                        }
                    }
                }
                FileStream fs = File.Create(realPath + "/" + scriptsName + ".cs");
                fs.Write(Encoding.UTF8.GetBytes(sb.ToString()));
                fs.Flush();
                fs.Close();
                AssetDatabase.Refresh();
            }
        }

        #region

        string GetGameObjectRootPath(GameObject go)
        {
            List<Transform> list = new List<Transform>();
            list.Add(go.transform);
            Transform tr = go.transform;
            while (tr.parent != null)
            {
                list.Add(tr.parent);
                tr = tr.parent;
            }
            list.Reverse();
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.Append("/" + item.name);
            }
            return sb.ToString();
        }

        private void FindChild(Transform tf, Dictionary<string, GameObject> dic)
        {
            for (int i = 0; i < tf.childCount; i++)
            {
                Transform child = tf.GetChild(i);
                bool isFindCild = true;
                if (child.name.StartsWith("@"))
                {
                    isFindCild = HandleGameObject(child.gameObject, dic);
                }
                if (isFindCild)
                {
                    FindChild(tf.GetChild(i), dic);
                }
            }
        }

        private bool HandleGameObject(GameObject go, Dictionary<string, GameObject> dic)
        {
            string name = go.name;
            foreach (var kv in Pattern.name2Type)
            {
                if (name.StartsWith(kv.Key))
                {

                    string key = name.Replace(kv.Key, "");
                    dic.Add(key, go);

                    if (kv.Value == typeof(UIGOTable))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// 根据命名空间 + 类名获取 Type 类型
        /// </summary>
        /// <param name="namespaceName">命名空间（如 "UnityEngine.UI"）</param>
        /// <param name="className">类名（如 "Button"）</param>
        /// <param name="assemblyName">可选：程序集名称（如 "UnityEngine.UI"，不含.dll）</param>
        /// <returns>对应的 Type，找不到返回 null</returns>
        public Type GetTypeByNamespaceAndClassName(string namespaceName, string className, string assemblyName = null)
        {
            // 1. 拼接完整类名（命名空间.类名）
            string fullClassName = string.IsNullOrEmpty(namespaceName)
                ? className
                : $"{namespaceName}.{className}";

            // 2. 优先尝试当前程序集/核心程序集查找
            Type type = Type.GetType(fullClassName);
            if (type != null)
                return type;

            // 3. 指定了程序集名称 → 带程序集查找
            if (!string.IsNullOrEmpty(assemblyName))
            {
                fullClassName = $"{fullClassName}, {assemblyName}";
                type = Type.GetType(fullClassName);
                if (type != null)
                    return type;
            }

            // 4. 遍历所有已加载程序集查找（兜底方案）
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullClassName);
                if (type != null)
                    return type;
            }

            // 5. 查找失败（大小写敏感，需严格匹配）
            Debug.LogError($"未找到类型：{fullClassName}（程序集：{assemblyName ?? "未指定"}）");
            return null;
        }

        /// <summary>
        /// 简化重载：直接传入完整类名（命名空间.类名）
        /// </summary>
        public Type GetTypeByFullName(string fullClassName, string assemblyName = null)
        {
            if (string.IsNullOrEmpty(fullClassName))
                return null;

            // 拆分命名空间和类名（兼容无命名空间的情况）
            string[] parts = fullClassName.Split('.');
            if (parts.Length == 1)
                return GetTypeByNamespaceAndClassName("", parts[0], assemblyName);

            string className = parts[^1];
            string namespaceName = string.Join(".", parts.Take(parts.Length - 1));
            return GetTypeByNamespaceAndClassName(namespaceName, className, assemblyName);
        }

        #endregion
    }
}