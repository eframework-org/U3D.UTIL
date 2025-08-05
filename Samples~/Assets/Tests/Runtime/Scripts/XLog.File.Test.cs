// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using EFramework.Utility;

public class TestXLogFile
{
    private class TestCase
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public TestExpected Expected { get; set; }
    }

    private class TestExpected
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }
    }

    private class WriteTestCase
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Rotate { get; set; }
        public bool Hourly { get; set; }
        public bool Daily { get; set; }
        public int MaxLine { get; set; }
        public int MaxFile { get; set; }
        public int WriteNum { get; set; }
        public bool CheckRotated { get; set; }
    }

    private class CleanupTestCase
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Hourly { get; set; }
        public int MaxHour { get; set; }
        public bool Daily { get; set; }
        public int MaxDay { get; set; }
        public bool CreateOld { get; set; }
    }

    private XLog.FileAdapter adapter;

    private XPrefs.IBase prefs;

    private string testLogDir;

    [SetUp]
    public void Setup()
    {
        adapter = new XLog.FileAdapter();
        prefs = new XPrefs.IBase();
        testLogDir = Path.Combine(Application.temporaryCachePath, "TestXLogFile");
        if (XFile.HasDirectory(testLogDir)) XFile.DeleteDirectory(testLogDir);
        XFile.CreateDirectory(testLogDir);
    }

    [TearDown]
    public void Reset()
    {
        adapter.Close();
        adapter = null;
        prefs = null;
        XFile.DeleteDirectory(testLogDir);
    }

    [Test]
    public void Prefs()
    {
        var prefsEditor = new XLog.FileAdapter.Prefs() as XPrefs.IEditor;

        // Save
        var targetPrefs = new XPrefs.IBase();
        prefsEditor.OnSave(source: new XPrefs.IBase(), target: targetPrefs);

        var targetConfig = targetPrefs.Get<XPrefs.IBase>(XLog.FileAdapter.Prefs.Config);
        Assert.NotNull(targetConfig, "Log/File 配置项应当存在。");

        Assert.AreEqual(XLog.FileAdapter.Prefs.LevelDefault, targetConfig.GetString(XLog.FileAdapter.Prefs.Level), "Log/File/Level 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.RotateDefault, targetConfig.GetBool(XLog.FileAdapter.Prefs.Rotate), "Log/File/Rotate 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.DailyDefault, targetConfig.GetBool(XLog.FileAdapter.Prefs.Daily), "Log/File/Daily 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.MaxDayDefault, targetConfig.GetInt(XLog.FileAdapter.Prefs.MaxDay), "Log/File/MaxDay 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.HourlyDefault, targetConfig.GetBool(XLog.FileAdapter.Prefs.Hourly), "Log/File/Hourly 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.MaxHourDefault, targetConfig.GetInt(XLog.FileAdapter.Prefs.MaxHour), "Log/File/MaxHour 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.PathDefault, targetConfig.GetString(XLog.FileAdapter.Prefs.Path), "Log/File/Path 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.MaxFileDefault, targetConfig.GetInt(XLog.FileAdapter.Prefs.MaxFile), "Log/File/MaxFile 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.MaxLineDefault, targetConfig.GetInt(XLog.FileAdapter.Prefs.MaxLine), "Log/File/MaxLine 配置项应当存在。");
        Assert.AreEqual(XLog.FileAdapter.Prefs.MaxSizeDefault, targetConfig.GetInt(XLog.FileAdapter.Prefs.MaxSize), "Log/File/MaxSize 配置项应当存在。");

        // Apply
        prefsEditor.OnApply(source: targetPrefs, target: targetPrefs, asset: false, remote: true);
        targetConfig = targetPrefs.Get<XPrefs.IBase>(XLog.FileAdapter.Prefs.Config);
        Assert.IsNull(targetConfig, "Log/File 配置项在远端应当被移除。");
    }

    [Test]
    public void Init()
    {
        var cases = new TestCase[] {
            new() {
                Name = "Normal Path",
                Path = XFile.PathJoin(testLogDir, "test.log"),
                Expected = new TestExpected {
                    Prefix = "test",
                    Suffix = ".log"
                }
            },
            new() {
                Name = "With Suffix Only",
                Path = XFile.PathJoin(testLogDir, ".log"),
                Expected = new TestExpected {
                    Prefix = "",
                    Suffix = ".log"
                }
            },
            new() {
                Name = "Directory Only",
                Path = testLogDir,
                Expected = new TestExpected {
                    Prefix = "",
                    Suffix = ".log"
                }
            }
        };

        foreach (var tc in cases)
        {
            TestContext.WriteLine($"Testing case: {tc.Name}");

            prefs.Set(XLog.FileAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
            prefs.Set(XLog.FileAdapter.Prefs.Path, tc.Path);

            adapter.Init(prefs);

            Assert.AreEqual(tc.Expected.Prefix, adapter.prefix, $"Case {tc.Name}: 期望文件名前缀为 {tc.Expected.Prefix}，实际为 {adapter.prefix}。");
            Assert.AreEqual(tc.Expected.Suffix, adapter.suffix, $"Case {tc.Name}: 期望文件扩展名为 {tc.Expected.Suffix}，实际为 {adapter.suffix}。");
        }
    }

    [Test]
    public void Rotate()
    {
        var cases = new WriteTestCase[] {
            new() {
                Name = "Normal Write Without Rotation",
                Path = XFile.PathJoin(testLogDir, "test1.log"),
                Rotate = false,
                WriteNum = 3,
                CheckRotated = false
            },
            new() {
                Name = "Write With Line Rotation",
                Path = XFile.PathJoin(testLogDir, "test2.log"),
                Rotate = true,
                MaxLine = 2,
                MaxFile = 2,
                WriteNum = 5,
                CheckRotated = true
            },
            new() {
                Name = "Write With Suffix Only",
                Path = XFile.PathJoin(testLogDir, ".customext"),
                Rotate = true,
                MaxLine = 2,
                MaxFile = 2,
                WriteNum = 5,
                CheckRotated = true
            },
            new() {
                Name = "Write With Directory Only",
                Path = testLogDir,
                Rotate = true,
                MaxLine = 2,
                MaxFile = 2,
                WriteNum = 5,
                CheckRotated = true
            }
        };

        foreach (var tc in cases)
        {
            TestContext.WriteLine($"Testing case: {tc.Name}");

            prefs.Set(XLog.FileAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
            prefs.Set(XLog.FileAdapter.Prefs.Path, tc.Path);
            prefs.Set(XLog.FileAdapter.Prefs.Rotate, tc.Rotate);
            prefs.Set(XLog.FileAdapter.Prefs.MaxLine, tc.MaxLine);
            prefs.Set(XLog.FileAdapter.Prefs.MaxFile, tc.MaxFile);
            prefs.Set(XLog.FileAdapter.Prefs.Hourly, tc.Hourly);
            prefs.Set(XLog.FileAdapter.Prefs.Daily, tc.Daily);

            adapter.Init(prefs);

            // 写入日志
            var logTime = DateTime.Now;
            for (var i = 0; i < tc.WriteNum; i++)
            {
                adapter.Write(new XLog.LogData
                {
                    Level = XLog.LevelType.Info,
                    Force = false,
                    Time = ((DateTimeOffset)logTime).ToUnixTimeMilliseconds(),
                    Data = $"Test log message {i}"
                });
            }
            adapter.Close();

            // 检查原始日志文件是否存在
            Assert.IsTrue(File.Exists(adapter.path), $"期望日志文件存在于路径 {adapter.path}。");

            if (tc.CheckRotated)
            {
                // 检查轮转的日志文件
                var dir = Path.GetDirectoryName(adapter.path);
                var files = Directory.GetFiles(dir)
                    .Where(f => !Directory.Exists(f) &&
                               Path.GetFileName(f).StartsWith(adapter.prefix) &&
                               Path.GetFileName(f).EndsWith(adapter.suffix))
                    .ToList();

                var expectedFiles = tc.MaxFile + 1; // 包括当前文件
                Assert.GreaterOrEqual(files.Count, expectedFiles,
                    $"Case {tc.Name}: 期望至少存在 {expectedFiles} 个日志文件，实际找到 {files.Count} 个。" +
                    $"找到的文件: {string.Join(", ", files.Select(Path.GetFileName))}。");
            }
        }
    }

    [Test]
    public void Cleanup()
    {
        var cases = new CleanupTestCase[] {
            new() {
                Name = "Hourly Cleanup",
                Path = XFile.PathJoin(testLogDir, "test1.log"),
                Hourly = true,
                MaxHour = 1,
                CreateOld = true
            },
            new() {
                Name = "Daily Cleanup",
                Path = XFile.PathJoin(testLogDir, "test2.log"),
                Daily = true,
                MaxDay = 1,
                CreateOld = true
            },
            new() {
                Name = "Suffix Only Cleanup",
                Path = XFile.PathJoin(testLogDir, ".log"),
                Hourly = true,
                MaxHour = 1,
                CreateOld = true
            }
        };

        foreach (var tc in cases)
        {
            TestContext.WriteLine($"Testing case: {tc.Name}");

            prefs.Set(XLog.FileAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
            prefs.Set(XLog.FileAdapter.Prefs.Path, tc.Path);
            prefs.Set(XLog.FileAdapter.Prefs.Rotate, true);
            prefs.Set(XLog.FileAdapter.Prefs.Hourly, tc.Hourly);
            prefs.Set(XLog.FileAdapter.Prefs.MaxHour, tc.MaxHour);
            prefs.Set(XLog.FileAdapter.Prefs.Daily, tc.Daily);
            prefs.Set(XLog.FileAdapter.Prefs.MaxDay, tc.MaxDay);

            adapter.Init(prefs);

            if (tc.CreateOld)
            {
                // 创建旧的日志文件
                var now = DateTime.Now;
                var oldTime = tc.Hourly ? now.AddHours(-2) : now.AddDays(-2);
                var oldTimeStr = tc.Hourly ?
                    oldTime.ToString("yyyy-MM-dd-HH") :
                    oldTime.ToString("yyyy-MM-dd");

                var oldFiles = string.IsNullOrEmpty(adapter.prefix) ?
                    new[] {
                        XFile.PathJoin(Path.GetDirectoryName(tc.Path), $"{oldTimeStr}.001{adapter.suffix}"),
                        XFile.PathJoin(Path.GetDirectoryName(tc.Path), $"{oldTimeStr}.002{adapter.suffix}")
                    } :
                    new[] {
                        XFile.PathJoin(Path.GetDirectoryName(tc.Path), $"{adapter.prefix}.{oldTimeStr}.001{adapter.suffix}"),
                        XFile.PathJoin(Path.GetDirectoryName(tc.Path), $"{adapter.prefix}.{oldTimeStr}.002{adapter.suffix}")
                    };

                // 创建文件并设置修改时间
                foreach (var file in oldFiles)
                {
                    File.WriteAllText(file, "old log");
                    File.SetLastWriteTime(file, oldTime);
                }

                // 执行清理
                adapter.DeleteOld();

                // 验证文件是否被删除
                foreach (var file in oldFiles)
                {
                    Assert.IsFalse(File.Exists(file), $"Case {tc.Name}: 期望过期日志文件 {file} 已被删除。");
                }
            }
        }
    }
}
#endif
