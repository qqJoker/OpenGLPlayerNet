using System.Drawing;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace FFmpegPlayerSharp.Core.FFmpeg;

internal sealed unsafe class FFmpegConverter : IDisposable
{
    private readonly IntPtr _convertedFrameBufferPtr;
    private readonly Size _destinationSize;
    private readonly byte_ptrArray4 _dstData;
    private readonly int_array4 _dstLinesize;
    private readonly SwsContext* _pConvertContext;

    public FFmpegConverter(Size sourceSize, 
        AVPixelFormat sourcePixelForamt,
        Size destinationSize,
        AVPixelFormat destinationPixelFormat)
    {
        this._destinationSize = destinationSize;
        this._pConvertContext = ffmpeg.sws_getContext(
            sourceSize.Width,
            sourceSize.Height,
            sourcePixelForamt,
            destinationSize.Width,
            destinationSize.Height,
            destinationPixelFormat,
            ffmpeg.SWS_FAST_BILINEAR,
            null,
            null,
            null);


        if (this._pConvertContext == null)
            throw new ApplicationException("Could not initialize the conversion context;");

        var convertedFrameBufferSize =
            ffmpeg.av_image_get_buffer_size(destinationPixelFormat, destinationSize.Width, destinationSize.Height, 1);
        this._convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
        this._dstData = new byte_ptrArray4();
        this._dstLinesize = new int_array4();

        ffmpeg.av_image_fill_arrays(ref _dstData,
            ref _dstLinesize,
            (byte*)_convertedFrameBufferPtr,
            destinationPixelFormat,
            destinationSize.Width,
            destinationSize.Height,
            1);
    }

    public AVFrame Convert(AVFrame sourceFrame)
    {
        ffmpeg.sws_scale(this._pConvertContext,
            sourceFrame.data,
            sourceFrame.linesize,
            0,
            sourceFrame.height,
            _dstData,
            _dstLinesize);

        var data = new byte_ptrArray8();
        data.UpdateFrom(_dstData);
        var linesize = new int_array8();
        linesize.UpdateFrom(_dstLinesize);
        return new AVFrame()
        {
            data = data,
            linesize = linesize,
            width = _destinationSize.Width,
            height = _destinationSize.Height
        };
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(_convertedFrameBufferPtr);
        ffmpeg.sws_freeContext(_pConvertContext);
    }
}