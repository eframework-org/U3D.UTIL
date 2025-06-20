// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace EFramework.Utility
{
    /// <summary>
    /// XMani 提供了一个文件清单管理工具，支持文件清单的生成、解析、对比和版本管理。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 支持文件清单的生成和解析
    /// - 支持本地和远程清单的读取
    /// - 支持清单文件的差异对比
    /// - 支持文件的 MD5 校验
    /// 
    /// 使用手册
    /// 1. 清单管理
    /// 
    /// 1.1 创建清单
    ///     // 创建一个新的清单实例
    ///     var manifest = new XMani.Manifest();
    ///     
    ///     // 或指定清单路径创建
    ///     var manifest = new XMani.Manifest("path/to/manifest/file");
    /// 
    /// 1.2 解析清单
    ///     // 解析清单文本
    ///     var data = "file1.txt|d41d8cd98f00b204e9800998ecf8427e|0\n" +
    ///                "file2.txt|d41d8cd98f00b204e9800998ecf8427e|123";
    ///     bool success = manifest.Parse(data, out string error);
    ///     
    ///     // 检查解析结果
    ///     if (success)
    ///     {
    ///         foreach (var file in manifest.Files)
    ///         {
    ///             Debug.Log($"文件：{file.Name}，MD5：{file.MD5}，大小：{file.Size}");
    ///         }
    ///     }
    /// 
    /// 2. 清单读取
    /// 
    /// 2.1 本地文件读取
    ///     // 从本地文件读取清单
    ///     var handler = manifest.Read("path/to/manifest/file");
    ///     while (!handler()) { } // 等待读取完成
    ///     
    ///     if (string.IsNullOrEmpty(manifest.Error))
    ///     {
    ///         Debug.Log($"读取成功，包含 {manifest.Files.Count} 个文件");
    ///     }
    /// 
    /// 2.2 远程文件读取
    ///     // 从远程 URL 读取清单，设置 10 秒超时
    ///     var handler = manifest.Read("http://example.com/Manifest.db", 10);
    ///     while (!handler()) { } // 等待读取完成
    /// 
    /// 3. 清单对比
    /// 
    /// 3.1 比较两个清单
    ///     // 创建两个清单实例
    ///     var manifest1 = new XMani.Manifest();
    ///     manifest1.Files.Add(new XMani.FileInfo 
    ///     { 
    ///         Name = "file1.txt", 
    ///         MD5 = "md5_1", 
    ///         Size = 100 
    ///     });
    ///     
    ///     var manifest2 = new XMani.Manifest();
    ///     manifest2.Files.Add(new XMani.FileInfo 
    ///     { 
    ///         Name = "file2.txt", 
    ///         MD5 = "md5_2", 
    ///         Size = 200 
    ///     });
    ///     
    ///     // 执行对比
    ///     var diff = manifest1.Compare(manifest2);
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XMani
    {
        /// <summary>
        /// 默认清单文件名。
        /// </summary>
        public static string Default = "Manifest.db";

        /// <summary>
        /// 文件信息类，用于存储单个文件的元数据。
        /// </summary>
        /// <remarks>
        /// 包含文件的基本信息：
        /// - 文件名（包含相对路径）
        /// - MD5 校验值
        /// - 文件大小（字节）
        /// </remarks>
        public class FileInfo
        {
            /// <summary>
            /// 文件名称，包含相对路径
            /// </summary>
            public string Name;

            /// <summary>
            /// 文件的 MD5 校验值
            /// </summary>
            public string MD5;

            /// <summary>
            /// 文件大小（字节）
            /// </summary>
            public long Size;
        }

        /// <summary>
        /// 差异信息类，用于存储两个清单文件的对比结果。
        /// </summary>
        /// <remarks>
        /// 包含三种差异类型：
        /// - 新增：目标清单中存在但当前清单中不存在的文件
        /// - 修改：两个清单中都存在但 MD5 不同的文件
        /// - 删除：当前清单中存在但目标清单中不存在的文件
        /// </remarks>
        public class DiffInfo
        {
            /// <summary>
            /// 新增的文件列表
            /// </summary>
            public readonly List<FileInfo> Added = new();

            /// <summary>
            /// 修改的文件列表（MD5 不同）
            /// </summary>
            public readonly List<FileInfo> Modified = new();

            /// <summary>
            /// 删除的文件列表
            /// </summary>
            public readonly List<FileInfo> Deleted = new();
        }

        /// <summary>
        /// 清单类，用于管理文件清单的读取、解析和对比。
        /// </summary>
        /// <remarks>
        /// 主要功能：
        /// - 解析清单文本和文件
        /// - 读取本地和远程清单
        /// - 比较清单差异
        /// - 格式化清单输出
        /// </remarks>
        public class Manifest
        {
            /// <summary>
            /// 清单文件的路径或 URL
            /// </summary>
            public virtual string Uri { get; internal set; }

            /// <summary>
            /// 解析或读取过程中的错误信息
            /// </summary>
            public virtual string Error { get; internal set; }

            /// <summary>
            /// 清单中包含的文件列表
            /// </summary>
            public readonly List<FileInfo> Files = new();

            /// <summary>
            /// 初始化清单实例。
            /// </summary>
            /// <param name="uri">清单文件的路径或 URL</param>
            public Manifest(string uri = "") { Uri = uri; }

            /// <summary>
            /// 解析清单文本内容。
            /// </summary>
            /// <remarks>
            /// 清单文件格式：每行一个文件记录，格式为 "文件名|MD5值|文件大小"
            /// </remarks>
            /// <param name="data">清单文本内容</param>
            /// <param name="error">解析错误信息</param>
            /// <returns>是否解析成功</returns>
            public virtual bool Parse(string data, out string error)
            {
                return Parse(Encoding.UTF8.GetBytes(data), out error);
            }

            /// <summary>
            /// 解析清单文件内容。
            /// </summary>
            /// <remarks>
            /// 清单文件格式：每行一个文件记录，格式为 "文件名|MD5值|文件大小"
            /// </remarks>
            /// <param name="bytes">清单文件的字节内容</param>
            /// <param name="error">解析错误信息</param>
            /// <returns>是否解析成功</returns>
            public virtual bool Parse(byte[] bytes, out string error)
            {
                error = "";
                if (bytes == null || bytes.Length == 0)
                {
                    error = "Null bytes for loading mainfest.";
                }
                else
                {
                    try
                    {
                        using var ms = new MemoryStream(bytes);
                        using var sr = new StreamReader(ms);
                        while (sr.EndOfStream == false)
                        {
                            var line = sr.ReadLine();
                            if (string.IsNullOrEmpty(line)) continue;
                            var strs = line.Split('|');
                            var file = new FileInfo();
                            file.Name = strs[0];
                            file.MD5 = strs[1];
                            long.TryParse(strs[2], out file.Size);
                            Files.Add(file);
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                        XLog.Panic(e);
                    }
                }
                return false;
            }

            /// <summary>
            /// 读取清单文件。
            /// </summary>
            /// <remarks>
            /// 支持本地文件和 HTTP(S) URL，返回一个用于检查读取完成状态的处理器。
            /// </remarks>
            /// <param name="uri">清单文件的路径或 URL，为空则使用实例的 Uri</param>
            /// <param name="timeout">HTTP 请求超时时间（秒）</param>
            /// <returns>状态检查处理器，返回 true 表示读取完成</returns>
            public virtual Func<bool> Read(string uri = "", int timeout = 10)
            {
                Error = string.Empty;
                if (string.IsNullOrEmpty(uri)) uri = Uri;
                else Uri = uri;
                var done = false;
                var www = uri.StartsWith("http");
                UnityWebRequest req = null;
                var handler = new Func<bool>(() =>
                {
                    if (done == false)
                    {
                        if (www)
                        {
                            try
                            {
                                if (req == null)
                                {
                                    XLog.Notice("XMani.Manifest.Read: requesting <a href=\"{0}\">{1}</a>.", uri, uri);
                                    req = UnityWebRequest.Get(uri);
                                    req.timeout = timeout;
                                    req.SendWebRequest();
                                }
                                else
                                {
                                    if (req.isDone)
                                    {
                                        done = true;
                                        if (req.responseCode == 200)
                                        {
                                            if (Parse(req.downloadHandler.data, out var perror) == false)
                                            {
                                                Error = "Request manifest succeeded, but parsing failed: {0}, content: {1}".Format(perror, req.downloadHandler.text);
                                            }
                                        }
                                        else
                                        {
                                            Error = "Request manifest response: {0}, error: {1}".Format(req.responseCode, req.error);
                                        }

                                        try { req.Dispose(); } catch (Exception e) { XLog.Panic(e); }

                                        if (string.IsNullOrEmpty(Error)) XLog.Notice("XMani.Manifest.Read: request and parse manifest succeeded.");
                                        else XLog.Error("XMani.Manifest.Read: request and parse failed with error: {0}", Error);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Error = e.Message;
                                done = true;
                                XLog.Error("XMani.Manifest.Read: request and parse failed with exception: {0}", Error);
                            }
                        }
                    }
                    return done;
                });

                if (!www)
                {
                    done = true;
                    XLog.Notice("XMani.Manifest.Read: loading <a href=\"file:///{0}\">{1}</a>.", Path.GetFullPath(uri), uri);

                    if (string.IsNullOrEmpty(uri)) Error = "Null file for reading mainfest.";
                    else if (!XFile.HasFile(uri)) Error = $"Non exist file {uri} for reading mainfest.";
                    else if (!Parse(XFile.OpenFile(uri), out var perror)) Error = perror;

                    if (string.IsNullOrEmpty(Error)) XLog.Notice("XMani.Manifest.Read: load and parse manifest succeeded.");
                    else XLog.Error("XMani.Manifest.Read: load and parse failed with error: {0}", Error);
                }

                return handler;
            }

            /// <summary>
            /// 比较两个清单的差异。
            /// </summary>
            /// <remarks>
            /// 对比规则：
            /// - 新增：目标清单中存在但当前清单中不存在的文件
            /// - 修改：两个清单中都存在但 MD5 不同的文件
            /// - 删除：当前清单中存在但目标清单中不存在的文件
            /// </remarks>
            /// <param name="other">要比较的目标清单</param>
            /// <returns>差异信息</returns>
            public virtual DiffInfo Compare(Manifest other)
            {
                var differ = new DiffInfo();
                var selfs = Files;
                var others = other.Files;
                var visited = new List<FileInfo>();
                for (var i = 0; i < selfs.Count; i++)
                {
                    var sf = selfs[i];
                    var sig = false;
                    for (var j = 0; j < others.Count; j++)
                    {
                        var of = others[j];
                        if (of.Name == sf.Name)
                        {
                            if (sf.MD5 != of.MD5)
                            {
                                differ.Modified.Add(of);
                            }
                            sig = true;
                            visited.Add(of);
                            break;
                        }
                    }
                    if (sig == false)
                    {
                        differ.Deleted.Add(sf);
                    }
                }
                for (var i = 0; i < others.Count; i++)
                {
                    var fi = others[i];
                    if (visited.Contains(fi) == false)
                    {
                        differ.Added.Add(fi);
                    }
                }
                return differ;
            }

            /// <summary>
            /// 将清单转换为文本格式。
            /// </summary>
            /// <returns>清单的文本表示，每行格式为 "文件名|MD5值|文件大小"</returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                for (var i = 0; i < Files.Count; i++)
                {
                    var fi = Files[i];
                    sb.AppendLine(fi.Name + "|" + fi.MD5 + "|" + fi.Size);
                }
                return sb.ToString();
            }
        }
    }
}
