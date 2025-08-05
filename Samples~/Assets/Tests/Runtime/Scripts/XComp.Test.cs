// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using EFramework.Utility;

/// <summary>
/// XComp 组件工具集单元测试。
/// </summary>
public class TestXComp
{
    #region 测试环境准备
    private GameObject rootObject;

    private GameObject childObject;

    private GameObject grandChildObject;

    [SetUp]
    public void Setup()
    {
        // 创建测试用的对象层级
        rootObject = new GameObject("Root");
        childObject = new GameObject("Child");
        grandChildObject = new GameObject("GrandChild");

        childObject.transform.SetParent(rootObject.transform);
        grandChildObject.transform.SetParent(childObject.transform);
    }

    [TearDown]
    public void Reset()
    {
        // 清理测试对象
        Object.DestroyImmediate(rootObject);
    }
    #endregion

    #region 节点封装测试
    /// <summary>
    /// 测试从不同路径获取 Transform 组件的功能
    /// </summary>
    /// <param name="path">目标对象的路径，null 表示根对象</param>
    [TestCase(null)]
    [TestCase("Child")]
    [TestCase("Child/GrandChild")]
    public void GetTransform(string path)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path) : rootObject.transform;

        // Act
        var result = target.GetTransform();

        // Assert - 验证获取的 Transform 与目标对象的 Transform 完全一致
        Assert.AreEqual(target, result, "获取的 Transform 应与目标对象的 Transform 相同");
    }

    /// <summary>
    /// 测试获取子对象功能，包括激活和未激活状态
    /// </summary>
    /// <param name="includeInactive">是否包含未激活的子对象</param>
    [TestCase(true)]
    [TestCase(false)]
    public void GetChildren(bool includeInactive)
    {
        // Arrange
        childObject.SetActive(!includeInactive);
        var expectedCount = includeInactive ? 1 : (childObject.activeSelf ? 1 : 0);

        // Act
        var children = rootObject.GetChildren(includeInactive);

        // Assert - 验证子对象数量和引用正确性
        Assert.AreEqual(expectedCount, children.Length, $"子对象数量应为 {expectedCount}");
        if (expectedCount > 0)
            Assert.AreEqual(childObject.transform, children[0], "第一个子对象应为 Child");
    }

    /// <summary>
    /// 测试设置局部缩放功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="x">X轴缩放值</param>
    /// <param name="y">Y轴缩放值</param>
    /// <param name="z">Z轴缩放值</param>
    [TestCase(null, 1, 1, 1)]
    [TestCase("Child", 2, 2, 2)]
    [TestCase("Child/GrandChild", 0.5f, 0.5f, 0.5f)]
    [TestCase(null, 0, 0, 0)]
    public void SetLocalScale(string path, float x, float y, float z)
    {
        // Arrange
        var expectedScale = new Vector3(x, y, z);
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        if (path != null)
            rootObject.SetLocalScale(path, expectedScale);
        else
            target.SetLocalScale(expectedScale);

        // Assert - 验证对象的局部缩放值是否正确设置
        Assert.AreEqual(expectedScale, target.transform.localScale, $"对象的局部缩放应为 ({x}, {y}, {z})");
    }

    /// <summary>
    /// 测试设置世界坐标位置功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="x">X轴位置</param>
    /// <param name="y">Y轴位置</param>
    /// <param name="z">Z轴位置</param>
    [TestCase(null, 1, 2, 3)]
    [TestCase("Child", -1, -2, -3)]
    [TestCase("Child/GrandChild", 100, 200, 300)]
    [TestCase(null, 0, 0, 0)]
    public void SetPosition(string path, float x, float y, float z)
    {
        // Arrange
        var expectedPosition = new Vector3(x, y, z);
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        if (path != null)
            rootObject.SetPosition(path, expectedPosition);
        else
            target.SetPosition(expectedPosition);

        // Assert - 验证对象的世界坐标位置是否正确设置
        Assert.AreEqual(expectedPosition, target.transform.position, $"对象的世界坐标位置应为 ({x}, {y}, {z})");
    }

    /// <summary>
    /// 测试设置局部坐标位置功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="x">X轴位置</param>
    /// <param name="y">Y轴位置</param>
    /// <param name="z">Z轴位置</param>
    [TestCase(null, 1, 2, 3)]
    [TestCase("Child", -1, -2, -3)]
    [TestCase("Child/GrandChild", 100, 200, 300)]
    [TestCase(null, 0, 0, 0)]
    public void SetLocalPosition(string path, float x, float y, float z)
    {
        // Arrange
        var expectedPosition = new Vector3(x, y, z);
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        if (path != null)
            rootObject.SetLocalPosition(path, expectedPosition);
        else
            target.SetLocalPosition(expectedPosition);

        // Assert - 验证对象的局部坐标位置是否正确设置
        Assert.AreEqual(expectedPosition, target.transform.localPosition, $"对象的局部坐标位置应为 ({x}, {y}, {z})");
    }

    /// <summary>
    /// 测试设置世界旋转角度功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="x">X轴旋转角度</param>
    /// <param name="y">Y轴旋转角度</param>
    /// <param name="z">Z轴旋转角度</param>
    [TestCase(null, 0, 0, 0)]
    [TestCase("Child", 30, 45, 60)]
    public void SetRotation(string path, float x, float y, float z)
    {
        // Arrange
        var expectedRotation = new Vector3(x, y, z);
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        if (path != null)
            rootObject.SetRotation(path, expectedRotation);
        else
            target.SetRotation(expectedRotation);

        var actualRotation = target.transform.rotation.eulerAngles;

        // Assert - 验证对象的世界旋转角度是否在误差范围内正确设置
        Assert.That(actualRotation.x, Is.EqualTo(expectedRotation.x).Within(0.01f), $"X轴旋转角度应为 {x} ± 0.01");
        Assert.That(actualRotation.y, Is.EqualTo(expectedRotation.y).Within(0.01f), $"Y轴旋转角度应为 {y} ± 0.01");
        Assert.That(actualRotation.z, Is.EqualTo(expectedRotation.z).Within(0.01f), $"Z轴旋转角度应为 {z} ± 0.01");
    }

    /// <summary>
    /// 测试设置局部旋转角度功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="x">X轴旋转角度</param>
    /// <param name="y">Y轴旋转角度</param>
    /// <param name="z">Z轴旋转角度</param>
    [TestCase(null, 0, 0, 0)]
    [TestCase("Child", 30, 45, 60)]
    public void SetLocalRotation(string path, float x, float y, float z)
    {
        // Arrange
        var expectedRotation = new Vector3(x, y, z);
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        if (path != null)
            rootObject.SetLocalRotation(path, expectedRotation);
        else
            target.SetLocalRotation(expectedRotation);

        var actualRotation = target.transform.localRotation.eulerAngles;

        // Assert - 验证对象的局部旋转角度是否在误差范围内正确设置
        Assert.That(actualRotation.x, Is.EqualTo(expectedRotation.x).Within(0.01f), $"局部X轴旋转角度应为 {x} ± 0.01");
        Assert.That(actualRotation.y, Is.EqualTo(expectedRotation.y).Within(0.01f), $"局部Y轴旋转角度应为 {y} ± 0.01");
        Assert.That(actualRotation.z, Is.EqualTo(expectedRotation.z).Within(0.01f), $"局部Z轴旋转角度应为 {z} ± 0.01");
    }

    /// <summary>
    /// 测试设置父对象功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="worldPositionStays">是否保持世界坐标位置不变</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void SetParent(string path, bool worldPositionStays)
    {
        // Arrange
        var newParent = new GameObject("NewParent");
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;
        var originalPosition = new Vector3(1, 2, 3);
        target.transform.position = originalPosition;

        try
        {
            // Act
            target.SetParent(newParent, worldPositionStays);

            // Assert - 验证父子关系和位置保持
            Assert.AreEqual(newParent.transform, target.transform.parent, "目标对象的父对象应为新创建的父对象");
            if (worldPositionStays) Assert.That(Vector3.Distance(target.transform.position, originalPosition), Is.LessThan(0.001f), "当worldPositionStays为true时，对象的世界坐标位置应保持不变");
        }
        finally
        {
            Object.DestroyImmediate(newParent);
        }
    }

    /// <summary>
    /// 测试销毁游戏对象功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="immediate">是否立即销毁</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void DestroyGameObject(string path, bool immediate)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        if (path != null)
            rootObject.DestroyGameObject(path, immediate);
        else
            target.DestroyGameObject(immediate);

        // Assert - 验证对象是否被正确销毁
        if (immediate)
            Assert.IsTrue(target == null, "当immediate为true时，对象应被立即销毁");
    }

    /// <summary>
    /// 测试克隆游戏对象功能
    /// </summary>
    [Test]
    public void CloneGameObject()
    {
        // Arrange
        var originalName = rootObject.name;

        // Act
        var clone = rootObject.CloneGameObject();

        try
        {
            // Assert - 验证克隆对象的属性
            Assert.IsNotNull(clone, "克隆对象不应为空");
            Assert.AreEqual(originalName, clone.name, "克隆对象应保持原对象的名称");
            Assert.AreNotEqual(rootObject, clone, "克隆对象应是一个新的实例");
        }
        finally
        {
            Object.DestroyImmediate(clone);
        }
    }

    /// <summary>
    /// 测试设置层级功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="layerName">目标层级名称</param>
    [TestCase(null, "UI")]
    [TestCase("Child", "UI")]
    public void SetLayer(string path, string layerName)
    {
        // Arrange
        var expectedLayer = LayerMask.NameToLayer(layerName);
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        if (path != null)
            rootObject.SetLayer(path, layerName);
        else
            target.SetLayer(layerName);

        // Assert - 验证对象的层级是否正确设置
        Assert.AreEqual(expectedLayer, target.layer, $"对象的层级应设置为 {layerName}");
    }

    /// <summary>
    /// 测试设置对象激活状态功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="active">目标激活状态</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void SetGameObjectActive(string path, bool active)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        if (path != null) rootObject.SetGameObjectActive(path, active);
        else target.SetGameObjectActive(active);

        // Assert - 验证对象的激活状态是否正确设置
        Assert.AreEqual(active, target.activeSelf, $"对象的激活状态应为 {active}");
    }

    /// <summary>
    /// 测试确保子对象数量功能
    /// </summary>
    /// <param name="count">期望的子对象数量</param>
    /// <param name="active">子对象的目标激活状态</param>
    [TestCase(5, true)]
    [TestCase(3, false)]
    public void EnsureChild(int count, bool active)
    {
        // Arrange
        // 先创建一个模板子对象并设置其激活状态
        var template = new GameObject("Template");
        template.transform.SetParent(rootObject.transform);

        try
        {
            // Act
            rootObject.transform.EnsureChild(count, active);

            // Assert - 验证子对象数量和激活状态
            Assert.AreEqual(count, rootObject.transform.childCount, $"子对象数量应为 {count}");
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(active, rootObject.transform.GetChild(i).gameObject.activeSelf, $"第 {i} 个子对象的激活状态应为 {active}");
            }
        }
        finally
        {
            Object.DestroyImmediate(template);
        }
    }

    /// <summary>
    /// 测试遍历子对象功能
    /// </summary>
    [Test]
    public void EachChild()
    {
        // Arrange
        int processedCount = 0;
        void CountChildren(int index, Transform child) => processedCount++;

        // Act
        rootObject.transform.EachChild(CountChildren);

        // Assert - 验证是否遍历了所有子对象
        Assert.AreEqual(rootObject.transform.childCount, processedCount, "处理的子对象数量应等于实际子对象数量");
    }

    /// <summary>
    /// 测试显示所有子对象功能
    /// </summary>
    [Test]
    public void ShowChild()
    {
        // Arrange
        childObject.SetActive(false);

        // Act
        rootObject.transform.ShowChild();

        // Assert - 验证子对象是否被正确显示
        Assert.IsTrue(childObject.activeSelf, "子对象应被设置为显示状态");
    }

    /// <summary>
    /// 测试隐藏所有子对象功能
    /// </summary>
    [Test]
    public void HideChild()
    {
        // Act
        rootObject.transform.HideChild();

        // Assert - 验证子对象是否被正确隐藏
        Assert.IsFalse(childObject.activeSelf, "子对象应被设置为隐藏状态");
    }
    #endregion

    #region 组件封装测试
    /// <summary>
    /// 测试获取父对象中的组件功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="includeInactive">是否包含未激活的对象</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void GetComponentInParent(string path, bool includeInactive)
    {
        // Arrange
        var collider = rootObject.AddComponent<BoxCollider>();
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        var result = target.GetComponentInParent(typeof(BoxCollider), includeInactive);

        // Assert - 验证获取的父级组件是否正确
        Assert.AreEqual(collider, result, "获取的父级组件应与根对象上添加的组件相同");
    }

    /// <summary>
    /// 测试获取当前对象组件功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="attachIfMissing">组件不存在时是否添加</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void GetComponent(string path, bool attachIfMissing)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        var result = target.GetComponent(typeof(BoxCollider), attachIfMissing);

        // Assert - 验证组件获取或添加结果
        if (attachIfMissing)
            Assert.IsNotNull(result, "当attachIfMissing为true时，应返回一个有效的组件");
        else
            Assert.IsNull(result, "当attachIfMissing为false时，应返回null");
    }

    /// <summary>
    /// 测试获取子对象中的组件功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="includeInactive">是否包含未激活的对象</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void GetComponentInChildren(string path, bool includeInactive)
    {
        // Arrange
        var collider = childObject.AddComponent<BoxCollider>();
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;
        childObject.SetActive(!includeInactive);

        // Act
        var result = target.GetComponentInChildren(typeof(BoxCollider), includeInactive);

        // Assert - 验证获取的子对象组件是否正确
        if (includeInactive || childObject.activeSelf)
            Assert.AreEqual(collider, result, "获取的子对象组件应与Child对象上添加的组件相同");
        else
            Assert.IsNull(result, "当子对象未激活且不包含未激活对象时，应返回null");
    }

    /// <summary>
    /// 测试获取父对象中的所有组件功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="includeInactive">是否包含未激活的对象</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void GetComponentsInParent(string path, bool includeInactive)
    {
        // Arrange
        var collider1 = rootObject.AddComponent<BoxCollider>();
        var collider2 = rootObject.AddComponent<BoxCollider>();
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        var components = target.GetComponentsInParent(typeof(BoxCollider), includeInactive);

        // Assert - 验证获取的父级组件数组
        Assert.AreEqual(2, components.Length, "应获取到两个BoxCollider组件");
        Assert.Contains(collider1, components, "组件数组中应包含第一个BoxCollider");
        Assert.Contains(collider2, components, "组件数组中应包含第二个BoxCollider");
    }

    /// <summary>
    /// 测试获取当前对象的所有组件功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    [TestCase(null)]
    [TestCase("Child")]
    public void GetComponents(string path)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;
        var collider1 = target.AddComponent<BoxCollider>();
        var collider2 = target.AddComponent<BoxCollider>();

        // Act
        var components = target.GetComponents(typeof(BoxCollider));

        // Assert - 验证获取的组件数组
        Assert.AreEqual(2, components.Length, "应获取到两个BoxCollider组件");
        Assert.Contains(collider1, components, "组件数组中应包含第一个BoxCollider");
        Assert.Contains(collider2, components, "组件数组中应包含第二个BoxCollider");
    }

    /// <summary>
    /// 测试获取子对象中的所有组件功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="includeInactive">是否包含未激活的对象</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void GetComponentsInChildren(string path, bool includeInactive)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;
        var collider1 = childObject.AddComponent<BoxCollider>();
        var collider2 = grandChildObject.AddComponent<BoxCollider>();

        // 设置对象的激活状态
        if (!includeInactive)
        {
            // 当不包含非激活对象时，将grandChild设为非激活
            grandChildObject.SetActive(false);
        }

        // Act
        var components = target.GetComponentsInChildren(typeof(BoxCollider), includeInactive);

        // Assert - 验证获取的子对象组件数组
        if (includeInactive)
        {
            // 包含非激活对象时，应该找到所有组件
            Assert.AreEqual(2, components.Length, "应获取到所有子对象的BoxCollider组件");
            Assert.That(components.Contains(collider1), "组件数组中应包含Child对象的BoxCollider");
            Assert.That(components.Contains(collider2), "组件数组中应包含GrandChild对象的BoxCollider");
        }
        else
        {
            // 不包含非激活对象时，只能找到激活对象上的组件
            Assert.AreEqual(1, components.Length, "应只获取到激活子对象的BoxCollider组件");
            Assert.That(components.Contains(collider1), "组件数组中应包含Child对象的BoxCollider");
            Assert.That(!components.Contains(collider2), "组件数组中不应包含非激活的GrandChild对象的BoxCollider");
        }
    }

    /// <summary>
    /// 测试添加组件功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    [TestCase(null)]
    [TestCase("Child")]
    public void AddComponent(string path)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;

        // Act
        var result = target.AddComponent(typeof(BoxCollider));

        // Assert - 验证组件添加结果
        Assert.IsNotNull(result, "添加的组件不应为空");
        Assert.IsTrue(result is BoxCollider, "添加的组件类型应为BoxCollider");
        Assert.AreEqual(target.GetComponent<BoxCollider>(), result, "添加的组件应能通过GetComponent获取");
    }

    /// <summary>
    /// 测试移除组件功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="immediate">是否立即移除</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void RemoveComponent(string path, bool immediate)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;
        target.AddComponent<BoxCollider>();

        // Act
        target.RemoveComponent(typeof(BoxCollider), immediate);

        // Assert - 验证组件移除结果
        if (immediate)
            Assert.IsNull(target.GetComponent<BoxCollider>(), "当immediate为true时，组件应被立即移除");
    }

    /// <summary>
    /// 测试设置组件启用状态功能
    /// </summary>
    /// <param name="path">目标对象的路径</param>
    /// <param name="enabled">目标启用状态</param>
    [TestCase(null, true)]
    [TestCase("Child", false)]
    public void SetComponentEnabled(string path, bool enabled)
    {
        // Arrange
        var target = path != null ? rootObject.GetTransform(path).gameObject : rootObject;
        var behaviour = target.AddComponent<AudioListener>();

        // Act
        target.SetComponentEnabled(typeof(AudioListener), enabled);

        // Assert - 验证组件启用状态
        Assert.AreEqual(enabled, behaviour.enabled, $"组件的启用状态应为 {enabled}");
    }
    #endregion

    #region 快速索引测试
    /// <summary>
    /// 用于测试的可索引组件
    /// </summary>
    class TestIndexable : MonoBehaviour, XComp.IIndexable
    {
        private readonly Dictionary<string, GameObject> children = new();

        public void AddIndexChild(string name, GameObject child)
        {
            children[name] = child;
        }

        public object Index(string name, System.Type type = null)
        {
            if (children.TryGetValue(name, out var child))
            {
                if (type != null)
                    return child.GetComponent(type);
                return child.transform;
            }
            return null;
        }
    }

    /// <summary>
    /// 测试对象索引功能
    /// </summary>
    /// <param name="name">目标对象名称</param>
    /// <param name="type">目标组件类型</param>
    [TestCase("Child", null)]
    [TestCase("CustomChild", typeof(BoxCollider))]
    public void Index(string name, System.Type type)
    {
        // Arrange
        var indexable = rootObject.AddComponent<TestIndexable>();
        var customChild = new GameObject(name);
        var expectedComponent = type != null ? customChild.AddComponent(type) : null;
        try
        {
            indexable.AddIndexChild(name, customChild);

            // Act
            var result = rootObject.Index(name, type);

            // Assert - 验证索引结果
            Assert.IsNotNull(result, "索引结果不应为空");
            if (type != null)
                Assert.AreEqual(expectedComponent, result, "当指定类型时，应返回对应的组件");
            else
                Assert.AreEqual(customChild.transform, result, "当未指定类型时，应返回Transform组件");
        }
        finally
        {
            Object.DestroyImmediate(customChild);
        }
    }
    #endregion
}
#endif
