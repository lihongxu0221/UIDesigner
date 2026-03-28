namespace BgControls.Windows.Controls.PropertyGrid.Commands;

/// <summary>
/// 表示与 PropertyItem 相关的命令.
/// </summary>
public static class PropertyItemCommands
{
    private static RoutedCommand resetValueCommand = new RoutedCommand();
    private static RoutedCommand copyValueCommand = new RoutedCommand();

    /// <summary>
    /// Gets 重置值的路由命令.
    /// </summary>
    public static RoutedCommand ResetValue
    {
        get
        {
            return resetValueCommand;
        }
    }

    /// <summary>
    /// Gets 复制当前值.
    /// </summary>
    public static RoutedCommand CopyValue
    {
        get
        {
            return copyValueCommand;
        }
    }
}