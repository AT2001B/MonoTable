using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MonoTable { 
public interface ITable 
{
    static string _AssetPath { get; set; }
    void BindTable(UIGOTable goTable);
}
}