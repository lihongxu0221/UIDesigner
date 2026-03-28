namespace BgControls.Windows.Controls;

/// <summary>
/// 对话框关闭事件处理程序委托.
/// </summary>
/// <param name="sender">事件发送者.</param>
/// <param name="eventArgs">对话框关闭事件参数.</param>
public delegate void DialogClosedEventHandler(object sender, DialogClosedEventArgs eventArgs);

/// <summary>
/// 对话框关闭事件参数类.
/// </summary>
public class DialogClosedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogClosedEventArgs"/> class.
    /// </summary>
    /// <param name="session">对话框会话对象.</param>
    /// <param name="routedEvent">路由事件对象.</param>
    public DialogClosedEventArgs(DialogSession session, RoutedEvent routedEvent)
        : base(routedEvent)
    {
        // 检查会话对象是否为空.
        ArgumentNullException.ThrowIfNull(session, nameof(session));
        this.Session = session;
    }

    /// <summary>
    /// Gets 最初提供给 CloseDialogCommand 的参数.
    /// </summary>
    public object? Parameter => this.Session.CloseParameter;

    /// <summary>
    /// Gets 允许与当前对话框会话交互的会话对象.
    /// </summary>
    public DialogSession Session { get; }
}