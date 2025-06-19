// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace EFramework.Utility
{
    #region 基础类型
    /// <summary>
    /// XPrefs 是一个灵活高效的配置系统，实现了多源化配置的读写，支持可视化编辑、变量求值和命令行参数覆盖等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 多源化配置：支持内置配置（只读）、本地配置（可写）和远程配置（只读），支持多个配置源按优先级顺序读取
    /// - 多数据类型：支持基础类型（整数、浮点数、布尔值、字符串）、数组类型及配置实例（IBase）
    /// - 变量求值：支持通过命令行参数动态覆盖配置项，使用 ${Prefs.Key} 语法引用其他配置项
    /// - 可视化编辑：支持通过自定义面板拓展可视化的配置编辑功能
    /// 
    /// 使用手册
    /// 1. 基础配置操作
    /// 
    /// 1.1 检查配置项
    ///     // 检查配置项是否存在
    ///     bool exists = XPrefs.HasKey("configKey");
    /// 
    /// 1.2 读写基本类型
    ///     // 写入配置
    ///     XPrefs.Local.Set("intKey", 42);
    ///     XPrefs.Local.Set("floatKey", 3.14f);
    ///     XPrefs.Local.Set("boolKey", true);
    ///     XPrefs.Local.Set("stringKey", "value");
    /// 
    ///     // 读取配置
    ///     int intValue = XPrefs.GetInt("intKey", 0);
    ///     float floatValue = XPrefs.GetFloat("floatKey", 0f);
    ///     bool boolValue = XPrefs.GetBool("boolKey", false);
    ///     string stringValue = XPrefs.GetString("stringKey", "");
    /// 
    /// 1.3 读写数组类型
    ///     // 写入数组
    ///     XPrefs.Local.Set("intArray", new[] { 1, 2, 3 });
    ///     XPrefs.Local.Set("stringArray", new[] { "a", "b", "c" });
    /// 
    ///     // 读取数组
    ///     int[] intArray = XPrefs.GetInts("intArray");
    ///     string[] stringArray = XPrefs.GetStrings("stringArray");
    /// 
    /// 2. 配置源管理
    /// 
    /// 2.1 内置配置（只读）
    ///     // 读取内置配置
    ///     string value = XPrefs.Asset.GetString("key");
    /// 
    /// 2.2 本地配置（可写）
    ///     // 写入本地配置
    ///     XPrefs.Local.Set("key", "value");
    ///     XPrefs.Local.Save();
    /// 
    ///     // 读取本地配置
    ///     string value = XPrefs.Local.GetString("key");
    /// 
    /// 2.3 远程配置（只读）
    ///     // 实现远程配置处理器
    ///     public class RemoteHandler : XPrefs.IRemote.IHandler
    ///     {
    ///         public string Uri => "http://example.com/config";
    ///         public int Timeout => 10;
    ///         
    ///         public void OnRequest(XPrefs.IRemote prefs) { }
    ///         public bool OnRetry(XPrefs.IRemote prefs, int count, out float wait)
    ///         {
    ///             wait = 1.0f;
    ///             return count < 3;
    ///         }
    ///         public void OnSucceed(XPrefs.IRemote prefs) { }
    ///         public void OnFailed(XPrefs.IRemote prefs) { }
    ///     }
    /// 
    ///     // 读取远程配置
    ///     StartCoroutine(XPrefs.Remote.Read(new RemoteHandler()));
    /// 
    /// 3. 变量求值
    /// 
    /// 3.1 基本用法
    ///     // 设置配置项
    ///     XPrefs.Local.Set("name", "John");
    ///     XPrefs.Local.Set("greeting", "Hello ${Prefs.name}");
    /// 
    ///     // 解析变量引用
    ///     string result = XPrefs.Local.Eval("${Prefs.greeting}"); // 输出: Hello John
    /// 
    /// 3.2 多级路径
    ///     // 设置嵌套配置
    ///     XPrefs.Local.Set("user.name", "John");
    ///     XPrefs.Local.Set("user.age", 30);
    /// 
    ///     // 使用多级路径引用
    ///     string result = XPrefs.Local.Eval("${Prefs.user.name} is ${Prefs.user.age}");
    /// 
    /// 4. 命令行参数
    /// 
    /// 4.1 覆盖配置路径
    ///     --Prefs@Asset=path/to/asset.json    # 覆盖内置配置路径（仅支持编辑器环境）
    ///     --Prefs@Local=path/to/local.json    # 覆盖本地配置路径
    /// 
    /// 4.2 覆盖配置值
    ///     --Prefs@Asset.key=value             # 覆盖内置配置项
    ///     --Prefs@Local.key=value             # 覆盖本地配置项
    ///     --Prefs.key=value                   # 覆盖所有配置源
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public partial class XPrefs
    {
        internal static string[] TestArgs { get; set; }

        /// <summary>
        /// 配置基类，提供配置的基本读写和变量替换功能。
        /// </summary>
        /// <remarks>
        /// 主要特性：
        /// 1. 支持 JSON 格式的配置存储
        /// 2. 支持配置项的读写和删除
        /// 3. 支持配置文件的加密存储
        /// 4. 支持变量引用和替换
        /// 5. 支持配置对象的比较和序列化
        /// </remarks>
        public partial class IBase : JSONObject, XObject.Json.IEncoder, XString.IEval
        {
            /// <summary>
            /// 配置文件路径。
            /// </summary>
            [XObject.Json.Exclude]
            public virtual string File { get; set; }

            /// <summary>
            /// 错误信息。
            /// </summary>
            [XObject.Json.Exclude]
            public virtual string Error { get; set; }

            /// <summary>
            /// 是否有未保存的修改。
            /// </summary>
            [XObject.Json.Exclude]
            public virtual bool Dirty { get; internal set; }

            /// <summary>
            /// 是否可写。
            /// </summary>
            internal bool writeable = true;

            /// <summary>
            /// 是否加密存储。
            /// </summary>
            internal bool encrypt = false;

            public IBase() : base() { }

            /// <summary>
            /// 使用指定的读写和加密选项初始化配置对象。
            /// </summary>
            /// <param name="writeable">是否可写。</param>
            /// <param name="encrypt">是否加密存储。</param>
            public IBase(bool writeable = true, bool encrypt = false)
            {
                this.writeable = writeable;
                this.encrypt = encrypt;
            }

            /// <summary>
            /// 解析配置中的变量引用，支持 ${Prefs.Key} 语法。
            /// </summary>
            /// <param name="input">包含变量引用的字符串。</param>
            /// <returns>替换后的字符串。</returns>
            /// <remarks>
            /// 变量引用规则：
            /// 1、使用 ${Prefs.Key} 格式引用其他配置项。
            /// 2、支持多级路径，如 ${Prefs.Section.Key}。
            /// 3、检测循环引用并返回错误提示。
            /// 4、检测嵌套引用并返回错误提示。
            /// </remarks>
            public string Eval(string input)
            {
                var pattern = @"\$\{Prefs\.([^}]+?)\}";
                var visited = new HashSet<string>();

                string ReplaceFunc(Match match)
                {
                    var path = match.Groups[1].Value;
                    if (path.Contains("${")) return $"{match.Value}(Nested)";
                    if (!visited.Add(path)) return $"${{Prefs.{path}}}(Recursive)";
                    try
                    {
                        if (path.Contains('.'))
                        {
                            var paths = path.Split('.');
                            var current = this as JSONNode;
                            for (int i = 0; i < paths.Length - 1; i++)
                            {
                                if (!current.HasKey(paths[i]))
                                {
                                    return $"${{Prefs.{path}}}(Unknown)";
                                }
                                current = current[paths[i]];
                            }
                            if (current.HasKey(paths[^1]))
                            {
                                var value = current[paths[^1]];
                                if (string.IsNullOrEmpty(value)) return $"${{Prefs.{path}}}(Unknown)";
                                return Regex.Replace(value, pattern, ReplaceFunc);
                            }
                        }
                        else if (HasKey(path))
                        {
                            var value = this[path];
                            if (string.IsNullOrEmpty(value)) return $"${{Prefs.{path}}}(Unknown)";
                            return Regex.Replace(value, pattern, ReplaceFunc);
                        }
                        return $"${{Prefs.{path}}}(Unknown)";
                    }
                    finally { visited.Remove(path); }
                }
                return Regex.Replace(input, pattern, ReplaceFunc);
            }

            /// <summary>
            /// 将配置对象编码为 JSON 节点。
            /// </summary>
            /// <returns>JSON 节点。</returns>
            public JSONNode Encode() { return EncodeInternal(new HashSet<IBase>()); }

            /// <summary>
            /// 内部编码方法，处理循环引用。
            /// </summary>
            /// <param name="visited">已访问的对象集合。</param>
            /// <returns>JSON 节点。</returns>
            internal JSONNode EncodeInternal(HashSet<IBase> visited)
            {
                var jobj = new JSONObject();
                if (visited.Add(this))
                {
                    foreach (var kvp in this)
                    {
                        jobj.Add(kvp.Key, kvp.Value);
                    }
                }
                return jobj;
            }

            /// <summary>
            /// 将配置对象转换为 JSON 字符串。
            /// </summary>
            /// <param name="pretty">是否格式化输出。</param>
            /// <returns>JSON 字符串。</returns>
            public virtual string Json(bool pretty = true)
            {
                var jobj = Encode();
                var keys = new List<string>();
                foreach (var kvp in jobj) keys.Add(kvp.Key);
                keys.Sort(); // 按照字母表排序，保证键值的顺序，提高文本可读性
                var njobj = new JSONObject();
                foreach (var key in keys) njobj[key] = jobj[key];
                return pretty ? njobj.ToString(4) : njobj.ToString();
            }

            /// <summary>
            /// 从文件读取配置。
            /// </summary>
            /// <param name="file">配置文件路径，为空则使用当前 File 属性。</param>
            /// <returns>是否读取成功。</returns>
            /// <remarks>
            /// 读取过程：
            /// 1、检查文件路径有效性。
            /// 2、读取文件内容。
            /// 3、如果启用加密，先解密内容。
            /// 4、解析 JSON 数据。
            /// 5、应用命令行参数覆盖。
            /// </remarks>
            public virtual bool Read(string file)
            {
                Error = string.Empty;
                File = file;
                for (var i = Count - 1; i >= 0; i--) Remove(i);
                if (string.IsNullOrEmpty(File)) Error = "Null file for instantiating preferences.";
                else if (!XFile.HasFile(File)) Error = $"Non exist file {File} for instantiating preferences.";
                else if (!Parse(encrypt ? XString.Decrypt(XFile.OpenText(File)) : XFile.OpenText(File), out var perror)) Error = perror;
                if (!string.IsNullOrEmpty(Error)) XLog.Error($"XPrefs.Read: load <a href=\"file:///{File}\">{File}</a> with error: {Error}");
                return string.IsNullOrEmpty(Error);
            }

            /// <summary>
            /// 解析配置文本。
            /// </summary>
            /// <param name="text">配置文本。</param>
            /// <param name="error">错误信息输出。</param>
            /// <returns>是否解析成功。</returns>
            /// <remarks>
            /// 解析过程：
            /// 1、重置脏标记。
            /// 2、解析 JSON 文本。
            /// 3、验证数据有效性。
            /// 4、应用命令行参数覆盖。
            /// </remarks>
            public virtual bool Parse(string text, out string error)
            {
                Dirty = false;
                error = "";
                try
                {
                    var node = JSON.Parse(text);
                    if (node == null)
                    {
                        error = "Null instance.";
                        return false;
                    }
                    if (node.IsString)
                    {
                        error = "Invalid instance.";
                        return false;
                    }
                    foreach (var kvp in node.AsObject)
                    {
                        Add(kvp.Key, kvp.Value);
                    }
                }
                catch (Exception e)
                {
                    XLog.Panic(e);
                    error = e.Message;
                    return false;
                }
                finally
                {
                    var args = XEnv.GetArgs();
                    foreach (var pair in args)
                    {
                        if (pair.Key.StartsWith("Prefs."))
                        {
                            var path = pair.Key["Prefs.".Length..];
                            var value = pair.Value.Trim('"');
                            if (path.Contains('.'))
                            {
                                var paths = path.Split('.');
                                var parent = this as JSONObject;
                                for (int i = 0; i < paths.Length - 1; i++)
                                {
                                    var part = paths[i];
                                    if (!parent.HasKey(part))
                                    {
                                        parent[part] = new JSONObject();
                                    }
                                    parent = parent[part].AsObject;
                                }
                                parent[paths[^1]] = value;
                            }
                            else
                            {
                                this[path] = value;
                            }
                            XLog.Notice($"XPrefs.Base.Parse: override {path} = {value}.");
                        }
                    }
                }
                error = null;
                return true;
            }

            /// <summary>
            /// 保存配置到文件。
            /// </summary>
            /// <param name="pretty">是否格式化输出。</param>
            /// <returns>是否保存成功。</returns>
            /// <remarks>
            /// 保存条件：
            /// 1、配置必须是可写的。
            /// 2、配置有未保存的修改或文件不存在。
            /// 3、文件路径不为空。
            /// </remarks>
            public virtual bool Save(bool pretty = true)
            {
                if (!writeable)
                {
                    XLog.Error("XPrefs.Save: preferences of {0} is readonly.", GetType().FullName);
                    return false;
                }
                if ((!Dirty && XFile.HasFile(File)) || string.IsNullOrEmpty(File))
                {
                    return false;
                }
                else
                {
                    Dirty = false;
                    var text = Json(pretty);
                    XFile.SaveText(File, encrypt ? XString.Encrypt(text) : text);
                    XLog.Notice("XPrefs.Save: persisted to <a href=\"file:///{0}\">{1}</a>.", Path.GetFullPath(File), File);
                    return true;
                }
            }

            /// <summary>
            /// 检查键是否存在。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <returns>是否存在。</returns>
            public virtual bool Has(string key) { return HasKey(key); }

            /// <summary>
            /// 设置配置项的值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="value">配置值。</param>
            /// <returns>是否设置成功。</returns>
            /// <remarks>
            /// 支持的值类型：
            /// 1、基本类型：string、int、bool、float、long、double、byte。
            /// 2、数组类型：以上基本类型的一维数组。
            /// 3、对象类型：继承自 IBase 的配置对象。
            /// </remarks>
            public virtual bool Set(string key, object value)
            {
                if (!writeable)
                {
                    XLog.Error("XPrefs.Set: preferences of {0} is readonly.", GetType().FullName);
                    return false;
                }
                if (value == null) return false;
                var type = value.GetType();
                if (type == typeof(string))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (string)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(int))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (int)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(bool))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (bool)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(float))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (float)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(long))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (long)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(double))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (double)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(byte))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (byte)value;
                    Dirty = true;
                    return true;
                }
                else if (value is Array arr) // 仅支持二维数组
                {
                    var jarr = new JSONArray();
                    this[key] = jarr;
                    foreach (var ele in arr)
                    {
                        var etype = ele.GetType();
                        if (etype == typeof(string)) jarr.Add((string)ele);
                        else if (etype == typeof(int)) jarr.Add((int)ele);
                        else if (etype == typeof(bool)) jarr.Add((bool)ele);
                        else if (etype == typeof(float)) jarr.Add((float)ele);
                        else if (etype == typeof(long)) jarr.Add((long)ele);
                        else if (etype == typeof(double)) jarr.Add((double)ele);
                        else if (etype == typeof(byte)) jarr.Add((byte)ele);
                    }
                    Dirty = true;
                    return true;
                }
                else if (typeof(IBase).IsAssignableFrom(type))
                {
                    var prefs = value as IBase;
                    if (Has(key))
                    {
                        var ovalue = Get<IBase>(key);
                        if (prefs.Equals(ovalue)) return false;
                    }
                    this[key] = prefs;
                    Dirty = true;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 移除配置项。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <returns>是否移除成功。</returns>
            public virtual bool Unset(string key)
            {
                if (!writeable)
                {
                    XLog.Error("XPrefs.Unset: preferences of {0} is readonly.", GetType().FullName);
                    return false;
                }
                if (HasKey(key))
                {
                    Remove(key);
                    Dirty = true;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 获取指定类型的配置值。
            /// </summary>
            /// <typeparam name="T">目标类型。</typeparam>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>配置值，不存在或类型不匹配时返回默认值。</returns>
            /// <remarks>
            /// 支持的类型：
            /// 1、基本类型：int、long、float、bool、string。
            /// 2、数组类型：以上基本类型的一维数组。
            /// 3、对象类型：继承自 IBase 的配置对象。
            /// </remarks>
            public virtual T Get<T>(string key, T defval = default)
            {
                var type = typeof(T);
                if (HasKey(key))
                {
                    var val = this[key];
                    if (val == null) return defval;
                    if (type == typeof(int))
                        return (T)(object)val.AsInt;
                    if (type == typeof(long))
                        return (T)(object)val.AsLong;
                    if (type == typeof(float))
                        return (T)(object)val.AsFloat;
                    if (type == typeof(bool))
                        return (T)(object)val.AsBool;
                    if (type == typeof(string))
                        return (T)(object)val.Value;
                    if (val.IsArray)
                    {
                        var jarr = val.AsArray;
                        var etype = type.GetElementType();
                        if (etype == typeof(int))
                        {
                            var arr = new int[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].AsInt;
                            return (T)(object)arr;
                        }
                        if (etype == typeof(long))
                        {
                            var arr = new long[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].AsLong;
                            return (T)(object)arr;
                        }
                        if (etype == typeof(float))
                        {
                            var arr = new float[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].AsFloat;
                            return (T)(object)arr;
                        }
                        if (etype == typeof(bool))
                        {
                            var arr = new bool[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].AsBool;
                            return (T)(object)arr;
                        }
                        if (etype == typeof(string))
                        {
                            var arr = new string[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].Value;
                            return (T)(object)arr;
                        }
                    }
                    if (typeof(IBase).IsAssignableFrom(type) && val.Tag == JSONNodeType.Object)
                    {
                        var newval = (IBase)Activator.CreateInstance(type);
                        foreach (var kvp in val)
                        {
                            newval[kvp.Key] = kvp.Value;
                        }
                        return (T)(object)newval;
                    }
                }
                return defval;
            }

            /// <summary>
            /// 获取指定类型的数组配置值。
            /// </summary>
            /// <typeparam name="T">数组元素类型。</typeparam>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>数组配置值，不存在或类型不匹配时返回默认值。</returns>
            public virtual T[] Gets<T>(string key, T[] defval = null)
            {
                if (HasKey(key))
                {
                    var val = this[key];
                    if (!val.IsArray) return defval;

                    var jsonArr = val.AsArray;
                    var type = typeof(T);

                    if (type == typeof(int))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].AsInt;
                        return result;
                    }
                    if (type == typeof(long))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].AsLong;
                        return result;
                    }
                    if (type == typeof(float))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].AsFloat;
                        return result;
                    }
                    if (type == typeof(bool))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].AsBool;
                        return result;
                    }
                    if (type == typeof(string))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].Value;
                        return result;
                    }
                }
                return defval;
            }

            /// <summary>
            /// 获取整数配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>整数值，不存在时返回默认值。</returns>
            public virtual int GetInt(string key, int defval = 0)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null)
                    {
                        return value.AsInt;
                    }
                }
                return defval;
            }

            /// <summary>
            /// 获取整数数组配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>整数数组，不存在时返回默认值。</returns>
            public virtual int[] GetInts(string key, int[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null && value.IsArray)
                    {
                        var arr = new int[value.Count];
                        var jarr = value.AsArray;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            arr[i] = jarr[i].AsInt;
                        }
                        return arr;
                    }
                }
                return defval;
            }

            /// <summary>
            /// 获取长整数配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>长整数值，不存在时返回默认值。</returns>
            public virtual long GetLong(string key, long defval = 0)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.AsLong;
                }
                return defval;
            }

            /// <summary>
            /// 获取长整数数组配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>长整数数组，不存在时返回默认值。</returns>
            public virtual long[] GetLongs(string key, long[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null && value.IsArray)
                    {
                        var arr = new long[value.Count];
                        var jarr = value.AsArray;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            arr[i] = jarr[i].AsLong;
                        }
                        return arr;
                    }
                }
                return defval;
            }

            /// <summary>
            /// 获取浮点数配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>浮点数值，不存在时返回默认值。</returns>
            public virtual float GetFloat(string key, float defval = 0f)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.AsFloat;
                }
                return defval;
            }

            /// <summary>
            /// 获取浮点数数组配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>浮点数数组，不存在时返回默认值。</returns>
            public virtual float[] GetFloats(string key, float[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null && value.IsArray)
                    {
                        var arr = new float[value.Count];
                        var jarr = value.AsArray;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            arr[i] = jarr[i].AsFloat;
                        }
                        return arr;
                    }
                }
                return defval;
            }

            /// <summary>
            /// 获取布尔配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>布尔值，不存在时返回默认值。</returns>
            public virtual bool GetBool(string key, bool defval = false)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.AsBool;
                }
                return defval;
            }

            /// <summary>
            /// 获取布尔数组配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>布尔数组，不存在时返回默认值。</returns>
            public virtual bool[] GetBools(string key, bool[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null && value.IsArray)
                    {
                        var arr = new bool[value.Count];
                        var jarr = value.AsArray;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            arr[i] = jarr[i].AsBool;
                        }
                        return arr;
                    }
                }
                return defval;
            }

            /// <summary>
            /// 获取字符串配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>字符串值，不存在时返回默认值。</returns>
            public virtual string GetString(string key, string defval = "")
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.Value;
                }
                return defval;
            }

            /// <summary>
            /// 获取字符串数组配置值。
            /// </summary>
            /// <param name="key">配置键。</param>
            /// <param name="defval">默认值。</param>
            /// <returns>字符串数组，不存在时返回默认值。</returns>
            public virtual string[] GetStrings(string key, string[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.AsStringArray;
                }
                return defval;
            }

            /// <summary>
            /// 比较两个配置对象是否相等。
            /// </summary>
            /// <param name="obj">要比较的对象。</param>
            /// <returns>是否相等。</returns>
            /// <remarks>
            /// 比较条件：
            /// 1、类型必须是 IBase。
            /// 2、文件路径相同。
            /// 3、配置项数量相同。
            /// 4、所有配置项的键和值都相同。
            /// </remarks>
            public override bool Equals(object obj)
            {
                if (obj is not IBase target) return false;
                if (File != target.File) return false;
                if (Count != target.Count) return false;
                foreach (var kvp in this)
                {
                    if (!target.HasKey(kvp.Key)) return false;
                    var val1 = kvp.Value;
                    var val2 = target[kvp.Key];
                    var ret = val1.Equals(val2);
                    if (!ret) return false;
                }
                return true;
            }

            /// <summary>
            /// 获取配置对象的哈希码。
            /// </summary>
            /// <returns>哈希码。</returns>
            public override int GetHashCode() { return base.GetHashCode(); }
        }
    }
    #endregion

    #region 内置配置（只读）
    public partial class XPrefs
    {
        /// <summary>
        /// 内置配置类，用于管理打包在应用程序中的只读配置。
        /// </summary>
        /// <remarks>
        /// 特点：
        /// 1. 只读配置，不支持修改
        /// 2. 配置文件打包在应用程序中
        /// 3. 支持加密存储
        /// 4. 支持变量替换
        /// </remarks>
        public partial class IAsset : IBase
        {
            public static string Uri
            {
                get
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !UnityEditor.BuildPipeline.isBuildingPlayer) // 编辑器环境且非构建状态
                    {
                        var path = XEnv.GetArg("Prefs@Asset");
                        if (!string.IsNullOrEmpty(path)) return path;

                        var key = XFile.PathJoin(Path.GetFullPath("./"), "Preferences");
                        return UnityEditor.EditorPrefs.GetString(key);
                    }
#endif
                    return XFile.PathJoin(XEnv.AssetPath, "Preferences.json");
                }
                set
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !UnityEditor.BuildPipeline.isBuildingPlayer)
                    {
                        var key = XFile.PathJoin(Path.GetFullPath("./"), "Preferences");
                        UnityEditor.EditorPrefs.SetString(key, value);
                    }
#endif
                }
            }

            internal IAsset() : base(writeable: Application.isEditor, encrypt: !Application.isEditor) { }

            public override bool Parse(string text, out string error)
            {
                var ret = base.Parse(text, out error);

                // 仅编辑器或 Dev/Test 模式支持变量覆盖
                var mode = GetString(XEnv.Prefs.Mode, "");
                if (Application.isEditor || mode == XEnv.ModeDev || mode == XEnv.ModeTest)
                {
                    var args = XEnv.GetArgs();
                    foreach (var pair in args)
                    {
                        if (pair.Key.StartsWith("Prefs@Asset."))
                        {
                            var path = pair.Key["Prefs@Asset.".Length..];
                            var value = pair.Value.Trim('"');
                            if (path.Contains('.'))
                            {
                                var paths = path.Split('.');
                                var parent = this as JSONObject;
                                for (int i = 0; i < paths.Length - 1; i++)
                                {
                                    var part = paths[i];
                                    if (!parent.HasKey(part))
                                    {
                                        parent[part] = new JSONObject();
                                    }
                                    parent = parent[part].AsObject;
                                }
                                parent[paths[^1]] = value;
                            }
                            else
                            {
                                this[path] = value;
                            }
                            XLog.Notice($"XPrefs.Asset.Parse: override {path} = {value}.");
                        }
                    }
                }

                return ret;
            }
        }

        internal static IAsset asset;
        /// <summary>
        /// 内置配置（只读）
        /// </summary>
        public static IAsset Asset
        {
            get
            {
                if (asset == null)
                {
                    asset = new IAsset();
                    asset.Read(IAsset.Uri);
                }
                return asset;
            }
        }
    }
    #endregion

    #region 本地配置（可写）
    public partial class XPrefs
    {
        /// <summary>
        /// 本地配置类，用于管理本地可写配置。
        /// </summary>
        /// <remarks>
        /// 特点：
        /// 1. 支持读写操作
        /// 2. 配置文件存储在本地文件系统
        /// 3. 支持加密存储
        /// 4. 支持变量替换
        /// 5. 支持命令行参数覆盖
        /// </remarks>
        public partial class ILocal : IBase
        {
            public static string Uri
            {
                get
                {
                    var path = XEnv.GetArg("Prefs@Local");
                    if (!string.IsNullOrEmpty(path)) return path;

                    return XFile.PathJoin(XEnv.LocalPath, "Preferences.json");
                }
            }

            internal ILocal() : base(writeable: true, encrypt: !(Application.isEditor || XEnv.Mode <= XEnv.ModeType.Test)) { }

            public override bool Parse(string text, out string error)
            {
                var ret = base.Parse(text, out error);

                // 仅编辑器或 Dev/Test 模式支持变量覆盖
                if (Application.isEditor || XEnv.Mode <= XEnv.ModeType.Test)
                {
                    var args = XEnv.GetArgs();
                    foreach (var pair in args)
                    {
                        if (pair.Key.StartsWith("Prefs@Local."))
                        {
                            var path = pair.Key["Prefs@Local.".Length..];
                            var value = pair.Value.Trim('"');
                            if (path.Contains('.'))
                            {
                                var paths = path.Split('.');
                                var parent = this as JSONObject;
                                for (int i = 0; i < paths.Length - 1; i++)
                                {
                                    var part = paths[i];
                                    if (!parent.HasKey(part))
                                    {
                                        parent[part] = new JSONObject();
                                    }
                                    parent = parent[part].AsObject;
                                }
                                parent[paths[^1]] = value;
                            }
                            else
                            {
                                this[path] = value;
                            }
                            XLog.Notice($"XPrefs.Local.Parse: override {path} = {value}.");
                        }
                    }
                }

                return ret;
            }
        }

        internal static ILocal local;
        /// <summary>
        /// 本地配置（可写）
        /// </summary>
        public static ILocal Local
        {
            get
            {
                if (local == null)
                {
                    local = new ILocal();
                    if (Application.isPlaying)
                    {
                        Application.quitting += () => local.Save();
                        SceneManager.activeSceneChanged += (_, _) => local.Save();
                    }

                    if (XFile.HasFile(ILocal.Uri)) local.Read(ILocal.Uri);
                    else local.File = ILocal.Uri;
                }
                return local;
            }
        }
    }
    #endregion

    #region 远端配置（只读）
    public partial class XPrefs
    {
        /// <summary>
        /// 远程配置类，用于管理从远程服务器获取的配置。
        /// </summary>
        /// <remarks>
        /// 特点：
        /// 1. 支持从远程服务器读取配置
        /// 2. 支持本地缓存
        /// 3. 支持加密存储
        /// 4. 支持变量替换
        /// 5. 支持超时和重试机制
        /// </remarks>
        public partial class IRemote : IBase
        {
            internal IRemote() : base(writeable: false, encrypt: false) { }

            public interface IHandler
            {
                string Uri { get; }

                int Timeout { get; }

                void OnRequest(IRemote prefs);

                bool OnRetry(IRemote prefs, int count, out float wait);

                void OnSucceeded(IRemote prefs);

                void OnFailed(IRemote prefs);
            }

            public IEnumerator Read(IHandler handler)
            {
                if (handler == null) throw new ArgumentNullException("handler");

                if (string.IsNullOrEmpty(handler.Uri)) Error = "Null uri for requesting preferences.";
                else
                {
                    var executeCount = 0;
                    while (true)
                    {
                        Error = string.Empty;
                        XLog.Notice("XPrefs.IRemote.Read: requesting <a href=\"{0}\">{1}</a> with timeout: {2}s", handler.Uri, handler.Uri, handler.Timeout);
                        if (executeCount == 0) handler.OnRequest(this);

                        executeCount++;

                        using var req = UnityWebRequest.Get(handler.Uri);
                        if (handler.Timeout > 0) req.timeout = handler.Timeout;
                        yield return req.SendWebRequest();
                        if (req.responseCode == 200)
                        {
                            if (Parse(req.downloadHandler.text, out var perror) == false)
                            {
                                Error = XString.Format("Request preferences succeeded, but parsing failed: {0}, content: {1}", perror, req.downloadHandler.text);
                            }
                        }
                        else Error = XString.Format("Request preferences response: {0}, error: {1}", req.responseCode, req.error);

                        if (string.IsNullOrEmpty(Error) == false)
                        {
                            XLog.Error($"XPrefs.IRemote.Read: request <a href=\"{handler.Uri}\">{handler.Uri}</a> with error: {Error}");
                            if (handler.OnRetry(this, executeCount, out var wait) && wait > 0)
                            {
                                yield return new WaitForSeconds(wait);
                            }
                            else
                            {
                                handler.OnFailed(this);
                                break;
                            }
                        }
                        else
                        {
                            XLog.Notice("XPrefs.IRemote.Read: request and parse preferences succeeded.");
                            handler.OnSucceeded(this);
                            break;
                        }
                    }
                }

                yield return null;
            }

            public override bool Save(bool pretty = true) { throw new Exception($"{GetType().FullName} is readonly."); }
        }

        internal static IRemote remote;
        /// <summary>
        /// 远端配置（只读）
        /// </summary>
        public static IRemote Remote { get => remote ??= new IRemote(); }
    }
    #endregion

    #region 公开接口（静态）
    public partial class XPrefs
    {
        /// <summary>
        /// 检查指定键是否存在于配置源中。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>是否存在。</returns>
        /// <remarks>
        /// 查找规则：
        /// 1、如果未提供配置源或非运行时，则仅在内置配置中查找。
        /// 2、按照配置源的顺序依次查找，找到即返回。
        /// </remarks>
        public static bool HasKey(string key, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.Has(key);
            foreach (var source in sources)
            {
                if (source.Has(key)) return true;
            }
            return false;
        }

        /// <summary>
        /// 获取整数配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>整数值，不存在时返回默认值。</returns>
        /// <remarks>
        /// 查找规则：
        /// 1、如果未提供配置源或非运行时，则从内置配置中获取。
        /// 2、按照配置源的顺序依次查找，找到即返回。
        /// </remarks>
        public static int GetInt(string key, int defval = 0, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetInt(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetInt(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取整数数组配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>整数数组，不存在时返回默认值。</returns>
        public static int[] GetInts(string key, int[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetInts(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetInts(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取长整数配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>长整数值，不存在时返回默认值。</returns>
        public static long GetLong(string key, long defval = 0, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetLong(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetLong(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取长整数数组配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>长整数数组，不存在时返回默认值。</returns>
        public static long[] GetLongs(string key, long[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetLongs(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetLongs(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取浮点数配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>浮点数值，不存在时返回默认值。</returns>
        public static float GetFloat(string key, float defval = 0f, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetFloat(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetFloat(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取浮点数数组配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>浮点数数组，不存在时返回默认值。</returns>
        public static float[] GetFloats(string key, float[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetFloats(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetFloats(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取布尔配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>布尔值，不存在时返回默认值。</returns>
        public static bool GetBool(string key, bool defval = false, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetBool(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetBool(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取布尔数组配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>布尔数组，不存在时返回默认值。</returns>
        public static bool[] GetBools(string key, bool[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetBools(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetBools(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取字符串配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>字符串值，不存在时返回默认值。</returns>
        public static string GetString(string key, string defval = "", params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetString(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetString(key);
            }
            return defval;
        }

        /// <summary>
        /// 获取字符串数组配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>字符串数组，不存在时返回默认值。</returns>
        public static string[] GetStrings(string key, string[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetStrings(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetStrings(key);
            }
            return defval;
        }
    }
    #endregion

    #region 编辑面板
    public partial class XPrefs
    {
        /// <summary>
        /// 配置编辑面板接口，定义配置的可视化编辑功能。
        /// </summary>
        /// <remarks>
        /// 功能特点：
        /// 1. 支持分节显示配置项
        /// 2. 支持配置项的提示信息
        /// 3. 支持折叠/展开配置组
        /// 4. 支持自定义显示顺序
        /// </remarks>
        public interface IPanel
        {
            /// <summary>
            /// 获取或设置目标配置对象。
            /// </summary>
            IBase Target { get; set; }

            /// <summary>
            /// 获取配置节名称。
            /// </summary>
            string Section { get; }

            /// <summary>
            /// 获取提示信息。
            /// </summary>
            string Tooltip { get; }

            /// <summary>
            /// 获取是否可折叠。
            /// </summary>
            bool Foldable { get; }

            /// <summary>
            /// 获取显示优先级。
            /// </summary>
            int Priority { get; }

            /// <summary>
            /// 面板激活时调用。
            /// </summary>
            /// <param name="searchContext">搜索上下文。</param>
            /// <param name="rootElement">根元素。</param>
            void OnActivate(string searchContext, VisualElement rootElement);

            /// <summary>
            /// 面板可视化时调用。
            /// </summary>
            /// <param name="searchContext">搜索上下文。</param>
            void OnVisualize(string searchContext);

            /// <summary>
            /// 面板停用时调用。
            /// </summary>
            void OnDeactivate();

            /// <summary>
            /// 保存配置时调用。
            /// </summary>
            void OnSave();

            /// <summary>
            /// 应用配置时调用。
            /// </summary>
            void OnApply();

            /// <summary>
            /// 验证配置是否有效。
            /// </summary>
            /// <returns>是否有效。</returns>
            bool Validate();

            /// <summary>
            /// 显示标题。
            /// </summary>
            /// <param name="text">标题文本。</param>
            /// <param name="tooltip">提示信息。</param>
            /// <param name="width">宽度，-1 表示使用默认宽度。</param>
            void Title(string text, string tooltip = "", int width = -1);
        }

        /// <summary>
        /// 配置编辑面板基类，提供配置的可视化编辑功能。
        /// </summary>
        public class Panel : ScriptableObject, IPanel
        {
            /// <summary>
            /// 配置节名称。
            /// </summary>
            private readonly string section;

            /// <summary>
            /// 配置节提示信息。
            /// </summary>
            private readonly string tooltip;

            /// <summary>
            /// 是否支持折叠。
            /// </summary>
            private readonly bool foldable = true;

            /// <summary>
            /// 显示优先级。
            /// </summary>
            private readonly int priority;

            public Panel() { }

            /// <summary>
            /// 初始化配置面板。
            /// </summary>
            /// <param name="section">配置节名称。</param>
            /// <param name="tooltip">提示信息。</param>
            /// <param name="foldable">是否可折叠。</param>
            /// <param name="priority">显示优先级。</param>
            public Panel(string section, string tooltip = "", bool foldable = true, int priority = 0)
            {
                this.section = section;
                this.tooltip = tooltip;
                this.foldable = foldable;
                this.priority = priority;
            }

            /// <summary>
            /// 获取或设置目标配置对象。
            /// </summary>
            public virtual IBase Target { get; set; }

            /// <summary>
            /// 获取配置节名称。
            /// </summary>
            public virtual string Section => section;

            /// <summary>
            /// 获取提示信息。
            /// </summary>
            public virtual string Tooltip => tooltip;

            /// <summary>
            /// 获取是否可折叠。
            /// </summary>
            public virtual bool Foldable => foldable;

            /// <summary>
            /// 获取显示优先级。
            /// </summary>
            public virtual int Priority => priority;

            /// <summary>
            /// 面板激活时调用。
            /// </summary>
            /// <param name="searchContext">搜索上下文。</param>
            /// <param name="rootElement">根元素。</param>
            public virtual void OnActivate(string searchContext, VisualElement rootElement) { }

            /// <summary>
            /// 面板可视化时调用。
            /// </summary>
            /// <param name="searchContext">搜索上下文。</param>
            public virtual void OnVisualize(string searchContext) { }

            /// <summary>
            /// 面板停用时调用。
            /// </summary>
            public virtual void OnDeactivate() { }

            /// <summary>
            /// 保存配置时调用。
            /// </summary>
            public virtual void OnSave() { }

            /// <summary>
            /// 应用配置时调用。
            /// </summary>
            public virtual void OnApply() { }

            /// <summary>
            /// 验证配置是否有效。
            /// </summary>
            /// <returns>是否有效。</returns>
            public virtual bool Validate() { return true; }

            /// <summary>
            /// 显示标题。
            /// </summary>
            /// <param name="text">标题文本。</param>
            /// <param name="tooltip">提示信息。</param>
            /// <param name="width">宽度，-1 表示使用默认宽度。</param>
            public virtual void Title(string text, string tooltip = "", int width = -1)
            {
                GUILayout.Label(new GUIContent(text, tooltip), GUILayout.Width(width == -1 ? 60 : width));
            }
        }
    }
    #endregion
}
