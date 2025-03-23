# XLoom

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)

XLoom 提供了一个统一的协程、线程和定时器管理工具，支持多线程任务调度、定时任务执行和协程管理。

## 功能特性

- 支持协程的创建、运行和停止
- 支持定时器的创建、管理和清除
- 支持多线程任务的调度和执行

## 使用手册

### 1. 协程管理

#### 1.1 启动协程
```csharp
// 创建并启动一个协程
IEnumerator YourCoroutine()
{
    yield return new WaitForSeconds(0.1f);
    // 执行协程逻辑
}

// 启动协程并获取句柄
Coroutine cr = XLoom.StartCR(YourCoroutine());
```

#### 1.2 停止协程
```csharp
// 使用协程句柄停止
XLoom.StopCR(cr);

// 或使用协程迭代器停止
XLoom.StopCR(YourCoroutine());
```

### 2. 定时器管理

#### 2.1 一次性定时器
```csharp
// 设置一次性定时器，3 秒后执行
var timer = XLoom.SetTimeout(() => 
{
    Debug.Log("3 秒后执行");
}, 3000);

// 取消定时器
XLoom.ClearTimeout(timer);
```

#### 2.2 重复定时器
```csharp
// 设置重复定时器，每 3 秒执行一次
var timer = XLoom.SetInterval(() => 
{
    Debug.Log("每 3 秒执行一次");
}, 3000);

// 取消定时器
XLoom.ClearInterval(timer);
```

### 3. 线程调度

#### 3.1 主线程执行
```csharp
// 在主线程中执行任务
await XLoom.RunInMain(() => 
{
    // 需要在主线程执行的逻辑
    Debug.Log("在主线程中执行");
});
```

#### 3.2 主线程检查
```csharp
// 检查当前是否在主线程中执行
bool isMainThread = XLoom.IsInMain();
```

#### 3.3 下一帧执行
```csharp
// 在主线程的下一帧执行任务
await XLoom.RunInNext(() => 
{
    // 将在下一帧执行的逻辑
    Debug.Log("在下一帧执行");
});
```

#### 3.4 异步执行
```csharp
// 在其他线程中异步执行任务
await XLoom.RunAsync(() => 
{
    // 异步执行的逻辑
    Debug.Log("在其他线程执行");
});
```

## 常见问题

### 1. 协程无法启动
- 检查是否在主线程中调用 StartCR
- 确保在 Play 模式下运行
- 验证 XLoom 实例是否正确初始化

### 2. 定时器未按预期执行
- 检查定时器时间单位是否正确（使用毫秒）
- 确保定时器未被提前清除
- 验证回调函数是否正确设置

### 3. 线程调度问题
- 确保 UI 相关操作在主线程中执行
- 避免在异步任务中直接操作 Unity 对象
- 使用 RunInMain 确保在主线程中执行必要操作

### 4. 内存管理问题
- 及时清除不再使用的定时器
- 停止不需要的协程
- 避免创建过多的定时器实例

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 