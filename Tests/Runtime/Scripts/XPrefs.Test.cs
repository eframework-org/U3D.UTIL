// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;
using EFramework.Utility;

/// <summary>
/// XPrefs 模块的单元测试类，验证配置系统的基本功能、数据类型支持、配置继承和变量替换等特性。
/// </summary>
public class TestXPrefs
{
    [Test]
    public void Basic()
    {
        #region 1. 基本操作测试
        {
            var prefs = new XPrefs.IBase();
            // 验证不存在的键返回 false
            Assert.IsFalse(prefs.Has("nonexistent"), "不存在的键应返回 false");

            // 验证设置和检查键值
            prefs.Set("key", "value");
            Assert.IsTrue(prefs.Has("key"), "设置后的键应该存在");

            // 验证移除键值
            prefs.Unset("key");
            Assert.IsFalse(prefs.Has("key"), "移除后的键应该不存在");
        }
        #endregion

        #region 2. 基本类型测试
        {
            var prefs = new XPrefs.IBase();

            var basicTests = new (string name, string key, object value, object expected)[]
            {
                ("String", "strKey", "value", "value"),
                ("Int", "intKey", 42, 42),
                ("Bool", "boolKey", true, true),
                ("Float", "floatKey", 3.14f, 3.14f)
            };

            foreach (var (name, key, value, expected) in basicTests)
            {
                prefs.Set(key, value);
                object result = name switch
                {
                    "String" => prefs.GetString(key),
                    "Int" => prefs.GetInt(key),
                    "Bool" => prefs.GetBool(key),
                    "Float" => prefs.GetFloat(key),
                    _ => null
                };
                Assert.AreEqual(expected, result, $"{name} 类型的值应正确存储和读取");
            }
        }
        #endregion

        #region 3. IBase对象测试
        {
            var prefs = new XPrefs.IBase();
            var child = new XPrefs.IBase();
            child.Set("stringKey", "childValue");
            child.Set("intKey", 42);
            child.Set("arrayKey", new[] { 1, 2, 3 });

            // 验证嵌套对象的存储
            Assert.IsTrue(prefs.Set("childPrefs", child), "应成功存储嵌套的配置对象");
            var retrieved = prefs.Get<XPrefs.IBase>("childPrefs");
            Assert.IsNotNull(retrieved, "应能获取到嵌套的配置对象");
            Assert.AreEqual("childValue", retrieved.GetString("stringKey"), "嵌套对象中的字符串值应正确保存");
            Assert.AreEqual(42, retrieved.GetInt("intKey"), "嵌套对象中的整数值应正确保存");
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, retrieved.GetInts("arrayKey"), "嵌套对象中的数组应正确保存");

            // 深层嵌套测试
            var grandChild = new XPrefs.IBase();
            grandChild.Set("deepKey", "deepValue");
            child.Set("grandChild", grandChild);

            var deepRetrieved = prefs.Get<XPrefs.IBase>("childPrefs").Get<XPrefs.IBase>("grandChild");
            Assert.IsNotNull(deepRetrieved, "应能获取到深层嵌套的配置对象");
            Assert.AreEqual("deepValue", deepRetrieved.GetString("deepKey"), "深层嵌套对象中的值应正确保存");
        }
        #endregion

        #region 4. 默认值测试
        {
            var prefs = new XPrefs.IBase();
            // 验证各种类型的默认值返回
            Assert.AreEqual("default", prefs.Get("missing", "default"), "缺失的字符串键应返回默认值");
            Assert.AreEqual(100, prefs.Get("missing", 100), "缺失的整数键应返回默认值");
            Assert.IsTrue(prefs.Get("missing", true), "缺失的布尔键应返回默认值");
            Assert.AreEqual(1.23f, prefs.Get("missing", 1.23f), "缺失的浮点数键应返回默认值");

            // 验证数组类型的默认值返回
            CollectionAssert.AreEqual(new[] { "default" }, prefs.Get("missing", new[] { "default" }), "缺失的字符串数组键应返回默认数组");
            CollectionAssert.AreEqual(new[] { 1, 2 }, prefs.Get("missing", new[] { 1, 2 }), "缺失的整数数组键应返回默认数组");
            CollectionAssert.AreEqual(new[] { 1.1f }, prefs.Get("missing", new[] { 1.1f }), "缺失的浮点数数组键应返回默认数组");
            CollectionAssert.AreEqual(new[] { true }, prefs.Get("missing", new[] { true }), "缺失的布尔数组键应返回默认数组");
        }
        #endregion

        #region 5. 数组类型测试
        {
            var prefs = new XPrefs.IBase();

            var arrayTests = new (string name, string key, object value, object expected)[]
            {
                    ("String Array", "strArray", new[] { "a", "b", "c" }, new[] { "a", "b", "c" }),
                    ("Int Array", "intArray", new[] { 1, 2, 3 }, new[] { 1, 2, 3 }),
                    ("Float Array", "floatArray", new[] { 1.1f, 2.2f, 3.3f }, new[] { 1.1f, 2.2f, 3.3f }),
                    ("Bool Array", "boolArray", new[] { true, false, true }, new[] { true, false, true })
            };

            foreach (var (name, key, value, expected) in arrayTests)
            {
                prefs.Set(key, value);
                object result = name switch
                {
                    "String Array" => prefs.GetStrings(key),
                    "Int Array" => prefs.GetInts(key),
                    "Float Array" => prefs.GetFloats(key),
                    "Bool Array" => prefs.GetBools(key),
                    _ => null
                };
                CollectionAssert.AreEqual((System.Array)expected, (System.Array)result, $"{name} 类型的数组应正确存储和读取");
            }
        }
        #endregion

        #region 6. 相等性测试
        {
            var prefs1 = new XPrefs.IBase();
            prefs1.Set("intKey", 42);
            prefs1.Set("floatKey", 3.14f);
            prefs1.Set("boolKey", true);
            prefs1.Set("stringsKey", new[] { "a", "b", "c" });
            prefs1.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
            prefs1.Set("boolsKey", new[] { true, false, true });

            var child1 = new XPrefs.IBase();
            child1.Set("key", "childValue");
            prefs1.Set("child", child1);

            var prefs2 = new XPrefs.IBase();
            prefs2.Set("intKey", 42);
            prefs2.Set("floatKey", 3.14f);
            prefs2.Set("boolKey", true);
            prefs2.Set("stringsKey", new[] { "a", "b", "c" });
            prefs2.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
            prefs2.Set("boolsKey", new[] { true, false, true });

            var child2 = new XPrefs.IBase();
            child2.Set("key", "childValue");
            prefs2.Set("child", child2);

            Assert.IsTrue(prefs1.Equals(prefs2), "具有相同内容的配置对象应该相等");
        }
        #endregion
    }

    [Test]
    public void Sources()
    {
        try
        {
            #region 1. 初始化测试数据
            LogAssert.ignoreFailingMessages = true;
            // 初始化Asset测试数据
            XPrefs.asset = null;
            XPrefs.Asset.writeable = true; // 设置为可写
            XPrefs.Asset.Set("intKey", 42);
            XPrefs.Asset.Set("intsKey", new[] { 1, 2, 3 });
            XPrefs.Asset.Set("stringKey", "assetValue");
            XPrefs.Asset.Set("floatKey", 3.14f);
            XPrefs.Asset.Set("boolKey", true);
            XPrefs.Asset.Set("stringsKey", new[] { "a", "b", "c" });
            XPrefs.Asset.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
            XPrefs.Asset.Set("boolsKey", new[] { true, false, true });

            // 初始化Local测试数据
            XPrefs.Local.Set("localIntKey", 100);
            XPrefs.Local.Set("localIntsKey", new[] { 4, 5, 6 });
            XPrefs.Local.Set("localStringKey", "localValue");
            XPrefs.Local.Set("overrideKey", "localOverride");
            #endregion

            #region 2. HasKey测试
            // 验证键存在检查
            Assert.IsTrue(XPrefs.HasKey("intKey"), "Asset 配置中应存在 intKey");
            Assert.IsFalse(XPrefs.HasKey("nonexistentKey"), "不存在的键应返回 false");
            Assert.IsTrue(XPrefs.HasKey("localIntKey", XPrefs.Local), "Local 配置中应存在 localIntKey");
            Assert.IsTrue(XPrefs.HasKey("intKey", XPrefs.Local, XPrefs.Asset), "多配置源中应能找到 intKey");
            Assert.IsFalse(XPrefs.HasKey("nonexistentKey", XPrefs.Local, XPrefs.Asset), "多配置源中不存在的键应返回 false");
            #endregion

            #region 3. GetInt测试
            // 验证整数值获取
            Assert.AreEqual(42, XPrefs.GetInt("intKey"), "应正确获取 Asset 中的整数值");
            Assert.AreEqual(100, XPrefs.GetInt("localIntKey", 0, XPrefs.Local), "应正确获取 Local 中的整数值");
            Assert.AreEqual(999, XPrefs.GetInt("nonexistentKey", 999, XPrefs.Local, XPrefs.Asset), "获取不存在的键应返回默认值");
            Assert.AreEqual(3, XPrefs.GetInt("floatKey"), "浮点数应正确转换为整数");
            #endregion

            #region 4. GetInts测试
            // 验证整数数组获取
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, XPrefs.GetInts("intsKey"), "应正确获取 Asset 中的整数数组");
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, XPrefs.GetInts("localIntsKey", null, XPrefs.Local), "应正确获取 Local 中的整数数组");
            CollectionAssert.AreEqual(new[] { 7, 8, 9 }, XPrefs.GetInts("nonexistentKey", new[] { 7, 8, 9 }, XPrefs.Local, XPrefs.Asset), "获取不存在的数组应返回默认值");
            #endregion

            #region 5. Get基本类型测试
            // 验证基本类型值获取
            Assert.AreEqual("assetValue", XPrefs.GetString("stringKey"), "应正确获取字符串值");
            Assert.AreEqual(3.14f, XPrefs.GetFloat("floatKey"), "应正确获取浮点数值");
            Assert.IsTrue(XPrefs.GetBool("boolKey"), "应正确获取布尔值");
            Assert.AreEqual("localOverride", XPrefs.GetString("overrideKey", "", XPrefs.Local, XPrefs.Asset), "Local 配置应覆盖 Asset 配置");
            #endregion

            #region 6. 类型特定测试
            // 验证各种类型的特定方法
            Assert.AreEqual("assetValue", XPrefs.GetString("stringKey"), "GetString 应正确获取字符串值");
            Assert.AreEqual("default", XPrefs.GetString("nonexistentKey", "default"), "GetString 应返回默认值");
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, XPrefs.GetStrings("stringsKey"), "GetStrings 应正确获取字符串数组");
            Assert.AreEqual(3.14f, XPrefs.GetFloat("floatKey"), "GetFloat 应正确获取浮点数值");

            var expectedFloats = new[] { 1.1f, 2.2f, 3.3f };
            var actualFloats = XPrefs.GetFloats("floatsKey");
            for (int i = 0; i < expectedFloats.Length; i++)
            {
                Assert.AreEqual(expectedFloats[i], actualFloats[i], 0.001f, "GetFloats 应正确获取浮点数数组");
            }

            Assert.IsTrue(XPrefs.GetBool("boolKey"), "GetBool 应正确获取布尔值");
            CollectionAssert.AreEqual(new[] { true, false, true }, XPrefs.GetBools("boolsKey"), "GetBools 应正确获取布尔数组");
            #endregion

            #region 7. 边界情况测试
            // 验证边界情况
            Assert.AreEqual(42, XPrefs.GetInt("intKey", 0, null), "空配置源列表应默认使用 Asset");
            Assert.AreEqual(42, XPrefs.GetInt("intKey", 0), "无配置源应默认使用 Asset");
            #endregion

            #region 8. 类型不匹配测试
            // 验证类型不匹配情况
            XPrefs.Asset.Set("mismatchKey", "not an int");
            Assert.AreEqual(0, XPrefs.GetInt("mismatchKey"), "类型不匹配时应返回类型默认值");
            #endregion
        }
        finally
        {
            // 清理测试数据
            XPrefs.Asset.Unset("intKey");
            XPrefs.Asset.Unset("intsKey");
            XPrefs.Asset.Unset("stringKey");
            XPrefs.Asset.Unset("floatKey");
            XPrefs.Asset.Unset("boolKey");
            XPrefs.Asset.Unset("stringsKey");
            XPrefs.Asset.Unset("floatsKey");
            XPrefs.Asset.Unset("boolsKey");
            XPrefs.Asset.Unset("mismatchKey");

            XPrefs.Local.Unset("localIntKey");
            XPrefs.Local.Unset("localIntsKey");
            XPrefs.Local.Unset("localStringKey");
            XPrefs.Local.Unset("overrideKey");
        }
    }

    [Test]
    public void Persist()
    {
        #region 1. 准备测试环境
        LogAssert.ignoreFailingMessages = true;
        var tmpDir = XFile.PathJoin(XEnv.LocalPath, "TestXPrefs");
        if (!XFile.HasDirectory(tmpDir)) XFile.CreateDirectory(tmpDir);

        try
        {
            var testFile = XFile.PathJoin(tmpDir, "test_persist.json");
            var prefs = new XPrefs.IBase();

            // 准备测试数据
            var testData = @"{
                    ""stringKey"": ""stringValue"",
                    ""intKey"": 123,
                    ""boolKey"": true,
                    ""intSliceKey"": [1, 2, 3],
                    ""floatSliceKey"": [1.1, 2.2, 3.3],
                    ""stringSliceKey"": [""a"", ""b"", ""c""],
                    ""boolSliceKey"": [true, false, true]
                }";

            // 写入测试文件
            XFile.SaveText(testFile, testData);
            #endregion

            #region 2. 测试读取配置
            Assert.IsTrue(prefs.Read(testFile), "Should read file successfully");

            // 验证各种类型的数据
            Assert.AreEqual("stringValue", prefs.GetString("stringKey"));
            Assert.AreEqual(123, prefs.GetInt("intKey"));
            Assert.IsTrue(prefs.GetBool("boolKey"));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, prefs.GetInts("intSliceKey"));

            var expectedFloats = new[] { 1.1f, 2.2f, 3.3f };
            var actualFloats = prefs.GetFloats("floatSliceKey");
            for (int i = 0; i < expectedFloats.Length; i++)
            {
                Assert.AreEqual(expectedFloats[i], actualFloats[i], 0.001f);
            }

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, prefs.GetStrings("stringSliceKey"));
            CollectionAssert.AreEqual(new[] { true, false, true }, prefs.GetBools("boolSliceKey"));
            #endregion

            #region 3. 测试读取不存在的文件
            var nonExistentFile = XFile.PathJoin(tmpDir, "nonexistent.json");
            Assert.IsFalse(prefs.Read(nonExistentFile), "Should fail reading non-existent file");
            #endregion

            #region 4. 测试读取无效的JSON
            var invalidFile = XFile.PathJoin(tmpDir, "invalid.json");
            XFile.SaveText(invalidFile, "invalid json");
            Assert.IsFalse(prefs.Read(invalidFile), "Should fail reading invalid JSON");
            #endregion

            #region 5. 测试复杂JSON
            var complexData = @"{
                    ""nullValue"": null,
                    ""emptyObject"": {},
                    ""emptyArray"": [],
                    ""nestedObject"": {
                        ""key"": ""value""
                    },
                    ""mixedArray"": [1, ""two"", true, null]
                }";

            var complexFile = XFile.PathJoin(tmpDir, "complex.json");
            XFile.SaveText(complexFile, complexData);

            var complexPrefs = new XPrefs.IBase();
            Assert.IsTrue(complexPrefs.Read(complexFile));

            Assert.IsNull(complexPrefs.Get<object>("nullValue"));
            Assert.IsNotNull(complexPrefs.Get<XPrefs.IBase>("emptyObject"));
            Assert.IsNull(complexPrefs.Get<object[]>("emptyArray"));
            Assert.IsNotNull(complexPrefs.Get<XPrefs.IBase>("nestedObject"));
            Assert.IsNull(complexPrefs.Get<object[]>("mixedArray"));
            #endregion

            #region 6. 测试大文件
            var largePrefs = new XPrefs.IBase();
            for (int i = 0; i < 1000; i++)
            {
                largePrefs.Set($"key{i}", $"value{i}");
            }

            var largeFile = XFile.PathJoin(tmpDir, "large.json");
            largePrefs.File = largeFile;
            Assert.IsTrue(largePrefs.Save(), "Should save large file successfully");

            var loadedLargePrefs = new XPrefs.IBase();
            Assert.IsTrue(loadedLargePrefs.Read(largeFile));
            Assert.AreEqual("value42", loadedLargePrefs.GetString("key42"));
            #endregion
        }
        finally
        {
            // 清理测试目录
            if (XFile.HasDirectory(tmpDir))
            {
                XFile.DeleteDirectory(tmpDir, true);
            }
        }
    }

    [Test]
    public void Eval()
    {
        #region 1. 基本替换测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("name", "John");
            pf.Set("greeting", "Hello ${Prefs.name}");

            var result = pf.Eval("${Prefs.greeting}");
            Assert.AreEqual("Hello John", result);
        }
        #endregion

        #region 2. 缺失变量测试
        {
            var pf = new XPrefs.IBase();
            var result = pf.Eval("${Prefs.missing}");
            Assert.AreEqual("${Prefs.missing}(Unknown)", result);
        }
        #endregion

        #region 3. 递归变量测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("recursive1", "${Prefs.recursive2}");
            pf.Set("recursive2", "${Prefs.recursive1}");

            var result = pf.Eval("${Prefs.recursive1}");
            Assert.AreEqual("${Prefs.recursive1}(Recursive)", result);
        }
        #endregion

        #region 4. 嵌套变量测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("outer", "value");

            var result = pf.Eval("${Prefs.outer${Prefs.inner}}");
            Assert.AreEqual("${Prefs.outer${Prefs.inner}(Nested)}", result);
        }
        #endregion

        #region 5. 多重替换测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("first", "John");
            pf.Set("last", "Doe");

            var child = new XPrefs.IBase();
            child.Set("name", "Mike");
            pf.Set("child", child);

            var result = pf.Eval("${Prefs.first} and ${Prefs.last} has a child named ${Prefs.child.name} age ${Prefs.child.age}");
            Assert.AreEqual("John and Doe has a child named Mike age ${Prefs.child.age}(Unknown)", result);
        }
        #endregion

        #region 6. 空值测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("empty", "");

            var result = pf.Eval("test${Prefs.empty}end");
            Assert.AreEqual("test${Prefs.empty}(Unknown)end", result);
        }
        #endregion
    }

    [Test]
    public void Override()
    {
        #region 1. 准备测试环境
        LogAssert.ignoreFailingMessages = true;
        var tmpDir = XFile.PathJoin(XEnv.LocalPath, "TestXPrefs-" + XTime.GetMillisecond());
        if (!XFile.HasDirectory(tmpDir)) XFile.CreateDirectory(tmpDir);

        try
        {
            // 准备配置文件
            var configData = @"{
                    ""key1"": ""value1"",
                    ""key2"": 42
                }";

            var assetFile = XFile.PathJoin(tmpDir, "asset.json");
            var localFile = XFile.PathJoin(tmpDir, "local.json");
            var customLocalFile = XFile.PathJoin(tmpDir, "custom_local.json");

            XFile.SaveText(assetFile, configData);
            XFile.SaveText(localFile, configData);
            XFile.SaveText(customLocalFile, @"{
                    ""customKey"": ""customValue""
                }");

            try
            {
                #region 2. 测试Local配置文件路径覆盖
                XEnv.ParseArgs(true, "--Prefs@Local=" + customLocalFile);
                XPrefs.local = null;
                var local = XPrefs.Local;
                Assert.AreEqual(customLocalFile, local.File);
                Assert.AreEqual("customValue", local.GetString("customKey"));
                #endregion

                #region 3. 测试Local配置文件不存在时的行为
                XEnv.ParseArgs(true, "--Prefs@Local=nonexistent.json");
                XPrefs.local = null;
                local = XPrefs.Local;
                Assert.AreEqual("nonexistent.json", local.File);
                Assert.IsFalse(local.Has("key1")); // 文件不存在时应该是空配置
                #endregion

                #region 4. 测试Asset配置文件路径覆盖
                if (Application.isEditor)
                {
                    XEnv.ParseArgs(true, "--Prefs@Asset=" + customLocalFile);

                    XPrefs.asset = null;
                    Assert.AreEqual(customLocalFile, XPrefs.Asset.File);
                    Assert.AreEqual("customValue", XPrefs.Asset.GetString("customKey"));
                }
                #endregion

                #region 5. 测试Asset配置文件不存在时的行为
                if (Application.isEditor)
                {
                    XEnv.ParseArgs(true, "--Prefs@Asset=nonexistent.json");
                    XPrefs.asset = null;
                    Assert.AreEqual("nonexistent.json", XPrefs.Asset.File);
                    Assert.IsFalse(XPrefs.Asset.Has("key1")); // 文件不存在时应该是空配置
                }
                #endregion

                #region 6. 测试Asset和Local参数混合
                XEnv.ParseArgs(true,
                    "--Prefs@Asset.key2=100",
                    "--Prefs@Asset.key3=asset value",
                    "--Prefs@Local.key2=200",
                    "--Prefs@Local.key3=local value",
                    "--Prefs@Local=" + localFile
                );
                XPrefs.local = null;

                var asset = new XPrefs.IAsset();
                Assert.IsTrue(asset.Read(assetFile));
                local = XPrefs.Local;

                // 验证Asset结果
                Assert.AreEqual("value1", asset.GetString("key1")); // 原值保持不变
                Assert.AreEqual(100, asset.GetInt("key2")); // 被Asset命令行参数覆盖
                Assert.AreEqual("asset value", asset.GetString("key3")); // Asset新增参数

                // 验证Local结果
                Assert.AreEqual("value1", local.GetString("key1")); // 原值保持不变
                Assert.AreEqual(200, local.GetInt("key2")); // 被Local命令行参数覆盖
                Assert.AreEqual("local value", local.GetString("key3")); // Local新增参数
                #endregion

                #region 7. 测试多级路径覆盖
                XEnv.ParseArgs(true,
                    "--Prefs.Log.Std.Config.Level=Debug",
                    "--Prefs@Asset.UI.Window.Style.Theme=Dark",
                    "--Prefs@Local.Network.Server.Config.Port=8080",
                    "--Prefs@Local=" + localFile
                );

                XPrefs.local = null;
                asset = new XPrefs.IAsset();
                Assert.IsTrue(asset.Read(assetFile));
                local = XPrefs.Local;

                // 验证Asset多级路径
                var logConfig = asset.Get<XPrefs.IBase>("Log")
                                    .Get<XPrefs.IBase>("Std")
                                    .Get<XPrefs.IBase>("Config");
                Assert.AreEqual("Debug", logConfig.GetString("Level"));

                var uiConfig = asset.Get<XPrefs.IBase>("UI")
                                   .Get<XPrefs.IBase>("Window")
                                   .Get<XPrefs.IBase>("Style");
                Assert.AreEqual("Dark", uiConfig.GetString("Theme"));

                // 验证Local多级路径
                var networkConfig = local.Get<XPrefs.IBase>("Network")
                                       .Get<XPrefs.IBase>("Server")
                                       .Get<XPrefs.IBase>("Config");
                Assert.AreEqual("8080", networkConfig.GetString("Port"));
                #endregion

                #region 8. 测试多层覆盖优先级
                XEnv.ParseArgs(true,
                    "--Prefs.sharedKey=base value",
                    "--Prefs@Asset.sharedKey=asset value",
                    "--Prefs@Local.sharedKey=local value",
                    "--Prefs@Local=" + localFile
                );

                XPrefs.local = null;
                asset = new XPrefs.IAsset();
                Assert.IsTrue(asset.Read(assetFile));
                local = XPrefs.Local;

                Assert.AreEqual("asset value", asset.GetString("sharedKey")); // Asset特定覆盖优先
                Assert.AreEqual("local value", local.GetString("sharedKey")); // Local特定覆盖优先
                #endregion

                #region 9. 测试Local配置文件和参数覆盖的顺序
                var localData = @"{
                        ""orderKey"": ""file value""
                    }";
                XFile.SaveText(localFile, localData);

                XEnv.ParseArgs(true,
                    "--Prefs@Local.orderKey=override value",
                    "--Prefs@Local=" + localFile
                );

                XPrefs.local = null;
                local = XPrefs.Local;
                Assert.AreEqual("override value", local.GetString("orderKey")); // 命令行参数应该优先于文件内容
                #endregion
            }
            finally
            {
                // 重置测试参数
                XEnv.ParseArgs(true);
                XPrefs.asset = null;
                XPrefs.local = null;
            }
        }
        finally
        {
            if (XFile.HasDirectory(tmpDir))
            {
                XFile.DeleteDirectory(tmpDir, true);
            }
        }
        #endregion
    }
}
#endif
