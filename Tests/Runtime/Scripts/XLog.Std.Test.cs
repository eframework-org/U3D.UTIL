// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;
using EFramework.Utility;

public class TestXLogStd
{
    private XLog.StdAdapter adapter;
    private XPrefs.IBase prefs;

    [SetUp]
    public void Setup()
    {
        adapter = new XLog.StdAdapter();
        prefs = new XPrefs.IBase();
    }

    [TearDown]
    public void Reset()
    {
        adapter = null;
        prefs = null;
        XLog.batchMode = Application.isBatchMode;
    }

    [Test]
    public void Init()
    {
        // 测试默认配置
        Assert.AreEqual(XLog.LevelType.Info, adapter.Init(prefs),
            "期望默认日志级别为 Info");

        // 测试自定义日志级别
        prefs.Set(XLog.StdAdapter.Prefs.Level, XLog.LevelType.Debug.ToString());
        Assert.AreEqual(XLog.LevelType.Debug, adapter.Init(prefs),
            "期望自定义日志级别设置为 Debug");

        // 测试无效日志级别
        prefs.Set(XLog.StdAdapter.Prefs.Level, "InvalidLevel");
        Assert.AreEqual(XLog.LevelType.Undefined, adapter.Init(prefs),
            "期望无效日志级别返回 Undefined");
    }

    [Test]
    public void Write()
    {
        XLog.batchMode = true;
        // 1. 测试不同级别的日志输出
        adapter.Init(prefs);

        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Test info message" });
        LogAssert.Expect(LogType.Log, new Regex(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[I\] Test info message"));

        // 2. 测试带标签的日志
        var tag = XLog.GetTag();
        tag.Set("key", "value");
        LogAssert.Expect(LogType.Log, new Regex(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[I\] \[Tags: key = value\] Tagged message"));
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Tagged message", Tag = tag.Text });
        XLog.PutTag(tag);

        // 3. 测试强制输出
        LogAssert.Expect(LogType.Log, new Regex(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[I\] Forced message"));
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Forced message", Force = true });

        // 4. 测试日志级别过滤
        prefs.Set(XLog.StdAdapter.Prefs.Level, XLog.LevelType.Error.ToString());
        adapter.Init(prefs);
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Should not appear" });
    }

    [Test]
    public void Color()
    {
        XLog.batchMode = false;
        // 1. 测试彩色输出
        prefs.Set(XLog.StdAdapter.Prefs.Color, true);
        adapter.Init(prefs);
        LogAssert.Expect(LogType.Log, new Regex(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] <color=grey><b>\[I\]</b></color> Colored message"));
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Colored message" });

        // 2. 测试禁用彩色输出
        prefs.Set(XLog.StdAdapter.Prefs.Color, false);
        adapter.Init(prefs);
        LogAssert.Expect(LogType.Log, new Regex(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[I\] Non-colored message"));
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Non-colored message" });
    }
}
#endif
