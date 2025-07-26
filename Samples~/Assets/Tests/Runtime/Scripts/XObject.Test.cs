// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using EFramework.Utility;
using NUnit.Framework;
using System.Runtime.InteropServices;

public class TestXObject
{
    private struct TestStruct
    {
        public int IntTest;
        public bool BoolTest;
    }

    private class TestClass
    {
        public int Id;
        public string Name;
    }

    [Test]
    public void ToByte()
    {
        // Arrange
        var testObj = new TestStruct { IntTest = 1, BoolTest = true };

        // Act
        var bytes = XObject.ToByte(testObj);

        // Assert
        Assert.IsNotNull(bytes, "结构体序列化后的字节数组不应为空");
        Assert.AreEqual(Marshal.SizeOf(typeof(TestStruct)), bytes.Length, "序列化后的字节数组长度应与结构体大小相同");
    }

    [Test]
    public void FromByte()
    {
        // Arrange
        var testObj = new TestStruct { IntTest = 1, BoolTest = false };

        // Act
        var bytes = XObject.ToByte(testObj);
        var deserializedObj = XObject.FromByte<TestStruct>(bytes);

        // Assert
        Assert.AreEqual(testObj.IntTest, deserializedObj.IntTest, "反序列化后的整数字段值应与原始值相同");
        Assert.AreEqual(testObj.BoolTest, deserializedObj.BoolTest, "反序列化后的布尔字段值应与原始值相同");
    }

    [Test]
    public void FromJson()
    {
        // Arrange
        string json = "{\"Id\":1,\"Name\":\"Test\"}";

        // Act
        var resultFromString = XObject.FromJson<TestClass>(json);
        var resultFromNode = XObject.FromJson<TestClass>(JSON.Parse(json));

        // Assert
        Assert.IsNotNull(resultFromString, "从字符串解析的对象不应为空");
        Assert.AreEqual(1, resultFromString.Id, "从字符串解析的对象 Id 应为 1");
        Assert.AreEqual("Test", resultFromString.Name, "从字符串解析的对象 Name 应为 'Test'");

        Assert.IsNotNull(resultFromNode, "从 JSONNode 解析的对象不应为空");
        Assert.AreEqual(1, resultFromNode.Id, "从 JSONNode 解析的对象 Id 应为 1");
        Assert.AreEqual("Test", resultFromNode.Name, "从 JSONNode 解析的对象 Name 应为 'Test'");
    }

    [Test]
    public void ToJson()
    {
        // Arrange
        var testObj = new TestClass { Id = 1, Name = "Test" };

        // Act
        var jsonPretty = XObject.ToJson(testObj, true);
        var jsonCompact = XObject.ToJson(testObj, false);

        // Assert
        Assert.IsNotNull(jsonPretty, "格式化的 JSON 字符串不应为空");
        Assert.IsTrue(jsonPretty.Contains("\"Id\": 1"), "格式化的 JSON 应包含格式化后的 Id 字段");
        Assert.IsTrue(jsonPretty.Contains("\"Name\": \"Test\""), "格式化的 JSON 应包含格式化后的 Name 字段");

        Assert.IsNotNull(jsonCompact, "压缩的 JSON 字符串不应为空");
        Assert.IsTrue(jsonCompact.Contains("\"Id\":1"), "压缩的 JSON 应包含未格式化的 Id 字段");
        Assert.IsTrue(jsonCompact.Contains("\"Name\":\"Test\""), "压缩的 JSON 应包含未格式化的 Name 字段");
    }
}
#endif
