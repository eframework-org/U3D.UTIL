// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using EFramework.Utility;

public class TestXLoom : MonoBehaviour
{
    [UnityTest]
    public IEnumerator Timeout()
    {
        // Arrange
        var called = false;
        var startTime = XTime.GetMillisecond();
        long deltaTime = 0;
        void callback()
        {
            called = true;
            deltaTime = XTime.GetMillisecond() - startTime;
        }

        // Act
        XLoom.SetTimeout(callback, 200);
        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(called, "Timeout 定时器应在到达指定时间后触发回调");
        var offset = Time.fixedDeltaTime * 1000;
        Assert.That(deltaTime, Is.InRange(200 - offset, 200 + offset), "Timeout 定时器应当在指定时间范围内回调");

        // Assert
        called = false;
        var timer = XLoom.SetTimeout(callback, 200);
        XLoom.ClearTimeout(timer); // 清理定时器
        yield return new WaitForSeconds(0.5f);
        Assert.IsFalse(called, "被清除的 Timeout 定时器不应当被回调");
    }

    [UnityTest]
    public IEnumerator Interval()
    {
        // Arrange
        var called = false;
        var startTime = XTime.GetMillisecond();
        long deltaTime = 0;
        void callback()
        {
            called = true;
            if (deltaTime == 0)
            {
                deltaTime = XTime.GetMillisecond() - startTime;
            }
        }

        // Act
        var timer = XLoom.SetInterval(callback, 200);
        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(called, "Interval 定时器应在到达指定时间后触发回调");
        var offset = Time.fixedDeltaTime * 1000;
        Assert.That(deltaTime, Is.InRange(200 - offset, 200 + offset), "Interval 定时器应当在指定时间范围内回调");

        called = false;
        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(called, "Interval 定时器应在到达指定时间后再次触发回调");

        // Assert
        called = false;
        XLoom.ClearInterval(timer); // 清理定时器
        yield return new WaitForSeconds(0.5f);
        Assert.IsFalse(called, "被清除的 Interval 定时器不应当被回调");
    }

    [Test]
    public void RunInMain()
    {
        // Arrange
        var called = false;
        void callback() => called = true;

        // Act
        XLoom.RunInMain(callback);

        // Assert
        Assert.IsTrue(called, "在主线程中的任务应立即执行");
    }

    [UnityTest]
    public IEnumerator RunInNext()
    {
        // Arrange
        var called = false;
        void callback() => called = true;

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
        var called = false;
        void callback() => called = true;

        // Act
        var task = XLoom.RunAsync(callback);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsTrue(called, "异步任务应在完成后设置标志");
    }

    [UnityTest]
    public IEnumerator StartCoroutine()
    {
        // Arrange
        var called = false;
        IEnumerator coroutine = TestCoroutine(() => called = true);

        // Act
        XLoom.StartCoroutine(coroutine);
        Assert.IsFalse(called, "协程不应立即执行完成");
        yield return new WaitForSeconds(0.1f);

        // Assert
        Assert.IsTrue(called, "协程应在等待时间后执行完成");
    }

    [UnityTest]
    public IEnumerator StopCoroutine()
    {
        // Arrange
        var called = false;
        IEnumerator coroutine = TestCoroutine(() => called = true);
        Coroutine cr = XLoom.StartCoroutine(coroutine);

        // Act
        XLoom.StopCoroutine(cr);
        yield return new WaitForSeconds(0.1f);

        // Assert
        Assert.IsFalse(called, "已停止的协程不应执行回调");
    }

    private IEnumerator TestCoroutine(Action action)
    {
        yield return new WaitForSeconds(0.1f);
        action.Invoke();
    }
}
#endif
