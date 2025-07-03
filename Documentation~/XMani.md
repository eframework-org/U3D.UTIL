# XMani

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XMani 提供了一个文件清单管理工具，支持文件清单的生成、解析、对比等功能。

## 功能特性

- 支持文件清单的生成和解析
- 支持本地和远端清单的读取
- 支持文件清单的差异对比

## 使用手册

### 1. 基础用法

#### 1.1 创建清单
```csharp
// 创建一个新的清单实例
var manifest = new XMani.Manifest();

// 或指定清单路径创建
var manifest = new XMani.Manifest("path/to/manifest/file");
```

#### 1.2 保存清单
```csharp
// 创建清单实例
var manifest = new XMani.Manifest();

// 添加文件信息
manifest.Files.Add(new XMani.FileInfo 
{ 
    Name = "file.txt", 
    MD5 = "md5_value", 
    Size = 100 
});

// 将清单格式化为文本
var text = manifest.ToString();

// 将清单持久化至文件
XFile.SaveText("path/to/manifest/file", text);
```

#### 1.3 解析清单
```csharp
// 解析清单文本
var data = "file1.txt|d41d8cd98f00b204e9800998ecf8427e|0\n" +
           "file2.txt|d41d8cd98f00b204e9800998ecf8427e|123";

var succeeded = manifest.Parse(data, out string error);

// 检查解析结果
if (succeeded)
{
    foreach (var file in manifest.Files)
    {
        Debug.Log($"文件：{file.Name}，MD5：{file.MD5}，大小：{file.Size}");
    }
}
else
{
    Debug.LogError($"解析失败：{error}");
}
```

### 2. 清单读取

#### 2.1 本地文件
```csharp
// 从本地文件读取清单
// 对称密钥为：12345678
var handler = manifest.Read(uri: "path/to/manifest/file", secret: "12345678");
while (!handler()) { } // 等待读取完成

if (string.IsNullOrEmpty(manifest.Error))
{
    Debug.Log($"读取成功，包含 {manifest.Files.Count} 个文件");
}
```

#### 2.2 远端文件
```csharp
// 从远端 URL 读取清单
// 对称密钥为：12345678
// 设置预请求回调
var handler = manifest.Read(uri: "http://example.com/Manifest.db", secret: "12345678", onPreRequest: req =>
{
    req.timeout = 10;
});
while (!handler()) { } // 等待读取完成

if (string.IsNullOrEmpty(manifest.Error))
{
    Debug.Log($"读取成功，包含 {manifest.Files.Count} 个文件");
}
```

### 3. 清单对比

```csharp
// 准备清单
var manifest1 = new XMani.Manifest();
manifest1.Files.Add(new XMani.FileInfo 
{ 
    Name = "file1.txt", 
    MD5 = "md5_1", 
    Size = 100 
});

var manifest2 = new XMani.Manifest();
manifest2.Files.Add(new XMani.FileInfo 
{ 
    Name = "file2.txt", 
    MD5 = "md5_2", 
    Size = 200 
});

// 执行对比
var diff = manifest1.Compare(manifest2);

// 检查差异
Debug.Log($"新增：{diff.Added.Count} 个文件");
Debug.Log($"修改：{diff.Modified.Count} 个文件");
Debug.Log($"删除：{diff.Deleted.Count} 个文件");
```

## 常见问题

### 1. 清单解析失败
- 检查清单文件格式是否正确（文件名|MD5值|文件大小）
- 确保文件编码为 UTF-8 文本内容
- 确定加密清单内容密钥的正确性
- 验证文件大小值是否为有效数字

### 2. 远端读取超时
- 检查网络连接是否正常
- 适当增加超时时间
- 确保 URL 可以正常访问

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 