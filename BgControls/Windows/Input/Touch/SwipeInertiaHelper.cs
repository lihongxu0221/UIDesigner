using BgControls.Collections.Generic;

namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 滑动惯性辅助类，负责计算和执行触控抬起后的惯性滚动逻辑.
/// </summary>
internal class SwipeInertiaHelper
{
    private static double oneSecondThreshold = 1000.0;

    private UIElement element;
    private Queue<Tuple<DateTime, Point>> positionTimeQueue;
    private Point position;
    private Point lastPosition;
    private DispatcherTimer? timer;
    private DateTime timerStartTime;
    private DateTime timerLastTickTime;
    private GestureDeactivationToken? gestureToken;
    private SwipeInertiaParams? inertiaParams;
    private ISwipeInertiaProvider inertiaProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwipeInertiaHelper"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    public SwipeInertiaHelper(UIElement element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        this.element = element;
        this.positionTimeQueue = new Queue<Tuple<DateTime, Point>>();
        this.inertiaProvider = new DefaultSwipeInertiaProvider();

        // 如果当前元素已经处于滑动惯性状态，则触发脉冲强制结束旧的惯性.
        if (GestureManager.GetActiveGesture(this.element) == TouchManager.SwipeInertiaGestureName)
        {
            TouchManager.RaiseTouchPulseEvent(this.element, new FinishSwipeInertiaPulse(this.element));
        }

        // 注册触控脉冲事件处理程序.
        TouchManager.AddTouchPulseEventHandler(this.element, this.OnTouchPulse);
    }

    /// <summary>
    /// Gets 速度计算的基准时长.
    /// </summary>
    public static double SpeedBaseDuration => 100.0;

    /// <summary>
    /// 当滑动开始时调用，记录初始位置和时间.
    /// </summary>
    /// <param name="swipePosition">滑动的起始位置.</param>
    public void OnSwipeStarted(Point swipePosition)
    {
        // 将初始位置和当前时间入队.
        this.positionTimeQueue.Enqueue(new Tuple<DateTime, Point>(DateTime.Now, swipePosition));
    }

    /// <summary>
    /// 当滑动进行中调用，持续记录轨迹点并清理过期数据.
    /// </summary>
    /// <param name="swipePosition">当前滑动的坐标点.</param>
    public void OnSwipe(Point swipePosition)
    {
        DateTime now = DateTime.Now;
        this.positionTimeQueue.Enqueue(new Tuple<DateTime, Point>(now, swipePosition));

        // 清理超过一秒的旧点，保持队列中至少有 2 个点.
        this.CleanUpPositionTimeQueue(now, oneSecondThreshold, 2);
    }

    /// <summary>
    /// 当滑动结束（手指抬起）时调用，开始惯性计算和动画.
    /// </summary>
    public void OnSwipeFinished()
    {
        // 轨迹点过少则不执行惯性.
        if (this.positionTimeQueue.Count < 2)
        {
            this.FinishInertia();
            return;
        }

        List<Tuple<DateTime, Point>> timePointsList = new List<Tuple<DateTime, Point>>(this.positionTimeQueue);
        this.position = timePointsList[timePointsList.Count - 1].Item2;

        // 通过提供者计算惯性参数.
        this.inertiaParams = this.inertiaProvider.GetInertiaParams(timePointsList);
        if (this.inertiaParams == null)
        {
            this.FinishInertia();
            return;
        }

        // 验证速度值的有效性.
        bool isSpeedValid = MathUtilities.IsValidNumber(this.inertiaParams.HorizontalSpeed) &&
                           MathUtilities.IsValidNumber(this.inertiaParams.VerticalSpeed);

        if (!(isSpeedValid && (this.inertiaParams.HorizontalSpeed != 0.0 || this.inertiaParams.VerticalSpeed != 0.0)) ||
            !GestureManager.CanActivateGesture(this.element, TouchManager.SwipeInertiaGestureName))
        {
            this.FinishInertia();
            return;
        }

        this.lastPosition = this.position;
        Action finishInertiaAction = new Action(this.FinishInertia);

        // 激活滑动惯性手势状态.
        this.gestureToken = GestureManager.ActivateGesture(this.element, TouchManager.SwipeInertiaGestureName, finishInertiaAction);

        TouchManager.RaiseSwipeInertiaStartedEvent(this.element, out var isHandled);
        if (isHandled)
        {
            this.StartInertiaTimer();
        }
        else
        {
            this.FinishInertia();
        }
    }

    /// <summary>
    /// 清理队列中超过指定时间阈值的坐标点.
    /// </summary>
    private void CleanUpPositionTimeQueue(DateTime now, double threshold, int minItemsCount)
    {
        while (this.positionTimeQueue.Count > minItemsCount)
        {
            Tuple<DateTime, Point> oldestPoint = this.positionTimeQueue.Peek();
            double elapsedMs = (now - oldestPoint.Item1).TotalMilliseconds;

            // 如果最老的点未超时，则停止清理.
            if (elapsedMs < threshold)
            {
                break;
            }

            this.positionTimeQueue.Dequeue();
        }
    }

    /// <summary>
    /// 启动惯性计时器.
    /// </summary>
    private void StartInertiaTimer()
    {
        this.timer = new DispatcherTimer();
        this.timer.Interval = TimeSpan.FromMilliseconds(1.0); // 设置极短的间隔以保证动画平滑度.
        this.timer.Tick += this.OnTimerTick;
        this.timerStartTime = DateTime.Now;
        this.timerLastTickTime = this.timerStartTime;
        this.timer.Start();
    }

    /// <summary>
    /// 惯性计时器触发逻辑，计算并分发惯性位移.
    /// </summary>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (this.timer == null || !this.timer.IsEnabled)
        {
            return;
        }

        DateTime now = DateTime.Now;
        double totalElapsedMs = (now - this.timerStartTime).TotalMilliseconds;

        // 如果惯性时长超过限制，则结束惯性.
        if (totalElapsedMs > (double)TouchManager.SwipeInertiaDuration)
        {
            this.FinishInertia();
            return;
        }

        // 计算当前进度和对应的缓动系数.
        double progress = totalElapsedMs / (double)TouchManager.SwipeInertiaDuration;
        double easeFactor = this.inertiaParams!.EasingFunction.Ease(1.0 - progress);

        double frameElapsedMs = (now - this.timerLastTickTime).TotalMilliseconds;
        this.timerLastTickTime = now;

        // 根据速度和时间增量计算位移.
        double horizontalDelta = (easeFactor * this.inertiaParams.HorizontalSpeed) * frameElapsedMs / SpeedBaseDuration;
        double verticalDelta = (easeFactor * this.inertiaParams.VerticalSpeed) * frameElapsedMs / SpeedBaseDuration;

        this.position = new Point(this.position.X + horizontalDelta, this.position.Y + verticalDelta);

        // 如果坐标发生了像素级的变化，则触发惯性事件.
        if ((int)this.position.X != (int)this.lastPosition.X || (int)this.position.Y != (int)this.lastPosition.Y)
        {
            TouchManager.RaiseSwipeInertiaEvent(this.element, this.position, this.position.X - this.lastPosition.X, this.position.Y - this.lastPosition.Y);
            this.lastPosition = this.position;
        }
    }

    /// <summary>
    /// 完成并停止惯性逻辑，执行资源清理.
    /// </summary>
    private void FinishInertia()
    {
        if (this.timer != null && this.timer.IsEnabled)
        {
            this.timer.Stop();
        }

        if (this.gestureToken != null)
        {
            this.gestureToken.DeactivateGesture();
            this.gestureToken = null;

            // 通知外部惯性阶段已结束.
            TouchManager.RaiseSwipeInertiaFinishedEvent(this.element);
        }

        // 移除脉冲事件监听.
        TouchManager.RemoveTouchPulseEventHandler(this.element, this.OnTouchPulse);
    }

    /// <summary>
    /// 响应触控脉冲事件.
    /// </summary>
    private void OnTouchPulse(object sender, TouchPulseEventArgs args)
    {
        // 检查是否为结束惯性的脉冲.
        if (args.Info is FinishSwipeInertiaPulse)
        {
            if (this.element != sender)
            {
                // 如果不是当前元素的脉冲，标记已处理（防止冒泡干扰）.
                args.Handled = true;
            }
            else
            {
                this.FinishInertia();
            }
        }
    }
}