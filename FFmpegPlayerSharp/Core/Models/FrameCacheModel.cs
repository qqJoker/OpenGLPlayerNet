using FFmpeg.AutoGen;

namespace FFmpegPlayerSharp.Core.Models;

public struct FrameCacheModel
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte_ptrArray8 Data { get; set; }
    public int PicSize { get; set; }
    public int LineSize { get; set; } 
    public int_array8 LineSizeArr { get; set; }
}