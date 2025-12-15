using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.UI;

namespace MonoTable {
    public static class Pattern
    {
        public static Dictionary<string, Type> name2Type = new Dictionary<string, Type>() {
        { "@btn_",typeof(Button) },
        { "@img_",typeof(Image) },
        { "@rawImg_",typeof(RawImage) },
        { "@txt_",typeof(Text) },
        { "@slider_",typeof(Slider) },
        { "@table_",typeof(UIGOTable) },
        //{ "@list_",typeof(LoopListView2) },
    };
        public static void AddPattern(string key,Type type) {
            name2Type[key] = type;
            UnityEngine.Debug.Log($"MonoTable AddPattern {key}:{type}");
        }

        public static Type GetTypeByName(string name)
        {
            foreach (var kv in Pattern.name2Type)
            {
                if (name.StartsWith(kv.Key))
                {
                    return kv.Value;
                }
            }
            return null;
        }
        public static string GetKeyByName(string name)
        {
            if (name.StartsWith("@"))
            {
                foreach (var kv in Pattern.name2Type)
                {
                    if (name.StartsWith(kv.Key))
                    {
                        string key = name.Replace(kv.Key, "");
                        return key;
                    }
                }
            }
            return name;
        }
    }
}