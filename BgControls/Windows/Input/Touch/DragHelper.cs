using BgControls.Windows.Input.Touch;

namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 拖拽辅助类，负责处理触控拖拽的手势状态管理与逻辑识别.
/// </summary>
internal class DragHelper
{
    /// <summary>
    /// 定义拖拽状态的内部枚举.
    /// </summary>
    private enum DragState
    {
        /// <summary>
        /// 手势从未开始.
        /// </summary>
        NeverStarted,

        /// <summary>
        /// 无效状态.
        /// </summary>
        Invalid,

        /// <summary>
        /// 处于点击并按住状态.
        /// </summary>
        TapHold,

        /// <summary>
        /// 正在进行拖拽.
        /// </summary>
        Drag,

        /// <summary>
        /// 拖拽已完成.
        /// </summary>
        DragFinished,
    }

    private int touchId;
    private UIElement element;
    private DragGestureRecognizer recognizer;
    private Point touchDownPosition;
    private TouchEventArgs lastArgs;
    private bool hasTouchCapture;
    private DragState state;
    private GestureDeactivationToken? gestureToken;
    private TouchDragStartTrigger trigger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragHelper"/> class.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    /// <param name="recognizer">关联的拖拽手势识别器.</param>
    internal DragHelper(GestureRecognizerEventArgs args, DragGestureRecognizer recognizer)
    {
        // 验证参数非空.
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        ArgumentNullException.ThrowIfNull(recognizer, nameof(recognizer));

        this.element = args.Element;
        this.touchId = args.TouchId;
        this.touchDownPosition = args.Position;
        this.recognizer = recognizer;
        this.lastArgs = args.TouchEventArgs;

        // 初始化触发器类型.
        this.OnDragStartTriggerChanged();
    }

    /// <summary>
    /// Gets 触控 ID.
    /// </summary>
    internal int TouchId => this.touchId;

    /// <summary>
    /// 处理触控移动事件.
    /// </summary>
    /// <param name="args">事件参数.</param>
    internal void OnTouchMove(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        // 如果状态已失效则不处理.
        if (this.state == DragState.Invalid)
        {
            return;
        }

        this.lastArgs = args.TouchEventArgs;

        // 如果当前处于拖拽中，直接触发拖拽事件.
        if (this.state == DragState.Drag)
        {
            TouchManager.RaiseDragEvent(this.element, args.TouchEventArgs);
            args.Handled = true;
            return;
        }

        // 检查是否移动出了点击范围.
        bool isOutsideRect = !this.IsInsideDragRect(this.touchDownPosition, args.Position);
        if (isOutsideRect && ((this.state == DragState.NeverStarted && this.trigger == TouchDragStartTrigger.TouchMove) ||
                             (this.state == DragState.TapHold && this.trigger == TouchDragStartTrigger.TapHoldAndMove)))
        {
            // 满足触发条件，尝试开始拖拽.
            this.HandleDragStarting(args.TouchEventArgs, out bool handled);
            args.Handled = handled;
        }
        else if (isOutsideRect)
        {
            // 如果移动出范围但不满足开始条件，标记为无效.
            this.state = DragState.Invalid;
        }
    }

    /// <summary>
    /// 处理触控抬起事件.
    /// </summary>
    /// <param name="args">事件参数.</param>
    internal void OnTouchUp(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        this.lastArgs = args.TouchEventArgs;

        // 处理拖拽结束逻辑.
        this.HandleDragFinishing(args.TouchEventArgs, null, false, out bool gestureHandled);
        this.Release();
        args.Handled = gestureHandled;
    }

    /// <summary>
    /// 处理触控离开元素事件.
    /// </summary>
    /// <param name="args">事件参数.</param>
    internal void OnTouchLeave(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        this.lastArgs = args.TouchEventArgs;

        // 尝试捕获触控以维持拖拽，如果不成功则释放.
        this.HandleDragFinishing(args.TouchEventArgs, null, true, out bool _);
        if (!this.hasTouchCapture)
        {
            this.Release();
        }
    }

    /// <summary>
    /// 当拖拽启动触发方式发生变化时的处理逻辑.
    /// </summary>
    internal void OnDragStartTriggerChanged()
    {
        // 重新订阅或取消订阅长按事件.
        TouchManager.RemoveTapAndHoldEventHandler(this.element, this.OnTapAndHold);
        this.trigger = TouchManager.GetDragStartTrigger(this.element);
        if (this.trigger == TouchDragStartTrigger.TapHoldAndMove || this.trigger == TouchDragStartTrigger.TapAndHold)
        {
            TouchManager.AddTapAndHoldEventHandler(this.element, this.OnTapAndHold);
        }
    }

    /// <summary>
    /// 停止手势识别.
    /// </summary>
    internal void CeaseGesture()
    {
        this.HandleDragFinishing(null, null, false, out bool _);
        this.Release();
    }

    /// <summary>
    /// 判断当前坐标是否在点击有效的矩形范围内.
    /// </summary>
    /// <param name="originPosition">按下的位置.</param>
    /// <param name="currentPosition">当前位置.</param>
    /// <returns>在范围内返回 true. </returns>
    private bool IsInsideDragRect(Point originPosition, Point currentPosition)
    {
        // 计算两点之间的距离并与系统拖拽启动距离比较.
        double distance = BgControls.Windows.Input.Touch.MathUtilities.CalculateDistance(originPosition, currentPosition);
        return distance <= TouchManager.DragStartDistance;
    }

    /// <summary>
    /// 检查父级窗口是否处于活动状态.
    /// </summary>
    /// <param name="child">子元素.</param>
    /// <returns>活动状态返回 true. </returns>
    private bool IsParentWindowActive(UIElement child)
    {
        return child.GetParent<Window>()?.IsActive ?? true;
    }

    /// <summary>
    /// 响应长按（点击并按住）事件的回调.
    /// </summary>
    private void OnTapAndHold(object sender, TouchEventArgs args)
    {
        // 校验 TouchId 和当前状态.
        if (this.touchId == args.TouchDevice.Id && this.state == DragState.NeverStarted)
        {
            this.state = DragState.TapHold;
            this.lastArgs = args;

            // 如果触发器是长按即开始，则启动拖拽.
            if (this.trigger == TouchDragStartTrigger.TapAndHold)
            {
                this.HandleDragStarting(args, out bool _);
            }
        }
    }

    /// <summary>
    /// 处理拖拽开始的激活逻辑.
    /// </summary>
    /// <param name="touchEventArgs">触控参数.</param>
    /// <param name="handled">输出是否已处理.</param>
    private void HandleDragStarting(TouchEventArgs touchEventArgs, out bool handled)
    {
        // 询问手势管理器是否可以激活拖拽.
        if (GestureManager.CanActivateGesture(this.element, TouchManager.DragGestureName))
        {
            this.state = DragState.Drag;

            // 激活手势并获取停用令牌.
            this.gestureToken = GestureManager.ActivateGesture(this.element, TouchManager.DragGestureName, this.CeaseGesture);
            TouchManager.RaiseDragStartedEvent(this.element, touchEventArgs);
            this.HandleDragStarted(touchEventArgs);
            handled = touchEventArgs.Handled;
        }
        else
        {
            handled = false;
        }
    }

    /// <summary>
    /// 当拖拽成功启动后的后续处理.
    /// </summary>
    private void HandleDragStarted(TouchEventArgs args)
    {
        // 如果事件没有被处理，则尝试结束拖拽流程.
        if (!args.Handled)
        {
            this.HandleDragFinishing(args, null, false, out bool _);
        }
    }

    /// <summary>
    /// 尝试捕获触控设备.
    /// </summary>
    /// <param name="device">触控设备.</param>
    /// <returns>捕获成功返回 true. </returns>
    private bool TryCaptureTouch(TouchDevice device)
    {
        if (TouchManager.GetCaptured(device) == null)
        {
            if (!TouchManager.CaptureTouch(this.element, device))
            {
                return false;
            }

            this.hasTouchCapture = true;

            // 注册丢失捕获的处理程序.
            TouchManager.AddLostTouchCaptureHandler(this.element, this.OnLostTouchCapture);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 响应触控捕获丢失的回调.
    /// </summary>
    private void OnLostTouchCapture(object sender, TouchEventArgs lostTouchCaptureArgs)
    {
        TouchDevice device = lostTouchCaptureArgs.TouchDevice;
        if (this.touchId == device.Id)
        {
            this.lastArgs = lostTouchCaptureArgs;

            // 强制结束拖拽.
            this.HandleDragFinishing(null, lostTouchCaptureArgs, false, out bool _);
        }
    }

    /// <summary>
    /// 处理拖拽结束前的判定逻辑.
    /// </summary>
    private void HandleDragFinishing(TouchEventArgs? touchEventArgs, TouchEventArgs? lostTouchCaptureArgs, bool tryCaptureTouch, out bool gestureHandled)
    {
        bool canActivate = GestureManager.CanActivateGesture(this.element, TouchManager.DragGestureName);
        bool shouldTryCapture = tryCaptureTouch && (this.state == DragState.Drag || canActivate);

        // 判定是否应当通过捕获延长手势生命周期.
        if (shouldTryCapture && this.IsParentWindowActive(this.element) && touchEventArgs != null && this.TryCaptureTouch(touchEventArgs.TouchDevice))
        {
            gestureHandled = true;
            return;
        }

        // 执行结束流程.
        if (this.state == DragState.Drag)
        {
            this.FinishDrag(touchEventArgs, lostTouchCaptureArgs);
            gestureHandled = true;
        }
        else if (this.state == DragState.NeverStarted || this.state == DragState.TapHold)
        {
            this.state = DragState.Invalid;
            gestureHandled = false;
        }
        else
        {
            gestureHandled = false;
        }
    }

    /// <summary>
    /// 执行最终的拖拽完成逻辑并分发事件.
    /// </summary>
    private void FinishDrag(TouchEventArgs? touchEventArgs, TouchEventArgs? lostTouchCaptureArgs)
    {
        this.state = DragState.DragFinished;
        this.ReleaseTouchCapture(touchEventArgs, lostTouchCaptureArgs);

        // 停用手势令牌并触发完成事件.
        if (this.gestureToken != null)
        {
            this.gestureToken.DeactivateGesture();
            this.gestureToken = null;
        }

        TouchManager.RaiseDragFinishedEvent(this.element, touchEventArgs, lostTouchCaptureArgs, this.lastArgs);
    }

    /// <summary>
    /// 释放所有资源和事件订阅.
    /// </summary>
    private void Release()
    {
        TouchManager.RemoveTapAndHoldEventHandler(this.element, this.OnTapAndHold);
        this.ReleaseTouchCapture(null, null);

        if (this.recognizer != null)
        {
            this.recognizer.ReleaseDragHelper(this);
        }

        this.lastArgs = null;
    }

    /// <summary>
    /// 释放触控捕获.
    /// </summary>
    private void ReleaseTouchCapture(TouchEventArgs? touchEventArgs, TouchEventArgs? lostTouchCaptureArgs)
    {
        if (this.hasTouchCapture)
        {
            this.hasTouchCapture = false;
            TouchManager.RemoveLostTouchCaptureHandler(this.element, this.OnLostTouchCapture);

            // 获取对应的触控设备并释放.
            var touchDevice = TouchManager.GetTouchDevice(touchEventArgs, lostTouchCaptureArgs, this.lastArgs);
            if (touchDevice != null)
            {
                TouchManager.ReleaseTouchCapture(this.element, touchDevice);
            }
        }
    }
}