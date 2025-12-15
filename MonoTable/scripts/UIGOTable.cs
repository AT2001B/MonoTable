using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MonoTable {
    public class UIGOTable : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string nameSpace;
        [SerializeField]
        private string className;

        [SerializeField] private List<string> _keys = new List<string>();
        [SerializeField] private List<GameObject> _values = new List<GameObject>();
        [SerializeField]
        public Dictionary<string, GameObject> GoDataDic = new Dictionary<string, GameObject>();
        public GameObject GetGameObjectByKey(string key)
        {
            if (GoDataDic != null && GoDataDic.TryGetValue(key, out var value))
            {
                return value;
            }
            Debug.LogError($"UIGOTable中未找到Key为:{key}的go");
            return null;
        }

        public void BindGO()
        {

        }

        public void BindCS(System.Object cs)
        {

            Type type = cs.GetType();
            foreach (var data in GoDataDic)
            {

                FieldInfo info = type.GetField(Pattern.GetKeyByName(data.Value.name), BindingFlags.Instance | BindingFlags.NonPublic);
                if (info == null)
                {
                    continue;
                }
                if (type == typeof(GameObject))
                {
                    info.SetValue(cs, data.Value);
                }
                else
                {
                    info.SetValue(cs, data.Value.GetComponent(info.FieldType));
                }
            }

        }

        public string GetSharpNameSpace()
        {
            return nameSpace;
        }

        public string GetClassName()
        {
            if (!string.IsNullOrEmpty(className))
            {
                return className;
            }
            return Pattern.GetKeyByName(gameObject.name);
        }

        private void Reset()
        {
            Debug.Log($"{gameObject.name} 已添加 {GetType().Name} 组件");

            this.nameSpace = MonoTableSetting.Instance.DefaultNameSpace;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }


        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (var kvp in GoDataDic)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            GoDataDic.Clear();
            for (int i = 0; i < Mathf.Min(_keys.Count, _values.Count); i++)
            {
                GoDataDic[_keys[i]] = _values[i];
            }
        }
    }
}