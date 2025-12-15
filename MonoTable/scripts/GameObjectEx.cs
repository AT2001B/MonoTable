using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
namespace MonoTable { 
public static class ComponentPathHelper
{
    /// <summary>
    /// 获取Component所属资源的文件路径（场景/Prefab）
    /// - 场景内对象 → 返回.scene文件路径（如 "Assets/Scenes/Game.unity"）
    /// - Prefab实例 → 返回源Prefab文件路径（如 "Assets/Prefabs/Player.prefab"）
    /// - Prefab预制体 → 返回自身.prefab文件路径
    /// - 临时对象 → 返回空字符串
    /// </summary>
    /// <param name="component">目标组件</param>
    /// <returns>资源文件路径（空字符串=无对应资源）</returns>
    public static string GetComponentResourcePath(this Component component)
    {
        if (component == null) return string.Empty;

        GameObject targetObj = component.gameObject;
        string assetPath = string.Empty;

        // ========== 1. Prefab相关：优先获取Prefab文件路径 ==========
        if (IsPrefabAsset(targetObj))
        {
            // Prefab预制体本身 → 直接获取自身路径
            assetPath = AssetDatabase.GetAssetPath(targetObj);
        }
        else if (IsPrefabInstance(targetObj))
        {
            // 场景中的Prefab实例 → 获取源Prefab路径
            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(targetObj);
            assetPath = prefabSource != null ? AssetDatabase.GetAssetPath(prefabSource) : string.Empty;
        }
        // ========== 2. 场景内对象：获取.scene文件路径 ==========
        else if (IsValidSceneObject(targetObj))
        {
            assetPath = GetSceneFilePath(targetObj.scene);
        }
        // ========== 3. 临时对象：无路径 ==========
        else
        {
            Debug.LogWarning($"[{targetObj.name}] 是临时对象（非Prefab/非场景内对象），无资源路径");
        }

        // 格式化路径（去除多余前缀，保证统一格式）
        return string.IsNullOrEmpty(assetPath) ? string.Empty : FormatAssetPath(assetPath);
    }

    /// <summary>
    /// 获取MonoBehaviour对应的脚本文件路径（.cs文件）
    /// </summary>
    public static string GetComponentScriptPath(MonoBehaviour mono)
    {
        if (mono == null) return string.Empty;
        MonoScript script = MonoScript.FromMonoBehaviour(mono);
        return script == null ? string.Empty : FormatAssetPath(AssetDatabase.GetAssetPath(script));
    }

    // ---------------- 私有辅助方法 ----------------
    /// <summary>
    /// 判断是否是Prefab预制体本身
    /// </summary>
    private static bool IsPrefabAsset(GameObject obj)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.IsPartOfPrefabAsset(obj);
#else
        return PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab;
#endif
    }

    /// <summary>
    /// 判断是否是场景中的Prefab实例
    /// </summary>
    private static bool IsPrefabInstance(GameObject obj)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.IsPartOfPrefabInstance(obj);
#else
        return PrefabUtility.GetPrefabType(obj) == PrefabType.PrefabInstance;
#endif
    }

    /// <summary>
    /// 判断是否是有效场景内对象（非Prefab、非临时）
    /// </summary>
    private static bool IsValidSceneObject(GameObject obj)
    {
        if (obj == null || !obj.scene.IsValid()) return false;
        if (obj.scene.name == "DontDestroyOnLoad") return false; // 排除临时场景
        return !EditorUtility.IsPersistent(obj) && obj.hideFlags == HideFlags.None;
    }

    /// <summary>
    /// 核心：正确获取场景文件路径（解决Scene结构体路径为空问题）
    /// </summary>
    private static string GetSceneFilePath(Scene scene)
    {
        // 方案3：遍历所有.scene文件
        string[] allSceneGUIDs = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in allSceneGUIDs)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName.Equals(scene.name, System.StringComparison.OrdinalIgnoreCase))
            {
                return scenePath;
            }
        }

        Debug.LogWarning($"场景[{scene.name}]未找到对应.scene文件");
        return string.Empty;
    }

    /// <summary>
    /// 格式化资源路径（统一格式，去除"Library/unity cache"等无效前缀）
    /// </summary>
    private static string FormatAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        // 确保路径以"Assets/"开头（Unity标准资源路径格式）
        return path.StartsWith("Assets/") ? path : string.Empty;
    }
}
}