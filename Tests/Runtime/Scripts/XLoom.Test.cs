// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.Utility;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.TestTools;

public class TestXLoom : MonoBehaviour
{
    [Test]
    public void SetTimeout()
    {
        // Arrange
        bool called = false;
        Action callback = () => called = true;

        // Act
        var timer = XLoom.SetTimeout(callback, 100);
        XLoom.Tick(50);
        Assert.IsFalse(called, "定时器在未到达指定时间时不应触发回调");
        XLoom.Tick(50);

        // Assert
        Assert.IsTrue(called, "定时器应在到达指定时间后触发回调");
        XLoom.ClearTimeout(timer); // 清理定时器
    }

    [Test]
    public void ClearTimeout()
    {
        // Arrange
        bool called = false;
        Action callback = () => called = true;
        var timer = XLoom.SetTimeout(callback, 100);

        // Act
        XLoom.ClearTimeout(timer);
        XLoom.Tick(200);

        // Assert
        Assert.IsFalse(called, "已清除的定时器不应触发回调");
    }

    [Test]
    public void SetInterval()
    {
        // Arrange
        int callCount = 0;
        Action callback = () =>
        {
            callCount++;
        };

        // Act
        var timer = XLoom.SetInterval(callback, 100);
        XLoom.Tick(100);
        XLoom.Tick(100);
        XLoom.Tick(100);

        // Assert
        Assert.Greater(callCount, 2, "重复定时器应多次触发回调");
        XLoom.ClearInterval(timer); // 清理定时器
    }

    [Test]
    public void ClearInterval()
    {
        // Arrange
        int callCount = 0;
        Action callback = () => callCount++;
        var timer = XLoom.SetInterval(callback, 100);

        // Act
        XLoom.ClearInterval(timer);
        XLoom.Tick(200);

        // Assert
        Assert.AreEqual(0, callCount, "已清除的重复定时器不应触发任何回调");
    }

    [Test]
    public void RunInMain()
    {
        // Arrange
        bool called = false;
        Action callback = () => called = true;

        // Act
        XLoom.RunInMain(callback);

        // Assert
        Assert.IsTrue(called, "在主线程中的任务应立即执行");
    }

    [UnityTest]
    public IEnumerator RunInNext()
    {
        // Arrange
        bool called = false;
        Action callback = () => called = true;

        // Act
        XLoom.RunInNext(callback);
        Assert.IsFalse(called, "下一帧执行的任务不应立即触发");
        yield return null;

        // Assert
        Assert.IsTrue(called, "任务应在下一帧被执行");
    }

    [UnityTest]
    public IEnumerator RunAsync()
    {
        // Arrange
        bool called = false;
        Action callback = () => called = true;

        // Act
        var task = XLoom.RunAsync(callback);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsTrue(called, "异步任务应在完成后设置标志");
    }

    [UnityTest]
    public IEnumerator StartCR()
    {
        // Arrange
        bool called = false;
        IEnumerator coroutine = TestCoroutine(() => called = true);

        // Act
        XLoom.StartCR(coroutine);
        Assert.IsFalse(called, "协程不应立即执行完成");
        yield return new WaitForSeconds(0.1f);

        // Assert
        Assert.IsTrue(called, "协程应在等待时间后执行完成");
    }

    private IEnumerator TestCoroutine(Action action)
    {
        yield return new WaitForSeconds(0.1f);
        action.Invoke();
    }

    [UnityTest]
    public IEnumerator StopCR()
    {
        // Arrange
        bool called = false;
        IEnumerator coroutine = TestCoroutine(() => called = true);
        Coroutine cr = XLoom.StartCR(coroutine);

        // Act
        XLoom.StopCR(cr);
        yield return new WaitForSeconds(0.1f);

        // Assert
        Assert.IsFalse(called, "已停止的协程不应执行回调");
    }
}
#endif
