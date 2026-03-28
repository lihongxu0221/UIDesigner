namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 滚动视图辅助类，用于处理 UI 元素与 ScrollViewer 之间的触摸滚动关联逻辑.
/// </summary>
internal class ScrollViewerHelper
{
    private UIElement element;
    private ScrollViewer? scrollViewer;
    private Point initialPosition;
    private int initialVerticalOffset;
    private int initialHorizontalOffset;
    private bool isActive;
    private bool isReleaseScheduled;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollViewerHelper"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    public ScrollViewerHelper(UIElement element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        this.element = element;

        // 注册触控进入事件监听.
        TouchManager.AddTouchEnterEventHandler(this.element, this.OnTouchEnter);

        // 确保平滑滚动模式配置正确.
        this.EnsurePanningMode();
    }

    /// <summary>
    /// Gets a value indicating whether 是否支持惯性滚动.
    /// </summary>
    public virtual bool SupportsInertia => true;

    /// <summary>
    /// 获取指定 UI 元素关联的滚动视图实例.
    /// </summary>
    /// <param name="uiElement">目标 UI 元素.</param>
    /// <returns>返回滚动视图对象，若无法获取则返回 null.</returns>
    public virtual ScrollViewer? GetScrollViewer(UIElement uiElement)
    {
        return uiElement as ScrollViewer;
    }

    /// <summary>
    /// 释放资源并解绑所有触控事件.
    /// </summary>
    public void Dispose()
    {
        // 移除进入事件监听并解绑其他所有触控处理器.
        TouchManager.RemoveTouchEnterEventHandler(this.element, this.OnTouchEnter);
        this.DetachHandlers();
    }

    /// <summary>
    /// 响应触控进入事件.
    /// </summary>
    private void OnTouchEnter(object sender, TouchEventArgs args)
    {
        // 刷新处理程序绑定状态.
        this.DetachHandlers();
        this.AttachHandlers();
    }

    /// <summary>
    /// 响应触控离开事件.
    /// </summary>
    private void OnTouchLeave(object sender, TouchEventArgs args)
    {
        // 计划释放资源.
        this.ScheduleRelease();
    }

    /// <summary>
    /// 从元素中移除所有滑动手势和惯性相关的处理程序.
    /// </summary>
    private void DetachHandlers()
    {
        TouchManager.RemoveTouchLeaveEventHandler(this.element, this.OnTouchLeave);
        TouchManager.RemoveSwipeStartedEventHandler(this.element, this.OnSwipeStarted);
        TouchManager.RemoveSwipeEventHandler(this.element, this.OnSwipe);
        TouchManager.RemoveSwipeFinishedEventHandler(this.element, this.OnSwipeFinished);
        TouchManager.RemoveSwipeInertiaStartedEventHandler(this.element, this.OnSwipeInertiaStarted);
        TouchManager.RemoveSwipeInertiaEventHandler(this.element, this.OnSwipeInertia);
        TouchManager.RemoveSwipeInertiaFinishedEventHandler(this.element, this.OnSwipeInertiaFinished);
    }

    /// <summary>
    /// 为元素添加所有滑动手势和惯性相关的处理程序.
    /// </summary>
    private void AttachHandlers()
    {
        TouchManager.AddTouchLeaveEventHandler(this.element, this.OnTouchLeave);
        TouchManager.AddSwipeStartedEventHandler(this.element, this.OnSwipeStarted);
        TouchManager.AddSwipeEventHandler(this.element, this.OnSwipe);
        TouchManager.AddSwipeFinishedEventHandler(this.element, this.OnSwipeFinished);
        TouchManager.AddSwipeInertiaStartedEventHandler(this.element, this.OnSwipeInertiaStarted);
        TouchManager.AddSwipeInertiaEventHandler(this.element, this.OnSwipeInertia);
        TouchManager.AddSwipeInertiaFinishedEventHandler(this.element, this.OnSwipeInertiaFinished);
    }

    /// <summary>
    /// 响应滑动开始事件.
    /// </summary>
    private void OnSwipeStarted(object sender, TouchEventArgs args)
    {
        this.scrollViewer = this.GetScrollViewer(this.element);
        if (this.scrollViewer != null)
        {
            // 记录初始触控位置及滚动条偏移量.
            this.initialPosition = args.GetTouchPoint(this.scrollViewer).Position;
            this.initialVerticalOffset = (int)this.scrollViewer.VerticalOffset;
            this.initialHorizontalOffset = (int)this.scrollViewer.HorizontalOffset;
            this.isActive = true;
            args.Handled = true;
        }
    }

    /// <summary>
    /// 响应滑动移动事件.
    /// </summary>
    private void OnSwipe(object sender, SwipeEventArgs args)
    {
        if (this.scrollViewer != null)
        {
            // 执行位置更新.
            this.PerformSwipe(args.GetTouchPoint(this.scrollViewer).Position);
            args.Handled = true;
        }
    }

    /// <summary>
    /// 根据当前触控点计算并执行滚动位移.
    /// </summary>
    /// <param name="currentPosition">当前触控坐标点.</param>
    private void PerformSwipe(Point currentPosition)
    {
        // 计算垂直滚动位移，考虑视口高度与实际高度比例.
        double verticalRatio = this.scrollViewer!.ActualHeight / this.scrollViewer.ViewportHeight;
        int verticalDelta = (int)((this.initialPosition.Y - currentPosition.Y) / verticalRatio);
        this.scrollViewer.ScrollToVerticalOffset(this.initialVerticalOffset + verticalDelta);

        // 计算水平滚动位移，考虑视口宽度与实际宽度比例.
        double horizontalRatio = this.scrollViewer.ActualWidth / this.scrollViewer.ViewportWidth;
        int horizontalDelta = (int)((this.initialPosition.X - currentPosition.X) / horizontalRatio);
        this.scrollViewer.ScrollToHorizontalOffset(this.initialHorizontalOffset + horizontalDelta);
    }

    /// <summary>
    /// 响应滑动完成事件.
    /// </summary>
    private void OnSwipeFinished(object sender, TouchEventArgs args)
    {
        if (this.scrollViewer != null)
        {
            this.isActive = false;
            this.ScheduleRelease();
            args.Handled = true;
        }
    }

    /// <summary>
    /// 响应惯性滑动开始事件.
    /// </summary>
    private void OnSwipeInertiaStarted(object sender, RoutedEventArgs args)
    {
        // 检查是否支持惯性.
        if (this.scrollViewer != null && this.SupportsInertia)
        {
            this.isActive = true;
            args.Handled = true;
        }
    }

    /// <summary>
    /// 响应惯性滑动过程中的更新事件.
    /// </summary>
    private void OnSwipeInertia(object sender, SwipeInertiaEventArgs args)
    {
        if (this.scrollViewer != null)
        {
            this.PerformSwipe(args.Position);
            args.Handled = true;
        }
    }

    /// <summary>
    /// 响应惯性滑动结束事件.
    /// </summary>
    private void OnSwipeInertiaFinished(object sender, RoutedEventArgs args)
    {
        if (this.scrollViewer != null)
        {
            this.isActive = false;
            this.ScheduleRelease();
            args.Handled = true;
        }
    }

    /// <summary>
    /// 通过调度器异步计划释放关联资源.
    /// </summary>
    private void ScheduleRelease()
    {
        if (!this.isReleaseScheduled)
        {
            this.isReleaseScheduled = true;
            this.element.Dispatcher.BeginInvoke(() =>
            {
                this.isReleaseScheduled = false;
                this.Release();
            });
        }
    }

    /// <summary>
    /// 执行具体的释放逻辑，解绑处理器并清空引用.
    /// </summary>
    private void Release()
    {
        // 仅在当前不处于活动状态（无滑动或惯性）时执行解绑.
        if (!this.isActive)
        {
            this.DetachHandlers();
            this.scrollViewer = null;
        }
    }

    /// <summary>
    /// 确保滚动视图的默认平滑滚动模式处于关闭状态，以避免逻辑冲突.
    /// </summary>
    private void EnsurePanningMode()
    {
        ScrollViewer? viewer = this.GetScrollViewer(this.element);

        // 如果辅助类直接作用于 ScrollViewer 实例，则禁用其原生 PanningMode.
        if (viewer == this.element && viewer != null)
        {
            viewer.PanningMode = PanningMode.None;
        }
    }
}