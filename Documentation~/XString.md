# XString

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XString 是一个高效的字符串工具类，实现了字符串处理、数值转换、加密解密和变量求值等功能。

## 功能特性

- 字符串处理：提供字符串格式化（Format）、缩略（Omit）、简化（Simplify）等处理功能，支持自定义省略符号
- 数值转换：支持字节大小（B、KB、MB、GB、TB）、版本号（一到三段式）、向量（Vector3/Vector4）、颜色（RGBA）等数值的字符串转换
- 加密解密：支持 DES 加密算法的字符串和字节数组加密解密，支持自定义密钥
- 变量求值：支持字符串中变量的替换和求值，可通过接口扩展自定义求值规则

## 使用手册

### 1. 字符串格式化

#### 1.1 基本格式化
```csharp
// 单参数格式化
var result = XString.Format("Hello, {0}!", "World");  // 输出：Hello, World!

// 多参数格式化
var result = XString.Format("Hello, {0}! You have {1} new messages.", "Alice", 5);
// 输出：Hello, Alice! You have 5 new messages.

// 重复参数格式化
var result = XString.Format("Value: {0}, Again: {0}", 42);  // 输出：Value: 42, Again: 42

// 数字格式化
var result = XString.Format("Number: {0:0.00}", 42);  // 输出：Number: 42.00
var result = XString.Format("Large: {0:N0}", 1234567);  // 输出：Large: 1,234,567
```

### 2. 数值转换

#### 2.1 字节大小转换
```csharp
var size = XString.ToSize(1024);  // 输出：1 KB
var size = XString.ToSize(1024 * 1024);  // 输出：1 MB
var size = XString.ToSize(1500);  // 输出：1.46 KB
```

#### 2.2 版本号转换
```csharp
// 字符串转数字
long version = XString.ToVersion("1.2.3");  // 输出：100020003
long version = XString.ToVersion("1.1");  // 输出：10001

// 数字转字符串
var ver = XString.FromVersion(100020003);  // 输出：1.2.3
var ver = XString.FromVersion(10001);  // 输出：1.1
```

#### 2.3 向量转换
```csharp
// Vector3 转换
var vec = XString.ToVector3("(1,2,3)");  // 输出：(1, 2, 3)
var vecStr = XString.FromVector3(new Vector3(1.5f, -2.5f, 3.0f));  // 输出：(1.5,-2.5,3)

// Vector4 转换
var vec = XString.ToVector4("(1,2,3,4)");  // 输出：(1, 2, 3, 4)
var vecStr = XString.FromVector4(new Vector4(1.5f, -2.5f, 3.0f, 1.0f));  // 输出：(1.5,-2.5,3,1)
```

#### 2.4 颜色转换
```csharp
// 字符串转颜色
var color = XString.ToColor("FF0000FF");  // 不透明红色
var color = XString.ToColor("00FF00FF");  // 不透明绿色

// 颜色转字符串
var colorStr = XString.FromColor(Color.red);  // 输出：FF0000FF
var colorStr = XString.FromColor(new Color(0, 1, 0, 1));  // 输出：00FF00FF
```

### 3. 字符串处理

#### 3.1 字符串缩略
```csharp
// 基本缩略
var result = "Hello World".Omit(5);  // 输出：Hello..

// 自定义后缀
var result = "Hello World".Omit(5, "...");  // 输出：Hello...
```

#### 3.2 字符串简化
```csharp
// 基本简化
var result = "Hello World".Simplify(7);  // 输出：Hel...ld

// 长文本简化
var result = "This is a long text".Simplify(10);  // 输出：Thi...ext
```

### 4. 加密解密

#### 4.1 字符串加密
```csharp
// 默认加密
var encrypted = "Hello".Encrypt();
var decrypted = encrypted.Decrypt();  // 输出：Hello

// 带密钥加密
var encrypted = "Hello".Encrypt("12345678");  // 密钥必须是 8 字节
var decrypted = encrypted.Decrypt("12345678");  // 输出：Hello
```

#### 4.2 字节数组加密
```csharp
var data = Encoding.UTF8.GetBytes("Hello");
var encrypted = data.Encrypt();
var decrypted = encrypted.Decrypt();
```

### 5. 变量求值

#### 5.1 使用字典求值
```csharp
var dict = new Dictionary<string, string> { {"name", "World"} };
var result = "${name}".Eval(dict);  // 输出：World

// 多字典求值
var dict1 = new Dictionary<string, string> { {"name", "World"} };
var dict2 = new Dictionary<string, string> { {"greeting", "Hello"} };
var result = "${greeting} ${name}".Eval(dict1, dict2);  // 输出：Hello World
```

#### 5.2 自定义求值器
```csharp
public class ConfigEvaluator : XString.IEval 
{
    private Dictionary<string, string> configs;
    
    public string Eval(string input)
    {
        foreach (var config in configs)
        {
            input = input.Replace($"${{{config.Key}}}", config.Value);
        }
        return input;
    }
}

// 使用自定义求值器
var evaluator = new ConfigEvaluator();
var result = "Hello ${name}".Eval(evaluator);
```

## 常见问题

### 1. 格式化字符串失败
- 检查格式化字符串中的占位符数量是否与参数数量匹配。
- 确保使用的格式说明符与参数类型兼容。
- 注意特殊字符的转义。

### 2. 加密解密失败
- 确保密钥长度为 8 字节。
- 解密时使用与加密相同的密钥。
- 检查加密字符串是否为有效的 Base64 格式。

### 3. 向量转换失败
- 确保字符串格式正确，包括括号和逗号。
- Vector3 必须包含 3 个数值，Vector4 必须包含 4 个数值。
- 数值必须可以转换为浮点数。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 