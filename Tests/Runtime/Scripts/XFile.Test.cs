// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System;
using System.IO;
using EFramework.Utility;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
/// XFile 单元测试类，验证文件系统操作的核心功能。
/// </summary>
/// <remarks>
/// 测试覆盖以下功能：
/// 1. 文件的基本操作（创建、读取、写入、删除）
/// 2. 目录的基本操作（创建、复制、删除）
/// 3. 路径处理（合并、归一化）
/// 4. 压缩解压功能
/// 5. 文件校验（MD5）
/// </remarks>
public class TestXFile
{
    /// <summary>
    /// 测试用的基础路径和相关文件路径。
    /// </summary>
    /// <remarks>
    /// - testBasePath：测试根目录
    /// - testFilePath：测试文件路径
    /// - testDirectoryPath：测试目录路径
    /// - testZipPath：压缩测试目录
    /// - testUnzipPath：解压测试目录
    /// </remarks>
    private string testBasePath;
    private string testFilePath;
    private string testDirectoryPath;
    private string testZipPath;
    private string testUnzipPath;

    /// <summary>
    /// 测试初始化：创建测试用的临时目录。
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        testBasePath = XFile.PathJoin(XEnv.LocalPath, "XFileTest_" + XTime.GetMillisecond());
        testFilePath = XFile.PathJoin(testBasePath, "TestFile.txt");
        testDirectoryPath = XFile.PathJoin(testBasePath, "TestDirectory");
        testZipPath = XFile.PathJoin(testBasePath, "TestZip");
        testUnzipPath = XFile.PathJoin(testBasePath, "TestUnzip");
    }

    /// <summary>
    /// 测试清理：删除测试用的临时目录。
    /// </summary>
    [OneTimeTearDown]
    public void Cleanup()
    {
        if (XFile.HasDirectory(testBasePath))
        {
            XFile.DeleteDirectory(testBasePath);
        }
    }

    /// <summary>
    /// 测试 FileSize 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 正确获取文件大小
    /// 2. 文件不存在时返回 -1
    /// </remarks>
    [Test]
    public void FileSize()
    {
        // Arrange
        var content = "Test content";
        XFile.SaveText(testFilePath, content);

        // Act
        var size = XFile.FileSize(testFilePath);

        // Assert
        Assert.AreEqual(content.Length, size, "文件大小应该与内容长度相等");
        Assert.AreEqual(-1, XFile.FileSize("nonexistent.txt"), "不存在的文件应返回 -1");
    }

    /// <summary>
    /// 测试 HasFile 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 存在的文件返回 true
    /// 2. 不存在的文件返回 false
    /// </remarks>
    [Test]
    public void HasFile()
    {
        // Arrange
        XFile.SaveText(testFilePath, "Test content");

        // Assert
        Assert.IsTrue(XFile.HasFile(testFilePath), "应能检测到已创建的文件");
        Assert.IsFalse(XFile.HasFile(XFile.PathJoin(testDirectoryPath, "NonExistingFile.txt")), "不存在的文件应返回 false");
    }

    /// <summary>
    /// 测试 OpenText 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 正确读取文本内容
    /// 2. 内容编码正确（UTF8）
    /// </remarks>
    [Test]
    public void OpenText()
    {
        // Arrange
        var content = "Hello, World!";
        XFile.SaveText(testFilePath, content);

        // Act
        var result = XFile.OpenText(testFilePath);

        // Assert
        Assert.AreEqual(content, result, "读取的文本内容应与写入的内容完全一致");
    }

    /// <summary>
    /// 测试 OpenFile 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 正确读取二进制内容
    /// 2. 文件长度正确
    /// </remarks>
    [Test]
    public void OpenFile()
    {
        // Arrange
        var content = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        XFile.SaveFile(testFilePath, content);

        // Act
        var result = XFile.OpenFile(testFilePath);

        // Assert
        Assert.AreEqual(content.Length, result.Length, "读取的文件长度应与原始内容长度相等");
        Assert.AreEqual(content, result, "读取的文件内容应与原始内容完全一致");
    }

    /// <summary>
    /// 测试 SaveText 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 成功保存文本内容
    /// 2. 自动创建目录
    /// 3. 内容正确性
    /// </remarks>
    [Test]
    public void SaveText()
    {
        // Arrange
        var content = "Hello, World!";

        // Act
        var result = XFile.SaveText(testFilePath, content);

        // Assert
        Assert.IsTrue(result, "保存文本操作应成功完成");
        Assert.IsTrue(XFile.HasFile(testFilePath), "文件应成功创建");
        Assert.AreEqual(content, XFile.OpenText(testFilePath), "保存的内容应与原始内容一致");
    }

    /// <summary>
    /// 测试 DeleteFile 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 成功删除文件
    /// 2. 删除后文件不存在
    /// </remarks>
    [Test]
    public void DeleteFile()
    {
        // Arrange
        XFile.SaveText(testFilePath, "Test content");

        // Act
        XFile.DeleteFile(testFilePath);

        // Assert
        Assert.IsFalse(XFile.HasFile(testFilePath), "文件应被成功删除");
    }

    /// <summary>
    /// 测试 CopyFile 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 成功复制文件
    /// 2. 目标文件存在
    /// 3. 文件内容一致
    /// </remarks>
    [Test]
    public void CopyFile()
    {
        // Arrange
        XFile.CreateDirectory(testDirectoryPath);
        var copyFilePath = XFile.PathJoin(testDirectoryPath, "CopyTestFile.txt");
        XFile.SaveText(testFilePath, "Test content");

        // Act
        XFile.CopyFile(testFilePath, copyFilePath);

        // Assert
        Assert.IsTrue(XFile.HasFile(copyFilePath), "目标文件应成功创建");
        Assert.AreEqual("Test content", XFile.OpenText(copyFilePath), "复制的文件内容应与源文件一致");
    }

    /// <summary>
    /// 测试 HasDirectory 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 存在的目录返回 true
    /// 2. 不存在的目录返回 false
    /// </remarks>
    [Test]
    public void HasDirectory()
    {
        // Arrange
        XFile.CreateDirectory(testDirectoryPath);

        // Assert
        Assert.IsTrue(XFile.HasDirectory(testDirectoryPath), "应能检测到已创建的目录");
        Assert.IsFalse(XFile.HasDirectory("NonExistingDirectory"), "不存在的目录应返回 false");
    }

    /// <summary>
    /// 测试 DeleteDirectory 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 成功删除目录及其内容
    /// 2. 删除后目录不存在
    /// </remarks>
    [Test]
    public void DeleteDirectory()
    {
        // Arrange
        XFile.CreateDirectory(testDirectoryPath);
        var subFilePath = XFile.PathJoin(testDirectoryPath, "SubFile.txt");
        XFile.SaveText(subFilePath, "Sub file content");

        // Act
        var result = XFile.DeleteDirectory(testDirectoryPath);

        // Assert
        Assert.IsTrue(result, "目录删除操作应成功完成");
        Assert.IsFalse(XFile.HasDirectory(testDirectoryPath), "目录应被成功删除");
    }

    /// <summary>
    /// 测试 CreateDirectory 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 成功创建目录
    /// 2. 创建后目录存在
    /// </remarks>
    [Test]
    public void CreateDirectory()
    {
        // Act
        XFile.CreateDirectory(testDirectoryPath);

        // Assert
        Assert.IsTrue(Directory.Exists(testDirectoryPath), "目录应被成功创建");
    }

    /// <summary>
    /// 测试 CopyDirectory 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 成功复制目录结构
    /// 2. 文件排除功能
    /// 3. 递归复制子目录
    /// 4. 文件内容正确性
    /// </remarks>
    [Test]
    public void CopyDirectory()
    {
        // 确保测试目录存在
        XFile.CreateDirectory(testDirectoryPath);

        // 准备源目录结构
        var srcDir = XFile.PathJoin(testDirectoryPath, "src");
        var dstDir = XFile.PathJoin(testDirectoryPath, "dst");
        var subDir = XFile.PathJoin(srcDir, "subdir");
        var excludeFile = XFile.PathJoin(srcDir, "exclude.txt");
        var normalFile = XFile.PathJoin(srcDir, "normal.txt");
        var subFile = XFile.PathJoin(subDir, "subfile.txt");

        // 创建目录结构
        XFile.CreateDirectory(srcDir);
        XFile.CreateDirectory(subDir);

        // 创建测试文件
        XFile.SaveText(excludeFile, "exclude content");
        XFile.SaveText(normalFile, "normal content");
        XFile.SaveText(subFile, "sub content");

        // 执行目录拷贝，排除.txt文件
        XFile.CopyDirectory(srcDir, dstDir, ".txt");

        // 验证结果
        Assert.That(XFile.HasFile(XFile.PathJoin(dstDir, "normal.txt")), Is.False, "被排除的文件不应被复制");
        Assert.That(XFile.HasFile(XFile.PathJoin(dstDir, "subdir/subfile.txt")), Is.False, "子目录中被排除的文件不应被复制");

        // 清理目标目录
        XFile.DeleteDirectory(dstDir);

        // 测试不带排除的拷贝
        var dstDir2 = XFile.PathJoin(testDirectoryPath, "dst2");
        XFile.CopyDirectory(srcDir, dstDir2);
        Assert.That(XFile.HasDirectory(dstDir2), Is.True, "目标目录应被成功创建");

        // 验证完整拷贝结果
        Assert.That(XFile.HasFile(XFile.PathJoin(dstDir2, "normal.txt")), Is.True, "普通文件应被成功复制");
        Assert.That(XFile.HasFile(XFile.PathJoin(dstDir2, "subdir/subfile.txt")), Is.True, "子目录文件应被成功复制");
        Assert.That(XFile.OpenText(XFile.PathJoin(dstDir2, "normal.txt")), Is.EqualTo("normal content"), "复制的文件内容应保持一致");
    }

    /// <summary>
    /// 测试 IsDirectory 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 正确识别目录路径
    /// 2. 正确识别文件路径
    /// 3. 特殊路径处理
    /// </remarks>
    [Test]
    public void IsDirectory()
    {
        // 测试以斜杠结尾的路径
        Assert.That(XFile.IsDirectory("path/to/dir/"), Is.True, "以斜杠结尾的路径应被识别为目录");

        // 测试不以斜杠结尾的路径
        Assert.That(XFile.IsDirectory("path/to/file"), Is.False, "不以斜杠结尾的路径应被识别为文件");

        // 测试根目录
        Assert.That(XFile.IsDirectory("/"), Is.True, "根目录路径应被识别为目录");
    }

    /// <summary>
    /// 测试 NormalizePath 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 路径分隔符统一
    /// 2. 相对路径处理
    /// 3. 特殊字符处理
    /// </remarks>
    [Test]
    public void NormalizePath()
    {
        // 测试基本路径转换
        Assert.AreEqual("C:/TestFile.txt", XFile.NormalizePath("C:\\Users\\..\\TestFile.txt"), "反斜杠应被转换为正斜杠，且相对路径应被解析");

        // 测试特殊前缀
        Assert.AreEqual("file://path/to/file", XFile.NormalizePath("file://path\\to\\file"), "特殊前缀应被保留，路径分隔符应被统一");
        Assert.AreEqual("jar:file://path/to/file", XFile.NormalizePath("jar:file://path\\to\\file"), "JAR文件路径应被正确处理");

        // 测试点和双点
        Assert.AreEqual("path/file", XFile.NormalizePath("path/./file"), "当前目录符号应被正确处理");
        Assert.AreEqual("path", XFile.NormalizePath("path/file/.."), "父目录符号应被正确处理");
    }

    /// <summary>
    /// 测试 PathJoin 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 基本路径合并
    /// 2. 处理重复分隔符
    /// 3. 空路径处理
    /// 4. 特殊路径合并
    /// </remarks>
    [Test]
    public void PathJoin()
    {
        // 测试基本路径合并
        Assert.AreEqual("path/to/file", XFile.PathJoin("path", "to", "file"), "多个路径段应被正确合并");

        // 测试带斜杠的路径合并
        Assert.AreEqual("path/to/file", XFile.PathJoin("path/", "/to/", "/file"), "重复的路径分隔符应被正确处理");

        // 测试空路径
        Assert.AreEqual("", XFile.PathJoin(), "空参数应返回空字符串");

        // 测试单个路径
        Assert.AreEqual("path", XFile.PathJoin("path"), "单个路径应保持不变");

        // 测试带点和双点的路径
        Assert.AreEqual("path/file", XFile.PathJoin("path/./file"), "当前目录符号应被正确处理");
        Assert.AreEqual("file", XFile.PathJoin("path/../file"), "父目录符号应被正确处理");
    }

    /// <summary>
    /// 测试 Zip 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 成功创建压缩文件
    /// 2. 压缩文件大小正确
    /// 3. 压缩操作成功完成
    /// </remarks>
    [Test]
    public void Zip()
    {
        // 创建测试目录和文件
        XFile.CreateDirectory(testZipPath);
        var file1Path = XFile.PathJoin(testZipPath, "File1.txt");
        var file2Path = XFile.PathJoin(testZipPath, "File2.txt");
        var zipFilePath = XFile.PathJoin(testBasePath, "TestZip.zip");

        // 写入测试文件
        File.WriteAllText(file1Path, "Content 1");
        File.WriteAllText(file2Path, "Content 2");

        // 执行压缩
        var zipResult = XFile.Zip(testZipPath, zipFilePath);

        // 验证结果
        Assert.That(zipResult, Is.True, "压缩操作应成功完成");
        Assert.That(XFile.HasFile(zipFilePath), Is.True, "压缩文件应被成功创建");
        Assert.That(new FileInfo(zipFilePath).Length, Is.GreaterThan(0), "压缩文件不应为空");
    }

    /// <summary>
    /// 测试 Unzip 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. 成功解压文件
    /// 2. 解压后文件完整性
    /// 3. 异步操作正确性
    /// 4. 超时处理
    /// </remarks>
    [Test]
    public void Unzip()
    {
        // 创建源文件和目录
        XFile.CreateDirectory(testZipPath);
        var file1Path = XFile.PathJoin(testZipPath, "File1.txt");
        var file2Path = XFile.PathJoin(testZipPath, "File2.txt");
        var zipFilePath = XFile.PathJoin(testBasePath, "TestUnzip.zip");

        // 写入测试文件
        File.WriteAllText(file1Path, "Content 1");
        File.WriteAllText(file2Path, "Content 2");

        // 创建zip文件
        Assert.That(XFile.Zip(testZipPath, zipFilePath), Is.True, "压缩文件应成功创建");

        // 创建解压目录
        XFile.CreateDirectory(testUnzipPath);

        // 创建TaskCompletionSource来等待异步操作完成
        var tcs = new TaskCompletionSource<bool>();

        // 执行解压
        XFile.Unzip(zipFilePath, testUnzipPath, () =>
        {
            try
            {
                // 验证解压结果
                Assert.That(XFile.HasFile(XFile.PathJoin(testUnzipPath, "File1.txt")), Is.True, "第一个文件应被成功解压");
                Assert.That(XFile.HasFile(XFile.PathJoin(testUnzipPath, "File2.txt")), Is.True, "第二个文件应被成功解压");
                Assert.That(XFile.OpenText(XFile.PathJoin(testUnzipPath, "File1.txt")), Is.EqualTo("Content 1"), "第一个文件的内容应保持一致");
                Assert.That(XFile.OpenText(XFile.PathJoin(testUnzipPath, "File2.txt")), Is.EqualTo("Content 2"), "第二个文件的内容应保持一致");
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        // 等待异步操作完成，设置20秒超时
        var timeoutTask = Task.Delay(20000);
        Task.WaitAny(tcs.Task, timeoutTask);

        if (!tcs.Task.IsCompleted)
        {
            Assert.Fail("解压操作超时（20秒）");
        }
    }

    /// <summary>
    /// 测试 FileMD5 方法。
    /// </summary>
    /// <remarks>
    /// 验证以下场景：
    /// 1. MD5计算正确性
    /// 2. 相同内容产生相同MD5
    /// 3. 不同内容产生不同MD5
    /// 4. 文件不存在的处理
    /// </remarks>
    [Test]
    public void FileMD5()
    {
        // 准备测试文件
        var testFile = XFile.PathJoin(testDirectoryPath, "md5test.txt");
        var content = "Hello, MD5!";
        XFile.SaveText(testFile, content);

        // 计算MD5
        var md5 = XFile.FileMD5(testFile);

        // 验证MD5不为空且长度正确（32个字符）
        Assert.That(md5, Is.Not.Empty, "MD5值不应为空");
        Assert.That(md5.Length, Is.EqualTo(32), "MD5值应为32个字符");

        // 验证相同内容产生相同的MD5
        var testFile2 = XFile.PathJoin(testDirectoryPath, "md5test2.txt");
        XFile.SaveText(testFile2, content);
        var md5_2 = XFile.FileMD5(testFile2);
        Assert.That(md5_2, Is.EqualTo(md5), "相同内容应产生相同的MD5值");

        // 验证不同内容产生不同的MD5
        XFile.SaveText(testFile2, "Different content");
        var md5_3 = XFile.FileMD5(testFile2);
        Assert.That(md5_3, Is.Not.EqualTo(md5), "不同内容应产生不同的MD5值");

        // 测试不存在的文件
        var nonExistentMD5 = XFile.FileMD5("nonexistent.txt");
        Assert.That(nonExistentMD5, Is.Empty, "不存在的文件应返回空MD5值");
    }
}
#endif
