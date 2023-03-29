using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using FFmpeg.AutoGen.Native;
using FFmpegPlayerSharp.Core.Log;
using FFmpegPlayerSharp.Helper;

namespace FFmpegPlayerSharp;

/// <summary>
/// 播放器初始化入口
/// </summary>
public static class Entrance
{
    /// <summary>
    /// 播放器未引用时最大存活周期，单位秒，最小3秒
    /// </summary>
    public static ulong PlayerMaxLiveTime { get; set; }

    /// <summary>
    /// 系统启动时间
    /// </summary>
    public static readonly DateTime SystemStartTime;

    /// <summary>
    /// 硬解模式
    /// </summary>
    public static AVHWDeviceType HWDeviceType { get; private set; }

    /// <summary>
    /// 是否启用日至
    /// </summary>
    public static bool EnableLog { get; private set; }

    /// <summary>
    /// Log
    /// </summary>
    internal static ILog Logger { get; private set; }

    /// <summary>
    /// 当前系统类型
    /// </summary>
    public readonly static PlatformID OS;

    static Entrance()
    {
        SystemStartTime = DateTime.Now;
        PlayerCollection.Instance.Init();

        OS = LibraryLoader.GetPlatformId();
        Logger = new ConsoleLog();
    }

    public static void InitFFmpegLibs(bool enableHwDecoder = true,
        bool enableLog = true)
    {
        EnableLog = enableLog;

        if (OS.IsWindowsOS())
        {
            var probe = Path.Combine("libs", "ffmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");
            var ffmpegBinaryPath = Path.Combine(Environment.CurrentDirectory, probe);
            if (!Directory.Exists(ffmpegBinaryPath))
                throw new FileNotFoundException("FFmpeg binary files not found!");
            ffmpeg.RootPath = ffmpegBinaryPath;
        }
        else if (OS.IsLinuxOS())
        {
            ffmpeg.RootPath = "/usr/lib/x86_64-linux-gnu";
        }
        else
        {
            throw new SystemException("SystemOS Not Found!");
        }

        if (enableLog)
            SetupLogger();

        if (enableHwDecoder)
        {
            HWDeviceType = ConfigHwDecoder();
        }
    }

    /// <summary>
    /// 配置日志
    /// </summary>
    private static unsafe void SetupLogger()
    {
        ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);
        av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
        {
            if (level > ffmpeg.av_log_get_level()) return;
            var lineSize = 1024;
            var lineBuffer = stackalloc byte[lineSize];
            var printPrefix = 1;
            ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
            var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
        };

        ffmpeg.av_log_set_callback(logCallback);
    }

    /// <summary>
    /// 寻找硬解设备
    /// </summary>
    /// <returns></returns>
    private static AVHWDeviceType ConfigHwDecoder()
    {
        AVHWDeviceType result = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        List<AVHWDeviceType> hwDevices = new List<AVHWDeviceType>();
        while ((type = ffmpeg.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            hwDevices.Add(type);
            Console.WriteLine(type);
        }

        if (hwDevices.Count == 0)
            return result;
        
        if (OS.IsLinuxOS() && hwDevices.Contains(AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI))
            result = AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI;
        else if (OS.IsWindowsOS() && hwDevices.Contains(AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2))
            result = AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2;

        unsafe
        {
            AVBufferRef* deviceRef = null;
            try
            {
                int ret = ffmpeg.av_hwdevice_ctx_create(&deviceRef, result, null, null, 0);
                if (ret < 0)
                {
                    result = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
                    Logger.Warn("未查找到:VAAPI解码设备,已切换为软解模式!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        return result;
    }

    /// <summary>
    /// 释放数据
    /// </summary>
    public static void Release()
    {
        PlayerCollection.Instance.Dispose();
    }


    internal static AVPixelFormat GetHWPixelFormat()
    {
        switch (HWDeviceType)
        {
            case AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU: return AVPixelFormat.AV_PIX_FMT_VDPAU;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA: return AVPixelFormat.AV_PIX_FMT_CUDA;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI: return AVPixelFormat.AV_PIX_FMT_VAAPI_VLD;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2:
            case AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA:
                return AVPixelFormat.AV_PIX_FMT_NV12;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_QSV: return AVPixelFormat.AV_PIX_FMT_QSV;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX: return AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_DRM: return AVPixelFormat.AV_PIX_FMT_DRM_PRIME;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL: return AVPixelFormat.AV_PIX_FMT_OPENCL;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC: return AVPixelFormat.AV_PIX_FMT_MEDIACODEC;
            case AVHWDeviceType.AV_HWDEVICE_TYPE_NONE:
            default:
                return AVPixelFormat.AV_PIX_FMT_NONE;
        }
    }
}