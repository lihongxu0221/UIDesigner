namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 定义触控交互模式的位标志枚举.
/// </summary>
[Flags]
public enum TouchMode
{
    /// <summary>
    /// 无模式.
    /// </summary>
    None = 0,

    /// <summary>
    /// 命中测试可见.
    /// </summary>
    HitTestVisible = 1 << 0,

    /// <summary>
    /// 命中测试隐藏.
    /// </summary>
    HitTestHidden = 1 << 1,

    /// <summary>
    /// 已锁定交互.
    /// </summary>
    Locked = 1 << 2,
}