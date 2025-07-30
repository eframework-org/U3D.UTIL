// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System;
using System.IO;
using UnityEngine;
using EFramework.Utility;

/// <summary>
/// XEnv 环境工具类的单元测试。
/// </summary>
public class TestXEnv
{
    /// <summary>
    /// 清理测试环境。
    /// </summary>
    [OneTimeTearDown]
    public void Cleanup() { XEnv.ParseArgs(reset: true); }

    /// <summary>
    /// 测试环境元数据的有效性。
    /// </summary>
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
    [Test]
    public void Prefs()
    {
        Assert.IsNotNull(XEnv.Prefs.AppDefault, "Env/App 应用类型默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.ModeDefault, "Env/Mode 运行模式默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.SolutionDefault, "Env/Solution 解决方案名称默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.ProjectDefault, "Env/Project 项目名称默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.ProductDefault, "Env/Product 产品名称默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.ChannelDefault, "Env/Channel 发布渠道默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.VersionDefault, "Env/Version 版本号默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.AuthorDefault, "Env/Author 作者信息默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.SecretDefault, "Env/Secret 应用密钥默认值不应为空");
        Assert.IsNotNull(XEnv.Prefs.RemoteDefault, "Env/Remote 远程配置地址默认值不应为空");

        var panel = ScriptableObject.CreateInstance<XEnv.Prefs>();
        var targetPrefs = new XPrefs.IBase();
        panel.OnSave(source: new XPrefs.IBase(), target: targetPrefs);

        // Save
        Assert.AreEqual(XEnv.Prefs.AppDefault, targetPrefs.GetString(XEnv.Prefs.App), "Env/App 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.ModeDefault, targetPrefs.GetString(XEnv.Prefs.Mode), "Env/Mode 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.SolutionDefault, targetPrefs.GetString(XEnv.Prefs.Solution), "Env/Solution 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.ProjectDefault, targetPrefs.GetString(XEnv.Prefs.Project), "Env/Project 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.ProductDefault, targetPrefs.GetString(XEnv.Prefs.Product), "Env/Product 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.ChannelDefault, targetPrefs.GetString(XEnv.Prefs.Channel), "Env/Channel 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.VersionDefault, targetPrefs.GetString(XEnv.Prefs.Version), "Env/Version 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.AuthorDefault, targetPrefs.GetString(XEnv.Prefs.Author), "Env/Author 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.SecretDefault, targetPrefs.GetString(XEnv.Prefs.Secret), "Env/Secret 配置项应当存在。");
        Assert.AreEqual(XEnv.Prefs.RemoteDefault, targetPrefs.GetString(XEnv.Prefs.Remote), "Env/Remote 配置项应当存在。");

        // Apply - asset
        var lastRemote = targetPrefs.GetString(XEnv.Prefs.Remote);
        panel.OnApply(source: new XPrefs.IBase(), target: targetPrefs, asset: true, local: false, remote: false);
        Assert.AreNotEqual(lastRemote, targetPrefs.GetString(XEnv.Prefs.Remote), "Env/Remote 配置项在资产中应当被求值。");

        // Apply - remote
        targetPrefs = new XPrefs.IBase();
        panel.OnApply(source: new XPrefs.IBase(), target: targetPrefs, asset: false, local: false, remote: true);
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.App), "Env/App 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Mode), "Env/Mode 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Solution), "Env/Solution 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Project), "Env/Project 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Product), "Env/Product 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Channel), "Env/Channel 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Version), "Env/Version 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Author), "Env/Author 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Secret), "Env/Secret 配置项在远端应当被移除。");
        Assert.IsFalse(targetPrefs.Has(XEnv.Prefs.Remote), "Env/Remote 配置项在远端应当被移除。");
    }

    /// <summary>
    /// 测试路径管理功能。
    /// </summary>
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
    [Test]
    public void Eval()
    {
        try
        {
            var testVar = $"TEST_VAR_{XTime.GetMillisecond()}";

            #region 测试命令行参数解析
            {
                XEnv.ParseArgs(true, "-test", "value");
                var result1 = "prefix ${Env.test} suffix".Eval(XEnv.Vars);
                Assert.AreEqual("prefix value suffix", result1, "应正确解析命令行参数的环境变量引用");
            }
            #endregion

            #region 测试系统环境变量解析
            {
                Environment.SetEnvironmentVariable(testVar, "env_value");
                var result2 = ("prefix ${Env." + testVar + "} suffix").Eval(XEnv.Vars);
                Assert.AreEqual("prefix env_value suffix", result2, "应正确解析系统环境变量引用");
            }
            #endregion

            #region 测试内置变量解析
            {
                Assert.AreEqual("${Env.LocalPath}".Eval(XEnv.Vars), XEnv.LocalPath, "${Env.LocalPath} 解析后应当和 XEnv.LocalPath 相等。");
                Assert.AreEqual("${Env.ProjectPath}".Eval(XEnv.Vars), XEnv.ProjectPath, "${Env.ProjectPath} 解析后应当和 XEnv.ProjectPath 相等。");
                Assert.AreEqual("${Env.AssetPath}".Eval(XEnv.Vars), XEnv.AssetPath, "${Env.AssetPath} 解析后应当和 XEnv.AssetPath 相等。");
                Assert.AreEqual("${Env.UserName}".Eval(XEnv.Vars), Environment.UserName, "${Env.UserName} 解析后应当和 Environment.UserName 相等。");
                Assert.AreEqual("${Env.Platform}".Eval(XEnv.Vars), XEnv.Platform.ToString(), "${Env.Platform} 解析后应当和 XEnv.Platform 相等。");
                Assert.AreEqual("${Env.App}".Eval(XEnv.Vars), XEnv.App.ToString(), "${Env.App} 解析后应当和 XEnv.App 相等。");
                Assert.AreEqual("${Env.Mode}".Eval(XEnv.Vars), XEnv.Mode.ToString(), "${Env.Mode} 解析后应当和 XEnv.Mode 相等。");
                Assert.AreEqual("${Env.Solution}".Eval(XEnv.Vars), XEnv.Solution, "${Env.Solution} 解析后应当和 XEnv.Solution 相等。");
                Assert.AreEqual("${Env.Project}".Eval(XEnv.Vars), XEnv.Project, "${Env.Project} 解析后应当和 XEnv.Project 相等。");
                Assert.AreEqual("${Env.Product}".Eval(XEnv.Vars), XEnv.Product, "${Env.Product} 解析后应当和 XEnv.Product 相等。");
                Assert.AreEqual("${Env.Channel}".Eval(XEnv.Vars), XEnv.Channel, "${Env.Channel} 解析后应当和 XEnv.Channel 相等。");
                Assert.AreEqual("${Env.Version}".Eval(XEnv.Vars), XEnv.Version, "${Env.Version} 解析后应当和 XEnv.Version 相等。");
                Assert.AreEqual("${Env.Author}".Eval(XEnv.Vars), XEnv.Author, "${Env.Author} 解析后应当和 XEnv.Author 相等。");
                Assert.AreEqual("${Env.Secret}".Eval(XEnv.Vars), XEnv.Secret, "${Env.Secret} 解析后应当和 XEnv.Secret 相等。");
                Assert.AreEqual("${Env.NumCPU}".Eval(XEnv.Vars), SystemInfo.processorCount.ToString(), "${Env.NumCPU} 解析后应当和 SystemInfo.processorCount 相等。");
            }
            #endregion

            #region 测试参数优先级
            {
                XEnv.ParseArgs(true, $"-{testVar}", "arg_value");
                var result3 = ("${Env." + testVar + "}").Eval(XEnv.Vars);
                Assert.AreEqual("arg_value", result3, "命令行参数应优先于系统环境变量");
            }
            #endregion

            #region 测试缺失变量处理
            {
                XEnv.ParseArgs(true);
                var result4 = "hello ${Env.missing}".Eval(XEnv.Vars);
                Assert.IsTrue(result4.Contains("(Unknown)"), "未定义的环境变量应标记为未知");
            }
            #endregion

            #region 测试嵌套变量处理
            {
                var result5 = "nested ${Env.outer${Env.inner}}".Eval(XEnv.Vars);
                Assert.IsTrue(result5.Contains("(Nested)"), "嵌套的环境变量引用应标记为嵌套");
            }
            #endregion
        }
        finally
        {
            // 重置参数缓存
            XEnv.ParseArgs(true);
        }
    }
}
#endif
