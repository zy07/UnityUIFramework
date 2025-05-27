using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public enum ELogPlatform
{
    Android,
    IOS,
    Win,
    Editor,
    WinEditor
}

/// <summary>
/// 判断对应的平台，将Log写入文件
/// </summary>
public class KaLogFileWriter : IKaLogWriter
{
    protected const string PATH_END = "/";

    private ELogPlatform mLogPlatform = ELogPlatform.Win;
    private StreamWriter mStreamWriter = null;
    private bool mHasInit = false;
    private string mDirectoryName = "LogSave";

    const int MAX_LOG_ENTRY_NUM = 10;

    public KaLogFileWriter(ELogPlatform platform)
    {
        mLogPlatform = platform;
        mHasInit = false;
    }

    //////////////////////////////////////////////////////////////////////////
    // As: IKaLogWriter
    public void InitLogWriter()
    {
        if (mHasInit)
            return;

        try
        {
            string logDir = GetLogDir();
            Debug.Log($"Launcher InitLogWriter logDir={logDir}");

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            string[] logs = GetLogFiles(logDir);

            if (logs.Length > MAX_LOG_ENTRY_NUM)
            {
                foreach (var log in logs)
                {
                    File.Delete(log);
                }
            }

            string fileName = logDir + "/" + string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute,
                DateTime.Now.Second, "kaboom.txt");
            Debug.Log($"Launcher InitLogWriter fileName={fileName}");

            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            mStreamWriter = new StreamWriter(fs);
            mStreamWriter.AutoFlush = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Launcher InitLogWriter Exception:{e.ToString()}");
            mStreamWriter = null;
        }

        mHasInit = true;
    }

    public void WriteLog(string line, int level)
    {
        if (mStreamWriter != null)
        {
            lock (mStreamWriter)
            {
                try
                {
                    string timeStr = DateTime.Now.ToString();
                    if (level == KaLog.LOG_LEVEL_DEBUG)
                        mStreamWriter.WriteLine($"{timeStr} Debug : {line}");
                    else if(level == KaLog.LOG_LEVEL_INFO)
                        mStreamWriter.WriteLine($"{timeStr} Info : {line}");
                    else if (level == KaLog.LOG_LEVEL_WARNING)
                        mStreamWriter.WriteLine($"{timeStr} Warning : {line}");
                    else if (level == KaLog.LOG_LEVEL_ERROR)
                        mStreamWriter.WriteLine($"{timeStr} Error : {line}");
                    else if (level == KaLog.LOG_LEVEL_CRITICAL)
                        mStreamWriter.WriteLine($"{timeStr} Critical : {line}");
                    else
                    {
                        mStreamWriter.WriteLine($"{timeStr} Lv_{level} : {line}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Launcher WriteLog Exception:{e.ToString()}");
                }
            }
        }
    }

    public bool IsLogLevelValid(int level)
    {
        return true;
    }

    //////////////////////////////////////////////////////////////////////////
    // As: This
    private static string[] GetLogFiles(string logFilePath)
    {
        return Directory.GetFiles(logFilePath, "*.log", SearchOption.TopDirectoryOnly);
    }

    public string GetLogDir()
    {
        switch (mLogPlatform)
        {
            case ELogPlatform.Android:
                return Application.persistentDataPath + PATH_END + "/" + mDirectoryName;
            case ELogPlatform.IOS:
                return Application.temporaryCachePath + "/" + mDirectoryName;
            case ELogPlatform.Win:
            case ELogPlatform.WinEditor:
            case ELogPlatform.Editor:
                return Application.dataPath + "/../" + mDirectoryName;
        }
        return Application.dataPath + "/../" + mDirectoryName;
    }
}
