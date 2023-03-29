namespace FFmpegPlayerSharp.Helper;

public static class Expand
{
    #region 操作系统判断

    public static bool IsWindowsOS(this PlatformID id)
    {
        return id == PlatformID.Win32S || id == PlatformID.Win32Windows || id == PlatformID.Win32NT;
    }

    public static bool IsLinuxOS(this PlatformID id)
    {
        return id == PlatformID.Unix;
    }

    public static bool IsMacOS(this PlatformID id)
    {
        return id == PlatformID.MacOSX;
    }

    #endregion
 
}