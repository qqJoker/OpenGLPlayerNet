namespace FFmpegPlayerSharp.Core.Models;

public class Enums
{
    public enum PlayerStatus
    {
        /// <summary>
        /// 已销毁
        /// </summary>
        Disposed,

        /// <summary>
        /// 销毁中
        /// </summary>
        Disposing,

        /// <summary>
        /// 播放中
        /// </summary>
        Playing,
    }

    public enum LogType
    {
        Console,
        Debug,
        File
    }

    public enum PlayerType
    {
        FFmpeg,
    }
}