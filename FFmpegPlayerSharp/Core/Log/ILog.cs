namespace FFmpegPlayerSharp.Core.Log;

internal interface ILog
{
    void Error(string error);
    void Error(Exception ex);
    void Debug(string debug);
    void Info(string info);
    void Warn(string warn);
}