namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 文档触控处理器的抽象基类，定义了处理各类触控手势的接口和默认行为.
/// </summary>
internal abstract class DocumentTouchHandler
{
    /// <summary>
    /// Gets a value indicating whether 是否可以处理点击操作.
    /// </summary>
    public abstract bool CanTap { get; }

    /// <summary>
    /// Gets a value indicating whether 是否可以处理捏合操作.
    /// </summary>
    public abstract bool CanPinch { get; }

    /// <summary>
    /// Gets a value indicating whether 是否可以处理滑动操作.
    /// </summary>
    public abstract bool CanSwipe { get; }

    /// <summary>
    /// 根据触摸位置判断当前处理器是否应当处理该触摸操作.
    /// </summary>
    /// <param name="positions">触摸点的坐标集合.</param>
    /// <returns>如果应当处理则返回 true; 否则返回 false.</returns>
    public abstract bool ShouldHandleTouch(params Point[] positions);

    /// <summary>
    /// 处理点击手势事件.
    /// </summary>
    /// <param name="args">点击事件参数.</param>
    public virtual void Tap(TapEventArgs args)
    {
    }

    /// <summary>
    /// 处理捏合手势开始事件.
    /// </summary>
    /// <param name="args">捏合事件参数.</param>
    public virtual void PinchStarted(PinchEventArgs args)
    {
    }

    /// <summary>
    /// 处理捏合手势过程事件.
    /// </summary>
    /// <param name="args">捏合事件参数制.</param>
    public virtual void Pinch(PinchEventArgs args)
    {
    }

    /// <summary>
    /// 处理捏合手势结束事件.
    /// </summary>
    /// <param name="args">捏合事件参数.</param>
    public virtual void PinchFinished(PinchEventArgs args)
    {
    }

    /// <summary>
    /// 处理滑动手势开始事件.
    /// </summary>
    /// <param name="args">触控事件参数.</param>
    public virtual void SwipeStarted(TouchEventArgs args)
    {
    }

    /// <summary>
    /// 处理滑动手势过程事件.
    /// </summary>
    /// <param name="args">滑动事件参数.</param>
    public virtual void Swipe(SwipeEventArgs args)
    {
    }

    /// <summary>
    /// 处理滑动惯性开始事件.
    /// </summary>
    /// <param name="args">路由事件参数.</param>
    public virtual void SwipeInertiaStarted(RoutedEventArgs args)
    {
    }

    /// <summary>
    /// 处理滑动惯性过程事件.
    /// </summary>
    /// <param name="args">滑动惯性事件参数.</param>
    public virtual void SwipeInertia(SwipeInertiaEventArgs args)
    {
    }

    /// <summary>
    /// 处理滑动惯性结束事件.
    /// </summary>
    /// <param name="args">路由事件参数.</param>
    public virtual void SwipeInertiaFinished(RoutedEventArgs args)
    {
    }

    /// <summary>
    /// 处理滑动手势结束事件.
    /// </summary>
    /// <param name="args">触控事件参数.</param>
    public virtual void SwipeFinished(TouchEventArgs args)
    {
    }

    /// <summary>
    /// 处理点击按住并释放事件.
    /// </summary>
    /// <param name="args">触控事件参数.</param>
    public virtual void TapHoldAndRelease(TouchEventArgs args)
    {
    }
}