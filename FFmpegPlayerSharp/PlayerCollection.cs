using System.Collections.Concurrent;
using FFmpegPlayerSharp.Base;
using FFmpegPlayerSharp.Core.Models;
using FFmpegPlayerSharp.Players;

namespace FFmpegPlayerSharp;

/// <summary>
/// 播放器管理类，使用单例模式
/// </summary>
public class PlayerCollection
{
    private static readonly object LockObj = new object();
    private static PlayerCollection _instnce;

    public static PlayerCollection Instance
    {
        get
        {
            if (_instnce == null)
            {
                lock (LockObj)
                {
                    if (_instnce == null)
                        _instnce = new PlayerCollection();
                }
            }

            return _instnce;
        }
    }

    /// <summary>
    /// 监控线程执行间隔
    /// </summary>
    private const ushort CheckTimerInterval = 1000;

    /// <summary>
    /// 播放器缓存
    /// </summary>
    private readonly ConcurrentDictionary<string, PlayerBase> _mPlayerCache;

    /// <summary>
    /// 监控线程运行状态
    /// </summary>
    private bool _mCheckThreadStatus = false;

    /// <summary>
    /// 运行时长（秒）
    /// </summary>
    private ulong _runningTime = 0;

    /// <summary>
    /// 监控线程
    /// </summary>
    private Thread _mCheckThread;

    private readonly ulong _playerMaxLiveTime = 3;

    private PlayerCollection()
    {
        _mPlayerCache = new ConcurrentDictionary<string, PlayerBase>();
        if (Entrance.PlayerMaxLiveTime > this._playerMaxLiveTime)
            this._playerMaxLiveTime = Entrance.PlayerMaxLiveTime;
    }

    internal void Init()
    {
        if (this._mCheckThreadStatus)
            return;
        this._mCheckThread = new Thread(new ThreadStart(this.Check));
        this._mCheckThread.IsBackground = true;
        this._mCheckThreadStatus = true;
        this._mCheckThread.Start();
    }

    private void Check()
    {
        while (this._mCheckThreadStatus)
        {
            try
            {
                _runningTime++;
                if (this._mPlayerCache.Count < 1)
                    continue;
                PlayerBase player = null;
                foreach (var path in this._mPlayerCache.Keys.ToList())
                {
                    if (!this._mPlayerCache.TryGetValue(path, out player) || player == null)
                        continue;
                    if (player.PlayerCount > 0)
                    {
                        player.RunTimers = _runningTime;
                        if (player.Status != Enums.PlayerStatus.Playing &&
                            player.Status != Enums.PlayerStatus.Disposing)
                            player.Play();
                        continue;
                    }

                    if (player.Status == Enums.PlayerStatus.Playing &&
                        (_runningTime - player.RunTimers) >= this._playerMaxLiveTime)
                    {
                        player.ChangeStatys(Enums.PlayerStatus.Disposing);
                        player.Dispose();
                    }
                }

                if (this._runningTime % 60 == 0)
                {
                    int mins = (int)(DateTime.Now - Entrance.SystemStartTime).TotalMinutes;
                    Entrance.Logger.Info($"系统运行时长：{mins} Min");
                }
            }
            catch (Exception e)
            {
                Entrance.Logger.Error(e);
            }
            finally
            {
                Thread.Sleep(CheckTimerInterval);
            }
        }
    }

    public PlayerBase GetPlayer(string rtsp)
    {
        PlayerBase player = null;
        lock (this._mPlayerCache)
        {
            if (!this._mPlayerCache.ContainsKey(rtsp))
            {
                player = CreatePlayer(rtsp);
                player.RunTimers = this._runningTime;
                this._mPlayerCache.TryAdd(rtsp, player);
            }
            else if (!this._mPlayerCache.TryGetValue(rtsp, out player))
            {
                return null;
            }

            player.AddPlayerCount();
        }

        return player;
    }

    public void ReleasePlayer(string rtsp)
    {
        lock (this._mPlayerCache)
        {
            if (!this._mPlayerCache.ContainsKey(rtsp) ||
                !this._mPlayerCache.TryGetValue(rtsp, out var player) ||
                player == null)
                return;
            player.RedPlayerCount();
        }
    }

    private static PlayerBase CreatePlayer(string rtsp)
    {
        return new FFmpegPlayer_WriteableBitmap(rtsp);
    }

    public void Dispose()
    {
        this._mCheckThreadStatus = false;
        this._mCheckThread.Join();
        lock (this._mPlayerCache)
        {
            foreach (var item in this._mPlayerCache)
            {
                item.Value.Dispose();
            }
            this._mPlayerCache.Clear();
        }
    }
}