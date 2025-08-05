# 更新记录

## [0.1.8] - 
### 变更
- 修改 XPrefs.IPanel 接口的 OnActivate、OnSave、OnApply 等函数的签名
- 修改 XEnv、XLog 模块对新版 XPrefs.IPanel 接口的适配
- 修改 XComp 若干函数的名称：CloneGO -> CloneGameObject、DestroyGO -> DestroyGameObject、SetActiveState -> SetGameObjectActive
- 修改 XLoom 若干函数的名称：StartCR -> StartCoroutine、StopCR -> StopCoroutine
- 修改 XString 的 IEval 接口的名称为 IEvaluator

## [0.1.7] - 2025-07-07
### 变更
- 优化 XFile.FileMD5 函数，支持分段采样模式，提高大文件哈希计算速度

## [0.1.6] - 2025-07-07
### 变更
- 新增 XMani.Parse 解析清单内容的加密功能（secret）
- 新增 XMani.Read 读取远端清单的预请求功能（onPreRequest）
- 新增 XPrefs.IRemote.IHandler 接口的 OnStarted 函数（原 OnRequest 函数）
- 修改 XPrefs.IRemote.IHandler 的 OnRequest 函数签名（增加了 UnityWebRequest 参数）
- 新增 EFRAMEWORK_PREFERENCES_INSECURE 宏定义用于控制是否启用 Staging/Prod 环境的首选项覆盖功能

### 修复
- 修复 XLog 标准适配器输出异常堆栈的 HyperLink 显示问题

## [0.1.5] - 2025-06-25
### 修复
- 修复 XLog 文件适配器潜在的轮转异常问题

### 变更
- 修改 XLog 在编辑器环境下调用 Close 的时机：compilationStarted -> beforeAssemblyReload

## [0.1.4] - 2025-06-24
### 修复
- 修复 XLog 调用 Unity 内置的 LogFormat 时格式化参数异常问题

## [0.1.3] - 2025-06-23
### 变更
- 重构 XLog 文件适配器的 Setup、Flush、Close 函数实现

## [0.1.2] - 2025-06-23
### 变更
- 修改 XLog 文件适配器，支持捕捉所有 Debug.Log* 的日志

### 修复
- 修复应用退出时 XLoom.StopCoroutine 因脚本执行时序抛出的空异常
- 修复潜在的 XPrefs.IAsset/ILocal 环境变量覆盖的安全问题
- 修复 XLog 文件适配器未读取文件的行数及大小导致的轮转错误问题
- 修复 XLog 文件适配器潜在的多线程并发写入及文件释放问题

## [0.1.1] - 2025-06-17
### 变更
- 抑制 XPool.SObject 调试日志的输出
- 修改 XLog 日志标签的格式为紧凑型
- 修改 XPrefs.ILocal 在 Dev/Test 模式下不进行加密

## [0.1.0] - 2025-06-12
### 变更
- 移除 XPool.GObject 不必要的 lock (instance)
- 修改 XPool.GObject 删除游戏对象为立即执行（DestroyImmediate）
- 优化 XPrefs 保存的键值对顺序，按照字母表排序，提高文本可读性

### 修复
- 修复 XPrefs.Read 多次调用时未清空旧的键值对的问题

## [0.0.9] - 2025-06-11
### 修复
- 修复 XLog 在未初始化时调用 XLog.* 函数 Std 适配器无法输出的问题
- 校正若干代码的日志输出信息

## [0.0.8] - 2025-06-07
### 变更
- 重构 XMani.Default 的值为 Manifest.db
- 公开 XMani.Default 字段，使得业务层可以自定义文件名
- 完善 XPool.GObject 模块的说明文档

## [0.0.7] - 2025-06-05
### 变更
- 优化 XPool.GObject 的单元测试

### 修复
- 修复 XFile.FileMD5 潜在的资源占用及释放问题
- 修复 XLoom.Timeout/Interval 的 Tick 精度及错误的单元测试

### 新增
- 新增 XPool.GObject.OnSet 钩子函数用于自定义缓存逻辑

## [0.0.6] - 2025-05-28
### 修复
- 修复多线程环境下访问 XEnv.ProjectPath 引发的异常

### 新增
- 新增 ${Env.NumCPU} 变量用于引用求值并完善了 XEnv.Eval 的单元测试

## [0.0.5] - 2025-05-13
### 修复
- 修复 XLog 编辑器模式下文件占用的问题
- 修复 XLoom 编辑器模式下定时器更新异常

## [0.0.4] - 2025-05-11
### 变更
- 修改 XLog 模块的日志标签格式为：[key1=value1, ...]

### 修复
- 修复 XLog 编辑器模式下文件占用的问题
- 修复 XEvent 单次回调潜在的内存泄漏问题

### 新增
- 新增 [DeepWiki](https://deepwiki.com) 智能索引，方便开发者快速查找相关文档

## [0.0.3] - 2025-03-31
### 修复
- 修复 Puer 接口适配问题

### 变更
- 修改 PlatformType.OSX 的枚举名称为 macOS

### 新增
- 支持多引擎测试工作流

## [0.0.2] - 2025-03-26
### 变更
- 重构 XPrefs.IPanel 为接口

## [0.0.1] - 2025-03-23
### 新增
- 首次发布
