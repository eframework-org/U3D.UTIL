// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EFramework.Utility
{
    /// <summary>
    /// XPool 提供了一个对象缓存工具集，实现了基础对象池、Unity 游戏对象池和字节流缓冲池。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 支持基础对象缓存：提供线程安全的泛型对象池，支持自动创建和复用对象
    /// - 支持游戏对象缓存：提供场景级和全局级的预制体实例池，支持自动回收和复用
    /// - 支持字节流缓存：提供高性能的字节缓冲池，支持自动扩容和复用
    /// - 线程安全设计：所有缓存操作都是线程安全的，支持多线程并发访问
    /// 
    /// 使用手册
    /// 1. 基础对象缓存
    /// 
    /// 1.1 泛型对象池
    ///     // 获取对象
    ///     var obj = XPool.SObject&lt;List&lt;int&gt;&gt;.Get();
    ///     obj.Add(1);
    ///     
    ///     // 回收对象
    ///     XPool.SObject&lt;List&lt;int&gt;&gt;.Put(obj);
    ///     
    ///     // 对象会被自动复用
    ///     var obj2 = XPool.SObject&lt;List&lt;int&gt;&gt;.Get();
    ///     Assert.That(obj2, Is.SameAs(obj));  // true
    /// 
    /// 1.2 非泛型对象池
    ///     // 使用类型创建对象池
    ///     var pool = new XPool.SObject(typeof(List&lt;int&gt;));
    ///     
    ///     // 使用委托创建对象池
    ///     var pool2 = new XPool.SObject(() =&lt; new List&lt;int&gt;());
    ///     
    ///     // 获取和回收对象
    ///     var obj = pool.Get();
    ///     pool.Put(obj);
    /// 
    /// 2. 游戏对象缓存
    /// 
    /// 2.1 设置预制体
    ///     // 设置钩子函数处理自定义的缓存逻辑，如加载 GameObject 对象，持有 AssetBundle 引用等
    ///     XPool.GObject.OnSet = new System.Func&lt;string, GameObject, XPool.GObject.CacheType, GameObject&gt;((key, origin, cache) =&lt;
    ///     {
    ///         if(origin == null) origin = Resources.Load&lt;GameObject&gt;(key); // 示例：若 origin 不存在，则使用 key 加载
    ///         return origin;
    ///     });
    /// 
    ///     // 注册预制体（场景级）
    ///     var prefab = new GameObject("TestPrefab");
    ///     XPool.GObject.Set("test_prefab", prefab);
    ///     
    ///     // 注册预制体（全局级）
    ///     XPool.GObject.Set("global_prefab", prefab, XPool.GObject.CacheType.Global);
    ///     
    ///     // 检查预制体是否存在
    ///     var exists = XPool.GObject.Has("test_prefab");
    /// 
    /// 2.2 获取实例
    ///     // 基本实例化
    ///     var obj = XPool.GObject.Get("test_prefab");
    ///     
    ///     // 带参数实例化
    ///     var obj2 = XPool.GObject.Get("test_prefab", 
    ///         active: true,                         // 是否激活
    ///         position: new Vector3(0, 1, 0),      // 世界坐标
    ///         rotation: Quaternion.identity,        // 世界朝向
    ///         scale: new Vector3(1, 1, 1),         // 本地缩放
    ///         life: 1000);                         // 生命周期（毫秒）
    /// 
    /// 2.3 回收实例
    ///     // 立即回收
    ///     XPool.GObject.Put(obj);
    ///     
    ///     // 延迟回收
    ///     XPool.GObject.Put(obj, delay: 1000);  // 1 秒后回收
    /// 
    /// 2.4 移除预制体
    ///     // 移除预制体及其所有实例
    ///     XPool.GObject.Del("test_prefab");
    /// 
    /// 3. 字节流缓存
    /// 
    /// 3.1 获取缓冲区
    ///     // 创建指定大小的缓冲区
    ///     var buffer = XPool.SBuffer.Get(1024);
    ///     
    ///     // 写入数据
    ///     buffer.Writer.Write(new byte[] { 1, 2, 3, 4 });
    ///     buffer.Flush();  // 更新长度并重置位置
    ///     
    ///     // 读取数据
    ///     var data = buffer.ToArray();
    /// 
    /// 3.2 复制数据
    ///     // 创建目标数组
    ///     var dst = new byte[1024];
    ///     
    ///     // 复制数据
    ///     buffer.CopyTo(srcOffset: 0, dst, dstOffset: 0, count: 1024);
    /// 
    /// 3.3 回收缓冲区
    ///     // 回收到缓冲池
    ///     XPool.SBuffer.Put(buffer);
    ///     
    ///     // 释放资源
    ///     buffer.Dispose();
    /// 
    /// 3.4 缓冲区长度说明
    ///     - Length 表示有效数据长度，而不是底层数组容量
    ///     - 写入数据后必须调用 Flush() 更新 Length
    ///     - Reset() 会将 Length 重置为 -1
    ///     - 使用 ToArray() 时以 Length 为准截取数据
    /// 
    /// 3.5 字节流缓冲池机制
    ///     - Get() 方法会优先查找大于等于请求大小的缓存对象
    ///     - Put() 方法仅缓存小于 60KB 的对象
    ///     - 当池满时（500个），会释放最早缓存的对象
    ///     - 使用完毕后应调用 Put() 而不是 Dispose()
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XPool
    {
        /// <summary>
        /// SObject 是基础对象（System.Object）的缓存池，提供线程安全的泛型对象池实现。
        /// </summary>
        /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
        public class SObject<T> where T : class
        {
            /// <summary>
            /// PoolMax 是对象池的最大容量。
            /// </summary>
            internal const int PoolMax = 500;

            /// <summary>
            /// pools 是对象池的队列。
            /// </summary>
            internal static readonly Queue<T> pools = new();

            /// <summary>
            /// Get 从对象池获取对象实例。
            /// 如果池中有可用对象则返回缓存的对象，否则创建新对象。
            /// </summary>
            /// <returns>对象实例</returns>
            public static T Get()
            {
                T ret = null;
                if (pools.Count > 0)
                {
                    lock (pools)
                    {
                        try { ret = pools.Dequeue(); }
                        catch (Exception e)
                        {
                            XLog.Warn($"XPool.SObject({typeof(T).FullName}): pools dequeue error: {e.Message}");
                        }
                    }
                }
                ret ??= Activator.CreateInstance<T>();
                return ret;
            }

            /// <summary>
            /// Put 回收对象实例到对象池。
            /// 如果池未满则缓存对象，否则丢弃。
            /// </summary>
            /// <param name="obj">要回收的对象实例</param>
            public static void Put(T obj)
            {
                if (obj == null) return;
                if (pools.Count < PoolMax)
                {
                    lock (pools) pools.Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// SObject 是基础对象（System.Object）的缓存池，提供线程安全的非泛型对象池实现。
        /// </summary>
        public class SObject
        {
            /// <summary>
            /// PoolMax 是对象池最大容量。
            /// </summary>
            internal const int PoolMax = 500;

            /// <summary>
            /// pools 是对象池的队列。
            /// </summary>
            internal readonly Queue pools = new();

            /// <summary>
            /// type 是对象的类型。
            /// </summary>
            internal readonly Type type;

            /// <summary>
            /// activator 是对象的创建器。
            /// </summary>
            internal readonly Func<object> activator;

            /// <summary>
            /// 使用类型构造对象池 SObject 实例。
            /// </summary>
            /// <param name="type">对象类型</param>
            public SObject(Type type) { this.type = type; }

            /// <summary>
            /// 使用创建器构造对象池 SObject 实例。
            /// </summary>
            /// <param name="activator">对象创建器</param>
            public SObject(Func<object> activator) { this.activator = activator; }

            /// <summary>
            /// Get 从对象池获取对象实例。
            /// 如果池中有可用对象则返回缓存的对象，否则使用类型或创建器创建新对象。
            /// </summary>
            /// <returns>对象实例</returns>
            public object Get()
            {
                object ret = null;
                if (pools.Count > 0)
                {
                    lock (pools)
                    {
                        try { ret = pools.Dequeue(); }
                        catch (Exception e)
                        {
                            var str = type != null ? type.FullName : activator != null ? activator.Method.DeclaringType.Name : "null";
                            XLog.Warn($"XPool.SObject({str}): pools dequeue error: {e.Message}");
                        }
                    }
                }
                ret ??= activator != null ? activator.Invoke() : Activator.CreateInstance(type);
                return ret;
            }

            /// <summary>
            /// Put 回收对象实例到对象池。
            /// 如果池未满则缓存对象，否则丢弃。
            /// </summary>
            /// <param name="obj">要回收的对象实例</param>
            public void Put(object obj)
            {
                if (obj == null) return;
                if (pools.Count < PoolMax)
                {
                    lock (pools) pools.Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// GObject 是 Unity 游戏对象（GameObject）的缓存池，提供预制体实例的缓存和复用。
        /// </summary>
        public class GObject : MonoBehaviour
        {
            /// <summary>
            /// CacheType 枚举了缓存类型，决定对象的生命周期范围。
            /// </summary>
            public enum CacheType
            {
                /// <summary>
                /// Scene 是场景级的缓存，场景卸载时自动清理。
                /// </summary>
                Scene,

                /// <summary>
                /// Global 是全局级的缓存，直到进程结束才清理。
                /// </summary>
                Global,
            }

            /// <summary>
            /// CacheHandler 是缓存句柄，管理预制体及其实例。
            /// </summary>
            internal class CacheHandler
            {
                /// <summary>
                /// Path 是预制体的路径。
                /// </summary>
                public string Path;

                /// <summary>
                /// Origin 是预制体的原型。
                /// </summary>
                public GameObject Origin;

                /// <summary>
                /// Type 是缓存的类型。
                /// </summary>
                public CacheType Type;

                /// <summary>
                /// Pool 是实例的对象池。
                /// </summary>
                public Queue<GameObject> Pool = new();
            }

            /// <summary>
            /// pools 维护了所有的缓存。
            /// </summary>
            internal static readonly Dictionary<string, CacheHandler> pools = new();

            /// <summary>
            /// objects 维护了所有的对象。
            /// </summary>
            internal static readonly Dictionary<GameObject, byte> objects = new();

            /// <summary>
            /// usings 表示使用中的对象。
            /// </summary>
            internal static readonly Dictionary<GameObject, CacheHandler> usings = new();

            /// <summary>
            /// keysToRemove 用于 GC 优化。
            /// </summary>
            internal static readonly List<GameObject> keysToRemove = new();

            /// <summary>
            /// keysToRemove2 用于 GC 优化。
            /// </summary>
            internal static readonly List<string> keysToRemove2 = new();

            /// <summary>
            /// disposed 表示是否已释放。
            /// </summary>
            internal static bool disposed;

            /// <summary>
            /// instance 是 GObject 的单例。
            /// </summary>
            internal static GObject instance;

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
            internal static void OnInit()
            {
                SceneManager.sceneLoaded += (scene, _) =>
                {
                    if (instance == null)
                    {
                        var gameObject = new GameObject("[XPool]");
                        gameObject.AddComponent<GObject>();
                        DontDestroyOnLoad(gameObject);
                    }
                };
                SceneManager.sceneUnloaded += scene =>
                {
                    if (!scene.isSubScene)
                    {
                        if (!disposed && instance)
                        {
                            keysToRemove.Clear();
                            foreach (var kvp in usings)
                            {
                                if (kvp.Value.Type == CacheType.Scene) keysToRemove.Add(kvp.Key);
                            }
                            foreach (var key in keysToRemove)
                            {
                                usings.Remove(key);
                                try { DestroyImmediate(key); }
                                catch (Exception e) { XLog.Panic(e); }
                                finally { objects.Remove(key); }
                            }
                            keysToRemove.Clear();

                            keysToRemove2.Clear();
                            foreach (var kvp in pools)
                            {
                                if (kvp.Value.Type == CacheType.Scene) keysToRemove2.Add(kvp.Key);
                            }
                            foreach (var key in keysToRemove2)
                            {
                                var handler = pools[key];
                                pools.Remove(key);
                                objects.Remove(handler.Origin);
                                while (handler.Pool.Count > 0)
                                {
                                    var gameObject = handler.Pool.Dequeue();
                                    try { DestroyImmediate(gameObject); }
                                    catch (Exception e) { XLog.Panic(e); }
                                    finally { objects.Remove(gameObject); }
                                }
                            }
                            keysToRemove2.Clear();
                        }
                    }
                };
            }

            private void Awake()
            {
                instance = this;
                disposed = false;
            }

            private void OnDestroy()
            {
                disposed = true;
                instance = null;
                pools.Clear();
                usings.Clear();
            }

            /// <summary>
            /// Has 检查预制体是否已注册到对象池。
            /// </summary>
            /// <param name="key">预制体标识</param>
            /// <returns>是否存在</returns>
            public static bool Has(string key)
            {
                if (disposed) return false;
                if (instance == null) throw new Exception("XPool instance is null.");
                if (string.IsNullOrEmpty(key)) { XLog.Error("XPool.GObject.Has: key is null."); return false; }
                return pools.ContainsKey(key);
            }

            /// <summary>
            /// OnSet 是 Set 的钩子函数。
            /// 可以在该函数处理自定义的缓存逻辑，如加载 GameObject 对象，持有 AssetBundle 引用等。
            /// 返回的对象将作为缓存源实例。
            /// </summary>
            public static Func<string, GameObject, CacheType, GameObject> OnSet;

            /// <summary>
            /// Set 注册预制体到对象池。
            /// </summary>
            /// <param name="key">预制体标识</param>
            /// <param name="origin">预制体对象</param>
            /// <param name="cache">缓存类型</param>
            /// <returns>是否注册成功</returns>
            public static bool Set(string key, GameObject origin = null, CacheType cache = CacheType.Scene)
            {
                if (disposed) return false;
                if (instance == null) throw new Exception("XPool instance is null.");
                if (string.IsNullOrEmpty(key)) { XLog.Error("XPool.GObject.Set: key is null."); return false; }
                if (OnSet != null) origin = OnSet(key, origin, cache);

                if (Has(key)) { XLog.Warn("XPool.GObject.Set: key exists: {0}", key); return false; }
                else
                {
                    if (origin == null) { XLog.Warn("XPool.GObject.Set: origin is null."); return false; }

                    var handler = SObject<CacheHandler>.Get();
                    handler.Path = key;
                    handler.Origin = origin;
                    handler.Type = cache;
                    pools[key] = handler;
                    if (!objects.ContainsKey(origin)) objects.Add(origin, 0);
                    return true;
                }
            }

            /// <summary>
            /// Del 从对象池移除预制体及其所有实例。
            /// </summary>
            /// <param name="key">预制体标识</param>
            /// <returns>是否移除成功</returns>
            public static bool Del(string key)
            {
                if (disposed) return false;
                if (instance == null) throw new Exception("XPool instance is null.");
                if (string.IsNullOrEmpty(key)) { XLog.Error("XPool.GObject.Del: key is null."); return false; }

                if (Has(key) == false) return false;
                else
                {
                    pools.TryGetValue(key, out var handler);
                    pools.Remove(key);

                    objects.Remove(handler.Origin);
                    foreach (var v in handler.Pool) objects.Remove(v);

                    keysToRemove.Clear();
                    foreach (var kvp in usings)
                    {
                        if (kvp.Value == handler) keysToRemove.Add(kvp.Key);
                    }
                    foreach (var k in keysToRemove)
                    {
                        usings.Remove(k);
                        objects.Remove(k);
                    }

                    var sig = keysToRemove.Count > 0;
                    if (sig) keysToRemove.Clear();
                    return true;
                }
            }

            /// <summary>
            /// Get 从对象池获取预制体实例。
            /// </summary>
            /// <param name="key">预制体标识</param>
            /// <param name="active">是否激活对象</param>
            /// <param name="position">世界坐标</param>
            /// <param name="rotation">世界朝向</param>
            /// <param name="scale">本地缩放</param>
            /// <param name="life">生命周期（毫秒），-1 表示永久</param>
            /// <returns>预制体实例</returns>
            public static GameObject Get(string key, bool active = true, Vector3 position = default, Quaternion rotation = default, Vector3 scale = default, long life = -1)
            {
                if (disposed) return null;
                if (instance == null) throw new Exception("XPool instance is null.");
                if (string.IsNullOrEmpty(key)) { XLog.Error("XPool.GObject.Get: key is null."); return null; }

                if (pools.TryGetValue(key, out var handler))
                {
                    GameObject gameObject;
                    if (handler.Pool.Count > 0) gameObject = handler.Pool.Dequeue();
                    else
                    {
                        gameObject = Instantiate(handler.Origin);
                        gameObject.name = handler.Origin.name;
                        objects.Add(gameObject, 0);
                    }
                    gameObject.SetActive(active);
                    if (position != default) gameObject.transform.position = position;
                    if (rotation != default) gameObject.transform.rotation = rotation;
                    if (scale != default) gameObject.transform.localScale = scale;
                    usings[gameObject] = handler;
                    if (life > 0) XLoom.SetTimeout(() => Put(gameObject), life);
                    return gameObject;
                }
                return null;
            }

            /// <summary>
            /// Put 回收预制体实例到对象池。
            /// </summary>
            /// <param name="gameObject">预制体实例</param>
            /// <param name="delay">延迟回收时间（毫秒）</param>
            public static void Put(GameObject gameObject, long delay = -1)
            {
                if (disposed) return;
                if (instance == null) throw new Exception("XPool instance is null.");
                if (gameObject == null) { XLog.Error("XPool.GObject.Put: gameObject is null."); return; }

                if (delay > 0) XLoom.SetTimeout(() => DoPut(gameObject), delay);
                else DoPut(gameObject);
            }

            internal static void DoPut(GameObject gameObject)
            {
                if (!disposed && instance && gameObject) // 在延迟的生命周期里可能被删除了
                {
                    if (usings.TryGetValue(gameObject, out var handler))
                    {
                        gameObject.transform.parent = instance.transform;
                        gameObject.SetActive(false);
                        handler.Pool.Enqueue(gameObject);
                        usings.Remove(gameObject);
                    }
                    else
                    {
                        if (!objects.ContainsKey(gameObject)) // 避免多次回收
                        {
                            DestroyImmediate(gameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// SBuffer 是字节流（StreamBuffer）的缓存池，提供高性能的字节数组缓冲和复用。
        /// </summary>
        public sealed class SBuffer : IDisposable
        {
            /// <summary>
            /// buffer 是字节数组。
            /// </summary>
            internal byte[] buffer;

            /// <summary>
            /// stream 是内存流。
            /// </summary>
            internal MemoryStream stream;

            /// <summary>
            /// reader 是二进制读取器。
            /// </summary>
            internal BinaryReader reader;

            /// <summary>
            /// writer 是二进制写入器。
            /// </summary>
            internal BinaryWriter writer;

            /// <summary>
            /// Capacity 获取字节容量。
            /// </summary>
            public int Capacity { get => buffer.Length; }

            /// <summary>
            /// Length 获取或设置有效字节长度。
            /// </summary>
            public int Length { get; internal set; }

            /// <summary>
            /// Position 获取或设置当前读写位置。
            /// </summary>
            public int Position { get => (int)Stream.Position; set => Stream.Position = value; }

            /// <summary>
            /// Stream 获取内存流实例。
            /// </summary>
            public MemoryStream Stream
            {
                get
                {
                    stream ??= new MemoryStream(buffer, 0, buffer.Length, true, true);
                    return stream;
                }
            }

            /// <summary>
            /// Reader 获取二进制读取器。
            /// </summary>
            public BinaryReader Reader
            {
                get
                {
                    reader ??= new BinaryReader(Stream);
                    return reader;
                }
            }

            /// <summary>
            /// Writer 获取二进制写入器。
            /// </summary>
            public BinaryWriter Writer
            {
                get
                {
                    writer ??= new BinaryWriter(Stream);
                    return writer;
                }
            }

            /// <summary>
            /// Buffer 获取底层字节数组。
            /// </summary>
            public byte[] Buffer { get => buffer; }

            /// <summary>
            /// 使用现有字节数组构造缓冲区 SBuffer 实例。
            /// </summary>
            /// <param name="buffer">字节数组</param>
            /// <param name="offset">起始偏移</param>
            public SBuffer(byte[] buffer, int offset = 0)
            {
                this.buffer = buffer;
                Length = buffer.Length;
                Position = offset;
            }

            /// <summary>
            /// 构造指定大小的缓冲区 SBuffer 实例。
            /// </summary>
            /// <param name="size">缓冲区大小</param>
            public SBuffer(int size)
            {
                if (size < 0) throw new Exception("size must >= 0");
                buffer = new byte[size];
            }

            /// <summary>
            /// ToArray 将缓冲区数据转换为字节数组。
            /// </summary>
            /// <param name="offset">起始偏移</param>
            /// <param name="count">复制长度</param>
            /// <returns>字节数组</returns>
            public byte[] ToArray(int offset = 0, int count = 0)
            {
                if (count == 0) count = Length;
                byte[] bytes = new byte[count - offset];
                CopyTo(offset, bytes, 0, bytes.Length);
                return bytes;
            }

            /// <summary>
            /// CopyTo 将缓冲区数据复制到目标数组。
            /// </summary>
            /// <param name="srcOffset">源偏移</param>
            /// <param name="dst">目标数组</param>
            /// <param name="dstOffset">目标偏移</param>
            /// <param name="count">复制长度</param>
            public void CopyTo(int srcOffset, Array dst, int dstOffset, int count) { System.Buffer.BlockCopy(buffer, srcOffset, dst, dstOffset, count); }

            /// <summary>
            /// Flush 完成写入操作，更新数据长度并重置位置。
            /// </summary>
            public void Flush() { Length = Position; Stream.Seek(0, SeekOrigin.Begin); }

            /// <summary>
            /// Reset 重置缓冲区的状态。
            /// </summary>
            public void Reset() { Length = -1; Stream.Seek(0, SeekOrigin.Begin); }

            /// <summary>
            /// Dispose 释放缓冲区的资源。
            /// </summary>
            public void Dispose()
            {
                Length = -1;
                buffer = null;
                try { reader?.Close(); } catch { }
                try { writer?.Close(); } catch { }
                try { stream?.Close(); } catch { }
                try { stream?.Dispose(); } catch { }
                reader = null;
                writer = null;
                stream = null;
            }

            /// <summary>
            /// PoolMax 是缓冲池的最大容量。
            /// </summary>
            public static int PoolMax = 500;

            /// <summary>
            /// ByteMax 是单个缓冲区的最大字节数。
            /// </summary>
            public static int ByteMax = 60 * 1024;

            /// <summary>
            /// buffers 是缓冲池的列表。
            /// </summary>
            internal static List<SBuffer> buffers = new();

            /// <summary>
            /// buffersHash 是缓冲池的哈希表。
            /// </summary>
            internal static Dictionary<int, byte> buffersHash = new();

            /// <summary>
            /// Get 获取指定大小的缓冲区。
            /// 如果池中有合适大小的缓冲区则返回缓存的实例，否则创建新实例。
            /// </summary>
            /// <param name="expected">预期大小</param>
            /// <returns>缓冲区实例</returns>
            public static SBuffer Get(int expected)
            {
                if (expected < 0) throw new Exception("expected size must >= 0");
                SBuffer buffer = null;
                if (expected < ByteMax)
                {
                    lock (buffers)
                    {
                        for (int i = buffers.Count - 1; i >= 0; i--)
                        {
                            var tmp = buffers[i];
                            if (tmp.Capacity >= expected)
                            {
                                buffer = tmp;
                                buffer.Reset();
                                buffers.RemoveAt(i);
                                buffersHash.Remove(buffer.GetHashCode());
                                break;
                            }
                        }
                    }
                }
                buffer ??= new SBuffer(expected);
                return buffer;
            }

            /// <summary>
            /// Put 回收缓冲区到缓冲池。
            /// 如果缓冲区大小超过限制或池已满则不缓存。
            /// </summary>
            /// <param name="buffer">缓冲区实例</param>
            public static void Put(SBuffer buffer)
            {
                if (buffer == null || buffer.Length == 0) return;
                if (buffer.Length > ByteMax) return;
                lock (buffers)
                {
                    buffer.Reset();
                    if (!buffersHash.ContainsKey(buffer.GetHashCode()))
                    {
                        if (buffers.Count >= PoolMax)
                        {
                            var tmp = buffers[0];
                            tmp.Dispose();
                            buffers.RemoveAt(0);
                            buffersHash.Remove(tmp.GetHashCode());
                        }
                        buffers.Add(buffer);
                        buffersHash.Add(buffer.GetHashCode(), 0);
                    }
                }
            }
        }
    }
}
