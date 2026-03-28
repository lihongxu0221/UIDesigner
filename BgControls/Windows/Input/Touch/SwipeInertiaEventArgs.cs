namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 惯性滑动事件处理程序委托.
/// </summary>
/// <param name="sender">事件发送者.</param>
/// <param name="args">惯性滑动事件参数.</param>
public delegate void SwipeInertiaEventHandler(object sender, SwipeInertiaEventArgs args);

/// <summary>
/// 惯性滑动事件参数类，包含惯性动画过程中的位置和位移信息.
/// </summary>
public class SwipeInertiaEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwipeInertiaEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识.</param>
    /// <param name="originalSource">事件的原始来源对象.</param>
    /// <param name="currentPosition">当前的坐标位置.</param>
    /// <param name="horizontalDelta">水平位移增量.</param>
    /// <param name="verticalDelta">垂直位移增量.</param>
    internal SwipeInertiaEventArgs(
        RoutedEvent routedEvent,
        object originalSource,
        Point currentPosition,
        double horizontalDelta,
        double verticalDelta)
        : base(routedEvent, originalSource)
    {
        // 检查原始来源对象是否为空.
        ArgumentNullException.ThrowIfNull(originalSource, nameof(originalSource));

        // 设置当前坐标位置.
        this.Position = currentPosition;
        // 设置水平位移量.
        this.HorizontalChange = horizontalDelta;
        // 设置垂直位移量.
        this.VerticalChange = verticalDelta;
    }

    /// <summary>
    /// Gets 惯性滑动的当前位置.
    /// </summary>
    public Point Position { get; }

    /// <summary>
    /// Gets 水平位移量.
    /// </summary>
    public double HorizontalChange { get; }

    /// <summary>
    /// Gets 垂直位移量.
    /// </summary>
    public double VerticalChange { get; }
}