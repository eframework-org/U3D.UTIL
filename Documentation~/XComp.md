# XComp

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.util)](https://www.npmjs.com/package/org.eframework.u3d.util)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.UTIL)

XComp 提供了一组 Unity 游戏对象和组件操作的扩展工具集，用于简化节点操作和组件管理。

## 功能特性

- 支持节点操作：提供完整的节点查找、创建、删除、变换等操作
- 支持组件管理：实现组件的获取、添加、删除和状态控制
- 支持快速索引：提供高效的节点和组件检索机制

## 使用手册

### 1. 节点操作

#### 1.1 获取 Transform
```csharp
// 从不同类型对象获取 Transform
Transform trans = gameObject.GetTransform();
Transform childTrans = gameObject.GetTransform("Child/SubChild");

// 获取子节点数组
Transform[] children = parentObj.GetChildren(includeInactive: true);
```

#### 1.2 变换操作
```csharp
// 设置世界坐标
gameObject.SetPosition(new Vector3(1, 1, 1));

// 设置局部坐标
gameObject.SetLocalPosition(new Vector3(0, 0, 0));

// 设置世界旋转
gameObject.SetRotation(new Vector3(0, 90, 0));

// 设置局部缩放
gameObject.SetLocalScale(new Vector3(2, 2, 2));
```

#### 1.3 层级管理
```csharp
// 设置父子关系
childObj.SetParent(parentObj, worldPositionStays: true);

// 设置层级
gameObject.SetLayer("UI");

// 设置激活状态
gameObject.SetActiveState(true);
```

#### 1.4 对象操作
```csharp
// 克隆对象
GameObject clone = sourceObj.CloneGO();

// 销毁对象
gameObject.DestroyGO(immediate: false);

// 添加子对象
Transform child = parentTrans.AddChild(prefab, layer: 5);
```

### 2. 组件管理

#### 2.1 获取组件
```csharp
// 获取当前节点组件
var collider = gameObject.GetComponent(typeof(BoxCollider));

// 获取父节点组件
var canvas = gameObject.GetComponentInParent(typeof(Canvas));

// 获取子节点组件
var renderers = gameObject.GetComponentsInChildren(typeof(MeshRenderer));
```

#### 2.2 组件操作
```csharp
// 添加组件
var rigidbody = gameObject.AddComponent(typeof(Rigidbody));

// 移除组件
gameObject.RemoveComponent(typeof(BoxCollider));

// 设置组件状态
gameObject.SetComponentEnabled(typeof(MonoBehaviour), enabled: false);
```

### 3. 快速索引

#### 3.1 基础索引
```csharp
// 通过名称查找对象
var targetObj = gameObject.Index("TargetNode");

// 通过名称和类型查找组件
var collider = gameObject.Index<BoxCollider>("ColliderNode");
```

#### 3.2 自定义索引
```csharp
// 实现 IIndexable 接口
public class CustomIndexer : MonoBehaviour, IIndexable
{
    public object Index(string name, System.Type type = null)
    {
        // 自定义索引逻辑
        return null;
    }
}
```

## 常见问题

### 1. Transform 获取失败
- 检查传入对象是否为 null
- 确认对象类型是否为 Transform、GameObject 或 Component
- 检查对象是否已被销毁

### 2. 组件操作异常
- 确保组件类型继承自 UnityEngine.Object
- 检查目标对象是否处于激活状态
- 验证组件类型是否可以被添加到目标对象

### 3. 索引查找返回 null
- 确认目标对象名称是否正确
- 检查是否在正确的层级范围内查找
- 验证目标组件类型是否正确

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md) 