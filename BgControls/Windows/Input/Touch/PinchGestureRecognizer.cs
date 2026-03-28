using BgControls.Collections.Generic;

namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 捏合手势识别器类，用于识别和分发捏合（Pinch）手势事件.
/// </summary>
public class PinchGestureRecognizer : GestureRecognizerBase
{
    private WeakReferenceList<PinchEventHandler> pinchStartedHandlers;
    private WeakReferenceList<PinchEventHandler> pinchHandlers;
    private WeakReferenceList<PinchEventHandler> pinchFinishedHandlers;
    private PinchHelper? pinchHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchGestureRecognizer"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    public PinchGestureRecognizer(UIElement element)
        : base(element)
    {
        // 初始化事件处理程序的弱引用列表.
        this.pinchStartedHandlers = new WeakReferenceList<PinchEventHandler>();
        this.pinchHandlers = new WeakReferenceList<PinchEventHandler>();
        this.pinchFinishedHandlers = new WeakReferenceList<PinchEventHandler>();
    }

    /// <summary>
    /// 处理触控进入事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public override void OnTouchEnter(GestureRecognizerEventArgs args)
    {
        // 捏合手势通常不需要在进入时进行处理.
    }

    /// <summary>
    /// 处理触控按下事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public override void OnTouchDown(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        // 如果辅助工具尚未创建，则进行初始化.
        if (this.pinchHelper == null)
        {
            this.pinchHelper = new PinchHelper(this.Element);
        }

        // 如果辅助工具可以关联新的触控 ID（捏合通常支持两个点），则执行关联.
        if (this.pinchHelper.CanAssociateTouchId())
        {
            this.pinchHelper.AssociateTouchId(args.TouchId, args.Position, args.TouchEventArgs);
        }
    }

    /// <summary>
    /// 处理触控移动事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public override void OnTouchMove(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        // 尝试获取与当前触控 ID 关联的辅助工具并处理移动逻辑.
        this.TryGetPinchHelper(args.TouchId)?.OnTouchMove(args);
    }

    /// <summary>
    /// 处理触控抬起事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public override void OnTouchUp(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        // 处理捏合点抬起的清理和状态转换逻辑.
        this.TryGetPinchHelper(args.TouchId)?.OnTouchUp(args);
    }

    /// <summary>
    /// 处理触控离开元素事件.
    /// </summary>
    /// <param name="args">手势识别事件参数.</param>
    public override void OnTouchLeave(GestureRecognizerEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        // 判定是否需要由于触控离开而结束手势.
        this.TryGetPinchHelper(args.TouchId)?.OnTouchLeave(args);
    }

    /// <summary>
    /// 当请求停止所有手势识别时触发.
    /// </summary>
    public override void OnCeaseGesturesRequested()
    {
        // 强制停止当前的捏合手势.
        this.pinchHelper?.CeaseGesture();
    }

    /// <summary>
    /// 获取当前所有已注册事件处理程序的总数.
    /// </summary>
    /// <returns>处理程序总数.</returns>
    internal int GetTotalHandlersCount()
    {
        return this.pinchStartedHandlers.Count + this.pinchHandlers.Count + this.pinchFinishedHandlers.Count;
    }

    /// <summary>
    /// 添加捏合开始事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序委托.</param>
    internal void AddPinchStartedHandler(PinchEventHandler handler)
    {
        this.pinchStartedHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除捏合开始事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序委托.</param>
    internal void RemovePinchStartedHandler(PinchEventHandler handler)
    {
        this.pinchStartedHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoPinchHelperMaintenance();
    }

    /// <summary>
    /// 添加捏合过程中事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序委托.</param>
    internal void AddPinchHandler(PinchEventHandler handler)
    {
        this.pinchHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除捏合过程中事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序委托.</param>
    internal void RemovePinchHandler(PinchEventHandler handler)
    {
        this.pinchHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoPinchHelperMaintenance();
    }

    /// <summary>
    /// 添加捏合完成事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序委托.</param>
    internal void AddPinchFinishedHandler(PinchEventHandler handler)
    {
        this.pinchFinishedHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除捏合完成事件处理程序.
    /// </summary>
    /// <param name="handler">处理程序委托.</param>
    internal void RemovePinchFinishedHandler(PinchEventHandler handler)
    {
        this.pinchFinishedHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoPinchHelperMaintenance();
    }

    /// <summary>
    /// 尝试获取与指定触控 ID 关联的捏合辅助工具.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <returns>捏合辅助工具实例，若不匹配则返回 null.</returns>
    private PinchHelper? TryGetPinchHelper(int touchId)
    {
        // 检查辅助工具是否存在，且该触控点是否参与了当前的捏合逻辑.
        if (this.pinchHelper != null && this.pinchHelper.HasAssociatedTouchId(touchId))
        {
            return this.pinchHelper;
        }

        return null;
    }

    /// <summary>
    /// 执行捏合辅助工具的维护清理工作.
    /// </summary>
    private void DoPinchHelperMaintenance()
    {
        // 如果不再有任何事件订阅者，则强制结束当前手势.
        if (!this.HasGestureHandlers && this.pinchHelper != null)
        {
            this.pinchHelper.CeaseGesture();
        }
    }
}