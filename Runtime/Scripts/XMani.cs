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
    /// XMani 提供了一个文件清单管理工具，支持文件清单的生成、解析、对比等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 支持文件清单的生成和解析
    /// - 支持本地和远端清单的读取
    /// - 支持文件清单的差异对比
    /// 
    /// 使用手册
    /// 1. 基础用法
    /// 
    /// 1.1 创建清单
    ///     // 创建一个新的清单实例
    ///     var manifest = new XMani.Manifest();
    ///     
    ///     // 或指定清单路径创建
    ///     var manifest = new XMani.Manifest("path/to/manifest/file");
    /// 
    /// 1.2 保存清单
    ///     // 创建清单实例
    ///     var manifest = new XMani.Manifest();
    ///     
    ///     // 添加文件信息
    ///     manifest.Files.Add(new XMani.FileInfo 
    ///     { 
    ///         Name = "file.txt", 
    ///         MD5 = "md5_value", 
    ///         Size = 100 
    ///     });
    ///     
    ///     // 将清单格式化为文本
    ///     var text = manifest.ToString();
    ///     
    ///     // 将清单持久化至文件
    ///     XFile.SaveText("path/to/manifest/file", text);
    /// 
    /// 1.3 解析清单
    ///     // 解析清单文本
    ///     var data = "file1.txt|d41d8cd98f00b204e9800998ecf8427e|0\n" +
    ///                "file2.txt|d41d8cd98f00b204e9800998ecf8427e|123";
    ///     
    ///     var succeeded = manifest.Parse(data, out string error);
    ///     
    ///     // 检查解析结果
    ///     if (succeeded)
    ///     {
    ///         foreach (var file in manifest.Files)
    ///         {
    ///             Debug.Log($"文件：{file.Name}，MD5：{file.MD5}，大小：{file.Size}");
    ///         }
    ///     }
    ///     else
    ///     {
    ///         Debug.LogError($"解析失败：{error}");
    ///     }
    /// 
    /// 2. 清单读取
    /// 
    /// 2.1 本地文件
    ///     // 从本地文件读取清单
    ///     // 对称密钥为：12345678
    ///     var handler = manifest.Read(uri: "path/to/manifest/file", secret: "12345678");
    ///     while (!handler()) { } // 等待读取完成
    ///     
    ///     if (string.IsNullOrEmpty(manifest.Error))
    ///     {
    ///         Debug.Log($"读取成功，包含 {manifest.Files.Count} 个文件");
    ///     }
    /// 
    /// 2.2 远端文件
    ///     // 从远端 URL 读取清单
    ///     // 对称密钥为：12345678
    ///     // 设置预请求回调
    ///     var handler = manifest.Read(uri: "http://example.com/Manifest.db", secret: "12345678", onPreRequest: req =&gt;
    ///     {
    ///         req.timeout = 10;
    ///     });
    ///     while (!handler()) { } // 等待读取完成
    ///     
    ///     if (string.IsNullOrEmpty(manifest.Error))
    ///     {
    ///         Debug.Log($"读取成功，包含 {manifest.Files.Count} 个文件");
    ///     }
    /// 
    /// 3. 清单对比
    ///     // 准备清单
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
    ///     
    ///     // 检查差异
    ///     Debug.Log($"新增：{diff.Added.Count} 个文件");
    ///     Debug.Log($"修改：{diff.Modified.Count} 个文件");
    ///     Debug.Log($"删除：{diff.Deleted.Count} 个文件");
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XMani
    {
        /// <summary>
        /// Default 是默认的清单文件名。
        /// </summary>
        public static string Default = "Manifest.db";

        /// <summary>
        /// FileInfo 是文件信息类，用于存储单个文件的元数据。
        /// </summary>
        public class FileInfo
        {
            /// <summary>
            /// Name 是文件的名称。
            /// </summary>
            public string Name;

            /// <summary>
            /// MD5 是文件的哈希值。
            /// </summary>
            public string MD5;

            /// <summary>
            /// Size 是文件的大小，单位：字节。
            /// </summary>
            public long Size;
        }

        /// <summary>
        /// DiffInfo 是差异信息类，用于存储两个清单文件的对比结果。
        /// </summary>
        public class DiffInfo
        {
            /// <summary>
            /// Added 是新增的文件列表。
            /// </summary>
            public readonly List<FileInfo> Added = new();

            /// <summary>
            /// Modified 是修改的文件列表。
            /// </summary>
            public readonly List<FileInfo> Modified = new();

            /// <summary>
            /// Deleted 是删除的文件列表。
            /// </summary>
            public readonly List<FileInfo> Deleted = new();
        }

        /// <summary>
        /// Manifest 是文件清单类，用于管理文件清单的读取、解析和对比。
        /// </summary>
        public class Manifest
        {
            /// <summary>
            /// Uri 是清单文件的路径或 URL。
            /// </summary>
            public virtual string Uri { get; internal set; }

            /// <summary>
            /// Error 表示解析或读取过程中的错误信息。
            /// </summary>
            public virtual string Error { get; internal set; }

            /// <summary>
            /// Files 是清单中包含的文件列表。
            /// </summary>
            public readonly List<FileInfo> Files = new();

            /// <summary>
            /// 初始化清单实例。
            /// </summary>
            /// <param name="uri">清单文件的路径或 URL</param>
            public Manifest(string uri = "") { Uri = uri; }

            /// <summary>
            /// Parse 解析清单文件内容。
            /// </summary>
            /// <param name="data">清单文件的内容</param>
            /// <param name="error">解析错误信息</param>
            /// <param name="secret">清单内容的对称密钥</param>
            /// <returns>是否解析成功</returns>
            public virtual bool Parse(string data, out string error, string secret = "")
            {
                error = "";
                if (string.IsNullOrEmpty(data)) error = "Null content for parsing mainfest.";
                else
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(secret)) data = XString.Decrypt(data, secret);
                        var bytes = Encoding.UTF8.GetBytes(data);
                        using var ms = new MemoryStream(bytes);
                        using var sr = new StreamReader(ms);
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();
                            if (string.IsNullOrEmpty(line)) continue;
                            var strs = line.Split('|');
                            if (strs.Length < 3)
                            {
                                error = $"Invalid line format: {line}";
                                return false;
                            }
                            var file = new FileInfo
                            {
                                Name = strs[0],
                                MD5 = strs[1]
                            };
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
            /// Read 读取清单文件。
            /// </summary>
            /// <param name="uri">清单文件的路径或 URL，为空则使用实例的 Uri</param>
            /// <param name="secret">清单内容的对称密钥</param>
            /// <param name="onPreRequest">HTTP 预请求回调</param>
            /// <returns>状态检查处理器，返回 true 表示读取完成</returns>
            public virtual Func<bool> Read(string uri = "", string secret = "", Action<UnityWebRequest> onPreRequest = null)
            {
                Error = string.Empty;
                if (string.IsNullOrEmpty(uri)) uri = Uri;
                else Uri = uri;
                var done = false;
                var www = uri.StartsWith("http");
                UnityWebRequest req = null;
                var handler = new Func<bool>(() =>
                {
                    if (!done)
                    {
                        if (www)
                        {
                            try
                            {
                                if (req == null)
                                {
                                    XLog.Notice("XMani.Manifest.Read: requesting <a href=\"{0}\">{1}</a>.", uri, uri);
                                    req = UnityWebRequest.Get(uri);
                                    req.timeout = 10;
                                    onPreRequest?.Invoke(req);
                                    req.SendWebRequest();
                                }
                                else
                                {
                                    if (req.isDone)
                                    {
                                        done = true;
                                        if (req.responseCode == 200)
                                        {
                                            if (!Parse(req.downloadHandler.text, out var perror, secret))
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
                    XLog.Notice("XMani.Manifest.Read: loading <a href=\"file:///{0}\">{1}</a>.", Path.GetFullPath(uri), Path.GetRelativePath(XEnv.ProjectPath, uri));

                    if (string.IsNullOrEmpty(uri)) Error = "Null file for reading mainfest.";
                    else if (!XFile.HasFile(uri)) Error = $"Non exist file {uri} for reading mainfest.";
                    else if (!Parse(XFile.OpenText(uri), out var perror, secret)) Error = perror;

                    if (string.IsNullOrEmpty(Error)) XLog.Notice("XMani.Manifest.Read: load and parse manifest succeeded.");
                    else XLog.Error("XMani.Manifest.Read: load and parse failed with error: {0}", Error);
                }

                return handler;
            }

            /// <summary>
            /// Compare 比较两个清单的差异。
            /// </summary>
            /// <param name="other">要比较的目标清单</param>
            /// <returns>差异信息</returns>
            public virtual DiffInfo Compare(Manifest other)
            {
                var diffInfo = new DiffInfo();
                var selfFiles = Files;
                var otherFiles = other.Files;
                var visited = new List<FileInfo>();
                for (var i = 0; i < selfFiles.Count; i++)
                {
                    var sf = selfFiles[i];
                    var sig = false;
                    for (var j = 0; j < otherFiles.Count; j++)
                    {
                        var of = otherFiles[j];
                        if (of.Name == sf.Name)
                        {
                            if (sf.MD5 != of.MD5)
                            {
                                diffInfo.Modified.Add(of);
                            }
                            sig = true;
                            visited.Add(of);
                            break;
                        }
                    }
                    if (!sig) diffInfo.Deleted.Add(sf);
                }
                for (var i = 0; i < otherFiles.Count; i++)
                {
                    var fi = otherFiles[i];
                    if (!visited.Contains(fi)) diffInfo.Added.Add(fi);
                }
                return diffInfo;
            }

            /// <summary>
            /// ToString 将清单转换为文本格式。
            /// </summary>
            /// <returns>清单的文本表示，每行格式为 "文件名|文件MD5|文件大小"</returns>
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
