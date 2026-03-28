namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 触控脉冲事件处理委托.
/// </summary>
/// <param name="sender">事件发送者.</param>
/// <param name="args">事件参数.</param>
internal delegate void TouchPulseEventHandler(object sender, TouchPulseEventArgs args);

/// <summary>
/// 触控脉冲事件参数类.
/// </summary>
internal class TouchPulseEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TouchPulseEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识.</param>
    /// <param name="pulseInfo">相关的脉冲信息对象.</param>
    public TouchPulseEventArgs(RoutedEvent routedEvent, object pulseInfo)
        : base(routedEvent)
    {
        // 使用 this 指示当前类的属性赋值.
        this.Info = pulseInfo;
    }

    /// <summary>
    /// Gets 相关的脉冲信息.
    /// </summary>
    public object Info { get; }
}