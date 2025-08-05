// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EFramework.Utility
{
    /// <summary>
    /// XEnv 是一个环境配置管理工具，支持多平台识别、应用配置管理、路径管理、命令行参数解析和环境变量求值等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 参数解析：支持多种参数形式和缓存管理
    /// - 环境配置：支持应用类型、运行模式、版本等环境配置
    /// - 变量求值：支持 ${Env.Key} 格式的环境变量引用和求值
    /// - 路径管理：提供本地路径和资产路径的统一管理
    /// 
    /// 使用手册
    /// 1. 平台环境
    /// 
    /// 1.1 获取平台类型
    /// 
    ///     // 获取当前运行平台
    ///     var platform = XEnv.Platform;
    ///     if (platform != XEnv.PlatformType.Unknown)
    ///     {
    ///         // 根据平台类型执行相应逻辑
    ///         switch (platform)
    ///         {
    ///             case XEnv.PlatformType.Windows:
    ///                 // Windows 平台特定处理
    ///                 break;
    ///             case XEnv.PlatformType.Android:
    ///                 // Android 平台特定处理
    ///                 break;
    ///         }
    ///     }
    /// 
    /// 2. 应用配置
    /// 
    /// 2.1 获取应用类型
    /// 
    ///     // 获取当前应用类型
    ///     var appType = XEnv.App;
    ///     if (appType == XEnv.AppType.Client)
    ///     {
    ///         // 客户端特定逻辑
    ///     }
    /// 
    /// 3. 路径管理
    /// 
    /// 3.1 获取项目路径
    /// 
    ///     // 获取项目根目录路径
    ///     var projectPath = XEnv.ProjectPath;
    /// 
    /// 4. 设备信息
    /// 
    /// 4.1 获取设备标识
    /// 
    ///     // 获取设备唯一标识符
    ///     var deviceId = XEnv.DeviceID;
    /// 
    /// 5. 命令行参数
    /// 
    /// 5.1 解析命令行参数
    /// 
    ///     // 解析命令行参数
    ///     XEnv.ParseArgs(true, "--config=dev.json", "-debug");
    /// 
    /// 6. 环境变量
    /// 
    /// 6.1 解析环境变量
    /// 
    ///     // 包含环境变量引用的字符串
    ///     var text = "Hello ${Env.UserName}!";
    ///     // 解析环境变量
    ///     var result = text.Eval(XEnv.Vars);
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public static partial class XEnv
    {
        /// <summary>
        /// PlatformType 是平台类型枚举，定义了支持的运行平台类型。
        /// </summary>
        public enum PlatformType
        {
            /// <summary>Unknown 表示未知或不支持的平台。</summary>
            Unknown,

            /// <summary>Windows 表示 Windows 10/11/Server 等操作系统平台。</summary>
            Windows,

            /// <summary>Linux 表示 Ubuntu/CentOS 等操作系统平台。</summary>
            Linux,

            /// <summary>macOS 表示 macOS 操作系统平台。</summary>
            macOS,

            /// <summary>Android 表示 Android 及其衍生的操作系统平台。</summary>
            Android,

            /// <summary>iOS 表示 iPhone 和 iPad 操作系统平台。</summary>
            iOS,

            /// <summary>Browser 表示浏览器平台，主要指 WebGL。</summary>
            Browser
        }

        /// <summary>PlatformUnknown 是未知平台的字符串标识。</summary>
        public static readonly string PlatformUnknown = PlatformType.Unknown.ToString();

        /// <summary>PlatformWindows 是 Windows 10/11/Server 等操作系统平台的字符串标识。</summary>
        public static readonly string PlatformWindows = PlatformType.Windows.ToString();

        /// <summary>PlatformLinux 是 Ubuntu/CentOS 等操作系统平台的字符串标识。</summary>
        public static readonly string PlatformLinux = PlatformType.Linux.ToString();

        /// <summary>PlatformmacOS 是 macOS 操作系统平台的字符串标识。</summary>
        public static readonly string PlatformmacOS = PlatformType.macOS.ToString();

        /// <summary>PlatformAndroid 是 Android 及其衍生的操作系统平台的字符串标识。</summary>
        public static readonly string PlatformAndroid = PlatformType.Android.ToString();

        /// <summary>PlatformiOS 是 iPhone 和 iPad 操作系统平台的字符串标识。</summary>
        public static readonly string PlatformiOS = PlatformType.iOS.ToString();

        /// <summary>PlatformBrowser 是浏览器平台，主要指 WebGL 的字符串标识。</summary>
        public static readonly string PlatformBrowser = PlatformType.Browser.ToString();

#if UNITY_EDITOR
        internal static PlatformType platform = PlatformType.Unknown;
        internal static bool platformGet = false;

        /// <summary>
        /// OnInit 是编辑器的初始化方法，确保平台类型正确设置。
        /// </summary>
        [UnityEditor.InitializeOnLoadMethod]
        internal static void OnInit() { if (Platform == PlatformType.Unknown) { } }
#endif

        /// <summary>
        /// Platform 获取当前运行平台类型。
        /// </summary>
        public static PlatformType Platform
        {
            get
            {
#if UNITY_EDITOR
                if (!platformGet)
                {
                    platformGet = true;
                    if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows ||
                                 UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64)
                    {
                        platform = PlatformType.Windows;
                    }
                    else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneLinux64)
                    {
                        platform = PlatformType.Linux;
                    }
                    else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneOSX)
                    {
                        platform = PlatformType.macOS;
                    }
                    else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                    {
                        platform = PlatformType.Android;
                    }
                    else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                    {
                        platform = PlatformType.iOS;
                    }
                    else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
                    {
                        platform = PlatformType.Browser;
                    }
                }
                return platform;
#elif UNITY_STANDALONE_WIN
                return PlatformType.Windows;
#elif UNITY_STANDALONE_LINUX
                return PlatformType.Linux;
#elif UNITY_STANDALONE_OSX
                return PlatformType.macOS;
#elif UNITY_ANDROID
                return PlatformType.Android;
#elif UNITY_IPHONE
                return PlatformType.iOS;
#elif UNITY_WEBGL
                return PlatformType.Browser;
#else
                return PlatformType.Unknown;
#endif
            }
        }

        /// <summary>
        /// AppType 是应用类型的枚举，定义应用程序的类型。
        /// </summary>
        public enum AppType
        {
            /// <summary>Unknown 表示未知或未定义的应用类型。</summary>
            Unknown,

            /// <summary>Server 表示服务器类型的应用，用于后端业务。</summary>
            Server,

            /// <summary>Client 表示客户端类型的应用，用于前端业务。</summary>
            Client
        }

        /// <summary>AppUnknown 是未知或未定义的应用类型的字符串标识。</summary>
        public static readonly string AppUnknown = AppType.Unknown.ToString();

        /// <summary>AppServer 是服务器类型的应用的字符串标识。</summary>
        public static readonly string AppServer = AppType.Server.ToString();

        /// <summary>AppClient 是客户端类型的应用的字符串标识。</summary>
        public static readonly string AppClient = AppType.Client.ToString();

        /// <summary>
        /// ModeType 是运行模式的枚举，定义应用程序的运行环境。
        /// </summary>
        public enum ModeType
        {
            /// <summary>Unknown 表示未知或未定义的运行模式。</summary>
            Unknown,

            /// <summary>Dev 表示开发模式，用于本地开发和调试。</summary>
            Dev,

            /// <summary>Test 表示测试模式，用于功能测试和验证。</summary>
            Test,

            /// <summary>Staging 表示预发模式，用于上线前测试。</summary>
            Staging,

            /// <summary>Prod 表示生产模式，用于正式环境。</summary>
            Prod
        }

        /// <summary>ModeUnknown 是未知或未定义的运行模式的字符串标识。</summary>
        public static readonly string ModeUnknown = ModeType.Unknown.ToString();

        /// <summary>ModeDev 是开发模式的字符串标识。</summary>
        public static readonly string ModeDev = ModeType.Dev.ToString();

        /// <summary>ModeTest 是测试模式的字符串标识。</summary>
        public static readonly string ModeTest = ModeType.Test.ToString();

        /// <summary>ModeStaging 是预发模式的字符串标识。</summary>
        public static readonly string ModeStaging = ModeType.Staging.ToString();

        /// <summary>ModeProd 是生产模式的字符串标识。</summary>
        public static readonly string ModeProd = ModeType.Prod.ToString();

        internal static string projectPath;
        /// <summary>
        /// ProjectPath 获取项目根目录路径，这个函数是线程安全的，可以多线程访问。
        /// </summary>
        public static string ProjectPath
        {
            get
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    projectPath = XFile.NormalizePath(Directory.GetCurrentDirectory());
                }
                return projectPath;
            }
        }

        internal static string localPath;
        /// <summary>
        /// LocalPath 获取数据存储目录路径，这个函数是线程不安全的，只能在主线程访问。
        /// </summary>
        public static string LocalPath
        {
            get
            {
                if (string.IsNullOrEmpty(localPath))
                {
                    if (Application.isEditor ||
                        Application.platform == RuntimePlatform.WindowsPlayer ||
                        Application.platform == RuntimePlatform.WindowsServer ||
                        Application.platform == RuntimePlatform.LinuxPlayer ||
                        Application.platform == RuntimePlatform.LinuxServer ||
                        Application.platform == RuntimePlatform.OSXPlayer ||
                        Application.platform == RuntimePlatform.OSXServer)
                    {
                        var cmdPath = GetArg("LocalPath");
                        if (!string.IsNullOrEmpty(cmdPath))
                        {
                            localPath = cmdPath;
                        }
                    }

                    if (string.IsNullOrEmpty(localPath))
                    {
                        if (Application.isEditor)
                        {
                            localPath = XFile.PathJoin(Application.dataPath, "..", "Local");
                        }
                        else if (Application.platform == RuntimePlatform.WindowsPlayer ||
                            Application.platform == RuntimePlatform.WindowsServer ||
                            Application.platform == RuntimePlatform.LinuxPlayer ||
                            Application.platform == RuntimePlatform.LinuxServer ||
                            Application.platform == RuntimePlatform.OSXPlayer ||
                            Application.platform == RuntimePlatform.OSXServer)
                        {
                            localPath = XFile.PathJoin(Application.streamingAssetsPath, "..", "Local");
                        }
                        else localPath = Application.persistentDataPath;
                    }

                    localPath = XFile.NormalizePath(localPath);
                    if (!XFile.HasDirectory(localPath)) XFile.CreateDirectory(localPath);
                }
                return localPath;
            }
        }

        internal static string assetPath;
        /// <summary>
        /// AssetPath 获取只读资源目录路径，这个函数是线程不安全的，只能在主线程访问。
        /// </summary>
        public static string AssetPath
        {
            get
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    assetPath = Application.streamingAssetsPath;
                }
                return assetPath;
            }
        }

        /// <summary>
        /// DeviceID 获取设备唯一标识符。
        /// </summary>
        public static string DeviceID { get { return SystemInfo.deviceUniqueIdentifier; } }

        /// <summary>
        /// MacAddr 获取设备MAC地址。
        /// </summary>
        public static string MacAddr
        {
            get
            {
                try
                {
                    var nis = NetworkInterface.GetAllNetworkInterfaces();
                    if (nis.Length <= 0) return "NO-MAC-ADDR";
                    else return nis[0].GetPhysicalAddress().ToString();
                }
                catch { return "MAC-ADDR-ERR"; }
            }
        }
    }

    public static partial class XEnv
    {
        /// <summary>
        /// Prefs 是环境配置的编辑器，提供环境相关配置项的管理功能。
        /// </summary>
        public class Prefs : XPrefs.IEditor
        {
#if UNITY_EDITOR
            [UnityEditor.InitializeOnLoadMethod]
#else
            [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
            /// <summary>
            /// OnInit 初始化环境配置的默认值。
            /// </summary>
            internal static void OnInit()
            {
                var projectName = Path.GetFileName(ProjectPath);
                var nameParts = projectName.Split('.');
                SolutionDefault = nameParts.Length == 2 ? nameParts[0] : projectName;
                ProjectDefault = nameParts.Length == 2 ? nameParts[1] : projectName;
                ProductDefault = Application.productName;
                VersionDefault = Application.version;
                SecretDefault = XString.Random()[..8];
            }

            /// <summary>App 是应用类型的配置键。</summary>
            public const string App = "Env/App";

            /// <summary>AppDefault 是应用类型的默认值：客户端。</summary>
            public static readonly string AppDefault = AppType.Client.ToString();

            /// <summary>Mode 是运行模式的配置键。</summary>
            public const string Mode = "Env/Mode";

            /// <summary>ModeDefault 是运行模式的默认值：开发模式。</summary>
            public static readonly string ModeDefault = ModeType.Dev.ToString();

            /// <summary>Solution 是解决方案名称的配置键。</summary>
            public const string Solution = "Env/Solution";

            /// <summary>SolutionDefault 是解决方案名称的默认值。</summary>
            public static string SolutionDefault { get; private set; }

            /// <summary>Project 是项目名称的配置键。</summary>
            public const string Project = "Env/Project";

            /// <summary>ProjectDefault 是项目名称的默认值。</summary>
            public static string ProjectDefault { get; private set; }

            /// <summary>Product 是产品名称的配置键。</summary>
            public const string Product = "Env/Product";

            /// <summary>ProductDefault 是产品名称的默认值。</summary>
            public static string ProductDefault { get; private set; }

            /// <summary>Channel 是发布渠道的配置键。</summary>
            public const string Channel = "Env/Channel";

            /// <summary>ChannelDefault 是发布渠道的默认值。</summary>
            public const string ChannelDefault = "Default";

            /// <summary>Version 是版本号的配置键。</summary>
            public const string Version = "Env/Version";

            /// <summary>VersionDefault 是版本号的默认值。</summary>
            public static string VersionDefault { get; private set; }

            /// <summary>Author 是作者名称的配置键。</summary>
            public const string Author = "Env/Author";

            /// <summary>AuthorDefault 是作者名称的默认值：使用系统用户名。</summary>
            public const string AuthorDefault = "${Env.UserName}";

            /// <summary>Secret 是应用密钥的配置键。</summary>
            public const string Secret = "Env/Secret";

            /// <summary>SecretDefault 是应用密钥的默认值：随机生成的8位字符串。</summary>
            public static string SecretDefault { get; private set; }

            /// <summary>Remote 是远程配置地址的配置键。</summary>
            public const string Remote = "Env/Remote";

            /// <summary>RemoteDefault 是远程配置地址的默认值：使用环境变量组合的 URL。</summary>
            public const string RemoteDefault = "${Env.OssPublic}/Prefs/${Env.Solution}/${Env.Channel}/${Env.Platform}/${Env.Version}/Preferences.json";

#if UNITY_EDITOR
            string XPrefs.IEditor.Section => "Env";

            string XPrefs.IEditor.Tooltip => "Preferences of Environment.";

            bool XPrefs.IEditor.Foldable => true;

            int XPrefs.IEditor.Priority => -1;

            void XPrefs.IEditor.OnActivate(string searchContext, VisualElement rootElement, XPrefs.IBase target) { }

            void XPrefs.IEditor.OnVisualize(string searchContext, XPrefs.IBase target)
            {
                UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("App"), GUILayout.Width(60));
                Enum.TryParse<AppType>(target.GetString(App, AppDefault), out var appType);
                target.Set(App, UnityEditor.EditorGUILayout.EnumPopup("", appType).ToString());

                GUILayout.Label(new GUIContent("Mode"), GUILayout.Width(60));
                Enum.TryParse<ModeType>(target.GetString(Mode, ModeDefault), out var modeType);
                target.Set(Mode, UnityEditor.EditorGUILayout.EnumPopup("", modeType).ToString());
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Solution"), GUILayout.Width(60));
                target.Set(Solution, UnityEditor.EditorGUILayout.TextField("", target.GetString(Solution, SolutionDefault)));

                GUILayout.Label(new GUIContent("Project"), GUILayout.Width(60));
                target.Set(Project, UnityEditor.EditorGUILayout.TextField("", target.GetString(Project, ProjectDefault)));
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Product"), GUILayout.Width(60));
                target.Set(Product, UnityEditor.EditorGUILayout.TextField("", target.GetString(Product, ProductDefault)));

                GUILayout.Label(new GUIContent("Channel"), GUILayout.Width(60));
                target.Set(Channel, UnityEditor.EditorGUILayout.TextField("", target.GetString(Channel, ChannelDefault)));
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Version"), GUILayout.Width(60));
                target.Set(Version, UnityEditor.EditorGUILayout.TextField("", target.GetString(Version, VersionDefault)));

                GUILayout.Label(new GUIContent("Author"), GUILayout.Width(60));
                target.Set(Author, UnityEditor.EditorGUILayout.TextField("", target.GetString(Author, AuthorDefault)));
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Secret"), GUILayout.Width(60));
                target.Set(Secret, UnityEditor.EditorGUILayout.TextField("", target.GetString(Secret, SecretDefault)));

                GUILayout.Label(new GUIContent("Remote"), GUILayout.Width(60));
                target.Set(Remote, UnityEditor.EditorGUILayout.TextField("", target.GetString(Remote, RemoteDefault)));
                UnityEditor.EditorGUILayout.EndHorizontal();
                UnityEditor.EditorGUILayout.EndVertical();
            }

            void XPrefs.IEditor.OnDeactivate(XPrefs.IBase target) { }

            bool XPrefs.IEditor.OnValidate(XPrefs.IBase target) { return true; }

            void XPrefs.IEditor.OnSave(XPrefs.IBase source, XPrefs.IBase target)
            {
                target.Set(App, source.Get(App, AppDefault));
                target.Set(Mode, source.Get(Mode, ModeDefault));
                target.Set(Solution, source.Get(Solution, SolutionDefault));
                target.Set(Project, source.Get(Project, ProjectDefault));
                target.Set(Product, source.Get(Product, ProductDefault));
                target.Set(Channel, source.Get(Channel, ChannelDefault));
                target.Set(Version, source.Get(Version, VersionDefault));
                target.Set(Author, source.Get(Author, AuthorDefault));
                target.Set(Secret, source.Get(Secret, SecretDefault));
                target.Set(Remote, source.Get(Remote, RemoteDefault));
            }

            void XPrefs.IEditor.OnApply(XPrefs.IBase source, XPrefs.IBase target, bool asset, bool remote)
            {
                if (asset) target.Set(Remote, target.Get(Remote, RemoteDefault).Eval(Vars));
                if (remote)
                {
                    target.Unset(App);
                    target.Unset(Mode);
                    target.Unset(Solution);
                    target.Unset(Project);
                    target.Unset(Product);
                    target.Unset(Channel);
                    target.Unset(Version);
                    target.Unset(Author);
                    target.Unset(Secret);
                    target.Unset(Remote);
                }
            }
#endif
        }

        internal static bool bSolution = false;
        internal static string solution = "";
        /// <summary>
        /// Solution 获取解决方案名称。
        /// </summary>
        public static string Solution
        {
            get
            {
                if (!bSolution
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bSolution = true;
                    solution = XPrefs.Asset.GetString(Prefs.Solution, Prefs.SolutionDefault).Eval(Vars);
                }
                return solution;
            }
        }

        internal static bool bProject = false;
        internal static string project = "";
        /// <summary>
        /// Project 获取项目名称。
        /// </summary>
        public static string Project
        {
            get
            {
                if (!bProject
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bProject = true;
                    project = XPrefs.Asset.GetString(Prefs.Project, Prefs.ProjectDefault).Eval(Vars);
                }
                return project;
            }
        }

        internal static bool bProduct = false;
        internal static string product = "";
        /// <summary>
        /// Product 获取产品名称。
        /// </summary>
        public static string Product
        {
            get
            {
                if (!bProduct
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bProduct = true;
                    product = XPrefs.Asset.GetString(Prefs.Product, Prefs.ProductDefault).Eval(Vars);
                }
                return product;
            }
        }

        internal static bool bChannel = false;
        internal static string channel = "";
        /// <summary>
        /// Channel 获取发布渠道。
        /// </summary>
        public static string Channel
        {
            get
            {
                if (!bChannel
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bChannel = true;
                    channel = XPrefs.Asset.GetString(Prefs.Channel, Prefs.ChannelDefault).Eval(Vars);
                }
                return channel;
            }
        }

        internal static bool bVersion = false;
        internal static string version = "";
        /// <summary>
        /// Version 获取版本号。
        /// </summary>
        public static string Version
        {
            get
            {
                if (!bVersion
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bVersion = true;
                    version = XPrefs.Asset.GetString(Prefs.Version, Prefs.VersionDefault).Eval(Vars);
                }
                return version;
            }
        }

        internal static bool bAuthor = false;
        internal static string author = "";
        /// <summary>
        /// Author 获取作者名称。
        /// </summary>
        public static string Author
        {
            get
            {
                if (!bAuthor
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bAuthor = true;
                    author = XPrefs.Asset.GetString(Prefs.Author, Prefs.AuthorDefault).Eval(Vars);
                }
                return author;
            }
        }

        internal static bool bApp = false;
        internal static AppType app = AppType.Unknown;
        /// <summary>
        /// App 获取应用类型。
        /// </summary>
        public static AppType App
        {
            get
            {
                if (!bApp
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bApp = true;
                    Enum.TryParse(XPrefs.Asset.GetString(Prefs.App, Prefs.AppDefault), out app);
                }
                return app;
            }
        }

        internal static bool bMode = false;
        internal static ModeType mode = ModeType.Unknown;
        /// <summary>
        /// Mode 获取运行模式。
        /// </summary>
        public static ModeType Mode
        {
            get
            {
                if (!bMode
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bMode = true;
                    Enum.TryParse(XPrefs.Asset.GetString(Prefs.Mode, Prefs.ModeDefault), out mode);
                }
                return mode;
            }
        }

        internal static bool bSecret = false;
        internal static string secret = "";
        /// <summary>
        /// Secret 获取应用密钥。
        /// </summary>
        public static string Secret
        {
            get
            {
                if (!bSecret
#if UNITY_EDITOR
                || Application.isEditor
#endif
                )
                {
                    bSecret = true;
                    if (!XPrefs.HasKey(Prefs.Secret)) XLog.Warn("XEnv.Secret: secret is not set, use default value.");
                    secret = XPrefs.Asset.GetString(Prefs.Secret, Prefs.SecretDefault).Eval(Vars);
                }
                return secret;
            }
        }

        internal static bool bRemote = false;
        internal static string remote = "";
        /// <summary>
        /// Remote 获取远程配置地址。
        /// </summary>
        public static string Remote
        {
            get
            {
                if (bRemote == false)
                {
                    bRemote = true;
                    remote = XPrefs.Asset.GetString(Prefs.Remote, Prefs.RemoteDefault).Eval(Vars);
                }
                return remote;
            }
        }
    }

    public static partial class XEnv
    {
        internal static List<KeyValuePair<string, string>> argsCache;

        internal static readonly object argsCacheLock = new();

        internal static bool argsCacheInitialized;

        /// <summary>
        /// GetArg 获取命令行参数值。
        /// </summary>
        /// <param name="key">参数名(不含--前缀)</param>
        public static string GetArg(string key)
        {
            var args = ParseArgs(false);
            foreach (var pair in args)
            {
                if (pair.Key == key)
                {
                    return pair.Value;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetArgs 获取所有命令行参数。
        /// </summary>
        /// <returns>参数列表，每个元素为一个键值对</returns>
        public static List<KeyValuePair<string, string>> GetArgs() { return ParseArgs(false); }

        /// <summary>
        /// ParseArgs 解析命令行参数并存储在缓存中。
        /// 支持以下格式：
        /// 1. -key 或 --key：无值参数
        /// 2. -key=value 或 --key=value：等号连接的键值对
        /// 3. -key value 或 --key value：空格分隔的键值对
        /// </summary>
        /// <param name="reset">是否重置缓存</param>
        /// <param name="extras">额外的命令行参数数组，将被追加到内置的命令行参数前</param>
        /// <returns>解析后的参数列表</returns>
        public static List<KeyValuePair<string, string>> ParseArgs(bool reset = true, params string[] extras)
        {
            lock (argsCacheLock)
            {
                // 如果需要重置缓存或缓存尚未初始化
                if (reset || argsCache == null || !argsCacheInitialized)
                {
                    argsCache = new List<KeyValuePair<string, string>>();

                    var args = new List<string>();
                    if (extras != null && extras.Length > 0) args.AddRange(extras);
                    args.AddRange(Environment.GetCommandLineArgs());

                    // 解析命令行参数
                    for (int i = 0; i < args.Count; i++)
                    {
                        var arg = args[i];
                        if (!arg.StartsWith("-")) continue;

                        // 移除前缀 (支持 - 和 --)
                        var key = arg.StartsWith("--") ? arg[2..] : arg[1..];

                        // 处理 -key=value 或 --key=value 格式
                        var equalIndex = key.IndexOf('=');
                        if (equalIndex != -1)
                        {
                            var value = key[(equalIndex + 1)..];
                            key = key[..equalIndex];
                            argsCache.Add(new KeyValuePair<string, string>(key, value));
                            continue;
                        }

                        // 处理无值参数 -key 或 --key
                        if (i + 1 >= args.Count || args[i + 1].StartsWith("-"))
                        {
                            argsCache.Add(new KeyValuePair<string, string>(key, string.Empty));
                            continue;
                        }

                        // 处理 -key value 或 --key value 格式
                        argsCache.Add(new KeyValuePair<string, string>(key, args[i + 1]));
                        i++; // 跳过下一个参数，因为它是值
                    }

                    // 标记缓存已初始化
                    argsCacheInitialized = true;
                }

                return argsCache;
            }
        }
    }

    public static partial class XEnv
    {
        /// <summary>
        /// Vars 是环境变量解析器实例。
        /// </summary>
        public static readonly Evaluator Vars = new();

        /// <summary>
        /// Evaluator 是环境变量解析器，用于处理配置中的环境变量引用。
        /// </summary>
        public sealed class Evaluator : XString.IEvaluator
        {
            /// <summary>
            /// pattern 是环境变量引用的正则表达式模式。
            /// </summary>
            internal static readonly Regex pattern = new(@"\$\{Env\.([^}]+?)\}", RegexOptions.Compiled);

            /// <summary>
            /// Eval 解析包含环境变量引用的字符串。
            /// </summary>
            /// <param name="input">要解析的字符串</param>
            /// <returns>解析后的结果</returns>
            string XString.IEvaluator.Eval(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;

                var visited = new HashSet<string>();

                string replaceFunc(Match match)
                {
                    var key = match.Groups[1].Value;

                    // 1. 检查嵌套变量
                    if (key.Contains("${"))
                    {
                        return $"{match.Value}(Nested)";
                    }

                    // 2. 检查循环引用
                    if (!visited.Add(key))
                    {
                        return $"${{Env.{key}}}(Recursive)";
                    }

                    try
                    {
                        // 3. 获取变量值
                        var value = key switch
                        {
                            "LocalPath" => LocalPath,
                            "ProjectPath" => ProjectPath,
                            "AssetPath" => AssetPath,
                            "UserName" => Environment.UserName,
                            "Platform" => Platform.ToString(),
                            "App" => App.ToString(),
                            "Mode" => Mode.ToString(),
                            "Solution" => Solution,
                            "Project" => Project,
                            "Product" => Product,
                            "Channel" => Channel,
                            "Version" => Version,
                            "Author" => Author,
                            "Secret" => Secret,
                            "NumCPU" => SystemInfo.processorCount.ToString(),
                            _ => GetArg(key)
                        };

                        if (string.IsNullOrEmpty(value))
                        {
                            value = Environment.GetEnvironmentVariable(key);
                        }

                        if (!string.IsNullOrEmpty(value))
                        {
                            // 递归处理嵌套的变量
                            return pattern.Replace(value, replaceFunc);
                        }

                        // 4. 处理未知变量
                        return $"${{Env.{key}}}(Unknown)";
                    }
                    finally
                    {
                        // 清理访问标记
                        visited.Remove(key);
                    }
                }

                return pattern.Replace(input, replaceFunc);
            }
        }
    }
}