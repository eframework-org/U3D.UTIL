// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.Utility;
using UnityEngine.TestTools;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// XEvent 单元测试类，验证事件管理器的核心功能。
/// </summary>
public class TestXEvent
{
    /// <summary>
    /// 测试事件注册功能。
    /// </summary>
    /// <param name="multiple">是否允许多重监听</param>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 首次注册回调应该成功
    /// 2. 多重监听模式下可以注册多个回调
    /// 3. 单一监听模式下不能注册多个回调
    /// </remarks>
    [TestCase(true)]   // 允许多重监听
    [TestCase(false)]  // 不允许多重监听
    public void Reg(bool multiple)
    {
        // Arrange
        var eventManager = new XEvent.Manager(multiple);
        void callback1(object[] args) { }
        void callback2(object[] args) { }

        // Act & Assert
        if (!multiple) LogAssert.Expect(LogType.Error, new Regex("doesn't support multiple register"));
        // 验证首次注册回调应该成功
        Assert.IsTrue(eventManager.Reg(1, callback1), "首次注册回调应该成功");
        // 验证多重监听设置的影响
        Assert.AreEqual(multiple, eventManager.Reg(1, callback2), "多重监听设置应正确影响重复注册的结果");
    }

    /// <summary>
    /// 测试事件注销功能。
    /// </summary>
    /// <param name="all">是否注销所有回调</param>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 注销指定回调应该成功
    /// 2. 注销所有回调应该成功
    /// </remarks>
    [TestCase(true)]    // 注销所有回调
    [TestCase(false)]   // 注销指定回调
    public void Unreg(bool all)
    {
        // Arrange
        var eventManager = new XEvent.Manager();
        void callback1(object[] args) { }
        void callback2(object[] args) { }
        eventManager.Reg(1, callback1);
        eventManager.Reg(1, callback2);

        // Act & Assert
        if (all)
        {
            // 验证注销所有回调应该成功
            Assert.IsTrue(eventManager.Unreg(1), "注销所有回调应该成功");
        }
        else
        {
            // 验证注销指定回调应该成功
            Assert.IsTrue(eventManager.Unreg(1, callback1), "注销指定回调应该成功");
        }
    }

    /// <summary>
    /// 测试事件清理功能。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 清理后所有事件回调应被移除
    /// </remarks>
    [Test]
    public void Clear()
    {
        // Arrange
        var eventManager = new XEvent.Manager();
        static void callback(object[] args) { }
        eventManager.Reg(1, callback);
        eventManager.Reg(2, callback);

        // Act
        eventManager.Clear();

        // Assert
        // 验证清理后无法获取之前注册的回调
        Assert.IsNull(eventManager.Get(1), "清理后事件1的回调应为空");
        Assert.IsNull(eventManager.Get(2), "清理后事件2的回调应为空");
    }

    /// <summary>
    /// 测试事件回调获取功能。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 获取已注册事件的回调应该成功
    /// 2. 获取未注册事件的回调应该返回空
    /// </remarks>
    [Test]
    public void Get()
    {
        // Arrange
        var eventManager = new XEvent.Manager();
        static void callback(object[] args) { }
        eventManager.Reg(1, callback);

        // Act & Assert
        var callbacks = eventManager.Get(1);
        // 验证已注册事件的回调列表
        Assert.IsNotNull(callbacks, "已注册事件的回调列表不应为空");
        Assert.AreEqual(1, callbacks.Count, "回调列表应包含1个回调");
        Assert.That(callbacks, Does.Contain((XEvent.Callback)callback), "回调列表应包含已注册的回调");

        var nonExistCallbacks = eventManager.Get(999);
        // 验证未注册事件的回调列表
        Assert.IsNull(nonExistCallbacks, "未注册事件的回调列表应为空");
    }

    /// <summary>
    /// 测试事件通知功能。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 普通回调的通知执行
    /// 2. 单次回调的通知执行
    /// 3. 回调执行顺序
    /// </remarks>
    [Test]
    public void Notify()
    {
        // Arrange
        var eventManager = new XEvent.Manager();
        int callCount1 = 0, callCount2 = 0;
        void callback1(object[] args) => callCount1++;
        void callback2(object[] args) => callCount2++;

        // 测试普通回调
        eventManager.Reg(1, callback1);
        eventManager.Reg(1, callback2);
        eventManager.Notify(1);
        // 验证普通回调的执行次数
        Assert.AreEqual(1, callCount1, "第一个回调应该被执行一次");
        Assert.AreEqual(1, callCount2, "第二个回调应该被执行一次");

        // 测试单次回调
        callCount1 = 0;
        callCount2 = 0;
        eventManager.Reg(2, callback1, true);  // once = true
        eventManager.Reg(2, callback2, false); // once = false

        eventManager.Notify(2);  // 两个回调都会执行
        // 验证首次通知时两个回调都应执行
        Assert.AreEqual(1, callCount1, "单次回调应该在首次通知时执行");
        Assert.AreEqual(1, callCount2, "普通回调应该在首次通知时执行");

        eventManager.Notify(2);  // 只有非单次回调会执行
        // 验证二次通知时只有普通回调执行
        Assert.AreEqual(1, callCount1, "单次回调不应在二次通知时执行");
        Assert.AreEqual(2, callCount2, "普通回调应该在二次通知时执行");

        // 测试回调执行顺序
        int callOrder = 0;
        int callback3Order = 0, callback4Order = 0;
        void callback3(object[] args) => callback3Order = ++callOrder;
        void callback4(object[] args) => callback4Order = ++callOrder;

        eventManager.Reg(3, callback3);
        eventManager.Reg(3, callback4);
        eventManager.Notify(3);

        // 验证回调执行顺序
        Assert.That(callback3Order, Is.GreaterThan(0), "第一个回调应该被执行");
        Assert.That(callback4Order, Is.GreaterThan(0), "第二个回调应该被执行");
        Assert.That(callback4Order, Is.GreaterThan(callback3Order), "回调应该按注册顺序执行");
    }
}
#endif
