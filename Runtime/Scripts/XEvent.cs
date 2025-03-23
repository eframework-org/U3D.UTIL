// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;

namespace EFramework.Utility
{
    /// <summary>
    /// XEvent 是一个轻量级的事件管理器，支持多重监听、单次及泛型回调和批量通知等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 多重监听：可配置是否允许同一事件注册多个回调
    /// - 单次回调：可设置回调函数仅执行一次后自动注销
    /// - 泛型回调：支持无参数、单参数和多参数的事件回调
    /// 
    /// 使用手册
    /// 1. 创建事件管理器
    /// 
    ///     // 创建支持多重监听的事件管理器
    ///     var eventManager = new XEvent.Manager(true);
    ///     
    ///     // 创建单一监听的事件管理器
    ///     var singleManager = new XEvent.Manager(false);
    /// 
    /// 2. 注册事件回调
    /// 
    ///     // 注册普通回调
    ///     eventManager.Reg(1, (args) => Console.WriteLine("Event 1"));
    ///     
    ///     // 注册单次回调
    ///     eventManager.Reg(2, (args) => Console.WriteLine("Once"), true);
    ///     
    ///     // 注册泛型回调
    ///     eventManager.Reg&lt;string&gt;(3, (msg) => Console.WriteLine(msg));
    /// 
    /// 3. 通知事件
    /// 
    ///     // 无参数通知
    ///     eventManager.Notify(1);
    ///     
    ///     // 带参数通知
    ///     eventManager.Notify(3, "Hello");
    /// 
    /// 4. 注销事件
    /// 
    ///     // 注销指定回调
    ///     eventManager.Unreg(1, callback);
    ///     
    ///     // 注销所有回调
    ///     eventManager.Clear();
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XEvent
    {
        /// <summary>
        /// 事件回调
        /// </summary>
        /// <param name="args">事件参数</param>
        public delegate void Callback(params object[] args);

        public class GenericProxy
        {
            public int ID;
            public Callback Callback;
        }

        /// <summary>
        /// 事件管理
        /// </summary>
        public class Manager
        {
            /// <summary>
            /// 多重监听
            /// </summary>
            protected bool Multiple;

            /// <summary>
            /// 事件回调
            /// </summary>
            protected Dictionary<int, List<Callback>> Callbacks;

            /// <summary>
            /// 回调一次
            /// </summary>
            protected Dictionary<Callback, bool> Onces;

            protected Dictionary<int, List<GenericProxy>> Proxies;

            /// <summary>
            /// 批量通知
            /// </summary>
            protected List<Callback> Batches;

            public Manager(bool multiple = true)
            {
                Multiple = multiple;
                Callbacks = new Dictionary<int, List<Callback>>();
                Onces = new Dictionary<Callback, bool>();
                Proxies = new Dictionary<int, List<GenericProxy>>();
                Batches = new List<Callback>(64);
            }

            /// <summary>
            /// 清除事件注册
            /// </summary>
            public virtual void Clear() { Callbacks.Clear(); Onces.Clear(); Proxies.Clear(); Batches.Clear(); }

            /// <summary>
            /// 获取事件回调
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <returns>事件回调</returns>
            public virtual List<Callback> Get(Enum eid) { return Callbacks.TryGetValue(eid.GetHashCode(), out var callbacks) ? callbacks : null; }

            /// <summary>
            /// 获取事件回调
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <returns>事件回调</returns>
            public virtual List<Callback> Get(int eid) { return Callbacks.TryGetValue(eid, out var callbacks) ? callbacks : null; }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg(Enum eid, Callback callback, bool once = false) { return Reg(eid.GetHashCode(), callback, once); }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg(int eid, Callback callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Reg: nil callback, eid={0}", eid);
                    return false;
                }
                if (Callbacks.TryGetValue(eid, out List<Callback> callbacks) == false)
                {
                    callbacks = new List<Callback>();
                    Callbacks.Add(eid, callbacks);
                }
                if (Multiple == false && callbacks.Count > 0)
                {
                    XLog.Error("XEvent.Manager.Reg: doesn't support multiple register, eid={0}", eid);
                    return false;
                }
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var temp = callbacks[i];
                    if (temp == callback) return false;
                }
                if (once) Onces[callback] = once;
                callbacks.Add(callback);
                return true;
            }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg(Enum eid, Action callback, bool once = false) { return Reg(eid.GetHashCode(), callback, once); }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg(int eid, Action callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Reg: nil callback, eid={0}", eid);
                    return false;
                }
                var ncallback = new Callback(args => callback?.Invoke());
                var ret = Reg(eid, ncallback, once);
                if (ret)
                {
                    if (!Proxies.ContainsKey(eid)) Proxies.Add(eid, new List<GenericProxy>());
                    var proxy = new GenericProxy
                    {
                        ID = callback.GetHashCode(),
                        Callback = ncallback
                    };
                    Proxies[eid].Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1>(Enum eid, Action<T1> callback, bool once = false) { return Reg(eid.GetHashCode(), callback, once); }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1>(int eid, Action<T1> callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Reg: nil callback, eid={0}", eid);
                    return false;
                }
                var ncallback = new Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    callback?.Invoke(arg1);
                });
                var ret = Reg(eid, ncallback, once);
                if (ret)
                {
                    if (!Proxies.ContainsKey(eid)) Proxies.Add(eid, new List<GenericProxy>());
                    var proxy = new GenericProxy
                    {
                        ID = callback.GetHashCode(),
                        Callback = ncallback
                    };
                    Proxies[eid].Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2>(Enum eid, Action<T1, T2> callback, bool once = false) { return Reg(eid.GetHashCode(), callback, once); }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2>(int eid, Action<T1, T2> callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Reg: nil callback, eid={0}", eid);
                    return false;
                }
                var ncallback = new Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    var arg2 = args != null && args.Length > 1 ? (T2)args[1] : default;
                    callback?.Invoke(arg1, arg2);
                });
                var ret = Reg(eid, ncallback, once);
                if (ret)
                {
                    if (!Proxies.ContainsKey(eid)) Proxies.Add(eid, new List<GenericProxy>());
                    var proxy = new GenericProxy
                    {
                        ID = callback.GetHashCode(),
                        Callback = ncallback
                    };
                    Proxies[eid].Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <typeparam name="T3">事件参数3</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2, T3>(Enum eid, Action<T1, T2, T3> callback, bool once = false) { return Reg(eid.GetHashCode(), callback, once); }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <typeparam name="T3">事件参数3</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2, T3>(int eid, Action<T1, T2, T3> callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Reg: nil callback, eid={0}", eid);
                    return false;
                }
                var ncallback = new Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    var arg2 = args != null && args.Length > 1 ? (T2)args[1] : default;
                    var arg3 = args != null && args.Length > 2 ? (T3)args[2] : default;
                    callback?.Invoke(arg1, arg2, arg3);
                });
                var ret = Reg(eid, ncallback, once);
                if (ret)
                {
                    if (!Proxies.ContainsKey(eid)) Proxies.Add(eid, new List<GenericProxy>());
                    var proxy = new GenericProxy
                    {
                        ID = callback.GetHashCode(),
                        Callback = ncallback
                    };
                    Proxies[eid].Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg(Enum eid, Callback callback = null) { return Unreg(eid.GetHashCode(), callback); }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg(int eid, Callback callback = null)
            {
                var ret = false;
                if (Callbacks.TryGetValue(eid, out var callbacks))
                {
                    if (callback != null)
                    {
                        if (callbacks.Count > 0)
                        {
                            ret = callbacks.Remove(callback);
                            if (callbacks.Count == 0) Callbacks.Remove(eid);
                        }
                        if (Onces.ContainsKey(callback)) Onces.Remove(callback);
                    }
                    else
                    {
                        ret = true;
                        for (var i = 0; i < callbacks.Count; i++)
                        {
                            var temp = callbacks[i];
                            if (Onces.ContainsKey(temp)) Onces.Remove(temp);
                        }
                        Callbacks.Remove(eid);
                    }
                }
                return ret;
            }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg(Enum eid, Action callback) { return Unreg(eid.GetHashCode(), callback); }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg(int eid, Action callback)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Unreg: nil callback, eid={0}", eid);
                    return false;
                }
                var ret = false;
                if (Proxies.TryGetValue(eid, out var proxies))
                {
                    for (int i = 0; i < proxies.Count;)
                    {
                        var proxy = proxies[i];
                        if (callback == null || proxy.ID == callback.GetHashCode())
                        {
                            proxies.RemoveAt(i);
                            if (Unreg(eid, proxy.Callback)) ret = true;
                        }
                        else i++;
                    }
                }
                return ret;
            }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1>(Enum eid, Action<T1> callback) { return Unreg(eid.GetHashCode(), callback); }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1>(int eid, Action<T1> callback)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Unreg: nil callback, eid={0}", eid);
                    return false;
                }
                var ret = false;
                if (Proxies.TryGetValue(eid, out var proxies))
                {
                    for (int i = 0; i < proxies.Count;)
                    {
                        var proxy = proxies[i];
                        if (callback == null || proxy.ID == callback.GetHashCode())
                        {
                            proxies.RemoveAt(i);
                            if (Unreg(eid, proxy.Callback)) ret = true;
                        }
                        else i++;
                    }
                }
                return ret;
            }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1, T2>(Enum eid, Action<T1, T2> callback) { return Unreg(eid.GetHashCode(), callback); }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1, T2>(int eid, Action<T1, T2> callback)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Unreg: nil callback, eid={0}", eid);
                    return false;
                }
                var ret = false;
                if (Proxies.TryGetValue(eid, out var proxies))
                {
                    for (int i = 0; i < proxies.Count;)
                    {
                        var proxy = proxies[i];
                        if (callback == null || proxy.ID == callback.GetHashCode())
                        {
                            proxies.RemoveAt(i);
                            if (Unreg(eid, proxy.Callback)) ret = true;
                        }
                        else i++;
                    }
                }
                return ret;
            }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <typeparam name="T3">事件参数3</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1, T2, T3>(Enum eid, Action<T1, T2, T3> callback) { return Unreg(eid.GetHashCode(), callback); }

            /// <summary>
            /// 注销事件
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <typeparam name="T3">事件参数3</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1, T2, T3>(int eid, Action<T1, T2, T3> callback)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Unreg: nil callback, eid={0}", eid);
                    return false;
                }
                var ret = false;
                if (Proxies.TryGetValue(eid, out var proxies))
                {
                    for (int i = 0; i < proxies.Count;)
                    {
                        var proxy = proxies[i];
                        if (callback == null || proxy.ID == callback.GetHashCode())
                        {
                            proxies.RemoveAt(i);
                            if (Unreg(eid, proxy.Callback)) ret = true;
                        }
                        else i++;
                    }
                }
                return ret;
            }

            /// <summary>
            /// 通知事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="args">事件参数</param>
            public virtual void Notify(Enum eid, params object[] args) { Notify(eid.GetHashCode(), args); }

            /// <summary>
            /// 通知事件
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="args">事件参数</param>
            public virtual void Notify(int eid, params object[] args)
            {
                if (Callbacks.TryGetValue(eid, out var callbacks))
                {
                    if (callbacks != null && callbacks.Count > 0)
                    {
                        Batches.Clear();
                        for (int i = 0; i < callbacks.Count;)
                        {
                            var callback = callbacks[i];
                            if (callback == null) callbacks.RemoveAt(i);
                            else
                            {
                                if (Onces.ContainsKey(callback)) callbacks.RemoveAt(i);
                                else i++;
                                Batches.Add(callback);
                            }
                        }
                        for (int i = 0; i < Batches.Count; i++)
                        {
                            var callback = Batches[i];
                            callback?.Invoke(args);
                        }
                        Batches.Clear(); // release references
                    }
                }
            }
        }
    }
}