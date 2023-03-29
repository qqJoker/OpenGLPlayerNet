using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using FFmpegPlayerSharp.Core.Models;

namespace FFmpegPlayerSharp.Core.FFmpeg;

internal sealed unsafe class FFmpegDecoder : IDisposable
{
    #region Private Property

    private string _url;
    private AVCodecContext* _codecContext;
    private AVFormatContext* _formatContext;
    private bool _isOpenStream = false;
    private int _streamIndex;
    private AVPacket* _packet;
    private AVFrame* _aFrame;
    private AVFrame* _pFrame;

    #endregion

    #region Public Property

    public bool CanHWDecoder { get; private set; }

    public bool IsOpend { get; private set; }

    public Size FrameSize { get; private set; }

    public AVPixelFormat PixelFormat { get; private set; }

    #endregion

    public FFmpegDecoder(string url)
    {
        this._url = url;
    }

    public void ConnectStream()
    {
        this._formatContext = ffmpeg.avformat_alloc_context();
        var formatContext = this._formatContext;
        int error = 0;

        #region avformat_open_input 打开视频输入流

        AVDictionary* avParamDic;
        ffmpeg.av_dict_set(&avParamDic, "rtsp_transport", "tcp", 0);
        ffmpeg.av_dict_set(&avParamDic, "stimeout", "2000000", 0);
        ffmpeg.av_dict_set(&avParamDic, "probesize", "409600", 0);

        error = ffmpeg.avformat_open_input(&formatContext, this._url, null, &avParamDic);
        if (error < 0)
        {
            error.ThrowExceptionIfError();
            ffmpeg.av_dict_free(&avParamDic);
            return;
        }

        ffmpeg.av_dict_free(&avParamDic);
        this._isOpenStream = true;

        #endregion

        #region avformat_find_stream_info 解析流信息

        error = ffmpeg.avformat_find_stream_info(this._formatContext, null);
        if (error < 0)
        {
            error.ThrowExceptionIfError();
            goto ClearCache;
        }

        #endregion

        #region 查找视频流

        this._streamIndex = -1;
        for (int i = 0; i < this._formatContext->nb_streams; i++)
        {
            if (this._formatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                this._streamIndex = i;
                break;
            }
        }

        if (this._streamIndex < 0)
        {
            Entrance.Logger.Warn("Not found Video Stream");
            goto ClearCache;
        }

        #endregion

        #region avcodec_alloc_context3 创建解码上下文

        this._codecContext = ffmpeg.avcodec_alloc_context3(null);
        if (this._codecContext == null)
            goto ClearCache;
        this._codecContext->thread_count = 1;

        #endregion

        #region avcodec_parameters_to_context 初始化解码上下文

        error = ffmpeg.avcodec_parameters_to_context(this._codecContext,
            this._formatContext->streams[this._streamIndex]->codecpar);
        if (error < 0)
        {
            error.ThrowExceptionIfError();
            goto ClearCache;
        }

        #endregion

        #region avcodec_find_decoder 查找解码器

        var dcodec = ffmpeg.avcodec_find_decoder(this._codecContext->codec_id);
        if (dcodec == null)
        {
            Entrance.Logger.Error($"Not fund HWDecoder Device :{this._codecContext->codec_id}");
            goto ClearCache;
        }

        #endregion

        #region 判断能否硬解以及硬解设备创建

        this.CanHWDecoder = this._codecContext->codec_id.CanHWDecode();
        if (this.CanHWDecoder)
        {
            error = ffmpeg.av_hwdevice_ctx_create(&this._codecContext->hw_device_ctx, Entrance.HWDeviceType, null, null,
                0);
            if (error < 0)
            {
                error.ThrowExceptionIfError();
                if (this._codecContext->hw_device_ctx != null)
                    ffmpeg.av_buffer_unref(&this._codecContext->hw_device_ctx);
                this.CanHWDecoder = false;
            }
        }

        #endregion

        #region avcodec_open2

        error = ffmpeg.avcodec_open2(this._codecContext, dcodec, null);
        if (error < 0)
        {
            error.ThrowExceptionIfError();
            goto ClearCache;
        }

        #endregion

        #region 初始化解码器上下文帧率

        this._codecContext->framerate = ffmpeg.av_guess_frame_rate(this._formatContext,
            this._formatContext->streams[this._streamIndex], null);

        #endregion

        this.FrameSize = new Size(this._codecContext->width, this._codecContext->height);
        this.PixelFormat = this._codecContext->pix_fmt;

        this._packet = ffmpeg.av_packet_alloc();
        this._aFrame = ffmpeg.av_frame_alloc();
        this._pFrame = ffmpeg.av_frame_alloc();

        this.IsOpend = true;
        return;
        
        ClearCache:
        Entrance.Logger.Warn("FFmpegDecoder.ConnectStream\tRTSP连接失败，进入销毁状态");
        this.Dispose();
    }

    public void Dispose()
    {
        try
        {
            this.IsOpend = false;
            if (this._aFrame != null)
            {
                ffmpeg.av_frame_unref(this._aFrame);
                var avFrame = this._aFrame;
                ffmpeg.av_frame_free(&avFrame);
                this._aFrame = null;
            }

            if (this._pFrame != null)
            {
                ffmpeg.av_frame_unref(this._pFrame);
                var pFrame = this._pFrame;
                ffmpeg.av_frame_free(&pFrame);
                this._pFrame = null;
            }

            if (this._packet != null)
            {
                ffmpeg.av_packet_unref(this._packet);
                var packet = this._packet;
                ffmpeg.av_packet_free(&packet);
                this._packet = null;
            }

            if (this._codecContext != null)
            {
                ffmpeg.avcodec_close(this._codecContext);
                if (this.CanHWDecoder)
                    ffmpeg.av_buffer_unref(&this._codecContext->hw_device_ctx);
                var codec = this._codecContext;
                ffmpeg.avcodec_free_context(&codec);
                this.CanHWDecoder = false;
                this._codecContext = null;
            }

            if (this._formatContext != null)
            {
                if (this._isOpenStream)
                {
                    var formatContext = this._formatContext;
                    ffmpeg.avformat_close_input(&formatContext);
                    this._isOpenStream = false;
                }

                this._formatContext = null;
            }
        }
        catch (Exception e)
        {
            Entrance.Logger.Error(e);
        }
    }

    /// <summary>
    /// 解码下一帧
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public bool DecodeNextFrame(out AVFrame frame)
    {
        frame = default;
        ffmpeg.av_frame_unref(this._pFrame);
        ffmpeg.av_frame_unref(this._aFrame);
        int error;
        do
        {
            try
            {
                do
                {
                    ffmpeg.av_packet_unref(this._packet);
                    if (!this.IsOpend) return false;
                    error = ffmpeg.av_read_frame(this._formatContext, this._packet);
                    if (error == ffmpeg.AVERROR_EOF || error < 0)
                    {
                        error.ThrowExceptionIfError();
                        return false;
                    }
                } while (this._packet->stream_index != this._streamIndex);

                if (!this.IsOpend || this._packet->data == null) return false;
                error = ffmpeg.avcodec_send_packet(this._codecContext, this._packet);
            }
            catch (Exception e)
            {
                Entrance.Logger.Error(e);
            }
            finally
            {
                ffmpeg.av_packet_unref(this._packet);
            }

            if (!this.IsOpend) return false;
            error = ffmpeg.avcodec_receive_frame(this._codecContext, this._pFrame);
        } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

        error.ThrowExceptionIfError();

        // if (Entrance.HWDeviceType == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE || !this.CanHWDecoder)
        // {
        //     ffmpeg.av_hwframe_transfer_data(this._aFrame, this._pFrame, 0).ThrowExceptionIfError();
        //     frame = *this._aFrame;
        // }
        // else
        //     frame = *this._pFrame;
        
        // ffmpeg.av_hwframe_transfer_data(this._aFrame, this._pFrame, 0).ThrowExceptionIfError();
        // frame = *this._aFrame;

        frame = *this._pFrame;

        return true;
    }
}