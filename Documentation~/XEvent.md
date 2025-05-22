# XEvent

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XEvent 是一个轻量级的事件管理器，支持多重监听、单次及泛型回调和批量通知等功能。

## 功能特性

- 多重监听：可配置是否允许同一事件注册多个回调
- 单次回调：可设置回调函数仅执行一次后自动注销
- 泛型回调：支持无参数、单参数和多参数的事件回调

## 使用手册

### 1. 创建事件管理器

#### 1.1 创建多重监听管理器
```csharp
// 创建支持多重监听的事件管理器
var eventManager = new XEvent.Manager(true);

// 注册多个回调
eventManager.Reg(1, (args) => Console.WriteLine("First"));
eventManager.Reg(1, (args) => Console.WriteLine("Second"));
```

#### 1.2 创建单一监听管理器
```csharp
// 创建单一监听的事件管理器
var singleManager = new XEvent.Manager(false);

// 注册回调，第二次注册会失败
singleManager.Reg(1, (args) => Console.WriteLine("Only One"));
```

### 2. 注册事件回调

#### 2.1 注册普通回调
```csharp
// 注册无参数回调
eventManager.Reg(1, () => Console.WriteLine("Event Triggered"));

// 注册带参数回调
eventManager.Reg<string>(2, (msg) => Console.WriteLine(msg));
eventManager.Reg<int, string>(3, (id, name) => Console.WriteLine($"{id}: {name}"));
```

#### 2.2 注册单次回调
```csharp
// 注册单次回调，执行后自动注销
eventManager.Reg(1, (args) => Console.WriteLine("Once"), true);
```

### 3. 通知事件

#### 3.1 无参数通知
```csharp
// 触发事件，不传递参数
eventManager.Notify(1);
```

#### 3.2 带参数通知
```csharp
// 触发事件，传递参数
eventManager.Notify(2, "Hello World");
eventManager.Notify(3, 1, "User");
```

### 4. 管理事件回调

#### 4.1 注销指定回调
```csharp
// 注销特定事件的指定回调
void callback(object[] args) { }
eventManager.Reg(1, callback);
eventManager.Unreg(1, callback);
```

#### 4.2 注销所有回调
```csharp
// 注销特定事件的所有回调
eventManager.Unreg(1);

// 清除所有事件的所有回调
eventManager.Clear();
```

## 常见问题

### 1. 多重监听失败
Q: 为什么无法注册多个回调？
A: 检查事件管理器是否以单一监听模式创建（multiple = false）。单一监听模式下每个事件只能注册一个回调。

### 2. 回调执行顺序
Q: 多个回调的执行顺序是怎样的？
A: 回调按照注册顺序依次执行。后注册的回调后执行。

### 3. 单次回调问题
Q: 单次回调什么时候被注销？
A: 单次回调在首次执行后自动注销，不需要手动注销。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 