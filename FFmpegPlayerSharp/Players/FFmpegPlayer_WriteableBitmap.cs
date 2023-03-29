using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using FFmpegPlayerSharp.Base;
using FFmpegPlayerSharp.Core.FFmpeg;
using FFmpegPlayerSharp.Core.Models;

namespace FFmpegPlayerSharp.Players;

internal class FFmpegPlayer_WriteableBitmap : FFmplayerBase
{
    private FFmpegConverter _converter;

    public FFmpegPlayer_WriteableBitmap(string url) : base(url)
    {
    }

    protected override void BeforRenderFrame()
    {
        // var framesize = this.Decoder.FrameSize;
        // if (framesize.Width == 0 || framesize.Height == 0)
        //     return;
        // var sourcePixelFormat = this.Decoder.CanHWDecoder ? Entrance.GetHWPixelFormat() : this.Decoder.PixelFormat;
        //
        // if (this._converter == null)
        // {
        //     if (framesize.Width <= 0 || framesize.Width <= 0)
        //         return;
        //     var frameSize = new Size(framesize.Width, framesize.Height);
        //     var destinationSize = frameSize;
        //     this._converter =
        //         new FFmpegConverter(frameSize,
        //             this.Decoder.PixelFormat,
        //             destinationSize,
        //             AVPixelFormat.AV_PIX_FMT_RGBA);
        // }
    }

    protected unsafe override void RenderFrame(AVFrame frame)
    {
        var pixelFormat = (AVPixelFormat)frame.format;
        // if (this._converter == null)
        // {
        //     this.BeforRenderFrame();
        //     return;
        // }

        if (pixelFormat == AVPixelFormat.AV_PIX_FMT_YUV420P)
        {
            var cacheModel = new FrameCacheModel();
            cacheModel.Data = frame.data;
            cacheModel.Width = frame.width;
            cacheModel.Height = frame.height;
            cacheModel.LineSizeArr = frame.linesize;
            this.FrameQueues.Enqueue(cacheModel);
            
        }

        // var converFrame = this._converter.Convert(frame);
        // FrameCacheModel cache = new FrameCacheModel();
        // cache.Width = converFrame.width;
        // cache.Height = converFrame.height;
        // unsafe
        // {
        //     cache.Data = converFrame.data;
        // }
        // cache.PicSize = converFrame.linesize[0] * converFrame.height;
        // cache.LineSize = converFrame.linesize[0];
        // cache.LineSizeArr = converFrame.linesize;
        // if (cache.PicSize == cache.Width * cache.Height * 4)
        // {
        //     this.FrameQueues.Enqueue(cache);
        // }
    }
}