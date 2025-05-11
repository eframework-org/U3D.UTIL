# XPool

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)  
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XPool 提供了一个对象缓存工具集，实现了基础对象池、Unity 游戏对象池和字节流缓冲池。

## 功能特性

- 支持基础对象缓存：提供线程安全的泛型对象池，支持自动创建和复用对象
- 支持游戏对象缓存：提供场景级和全局级的预制体实例池，支持自动回收和复用
- 支持字节流缓存：提供高性能的字节缓冲池，支持自动扩容和复用
- 线程安全设计：所有缓存操作都是线程安全的，支持多线程并发访问

## 使用手册

### 1. 基础对象缓存

#### 1.1 泛型对象池
```csharp
// 获取对象
var obj = XPool.SObject<List<int>>.Get();
obj.Add(1);

// 回收对象
XPool.SObject<List<int>>.Put(obj);

// 对象会被自动复用
var obj2 = XPool.SObject<List<int>>.Get();
Assert.That(obj2, Is.SameAs(obj));  // true
```

#### 1.2 非泛型对象池
```csharp
// 使用类型创建对象池
var pool = new XPool.SObject(typeof(List<int>));

// 使用委托创建对象池
var pool2 = new XPool.SObject(() => new List<int>());

// 获取和回收对象
var obj = pool.Get();
pool.Put(obj);
```

### 2. 游戏对象缓存

#### 2.1 设置预制体
```csharp
// 注册预制体（场景级）
var prefab = new GameObject("TestPrefab");
XPool.GObject.Set("test_prefab", prefab);

// 注册预制体（全局级）
XPool.GObject.Set("global_prefab", prefab, XPool.GObject.CacheType.Global);

// 检查预制体是否存在
bool exists = XPool.GObject.Has("test_prefab");
```

#### 2.2 获取实例
```csharp
// 基本实例化
var obj = XPool.GObject.Get("test_prefab");

// 带参数实例化
var obj2 = XPool.GObject.Get("test_prefab", 
    active: true,                         // 是否激活
    position: new Vector3(0, 1, 0),      // 世界坐标
    rotation: Quaternion.identity,        // 世界朝向
    scale: new Vector3(1, 1, 1),         // 本地缩放
    life: 1000);                         // 生命周期（毫秒）
```

#### 2.3 回收实例
```csharp
// 立即回收
XPool.GObject.Put(obj);

// 延迟回收
XPool.GObject.Put(obj, delay: 1000);  // 1 秒后回收
```

#### 2.4 移除预制体
```csharp
// 移除预制体及其所有实例
XPool.GObject.Del("test_prefab");
```

### 3. 字节流缓存

#### 3.1 获取缓冲区
```csharp
// 创建指定大小的缓冲区
var buffer = XPool.SBuffer.Get(1024);

// 写入数据
buffer.Writer.Write(new byte[] { 1, 2, 3, 4 });
buffer.Flush();  // 更新长度并重置位置

// 读取数据
var data = buffer.ToArray();
```

#### 3.2 复制数据
```csharp
// 创建目标数组
var dst = new byte[1024];

// 复制数据
buffer.CopyTo(srcOffset: 0, dst, dstOffset: 0, count: 1024);
```

#### 3.3 回收缓冲区
```csharp
// 回收到缓冲池
XPool.SBuffer.Put(buffer);

// 释放资源
buffer.Dispose();
```

#### 3.4 缓冲区长度说明
- Length 表示有效数据长度，而不是底层数组容量
- 写入数据后必须调用 Flush() 更新 Length
- Reset() 会将 Length 重置为 -1
- 使用 ToArray() 时以 Length 为准截取数据

#### 3.5 字节流缓冲池机制
- Get() 方法会优先查找大于等于请求大小的缓存对象
- Put() 方法仅缓存小于 60KB 的对象
- 当池满时（500个），会释放最早缓存的对象
- 使用完毕后应调用 Put() 而不是 Dispose()

## 常见问题

### 1. 对象池容量限制
**问题**：对象池中的对象数量不断增长。

**解决方案**：
- 基础对象池限制为 500 个对象
- 超出限制的对象将被丢弃而不是缓存
- 建议在使用高峰期监控对象池大小

### 2. 游戏对象回收异常
**问题**：回收的游戏对象在场景切换后丢失。

**解决方案**：
- 场景级缓存（默认）的对象会在场景卸载时自动清理
- 需要跨场景保留的对象应使用全局级缓存
- 确保不要重复回收同一个对象

### 3. 字节流缓存限制
**问题**：大容量的字节流无法被缓存。

**解决方案**：
- 单个缓冲区限制为 60KB
- 缓冲池容量限制为 500 个实例
- 超出限制的缓冲区将被直接释放
- 建议对大容量数据使用流式处理

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 