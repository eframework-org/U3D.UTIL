// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace EFramework.Utility
{
    /// <summary>
    /// XLog 提供了一个遵循 RFC5424 标准的日志系统，支持多级别日志输出、多适配器管理、日志轮转和结构化标签等特性。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 支持 RFC5424 标准的 8 个日志级别
    /// - 支持标准输出和文件存储两种适配器
    /// - 支持日志文件的自动轮转和清理
    /// - 支持异步写入和线程安全操作
    /// - 支持结构化的日志标签系统
    /// 
    /// 使用手册
    /// 1. 基础日志记录
    /// 
    /// 1.1 日志级别
    ///     // 不同级别的日志记录（按严重程度从高到低排序）
    ///     XLog.Emergency("系统崩溃");    // 级别 0：系统不可用
    ///     XLog.Alert("需要立即处理");    // 级别 1：必须立即采取措施
    ///     XLog.Critical("严重错误");     // 级别 2：严重条件
    ///     XLog.Error("操作失败");        // 级别 3：错误条件
    ///     XLog.Warn("潜在问题");         // 级别 4：警告条件
    ///     XLog.Notice("重要信息");       // 级别 5：正常但重要的情况
    ///     XLog.Info("一般信息");         // 级别 6：信息消息
    ///     XLog.Debug("调试信息");        // 级别 7：调试级别消息
    /// 
    /// 2. 日志配置
    /// 
    /// 2.1 文件输出配置
    ///     var prefs = new XPrefs.IBase();
    ///     var fileConf = new XPrefs.IBase();
    ///     
    ///     // 基础配置
    ///     fileConf.Set("Path", "${Env.LocalPath}/Log/app.log");  // 日志文件路径
    ///     fileConf.Set("Level", "Debug");                        // 日志级别
    ///     
    ///     // 轮转配置
    ///     fileConf.Set("Rotate", true);        // 是否启用日志轮转
    ///     fileConf.Set("Daily", true);         // 是否按天轮转
    ///     fileConf.Set("MaxDay", 7);           // 日志文件保留天数
    ///     fileConf.Set("Hourly", false);       // 是否按小时轮转
    ///     fileConf.Set("MaxHour", 168);        // 日志文件保留小时数
    ///     
    ///     // 文件限制
    ///     fileConf.Set("MaxFile", 100);        // 最大文件数量
    ///     fileConf.Set("MaxLine", 1000000);    // 单文件最大行数
    ///     fileConf.Set("MaxSize", 134217728);  // 单文件最大体积（128MB）
    ///     
    ///     prefs.Set("Log/File", fileConf);
    ///     XLog.Setup(prefs);
    /// 
    /// 3. 日志标签
    /// 
    /// 3.1 基本用法
    ///     // 创建和使用标签
    ///     var tag = XLog.GetTag()
    ///         .Set("module", "network")
    ///         .Set("action", "connect")
    ///         .Set("userId", "12345");
    ///     
    ///     // 使用标签记录日志
    ///     XLog.Info(tag, "用户连接成功");
    ///     
    ///     // 使用完后回收标签
    ///     XLog.PutTag(tag);
    /// 
    /// 4. 生命周期管理
    /// 
    /// 4.1 初始化
    ///     // 自动初始化时机：
    ///     // 1. Unity 编辑器加载时
    ///     // 2. 运行时程序集加载后
    ///     // 3. 编辑器播放模式切换时
    ///     
    ///     // 手动初始化（使用默认配置）
    ///     XLog.Setup(XPrefs.Asset);
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public partial class XLog
    {
        /// <summary>
        /// LevelType 是日志的等级类型。
        /// RFC5424 日志标准，包括结构化数据格式。
        /// 规定了八个日志消息的严重性级别，用于表示被记录事件的严重程度或紧急程度。
        /// </summary>
        public enum LevelType : short
        {
            /// <summary>
            /// Undefined 表示未定义的日志类型。
            /// </summary>
            Undefined = -1,

            /// <summary>
            /// Emergency 表示紧急（0）的日志类型，系统不可用，通常用于灾难性故障。
            /// </summary>
            Emergency = 0,

            /// <summary>
            /// Alert 表示警报（1）的日志类型，必须立即采取行动，指示需要立即注意的情况。
            /// </summary>
            Alert = 1,

            /// <summary>
            /// Critical 表示严重（2）的日志类型，指示需要立即注意的严重故障。
            /// </summary>
            Critical = 2,

            /// <summary>
            /// Error 表示错误（3）的日志类型，指示应该解决的错误。
            /// </summary>
            Error = 3,

            /// <summary>
            /// Warn 表示警告（4）的日志类型，指示潜在问题，如果不解决可能会导致错误。
            /// </summary>
            Warn = 4,

            /// <summary>
            /// Notice 表示通知（5）的日志类型，指示值得注意但不一定有问题的事件。
            /// </summary>
            Notice = 5,

            /// <summary>
            /// Info 表示信息（6）的日志类型，用于系统操作的一般信息。
            /// </summary>
            Info = 6,

            /// <summary>
            /// Debug 表示调试（7）的日志类型，用于调试和故障排除目的的消息
            /// </summary>
            Debug = 7,
        }

        /// <summary>
        /// LogData 是日志的数据类，用于封装单条日志的所有相关信息。
        /// </summary>
        internal class LogData
        {
            /// <summary>
            /// pooled 表示标记对象是否已被池化。
            /// </summary>
            private bool pooled;

            /// <summary>
            /// Level 是日志的级别。
            /// </summary>
            public LevelType Level;

            /// <summary>
            /// Force 表示是否强制输出，忽略日志级别限制。
            /// </summary>
            public bool Force;

            /// <summary>
            /// Data 是日志的内容。
            /// </summary>
            public object Data;

            /// <summary>
            /// Args 是格式化的参数。
            /// </summary>
            public object[] Args;

            /// <summary>
            /// Tag 是日志的标签文本。
            /// </summary>
            public string Tag;

            /// <summary>
            /// Time 是日志的时间戳，单位：毫秒。
            /// </summary>
            public long Time;

            /// <summary>
            /// Text 是格式化的日志文本。
            /// </summary>
            /// <param name="tag">是否包含标签信息</param>
            /// <returns>格式化后的日志文本</returns>
            public string Text(bool tag)
            {
                var fmt = Data is string str ? str : null;
                return tag ? logLabels[(int)Level] + " " + (string.IsNullOrEmpty(Tag) ? "" : Tag + " ") + (fmt != null ? XString.Format(fmt, Args) : Data.ToString()) :
                    logLabels[(int)Level] + " " + (fmt != null ? XString.Format(fmt, Args) : Data.ToString());
            }

            /// <summary>
            /// Reset 重置日志数据的所有字段为默认值。
            /// </summary>
            public void Reset()
            {
                Level = LevelType.Undefined;
                Force = false;
                Data = null;
                Args = null;
                Tag = null;
                Time = 0;
            }

            /// <summary>
            /// pool 是日志数据的对象池。
            /// </summary>
            private static readonly ObjectPool<LogData> pool = new(() => new LogData(), null, null, null);

            /// <summary>
            /// Get 从对象池获取一个日志数据对象。
            /// </summary>
            /// <returns>日志数据对象</returns>
            public static LogData Get()
            {
                lock (pool)
                {
                    var data = pool.Get();
                    data.pooled = false;
                    return data;
                }
            }

            /// <summary>
            /// Put 将日志数据对象返回到对象池。
            /// </summary>
            /// <param name="data">要返回的日志数据对象</param>
            public static void Put(LogData data)
            {
                if (data != null)
                {
                    lock (pool)
                    {
                        if (!data.pooled)
                        {
                            data.Reset();
                            data.pooled = true;
                            pool.Release(data);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// IAdapter 是日志适配器的接口，定义了日志输出的基本操作。
        /// </summary>
        internal interface IAdapter
        {
            /// <summary>
            /// Init 初始化日志适配器。
            /// </summary>
            /// <param name="prefs">配置参数</param>
            /// <returns>日志输出级别</returns>
            LevelType Init(XPrefs.IBase prefs);

            /// <summary>
            /// Write 写入日志数据。
            /// </summary>
            /// <param name="data">日志数据</param>
            void Write(LogData data);

            /// <summary>
            /// Flush 刷新日志缓冲区。
            /// </summary>
            void Flush();

            /// <summary>
            /// Close 关闭日志适配器。
            /// </summary>
            void Close();
        }

        /// <summary>
        /// logLabels 是日志的标签数组，用于标识不同级别的日志。
        /// </summary>
        internal static readonly string[] logLabels = new string[] {
            "[M]", // Emergency
            "[A]", // Alert
            "[C]", // Critical
            "[E]", // Error
            "[W]", // Warn
            "[N]", // Notice
            "[I]", // Info
            "[D]", // Debug
        };

        /// <summary>
        /// batchMode 表示是否在批处理模式下运行。
        /// </summary>
        internal static bool batchMode;

        /// <summary>
        /// editorMode 表示是否在编辑器模式下运行。
        /// </summary>
        internal static bool editorMode;

        /// <summary>
        /// levelMax 是当前最高的日志级别。
        /// </summary>
        internal static LevelType levelMax = LevelType.Undefined;

        /// <summary>
        /// adapters 是日志适配器的映射表。
        /// </summary>
        internal static readonly Dictionary<string, IAdapter> adapters = new();

        /// <summary>
        /// usingDefault 表示是否使用内置的适配器输出。
        /// </summary>
        internal static bool usingDefault = true;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        internal static void OnInit()
        {
            UnityEngine.Debug.unityLogger.logHandler = Handler;
            batchMode = Application.isBatchMode;

#if UNITY_EDITOR
            editorMode = !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
            UnityEditor.EditorApplication.playModeStateChanged += mode =>
            {
                if (mode == UnityEditor.PlayModeStateChange.ExitingEditMode)
                {
                    Flush();
                    editorMode = false;
                }
                else if (mode == UnityEditor.PlayModeStateChange.EnteredEditMode)
                {
                    editorMode = true;

                    // 程序集被重载了，重新初始化日志系统
                    Setup(XPrefs.Asset);
                }
            };
            UnityEditor.EditorApplication.quitting += () => Close();

            // 程序集重载之前（如：编译脚本）主动关闭日志系统，避免 IOException: Sharing violation on path xxx
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += () => Close();
#endif

            Application.quitting += Close;
            SceneManager.sceneUnloaded += scene => Flush();

            Setup(XPrefs.Asset);
        }

        /// <summary>
        /// Setup 设置日志系统的配置。
        /// </summary>
        /// <param name="prefs">配置参数</param>
        public static void Setup(XPrefs.IBase prefs)
        {
            // 清理旧的适配器
            Flush();
            Close();

            // 初始化适配器
            var tempLevel = LevelType.Undefined;
            foreach (var kvp in prefs)
            {
                if (!kvp.Key.StartsWith("Log/")) continue;

                var name = kvp.Key.Split('/')[1];
                var conf = prefs.Get<XPrefs.IBase>(kvp.Key);
                if (conf == null) continue;

                IAdapter adapter = null;
                switch (name)
                {
                    case "Std": adapter = new StdAdapter(); break;
                    case "File": adapter = new FileAdapter(); break;
                    default: break;
                }

                if (adapter != null)
                {
                    var level = adapter.Init(conf);
                    if (level > tempLevel) tempLevel = level;
                    adapters[name] = adapter;
                }
            }

            if (adapters.Count == 0 || (Application.isEditor && !adapters.ContainsKey("Std")))
            {
                // 设置默认的输出适配器，避免调用 XLog.* 无法输出
                tempLevel = LevelType.Debug;
                adapters["Std"] = new StdAdapter() { level = LevelType.Debug, colored = true };
            }

            // 更新最大日志级别
            levelMax = tempLevel;
            usingDefault = false;

            Print(level: LevelType.Notice, force: true, tag: null, data: "XLog.Setup: performed setup with {0} adapter(s), max level is {1}.", adapters.Count, levelMax);
        }

        /// <summary>
        /// Flush 将所有缓冲的日志条目写入到目标位置。
        /// </summary>
        public static void Flush()
        {
            if (adapters.Count > 0)
            {
                foreach (var adapter in adapters.Values) adapter.Flush();
                Print(level: LevelType.Notice, force: true, tag: null, data: "XLog.Flush: performed flush with {0} adapter(s).", adapters.Count);
            }
        }

        /// <summary>
        /// Close 刷新并关闭日志系统。
        /// </summary>
        public static void Close()
        {
            if (adapters.Count > 0)
            {
                Print(level: LevelType.Notice, force: true, tag: null, data: "XLog.Close: performed close with {0} adapter(s).", adapters.Count);
                foreach (var adapter in adapters.Values) adapter.Close();
                adapters.Clear();
            }
        }

        /// <summary>
        /// Level 获取当前日志最大级别。
        /// </summary>
        /// <returns>当前日志最大级别</returns>
        public static LevelType Level() { return levelMax; }

        /// <summary>
        /// Able 检查给定的日志级别是否可以根据配置的最大级别输出。
        /// </summary>
        /// <param name="level">需要检查的日志级别</param>
        /// <returns>是否可以输出</returns>
        public static bool Able(LevelType level)
        {
            Condition(level, null, out var able, out _, out var _, out var _);
            return able;
        }

        /// <summary>
        /// Condition 检查给定的日志级别是否可以根据配置的最大级别输出。
        /// </summary>
        /// <param name="level">需要检查的日志级别</param>
        /// <param name="args">格式参数</param>
        /// <param name="able">是否可以输出</param>
        /// <param name="force">是否强制输出</param>
        /// <param name="tag">日志标签</param>
        /// <param name="nargs">处理后的格式参数</param>
        internal static void Condition(LevelType level, object[] args, out bool able, out bool force, out LogTag tag, out object[] nargs)
        {
            // 优化1: 提前处理 null 或空数组的情况
            if (args == null || args.Length == 0)
            {
                nargs = args;
                tag = Tag();
                able = level <= levelMax;
                force = false;
#if UNITY_EDITOR
                if (editorMode)
                {
                    able = true;
                    force = true;
                }
#endif
                return;
            }

            // 优化2: 避免重复的类型检查，一次性完成类型转换
            if (args[0] is LogTag logTag)
            {
                tag = logTag;
                int newLength = args.Length - 1;
                if (newLength > 0)
                {
                    // 优化3: 只在必要时创建新数组
                    nargs = new object[newLength];
                    Array.Copy(args, 1, nargs, 0, newLength);
                }
                else
                {
                    nargs = Array.Empty<object>();
                }

                // 优化4: 简化条件判断，减少分支
                if (tag.Level != LevelType.Undefined)
                {
                    able = level <= tag.Level;
                    force = true;
#if UNITY_EDITOR
                    if (editorMode)
                    {
                        able = true;
                        force = true;
                    }
#endif
                    return;
                }
            }
            else
            {
                tag = Tag();
                nargs = args;
            }

            // 优化5: 默认情况的处理
            able = level <= levelMax;
            force = false;
#if UNITY_EDITOR
            if (editorMode)
            {
                able = true;
                force = true;
            }
#endif
        }

        /// <summary>
        /// Panic 输出异常的日志。
        /// </summary>
        /// <param name="exception">异常信息</param>
        /// <param name="extras">附加信息</param>
        public static void Panic(Exception exception, string extras = "")
        {
            if (string.IsNullOrEmpty(extras)) UnityEngine.Debug.LogException(exception);
            else UnityEngine.Debug.LogException(new Exception(extras, exception));
        }

        /// <summary>
        /// Emergency 输出紧急（0）级别的日志，系统不可用，通常用于灾难性故障。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Emergency(object data, params object[] args)
        {
            Condition(LevelType.Emergency, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Emergency, force, tag, data, nargs);
        }

        /// <summary>
        /// Alert 输出警报（1）级别的日志，必须立即采取行动，指示需要立即注意的情况。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Alert(object data, params object[] args)
        {
            Condition(LevelType.Alert, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Alert, force, tag, data, nargs);
        }

        /// <summary>
        /// Critical 输出严重（2）级别的日志，指示需要立即注意的严重故障。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Critical(object data, params object[] args)
        {
            Condition(LevelType.Critical, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Critical, force, tag, data, nargs);
        }

        /// <summary>
        /// Error 输出错误（3）级别的日志，指示应该解决的错误。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Error(object data, params object[] args)
        {
            Condition(LevelType.Error, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Error, force, tag, data, nargs);
        }

        /// <summary>
        /// Warn 输出警告（4）级别的日志，指示潜在问题，如果不解决可能会导致错误。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Warn(object data, params object[] args)
        {
            Condition(LevelType.Warn, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Warn, force, tag, data, nargs);
        }

        /// <summary>
        /// Notice 输出通知（5）级别的日志：正常但重要的情况，指示值得注意但不一定有问题的事件。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Notice(object data, params object[] args)
        {
            Condition(LevelType.Notice, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Notice, force, tag, data, nargs);
        }

        /// <summary>
        /// Info 输出信息（6）级别的日志，用于系统操作的一般信息。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Info(object data, params object[] args)
        {
            Condition(LevelType.Info, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Info, force, tag, data, nargs);
        }

        /// <summary>
        /// Debug 输出调试（7）级别的日志：调试级别的消息，用于调试和故障排除目的的消息。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Debug(object data, params object[] args)
        {
            Condition(LevelType.Debug, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Debug, force, tag, data, nargs);
        }

        /// <summary>
        /// Print 格式化并输出日志。
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="force">强制输出</param>
        /// <param name="tag">日志标签</param>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式化参数</param>
        public static void Print(LevelType level, bool force, LogTag tag, object data, params object[] args)
        {
            if (data == null) return;

            var log = LogData.Get();
            log.Level = level;
            log.Force = force;
            log.Data = data;
            log.Args = args;
            log.Time = XTime.GetMillisecond();
            log.Tag = tag?.Text ?? string.Empty;

            if (usingDefault)
            {
                if (level <= LevelType.Error) Handler.Default.LogFormat(LogType.Error, null, "{0}", log.Text(true));
                else Handler.Default.LogFormat(LogType.Log, null, "{0}", log.Text(true));
            }
            else
            {
                if (level <= LevelType.Error) UnityEngine.Debug.LogErrorFormat(string.Empty, log);
                else UnityEngine.Debug.LogFormat(string.Empty, log);
            }

            LogData.Put(log);
        }
    }

    public partial class XLog
    {
        /// <summary>
        /// Handler 是全局的日志处理器。
        /// </summary>
        public static readonly LogHandler Handler = new();

        /// <summary>
        /// LogHandler 是日志的处理器，用于控制适配器输出。
        /// </summary>
        public class LogHandler : ILogHandler
        {
            /// <summary>
            /// Default 是 Unity 默认的日志处理器。
            /// </summary>
            public readonly ILogHandler Default = UnityEngine.Debug.unityLogger.logHandler;

            /// <summary>
            /// LogFormat 用于处理并打印日志信息。
            /// </summary>
            /// <param name="logType"></param>
            /// <param name="context"></param>
            /// <param name="format"></param>
            /// <param name="args"></param>
            public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                if (args != null && args.Length > 0 && args[0] is LogData rawLog)
                {
                    if (usingDefault) Default.LogFormat(logType, context, rawLog.Text(true));
                    else
                    {
                        foreach (var adapter in adapters.Values)
                        {
                            var log = LogData.Get();
                            log.Level = rawLog.Level;
                            log.Force = rawLog.Force;
                            log.Data = rawLog.Data;
                            log.Args = rawLog.Args;
                            log.Time = rawLog.Time;
                            log.Tag = rawLog.Tag;
                            adapter.Write(log);
                        }
                    }
                }
                else
                {
                    if (usingDefault) Default.LogFormat(logType, context, format, args);
                    else
                    {
                        foreach (var adapter in adapters.Values)
                        {
                            var log = LogData.Get();
                            log.Level = logType == LogType.Error ? LevelType.Error : (logType == LogType.Warning ? LevelType.Warn : LevelType.Info);
                            log.Force = false;
                            log.Data = format;
                            log.Args = args;
                            log.Time = XTime.GetMillisecond();
                            adapter.Write(log);
                        }
                    }
                }
            }

            /// <summary>
            /// LogException 用于处理并打印异常信息。
            /// </summary>
            /// <param name="exception"></param>
            /// <param name="context"></param>
            public void LogException(Exception exception, UnityEngine.Object context)
            {
                if (usingDefault) Default.LogException(exception, context);
                else
                {
                    foreach (var adapter in adapters.Values)
                    {
                        var log = LogData.Get();
                        log.Level = LevelType.Emergency;
                        log.Force = true;
                        log.Data = exception;
                        log.Time = XTime.GetMillisecond();
                        adapter.Write(log);
                    }
                }
            }
        }
    }
}
