namespace FFmpegPlayerSharp.Core.Log;

internal class DebugLog : ILog
{
    public void Error(string error)
    {
        System.Diagnostics.Debug.Fail(error);
    }

    public void Error(Exception ex)
    {
        System.Diagnostics.Debug.Fail(ex.ToString());
    }

    public void Debug(string debug)
    {
        System.Diagnostics.Debug.WriteLine(debug);
    }

    public void Info(string info)
    {
        System.Diagnostics.Debug.WriteLine(info);
    }

    public void Warn(string warn)
    {
        System.Diagnostics.Debug.WriteLine(warn);
    }
}