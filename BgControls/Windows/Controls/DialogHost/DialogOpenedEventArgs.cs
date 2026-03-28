namespace BgControls.Windows.Controls;

/// <summary>
/// 对话框打开事件处理程序委托.
/// </summary>
/// <param name="sender">事件发送者.</param>
/// <param name="eventArgs">对话框打开事件参数.</param>
public delegate void DialogOpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs);

/// <summary>
/// 对话框打开事件参数.
/// </summary>
public class DialogOpenedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogOpenedEventArgs"/> class.
    /// </summary>
    /// <param name="session">对话框会话对象.</param>
    /// <param name="routedEvent">路由事件对象.</param>
    public DialogOpenedEventArgs(DialogSession session, RoutedEvent routedEvent)
        : base(routedEvent)
    {
        // 检查对话框会话对象是否为空.
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        // 将对话框会话实例赋值给当前对象的属性.
        this.Session = session;
    }

    /// <summary>
    /// Gets 允许与当前对话框会话交互的对象.
    /// </summary>
    public DialogSession Session { get; }
}