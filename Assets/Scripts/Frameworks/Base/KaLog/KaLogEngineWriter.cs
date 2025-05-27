using System.Collections;
using System.Collections.Generic;



//////////////////////////////////////////////////////////////////////////
/// <summary>
/// 引擎日志打印：UnityEngineLogWriter放dll里为了编辑器双击日志时，能跳到日志调用的地方，而不是封装Log的地方。
/// 这里针对项目，可以做一些临时特化
/// </summary>
public class KaLogEngineWriter : UnityEngineLogWriter
{
    public override void InitLogWriter()
    {
    }

    public override bool IsLogLevelValid(int level)
    {
        return true;
    }

    //////////////////////////////////////////////////////////////////////////
    // as: UnityEngineLogWriter
    // level和Unity系统日志类别的对应关系:

    //protected override bool IsLogError(int level)
    //{
    //    return base.IsLogError(level);
    //}

    protected override bool IsLogWarning(int level)
    {
        if (level == KaLog.LOG_LEVEL_WARNING || level == KaLog.LOG_LEVEL_CRITICAL)
        {
            return true;
        }
        return false;
    }

    //protected override bool IsLogInfo(int level)
    //{
    //    return base.IsLogInfo(level);
    //}
}