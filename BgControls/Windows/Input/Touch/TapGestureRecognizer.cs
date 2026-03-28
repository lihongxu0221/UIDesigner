using BgControls.Collections.Generic;

namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 点击手势识别器，用于处理单击、多击以及点击并按住等交互逻辑.
/// </summary>
public class TapGestureRecognizer : GestureRecognizerBase
{
    private WeakReferenceList<TapEventHandler> tapHandlers;
    private WeakReferenceList<TouchEventHandler> tapAndHoldHandlers;
    private WeakReferenceList<TouchEventHandler> tapHoldAndReleaseHandlers;
    private TapHelper? tapHelper;
    private int tapCount;
    private Point lastTapPosition;
    private DateTime lastTapTime;
    private Window? window;

    /// <summary>
    /// Initializes a new instance of the <see cref="TapGestureRecognizer"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    internal TapGestureRecognizer(UIElement element)
        : base(element)
    {
        // 初始化弱引用处理程序列表.
        this.tapHandlers = new WeakReferenceList<TapEventHandler>();
        this.tapAndHoldHandlers = new WeakReferenceList<TouchEventHandler>();
        this.tapHoldAndReleaseHandlers = new WeakReferenceList<TouchEventHandler>();

        Dispatcher currentDispatcher = this.Element.Dispatcher;
        Action registerAction = new Action(() =>
        {
            // 在调度器中异步注册活动手势更改事件.
            GestureManager.AddActiveGestureChangedEventHandler(this.Element, this.OnActiveGestureChanged);
        });
        currentDispatcher.BeginInvoke(registerAction);
    }

    /// <summary>
    /// 当触控进入元素时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchEnter(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        VerifyElementsMismatch(this.Element, args.Element);
    }

    /// <summary>
    /// 当触控按下时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchDown(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        VerifyElementsMismatch(this.Element, args.Element);

        // 如果辅助工具为空或者触控 ID 匹配，则初始化点击辅助工具.
        if (this.tapHelper == null || this.tapHelper.TouchId == args.TouchId)
        {
            this.tapHelper = new TapHelper(args, this);
            this.SubscribeWindowDeactivated();
        }

        // 判断是否需要显示触控指示器（存在相关处理程序时显示）.
        bool shouldShowIndicator = this.tapAndHoldHandlers.Any(handler => handler != null) ||
                                   this.tapHoldAndReleaseHandlers.Any(handler => handler != null);

        this.tapHelper.OnTouchDown(args, shouldShowIndicator);
    }

    /// <summary>
    /// 当触控移动时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchMove(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        VerifyElementsMismatch(this.Element, args.Element);

        if (this.tapHelper != null && this.tapHelper.TouchId == args.TouchId)
        {
            this.tapHelper.OnTouchMove(args);
        }
    }

    /// <summary>
    /// 当触控抬起时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchUp(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        VerifyElementsMismatch(this.Element, args.Element);
        this.UnsubscribeWindowDeactivated();

        if (this.tapHelper != null && this.tapHelper.TouchId == args.TouchId)
        {
            this.tapHelper.OnTouchUp(args);
        }
    }

    /// <summary>
    /// 当触控离开元素时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchLeave(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        VerifyElementsMismatch(this.Element, args.Element);
        this.UnsubscribeWindowDeactivated();

        if (this.tapHelper != null && this.tapHelper.TouchId == args.TouchId)
        {
            this.tapHelper.OnTouchLeave();
        }
    }

    /// <summary>
    /// 当请求停止手势时触发.
    /// </summary>
    public override void OnCeaseGesturesRequested()
    {
        this.OnRootDeactivated();
    }

    /// <summary>
    /// 获取所有注册的处理程序总数.
    /// </summary>
    /// <returns>处理程序总数.</returns>
    internal int GetTotalHandlersCount()
    {
        return this.tapHandlers.Count + this.tapAndHoldHandlers.Count + this.tapHoldAndReleaseHandlers.Count;
    }

    /// <summary>
    /// 添加点击事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序.</param>
    internal void AddTapHandler(TapEventHandler handler)
    {
        this.tapHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除点击事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序.</param>
    internal void RemoveTapHandler(TapEventHandler handler)
    {
        this.tapHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoTapHelperMaintenance();
    }

    /// <summary>
    /// 添加点击并按住事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序.</param>
    internal void AddTapAndHoldHandler(TouchEventHandler handler)
    {
        this.tapAndHoldHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除点击并按住事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序.</param>
    internal void RemoveTapAndHoldHandler(TouchEventHandler handler)
    {
        this.tapAndHoldHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoTapHelperMaintenance();
    }

    /// <summary>
    /// 添加点击按住并抬起事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序.</param>
    internal void AddTapHoldAndReleaseHandler(TouchEventHandler handler)
    {
        this.tapHoldAndReleaseHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除点击按住并抬起事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序.</param>
    internal void RemoveTapHoldAndReleaseHandler(TouchEventHandler handler)
    {
        this.tapHoldAndReleaseHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoTapHelperMaintenance();
    }

    /// <summary>
    /// 释放指定的点击辅助工具实例.
    /// </summary>
    /// <param name="helper">点击辅助工具.</param>
    internal void ReleaseTapHelper(TapHelper helper)
    {
        if (this.tapHelper == helper)
        {
            this.tapHelper = null;
        }
    }

    /// <summary>
    /// 触发点击事件并处理多击计数逻辑.
    /// </summary>
    /// <param name="originalArgs">原始触控事件参数.</param>
    /// <param name="currentPosition">当前坐标点.</param>
    internal void RaiseTapEvent(TouchEventArgs originalArgs, Point currentPosition)
    {
        // 检查是否满足多击（双击等）条件.
        if (this.IsMultipleTap(currentPosition))
        {
            this.tapCount++;
        }
        else
        {
            this.tapCount = 1;
            this.lastTapPosition = currentPosition;
            this.lastTapTime = DateTime.Now;
        }

        // 调用管理器触发点击事件.
        TouchManager.RaiseTapEvent(this.Element, originalArgs, this.tapCount);
    }

    /// <summary>
    /// 判断是否满足多击的时间和距离阈值条件.
    /// </summary>
    /// <param name="currentPosition">当前位置.</param>
    /// <returns>如果是多击则返回 true; 否则返回 false.</returns>
    private bool IsMultipleTap(Point currentPosition)
    {
        double distance = MathUtilities.CalculateDistance(this.lastTapPosition, currentPosition);
        if (distance > TouchManager.MultiTapVicinity)
        {
            return false;
        }

        double elapsedMs = (DateTime.Now - this.lastTapTime).TotalMilliseconds;
        return elapsedMs <= (double)TouchManager.MultiTapTime;
    }

    /// <summary>
    /// 取消订阅窗口失去激活状态的事件.
    /// </summary>
    private void UnsubscribeWindowDeactivated()
    {
        if (this.window != null)
        {
            this.window.Deactivated -= this.Window_Deactivated;
            this.window = null;
        }
    }

    /// <summary>
    /// 订阅窗口失去激活状态的事件.
    /// </summary>
    private void SubscribeWindowDeactivated()
    {
        this.UnsubscribeWindowDeactivated();
        this.window = this.Element.GetParent<Window>();
        if (this.window != null)
        {
            this.window.Deactivated += this.Window_Deactivated;
        }
    }

    /// <summary>
    /// 窗口失去激活状态时的事件回调.
    /// </summary>
    private void Window_Deactivated(object? sender, EventArgs e)
    {
        this.OnRootDeactivated();
    }

    /// <summary>
    /// 根级 UI 停用时的清理逻辑.
    /// </summary>
    private void OnRootDeactivated()
    {
        this.tapHelper?.CeaseGesture();
    }

    /// <summary>
    /// 当活动手势发生改变时的处理逻辑.
    /// </summary>
    private void OnActiveGestureChanged(object? sender, EventArgs args)
    {
        string activeGestureName = GestureManager.GetActiveGesture(this.Element);

        // 如果当前活动手势变为非点击相关手势，则强制停止点击手势判定.
        if (activeGestureName != null &&
            activeGestureName != TouchManager.TapPendingGestureName &&
            activeGestureName != TouchManager.TapAndHoldGestureName &&
            activeGestureName != TouchManager.TapGestureName &&
            activeGestureName != TouchManager.TapHoldAndReleaseGestureName)
        {
            TouchManager.RaiseTouchPulseEvent(this.Element, new FinishTapGesturesForciblyPulse());
        }
    }

    /// <summary>
    /// 执行点击辅助工具的维护清理工作.
    /// </summary>
    private void DoTapHelperMaintenance()
    {
        // 如果不再有任何处理程序订阅，则停止当前手势辅助.
        if (!this.HasGestureHandlers && this.tapHelper != null)
        {
            this.tapHelper.CeaseGesture();
        }
    }
}