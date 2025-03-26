// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using UnityEngine;

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
        /// 平台类型枚举，定义了支持的运行平台类型。
        /// </summary>
        /// <remarks>
        /// 用于标识应用程序运行的目标平台，支持主流桌面、移动和浏览器平台。
        /// </remarks>
        public enum PlatformType
        {
            /// <summary>未知或不支持的平台</summary>
            Unknown,

            /// <summary>Windows 桌面平台</summary>
            Windows,

            /// <summary>Linux 桌面平台</summary>
            Linux,

            /// <summary>macOS 桌面平台</summary>
            OSX,

            /// <summary>Android 移动平台</summary>
            Android,

            /// <summary>iOS 移动平台，包括 iPhone 和 iPad</summary>
            iOS,

            /// <summary>浏览器平台，主要指 WebGL</summary>
            Browser
        }

        /// <summary>未知平台的字符串标识</summary>
        public static readonly string PlatformUnknown = PlatformType.Unknown.ToString();

        /// <summary>Windows 平台的字符串标识</summary>
        public static readonly string PlatformWindows = PlatformType.Windows.ToString();

        /// <summary>Linux 平台的字符串标识</summary>
        public static readonly string PlatformLinux = PlatformType.Linux.ToString();

        /// <summary>macOS 平台的字符串标识</summary>
        public static readonly string PlatformOSX = PlatformType.OSX.ToString();

        /// <summary>Android 平台的字符串标识</summary>
        public static readonly string PlatformAndroid = PlatformType.Android.ToString();

        /// <summary>iOS 平台的字符串标识</summary>
        public static readonly string PlatformiOS = PlatformType.iOS.ToString();

        /// <summary>浏览器平台的字符串标识</summary>
        public static readonly string PlatformBrowser = PlatformType.Browser.ToString();

#if UNITY_EDITOR
        internal static PlatformType platform = PlatformType.Unknown;
        internal static bool platformGet = false;

        /// <summary>
        /// 编辑器初始化方法，确保平台类型正确设置。
        /// </summary>
        [UnityEditor.InitializeOnLoadMethod]
        internal static void OnInit() { if (Platform == PlatformType.Unknown) { } }
#endif

        /// <summary>
        /// 获取当前运行平台类型。
        /// </summary>
        /// <remarks>
        /// 在编辑器中，根据当前的构建目标平台返回对应类型。
        /// 在运行时，根据实际运行平台返回对应类型。
        /// </remarks>
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
                        platform = PlatformType.OSX;
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
                return PlatformType.OSX;
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
        /// 应用类型枚举，定义应用程序的类型。
        /// </summary>
        /// <remarks>
        /// 用于区分应用程序的运行角色，支持服务器端和客户端应用。
        /// </remarks>
        public enum AppType
        {
            /// <summary>未知或未定义的应用类型</summary>
            Unknown,

            /// <summary>服务器应用，用于后端业务</summary>
            Server,

            /// <summary>客户端应用，用于前端业务</summary>
            Client
        }

        /// <summary>未知应用类型的字符串标识</summary>
        public static readonly string AppUnknown = AppType.Unknown.ToString();

        /// <summary>服务器应用类型的字符串标识</summary>
        public static readonly string AppServer = AppType.Server.ToString();

        /// <summary>客户端应用类型的字符串标识</summary>
        public static readonly string AppClient = AppType.Client.ToString();

        /// <summary>
        /// 运行模式枚举，定义应用程序的运行环境。
        /// </summary>
        /// <remarks>
        /// 用于区分应用程序的运行环境，支持开发、测试、预发和生产环境。
        /// </remarks>
        public enum ModeType
        {
            /// <summary>未知或未定义的运行模式</summary>
            Unknown,

            /// <summary>开发模式，用于本地开发和调试</summary>
            Dev,

            /// <summary>测试模式，用于功能测试和验证</summary>
            Test,

            /// <summary>预发模式，用于上线前测试</summary>
            Staging,

            /// <summary>生产模式，用于正式环境</summary>
            Prod
        }

        /// <summary>未知运行模式的字符串标识</summary>
        public static readonly string ModeUnknown = ModeType.Unknown.ToString();

        /// <summary>开发模式的字符串标识</summary>
        public static readonly string ModeDev = ModeType.Dev.ToString();

        /// <summary>测试模式的字符串标识</summary>
        public static readonly string ModeTest = ModeType.Test.ToString();

        /// <summary>预发模式的字符串标识</summary>
        public static readonly string ModeStaging = ModeType.Staging.ToString();

        /// <summary>生产模式的字符串标识</summary>
        public static readonly string ModeProd = ModeType.Prod.ToString();

        internal static string projectPath;
        /// <summary>
        /// 获取项目根目录路径
        /// </summary>
        /// <remarks>
        /// 在编辑器中为Assets的父目录
        /// 在运行时为应用程序的根目录
        /// </remarks>
        public static string ProjectPath
        {
            get
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    projectPath = XFile.NormalizePath(Directory.GetParent(Application.dataPath).ToString());
                }
                return projectPath;
            }
        }

        internal static string localPath;
        /// <summary>
        /// 获取数据存储目录路径
        /// </summary>
        /// <remarks>
        /// 在编辑器中为Local目录
        /// 在Windows平台为Local目录
        /// 在其他平台为persistentDataPath
        /// </remarks>
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
        /// 获取只读资源目录路径
        /// </summary>
        /// <remarks>
        /// 对应Unity的StreamingAssets目录
        /// 用于存放只读的资源文件
        /// </remarks>
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
        /// 获取设备唯一标识符
        /// </summary>
        /// <remarks>
        /// 使用SystemInfo.deviceUniqueIdentifier获取
        /// 在同一设备上保持不变
        /// </remarks>
        public static string DeviceID { get { return SystemInfo.deviceUniqueIdentifier; } }

        /// <summary>
        /// 获取设备MAC地址
        /// </summary>
        /// <remarks>
        /// 获取第一个网络接口的物理地址
        /// 如果获取失败返回错误信息
        /// </remarks>
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
        /// 环境配置面板类，提供环境相关配置项的管理功能。
        /// </summary>
        /// <remarks>
        /// 提供以下配置项：
        /// - App：应用程序类型（Client/Server）
        /// - Mode：运行模式（Dev/Test/Staging/Prod）
        /// - Solution：解决方案名称
        /// - Project：项目名称
        /// - Product：产品名称
        /// - Channel：发布渠道
        /// - Version：版本号
        /// - Author：作者信息
        /// - Secret：密钥
        /// - Remote：远程配置地址
        /// </remarks>
        public class Prefs : XPrefs.Panel
        {
#if UNITY_EDITOR
            [UnityEditor.InitializeOnLoadMethod]
#else
            [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
            /// <summary>
            /// 初始化环境配置的默认值。
            /// </summary>
            /// <remarks>
            /// - 从项目路径解析 Solution 和 Project 默认值
            /// - 从 Unity 配置获取 Product 和 Version 默认值
            /// - 生成随机的 Secret 默认值
            /// </remarks>
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

            /// <summary>应用类型配置键</summary>
            public const string App = "Env/App";

            /// <summary>应用类型默认值：客户端</summary>
            public static readonly string AppDefault = AppType.Client.ToString();

            /// <summary>运行模式配置键</summary>
            public const string Mode = "Env/Mode";

            /// <summary>运行模式默认值：开发模式</summary>
            public static readonly string ModeDefault = ModeType.Dev.ToString();

            /// <summary>解决方案名称配置键</summary>
            public const string Solution = "Env/Solution";

            /// <summary>解决方案名称默认值</summary>
            public static string SolutionDefault { get; private set; }

            /// <summary>项目名称配置键</summary>
            public const string Project = "Env/Project";

            /// <summary>项目名称默认值</summary>
            public static string ProjectDefault { get; private set; }

            /// <summary>产品名称配置键</summary>
            public const string Product = "Env/Product";

            /// <summary>产品名称默认值</summary>
            public static string ProductDefault { get; private set; }

            /// <summary>发布渠道配置键</summary>
            public const string Channel = "Env/Channel";

            /// <summary>发布渠道默认值</summary>
            public const string ChannelDefault = "Default";

            /// <summary>版本号配置键</summary>
            public const string Version = "Env/Version";

            /// <summary>版本号默认值</summary>
            public static string VersionDefault { get; private set; }

            /// <summary>作者信息配置键</summary>
            public const string Author = "Env/Author";

            /// <summary>作者信息默认值：使用系统用户名</summary>
            public const string AuthorDefault = "${Env.UserName}";

            /// <summary>密钥配置键</summary>
            public const string Secret = "Env/Secret";

            /// <summary>密钥默认值：随机生成的8位字符串</summary>
            public static string SecretDefault { get; private set; }

            /// <summary>远程配置地址配置键</summary>
            public const string Remote = "Env/Remote";

            /// <summary>远程配置地址默认值：使用环境变量组合的URL</summary>
            public const string RemoteDefault = "${Env.OssPublic}/Prefs/${Env.Solution}/${Env.Channel}/${Env.Platform}/${Env.Version}/Preferences.json";

#if UNITY_EDITOR
            public override string Section => "Env";

            public override int Priority => -1;

            public override string Tooltip => "Preferences of Environment.";

            public override void OnVisualize(string searchContext)
            {
                UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("App");
                Enum.TryParse<AppType>(Target.GetString(App, AppDefault), out var appType);
                Target.Set(App, UnityEditor.EditorGUILayout.EnumPopup("", appType).ToString());

                Title("Mode");
                Enum.TryParse<ModeType>(Target.GetString(Mode, ModeDefault), out var modeType);
                Target.Set(Mode, UnityEditor.EditorGUILayout.EnumPopup("", modeType).ToString());
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("Solution");
                Target.Set(Solution, UnityEditor.EditorGUILayout.TextField("", Target.GetString(Solution, SolutionDefault)));

                Title("Project");
                Target.Set(Project, UnityEditor.EditorGUILayout.TextField("", Target.GetString(Project, ProjectDefault)));
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("Product");
                Target.Set(Product, UnityEditor.EditorGUILayout.TextField("", Target.GetString(Product, ProductDefault)));

                Title("Channel");
                Target.Set(Channel, UnityEditor.EditorGUILayout.TextField("", Target.GetString(Channel, ChannelDefault)));
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("Version");
                Target.Set(Version, UnityEditor.EditorGUILayout.TextField("", Target.GetString(Version, VersionDefault)));

                Title("Author");
                Target.Set(Author, UnityEditor.EditorGUILayout.TextField("", Target.GetString(Author, AuthorDefault)));
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("Secret");
                Target.Set(Secret, UnityEditor.EditorGUILayout.TextField("", Target.GetString(Secret, SecretDefault)));

                Title("Remote");
                Target.Set(Remote, UnityEditor.EditorGUILayout.TextField("", Target.GetString(Remote, RemoteDefault)));
                UnityEditor.EditorGUILayout.EndHorizontal();
                UnityEditor.EditorGUILayout.EndVertical();
            }
#endif
        }

        internal static bool bSolution = false;
        internal static string solution = "";
        /// <summary>
        /// 获取解决方案名称。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// </remarks>
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
        /// 获取项目名称。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// </remarks>
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
        /// 获取产品名称。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// </remarks>
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
        /// 获取发布渠道。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// </remarks>
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
        /// 获取版本号。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// </remarks>
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
        /// 获取作者信息。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// </remarks>
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
        /// 获取应用类型。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// </remarks>
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
        /// 获取运行模式。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// </remarks>
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
        /// 获取密钥。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 在编辑器中每次访问都会重新获取。
        /// 如果未设置则使用默认值并输出警告。
        /// </remarks>
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
        /// 获取远程配置地址。
        /// </summary>
        /// <remarks>
        /// 从配置中获取，支持环境变量解析。
        /// 使用环境变量组合生成完整的URL地址。
        /// </remarks>
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
        /// 获取命令行参数值
        /// </summary>
        /// <param name="key">参数名(不含--前缀)</param>
        /// <returns>参数值，未找到返回空字符串</returns>
        /// <example>
        /// <code>
        /// // 命令行: --port 8080
        /// var port = XEnv.GetArg("port"); // 返回 "8080"
        /// </code>
        /// </example>
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
        /// 获取所有命令行参数
        /// </summary>
        /// <returns>参数列表，每个元素为一个键值对</returns>
        /// <remarks>
        /// 支持以下格式：
        /// 1. -key 或 --key：无值参数
        /// 2. -key=value 或 --key=value：等号连接的键值对
        /// 3. -key value 或 --key value：空格分隔的键值对
        /// </remarks>
        /// <example>
        /// <code>
        /// // 命令行: --port 8080 --config=dev.json --flag
        /// var args = XEnv.GetArgs();
        /// // args 包含 [{"--port", "8080"}, {"--config", "dev.json"}, {"--flag", ""}]
        /// </code>
        /// </example>
        public static List<KeyValuePair<string, string>> GetArgs() { return ParseArgs(false); }

        /// <summary>
        /// 解析命令行参数并存储在缓存中。
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
        /// 环境变量解析器实例。
        /// </summary>
        public static readonly Eval Vars = new();

        /// <summary>
        /// 环境变量解析器，用于处理配置中的环境变量引用。
        /// </summary>
        /// <remarks>
        /// 支持以下功能：
        /// 1. 解析 ${Env.xxx} 格式的环境变量引用
        /// 2. 支持嵌套变量解析
        /// 3. 防止循环引用
        /// 4. 提供默认值和错误处理
        /// </remarks>
        public sealed class Eval : XString.IEval
        {
            /// <summary>
            /// 环境变量引用的正则表达式模式。
            /// </summary>
            /// <remarks>
            /// 格式：${Env.变量名}
            /// 变量名允许使用除}外的任意字符。
            /// </remarks>
            internal static readonly Regex pattern = new(@"\$\{Env\.([^}]+?)\}", RegexOptions.Compiled);

            /// <summary>
            /// 解析包含环境变量引用的字符串。
            /// </summary>
            /// <param name="input">要解析的字符串</param>
            /// <returns>解析后的结果</returns>
            /// <remarks>
            /// 解析过程：
            /// 1. 检查并处理嵌套变量
            /// 2. 检测并防止循环引用
            /// 3. 按优先级查找变量值：
            ///    - 内置环境变量
            ///    - 命令行参数
            ///    - 系统环境变量
            /// 4. 处理未知变量
            /// </remarks>
            string XString.IEval.Eval(string input)
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