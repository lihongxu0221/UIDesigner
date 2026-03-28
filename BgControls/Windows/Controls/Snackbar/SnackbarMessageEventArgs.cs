namespace BgControls.Windows.Controls;

/// <summary>
/// 消息条消息事件参数类.
/// </summary>
public class SnackbarMessageEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnackbarMessageEventArgs"/> class.
    /// </summary>
    /// <param name="message">消息条消息内容.</param>
    public SnackbarMessageEventArgs(SnackbarMessage message)
    {
        // 检查消息对象是否为空.
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        // 将消息实例赋值给属性.
        this.Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnackbarMessageEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件对象.</param>
    /// <param name="message">消息条消息内容.</param>
    public SnackbarMessageEventArgs(RoutedEvent routedEvent, SnackbarMessage message)
        : base(routedEvent)
    {
        // 检查消息对象是否为空.
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        // 将消息实例赋值给属性.
        this.Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnackbarMessageEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件对象.</param>
    /// <param name="source">事件源对象.</param>
    /// <param name="message">消息条消息内容.</param>
    public SnackbarMessageEventArgs(RoutedEvent routedEvent, object source, SnackbarMessage message)
        : base(routedEvent, source)
    {
        // 检查消息对象是否为空.
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        // 将消息实例赋值给属性.
        this.Message = message;
    }

    /// <summary>
    /// Gets 相关的消息条消息内容.
    /// </summary>
    public SnackbarMessage Message { get; }
}