using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace FFmpegPlayerSharp.Core.FFmpeg;

public static class FFmpegExtend
{
    public static unsafe string av_strerror(int error)
    {
        var bufferSize = 1024;
        var buffer = stackalloc byte[bufferSize];
        ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
        var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
        return message;
    }

    public static int ThrowExceptionIfError(this int error, string msg = "")
    {
        if (error < 0)
        {
            string errorMsg = av_strerror(error);
            Entrance.Logger.Error($"{errorMsg}\t{msg}");
        }

        return error;
    }

    public static bool CanHWDecode(this AVCodecID codecid)
    {
        if (Entrance.HWDeviceType == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            return false;
        switch (codecid)
        {
            case AVCodecID.AV_CODEC_ID_H264:
            case AVCodecID.AV_CODEC_ID_HEVC:
            case AVCodecID.AV_CODEC_ID_VC1:
            case AVCodecID.AV_CODEC_ID_AV1:
            case AVCodecID.AV_CODEC_ID_MPEG2VIDEO:
            case AVCodecID.AV_CODEC_ID_VP9:
                return true;
            default:
                return false;
        }
    }
}