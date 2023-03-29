using FFmpeg.AutoGen;
using FFmpegPlayerSharp.Core.FFmpeg;

namespace FFmpegPlayerSharp.Base;

internal class FFmplayerBase : PlayerBase
{
    private FFmpegDecoder _decoder;
    public FFmpegDecoder Decoder => this._decoder;

    private bool _decodeStatus;
    public bool DecodeStatus => this._decodeStatus;

    public FFmplayerBase(string url) : base(url)
    {
        this._decoder = new FFmpegDecoder(url);
    }

    protected override void BeginPlay()
    {
        try
        {
            this._decoder.ConnectStream();
            if (!this._decoder.IsOpend) return;
            this.BeforRenderFrame();
            this._decodeStatus = true;
            byte errorCount = 0;
            while (this._decodeStatus)
            {
                try
                {
                    #region 解码失败次数判断

                    if (!this._decoder.DecodeNextFrame(out var frame))
                    {
                        if (++errorCount >= 2)
                        {
                            Entrance.Logger.Warn($"{this.Url}解码异常！");
                            this._decodeStatus = false;
                        }

                        continue;
                    }

                    errorCount = 0;

                    #endregion

                    if (frame.width > 0 && frame.height > 0)
                        this.RenderFrame(frame);
                    else
                        Entrance.Logger.Warn($"{this.Url} Frame Wdith Or Height Is 0!");
                }
                catch (Exception e)
                {
                    Entrance.Logger.Error(e);
                }
            }

            this.AfterRenderFrame();
            this._decoder.Dispose();
        }
        catch (Exception e)
        {
            Entrance.Logger.Error(e);
        }
    }

    protected virtual void BeforRenderFrame()
    {
    }

    protected virtual void RenderFrame(AVFrame frame)
    {
    }

    protected virtual void AfterRenderFrame()
    {
    }

    public override void Dispose()
    {
        this._decodeStatus = false;
    }
}