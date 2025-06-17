// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine.Pool;

namespace EFramework.Utility
{
    public partial class XLog
    {
        /// <summary>
        /// 日志标签类，用于为日志添加结构化的标签信息。
        /// 支持键值对形式的标签和线程关联等特性。
        /// </summary>
        /// <remarks>
        /// 功能特性:
        /// - 支持键值对形式的标签
        /// - 支持标签的线程关联
        /// - 支持标签的对象池管理
        /// - 支持标签的字符串和字典表示
        /// </remarks>
        public class LogTag
        {
            /// <summary>
            /// 用于同步的锁对象
            /// </summary>
            internal readonly object lockObj = new();

            /// <summary>
            /// 日志输出级别
            /// </summary>
            internal LevelType level;

            /// <summary>
            /// 标签键列表
            /// </summary>
            internal readonly List<string> keys = new();

            /// <summary>
            /// 标签值列表
            /// </summary>
            internal readonly List<string> values = new();

            /// <summary>
            /// 标签的字符串表示
            /// </summary>
            internal string text;

            /// <summary>
            /// 标签的字典表示
            /// </summary>
            internal Dictionary<string, string> data;

            /// <summary>
            /// 当前标签数量
            /// </summary>
            internal int count;

            /// <summary>
            /// 是否需要重建字符串表示
            /// </summary>
            internal bool rebuildText;

            /// <summary>
            /// 是否需要重建字典表示
            /// </summary>
            internal bool rebuildData;

            /// <summary>
            /// 是否已被对象池回收
            /// </summary>
            internal bool pooled;

            /// <summary>
            /// 获取字符串表示
            /// </summary>
            public string Text
            {
                get
                {
                    lock (lockObj)
                    {
                        if (!rebuildText) return text;

                        rebuildText = false;
                        if (count > 0)
                        {
                            var builder = new StringBuilder();
                            builder.Append("[");
                            bool first = true;
                            for (int i = 0; i < count; i++)
                            {
                                if (!first)
                                {
                                    builder.Append(", ");
                                }
                                else
                                {
                                    first = false;
                                }
                                builder.AppendFormat("{0}={1}", keys[i], values[i]);
                            }
                            builder.Append("]");
                            text = builder.ToString();
                        }
                        else
                        {
                            text = "";
                        }
                        return text;
                    }
                }
            }

            /// <summary>
            /// 获取数据字典表示
            /// </summary>
            public Dictionary<string, string> Data
            {
                get
                {
                    lock (lockObj)
                    {
                        if (!rebuildData) return data;

                        rebuildData = false;
                        data = new Dictionary<string, string>();
                        if (count > 0)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                data[keys[i]] = values[i];
                            }
                        }
                        return data;
                    }
                }
            }

            /// <summary>
            /// 日志级别
            /// </summary>
            public LevelType Level
            {
                get => level;
                set => level = value;
            }

            public LogTag() { Reset(); }

            /// <summary>
            /// 重置标签状态
            /// </summary>
            public LogTag Reset()
            {
                level = LevelType.Undefined;
                text = "";
                data = null;
                count = 0;
                rebuildText = true;
                rebuildData = true;
                return this;
            }

            /// <summary>
            /// 设置键值对
            /// </summary>
            public LogTag Set(string key, string value)
            {
                lock (lockObj)
                {
                    int oindex = -1;
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (keys[i] == key)
                            {
                                oindex = i;
                                break;
                            }
                        }
                    }

                    if (oindex != -1)
                    {
                        keys[oindex] = key;
                        values[oindex] = value;
                    }
                    else
                    {
                        if (count >= keys.Count || keys.Count == 0)
                        {
                            keys.Add(key);
                            values.Add(value);
                        }
                        else
                        {
                            keys[count] = key;
                            values[count] = value;
                        }
                        count++;
                    }
                    rebuildText = true;
                    rebuildData = true;
                }
                return this;
            }

            /// <summary>
            /// 获取指定键的值
            /// </summary>
            public string Get(string key)
            {
                lock (lockObj)
                {
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (keys[i] == key)
                            {
                                return values[i];
                            }
                        }
                    }
                    return "";
                }
            }

            /// <summary>
            /// 创建新的标签实例
            /// </summary>
            public LogTag Clone()
            {
                lock (lockObj)
                {
                    var ntag = GetTag();
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            ntag.Set(keys[i], values[i]);
                        }
                    }
                    return ntag;
                }
            }

            public override string ToString() { return Text; }
        }

        /// <summary>
        /// 线程关联的标签映射
        /// </summary>
        internal static readonly ConcurrentDictionary<int, LogTag> tagContext = new();

        /// <summary>
        /// 标签对象池
        /// </summary>
        internal static ObjectPool<LogTag> tagPool = new(() => new LogTag(), null, null, null);

        /// <summary>
        /// 获取标签实例
        /// </summary>
        /// <returns>新的标签实例</returns>
        public static LogTag GetTag()
        {
            lock (tagPool)
            {
                var tag = tagPool.Get();
                tag.pooled = false;
                return tag;
            }
        }

        /// <summary>
        /// 回收标签实例到对象池
        /// </summary>
        /// <param name="tag">要回收的标签实例</param>
        public static void PutTag(LogTag tag)
        {
            if (tag != null && !tag.pooled)
            {
                lock (tagPool)
                {
                    if (!tag.pooled)
                    {
                        tag.Reset();
                        tag.pooled = true;
                        tagPool.Release(tag);
                    }
                }
            }
        }

        /// <summary>
        /// 监视当前线程的标签
        /// </summary>
        /// <param name="tag">要监视的标签实例，为null时创建新实例</param>
        /// <returns>监视的标签实例</returns>
        public static LogTag Watch(LogTag tag = null)
        {
            var tmpTag = tag ?? GetTag();
            tagContext[Thread.CurrentThread.ManagedThreadId] = tmpTag;
            return tmpTag;
        }

        /// <summary>
        /// 获取或创建当前线程的标签
        /// </summary>
        /// <param name="pairs">标签键值对数组</param>
        /// <returns>当前线程的标签实例</returns>
        public static LogTag Tag(params string[] pairs)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            if (pairs == null || pairs.Length == 0)
            {
                return tagContext.TryGetValue(threadId, out var existingTag) ? existingTag : null;
            }

            var tmpTag = GetTag();
            if (!tagContext.TryAdd(threadId, tmpTag))
            {
                PutTag(tmpTag);
                tmpTag = tagContext[threadId];
            }

            if (pairs.Length > 0)
            {
                if (pairs.Length == 1)
                {
                    tmpTag.Set(pairs[0], "");
                }
                else
                {
                    for (int i = 0; i < pairs.Length; i += 2)
                    {
                        tmpTag.Set(pairs[i], i + 1 < pairs.Length ? pairs[i + 1] : "");
                    }
                }
            }

            return tmpTag;
        }

        /// <summary>
        /// 移除当前线程的标签
        /// </summary>
        public static void Defer()
        {
            if (tagContext.TryRemove(Thread.CurrentThread.ManagedThreadId, out var tag))
            {
                PutTag(tag);
            }
        }
    }
}