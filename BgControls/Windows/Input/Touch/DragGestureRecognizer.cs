using BgControls.Collections.Generic;

namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 拖拽手势识别器，用于识别和分发拖拽相关的触控事件.
/// </summary>
public class DragGestureRecognizer : GestureRecognizerBase
{
    /// <summary>
    /// 存储拖拽开始事件处理程序的弱引用列表.
    /// </summary>
    private WeakReferenceList<TouchEventHandler> dragStartedHandlers;

    /// <summary>
    /// 存储拖拽进行中事件处理程序的弱引用列表.
    /// </summary>
    private WeakReferenceList<TouchEventHandler> dragHandlers;

    /// <summary>
    /// 存储拖拽完成事件处理程序的弱引用列表.
    /// </summary>
    private WeakReferenceList<TouchEventHandler> dragFinishedHandlers;

    /// <summary>
    /// 当前正在处理拖拽逻辑的辅助对象.
    /// </summary>
    private DragHelper? dragHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragGestureRecognizer"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    public DragGestureRecognizer(UIElement element)
        : base(element)
    {
        // 初始化各事件处理程序的弱引用集合.
        this.dragStartedHandlers = new WeakReferenceList<TouchEventHandler>();
        this.dragHandlers = new WeakReferenceList<TouchEventHandler>();
        this.dragFinishedHandlers = new WeakReferenceList<TouchEventHandler>();
    }

    /// <summary>
    /// 当触控点进入元素范围时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchEnter(GestureRecognizerEventArgs args)
    {
        // 验证当前元素与事件发送者是否匹配.
        VerifyElementsMismatch(this.Element, args.Element);
    }

    /// <summary>
    /// 当触控点在元素上按下时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchDown(GestureRecognizerEventArgs args)
    {
        // 验证元素匹配情况.
        VerifyElementsMismatch(this.Element, args.Element);

        // 如果当前没有辅助对象，或者触控 ID 匹配，则初始化拖拽辅助实例.
        if (this.dragHelper == null || this.dragHelper.TouchId == args.TouchId)
        {
            this.dragHelper = new DragHelper(args, this);
        }
    }

    /// <summary>
    /// 当触控点在元素上移动时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchMove(GestureRecognizerEventArgs args)
    {
        // 验证元素匹配情况.
        VerifyElementsMismatch(this.Element, args.Element);

        // 如果辅助实例存在且触控 ID 匹配，则处理移动逻辑.
        if (this.dragHelper != null && this.dragHelper.TouchId == args.TouchId)
        {
            this.dragHelper.OnTouchMove(args);
        }
    }

    /// <summary>
    /// 当触控点从元素上抬起时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchUp(GestureRecognizerEventArgs args)
    {
        // 验证元素匹配情况.
        VerifyElementsMismatch(this.Element, args.Element);

        // 如果辅助实例存在且触控 ID 匹配，则处理抬起逻辑.
        if (this.dragHelper != null && this.dragHelper.TouchId == args.TouchId)
        {
            this.dragHelper.OnTouchUp(args);
        }
    }

    /// <summary>
    /// 当触控点离开元素范围时触发.
    /// </summary>
    /// <param name="args">手势识别器事件参数.</param>
    public override void OnTouchLeave(GestureRecognizerEventArgs args)
    {
        // 验证元素匹配情况.
        VerifyElementsMismatch(this.Element, args.Element);

        // 如果辅助实例存在且触控 ID 匹配，则处理离开逻辑.
        if (this.dragHelper != null && this.dragHelper.TouchId == args.TouchId)
        {
            this.dragHelper.OnTouchLeave(args);
        }
    }

    /// <summary>
    /// 当请求停止所有手势识别时触发.
    /// </summary>
    public override void OnCeaseGesturesRequested()
    {
        // 如果拖拽辅助对象正在运行，强制停止它.
        if (this.dragHelper != null)
        {
            this.dragHelper.CeaseGesture();
        }
    }

    /// <summary>
    /// 获取当前识别器关联的所有事件处理程序的总数.
    /// </summary>
    /// <returns>处理程序总数.</returns>
    internal int GetTotalHandlersCount()
    {
        return this.dragStartedHandlers.Count + this.dragHandlers.Count + this.dragFinishedHandlers.Count;
    }

    /// <summary>
    /// 添加拖拽开始事件的处理程序.
    /// </summary>
    /// <param name="handler">事件处理委托.</param>
    internal void AddDragStartedHandler(TouchEventHandler handler)
    {
        this.dragStartedHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除拖拽开始事件的处理程序.
    /// </summary>
    /// <param name="handler">事件处理委托.</param>
    internal void RemoveDragStartedHandler(TouchEventHandler handler)
    {
        this.dragStartedHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoDragHelperMaintenance();
    }

    /// <summary>
    /// 添加拖拽进行中事件的处理程序.
    /// </summary>
    /// <param name="handler">事件处理委托.</param>
    internal void AddDragHandler(TouchEventHandler handler)
    {
        this.dragHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除拖拽进行中事件的处理程序.
    /// </summary>
    /// <param name="handler">事件处理委托.</param>
    internal void RemoveDragHandler(TouchEventHandler handler)
    {
        this.dragHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoDragHelperMaintenance();
    }

    /// <summary>
    /// 添加拖拽完成事件的处理程序.
    /// </summary>
    /// <param name="handler">事件处理委托.</param>
    internal void AddDragFinishedHandler(TouchEventHandler handler)
    {
        this.dragFinishedHandlers.Add(handler);
        this.HasGestureHandlers = true;
    }

    /// <summary>
    /// 移除拖拽完成事件的处理程序.
    /// </summary>
    /// <param name="handler">事件处理委托.</param>
    internal void RemoveDragFinishedHandler(TouchEventHandler handler)
    {
        this.dragFinishedHandlers.Remove(handler);
        this.HasGestureHandlers = this.GetTotalHandlersCount() > 0;
        this.DoDragHelperMaintenance();
    }

    /// <summary>
    /// 释放当前的拖拽辅助对象引用.
    /// </summary>
    /// <param name="helper">需要释放的辅助实例.</param>
    internal void ReleaseDragHelper(DragHelper helper)
    {
        // 仅在传入的对象是当前持有的实例时才重置字段.
        if (this.dragHelper == helper)
        {
            this.dragHelper = null;
        }
    }

    /// <summary>
    /// 当拖拽启动触发条件发生变更时通知辅助类.
    /// </summary>
    internal void OnDragStartTriggerChanged()
    {
        if (this.dragHelper != null)
        {
            this.dragHelper.OnDragStartTriggerChanged();
        }
    }

    /// <summary>
    /// 执行拖拽辅助对象的维护清理工作.
    /// </summary>
    private void DoDragHelperMaintenance()
    {
        // 如果当前没有任何手势处理程序且辅助对象存在，则停止该手势.
        if (!this.HasGestureHandlers && this.dragHelper != null)
        {
            this.dragHelper.CeaseGesture();
        }
    }
}