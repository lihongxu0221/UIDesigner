namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 滑动手势辅助类，用于处理滑动过程中的状态追踪、触控捕获及惯性触发逻辑.
/// </summary>
internal class SwipeHelper
{
    private int touchId;
    private Point? pivotPosition;
    private Point lastSwipePosition;
    private bool hasTouchCapture;
    private GestureState state;
    private SwipeInertiaHelper inertiaHelper;
    private UIElement element;
    private GestureDeactivationToken? gestureToken;
    private SwipeGestureRecognizer? recognizer;
    private BgControls.Windows.Input.Touch.TouchEventArgs? lastArgs;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwipeHelper"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    /// <param name="touchId">触控 ID.</param>
    /// <param name="recognizer">所属的滑动手势识别器.</param>
    public SwipeHelper(UIElement element, int touchId, SwipeGestureRecognizer recognizer)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        ArgumentNullException.ThrowIfNull(recognizer, nameof(recognizer));

        this.element = element;
        this.touchId = touchId;
        this.recognizer = recognizer;

        // 初始化惯性辅助工具.
        this.inertiaHelper = new SwipeInertiaHelper(this.element);
    }

    /// <summary>
    /// Gets 触控 ID.
    /// </summary>
    public int TouchId => this.touchId;

    /// <summary>
    /// 响应触控按下事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public void OnTouchDown(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        // 记录当前的事件参数和初始坐标点.
        this.lastArgs = args.TouchEventArgs;
        this.pivotPosition = args.Position;
    }

    /// <summary>
    /// 响应触控移动事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public void OnTouchMove(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        this.lastArgs = args.TouchEventArgs;

        // 如果初始点丢失，补偿记录当前点.
        if (!this.pivotPosition.HasValue)
        {
            this.pivotPosition = args.Position;
        }

        // 如果手势已处于激活状态，则计算位移并触发滑动事件.
        if (this.state == GestureState.Active)
        {
            TouchManager.RaiseSwipeEvent(this.element, args.TouchEventArgs, args.Position.X - this.lastSwipePosition.X, args.Position.Y - this.lastSwipePosition.Y);
            args.Handled = true;
            this.lastSwipePosition = args.Position;

            // 持续向惯性辅助工具喂入轨迹点.
            this.inertiaHelper.OnSwipe(this.lastSwipePosition);
        }
        else
        {
            // 尝试判定并启动滑动手势.
            this.HandleSwipeStarting(args);
        }
    }

    /// <summary>
    /// 响应触控抬起事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public void OnTouchUp(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        this.lastArgs = args.TouchEventArgs;

        // 处理手势结束逻辑.
        this.HandleSwipeFinishing(args.TouchEventArgs, null, tryCaptureTouch: false, isForcedFinish: false, out bool gestureHandled);

        this.Release();
        args.Handled = gestureHandled;
    }

    /// <summary>
    /// 响应触控离开元素事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public void OnTouchLeave(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        this.lastArgs = args.TouchEventArgs;

        // 尝试捕获触控以维持滑动状态.
        this.HandleSwipeFinishing(args.TouchEventArgs, null, tryCaptureTouch: true, isForcedFinish: false, out bool gestureHandled);

        // 如果没有成功捕获触控，则释放辅助对象.
        if (!this.hasTouchCapture)
        {
            this.Release();
        }

        args.Handled = gestureHandled;
    }

    /// <summary>
    /// 停止手势识别.
    /// </summary>
    public void CeaseGesture()
    {
        this.HandleSwipeFinishing(null, null, tryCaptureTouch: false, isForcedFinish: true, out bool _);
        this.Release();
    }

    /// <summary>
    /// 响应由于状态转换导致的手势完成回调.
    /// </summary>
    private void OnTransitionedFinish()
    {
        this.HandleSwipeFinishing(null, null, tryCaptureTouch: false, isForcedFinish: true, out bool _);
    }

    /// <summary>
    /// 处理滑动手势的启动判定.
    /// </summary>
    /// <param name="args">事件参数.</param>
    private void HandleSwipeStarting(GestureRecognizerEventArgs args)
    {
        bool canActivate = GestureManager.CanActivateGesture(this.element, TouchManager.SwipeGestureName);

        // 判定移动距离是否超过启动阈值且手势管理器允许激活.
        double distance = MathUtilities.CalculateDistance(this.pivotPosition!.Value, args.TouchPoint.Position);
        if (distance >= TouchManager.SwipeStartDistance && canActivate)
        {
            this.state = GestureState.Active;
            this.lastSwipePosition = args.Position;

            // 激活手势状态并绑定结束回调.
            this.gestureToken = GestureManager.ActivateGesture(this.element, TouchManager.SwipeGestureName, this.OnTransitionedFinish);
            TouchManager.RaiseSwipeStartedEvent(this.element, args.TouchEventArgs);

            args.Handled = true;

            // 通知惯性辅助类滑动已开始.
            this.inertiaHelper.OnSwipeStarted(this.lastSwipePosition);
        }
    }

    /// <summary>
    /// 处理滑动手势的结束逻辑.
    /// </summary>
    private void HandleSwipeFinishing(
        BgControls.Windows.Input.Touch.TouchEventArgs? touchEventArgs,
        BgControls.Windows.Input.Touch.TouchEventArgs? lostTouchCaptureArgs,
        bool tryCaptureTouch,
        bool isForcedFinish,
        out bool gestureHandled)
    {
        if (this.state == GestureState.Active)
        {
            // 如果需要尝试捕获触控，且捕获成功，则暂不结束手势.
            if (tryCaptureTouch && touchEventArgs != null && this.TryCaptureTouch(touchEventArgs.TouchDevice))
            {
                gestureHandled = true;
                return;
            }

            // 执行最终的滑动完成逻辑.
            this.FinishSwipe(touchEventArgs, lostTouchCaptureArgs, isForcedFinish);
            gestureHandled = true;
        }
        else
        {
            gestureHandled = false;
            this.pivotPosition = null;
        }
    }

    /// <summary>
    /// 尝试为当前元素捕获指定的触控设备.
    /// </summary>
    private bool TryCaptureTouch(TouchDevice device)
    {
        // 检查设备当前是否未被捕获.
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
    /// 处理丢失触控捕获的事件回调.
    /// </summary>
    private void OnLostTouchCapture(object sender, BgControls.Windows.Input.Touch.TouchEventArgs lostTouchCaptureArgs)
    {
        TouchDevice device = lostTouchCaptureArgs.TouchDevice;
        if (this.touchId == device.Id)
        {
            this.lastArgs = lostTouchCaptureArgs;

            // 触发完成逻辑（非强制结束）.
            this.HandleSwipeFinishing(null, lostTouchCaptureArgs, tryCaptureTouch: false, isForcedFinish: false, out bool _);
            this.ReleaseTouchCapture(device);
        }
    }

    /// <summary>
    /// 完成滑动逻辑并触发相应的事件和惯性处理.
    /// </summary>
    private void FinishSwipe(
        BgControls.Windows.Input.Touch.TouchEventArgs? touchEventArgs,
        BgControls.Windows.Input.Touch.TouchEventArgs? lostTouchCaptureArgs,
        bool isForcedFinish)
    {
        this.state = GestureState.Finished;

        // 确定用于释放捕获的触控设备.
        TouchDevice? deviceToRelease = this.hasTouchCapture ? TouchManager.GetTouchDevice(touchEventArgs, lostTouchCaptureArgs, this.lastArgs) : null;
        this.ReleaseTouchCapture(deviceToRelease);

        // 停用手势 Token.
        if (this.gestureToken != null)
        {
            this.gestureToken.DeactivateGesture();
            this.gestureToken = null;
        }

        // 触发滑动完成事件.
        TouchManager.RaiseSwipeFinishedEvent(this.element, touchEventArgs, lostTouchCaptureArgs, this.lastArgs);

        this.lastArgs = null;
        this.pivotPosition = null;

        // 如果不是强制结束（如由于焦点丢失），则触发惯性逻辑.
        if (!isForcedFinish)
        {
            this.inertiaHelper.OnSwipeFinished();
        }
    }

    /// <summary>
    /// 释放辅助类在识别器中的占用并解绑.
    /// </summary>
    private void Release()
    {
        if (this.recognizer != null)
        {
            this.recognizer.ReleaseHelper(this);
            this.recognizer = null;
        }
    }

    /// <summary>
    /// 释放元素的触控捕获并移除监听程序.
    /// </summary>
    private void ReleaseTouchCapture(TouchDevice? touchDevice)
    {
        if (this.hasTouchCapture)
        {
            this.hasTouchCapture = false;
            TouchManager.RemoveLostTouchCaptureHandler(this.element, this.OnLostTouchCapture);
            if (touchDevice != null)
            {
                TouchManager.ReleaseTouchCapture(this.element, touchDevice);
            }
        }
    }
}