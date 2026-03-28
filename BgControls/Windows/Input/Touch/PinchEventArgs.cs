namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 捏合手势事件处理程序委托.
/// </summary>
/// <param name="sender">事件发送者.</param>
/// <param name="args">捏合事件参数.</param>
public delegate void PinchEventHandler(object sender, PinchEventArgs args);

/// <summary>
/// 捏合手势事件参数类，包含两个触控点的信息及缩放因子.
/// </summary>
public class PinchEventArgs : RoutedEventArgs
{
    private BgControls.Windows.Input.Touch.TouchEventArgs touchArgs1;
    private BgControls.Windows.Input.Touch.TouchEventArgs touchArgs2;

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识.</param>
    /// <param name="firstTouchArgs">第一个触控点的原始事件参数.</param>
    /// <param name="secondTouchArgs">第二个触控点的原始事件参数.</param>
    /// <param name="scaleFactor">计算出的缩放因子.</param>
    internal PinchEventArgs(
        RoutedEvent routedEvent,
        BgControls.Windows.Input.Touch.TouchEventArgs firstTouchArgs,
        BgControls.Windows.Input.Touch.TouchEventArgs secondTouchArgs,
        double scaleFactor)
        : base(routedEvent, firstTouchArgs.OriginalSource)
    {
        // 验证触控参数是否为空.
        ArgumentNullException.ThrowIfNull(firstTouchArgs, nameof(firstTouchArgs));
        ArgumentNullException.ThrowIfNull(secondTouchArgs, nameof(secondTouchArgs));

        // 设置内部字段和属性值.
        this.touchArgs1 = firstTouchArgs;
        this.touchArgs2 = secondTouchArgs;
        this.Factor = scaleFactor;
    }

    /// <summary>
    /// Gets 捏合缩放因子.
    /// </summary>
    public double Factor { get; }

    /// <summary>
    /// 获取第一个触控点相对于指定元素的坐标信息.
    /// </summary>
    /// <param name="relativeTo">参考的输入元素.</param>
    /// <returns>第一个触控点位置信息.</returns>
    public TouchPoint GetTouchPoint1(IInputElement relativeTo)
    {
        // 调用内部存储的第一个触控参数获取点信息.
        return this.touchArgs1.GetTouchPoint(relativeTo);
    }

    /// <summary>
    /// 获取第二个触控点相对于指定元素的坐标信息.
    /// </summary>
    /// <param name="relativeTo">参考的输入元素.</param>
    /// <returns>第二个触控点位置信息.</returns>
    public TouchPoint GetTouchPoint2(IInputElement relativeTo)
    {
        // 调用内部存储的第二个触控参数获取点信息.
        return this.touchArgs2.GetTouchPoint(relativeTo);
    }
}