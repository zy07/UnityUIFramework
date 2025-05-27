using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// KaLog.dll 的扩展使用
// KaLog 产生的动机: 
// 1、支持日志写文件、日志写自定义GUI 等多种日志记录、输出方式
// 2、支持额外Log功能的封装，比如log等级控制，业务领域Log的开关
// 3、确保封装Log后，对于编辑器Console, 双击Log条目时，代码不会跟踪到Log的封装函数里（所以打成dll）
//////////////////////////////////////////////////////////////////////////
public static partial class KaLogEx
{
    /* Form KaLog.dll
     KaLog.LOG_LEVEL_DEBUG = 500;
     KaLog.LOG_LEVEL_INFO = 400;
     KaLog.LOG_LEVEL_WARNING = 300;
     KaLog.LOG_LEVEL_ERROR = 200;
     KaLog.LOG_LEVEL_CRITICAL = 100;
    */
    public static bool IsDev = false;

    // Domain Log Level:
    public const int LogCore = 501;
    public const int LogNet = 502;
    public const int LogUI = 503;
    public const int LogInput = 504;
    public const int LogInGame = 505;
    public const int LogDungeon = 506;
    public const int LogSkill = 507;
    public const int LogSubobj = 508;

    // Programmer Log Level:
    public const int LogZY = 600;
    public const int LogZn = 601;

    public static LogWritersRoot RootWriters;

    public static void InitKaLog()
    {
        RootWriters = new LogWritersRoot();
        RootWriters.AddLogWriter(new KaLogEngineWriter());

#if UNITY_EDITOR
        IsDev = true;
#endif
        int devLogLv = PlayerPrefs.GetInt("DevLogLevel", 0);
        if (devLogLv > 0)
        {
            IsDev = true;
        }
        Debug.Log($"Launcher InitKaLog Application.platform={Application.platform}");
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                RootWriters.AddLogWriter(new KaLogFileWriter(ELogPlatform.Win));
                break;
            case RuntimePlatform.Android:
                if (Debug.isDebugBuild || IsDev)
                {
                    Debug.LogWarning("Launcher AddLogWriter Android");
                    RootWriters.AddLogWriter(new KaLogFileWriter(ELogPlatform.Android));
                }
                break;
            case RuntimePlatform.IPhonePlayer:
                if (IsDev)
                    RootWriters.AddLogWriter(new KaLogFileWriter(ELogPlatform.IOS));
                break;
            default:
                break;
        }
        KaLog.LogWriter = RootWriters;
        RootWriters.InitLogWriter();
        RootWriters.LogLevelMax = KaLog.LOG_LEVEL_INFO;

        KaLog.LogCritical("Core: Init Log");

    }
}



//////////////////////////////////////////////////////////////////////////
/// <summary>
/// Log等级总控, 根据项目临时需要做特化
/// </summary>
public class LogWritersRoot : KaLogWriters
{

    private int mCurLogLevelMax = KaLog.LOG_LEVEL_DEBUG;
    public int LogLevelMax
    {
        get
        {
            return mCurLogLevelMax;
        }
        set
        {
            mCurLogLevelMax = value;
        }
    }

    public override bool IsLogLevelValid(int level)
    {
        //控制屏蔽的LogLevel
        switch (level)
        {
            case KaLogEx.LogCore:
            case KaLogEx.LogInGame:
            case KaLogEx.LogDungeon:
            case KaLogEx.LogNet:
            //case KaLogEx.LOG_Input:
            //case KaLogEx.LogSubobj:
            //case KaLogEx.LogZn:
                return true;
        }

        if (level > mCurLogLevelMax)
        {
            return false;
        }
        return true;
        //return base.IsLogLevelValid(level);
    }
}