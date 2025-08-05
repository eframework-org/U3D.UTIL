// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using UnityEngine;

namespace EFramework.Utility
{
    /// <summary>
    /// XComp 提供了一组 Unity 游戏对象和组件操作的扩展工具集，用于简化节点操作和组件管理。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 支持节点操作：提供完整的节点查找、创建、删除、变换等操作
    /// - 支持组件管理：实现组件的获取、添加、删除和状态控制
    /// - 支持快速索引：提供高效的节点和组件检索机制
    /// 
    /// 使用手册
    /// 1. 节点操作
    /// 
    ///     1.1 获取 Transform
    ///     // 从不同类型对象获取 Transform
    ///     var transform = gameObject.GetTransform();
    ///     var childTransforms = gameObject.GetTransform("Child/SubChild");
    ///     
    ///     1.2 变换操作
    ///     // 设置世界坐标和旋转
    ///     gameObject.SetPosition(new Vector3(1, 1, 1));
    ///     gameObject.SetRotation(new Vector3(0, 90, 0));
    /// 
    /// 2. 组件管理
    /// 
    ///     2.1 获取组件
    ///     // 获取当前节点和父节点组件
    ///     var collider = gameObject.GetComponent(typeof(BoxCollider));
    ///     var canvas = gameObject.GetComponentInParent(typeof(Canvas));
    ///     
    ///     2.2 组件操作
    ///     // 添加和控制组件
    ///     var rigidbody = gameObject.AddComponent(typeof(Rigidbody));
    ///     gameObject.SetComponentEnabled(typeof(MonoBehaviour), false);
    /// 
    /// 3. 快速索引
    /// 
    ///     3.1 基础索引
    ///     // 通过名称和类型查找
    ///     var targetObj = gameObject.Index("TargetNode");
    ///     var collider = gameObject.Index&lt;BoxCollider&gt;("ColliderNode");
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public static class XComp
    {
        #region 节点操作
        /// <summary>
        /// ForeachChildHandler 是遍历子节点时的回调委托。
        /// </summary>
        /// <param name="index">子节点的索引序号</param>
        /// <param name="child">子节点的 Transform 组件</param>
        public delegate void ForeachChildHandler(int index, Transform child);

        /// <summary>
        /// GetTransform 获取对象的 Transform 组件。支持从 Transform、GameObject 和 Component 类型获取。
        /// </summary>
        /// <param name="rootObj">目标对象，可以是 Transform、GameObject 或其他 Component</param>
        /// <returns>目标对象的 Transform 组件，若目标对象为空或类型不支持则返回 null</returns>
        public static Transform GetTransform(this Object rootObj)
        {
            if (rootObj is Transform)
            {
                if (rootObj == null) return null;
                return rootObj as Transform;
            }
            else if (rootObj is GameObject)
            {
                if (rootObj == null) return null;
                var obj = rootObj as GameObject;
                return obj.transform;
            }
            else if (rootObj is Component)
            {
                if (rootObj == null) return null;
                var obj = rootObj as Component;
                return obj.transform;
            }
            return null;
        }

        /// <summary>
        /// GetTransform 根据路径获取对象的 Transform 组件。
        /// </summary>
        /// <param name="parentObj">父级对象，可以是 Transform、GameObject 或其他 Component</param>
        /// <param name="path">目标对象的相对路径，支持层级查找，如 "Child/SubChild"</param>
        /// <returns>目标路径对应的 Transform 组件，若路径无效则返回 null</returns>
        public static Transform GetTransform(this Object parentObj, string path)
        {
            var parent = GetTransform(parentObj);
            if (string.IsNullOrEmpty(path)) return parent;
            else if (parent) return parent.Find(path);
            else return null;
        }

        /// <summary>
        /// GetChildren 获取所有子节点的 Transform 组件。
        /// </summary>
        /// <param name="rootObj">父级对象</param>
        /// <param name="includeInactive">是否包含未激活的子节点，默认为 true</param>
        /// <returns>子节点的 Transform 组件数组</returns>
        public static Transform[] GetChildren(this Object rootObj, bool includeInactive = true) { return GetChildren(rootObj, null, includeInactive); }

        /// <summary>
        /// GetChildren 根据路径获取所有子节点的 Transform 组件。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">目标对象的相对路径</param>
        /// <param name="includeInactive">是否包含未激活的子节点，默认为 true</param>
        /// <returns>指定路径下子节点的 Transform 组件数组</returns>
        public static Transform[] GetChildren(this Object parentObj, string path, bool includeInactive = true)
        {
            var root = GetTransform(parentObj, path);
            var rets = new List<Transform>();
            if (includeInactive) for (int i = 0; i < root.childCount; i++) { rets.Add(root.GetChild(i)); }
            else for (int i = 0; i < root.childCount; i++) { if (root.GetChild(i).gameObject.activeInHierarchy) rets.Add(root.GetChild(i)); }
            return rets.ToArray();
        }

        /// <summary>
        /// SetPosition 设置对象的世界坐标位置。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="position">世界坐标系中的目标位置</param>
        public static void SetPosition(this Object rootObj, Vector3 position)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject) root.position = position;
        }

        /// <summary>
        /// SetPosition 根据路径设置对象的世界坐标位置。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">目标对象的相对路径</param>
        /// <param name="position">世界坐标系中的目标位置</param>
        public static void SetPosition(this Object parentObj, string path, Vector3 position)
        {
            var parent = GetTransform(parentObj);
            if (parent && parent.gameObject)
            {
                var root = parent.Find(path);
                if (root && root.gameObject) root.position = position;
            }
        }

        /// <summary>
        /// SetLocalPosition 设置对象的局部坐标位置（相对于父级对象）。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="position">局部坐标系中的目标位置</param>
        public static void SetLocalPosition(this Object rootObj, Vector3 position)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject) root.localPosition = position;
        }

        /// <summary>
        /// SetLocalPosition 根据路径设置对象的局部坐标位置。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">目标对象的相对路径</param>
        /// <param name="position">局部坐标系中的目标位置</param>
        public static void SetLocalPosition(this Object parentObj, string path, Vector3 position)
        {
            var parent = GetTransform(parentObj);
            if (parent && parent.gameObject)
            {
                var root = parent.Find(path);
                if (root && root.gameObject) root.localPosition = position;
            }
        }

        /// <summary>
        /// SetRotation 设置对象的世界旋转角度。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="eulerAngles">世界坐标系中的欧拉角旋转值（度）</param>
        public static void SetRotation(this Object rootObj, Vector3 eulerAngles)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject) root.rotation = Quaternion.Euler(eulerAngles);
        }

        /// <summary>
        /// SetRotation 根据路径设置对象的世界旋转角度。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">目标对象的相对路径</param>
        /// <param name="eulerAngles">世界坐标系中的欧拉角旋转值（度）</param>
        public static void SetRotation(this Object parentObj, string path, Vector3 eulerAngles)
        {
            var parent = GetTransform(parentObj);
            if (parent && parent.gameObject)
            {
                var root = parent.Find(path);
                if (root && root.gameObject) root.rotation = Quaternion.Euler(eulerAngles);
            }
        }

        /// <summary>
        /// SetLocalRotation 设置对象的局部旋转角度（相对于父级对象）。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="eulerAngles">局部坐标系中的欧拉角旋转值（度）</param>
        public static void SetLocalRotation(this Object rootObj, Vector3 eulerAngles)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject) root.localRotation = Quaternion.Euler(eulerAngles);
        }

        /// <summary>
        /// SetLocalRotation 根据路径设置对象的局部旋转角度。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">目标对象的相对路径</param>
        /// <param name="eulerAngles">局部坐标系中的欧拉角旋转值（度）</param>
        public static void SetLocalRotation(this Object parentObj, string path, Vector3 eulerAngles)
        {
            var parent = GetTransform(parentObj);
            if (parent && parent.gameObject)
            {
                var root = parent.Find(path);
                if (root && root.gameObject) root.localRotation = Quaternion.Euler(eulerAngles);
            }
        }

        /// <summary>
        /// SetLocalScale 设置对象的局部缩放值。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="scale">局部坐标系中的缩放值，xyz 分别对应三个轴向的缩放比例</param>
        public static void SetLocalScale(this Object rootObj, Vector3 scale)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject) root.localScale = scale;
        }

        /// <summary>
        /// SetLocalScale 根据路径设置对象的局部缩放值。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">目标对象的相对路径</param>
        /// <param name="scale">局部坐标系中的缩放值，xyz 分别对应三个轴向的缩放比例</param>
        public static void SetLocalScale(this Object parentObj, string path, Vector3 scale)
        {
            var parent = GetTransform(parentObj);
            if (parent && parent.gameObject)
            {
                var root = parent.Find(path);
                if (root && root.gameObject) root.localScale = scale;
            }
        }

        /// <summary>
        /// SetParent 设置对象的父级对象。
        /// </summary>
        /// <param name="childObj">子对象</param>
        /// <param name="parentObj">父对象</param>
        /// <param name="worldPositionStays">是否保持世界坐标位置不变，默认为 true</param>
        /// <returns>父对象的 Transform 组件</returns>
        public static Transform SetParent(this Object childObj, Object parentObj, bool worldPositionStays = true)
        {
            var child = GetTransform(childObj);
            var parent = GetTransform(parentObj);
            if (child && child.gameObject)
            {
                child.SetParent(parent, worldPositionStays);
            }
            return parent;
        }

        /// <summary>
        /// SetParent 根据路径设置对象的父级对象。
        /// </summary>
        /// <param name="childObj">子对象</param>
        /// <param name="rootObj">根对象</param>
        /// <param name="parentPath">父对象的相对路径</param>
        /// <param name="worldPositionStays">是否保持世界坐标位置不变，默认为 true</param>
        /// <returns>父对象的 Transform 组件</returns>
        public static Transform SetParent(this Object childObj, Object rootObj, string parentPath, bool worldPositionStays = true)
        {
            var child = GetTransform(childObj);
            var root = GetTransform(rootObj);
            Transform parent = null;
            if (root && root.gameObject)
            {
                parent = root.Find(parentPath);
            }
            if (child && child.gameObject && parent && parent.gameObject)
            {
                child.SetParent(parent, worldPositionStays);
            }
            return parent;
        }

        /// <summary>
        /// DestroyGameObject 销毁游戏对象。
        /// </summary>
        /// <param name="rootObj">要销毁的对象</param>
        public static void DestroyGameObject(this Object rootObj) { DestroyGameObject(rootObj, false); }

        /// <summary>
        /// DestroyGameObject 销毁游戏对象。
        /// </summary>
        /// <param name="rootObj">要销毁的对象</param>
        /// <param name="immediate">是否立即销毁，若为 false 则在当前帧结束时销毁</param>
        public static void DestroyGameObject(this Object rootObj, bool immediate)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject)
            {
                if (immediate) Object.DestroyImmediate(root.gameObject);
                else Object.Destroy(root.gameObject);
            }
        }

        /// <summary>
        /// DestroyGameObject 根据路径销毁游戏对象。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">要销毁对象的相对路径</param>
        public static void DestroyGameObject(this Object parentObj, string path) { DestroyGameObject(parentObj, path, false); }

        /// <summary>
        /// DestroyGameObject 根据路径销毁游戏对象。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">要销毁对象的相对路径</param>
        /// <param name="immediate">是否立即销毁，若为 false 则在当前帧结束时销毁</param>
        public static void DestroyGameObject(this Object parentObj, string path, bool immediate)
        {
            var root = GetTransform(parentObj, path);
            if (root && root.gameObject)
            {
                if (immediate) Object.DestroyImmediate(root.gameObject);
                else Object.Destroy(root.gameObject);
            }
        }

        /// <summary>
        /// CloneGameObject 克隆游戏对象。
        /// </summary>
        /// <param name="rootObj">要克隆的对象</param>
        /// <returns>克隆出的新游戏对象，克隆失败时返回 null</returns>
        /// <remarks>克隆后的对象会保持原对象的名称（不会添加 "(Clone)" 后缀）</remarks>
        public static GameObject CloneGameObject(this Object rootObj)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject)
            {
                string goName = root.name;
                var go = Object.Instantiate(root.gameObject);
                if (go) go.name = goName;
                return go;
            }
            return null;
        }

        /// <summary>
        /// SetLayer 设置游戏对象及其所有子对象的层级。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="layerName">目标层级的名称</param>
        public static void SetLayer(this Object rootObj, string layerName)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject)
            {
                int layer = LayerMask.NameToLayer(layerName);
                SetLayer(root.gameObject, layer);
            }
        }

        /// <summary>
        /// SetLayer 根据路径设置游戏对象及其所有子对象的层级。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">目标对象的相对路径</param>
        /// <param name="layerName">目标层级的名称</param>
        public static void SetLayer(this Object parentObj, string path, string layerName)
        {
            var parent = GetTransform(parentObj);
            if (parent && parent.gameObject)
            {
                var root = parent.Find(path);
                if (root && root.gameObject)
                {
                    int layer = LayerMask.NameToLayer(layerName);
                    SetLayer(root.gameObject, layer);
                }
            }
        }

        /// <summary>
        /// SetLayer 递归设置游戏对象及其所有子对象的层级。
        /// </summary>
        /// <param name="gameObject">目标游戏对象</param>
        /// <param name="layer">目标层级的索引值</param>
        public static void SetLayer(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            var t = gameObject.transform;
            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                var child = t.GetChild(i);
                SetLayer(child.gameObject, layer);
            }
        }

        /// <summary>
        /// SetGameObjectActive 设置游戏对象的激活状态。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="active">是否激活</param>
        public static void SetGameObjectActive(this Object rootObj, bool active)
        {
            var root = GetTransform(rootObj);
            if (root && root.gameObject) root.gameObject.SetActive(active);
        }

        /// <summary>
        /// SetGameObjectActive 根据路径设置游戏对象的激活状态。
        /// </summary>
        /// <param name="parentObj">父级对象</param>
        /// <param name="path">目标对象的相对路径</param>
        /// <param name="active">是否激活</param>
        public static void SetGameObjectActive(this Object parentObj, string path, bool active)
        {
            var parent = GetTransform(parentObj);
            if (parent != null) SetGameObjectActive(parent.Find(path), active);
        }

        /// <summary>
        /// AddChild 添加预制体实例作为子对象。
        /// </summary>
        /// <param name="parent">父级 Transform</param>
        /// <param name="prefab">预制体对象</param>
        /// <param name="layer">目标层级索引，-1 表示使用父对象的层级</param>
        /// <returns>添加的子对象实例，添加失败时返回 null</returns>
        /// <remarks>
        /// 添加后的子对象会：
        /// - 重置局部位置为零点
        /// - 重置局部旋转为零度
        /// - 重置局部缩放为 1
        /// - 继承父对象的层级（当 layer 为 -1 时）
        /// </remarks>
        public static GameObject AddChild(this Transform parent, GameObject prefab, int layer = -1)
        {
            var go = Object.Instantiate(prefab);
            if (go != null && parent != null)
            {
                var t = go.transform;
                t.SetParent(parent.transform);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                if (layer == -1) go.layer = parent.gameObject.layer;
                else if (layer > -1 && layer < 32) go.layer = layer;
            }
            return go;
        }

        /// <summary>
        /// EnsureChild 确保父对象下有指定数量的子对象，不足时通过复制补足。
        /// </summary>
        /// <param name="parent">父级 Transform</param>
        /// <param name="count">目标子对象数量</param>
        /// <param name="active">子对象的激活状态</param>
        /// <param name="prefab">用于复制的预制体，为 null 时使用第一个子对象作为模板</param>
        /// <remarks>
        /// - 当子对象数量不足时，会自动复制补足到指定数量
        /// - 所有子对象（包括原有的）都会被设置为指定的激活状态
        /// </remarks>
        public static void EnsureChild(this Transform parent, int count, bool active = false, GameObject prefab = null)
        {
            if (parent.childCount < count)
            {
                var newCount = count - parent.childCount;
                if (newCount > 0)
                {
                    if (prefab == null) prefab = parent.GetChild(0).gameObject;
                    for (int i = 0; i < newCount; i++) AddChild(parent, prefab);
                }
            }
            for (int i = 0; i < count; i++)
            {
                parent.GetChild(i).SetGameObjectActive(active);
            }
        }

        /// <summary>
        /// EachChild 遍历所有子对象并执行回调操作。
        /// </summary>
        /// <param name="parent">父级 Transform</param>
        /// <param name="handler">子对象处理回调，提供子对象的索引和 Transform 组件</param>
        public static void EachChild(this Transform parent, ForeachChildHandler handler)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                handler?.Invoke(i, child);
            }
        }

        /// <summary>
        /// ShowChild 显示所有子对象，并可选择执行额外操作。
        /// </summary>
        /// <param name="parent">父级 Transform</param>
        /// <param name="handler">子对象处理回调，在显示后执行</param>
        public static void ShowChild(this Transform parent, ForeachChildHandler handler = null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.gameObject.SetActive(true);
                handler?.Invoke(i, child);
            }
        }

        /// <summary>
        /// HideChild 隐藏所有子对象，并可选择执行额外操作。
        /// </summary>
        /// <param name="parent">父级 Transform</param>
        /// <param name="handler">子对象处理回调，在隐藏后执行</param>
        public static void HideChild(this Transform parent, ForeachChildHandler handler = null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.gameObject.SetActive(false);
                handler?.Invoke(i, child);
            }
        }
        #endregion

        #region 组件管理
        /// <summary>
        /// ClazzUnityObject 是 Unity Object 类型的内部缓存引用。
        /// </summary>
        internal static System.Type ClazzUnityObject = typeof(Object);

        /// <summary>
        /// GetComponentInParent 获取对象父级中的指定类型组件。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="type">组件类型</param>
        /// <param name="includeInactive">是否包含未激活的对象，默认为 false</param>
        /// <returns>找到的组件实例，未找到时返回 null</returns>
        /// <exception cref="System.Exception">当类型参数为 null 或不是 UnityEngine.Object 的子类时抛出</exception>
        public static object GetComponentInParent(this Object rootObj, System.Type type, bool includeInactive = false) { return GetComponentInParent(rootObj, null, type, includeInactive); }

        /// <summary>
        /// GetComponentInParent 从父节点中获取组件。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <param name="includeInactive">包含隐藏的物体</param>
        /// <returns>获取到的组件</returns>
        public static object GetComponentInParent(this Object parentObj, string path, System.Type type, bool includeInactive = false)
        {
            if (type == null) throw new System.Exception("type is null");
            var root = GetTransform(parentObj, path);
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            if (root && root.gameObject) return root.gameObject.GetComponentInParent(type, includeInactive);
            return null;
        }

        /// <summary>
        /// GetComponent 获取对象上的指定类型组件。
        /// </summary>
        /// <param name="rootObj">目标对象</param>
        /// <param name="type">组件类型</param>
        /// <param name="attachIfMissing">当组件不存在时是否自动添加，默认为 false</param>
        /// <returns>找到或新添加的组件实例，未找到且未添加时返回 null</returns>
        /// <exception cref="System.Exception">当类型参数为 null 或不是 UnityEngine.Object 的子类时抛出</exception>
        public static object GetComponent(this Object rootObj, System.Type type, bool attachIfMissing = false) { return GetComponent(rootObj, null, type, attachIfMissing); }

        /// <summary>
        /// GetComponent 从当前节点中获取组件。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <param name="attachIfMissing">若未找到则挂载</param>
        /// <returns>获取到的组件</returns>
        public static object GetComponent(this Object parentObj, string path, System.Type type, bool attachIfMissing = false)
        {
            if (type == null) throw new System.Exception("type is null");
            var root = GetTransform(parentObj, path);
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            if (root && root.gameObject)
            {
                var comp = root.gameObject.GetComponent(type);
                return comp ? comp : attachIfMissing ? root.gameObject.AddComponent(type) : null;
            }
            return null;
        }

        /// <summary>
        /// GetComponentInChildren 从子节点中获取组件。
        /// </summary>
        /// <param name="rootObj">节点</param>
        /// <param name="type">类型</param>
        /// <param name="includeInactive">包含隐藏的物体</param>
        /// <returns>获取到的组件</returns>
        public static object GetComponentInChildren(this Object rootObj, System.Type type, bool includeInactive = false) { return GetComponentInChildren(rootObj, null, type, includeInactive); }

        /// <summary>
        /// GetComponentInChildren 从子节点中获取组件。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <param name="includeInactive">包含隐藏的物体</param>
        /// <returns>获取到的组件</returns>
        public static object GetComponentInChildren(this Object parentObj, string path, System.Type type, bool includeInactive = false)
        {
            if (type == null) throw new System.Exception("type is null");
            var root = GetTransform(parentObj, path);
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            if (root && root.gameObject) return root.gameObject.GetComponentInChildren(type, includeInactive);
            return null;
        }

        /// <summary>
        /// GetComponentsInParent 从父节点中获取组件数组。
        /// </summary>
        /// <param name="rootObj">节点</param>
        /// <param name="type">类型</param>
        /// <param name="includeInactive">包含隐藏的物体</param>
        /// <returns>获取到的组件数组</returns>
        public static object[] GetComponentsInParent(this Object rootObj, System.Type type, bool includeInactive = false) { return GetComponentsInParent(rootObj, null, type, includeInactive); }

        /// <summary>
        /// GetComponentsInParent 从父节点中获取组件数组。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <param name="includeInactive">包含隐藏的物体</param>
        /// <returns>获取到的组件数组</returns>
        public static object[] GetComponentsInParent(this Object parentObj, string path, System.Type type, bool includeInactive = false)
        {
            if (type == null) throw new System.Exception("type is null");
            var root = GetTransform(parentObj, path);
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            System.Array arr;
            if (root && root.gameObject)
            {
                var comps = root.gameObject.GetComponentsInParent(type, includeInactive);
                arr = System.Array.CreateInstance(type, comps.Length);
                for (int i = 0; i < comps.Length; i++)
                {
                    arr.SetValue(comps[i], i);
                }
            }
            else
            {
                arr = System.Array.CreateInstance(type, 0);
            }
            return arr as object[];
        }

        /// <summary>
        /// GetComponents 从当前节点中获取组件数组。
        /// </summary>
        /// <param name="rootObj">节点</param>
        /// <param name="type">类型</param>
        /// <returns>获取到的组件数组</returns>
        public static object[] GetComponents(this Object rootObj, System.Type type) { return GetComponents(rootObj, null, type); }

        /// <summary>
        /// GetComponents 从当前节点中获取组件数组。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <returns>获取到的组件数组</returns>
        public static object[] GetComponents(this Object parentObj, string path, System.Type type)
        {
            if (type == null) throw new System.Exception("type is null");
            var root = GetTransform(parentObj, path);
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            System.Array arr;
            if (root && root.gameObject)
            {
                var comps = root.gameObject.GetComponents(type);
                arr = System.Array.CreateInstance(type, comps.Length);
                for (int i = 0; i < comps.Length; i++)
                {
                    arr.SetValue(comps[i], i);
                }
            }
            else
            {
                arr = System.Array.CreateInstance(type, 0);
            }
            return arr as object[];
        }

        /// <summary>
        /// GetComponentsInChildren 从子节点中获取组件数组。
        /// </summary>
        /// <param name="rootObj">节点</param>
        /// <param name="type">类型</param>
        /// <param name="includeInactive">包含隐藏的物体</param>
        /// <returns>获取到的组件数组</returns>
        public static object[] GetComponentsInChildren(this Object rootObj, System.Type type, bool includeInactive = false) { return GetComponentsInChildren(rootObj, null, type, includeInactive); }

        /// <summary>
        /// GetComponentsInChildren 从子节点中获取组件数组。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <param name="includeInactive">包含隐藏的物体</param>
        /// <returns>获取到的组件数组</returns>
        public static object[] GetComponentsInChildren(this Object parentObj, string path, System.Type type, bool includeInactive = false)
        {
            if (type == null) throw new System.Exception("type is null");
            var root = GetTransform(parentObj, path);
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            System.Array arr;
            if (root && root.gameObject)
            {
                var comps = root.gameObject.GetComponentsInChildren(type, includeInactive);
                arr = System.Array.CreateInstance(type, comps.Length);
                for (int i = 0; i < comps.Length; i++)
                {
                    arr.SetValue(comps[i], i);
                }
            }
            else
            {
                arr = System.Array.CreateInstance(type, 0);
            }
            return arr as object[];
        }

        /// <summary>
        /// AddComponent 添加组件。
        /// </summary>
        /// <param name="rootObj">节点</param>
        /// <param name="type">类型</param>
        /// <returns>添加的组件</returns>
        public static object AddComponent(this Object rootObj, System.Type type) { return AddComponent(rootObj, null, type); }

        /// <summary>
        /// AddComponent 添加组件。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <returns>添加的组件</returns>
        public static object AddComponent(this Object parentObj, string path, System.Type type)
        {
            if (type == null) throw new System.Exception("type is null");
            var root = GetTransform(parentObj, path);
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            if (root && root.gameObject) return root.gameObject.AddComponent(type);
            return null;
        }

        /// <summary>
        /// RemoveComponent 移除组件。
        /// </summary>
        /// <param name="rootObj">节点</param>
        /// <param name="type">类型</param>
        /// <param name="immediate">立即移除</param>
        public static void RemoveComponent(this Object rootObj, System.Type type, bool immediate = false) { RemoveComponent(rootObj, null, type, immediate); }

        /// <summary>
        /// RemoveComponent 移除组件。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <param name="immediate">立即移除</param>
        public static void RemoveComponent(this Object parentObj, string path, System.Type type, bool immediate = false)
        {
            if (type == null) throw new System.Exception("type is null");
            var root = GetTransform(parentObj, path);
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            if (root && root.gameObject)
            {
                var obj = root.gameObject.GetComponent(type);
                if (immediate) Object.DestroyImmediate(obj);
                else Object.Destroy(obj);
            }
        }

        /// <summary>
        /// SetComponentEnabled 设置组件启用状态。
        /// </summary>
        /// <param name="rootObj">节点</param>
        /// <param name="type">类型</param>
        /// <param name="enabled">启用状态</param>
        /// <returns>设置的组件</returns>
        public static object SetComponentEnabled(this Object rootObj, System.Type type, bool enabled) { return SetComponentEnabled(rootObj, null, type, enabled); }

        /// <summary>
        /// SetComponentEnabled 设置组件启用状态。
        /// </summary>
        /// <param name="parentObj">父节点</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">类型</param>
        /// <param name="enabled">启用状态</param>
        /// <returns>设置的组件</returns>
        public static object SetComponentEnabled(this Object parentObj, string path, System.Type type, bool enabled)
        {
            if (type == null) throw new System.Exception("type is null");
            if (!type.IsSubclassOf(ClazzUnityObject)) throw new System.Exception($"type: {type.FullName} is not a sub class of {ClazzUnityObject.FullName}");
            var behaviour = GetComponent(parentObj, path, type) as Behaviour;
            if (behaviour) behaviour.enabled = enabled;
            return behaviour;
        }
        #endregion

        #region 快速索引
        /// <summary>
        /// IIndexable 定义了可索引对象的接口。
        /// </summary>
        public interface IIndexable
        {
            /// <summary>
            /// Index 根据名称和类型查找对象。
            /// </summary>
            /// <param name="name">目标对象名称</param>
            /// <param name="type">可选的组件类型</param>
            /// <returns>找到的对象或组件实例</returns>
            object Index(string name, System.Type type = null);
        }

        /// <summary>
        /// Index 在对象层级中查找指定名称的对象或组件。
        /// </summary>
        /// <param name="obj">起始对象</param>
        /// <param name="name">目标对象名称</param>
        /// <param name="type">可选的组件类型，为 null 时查找 Transform</param>
        /// <returns>找到的对象或组件实例，未找到时返回 null</returns>
        /// <remarks>
        /// 查找规则：
        /// 1. 如果对象实现了 IIndexable 接口，则使用接口方法查找
        /// 2. 如果是 Unity 对象：
        ///    - 不指定类型时，在子对象中递归查找匹配名称的 Transform
        ///    - 指定类型时，在所有子对象中查找匹配名称的指定类型组件
        /// </remarks>
        public static object Index(object obj, string name, System.Type type = null)
        {
            if (obj != null)
            {
                if (obj is IIndexable indexable) return indexable.Index(name, type);
                else if (obj is Object)
                {
                    var root = GetTransform(obj as Object);
                    if (root)
                    {
                        if (type == null)
                        {
                            for (var i = 0; i < root.childCount; i++)
                            {
                                var child = root.GetChild(i);
                                if (child.name == name) return child;
                            }
                            for (var i = 0; i < root.childCount; i++)
                            {
                                var child = root.GetChild(i);
                                var ret = Index(child, name, type);
                                if (ret != null) return ret;
                            }
                        }
                        else
                        {
                            var comps = root.GetComponentsInChildren(type, true);
                            foreach (var comp in comps)
                            {
                                if (comp.name == name) return comp;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Index 在 GameObject 的层级中查找指定名称的对象或组件。
        /// </summary>
        /// <param name="gameObject">起始节点</param>
        /// <param name="name">目标名称</param>
        /// <param name="type">目标类型</param>
        /// <returns>找到的对象或组件实例</returns>
        public static object Index(this GameObject gameObject, string name, System.Type type = null)
        {
            if (gameObject)
            {
                var index = gameObject.GetComponent<IIndexable>();
                if (index != null) return Index(index, name, type);
                else return Index((object)gameObject, name, type);
            }
            return null;
        }

        /// <summary>
        /// Index 在 GameObject 的层级中查找指定名称和类型的组件。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="gameObject">起始节点</param>
        /// <param name="name">目标名称</param>
        /// <returns>找到的组件实例</returns>
        public static T Index<T>(this GameObject gameObject, string name) where T : class { return Index(gameObject, name, typeof(T)) as T; }

        /// <summary>
        /// Index 在 Transform 的层级中查找指定名称的对象或组件。
        /// </summary>
        /// <param name="transform">起始节点</param>
        /// <param name="name">目标名称</param>
        /// <param name="type">目标类型</param>
        /// <returns>找到的对象或组件实例</returns>
        public static object Index(this Transform transform, string name, System.Type type = null) { if (transform) return Index(transform.gameObject, name, type); else return null; }

        /// <summary>
        /// Index 在 Transform 的层级中查找指定名称和类型的组件。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="transform">起始节点</param>
        /// <param name="name">目标名称</param>
        /// <returns>找到的组件实例</returns>
        public static T Index<T>(this Transform transform, string name) where T : class { if (transform) return Index<T>(transform.gameObject, name); else return null; }
        #endregion
    }
}
