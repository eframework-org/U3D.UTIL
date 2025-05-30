# 更新记录

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
