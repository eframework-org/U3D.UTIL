// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;
using System.Collections.Generic;
using EFramework.Utility;
using System.Collections;
using System.Text.RegularExpressions;

public class TestXPool
{
    [Test]
    public void SObject()
    {
        // 测试基本的Get/Put功能
        var obj1 = XPool.SObject<List<int>>.Get();
        Assert.IsNotNull(obj1, "从对象池获取的实例不应为空");
        obj1.Add(1);
        XPool.SObject<List<int>>.Put(obj1);

        // 测试对象复用
        var obj2 = XPool.SObject<List<int>>.Get();
        Assert.That(obj2, Is.SameAs(obj1), "对象池应返回之前缓存的同一个实例");
        Assert.That(obj2, Has.Count.EqualTo(1), "复用的对象应保持原有状态");

        // 测试池子上限
        var objects = new List<List<int>>();
        for (int i = 0; i < XPool.SObject<List<int>>.PoolMax + 10; i++)
        {
            objects.Add(XPool.SObject<List<int>>.Get());
        }
        objects.ForEach(XPool.SObject<List<int>>.Put);
        Assert.That(XPool.SObject<List<int>>.pools, Has.Count.LessThanOrEqualTo(XPool.SObject<List<int>>.PoolMax), "对象池数量不应超过设定的上限值");

        // 测试多线程安全性
        var tasks = new List<Task>();
        var threadCount = 10;
        var operationsPerThread = 1000;
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    var obj = XPool.SObject<List<int>>.Get();
                    Assert.IsNotNull(obj, "多线程环境下从对象池获取的实例不应为空");
                    XPool.SObject<List<int>>.Put(obj);
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }

    [UnityTest]
    public IEnumerator GObject()
    {
        // 创建测试预制体
        var obj = new GameObject("TestObject");
        var key = "test_object";

        // 测试Set功能
        var onSet = XPool.GObject.OnSet;
        string onSetKey = null;
        GameObject onSetOrigin = null;
        XPool.GObject.CacheType onSetCache = XPool.GObject.CacheType.Scene;
        XPool.GObject.OnSet = new System.Func<string, GameObject, XPool.GObject.CacheType, GameObject>((key, origin, cache) =>
        {
            onSetKey = key;
            onSetOrigin = origin;
            onSetCache = cache;
            return origin;
        });
        Assert.IsTrue(XPool.GObject.Set(key, obj, XPool.GObject.CacheType.Global), "设置预制体到全局对象池应成功");
        Assert.IsTrue(XPool.GObject.Has(key), "对象池中应能找到已设置的预制体");
        Assert.AreEqual(onSetKey, key, "设置预制体的钩子函数 key 参数应当和传入的相等");
        Assert.AreEqual(onSetOrigin, obj, "设置预制体的钩子函数 origin 参数应当和传入的相等");
        Assert.AreEqual(onSetCache, XPool.GObject.CacheType.Global, "设置预制体的钩子函数 cache 参数应当和传入的相等");
        XPool.GObject.OnSet = onSet;

        LogAssert.Expect(LogType.Error, new Regex(Regex.Escape("XPool.GObject.Set: key is null.")));
        Assert.IsFalse(XPool.GObject.Set(null), "设置空键的对象池应当不成功");
        LogAssert.Expect(LogType.Error, new Regex(Regex.Escape("XPool.GObject.Set: key is null.")));
        Assert.IsFalse(XPool.GObject.Set(""), "设置空键的对象池应当不成功");
        Assert.IsFalse(XPool.GObject.Set(key), "重复设置相同键的对象池应当不成功");
        Assert.IsFalse(XPool.GObject.Set(key + "2", null), "设置空对象的对象池应当不成功");

        // 测试Get功能
        var obj1 = XPool.GObject.Get(key);
        Assert.IsNotNull(obj1, "从对象池实例化的游戏对象不应为空");
        Assert.AreEqual(obj.name, obj1.name, "实例化的对象名称应与预制体一致");

        // 测试Put功能
        XPool.GObject.Put(obj1);

        // 测试对象复用
        var obj2 = XPool.GObject.Get(key);
        Assert.That(obj2, Is.SameAs(obj1), "对象池应返回之前回收的同一个游戏对象实例");
        XPool.GObject.Put(obj2);

        // 测试自动回收
        var obj3 = XPool.GObject.Get(key, life: 500);
        yield return new WaitForSeconds(1);
        Assert.IsFalse(obj3.activeSelf, "对象池应自动回收游戏对象实例");

        // 测试延迟回收
        var obj4 = XPool.GObject.Get(key);
        XPool.GObject.Put(obj4, delay: 500);
        yield return new WaitForSeconds(1);
        Assert.IsFalse(obj4.activeSelf, "对象池应延迟回收游戏对象实例");

        // 测试Del功能
        Assert.IsTrue(XPool.GObject.Del(key), "从对象池中删除预制体应成功");
        Assert.IsFalse(XPool.GObject.Has(key), "删除后对象池中不应再存在该预制体");

        // 清理
        Object.Destroy(obj);
        Object.Destroy(obj1);
    }

    [Test]
    public void SBuffer()
    {
        // 测试Get创建新对象
        var buffer1 = XPool.SBuffer.Get(1024);
        Assert.IsNotNull(buffer1, "从缓冲池获取的字节流不应为空");
        Assert.AreEqual(1024, buffer1.Capacity, "字节流容量应与请求的大小一致");
        Assert.AreEqual(0, buffer1.Length, "新创建的字节流长度应为0");
        Assert.AreEqual(0, buffer1.Position, "新创建的字节流位置应为0");

        // 测试写入和Flush
        var testData = new byte[] { 1, 2, 3, 4 };
        buffer1.Writer.Write(testData);
        Assert.AreEqual(4, buffer1.Position, "写入数据后流位置应等于写入的数据长度");
        buffer1.Flush();
        Assert.AreEqual(4, buffer1.Length, "Flush后流长度应等于最后写入位置");
        Assert.AreEqual(0, buffer1.Position, "Flush后流位置应重置为0");

        // 测试Put和对象池
        var originalBuffer = buffer1.Buffer;
        XPool.SBuffer.Put(buffer1);
        var buffer2 = XPool.SBuffer.Get(1024);
        Assert.That(buffer2, Is.SameAs(buffer1), "缓冲池应返回之前缓存的同一个字节流实例");
        Assert.AreEqual(-1, buffer2.Length, "复用的字节流长度应被重置为-1");
        Assert.AreEqual(0, buffer2.Position, "复用的字节流位置应被重置为0");

        // 测试获取更大容量的buffer
        var buffer3 = XPool.SBuffer.Get(2048);
        Assert.That(buffer3, Is.Not.SameAs(buffer1), "请求更大容量时应创建新的字节流实例");
        Assert.AreEqual(2048, buffer3.Capacity, "新字节流容量应与请求的大小一致");

        // 测试ByteMax限制
        var largeBuffer = XPool.SBuffer.Get(XPool.SBuffer.ByteMax + 1);
        var largeBufferArray = largeBuffer.Buffer;
        XPool.SBuffer.Put(largeBuffer);
        var newLargeBuffer = XPool.SBuffer.Get(XPool.SBuffer.ByteMax + 1);
        Assert.That(newLargeBuffer, Is.Not.SameAs(largeBuffer), "超过最大字节限制的缓冲不应被复用");

        // 测试PoolMax限制
        var buffers = new List<XPool.SBuffer>();
        for (int i = 0; i < XPool.SBuffer.PoolMax + 10; i++)
        {
            buffers.Add(XPool.SBuffer.Get(1024));
        }
        buffers.ForEach(XPool.SBuffer.Put);
        Assert.That(XPool.SBuffer.buffers, Has.Count.LessThanOrEqualTo(XPool.SBuffer.PoolMax), "缓冲池中的实例数量不应超过设定的上限值");

        // 测试多线程安全性
        var tasks = new List<Task>();
        var threadCount = 10;
        var operationsPerThread = 100;
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    var buffer = XPool.SBuffer.Get(1024);
                    buffer.Writer.Write(j);
                    buffer.Flush();
                    XPool.SBuffer.Put(buffer);
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }
}
#endif
