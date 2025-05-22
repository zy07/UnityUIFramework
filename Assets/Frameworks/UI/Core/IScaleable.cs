using System;

public interface IScaleable
{
    void DoScale();
    void DoScaleReverse(Action<string> callBack);
}
