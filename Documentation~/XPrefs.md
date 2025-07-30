# XPrefs

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XPrefs 是一个灵活高效的配置系统，实现了多源化配置的读写，支持可视化编辑、变量求值和命令行参数覆盖等功能。

## 功能特性

- 多源化配置：支持内置配置（只读）、本地配置（可写）和远端配置（只读），支持多个配置源按优先级顺序读取
- 多数据类型：支持基础类型（整数、浮点数、布尔值、字符串）、数组类型及配置实例（IBase）
- 变量求值：支持通过命令行参数动态覆盖配置项，使用 ${Prefs.Key} 语法引用其他配置项
- 可视化编辑：支持通过自定义面板拓展可视化的配置编辑功能

## 使用手册

### 1. 基础配置操作

#### 1.1 检查配置项
```csharp
// 检查配置项是否存在
var exists = XPrefs.HasKey("configKey");
```

#### 1.2 读写基本类型
```csharp
// 写入配置
XPrefs.Local.Set("intKey", 42);
XPrefs.Local.Set("floatKey", 3.14f);
XPrefs.Local.Set("boolKey", true);
XPrefs.Local.Set("stringKey", "value");

// 读取配置
var intValue = XPrefs.GetInt("intKey", 0);
var floatValue = XPrefs.GetFloat("floatKey", 0f);
var boolValue = XPrefs.GetBool("boolKey", false);
var stringValue = XPrefs.GetString("stringKey", "");
```

#### 1.3 读写数组类型
```csharp
// 写入数组
XPrefs.Local.Set("intArray", new[] { 1, 2, 3 });
XPrefs.Local.Set("stringArray", new[] { "a", "b", "c" });

// 读取数组
var intArray = XPrefs.GetInts("intArray");
var stringArray = XPrefs.GetStrings("stringArray");
```

### 2. 配置源管理

#### 2.1 内置配置（只读）
```csharp
// 读取内置配置
var value = XPrefs.Asset.GetString("key");
```

#### 2.2 本地配置（可写）
```csharp
// 写入本地配置
XPrefs.Local.Set("key", "value");
XPrefs.Local.Save();

// 读取本地配置
var value = XPrefs.Local.GetString("key");
```

#### 2.3 远端配置（只读）
```csharp
// 实现远端配置处理器
public class RemoteHandler : XPrefs.IRemote.IHandler
{
    /// <summary>
    /// Uri 是远端的地址。
    /// </summary>
    public string Uri => "http://example.com/config";

    /// <summary>
    /// OnStarted 是流程启动的回调。
    /// </summary>
    /// <param name="context">上下文实例</param>
    public void OnStarted(XPrefs.IRemote context) { }
    
    /// <summary>
    /// OnRequest 是预请求的回调。
    /// </summary>
    /// <param name="context">上下文实例</param>
    /// <param name="request">HTTP 请求实例</param>
    public void OnRequest(XPrefs.IRemote context, UnityWebRequest request) { 
        request.timeout = 10;
    }

    /// <summary>
    /// OnRetry 是错误重试的回调。
    /// </summary>
    /// <param name="context">上下文实例</param>
    /// <param name="count">重试次数</param>
    /// <param name="pending">重试等待</param>
    /// <returns></returns>
    public bool OnRetry(XPrefs.IRemote context, int count, out float pending)
    {
        pending = 1.0f;
        return count < 3;
    }

    /// <summary>
    /// OnSucceeded 是请求成功的回调。
    /// </summary>
    /// <param name="context">上下文实例</param>
    public void OnSucceeded(XPrefs.IRemote context) { }

    /// <summary>
    /// OnFailed 是请求失败的回调。
    /// </summary>
    /// <param name="context">上下文实例</param>
    public void OnFailed(XPrefs.IRemote context) { }
}

// 读取远端配置
StartCoroutine(XPrefs.Remote.Read(new RemoteHandler()));
```

### 3. 变量求值

#### 3.1 基本用法
```csharp
// 设置配置项
XPrefs.Local.Set("name", "John");
XPrefs.Local.Set("greeting", "Hello ${Prefs.name}");

// 解析变量引用
var result = XPrefs.Local.Eval("${Prefs.greeting}"); // 输出: Hello John
```

#### 3.2 多级路径
```csharp
// 设置嵌套配置
XPrefs.Local.Set("user.name", "John");
XPrefs.Local.Set("user.age", 30);

// 使用多级路径引用
var result = XPrefs.Local.Eval("${Prefs.user.name} is ${Prefs.user.age}");
```

### 4. 命令行参数

#### 4.1 覆盖配置路径
```bash
--Prefs@Asset=path/to/asset.json    # 覆盖内置配置路径（仅支持编辑器环境）
--Prefs@Local=path/to/local.json    # 覆盖本地配置路径
```

#### 4.2 覆盖配置值
```bash
--Prefs@Asset.key=value             # 覆盖内置配置项
--Prefs@Local.key=value             # 覆盖本地配置项
--Prefs.key=value                   # 覆盖所有配置源
```

## 常见问题

### 1. 配置无法保存
- 检查配置对象是否可写（writeable = true）。
- 确认文件路径有效且具有写入权限。
- 验证是否调用了 Save() 方法。

### 2. 变量替换失败
- 确认变量引用格式正确（${Prefs.key}）。
- 检查引用的配置项是否存在。
- 注意避免循环引用和嵌套引用。

### 3. 远端配置加载失败
- 检查网络连接是否正常。
- 确认远端服务器地址正确。
- 验证超时和重试参数设置。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 