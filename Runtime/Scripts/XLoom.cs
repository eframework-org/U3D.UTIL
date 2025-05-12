// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace EFramework.Utility
{
    /// <summary>
    /// XLoom 提供了一个统一的协程、线程和定时器管理工具，支持多线程任务调度、定时任务执行和协程管理。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 支持协程的创建、运行和停止
    /// - 支持定时器的创建、管理和清除
    /// - 支持多线程任务的调度和执行
    /// 
    /// 使用手册
    /// 1. 协程管理
    /// 
    /// 1.1 启动协程
    ///     // 创建并启动一个协程
    ///     IEnumerator YourCoroutine()
    ///     {
    ///         yield return new WaitForSeconds(0.1f);
    ///         // 执行协程逻辑
    ///     }
    ///     
    ///     // 启动协程并获取句柄
    ///     Coroutine cr = XLoom.StartCR(YourCoroutine());
    /// 
    /// 1.2 停止协程
    ///     // 使用协程句柄停止
    ///     XLoom.StopCR(cr);
    ///     
    ///     // 或使用协程迭代器停止
    ///     XLoom.StopCR(YourCoroutine());
    /// 
    /// 2. 定时器管理
    /// 
    /// 2.1 一次性定时器
    ///     // 设置一次性定时器，3 秒后执行
    ///     var timer = XLoom.SetTimeout(() => 
    ///     {
    ///         Debug.Log("3 秒后执行");
    ///     }, 3000);
    ///     
    ///     // 取消定时器
    ///     XLoom.ClearTimeout(timer);
    /// 
    /// 2.2 重复定时器
    ///     // 设置重复定时器，每 3 秒执行一次
    ///     var timer = XLoom.SetInterval(() => 
    ///     {
    ///         Debug.Log("每 3 秒执行一次");
    ///     }, 3000);
    ///     
    ///     // 取消定时器
    ///     XLoom.ClearInterval(timer);
    /// 
    /// 3. 线程调度
    /// 
    /// 3.1 主线程执行
    ///     // 在主线程中执行任务
    ///     await XLoom.RunInMain(() => 
    ///     {
    ///         // 需要在主线程执行的逻辑
    ///         Debug.Log("在主线程中执行");
    ///     });
    /// 
    /// 3.2 主线程检查
    ///     // 检查当前是否在主线程中执行
    ///     bool isMainThread = XLoom.IsInMain();
    /// 
    /// 3.3 下一帧执行
    ///     // 在主线程的下一帧执行任务
    ///     await XLoom.RunInNext(() => 
    ///     {
    ///         // 将在下一帧执行的逻辑
    ///         Debug.Log("在下一帧执行");
    ///     });
    /// 
    /// 3.4 异步执行
    ///     // 在其他线程中异步执行任务
    ///     await XLoom.RunAsync(() => 
    ///     {
    ///         // 异步执行的逻辑
    ///         Debug.Log("在其他线程执行");
    ///     });
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XLoom : MonoBehaviour
    {
        /// <summary>
        /// 定时器类，用于管理定时执行的任务。
        /// </summary>
        public class Timer
        {
            /// <summary>
            /// 原始时长（毫秒）
            /// </summary>
            public long Period;

            /// <summary>
            /// 剩余时长（毫秒）
            /// </summary>
            public long Tick;

            /// <summary>
            /// 是否重复执行
            /// </summary>
            public bool Repeat;

            /// <summary>
            /// 定时器回调函数
            /// </summary>
            public Action Callback;
        }

        /// <summary>
        /// XLoom 单例实例
        /// </summary>
        internal static XLoom instance;

        /// <summary>
        /// 是否已销毁
        /// </summary>
        internal static bool disposed;

        /// <summary>
        /// 主线程 ID
        /// </summary>
        internal static int mainThread;

        /// <summary>
        /// 待执行的任务队列
        /// </summary>
        internal static Queue<Action> allTasks = new();

        /// <summary>
        /// 批处理任务队列
        /// </summary>
        internal static Queue<Action> batchTasks = new();

        /// <summary>
        /// 定时器对象池
        /// </summary>
        internal static ObjectPool<Timer> timerPool = new(() => new Timer(), null, null, null);

        /// <summary>
        /// 所有活动的定时器列表
        /// </summary>
        internal static List<Timer> allTimers = new();

        /// <summary>
        /// 批处理定时器队列
        /// </summary>
        internal static Queue<Timer> batchTimers = new();

        /// <summary>
        /// 初始化 XLoom 实例。
        /// </summary>
        internal void Awake() { disposed = false; instance = this; }

        /// <summary>
        /// 更新定时器状态。
        /// </summary>
        internal void Update() { Tick((long)(Time.deltaTime * 1000)); }

        /// <summary>
        /// 清理 XLoom 实例。
        /// </summary>
        internal void OnDestroy() { disposed = true; instance = null; Reset(); }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        internal static void OnInit()
        {
            mainThread = Thread.CurrentThread.ManagedThreadId;
            Reset();

            void onPlay()
            {
                SceneManager.sceneLoaded += (scene, _) =>
                {
                    if (instance == null)
                    {
                        var go = new GameObject("[XLoom]");
                        go.AddComponent<XLoom>();
                        DontDestroyOnLoad(go);
                    }
                };
            }

#if UNITY_EDITOR
            var ltime = XTime.GetMillisecond();
            void onEdit()
            {
                var ntime = XTime.GetMillisecond();
                var dtime = ntime - ltime;
                ltime = ntime;
                Tick(dtime);
            }

            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) onPlay();
            else UnityEditor.EditorApplication.update += onEdit;

            UnityEditor.EditorApplication.playModeStateChanged += (mode) =>
            {
                if (mode == UnityEditor.PlayModeStateChange.ExitingEditMode)
                {
                    UnityEditor.EditorApplication.update -= onEdit;
                }
                else if (mode == UnityEditor.PlayModeStateChange.EnteredEditMode)
                {
                    ltime = XTime.GetMillisecond(); // 重置记录的最近时间
                    UnityEditor.EditorApplication.update += onEdit;
                }
            };
#else
            onPlay();
#endif
        }

        /// <summary>
        /// 更新定时器和任务状态。
        /// </summary>
        /// <param name="delta">时间增量（毫秒）</param>
        internal static void Tick(long delta)
        {
            // 处理任务队列
            lock (allTasks)
            {
                batchTasks.Clear();
                while (allTasks.Count > 0)
                {
                    batchTasks.Enqueue(allTasks.Dequeue());
                }
            }
            while (batchTasks.Count > 0)
            {
                var callback = batchTasks.Dequeue();
                try { callback?.Invoke(); }
                catch (Exception e) { XLog.Panic(e); }
            }

            // 处理定时器
            lock (allTimers)
            {
                batchTimers.Clear();
                for (int i = 0; i < allTimers.Count; i++)
                {
                    var timer = allTimers[i];
                    timer.Tick -= delta;
                    if (timer.Tick <= 0)
                    {
                        batchTimers.Enqueue(timer);
                    }
                }
            }
            while (batchTimers.Count > 0)
            {
                var timer = batchTimers.Dequeue();
                try { timer.Callback?.Invoke(); }
                catch (Exception e) { XLog.Panic(e); }
                if (timer.Repeat) timer.Tick = timer.Period;
                else ClearTimeout(timer);
            }
        }

        /// <summary>
        /// 重置所有任务和定时器。
        /// </summary>
        internal static void Reset()
        {
            lock (allTasks)
            {
                while (allTasks.Count > 0) // 在清理之前将异步任务执行完，避免阻塞线程
                {
                    var callback = allTasks.Dequeue();
                    try { callback?.Invoke(); }
                    catch (Exception e) { XLog.Panic(e); }
                }
                allTasks.Clear();
            }
            lock (batchTasks)
            {
                while (batchTasks.Count > 0) // 在清理之前将异步任务执行完，避免阻塞线程
                {
                    var callback = batchTasks.Dequeue();
                    try { callback?.Invoke(); }
                    catch (Exception e) { XLog.Panic(e); }
                }
                batchTasks.Clear();
            }
            lock (allTimers) allTimers.Clear();
            lock (batchTimers) batchTimers.Clear();
        }

        /// <summary>
        /// 启动协程。
        /// </summary>
        /// <remarks>
        /// 只能在主线程中调用，且必须在 Play 模式下。
        /// </remarks>
        /// <param name="cr">协程迭代器</param>
        /// <returns>协程句柄</returns>
        /// <exception cref="Exception">当应用未在运行或实例未初始化时抛出</exception>
        public static Coroutine StartCR(IEnumerator cr)
        {
            if (!Application.isPlaying) throw new Exception("Application is not playing.");
            if (instance == null) throw new Exception("XLoom instance is null.");
            if (cr == null) return null;
            if (disposed) return null;
            return instance.StartCoroutine(cr);
        }

        /// <summary>
        /// 停止协程。
        /// </summary>
        /// <remarks>
        /// 只能在主线程中调用，且必须在 Play 模式下。
        /// </remarks>
        /// <param name="cr">协程句柄</param>
        /// <exception cref="Exception">当应用未在运行或实例未初始化时抛出</exception>
        public static void StopCR(Coroutine cr)
        {
            if (!Application.isPlaying) throw new Exception("Application is not playing.");
            if (instance == null) throw new Exception("XLoom instance is null.");
            if (cr == null) return;
            if (disposed) return;
            instance.StopCoroutine(cr);
        }

        /// <summary>
        /// 停止协程。
        /// </summary>
        /// <remarks>
        /// 只能在主线程中调用，且必须在 Play 模式下。
        /// </remarks>
        /// <param name="cr">协程迭代器</param>
        /// <exception cref="Exception">当应用未在运行或实例未初始化时抛出</exception>
        public static void StopCR(IEnumerator cr)
        {
            if (!Application.isPlaying) throw new Exception("Application is not playing.");
            if (instance == null) throw new Exception("XLoom instance is null.");
            if (cr == null) return;
            if (disposed) return;
            instance.StopCoroutine(cr);
        }

        /// <summary>
        /// 检查当前是否在主线程中执行。
        /// </summary>
        /// <returns>是否在主线程中</returns>
        public static bool IsInMain() { return Thread.CurrentThread.ManagedThreadId == mainThread; }

        /// <summary>
        /// 在主线程中执行任务。
        /// </summary>
        /// <remarks>
        /// 如果当前已在主线程中，则直接执行；否则将任务加入队列等待主线程执行。
        /// </remarks>
        /// <param name="callback">要执行的任务</param>
        /// <returns>任务的异步操作句柄</returns>
        public static Task RunInMain(Action callback)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (IsInMain())
            {
                try { callback?.Invoke(); tcs.SetResult(true); }
                catch (Exception e) { XLog.Panic(e); tcs.SetException(e); }
            }
            else
            {
                var ncallback = new Action(() =>
                {
                    try { callback?.Invoke(); tcs.SetResult(true); }
                    catch (Exception e) { XLog.Panic(e); tcs.SetException(e); }
                });
                lock (allTasks) allTasks.Enqueue(ncallback);
            }
            return tcs.Task;
        }

        /// <summary>
        /// 在主线程的下一帧执行任务。
        /// </summary>
        /// <param name="callback">要执行的任务</param>
        /// <returns>任务的异步操作句柄</returns>
        public static Task RunInNext(Action callback)
        {
            var tcs = new TaskCompletionSource<bool>();
            var ncallback = new Action(() =>
            {
                try { callback?.Invoke(); tcs.SetResult(true); }
                catch (Exception e) { XLog.Panic(e); tcs.SetException(e); }
            });
            lock (allTasks) allTasks.Enqueue(ncallback);
            return tcs.Task;
        }

        /// <summary>
        /// 异步执行任务。
        /// </summary>
        /// <param name="callback">要执行的任务</param>
        /// <returns>任务的异步操作句柄</returns>
        public static Task RunAsync(Action callback)
        {
            var tcs = new TaskCompletionSource<bool>();
            var ncallback = new Action(() =>
            {
                try { callback?.Invoke(); tcs.SetResult(true); }
                catch (Exception e) { XLog.Panic(e); tcs.SetException(e); }
            });
            Task.Run(ncallback);
            return tcs.Task;
        }

        /// <summary>
        /// 设置一次性定时器。
        /// </summary>
        /// <param name="callback">定时器回调函数</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>定时器实例</returns>
        public static Timer SetTimeout(Action callback, long timeout)
        {
            var timer = timerPool.Get();
            timer.Callback = callback;
            timer.Period = timeout;
            timer.Tick = timeout;
            timer.Repeat = false;
            lock (allTimers) allTimers.Add(timer);
            return timer;
        }

        /// <summary>
        /// 清除一次性定时器。
        /// </summary>
        /// <param name="timer">要清除的定时器实例</param>
        public static void ClearTimeout(Timer timer)
        {
            lock (allTimers)
            {
                var idx = allTimers.IndexOf(timer);
                if (idx >= 0)
                {
                    allTimers.RemoveAt(idx);
                    timerPool.Release(timer);
                }
            }
        }

        /// <summary>
        /// 设置重复定时器。
        /// </summary>
        /// <param name="callback">定时器回调函数</param>
        /// <param name="interval">时间间隔（毫秒）</param>
        /// <returns>定时器实例</returns>
        public static Timer SetInterval(Action callback, long interval)
        {
            var timer = timerPool.Get();
            timer.Callback = callback;
            timer.Period = interval;
            timer.Tick = interval;
            timer.Repeat = true;
            lock (allTimers) allTimers.Add(timer);
            return timer;
        }

        /// <summary>
        /// 清除重复定时器。
        /// </summary>
        /// <param name="timer">要清除的定时器实例</param>
        public static void ClearInterval(Timer timer)
        {
            lock (allTimers)
            {
                var idx = allTimers.IndexOf(timer);
                if (idx >= 0)
                {
                    allTimers.RemoveAt(idx);
                    timerPool.Release(timer);
                }
            }
        }
    }
}
