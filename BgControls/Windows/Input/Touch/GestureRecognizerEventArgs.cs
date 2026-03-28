namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 手势识别器事件参数类，包含触控点、位置及处理状态信息.
/// </summary>
public class GestureRecognizerEventArgs : EventArgs
{
    /// <summary>
    /// 缓存的触控点信息.
    /// </summary>
    private TouchPoint? touchPoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="GestureRecognizerEventArgs"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    /// <param name="args">触控事件参数.</param>
    internal GestureRecognizerEventArgs(UIElement element, BgControls.Windows.Input.Touch.TouchEventArgs args)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        // 初始化内部字段.
        this.Element = element;
        this.TouchEventArgs = args;
    }

    /// <summary>
    /// Gets 关联的 UI 元素.
    /// </summary>
    public UIElement Element { get; }

    /// <summary>
    /// Gets 原始触控事件参数.
    /// </summary>
    public BgControls.Windows.Input.Touch.TouchEventArgs TouchEventArgs { get; }

    /// <summary>
    /// Gets 触控点信息.
    /// </summary>
    public TouchPoint TouchPoint
    {
        get
        {
            // 如果触控点尚未计算，则根据当前元素获取触控点.
            if (this.touchPoint == null)
            {
                this.touchPoint = this.TouchEventArgs.GetTouchPoint(this.Element);
            }

            return this.touchPoint;
        }
    }

    /// <summary>
    /// Gets 触控设备 ID.
    /// </summary>
    public int TouchId => this.TouchEventArgs.TouchDevice.Id;

    /// <summary>
    /// Gets 触控位置坐标.
    /// </summary>
    public Point Position => this.TouchPoint.Position;

    /// <summary>
    /// Gets or sets a value indicating whether 手势事件是否已处理.
    /// </summary>
    public bool Handled { get; set; }
}