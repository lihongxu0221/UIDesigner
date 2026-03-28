namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 点击并按住操作的工具类，用于管理触控指示器的状态和关联逻辑.
/// </summary>
internal static class TapAndHoldUtilities
{
    /// <summary>
    /// 触控指示器信息内部类，用于维护特定触控 ID 的计时器和视觉元素.
    /// </summary>
    private class TouchIndicatorInfo
    {
        private int touchId;
        private DispatcherTimer timer;
        private TouchIndicator touchIndicator;
        private List<UIElement> elements;
        private List<UIElement> elementsWithTouchIndicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchIndicatorInfo"/> class.
        /// </summary>
        /// <param name="touchId">触控 ID.</param>
        public TouchIndicatorInfo(int touchId)
        {
            // 初始化触控 ID 和相关组件.
            this.touchId = touchId;
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(TouchManager.TapTime);
            this.timer.Tick += this.OnTimerTick;
            this.touchIndicator = new TouchIndicator();
            this.elements = new List<UIElement>();
            this.elementsWithTouchIndicator = new List<UIElement>();
        }

        /// <summary>
        /// 添加 UI 元素与触控操作的关联.
        /// </summary>
        /// <param name="element">关联的 UI 元素.</param>
        /// <param name="showTouchIndicator">是否显示触控指示器.</param>
        /// <param name="position">显示位置.</param>
        public void AddAssociation(UIElement element, bool showTouchIndicator, Point position)
        {
            this.elements.Add(element);

            // 如果是第一个关联元素，则启动计时器.
            if (this.elements.Count == 1)
            {
                this.StartTimer();
            }

            // 如果需要显示指示器，则将其加入集合并显示.
            if (showTouchIndicator)
            {
                this.elementsWithTouchIndicator.Add(element);
                if (this.elementsWithTouchIndicator.Count == 1)
                {
                    this.ShowIndicator(element, position);
                }
            }
        }

        /// <summary>
        /// 移除 UI 元素与触控操作的关联.
        /// </summary>
        /// <param name="element">待移除的 UI 元素.</param>
        public void RemoveAssociation(UIElement element)
        {
            this.elements.Remove(element);

            // 如果没有关联元素了，停止点击并按住逻辑.
            if (this.elements.Count == 0)
            {
                CeaseTapAndHold(this.touchId);
                return;
            }

            this.elementsWithTouchIndicator.Remove(element);

            // 如果不再需要显示指示器，则隐藏它.
            if (this.elementsWithTouchIndicator.Count == 0)
            {
                this.HideIndicator();
            }
        }

        /// <summary>
        /// 停止所有逻辑并隐藏指示器.
        /// </summary>
        public void Cease()
        {
            this.StopTimer();
            this.HideIndicator();
        }

        /// <summary>
        /// 启动计时器.
        /// </summary>
        private void StartTimer()
        {
            this.timer.Start();
        }

        /// <summary>
        /// 停止计时器.
        /// </summary>
        private void StopTimer()
        {
            this.timer.Stop();
        }

        /// <summary>
        /// 显示触控指示器视觉效果.
        /// </summary>
        /// <param name="element">宿主元素.</param>
        /// <param name="position">坐标位置.</param>
        private void ShowIndicator(UIElement element, Point position)
        {
            this.touchIndicator.Show(element, position);
            this.touchIndicator.ShowTapAndHoldIndicator();
        }

        /// <summary>
        /// 隐藏触控指示器视觉效果.
        /// </summary>
        private void HideIndicator()
        {
            this.touchIndicator.Hide();
        }

        /// <summary>
        /// 计时器周期到达回调，触发触控脉冲事件.
        /// </summary>
        private void OnTimerTick(object? sender, EventArgs args)
        {
            this.StopTimer();

            // 如果仍有元素关联，则对首个元素触发脉冲.
            if (this.elements.Count > 0)
            {
                TouchManager.RaiseTouchPulseEvent(this.elements[0], new TapAndHoldTimerTickPulse(this.touchId));
            }
        }
    }

    /// <summary>
    /// 触控 ID 到指示器信息对象的映射字典.
    /// </summary>
    private static Dictionary<int, TouchIndicatorInfo> touchIdToTouchIndicatorInfo = new Dictionary<int, TouchIndicatorInfo>();

    /// <summary>
    /// 添加点击并按住的关联逻辑.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <param name="element">目标 UI 元素.</param>
    /// <param name="position">坐标位置.</param>
    /// <param name="showTouchIndicator">是否显示指示器.</param>
    public static void AddTapAndHoldAssociation(int touchId, UIElement element, Point position, bool showTouchIndicator)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        // 获取或创建信息对象并添加关联.
        TouchIndicatorInfo indicatorInfo = TapAndHoldUtilities.GetOrCreateTouchIndicatorInfo(touchId);
        indicatorInfo.AddAssociation(element, showTouchIndicator, position);
    }

    /// <summary>
    /// 移除点击并按住的关联逻辑.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <param name="element">目标 UI 元素.</param>
    public static void RemoveTapAndHoldAssociation(int touchId, UIElement element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        // 尝试获取并移除关联.
        TryGetTouchIndicatorInfo(touchId)?.RemoveAssociation(element);
    }

    /// <summary>
    /// 停止特定触控 ID 的点击并按住逻辑.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    public static void CeaseTapAndHold(int touchId)
    {
        var indicatorInfo = TryGetTouchIndicatorInfo(touchId);
        if (indicatorInfo != null)
        {
            // 执行清理操作并从静态字典中移除.
            indicatorInfo.Cease();
            touchIdToTouchIndicatorInfo.Remove(touchId);
        }
    }

    /// <summary>
    /// 获取现有的或创建新的触控指示器信息对象.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <returns>触控指示器信息实例.</returns>
    private static TouchIndicatorInfo GetOrCreateTouchIndicatorInfo(int touchId)
    {
        if (!TapAndHoldUtilities.touchIdToTouchIndicatorInfo.TryGetValue(touchId, out TouchIndicatorInfo? info))
        {
            info = new TouchIndicatorInfo(touchId);
            TapAndHoldUtilities.touchIdToTouchIndicatorInfo[touchId] = info;
        }

        return info;
    }

    /// <summary>
    /// 尝试获取与指定触控 ID 关联的指示器信息对象.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <returns>指示器信息实例，若不存在则返回 null.</returns>
    private static TouchIndicatorInfo? TryGetTouchIndicatorInfo(int touchId)
    {
        TapAndHoldUtilities.touchIdToTouchIndicatorInfo.TryGetValue(touchId, out TouchIndicatorInfo? info);
        return info;
    }
}