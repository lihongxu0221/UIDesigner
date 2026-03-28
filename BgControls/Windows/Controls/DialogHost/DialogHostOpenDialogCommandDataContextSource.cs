namespace BgControls.Windows.Controls;

/// <summary>
/// 定义在使用 <see cref="DialogHost.OpenDialogCommand"/> 且命令参数为 <see cref="FrameworkElement"/> 时，对话框数据上下文（DataContext）的来源方式.
/// </summary>
public enum DialogHostOpenDialogCommandDataContextSource
{
    /// <summary>
    /// 将发送者元素（通常是 <see cref="System.Windows.Controls.Button"/>）的数据上下文应用到对话框内容中.
    /// </summary>
    SenderElement,

    /// <summary>
    /// 将 <see cref="DialogHost"/> 实例自身的数据上下文应用到对话框内容中.
    /// </summary>
    DialogHostInstance,

    /// <summary>
    /// 数据上下文被显式设置为 <c>null</c>.
    /// </summary>
    None,
}