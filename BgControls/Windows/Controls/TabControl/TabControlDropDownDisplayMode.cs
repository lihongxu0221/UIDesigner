namespace BgControls.Windows.Controls;

/// <summary>
/// 指定选项卡控件（TabControl）中下拉菜单按钮的显示模式.
/// </summary>
public enum TabControlDropDownDisplayMode
{
    /// <summary>
    /// 始终隐藏并折叠下拉菜单按钮.
    /// </summary>
    Collapsed,

    /// <summary>
    /// 始终显示下拉菜单按钮.
    /// </summary>
    Visible,

    /// <summary>
    /// 仅在选项卡项超出可用显示区域并发生溢出时，才显示下拉菜单按钮.
    /// </summary>
    WhenNeeded,
}