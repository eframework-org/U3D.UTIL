# XFile

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XFile 提供了文件系统操作功能，支持文件和目录的基本操作、路径处理、压缩解压和文件校验。

## 功能特性

- 支持文件的基本操作：创建、读取、写入、删除和复制文件，支持文本和二进制内容
- 支持目录的基本操作：创建、复制、删除和遍历目录，支持递归操作和文件过滤
- 支持路径处理：合并路径、归一化处理、相对路径转换，支持特殊前缀（file://、jar:file://）
- 支持压缩和解压：ZIP 格式文件的压缩和解压，支持异步操作和进度回调
- 支持文件校验：计算文件的 MD5 值，支持文件完整性验证

## 使用手册

### 1. 文件操作

#### 1.1 读取文件
```csharp
// 读取文本文件
string content = XFile.OpenText("config.txt");

// 读取二进制文件
byte[] data = XFile.OpenFile("data.bin");

// 获取文件大小
long size = XFile.FileSize("file.dat");
```

#### 1.2 写入文件
```csharp
// 写入文本文件
XFile.SaveText("config.txt", "Hello World");

// 写入二进制文件
byte[] data = new byte[] { 1, 2, 3 };
XFile.SaveFile("data.bin", data);
```

#### 1.3 文件管理
```csharp
// 检查文件是否存在
if (XFile.HasFile("file.txt"))
{
    // 删除文件
    XFile.DeleteFile("file.txt");
}

// 复制文件
XFile.CopyFile("source.txt", "target.txt", true);
```

### 2. 目录操作

#### 2.1 目录管理
```csharp
// 创建目录
XFile.CreateDirectory("data");

// 检查目录是否存在
if (XFile.HasDirectory("logs"))
{
    // 删除目录（递归删除）
    XFile.DeleteDirectory("logs", true);
}
```

#### 2.2 目录复制
```csharp
// 复制目录（包含所有文件）
XFile.CopyDirectory("source", "target");

// 复制目录（排除特定文件）
XFile.CopyDirectory("source", "target", ".meta", ".tmp");
```

### 3. 路径处理

#### 3.1 路径合并
```csharp
// 合并多段路径
string path = XFile.PathJoin("root", "sub", "file.txt");
// 结果: root/sub/file.txt

// 处理带分隔符的路径
path = XFile.PathJoin("root/", "/sub/", "/file.txt");
// 结果: root/sub/file.txt
```

#### 3.2 路径归一化
```csharp
// 统一分隔符
string path = XFile.NormalizePath("root\\sub\\file.txt");
// 结果: root/sub/file.txt

// 处理特殊路径
path = XFile.NormalizePath("root/./sub/../file.txt");
// 结果: root/file.txt

// 处理特殊前缀
path = XFile.NormalizePath("file://root\\sub\\file.txt");
// 结果: file://root/sub/file.txt
```

### 4. 压缩解压

#### 4.1 压缩文件
```csharp
// 压缩目录
XFile.Zip("sourceDir", "target.zip");

// 压缩目录（排除特定文件）
var exclude = new List<string> { ".meta", ".tmp" };
XFile.Zip("sourceDir", "target.zip", exclude);
```

#### 4.2 解压文件
```csharp
// 解压文件（带进度和回调）
XFile.Unzip("source.zip", "targetDir",
    onComplete: () => { Console.WriteLine("解压完成"); },
    onError: (error) => { Console.WriteLine($"解压错误: {error}"); },
    onProgress: (progress) => { Console.WriteLine($"解压进度: {progress:P}"); }
);
```

### 5. 文件校验

#### 5.1 计算 MD5
```csharp
// 获取文件的 MD5 值
string md5 = XFile.FileMD5("file.dat");
if (!string.IsNullOrEmpty(md5))
{
    Console.WriteLine($"文件 MD5: {md5}");
}
```

## 常见问题

### 1. Android 平台文件访问
Q: 如何访问 Android 平台的 JAR 文件？
A: 对于以 "jar:file://" 开头的路径，XFile 会自动使用 Android 原生接口访问文件。

### 2. 路径分隔符
Q: 在不同平台上路径分隔符如何处理？
A: XFile 统一使用正斜杠（/）作为路径分隔符，会自动转换反斜杠（\\）。

### 3. 压缩文件大小限制
Q: 压缩大文件时出现内存不足？
A: Android 平台使用原生接口实现压缩解压，避免 SharpZipLib 的内存泄露问题。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 