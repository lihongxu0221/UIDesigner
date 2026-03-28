namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 点击手势事件处理程序委托.
/// </summary>
/// <param name="sender">事件发送者.</param>
/// <param name="args">点击手势事件参数.</param>
public delegate void TapEventHandler(object sender, TapEventArgs args);

/// <summary>
/// 点击手势事件参数类，包含点击次数等信息.
/// </summary>
public class TapEventArgs : TouchEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TapEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识.</param>
    /// <param name="originalTouchArgs">原始触控事件参数.</param>
    /// <param name="tapCount">连续点击的次数.</param>
    internal TapEventArgs(RoutedEvent routedEvent, TouchEventArgs originalTouchArgs, int tapCount)
        : base(routedEvent, originalTouchArgs)
    {
        // 验证原始触控参数是否为空.
        ArgumentNullException.ThrowIfNull(originalTouchArgs, nameof(originalTouchArgs));

        // 记录点击次数.
        this.TapCount = tapCount;
    }

    /// <summary>
    /// Gets 点击次数.
    /// </summary>
    public int TapCount { get; }
}