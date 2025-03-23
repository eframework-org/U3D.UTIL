// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using System.IO;
using EFramework.Utility;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// XEnv 环境工具类的单元测试。
/// </summary>
public class TestXEnv
{
    /// <summary>
    /// 清理测试环境。
    /// </summary>
    /// <remarks>
    /// 重置命令行参数缓存，确保不影响其他测试。
    /// </remarks>
    [OneTimeTearDown]
    public void Cleanup() { XEnv.ParseArgs(reset: true); }

    /// <summary>
    /// 测试环境元数据的有效性。
    /// </summary>
    /// <remarks>
    /// 验证以下环境信息是否正确设置：
    /// - 平台类型不为未知
    /// - 设备标识符不为空
    /// - 解决方案名称不为空
    /// - 项目名称不为空
    /// - 产品名称不为空
    /// - 发布渠道不为空
    /// - 版本号不为空
    /// - 作者信息不为空
    /// </remarks>
    [Test]
    public void Metas()
    {
        Assert.IsTrue(XEnv.Platform != XEnv.PlatformType.Unknown, "平台类型不应为未知");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.DeviceID), "设备标识符不应为空");
        // Assert.IsFalse(string.IsNullOrEmpty(XEnv.MacAddr), "MAC 地址不应为空"); // 注意：Ubuntu、iOS等平台下有可能获取不到，这里不进行测试
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Solution), "解决方案名称不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Project), "项目名称不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Product), "产品名称不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Channel), "发布渠道不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Version), "版本号不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Author), "作者信息不应为空");
    }

    /// <summary>
    /// 测试环境配置的默认值。
    /// </summary>
    /// <remarks>
    /// 验证所有配置项的默认值是否正确设置：
    /// - 应用类型默认值
    /// - 运行模式默认值
    /// - 解决方案名称默认值
    /// - 项目名称默认值
    /// - 产品名称默认值
    /// - 发布渠道默认值
    /// - 版本号默认值
    /// - 作者信息默认值
    /// - 密钥默认值
    /// - 远程配置地址默认值
    /// </remarks>
    [Test]
    public void Prefs()
    {
        Assert.IsNotNull(XEnv.Prefs.AppDefault, "应用类型默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.ModeDefault, "运行模式默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.SolutionDefault, "解决方案名称默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.ProjectDefault, "项目名称默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.ProductDefault, "产品名称默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.ChannelDefault, "发布渠道默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.VersionDefault, "版本号默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.AuthorDefault, "作者信息默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.SecretDefault, "密钥默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.RemoteDefault, "远程配置地址默认值不应为空");
    }

    /// <summary>
    /// 测试路径管理功能。
    /// </summary>
    /// <remarks>
    /// 测试以下路径相关功能：
    /// 1. 项目路径的有效性
    /// 2. 资源路径的正确性
    /// 3. 本地数据目录的创建
    /// 4. 自定义本地路径的设置和验证
    /// </remarks>
    [Test]
    public void Paths()
    {
        var tempDir = XFile.PathJoin(XEnv.LocalPath, "TestXEnv-" + XTime.GetMillisecond());
        if (!XFile.HasDirectory(tempDir)) XFile.DeleteDirectory(tempDir);

        try
        {
            // 验证 ProjectPath 是否存在
            if (Application.isEditor) Assert.That(XFile.HasDirectory(XEnv.ProjectPath), Is.True);

            // 验证 AssetPath 是否正确
            Assert.AreEqual(XEnv.AssetPath, Application.streamingAssetsPath);

            // 验证 LocalPath 是否创建
            Assert.IsTrue(XFile.HasDirectory(XEnv.LocalPath));

            // 测试自定义LocalPath
            var customLocal = XFile.PathJoin(tempDir, "CustomLocal");

            // 重置参数缓存并设置自定义路径
            XEnv.ParseArgs(true, "-LocalPath", customLocal);
            XEnv.localPath = null;

            // 验证自定义值
            Assert.AreEqual(Path.GetFullPath(customLocal), Path.GetFullPath(XEnv.LocalPath));

            // 验证本地目录已创建
            Assert.IsTrue(XFile.HasDirectory(XEnv.LocalPath));
        }
        finally
        {
            XFile.DeleteDirectory(tempDir);
            XEnv.ParseArgs(true);
            XEnv.localPath = null;
        }
    }

    /// <summary>
    /// 测试命令行参数解析功能。
    /// </summary>
    /// <remarks>
    /// 测试以下命令行参数场景：
    /// 1. 基本参数形式的解析
    /// 2. 多种参数形式的组合
    /// 3. 特殊值的处理
    /// 4. 参数缓存的控制
    /// 5. 缓存重置功能
    /// 6. 空参数列表处理
    /// 7. 无效参数处理
    /// 8. 参数列表直接访问
    /// </remarks>
    [Test]
    public void Args()
    {
        try
        {
            // 测试用例1：基本参数形式
            XEnv.ParseArgs(true, "--test=value");
            Assert.AreEqual("value", XEnv.GetArg("test"), "基本参数形式应正确解析键值对");

            // 测试用例2：多种参数形式
            XEnv.ParseArgs(true,
              "--key1=value1",          // 双横杠等号
              "-key2=value2",           // 单横杠等号
              "--flag1",                // 双横杠无值
              "-flag2",                 // 单横杠无值
              "--key3=value3",          // 双横杠等号
              "-key4", "value4",        // 单横杠空格
              "--key5", "value5",       // 双横杠空格
              "-flag3",                 // 单横杠无值
              "-key6=with=equals",      // 单横杠多等号
              "--key7=with=equals"      // 双横杠多等号
            );

            Assert.AreEqual("value1", XEnv.GetArg("key1"), "双横杠等号形式的参数应正确解析");
            Assert.AreEqual("value2", XEnv.GetArg("key2"), "单横杠等号形式的参数应正确解析");
            Assert.AreEqual("", XEnv.GetArg("flag1"), "双横杠无值标志应解析为空字符串");
            Assert.AreEqual("", XEnv.GetArg("flag2"), "单横杠无值标志应解析为空字符串");
            Assert.AreEqual("value3", XEnv.GetArg("key3"), "双横杠等号形式的参数应正确解析");
            Assert.AreEqual("value4", XEnv.GetArg("key4"), "单横杠空格分隔的参数应正确解析");
            Assert.AreEqual("value5", XEnv.GetArg("key5"), "双横杠空格分隔的参数应正确解析");
            Assert.AreEqual("", XEnv.GetArg("flag3"), "单横杠无值标志应解析为空字符串");
            Assert.AreEqual("with=equals", XEnv.GetArg("key6"), "含等号的值应完整保留");
            Assert.AreEqual("with=equals", XEnv.GetArg("key7"), "含等号的值应完整保留");

            // 测试用例3：特殊值处理
            XEnv.ParseArgs(true,
                "--empty=",
                "--spaces=value with spaces",
                "--chinese=中文参数",
                "--symbols=!@#$%^&*()",
                "--multi=value=with=equals"
            );

            Assert.AreEqual("", XEnv.GetArg("empty"), "空值参数应解析为空字符串");
            Assert.AreEqual("value with spaces", XEnv.GetArg("spaces"), "含空格的值应完整保留");
            Assert.AreEqual("中文参数", XEnv.GetArg("chinese"), "中文参数应正确解析");
            Assert.AreEqual("!@#$%^&*()", XEnv.GetArg("symbols"), "特殊字符应完整保留");
            Assert.AreEqual("value=with=equals", XEnv.GetArg("multi"), "多等号的值应完整保留");

            // 测试用例4：缓存控制
            XEnv.ParseArgs(false, "--newkey=newvalue");
            Assert.AreEqual("中文参数", XEnv.GetArg("chinese"), "不重置缓存时应保留原有参数值");
            Assert.AreEqual("!@#$%^&*()", XEnv.GetArg("symbols"), "不重置缓存时应保留原有参数值");

            // 测试用例5：重置缓存
            XEnv.ParseArgs(true, "--single=value");
            Assert.AreEqual("value", XEnv.GetArg("single"), "重置缓存后新参数应生效");
            Assert.AreEqual("", XEnv.GetArg("chinese"), "重置缓存后原有参数应被清除");

            // 测试用例6：空参数列表
            XEnv.ParseArgs(true);

            // 测试用例7：无效参数形式
            XEnv.ParseArgs(true,
                "invalid",
                "--valid=value",
                "--"
            );
            Assert.AreEqual("value", XEnv.GetArg("valid"), "有效参数应正确解析，忽略无效参数");

            // 测试用例8：参数列表直接访问
            var args8 = XEnv.ParseArgs(true,
                "--key1=value1",
                "--key2=value2"
            );

            bool foundKey1 = false;
            bool foundKey2 = false;

            foreach (var pair in args8)
            {
                if (pair.Key == "key1")
                {
                    Assert.AreEqual("value1", pair.Value, "参数列表中的键值对应正确保存");
                    foundKey1 = true;
                }
                else if (pair.Key == "key2")
                {
                    Assert.AreEqual("value2", pair.Value, "参数列表中的键值对应正确保存");
                    foundKey2 = true;
                }
            }

            Assert.IsTrue(foundKey1, "参数列表应包含第一个测试键值对");
            Assert.IsTrue(foundKey2, "参数列表应包含第二个测试键值对");
        }
        finally
        {
            // 重置参数缓存
            XEnv.ParseArgs(true);
        }
    }

    /// <summary>
    /// 测试环境变量解析功能。
    /// </summary>
    /// <remarks>
    /// 测试以下环境变量解析场景：
    /// 1. 命令行参数的解析
    /// 2. 系统环境变量的解析
    /// 3. 参数优先级处理
    /// 4. 缺失变量处理
    /// 5. 嵌套变量处理
    /// </remarks>
    [Test]
    public void Eval()
    {
        try
        {
            var testVar = $"TEST_VAR_{XTime.GetMillisecond()}";

            // 测试命令行参数解析
            XEnv.ParseArgs(true, "-test", "value");
            var result1 = "prefix ${Env.test} suffix".Eval(XEnv.Vars);
            Assert.AreEqual("prefix value suffix", result1, "应正确解析命令行参数的环境变量引用");

            // 测试系统环境变量解析
            Environment.SetEnvironmentVariable(testVar, "env_value");
            var result2 = ("prefix ${Env." + testVar + "} suffix").Eval(XEnv.Vars);
            Assert.AreEqual("prefix env_value suffix", result2, "应正确解析系统环境变量引用");

            // 测试参数优先级
            XEnv.ParseArgs(true, $"-{testVar}", "arg_value");
            var result3 = ("${Env." + testVar + "}").Eval(XEnv.Vars);
            Assert.AreEqual("arg_value", result3, "命令行参数应优先于系统环境变量");

            // 测试缺失变量处理
            XEnv.ParseArgs(true);
            var result4 = "hello ${Env.missing}".Eval(XEnv.Vars);
            Assert.IsTrue(result4.Contains("(Unknown)"), "未定义的环境变量应标记为未知");

            // 测试嵌套变量处理
            var result5 = "nested ${Env.outer${Env.inner}}".Eval(XEnv.Vars);
            Assert.IsTrue(result5.Contains("(Nested)"), "嵌套的环境变量引用应标记为嵌套");
        }
        finally
        {
            // 重置参数缓存
            XEnv.ParseArgs(true);
        }
    }
}
#endif
