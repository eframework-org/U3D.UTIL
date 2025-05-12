// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;

namespace EFramework.Utility
{
    public partial class XLog
    {
        /// <summary>
        /// 文件日志适配器，实现日志的文件存储功能。
        /// 支持日志轮转、按时间分割和自动清理等特性。
        /// </summary>
        /// <remarks>
        /// 功能特性:
        /// - 支持日志级别过滤
        /// - 支持日志文件轮转
        /// - 支持按天/小时分割日志
        /// - 支持自动清理过期日志
        /// - 支持异步写入日志
        /// </remarks>
        internal partial class FileAdapter : IAdapter
        {
            /// <summary>
            /// 用于同步的锁对象
            /// </summary>
            internal readonly object lockObj = new();

            /// <summary>
            /// 日志输出级别
            /// </summary>
            internal LevelType level;

            /// <summary>
            /// 是否启用日志轮转
            /// </summary>
            internal bool rotate;

            /// <summary>
            /// 是否按天分割日志
            /// </summary>
            internal bool daily;

            /// <summary>
            /// 日志保留最大天数
            /// </summary>
            internal int maxDay;

            /// <summary>
            /// 是否按小时分割日志
            /// </summary>
            internal bool hourly;

            /// <summary>
            /// 日志保留最大小时数
            /// </summary>
            internal int maxHour;

            /// <summary>
            /// 日志文件路径
            /// </summary>
            internal string path;

            /// <summary>
            /// 最大日志文件数
            /// </summary>
            internal int maxFile;

            /// <summary>
            /// 单个日志文件最大行数
            /// </summary>
            internal int maxLine;

            /// <summary>
            /// 单个日志文件最大大小(字节)
            /// </summary>
            internal int maxSize;

            /// <summary>
            /// 当前日志文件的行数
            /// </summary>
            internal int currentLines;

            /// <summary>
            /// 当前日志文件序号
            /// </summary>
            internal int currentFileNum;

            /// <summary>
            /// 当前日志文件大小(字节)
            /// </summary>
            internal long currentSize;

            /// <summary>
            /// 当前日志文件的创建时间(按天)
            /// </summary>
            internal DateTime dailyOpenTime;

            /// <summary>
            /// 当前日志文件的创建时间(按小时)
            /// </summary>
            internal DateTime hourlyOpenTime;

            /// <summary>
            /// 日志文件名前缀
            /// </summary>
            internal string prefix;

            /// <summary>
            /// 日志文件扩展名
            /// </summary>
            internal string suffix;

            /// <summary>
            /// 日志写入队列
            /// </summary>
            internal readonly ConcurrentQueue<LogData> logQueue = new();

            /// <summary>
            /// 写入线程同步事件
            /// </summary>
            internal readonly AutoResetEvent writeEvent = new(false);

            /// <summary>
            /// 写入线程运行状态
            /// </summary>
            internal bool isRunning;

            /// <summary>
            /// 日志写入流
            /// </summary>
            internal StreamWriter streamWriter;

            /// <summary>
            /// 日志写入线程
            /// </summary>
            internal Thread writerThread;

            /// <summary>
            /// 初始化文件日志适配器
            /// </summary>
            /// <param name="prefs">配置参数</param>
            /// <returns>日志输出级别</returns>
            public LevelType Init(XPrefs.IBase prefs)
            {
                if (isRunning) Close(); // 适配单元测试
                if (prefs == null) return level;

                if (!Enum.TryParse(prefs.GetString(Prefs.Level, Prefs.LevelDefault), out level))
                {
                    level = LevelType.Undefined;
                }
                rotate = prefs.GetBool(Prefs.Rotate, Prefs.RotateDefault);
                daily = prefs.GetBool(Prefs.Daily, Prefs.DailyDefault);
                maxDay = prefs.GetInt(Prefs.MaxDay, Prefs.MaxDayDefault);
                hourly = prefs.GetBool(Prefs.Hourly, Prefs.HourlyDefault);
                maxHour = prefs.GetInt(Prefs.MaxHour, Prefs.MaxHourDefault);
                path = XString.Eval(prefs.GetString(Prefs.Path, Prefs.PathDefault), XEnv.Vars, XPrefs.Asset);
                maxFile = prefs.GetInt(Prefs.MaxFile, Prefs.MaxFileDefault);
                maxLine = prefs.GetInt(Prefs.MaxLine, Prefs.MaxLineDefault);
                maxSize = prefs.GetInt(Prefs.MaxSize, Prefs.MaxSizeDefault);

                // 处理路径逻辑
                suffix = Path.GetExtension(path);
                if (string.IsNullOrEmpty(suffix))
                {
                    // 如果路径没有扩展名，认为是目录
                    suffix = ".log";
                    prefix = "";
                    // 确保路径以分隔符结尾
                    path = Path.Combine(path, suffix);
                }
                else
                {
                    var fileName = Path.GetFileName(path);
                    if (fileName == suffix)
                    {
                        // 如果基本名称就是后缀（如 .log），则没有文件名
                        prefix = "";
                    }
                    else
                    {
                        // 正常的文件名情况
                        prefix = Path.GetFileNameWithoutExtension(path);
                        // 确保前缀不以点结尾
                        prefix = prefix.TrimEnd('.');
                    }
                }

                path = XFile.NormalizePath(path);

                currentFileNum = 0;

                if (NewWriter()) AsyncWrite();

                return level;
            }

            /// <summary>
            /// 写入日志数据
            /// </summary>
            /// <param name="data">日志数据</param>
            public void Write(LogData data)
            {
                if (data == null) return;
                if (data.Level > level && !data.Force) return;

                logQueue.Enqueue(data);
                writeEvent.Set();
            }

            /// <summary>
            /// 刷新日志缓冲区
            /// </summary>
            public void Flush()
            {
                lock (lockObj)
                {
                    if (!isRunning) return;
                    streamWriter?.Flush();
                }
            }

            /// <summary>
            /// 关闭日志适配器
            /// </summary>
            public void Close()
            {
                lock (lockObj)
                {
                    if (!isRunning) return;
                    isRunning = false; // 标记停止但继续处理队列
                }

                writeEvent.Set();  // 唤醒线程处理剩余日志
                writerThread?.Join();  // 等待线程完成所有写入

                lock (lockObj)
                {
                    try
                    {
                        if (streamWriter != null)
                        {
                            streamWriter.Flush();
                            streamWriter.Close();
                            if (streamWriter.BaseStream != null && streamWriter.BaseStream is FileStream fs)
                            {
                                fs.Dispose(); // 显式释放底层流
                            }
                            streamWriter = null;
                        }
                    }
                    catch (Exception e) { Panic(e, "Failed to close writer."); }
                    finally { writeEvent.Reset(); }
                }
            }

            /// <summary>
            /// 创建新的日志写入流
            /// </summary>
            /// <returns>是否创建成功</returns>
            internal bool NewWriter()
            {
                try
                {
                    dailyOpenTime = DateTime.Now;
                    hourlyOpenTime = DateTime.Now;
                    XFile.CreateDirectory(Path.GetDirectoryName(path));
                    streamWriter = new StreamWriter(path, true, Encoding.UTF8);
                    return true;
                }
                catch (Exception e) { Panic(e, "Failed to new writer."); return false; }
            }

            /// <summary>
            /// 启动异步写入线程
            /// </summary>
            internal void AsyncWrite()
            {
                isRunning = true;
                writerThread = new Thread(() =>
                {
                    while (isRunning || !logQueue.IsEmpty)
                    {
                        if (logQueue.IsEmpty)
                        {
                            writeEvent.WaitOne();
                            continue;
                        }
                        while (logQueue.TryDequeue(out var data))
                        {
                            lock (lockObj)
                            {
                                try
                                {
                                    if (NeedRotate()) DoRotate();

                                    var timeStr = XTime.Format(data.Time, "MM/dd HH:mm:ss.fff");
                                    var text = $"[{timeStr}] {data.Text(true)}\n";
                                    streamWriter.Write(text);

                                    currentLines++;
                                    currentSize += text.Length;
                                }
                                catch (Exception e) { Panic(e, "Failed to write log."); }
                                finally { LogData.Put(data); }
                            }
                        }
                    }
                    lock (lockObj) streamWriter?.Flush(); // 确保最后的数据被写入
                })
                {
                    IsBackground = true,
                    Name = "XLog.FileWriter"
                };
                writerThread.Start();
            }

            /// <summary>
            /// 检查是否需要轮转日志
            /// </summary>
            /// <returns>是否需要轮转</returns>
            internal bool NeedRotate()
            {
                if (!rotate) return false;

                var now = DateTime.Now;
                return (maxLine > 0 && currentLines >= maxLine) ||
                       (maxSize > 0 && currentSize >= maxSize) ||
                       (daily && now.Date != dailyOpenTime.Date) ||
                       (hourly && now.Hour != hourlyOpenTime.Hour);
            }

            /// <summary>
            /// 执行日志轮转
            /// </summary>
            internal void DoRotate()
            {
                try
                {
                    if (streamWriter != null)
                    {
                        streamWriter.Flush();
                        streamWriter.Close();
                        if (streamWriter.BaseStream != null && streamWriter.BaseStream is FileStream fs)
                        {
                            fs.Dispose(); // 显式释放底层流
                        }
                    }

                    var newPath = path;
                    var format = "";
                    var openTime = DateTime.Now;

                    // 检查原始文件是否存在
                    if (!XFile.HasFile(path))
                    {
                        goto RESTART_LOGGER;
                    }

                    if (hourly)
                    {
                        format = "yyyy-MM-dd-HH";
                        openTime = hourlyOpenTime;
                    }
                    else if (daily)
                    {
                        format = "yyyy-MM-dd";
                        openTime = dailyOpenTime;
                    }

                    var num = currentFileNum + 1;
                    var dir = Path.GetDirectoryName(path);

                    // 生成轮转文件名
                    if (maxLine > 0 || maxSize > 0)
                    {
                        for (; num <= maxFile; num++)
                        {
                            string fName;
                            if (string.IsNullOrEmpty(prefix))
                            {
                                // 无文件名情况：使用序号作为文件名
                                fName = XFile.PathJoin(dir, $"{num:000}{suffix}");
                            }
                            else
                            {
                                // 有文件名情况：在原文件名后添加序号
                                if (!string.IsNullOrEmpty(format))
                                {
                                    // 按时间轮转
                                    fName = XFile.PathJoin(dir, $"{prefix}.{DateTime.Now.ToString(format)}.{num:000}{suffix}");
                                }
                                else
                                {
                                    // 按行数或大小轮转
                                    fName = XFile.PathJoin(dir, $"{prefix}.{num:000}{suffix}");
                                }
                            }

                            if (!XFile.HasFile(fName))
                            {
                                newPath = fName;
                                currentFileNum = num;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(prefix))
                        {
                            newPath = XFile.PathJoin(dir, $"{openTime.ToString(format)}.{num:000}{suffix}");
                        }
                        else
                        {
                            newPath = XFile.PathJoin(dir, $"{prefix}.{openTime.ToString(format)}.{num:000}{suffix}");
                        }
                        currentFileNum = num;
                    }

                    newPath = XFile.NormalizePath(newPath);
                    File.Move(path, newPath);

RESTART_LOGGER:
                    NewWriter();
                    currentLines = 0;
                    currentSize = 0;

                    DeleteOld();
                }
                catch (Exception e) { Panic(e, "Failed to rotate log."); }
            }

            /// <summary>
            /// 清理过期的日志文件
            /// </summary>
            internal void DeleteOld()
            {
                try
                {
                    var dir = Path.GetDirectoryName(path);
                    var files = Directory.GetFiles(dir);
                    var now = DateTime.Now;

                    foreach (var file in files)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(file);
                            var fileInfo = new FileInfo(file);

                            // 检查文件是否匹配我们的模式
                            bool isMatch;
                            if (string.IsNullOrEmpty(prefix))
                            {
                                // 无前缀情况
                                isMatch = fileName.EndsWith(suffix) &&
                                    (fileName.StartsWith("[0-9][0-9][0-9]") || // 按行数轮转
                                     fileName.Contains(".")); // 按时间轮转
                            }
                            else
                            {
                                // 有前缀情况
                                isMatch = fileName.StartsWith(prefix) && fileName.EndsWith(suffix);
                            }

                            if (!isMatch) continue;

                            // 根据轮转模式检查文件时间
                            if (hourly && fileInfo.LastWriteTime.AddHours(maxHour).CompareTo(now) < 0)
                            {
                                XFile.DeleteFile(file);
                            }
                            else if (daily && fileInfo.LastWriteTime.AddDays(maxDay).CompareTo(now) < 0)
                            {
                                XFile.DeleteFile(file);
                            }
                        }
                        catch (Exception e) { Panic(e, $"Failed to process log file {file}."); }
                    }
                }
                catch (Exception e) { Panic(e, "Failed to delete old log file(s)."); }
            }
        }

        internal partial class FileAdapter
        {
            public class Prefs : XPrefs.Panel
            {
                public const string Config = "Log/File";

                public static readonly XPrefs.IBase ConfigDefault = new();

                public const string Level = "Level";

                public static readonly string LevelDefault = LevelType.Notice.ToString();

                public const string Rotate = "Rotate";

                public static readonly bool RotateDefault = true;

                public const string Daily = "Daily";

                public static readonly bool DailyDefault = true;

                public const string MaxDay = "MaxDay";

                public static readonly int MaxDayDefault = 7;

                public const string Hourly = "Hourly";

                public static readonly bool HourlyDefault = false;

                public const string MaxHour = "MaxHour";

                public static readonly int MaxHourDefault = 168;

                public const string Path = "Path@Const";

                public static readonly string PathDefault = "${Env.LocalPath}/Log/";

                public const string MaxFile = "MaxFile";

                public static readonly int MaxFileDefault = 100;

                public const string MaxLine = "MaxLine";

                public static readonly int MaxLineDefault = 1000000;

                public const string MaxSize = "MaxSize";

                public static readonly int MaxSizeDefault = 1 << 27; // 128MB

#if UNITY_EDITOR
                public override string Section => "Log";

                public override int Priority => 11;

                [SerializeField] protected bool foldout = true;

                public override void OnVisualize(string searchContext)
                {
                    var config = Target.Get(Config, ConfigDefault);
                    UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                    foldout = UnityEditor.EditorGUILayout.Foldout(foldout, new GUIContent("File", "File Persistent Adapter."));
                    if (foldout)
                    {
                        UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);

                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        Title("Level", "Log Level.");
                        Enum.TryParse<LevelType>(config.GetString(Level, LevelDefault), out var levelType);
                        config.Set(Level, UnityEditor.EditorGUILayout.EnumPopup("", levelType).ToString());

                        Title("Rotate", "Enable log rotation.");
                        var rotate = config.GetBool(Rotate, RotateDefault);
                        config.Set(Rotate, UnityEditor.EditorGUILayout.Toggle("", rotate));
                        UnityEditor.EditorGUILayout.EndHorizontal();

                        var ocolor = GUI.color;
                        if (!rotate) GUI.color = GUI.color = Color.gray;

                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        Title("Daily", "Enable daily log rotation.");
                        var daily = UnityEditor.EditorGUILayout.Toggle(config.GetBool(Daily, DailyDefault));
                        if (rotate) config.Set(Daily, daily);
                        var maxDay = UnityEditor.EditorGUILayout.IntField("", config.GetInt(MaxDay, MaxDayDefault));
                        if (rotate) config.Set(MaxDay, maxDay);
                        UnityEditor.EditorGUILayout.EndHorizontal();

                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        Title("Hourly", "Enable hourly log rotation.");
                        var hourly = UnityEditor.EditorGUILayout.Toggle("", config.GetBool(Hourly, HourlyDefault));
                        if (rotate) config.Set(Hourly, hourly);
                        var maxHour = UnityEditor.EditorGUILayout.IntField("", config.GetInt(MaxHour, MaxHourDefault));
                        if (rotate) config.Set(MaxHour, maxHour);
                        UnityEditor.EditorGUILayout.EndHorizontal();

                        GUI.color = ocolor;
                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        Title("Path", "Log file path.");
                        var path = config.GetString(Path, PathDefault);
                        config.Set(Path, UnityEditor.EditorGUILayout.TextField("", path));

                        if (!rotate) GUI.color = GUI.color = Color.gray;
                        Title("Count", "Max file count.");
                        var maxFile = UnityEditor.EditorGUILayout.IntField("", config.GetInt(MaxFile, MaxFileDefault));
                        if (rotate) config.Set(MaxFile, maxFile);
                        UnityEditor.EditorGUILayout.EndHorizontal();

                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        Title("Line", "Max line count.");
                        var maxLine = UnityEditor.EditorGUILayout.IntField("", config.GetInt(MaxLine, MaxLineDefault));
                        if (rotate) config.Set(MaxLine, maxLine);

                        Title("Size", "Max file size(MB).");
                        var maxSize = UnityEditor.EditorGUILayout.IntField("", config.GetInt(MaxSize, MaxSizeDefault) / 1024 / 1024) * 1024 * 1024;
                        if (rotate) config.Set(MaxSize, maxSize);
                        UnityEditor.EditorGUILayout.EndHorizontal();

                        GUI.color = ocolor;

                        UnityEditor.EditorGUILayout.EndVertical();
                    }
                    UnityEditor.EditorGUILayout.EndVertical();
                    if (!Target.Has(Config) || config.Dirty) Target.Set(Config, config);
                }

                public override bool Validate()
                {
                    levelMax = LevelType.Undefined; // 重置最大值
                    return base.Validate();
                }

                public override void OnApply()
                {
                    var config = Target.Get(Config, ConfigDefault);
                    Enum.TryParse<LevelType>(config.GetString(Level, LevelDefault), out var levelType);
                    if (levelType > levelMax) levelMax = levelType;
                }
#endif
            }
        }
    }
}
