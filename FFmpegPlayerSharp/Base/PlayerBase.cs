using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FFmpeg.AutoGen;
using FFmpegPlayerSharp.Core.Models;

namespace FFmpegPlayerSharp.Base;

public abstract class PlayerBase : IDisposable
{
    #region Private Property

    /// <summary>
    /// 当前播放器复用路数，路数为0表示未被引用，需要被销毁
    /// </summary>
    private int mPlayerCount;

    /// <summary>
    /// 播放线程
    /// </summary>
    private Thread _playThread;

    #endregion

    #region Public Property

    public string Url { get; }

    public Enums.PlayerStatus Status { get; private set; }

    public int PlayerCount => this.mPlayerCount;

    public ulong RunTimers { get; set; }

    public readonly ConcurrentQueue<FrameCacheModel> FrameQueues;

    #endregion

    public PlayerBase(string url)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("Url is empty!");
        this.Url = url;
        this.FrameQueues = new ConcurrentQueue<FrameCacheModel>();
    }

    public void AddPlayerCount() => Interlocked.Increment(ref this.mPlayerCount);

    public void RedPlayerCount() => Interlocked.Decrement(ref this.mPlayerCount);

    public void Play()
    {
        if (this.Status != Enums.PlayerStatus.Disposed)
            return;
        this.Status = Enums.PlayerStatus.Playing;
        this._playThread = new Thread(new ThreadStart(() =>
        {
            try
            {
                this.BeginPlay();
            }
            catch (Exception e)
            {
               Entrance.Logger.Error(e);
            }

            this.Status = Enums.PlayerStatus.Disposed;
            this.FrameQueues.Clear();
        }));
        this._playThread.IsBackground = true;
        this._playThread.Start();
    }
    
    public virtual void Dispose()
    {
    }

    protected virtual void BeginPlay()
    {
        throw new NotImplementedException("未实现BeginPlay方法");
    }

    public void ChangeStatys(Enums.PlayerStatus status)
    {
        this.Status = status;
    }
}