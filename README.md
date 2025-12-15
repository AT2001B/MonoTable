## 🌟 MonoTable
一个轻量Unity插件;按规则自动收集Gameobject下MonoScripts，并生成访问代码。
在ui频繁变更时，不需要每次修改ui都去修改代码中Find的路径，只需要保证组件引用不变以及组件作用不变就可以完全不修改代码.

## 🚀 快速开始
### 安装/部署
# 1. 导入插件
下载下来并通过packageManager添加插件。

# 2. 自定义收集规则
示例代码如下，当然这里是给编辑器使用的，还需要在运行时调用AddPattern(),注入规则。
```csharp
[InitializeOnLoad]
public static class MonoTablePatternProvider
{
    static MonoTablePatternProvider()
    {
        AddPattern();
    }

    public static void AddPattern() {
        Pattern.AddPattern("@Txt_",typeof(Text));
    }

}
```
# 3. 添加UIGoTable，并编辑预制体
打开一个ui预制体为它添加UIGoTable,并将需要获取到的组件的Gameobject名字改为 @Txt_AAA;
如果我通过上述代码添加了收集规则，那么点击UIGoTable组件上的刷新后，可以在UIGoTable的Inspector上看到GoDic中对Image的引用，

# 4. 生成代码
在UIGoTable上也可以自定义ClassName 和Namespace 
点击生成代码后会自动生成如下代码

```csharp
using MonoTable;
public partial ClassName : ITable
{
    public static string _AssetPath { get => "资源路径";  set => throw new System.NotSupportedException("该属性禁止赋值"); }

    public static string _RootPath { get => "UIGoTable在预制体或场景中的路径";  set => throw new System.NotSupportedException("该属性禁止赋值"); }

    private UnityEngine.UI.Text AAA;

    public  void BindTable(UIGOTable goTable){

        AAA = goTable.GetGameObjectByKey("AAA").GetComponent<UnityEngine.UI.Text>();

    }
}
```
可以通过修改Editor->ProjectSetting->MonoTableSetting中的生成路径，修改自动生成代码的路径。

## 5.使用
在使用时你可以直接
```csharp
public partial class MainWindow 
{
    public void Start()
    {
        var table = ui.GetComponent<UIGOTable>();
        //调用自动生成的代码给字段赋值
        this.BindTable(table);
    }
    public void Show(){
      AAA.text ="xxxx";
    }
}
```


