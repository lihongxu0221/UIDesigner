namespace BgControls.Windows.Controls;

/// <summary>
/// 指定当选项卡项超出可用布局空间时的处理模式.
/// </summary>
public enum TabOverflowMode
{
    /// <summary>
    /// 滚动模式. 选项卡保持在单行或单列中，并通过滚动行为进行访问.
    /// </summary>
    Scroll,

    /// <summary>
    /// 换行模式. 当空间不足时，选项卡将自动排列到新行或新列中.
    /// </summary>
    Wrap,
}