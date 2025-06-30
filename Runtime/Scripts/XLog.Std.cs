// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using UnityEngine;

namespace EFramework.Utility
{
    public partial class XLog
    {
        /// <summary>
        /// StdAdapter 是日志标准输出适配器，实现日志的控制台输出功能。
        /// 支持日志着色和级别过滤等特性。
        /// </summary>
        /// <remarks>
        /// 功能特性:
        /// - 支持日志级别过滤
        /// - 支持日志着色输出
        /// - 支持批处理模式
        /// </remarks>
        internal partial class StdAdapter : IAdapter
        {
            /// <summary>
            /// logBrush 是日志的着色器，用于生成带颜色的日志文本。
            /// </summary>
            /// <param name="color">颜色值</param>
            /// <returns>着色函数</returns>
            internal static Func<string, string> logBrush(string color) { return (text) => $"<color={color}><b>{text}</b></color>"; }

            /// <summary>
            /// logBrushes 是日志级别对应的着色函数数组。
            /// </summary>
            internal static readonly Func<string, string>[] logBrushes = new Func<string, string>[] {
                logBrush("black"), // Emergency
                logBrush("cyan"), // Alert
                logBrush("magenta"), // Critical
                logBrush("red"), // Error
                logBrush("yellow"), // Warn
                logBrush("green"), // Notice
                logBrush("grey"), // Info
                logBrush("blue"), // Debug
            };

            /// <summary>
            /// level 是日志输出的级别。
            /// </summary>
            internal LevelType level;

            /// <summary>
            /// colored 表示是否启用日志着色。
            /// </summary>
            internal bool colored;

            /// <summary>
            /// Init 初始化标准输出适配器。
            /// </summary>
            /// <param name="prefs">配置参数</param>
            /// <returns>日志输出级别</returns>
            public LevelType Init(XPrefs.IBase prefs)
            {
                if (prefs == null) return LevelType.Undefined;
                if (!Enum.TryParse(prefs.GetString(Prefs.Level, Prefs.LevelDefault), out level))
                {
                    level = LevelType.Undefined;
                }
                colored = prefs.GetBool(Prefs.Color, Prefs.ColorDefault);
                return level;
            }

            /// <summary>
            /// Write 写入日志数据。
            /// </summary>
            /// <param name="data">日志数据</param>
            public void Write(LogData data)
            {
                try
                {
                    if (data == null) return;
                    if (data.Level > level && !data.Force) return;
                    if (data.Level == LevelType.Emergency && data.Data is Exception exception)
                    {
                        Handler.Default.LogException(exception, null);
                    }
                    else
                    {
                        var text = data.Text(true);
                        if (colored && !batchMode)
                        {
                            var idx = (int)data.Level;
                            text = text.Replace(logLabels[idx], logBrushes[idx](logLabels[idx]));
                        }

                        var timeStr = XTime.Format(data.Time, "MM/dd HH:mm:ss.fff");
                        var fullText = $"[{timeStr}] {text}";

                        if (data.Level == LevelType.Emergency) Handler.Default.LogFormat(LogType.Exception, null, "{0}", fullText);
                        else if (data.Level <= LevelType.Error) Handler.Default.LogFormat(LogType.Error, null, "{0}", fullText);
                        else Handler.Default.LogFormat(LogType.Log, null, "{0}", fullText);
                    }
                }
                catch (Exception e) { Handler.Default.LogException(e, null); }
                finally { LogData.Put(data); }
            }

            /// <summary>
            /// Flush 刷新日志缓冲区。
            /// </summary>
            public void Flush() { }

            /// <summary>
            /// Close 关闭日志适配器。
            /// </summary>
            public void Close() { }
        }

        internal partial class StdAdapter
        {
            public class Prefs : XPrefs.Panel
            {
                public const string Config = "Log/Std";

                public static readonly XPrefs.IBase ConfigDefault = new();

                public const string Level = "Level";

                public static readonly string LevelDefault = LevelType.Info.ToString();

                public const string Color = "Color";

                public static readonly bool ColorDefault = true;

#if UNITY_EDITOR
                public override string Section => "Log";

                public override int Priority => 10;

                [SerializeField] protected bool foldout = true;

                public override void OnVisualize(string searchContext)
                {
                    var config = Target.Get(Config, ConfigDefault);
                    UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                    foldout = UnityEditor.EditorGUILayout.Foldout(foldout, new GUIContent("Std", "Standard Output Adapter."));
                    if (foldout)
                    {
                        UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        Title("Level", "Log Level.");
                        Enum.TryParse<LevelType>(config.GetString(Level, LevelDefault), out var levelType);
                        config.Set(Level, UnityEditor.EditorGUILayout.EnumPopup("", levelType).ToString());

                        Title("Color", "Enable color log.");
                        config.Set(Color, UnityEditor.EditorGUILayout.Toggle("", config.GetBool(Color, ColorDefault)));
                        UnityEditor.EditorGUILayout.EndHorizontal();
                        UnityEditor.EditorGUILayout.EndVertical();
                    }
                    UnityEditor.EditorGUILayout.EndVertical();
                    if (!Target.Has(Config) || config.Dirty) Target.Set(Config, config);
                }

                public override bool Validate()
                {
                    levelMax = LevelType.Undefined; // 重置最大值
                    return base.Validate();
                }

                public override void OnApply()
                {
                    var config = Target.Get(Config, ConfigDefault);
                    Enum.TryParse<LevelType>(config.GetString(Level, LevelDefault), out var levelType);
                    if (levelType > levelMax) levelMax = levelType;
                }
#endif
            }
        }
    }
}
