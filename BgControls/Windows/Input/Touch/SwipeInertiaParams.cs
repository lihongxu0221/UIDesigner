namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 滑动惯性参数类，用于存储滑动操作中的速度和缓动函数信息.
/// </summary>
internal class SwipeInertiaParams
{
    private double horizontalSpeed;
    private double verticalSpeed;
    private EasingFunctionBase easingFunction;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwipeInertiaParams"/> class.
    /// </summary>
    /// <param name="horizontalSpeed">水平滑动速度.</param>
    /// <param name="verticalSpeed">垂直滑动速度.</param>
    /// <param name="easingFunction">缓动算法函数.</param>
    public SwipeInertiaParams(double horizontalSpeed, double verticalSpeed, EasingFunctionBase easingFunction)
    {
        // 检查缓动函数参数是否为空.
        ArgumentNullException.ThrowIfNull(easingFunction, nameof(easingFunction));

        // 将参数赋值给内部字段.
        this.horizontalSpeed = horizontalSpeed;
        this.verticalSpeed = verticalSpeed;
        this.easingFunction = easingFunction;
    }

    /// <summary>
    /// Gets 水平滑动速度.
    /// </summary>
    public double HorizontalSpeed => this.horizontalSpeed;

    /// <summary>
    /// Gets 垂直滑动速度.
    /// </summary>
    public double VerticalSpeed => this.verticalSpeed;

    /// <summary>
    /// Gets 缓动算法函数.
    /// </summary>
    public EasingFunctionBase EasingFunction => this.easingFunction;
}