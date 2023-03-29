namespace FFmpegPlayerSharp.Core.Log;

internal class ConsoleLog : ILog
{
    public void Error(string error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ResetColor();
    }

    public void Error(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex);
        Console.ResetColor();
    }

    public void Debug(string debug)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(debug);
        Console.ResetColor();
    }

    public void Info(string info)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(info);
        Console.ResetColor();
    }

    public void Warn(string warn)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(warn);
        Console.ResetColor();
    }
}