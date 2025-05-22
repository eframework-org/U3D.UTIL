# XLog

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XLog 提供了一个遵循 RFC5424 标准的日志系统，支持多级别日志输出、多适配器管理、日志轮转和结构化标签等特性。

## 功能特性

- 支持 RFC5424 标准的 8 个日志级别
- 支持标准输出和文件存储两种适配器
- 支持日志文件的自动轮转和清理
- 支持异步写入和线程安全操作
- 支持结构化的日志标签系统

## 使用手册

### 1. 基础日志记录

#### 1.1 日志级别
```csharp
// 不同级别的日志记录（按严重程度从高到低排序）
XLog.Emergency("系统崩溃");    // 级别 0：系统不可用
XLog.Alert("需要立即处理");     // 级别 1：必须立即采取措施
XLog.Critical("严重错误");     // 级别 2：严重条件
XLog.Error("操作失败");        // 级别 3：错误条件
XLog.Warn("潜在问题");         // 级别 4：警告条件
XLog.Notice("重要信息");       // 级别 5：正常但重要的情况
XLog.Info("一般信息");         // 级别 6：信息消息
XLog.Debug("调试信息");        // 级别 7：调试级别消息

// 检查日志级别
var currentLevel = XLog.Level();
var canLog = XLog.Able(XLog.LevelType.Debug);
```

### 2. 日志配置

#### 2.1 文件输出配置

文件输出适配器支持以下配置项：

```csharp
var prefs = new XPrefs.IBase();
var fileConf = new XPrefs.IBase();

// 基础配置
fileConf.Set("Path", "${Env.LocalPath}/Log/app.log");  // 日志文件路径
fileConf.Set("Level", "Debug");                        // 日志级别

// 轮转配置
fileConf.Set("Rotate", true);        // 是否启用日志轮转
fileConf.Set("Daily", true);         // 是否按天轮转
fileConf.Set("MaxDay", 7);           // 日志文件保留天数
fileConf.Set("Hourly", false);       // 是否按小时轮转
fileConf.Set("MaxHour", 168);        // 日志文件保留小时数

// 文件限制
fileConf.Set("MaxFile", 100);        // 最大文件数量
fileConf.Set("MaxLine", 1000000);    // 单文件最大行数
fileConf.Set("MaxSize", 134217728);  // 单文件最大体积（128MB）

prefs.Set("Log/File", fileConf);
XLog.Setup(prefs);
```

#### 2.2 标准输出配置

标准输出适配器支持以下配置项：

```csharp
var stdConf = new XPrefs.IBase();

// 基础配置
stdConf.Set("Level", "Info");        // 日志级别
stdConf.Set("Color", true);          // 是否启用彩色输出

prefs.Set("Log/Std", stdConf);
XLog.Setup(prefs);
```

#### 2.3 配置说明

1. 日志级别控制：
   - 每个适配器可以独立配置日志级别
   - 可以通过 XLog.Level() 获取当前最大级别
   - 可以通过 LogTag 的 Level() 方法设置特定标签的日志级别
   - 示例：
     ```csharp
     var tag = XLog.GetTag();
     tag.Level(XLog.LevelType.Debug);  // 设置标签级别
     XLog.Debug(tag, "调试信息");       // 此日志会被输出
     XLog.Watch(tag);                  // 设置为上下文标签
     XLog.Debug("调试信息");            // 同样会被输出
     XLog.Defer();                     // 清除上下文标签
     ```

2. 文件轮转策略：
   - 按天轮转：每天创建新文件，自动清理超过 MaxDay 天数的文件
   - 按小时轮转：每小时创建新文件，自动清理超过 MaxHour 小时数的文件
   - 按大小轮转：当文件超过 MaxSize 时创建新文件
   - 按行数轮转：当文件超过 MaxLine 时创建新文件
   - 文件数量限制：通过 MaxFile 控制最大文件数

3. 日志文件命名规则：
   假设配置 Path 为 "./logs/app.log"：
   - 按天轮转：
     - 当前文件：app.log
     - 历史文件：app.2024-03-21.001.log, app.2024-03-21.002.log, ...
   - 按小时轮转：
     - 当前文件：app.log
     - 历史文件：app.2024-03-21-15.001.log, app.2024-03-21-15.002.log, ...
   - 按大小/行数轮转：
     - 当前文件：app.log
     - 历史文件：app.001.log, app.002.log, ...

4. 标准输出颜色：
   - Emergency: 黑色背景
   - Alert: 青色
   - Critical: 品红色
   - Error: 红色
   - Warn: 黄色
   - Notice: 绿色
   - Info: 灰色
   - Debug: 蓝色

### 3. 日志标签

#### 3.1 基本用法
```csharp
// 创建和使用标签
var tag = XLog.GetTag()
    .Set("module", "network")
    .Set("action", "connect")
    .Set("userId", "12345");

// 使用标签记录日志
XLog.Info(tag, "用户连接成功");

// 使用完后回收标签
XLog.PutTag(tag);
```

#### 3.2 上下文标签
```csharp
// 设置当前线程的标签
var tag = XLog.GetTag()
    .Set("threadId", "main")
    .Set("session", "abc123");
XLog.Watch(tag);

// 使用当前线程的标签
XLog.Info("处理请求");  // 自动带上上下文标签
XLog.Debug("详细信息"); // 同样带上上下文标签

// 清理上下文标签
XLog.Defer();
```

#### 3.3 标签格式化
```csharp
var tag = XLog.GetTag()
    .Set("module", "auth")
    .Set("userId", "12345")
    .Set("ip", "192.168.1.1");

// 输出格式：[时间] [级别] [module=auth userId=12345 ip=192.168.1.1] 消息内容
XLog.Info(tag, "用户登录成功");
```

### 4. 生命周期管理

#### 4.1 初始化
```csharp
// 自动初始化时机：
// 1. Unity 编辑器加载时
// 2. 运行时程序集加载后
// 3. 编辑器播放模式切换时

// 手动初始化（使用默认配置）
XLog.Setup(XPrefs.Asset);

// 手动初始化（使用自定义配置）
var prefs = new XPrefs.IBase();
// ... 配置适配器 ...
XLog.Setup(prefs);
```

#### 4.2 清理资源
```csharp
// 刷新所有适配器的缓冲区
XLog.Flush();

// 完全关闭日志系统
XLog.Close();
```

## 常见问题

### 1. 日志文件没有轮转？
检查以下配置：
- Rotate 是否设置为 true
- Daily/Hourly 是否正确设置
- MaxDay/MaxHour 是否合理设置
- MaxLine/MaxSize 是否达到触发条件

### 2. 文件日志性能问题？
- 合理设置日志级别，避免过多调试日志
- 使用异步写入模式
- 适当调整文件轮转参数
- 及时清理过期日志文件

### 3. 内存占用过高？
- 及时回收日志标签（使用 PutTag）
- 清理不再使用的上下文标签（使用 Defer）
- 合理设置缓冲区大小
- 定期执行日志清理

### 4. 日志丢失问题？
- 确保正确调用 Flush 和 Close
- 检查文件系统权限
- 验证磁盘空间是否充足
- 检查文件路径是否正确

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 