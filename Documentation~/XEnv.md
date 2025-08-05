# XEnv

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XEnv 是一个环境配置管理工具，支持多平台识别、应用配置管理、路径管理、命令行参数解析和环境变量求值等功能。

## 功能特性

- 参数解析：支持多种参数形式和缓存管理
- 环境配置：支持应用类型、运行模式、版本等环境配置
- 变量求值：支持 ${Env.Key} 格式的环境变量引用和求值
- 路径管理：提供本地路径和资产路径的统一管理

## 使用手册

### 1. 平台环境

#### 1.1 获取平台类型
```csharp
// 获取当前运行平台
var platform = XEnv.Platform;
if (platform != XEnv.PlatformType.Unknown)
{
    // 根据平台类型执行相应逻辑
    switch (platform)
    {
        case XEnv.PlatformType.Windows:
            // Windows 平台特定处理
            break;
        case XEnv.PlatformType.Android:
            // Android 平台特定处理
            break;
    }
}
```

### 2. 应用配置

#### 2.1 获取应用类型
```csharp
// 获取当前应用类型
var appType = XEnv.App;
if (appType == XEnv.AppType.Client)
{
    // 客户端特定逻辑
}
```

#### 2.2 获取运行模式
```csharp
// 获取当前运行模式
var mode = XEnv.Mode;
if (mode == XEnv.ModeType.Dev)
{
    // 开发模式特定逻辑
}
```

#### 2.3 获取基础配置
```csharp
// 获取解决方案名称
var solution = XEnv.Solution;

// 获取项目名称
var project = XEnv.Project;

// 获取产品名称
var product = XEnv.Product;

// 获取发布渠道
var channel = XEnv.Channel;

// 获取版本号
var version = XEnv.Version;

// 获取作者名称
var author = XEnv.Author;
```

### 3. 路径管理

#### 3.1 获取项目路径
```csharp
// 获取项目根目录路径
var projectPath = XEnv.ProjectPath;
```

#### 3.2 获取数据目录
```csharp
// 获取数据存储目录路径
var localPath = XEnv.LocalPath;
```

#### 3.3 获取资源目录
```csharp
// 获取只读资源目录路径
var assetPath = XEnv.AssetPath;
```

### 4. 设备信息

#### 4.1 获取设备标识
```csharp
// 获取设备唯一标识符
var deviceId = XEnv.DeviceID;

// 获取设备 MAC 地址
var macAddr = XEnv.MacAddr;
```

### 5. 命令行参数

#### 5.1 解析命令行参数
```csharp
// 解析命令行参数
XEnv.ParseArgs(true, "--config=dev.json", "-debug");

// 获取参数值
var config = XEnv.GetArg("config"); // 返回 "dev.json"
var debug = XEnv.GetArg("debug");   // 返回 ""

// 获取所有参数
var args = XEnv.GetArgs();
foreach (var pair in args)
{
    Console.WriteLine($"{pair.Key}={pair.Value}");
}
```

### 6. 环境变量

#### 6.1 解析环境变量
```csharp
// 包含环境变量引用的字符串
var text = "Hello ${Env.UserName}!";

// 解析环境变量
var result = text.Eval(XEnv.Vars);
```

#### 6.2 内置变量列表

| 名称 | 说明 |
| :-: | :-: |
| LocalPath | 存储目录 |
| ProjectPath | 项目目录 |
| UserName | 系统用户 |
| Platform | 运行平台 |
| App | 应用类型 |
| Mode | 运行模式 |
| Solution | 解决方案 |
| Project | 项目名称 |
| Product | 产品名称 |
| Channel | 发布渠道 |
| Version | 应用版本 |
| Author | 作者名称 |
| Secret | 应用密钥 |
| NumCPU | CPU 核心数 |

## 常见问题

### 1. 路径获取问题
Q: 为什么在不同平台获取的 LocalPath 路径不同？
A: LocalPath 会根据运行平台返回适合的路径：
- 编辑器：返回项目下的 Local 目录
- Windows/Linux/macOS：返回应用程序同级的 Local 目录
- 移动平台：返回 persistentDataPath

### 2. 环境变量解析问题
Q: 环境变量解析时出现 (Unknown) 或 (Nested) 标记是什么意思？
A: 
- (Unknown)：表示引用的环境变量未定义
- (Nested)：表示存在嵌套的环境变量引用，不支持解析

### 3. 命令行参数问题
Q: 命令行参数支持哪些格式？
A: 支持以下格式：
- -key=value 或 --key=value
- -key value 或 --key value
- -flag 或 --flag（无值标志）

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 