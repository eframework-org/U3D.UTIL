// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.Utility;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class TestXLogTag
{
    [Test]
    public void Basic()
    {
        // 测试基本的Set/Get操作
        var tag = XLog.GetTag();
        tag.Set("key1", "value1");
        tag.Set("key2", "value2");

        Assert.AreEqual("value1", tag.Get("key1"), "期望 key1 的值为 'value1'");
        Assert.AreEqual("value2", tag.Get("key2"), "期望 key2 的值为 'value2'");
        Assert.AreEqual("", tag.Get("nonexistent"), "期望不存在的键返回空字符串");

        // 测试文本格式化
        Assert.AreEqual("[key1=value1, key2=value2]", tag.Text, "期望标签文本格式正确");

        // 测试数据字典
        var data = tag.Data;
        Assert.AreEqual(2, data.Count, "期望字典包含2个键值对");
        Assert.AreEqual("value1", data["key1"], "期望字典中 key1 的值为 'value1'");
        Assert.AreEqual("value2", data["key2"], "期望字典中 key2 的值为 'value2'");

        // 测试日志级别
        tag.Level = XLog.LevelType.Debug;
        Assert.AreEqual(XLog.LevelType.Debug, tag.Level, "期望日志级别设置为 Debug");

        tag.Level = XLog.LevelType.Info;
        Assert.AreEqual(XLog.LevelType.Info, tag.Level, "期望日志级别设置为 Info");

        // 测试克隆功能
        var clonedTag = tag.Clone();
        Assert.AreEqual("value1", clonedTag.Get("key1"), "期望克隆标签包含原始标签的 key1 值");
        Assert.AreEqual("value2", clonedTag.Get("key2"), "期望克隆标签包含原始标签的 key2 值");
        Assert.AreEqual(tag.Text, clonedTag.Text, "期望克隆标签的文本表示与原始标签相同");

        clonedTag.Set("key3", "value3");
        Assert.AreEqual("", tag.Get("key3"), "期望原始标签不受克隆标签修改的影响");
        Assert.AreEqual("value3", clonedTag.Get("key3"), "期望克隆标签可以独立设置新的键值对");

        // 测试空标签
        var emptyTag = XLog.GetTag();
        Assert.AreEqual("", emptyTag.Text, "期望空标签的文本表示为空字符串");
        Assert.AreEqual(0, emptyTag.Data.Count, "期望空标签的字典为空");

        // 清理资源
        XLog.PutTag(tag);
        XLog.PutTag(clonedTag);
        XLog.PutTag(emptyTag);
    }

    [Test]
    public void Context()
    {
        const int ThreadCount = 4;  // 减少线程数
        var tasks = new Task[ThreadCount];
        var exceptions = new List<Exception>();
        var lockObj = new object();

        for (int i = 0; i < ThreadCount; i++)
        {
            var threadId = i;
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    // 每个线程设置自己的tag
                    var tag = XLog.GetTag();
                    tag.Set("thread", $"thread_{threadId}");
                    XLog.Watch(tag);

                    // 验证每个线程都能获取到自己的tag
                    var myTag = XLog.Tag();
                    Assert.That(myTag, Is.SameAs(tag));
                    Assert.AreEqual($"thread_{threadId}", myTag.Get("thread"));

                    // 添加更多的键值对，验证不会影响其他线程
                    myTag = XLog.Tag("key1", $"value1_{threadId}", "key2", $"value2_{threadId}");
                    Assert.That(myTag, Is.SameAs(tag));
                    Assert.AreEqual($"thread_{threadId}", myTag.Get("thread"));
                    Assert.AreEqual($"value1_{threadId}", myTag.Get("key1"));
                    Assert.AreEqual($"value2_{threadId}", myTag.Get("key2"));

                    // 清理当前线程的tag
                    XLog.Defer();
                    Assert.That(XLog.Tag(), Is.Null);
                }
                catch (Exception ex)
                {
                    lock (lockObj) exceptions.Add(ex);
                }
            });
        }

        // 等待所有线程完成
        Task.WaitAll(tasks);

        // 检查是否有异常发生
        Assert.That(exceptions, Is.Empty, "并发测试过程中不应出现异常");
    }
}
#endif
