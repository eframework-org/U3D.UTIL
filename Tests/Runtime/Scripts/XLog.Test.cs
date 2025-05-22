// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EFramework.Utility;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

/// <summary>
/// XLog模块的单元测试类。
/// 测试日志系统的核心功能，包括：
/// 1. 初始化和配置
/// 2. 不同日志级别的处理
/// 3. 日志打印功能
/// 4. 异常处理
/// 5. 日志标签系统
/// </summary>
public class TestXLog
{
    /// <summary>
    /// 测试日志文件的临时存储路径
    /// </summary>
    private string tempLogPath;

    /// <summary>
    /// 测试前的初始化工作：创建临时日志目录
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        tempLogPath = XFile.PathJoin(XEnv.LocalPath, "TestXLog-" + XTime.GetMillisecond());
        if (!XFile.HasDirectory(tempLogPath)) XFile.CreateDirectory(tempLogPath);
    }

    /// <summary>
    /// 测试后的清理工作：删除临时日志目录
    /// </summary>
    [OneTimeTearDown]
    public void Cleanup()
    {
        if (XFile.HasDirectory(tempLogPath))
        {
            XFile.DeleteDirectory(tempLogPath);
        }
        XLog.Setup(XPrefs.Asset);
    }

    /// <summary>
    /// 测试日志系统的初始化功能
    /// 验证：
    /// 1. 配置文件的正确加载
    /// 2. 日志文件的创建
    /// 3. 基本日志写入功能
    /// </summary>
    [Test]
    public void Init()
    {
        // 创建基本配置
        var prefs = new XPrefs.IBase();
        var fileConf = new XPrefs.IBase();
        var logPath = XFile.PathJoin(tempLogPath, "TestInit.log");
        fileConf.Set(XLog.FileAdapter.Prefs.Path, logPath);
        fileConf.Set(XLog.FileAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
        fileConf.Set(XLog.FileAdapter.Prefs.Rotate, false); // 禁用日志轮转，便于测试
        prefs.Set("Log/File", fileConf);

        // 初始化日志系统
        XLog.Setup(prefs);

        // 写入测试日志
        var message = "Test message";
        XLog.Info(message);

        // 确保日志写入并关闭
        XLog.Close();

        // 验证日志文件存在
        Assert.That(XFile.HasFile(logPath), Is.True,
            $"期望日志文件存在于路径 {logPath}");

        // 验证日志内容
        var content = XFile.OpenText(logPath);
        Assert.That(content, Does.Contain(message),
            $"期望日志内容包含消息 '{message}'");
    }

    /// <summary>
    /// 测试日志级别系统
    /// 验证：
    /// 1. 所有有效日志级别的处理
    /// 2. 无效日志级别的处理
    /// 3. 日志级别过滤功能
    /// </summary>
    [Test]
    public void Levels()
    {
        // 定义测试用例
        var tests = new[]
        {
            new { level = XLog.LevelType.Emergency, expected = true },
            new { level = XLog.LevelType.Alert, expected = true },
            new { level = XLog.LevelType.Critical, expected = true },
            new { level = XLog.LevelType.Error, expected = true },
            new { level = XLog.LevelType.Warn, expected = true },
            new { level = XLog.LevelType.Notice, expected = true },
            new { level = XLog.LevelType.Info, expected = true },
            new { level = XLog.LevelType.Debug, expected = true },
            new { level = (XLog.LevelType)100, expected = false } // 测试无效级别
        };

        // 配置最高日志级别为Debug
        var prefs = new XPrefs.IBase();
        var fileConf = new XPrefs.IBase();
        fileConf.Set(XLog.FileAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
        fileConf.Set(XLog.FileAdapter.Prefs.Path, XFile.PathJoin(tempLogPath, "TestLevels.log"));
        prefs.Set("Log/File", fileConf);

        XLog.Setup(prefs);

        // 验证每个日志级别
        foreach (var test in tests)
        {
            var result = XLog.Able(test.level);
            Assert.That(result, Is.EqualTo(test.expected),
                $"Able({test.level}) = {result}，期望值为 {test.expected}");
        }
    }

    /// <summary>
    /// 测试日志打印功能
    /// 验证：
    /// 1. 所有日志级别的消息写入
    /// 2. 日志格式的正确性
    /// 3. 异步写入的可靠性
    /// </summary>
    [Test]
    public void Prints()
    {
        // 创建配置
        var prefs = new XPrefs.IBase();
        var fileConf = new XPrefs.IBase();
        var logPath = XFile.PathJoin(tempLogPath, "TestPrints.log");
        fileConf.Set(XLog.FileAdapter.Prefs.Path, logPath);
        fileConf.Set(XLog.FileAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
        fileConf.Set(XLog.FileAdapter.Prefs.Rotate, false);
        prefs.Set("Log/File", fileConf);

        XLog.Setup(prefs);

        // 定义不同级别的测试消息
        var tests = new[]
        {
            new { level = XLog.LevelType.Emergency, action = new Action(() => XLog.Emergency("Emergency message")), message = "Emergency message" },
            new { level = XLog.LevelType.Alert, action = new Action(() => XLog.Alert("Alert message")), message = "Alert message" },
            new { level = XLog.LevelType.Critical, action = new Action(() => XLog.Critical("Critical message")), message = "Critical message" },
            new { level = XLog.LevelType.Error, action = new Action(() => XLog.Error("Error message")), message = "Error message" },
            new { level = XLog.LevelType.Warn, action = new Action(() => XLog.Warn("Warning message")), message = "Warning message" },
            new { level = XLog.LevelType.Notice, action = new Action(() => XLog.Notice("Notice message")), message = "Notice message" },
            new { level = XLog.LevelType.Info, action = new Action(() => XLog.Info("Info message")), message = "Info message" },
            new { level = XLog.LevelType.Debug, action = new Action(() => XLog.Debug("Debug message")), message = "Debug message" }
        };

        // 写入所有测试消息
        foreach (var test in tests)
        {
            test.action();
        }

        // 确保所有日志都被写入
        XLog.Close();

        // 验证日志文件存在
        Assert.That(XFile.HasFile(logPath), Is.True, "日志文件未能成功创建");

        // 验证日志内容
        var content = XFile.OpenText(logPath);
        foreach (var test in tests)
        {
            Assert.That(content, Does.Contain(test.message),
                $"未找到期望的日志消息: {test.message}");
        }
    }

    /// <summary>
    /// 测试异常处理功能
    /// 验证：
    /// 1. 异常日志的正确输出
    /// 2. 异常信息的格式化
    /// </summary>
    [Test]
    public void Panic()
    {
        var errorMessage = "Test panic";
        LogAssert.Expect(LogType.Exception, new Regex(errorMessage));
        XLog.Panic(new Exception(errorMessage));
    }

    /// <summary>
    /// 测试日志标签系统
    /// 验证：
    /// 1. 标签的设置和使用
    /// 2. 上下文标签的控制
    /// 3. 多值标签的处理
    /// 4. 带格式化的标签消息
    /// </summary>
    [Test]
    public void Tags()
    {
        // 创建配置
        var prefs = new XPrefs.IBase();
        var fileConf = new XPrefs.IBase();
        var logPath = XFile.PathJoin(tempLogPath, "TestTags.log");
        fileConf.Set(XLog.FileAdapter.Prefs.Path, logPath);
        fileConf.Set(XLog.FileAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
        fileConf.Set(XLog.FileAdapter.Prefs.Rotate, false);
        prefs.Set("Log/File", fileConf);

        XLog.Setup(prefs);

        // 定义标签测试用例
        var tests = new[]
        {
            new
            {
                name = "Tag in args overrides global level",
                setup = new Action(() =>
                {
                    var tag = XLog.GetTag();
                    tag.Level = XLog.LevelType.Debug;
                    tag.Set("source", "test1");
                    XLog.Debug("Debug message", tag);
                }),
                expected = "[source = test1] Debug message"
            },
            new
            {
                name = "Context tag controls output",
                setup = new Action(() =>
                {
                    var tag = XLog.GetTag();
                    tag.Level = XLog.LevelType.Debug;
                    tag.Set("context", "test2");
                    XLog.Watch(tag);
                    XLog.Debug("Context debug message");
                    XLog.Defer();
                }),
                expected = "[context = test2] Context debug message"
            },
            new
            {
                name = "Multiple tag values",
                setup = new Action(() =>
                {
                    var tag = XLog.GetTag();
                    tag.Set("key1", "value1");
                    tag.Set("key2", "value2");
                    XLog.Info("Multi tag message", tag);
                }),
                expected = "[key1 = value1, key2 = value2] Multi tag message"
            },
            new
            {
                name = "Format string with tag",
                setup = new Action(() =>
                {
                    var tag = XLog.GetTag();
                    tag.Set("format", "test");
                    XLog.Info("Count: {0}", tag, 42);
                }),
                expected = "[format = test] Count: 42"
            }
        };

        // 执行所有测试用例
        foreach (var test in tests)
        {
            test.setup();
        }

        // 确保所有日志写入
        XLog.Close();

        // 验证结果
        Assert.That(XFile.HasFile(logPath), Is.True, "Log file does not exist");
        var content = XFile.OpenText(logPath);
        foreach (var test in tests)
        {
            Assert.That(content, Does.Contain(test.expected), test.name);
        }
    }

    /// <summary>
    /// 测试并发日志写入功能
    /// 验证：
    /// 1. 多线程并发写入的正确性
    /// 2. 日志内容的完整性
    /// 3. 异常处理机制
    /// 4. 线程安全性
    /// </summary>
    [Test]
    public void Concurrent()
    {
        // 创建基本配置
        var prefs = new XPrefs.IBase();
        var fileConf = new XPrefs.IBase();
        var logPath = Path.Combine(tempLogPath, "TestConcurrent.log");
        fileConf.Set(XLog.FileAdapter.Prefs.Path, logPath);
        fileConf.Set(XLog.FileAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
        fileConf.Set(XLog.FileAdapter.Prefs.Rotate, false); // 禁用日志轮转，便于测试
        prefs.Set("Log/File", fileConf);

        // 初始化日志系统
        XLog.Setup(prefs);

        // 配置并发测试参数
        const int numThreads = 2;  // 使用较少的线程数以便于测试
        const int numLogs = 10;    // 每个线程的日志数量
        var threads = new List<Thread>();
        var exceptions = new List<Exception>();
        var lockObj = new object();
        var barrier = new Barrier(numThreads);  // 用于同步所有线程的启动

        // 创建并启动测试线程
        for (int i = 0; i < numThreads; i++)
        {
            int threadId = i;
            var thread = new Thread(() =>
            {
                try
                {
                    // 等待所有线程就绪
                    barrier.SignalAndWait();

                    // 写入测试日志
                    var msg = "Thread " + threadId.ToString();
                    for (int j = 0; j < numLogs; j++)
                    {
                        XLog.Info(msg);
                        Thread.Sleep(1);  // 添加小延迟，避免写入过于密集
                    }
                }
                catch (Exception ex)
                {
                    // 安全地记录异常
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        // 等待所有线程完成
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // 确保日志写入并关闭系统
        XLog.Close();

        // 检查是否有异常发生
        if (exceptions.Count > 0) Assert.Fail($"并发测试过程中发生了 {exceptions.Count} 个异常");

        // 验证日志文件存在
        Assert.That(XFile.HasFile(logPath), Is.True, "日志文件未能成功创建");

        // 读取并验证日志内容
        var content = XFile.OpenText(logPath);
        Assert.That(content.Length, Is.GreaterThan(0), "日志文件内容为空");

        // 统计每个线程的日志数量
        var threadCounts = new int[numThreads];
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            for (int i = 0; i < numThreads; i++)
            {
                if (line.Contains($"Thread {i}"))
                {
                    threadCounts[i]++;
                    break;
                }
            }
        }

        // 验证每个线程的日志都被正确写入
        for (int i = 0; i < numThreads; i++)
        {
            Assert.That(threadCounts[i], Is.GreaterThan(0),
                $"线程 {i} 的日志未被写入");
            Assert.That(threadCounts[i], Is.LessThanOrEqualTo(numLogs),
                $"线程 {i} 写入的日志数量超出预期");
        }

        // 验证总日志数量在合理范围内
        var totalLogs = threadCounts.Sum();
        Assert.That(totalLogs, Is.InRange(numThreads * numLogs * 0.5, numThreads * numLogs * 1.5),
            $"总日志数量 {totalLogs} 不在预期范围内");
    }
}
#endif
