using BgControls.Windows.Controls;
using BgControls.Windows.Input.Touch;

namespace BgControls.Windows.DragDrop;

/// <summary>
/// 拖拽操作管理类，负责处理拖拽生命周期中的交互、事件分发及视觉反馈.
/// </summary>
internal class DragOperation
{
    private DragEventArgs? lastDragEventArgs;
    private GiveFeedbackEventArgs? lastGiveFeedbackEventArgs;
    private QueryContinueDragEventArgs? lastQueryContinueDragEventArgs;
    private Visual? rootVisual;
    private DependencyObject? dragSource;
    private WeakReference? focusedElementWeakRef;
    private DependencyObject? lastDragOverElement;
    private DragDropEffects allowedEffects;
    private DragDropKeyStates keyStates;
    private bool escapePressed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragOperation"/> class.
    /// </summary>
    private DragOperation()
    {
        // 构造函数初始化逻辑.
    }

    /// <summary>
    /// Gets 拖拽辅助对象.
    /// </summary>
    internal DragHelper? DragHelper { get; private set; }

    /// <summary>
    /// Gets or sets 拖拽提示的偏移量.
    /// </summary>
    private Point DragCueOffset { get; set; }

    /// <summary>
    /// Gets or sets 拖拽期间的按键状态.
    /// </summary>
    private DragDropKeyStates KeyStates
    {
        get
        {
            // 获取当前键盘修饰键并更新状态位.
            ModifierKeys modifiers = Keyboard.Modifiers;
            if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                this.keyStates |= DragDropKeyStates.ControlKey;
            }
            else
            {
                this.keyStates &= ~DragDropKeyStates.ControlKey;
            }

            if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                this.keyStates |= DragDropKeyStates.ShiftKey;
            }
            else
            {
                this.keyStates &= ~DragDropKeyStates.ShiftKey;
            }

            if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                this.keyStates |= DragDropKeyStates.AltKey;
            }
            else
            {
                this.keyStates &= ~DragDropKeyStates.AltKey;
            }

            return this.keyStates;
        }

        set
        {
            this.keyStates = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether 当前是否处于触控拖拽状态.
    /// </summary>
    internal bool IsTouchDrag => DragDropManager.IsInTouchDrag;

    /// <summary>
    /// Gets or sets 最后一次触控的偏移位置.
    /// </summary>
    internal Point LastTouchOffset { get; set; }

    /// <summary>
    /// 获取指定拖拽源的根视觉元素.
    /// </summary>
    /// <param name="dragSource">拖拽源对象.</param>
    /// <returns>找到的根视觉元素; 否则返回 null. </returns>
    internal static Visual? GetRootVisual(DependencyObject dragSource)
    {
        ArgumentNullException.ThrowIfNull(dragSource, nameof(dragSource));

        DependencyObject current = dragSource;

        // 循环向上查找视觉树父级.
        while (current != null && VisualTreeHelper.GetParent(current) is Visual visual)
        {
            current = visual;
        }

        // 处理窗口内容的情况.
        if (current is Window window && window.Content is Visual contentVisual)
        {
            return contentVisual;
        }

        return current as Visual;
    }

    /// <summary>
    /// 执行拖拽放置操作.
    /// </summary>
    /// <param name="dragSource">拖拽源.</param>
    /// <param name="data">拖拽的数据.</param>
    /// <param name="allowedEffects">允许的拖拽效果.</param>
    /// <param name="initialKeyState">初始按键状态.</param>
    /// <param name="dragVisual">拖拽视觉对象.</param>
    /// <param name="relativeStartPoint">相对起始点.</param>
    /// <param name="dragVisualOffset">视觉对象偏移量.</param>
    /// <returns>返回创建的拖拽操作对象; 如果无法启动则返回 null. </returns>
    internal static DragOperation? DoDragDrop(DependencyObject dragSource, object data, DragDropEffects allowedEffects, DragDropKeyStates initialKeyState, object dragVisual, Point relativeStartPoint, Point dragVisualOffset)
    {
        ArgumentNullException.ThrowIfNull(dragSource, nameof(dragSource));

        DragOperation operation = new DragOperation();
        operation.dragSource = dragSource;
        operation.allowedEffects = allowedEffects;
        operation.KeyStates = initialKeyState;
        operation.rootVisual = DragOperation.GetRootVisual(dragSource);
        operation.lastDragEventArgs = new BgControls.Windows.DragDrop.DragEventArgs(allowedEffects, data, allowedEffects, null);

        // 配置辅助器.
        operation.DragHelper = new DragHelper();
        operation.DragHelper.DragVisualOffset = dragVisualOffset;
        operation.DragHelper.RelativeStartPoint = relativeStartPoint;
        operation.DragHelper.Content = dragVisual;
        operation.DragHelper.Show(dragSource);

        // 尝试捕获鼠标或进入触控拖拽模式.
        if (operation.DragHelper.CaptureMouse() || DragDropManager.IsInTouchDrag)
        {
            operation.DragHelper.MouseMove += operation.DragSourcePreviewMouseMove;
            operation.DragHelper.LostMouseCapture += operation.DragSourceLostMouseCapture;
            operation.DragHelper.MouseLeftButtonUp += operation.DragSourceMouseLeftButtonUp;
            operation.DragHelper.Focus();

            DragOperation.AddKeyboardHandler(operation);
            DragOperation? attemptResult = DragOperation.AttemptDrag(operation, data);

            if (attemptResult != null)
            {
                return attemptResult;
            }
        }
        else
        {
            operation.Cleanup();
        }

        return null;
    }

    /// <summary>
    /// 为拖拽操作添加键盘处理器.
    /// </summary>
    /// <param name="dragOperation">当前的拖拽操作对象.</param>
    private static void AddKeyboardHandler(DragOperation dragOperation)
    {
        IInputElement focusedElement = Keyboard.FocusedElement;

        // 如果没有焦点元素，默认使用主窗口.
        if (focusedElement == null)
        {
            focusedElement = Application.Current.MainWindow;
        }

        // 根据元素类型绑定预览按键事件.
        if (focusedElement is UIElement uiElement)
        {
            uiElement.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(dragOperation.OnDraggedElementKeyDown), handledEventsToo: true);
            uiElement.AddHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(dragOperation.OnDraggedElementKeyUp), handledEventsToo: true);
            dragOperation.focusedElementWeakRef = new WeakReference(uiElement);
        }
        else if (focusedElement is ContentElement contentElement)
        {
            contentElement.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(dragOperation.OnDraggedElementKeyDown), handledEventsToo: true);
            contentElement.AddHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(dragOperation.OnDraggedElementKeyUp), handledEventsToo: true);
            dragOperation.focusedElementWeakRef = new WeakReference(contentElement);
        }
        else if (focusedElement is UIElement3D uiElement3D)
        {
            uiElement3D.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(dragOperation.OnDraggedElementKeyDown), handledEventsToo: true);
            uiElement3D.AddHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(dragOperation.OnDraggedElementKeyUp), handledEventsToo: true);
            dragOperation.focusedElementWeakRef = new WeakReference(uiElement3D);
        }
    }

    /// <summary>
    /// 尝试启动拖拽流程并分发初始事件.
    /// </summary>
    /// <param name="dragOperation">拖拽操作实例.</param>
    /// <param name="dataObject">拖拽数据.</param>
    /// <returns>如果继续拖拽则返回操作实例; 否则返回 null. </returns>
    private static DragOperation? AttemptDrag(DragOperation dragOperation, object dataObject)
    {
        var e = new QueryContinueDragEventArgs(DragAction.Continue, dragOperation.escapePressed, dragOperation.KeyStates);
        dragOperation.RaiseQueryContinueDragEvent(e);

        // 处理初始状态下的取消或放下请求.
        if (e.Handled && e.Action == DragAction.Drop)
        {
            dragOperation.FinishDragOperation(null);
            return null;
        }

        if (e.Handled && e.Action == DragAction.Cancel)
        {
            dragOperation.RaiseDragDropCompleted(DragDropEffects.None, dataObject);
            dragOperation.Cleanup();
            return null;
        }

        if (dragOperation.lastDragEventArgs != null)
        {
            dragOperation.OnDragSourceGiveFeedback(dragOperation.lastDragEventArgs);
        }

        return dragOperation;
    }

    /// <summary>
    /// 处理默认的反馈逻辑.
    /// </summary>
    /// <param name="e">反馈事件参数.</param>
    private static void OnDefaultGiveFeedback(GiveFeedbackEventArgs e)
    {
        // 默认使用系统光标.
        e.UseDefaultCursors = true;
    }

    /// <summary>
    /// 执行拖拽悬停检查逻辑.
    /// </summary>
    /// <param name="currentTarget">当前命中的目标元素.</param>
    private void DoDragOver(DependencyObject? currentTarget)
    {
        if (this.lastQueryContinueDragEventArgs == null)
        {
            return;
        }

        bool isHandled = this.lastQueryContinueDragEventArgs.Handled;
        DragAction action = this.lastQueryContinueDragEventArgs.Action;

        // 如果状态不是继续，则结束拖拽.
        if (isHandled && action != DragAction.Continue)
        {
            this.FinishDragOperation(currentTarget);
        }
        else if (currentTarget != null)
        {
            this.OnDragOver(currentTarget);
        }
    }

    /// <summary>
    /// 处理按键按下事件（主要捕获 ESC 取消操作）.
    /// </summary>
    /// <param name="sender">事件源.</param>
    /// <param name="e">按键参数.</param>
    private void OnDraggedElementKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.escapePressed = true;
        }

        this.DoDragOver(this.lastDragOverElement);
        if (this.lastDragEventArgs != null)
        {
            this.RaiseDragSourceEvents(this.lastDragEventArgs);
        }

        e.Handled = true;
    }

    /// <summary>
    /// 处理按键抬起事件.
    /// </summary>
    /// <param name="sender">事件源.</param>
    /// <param name="e">按键参数.</param>
    private void OnDraggedElementKeyUp(object sender, KeyEventArgs e)
    {
        this.DoDragOver(this.lastDragOverElement);
        if (this.lastDragEventArgs != null)
        {
            this.RaiseDragSourceEvents(this.lastDragEventArgs);
        }

        e.Handled = true;
        if (e.Key == Key.Escape)
        {
            this.RemoveHandlersAndHideDragHelper();
            this.OnCancel();
            this.Cleanup();
        }
    }

    /// <summary>
    /// 鼠标左键抬起时的回调.
    /// </summary>
    private void DragSourceMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        this.OnMouseFinishDrag(e);
    }

    /// <summary>
    /// 丢失鼠标捕获时的回调.
    /// </summary>
    private void DragSourceLostMouseCapture(object sender, MouseEventArgs e)
    {
        this.OnMouseFinishDrag(e);
    }

    /// <summary>
    /// 结束由鼠标驱动的拖拽.
    /// </summary>
    /// <param name="e">鼠标参数.</param>
    private void OnMouseFinishDrag(MouseEventArgs e)
    {
        if (this.DragHelper != null)
        {
            this.DragHelper.LostMouseCapture -= this.DragSourceLostMouseCapture;
        }

        Point absolutePosition = this.GetOffset(e);
        DependencyObject? currentTarget = this.GetCurrentTarget(absolutePosition);
        this.FinishDragOperation(currentTarget);
    }

    /// <summary>
    /// 鼠标移动时的预览处理.
    /// </summary>
    private void DragSourcePreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (DragDropManager.IsDragInProgress)
        {
            Point absolutePosition = this.GetOffset(e);
            this.OnMove(absolutePosition);
        }
    }

    /// <summary>
    /// 执行移动逻辑，包括视觉辅助移动和事件分发.
    /// </summary>
    /// <param name="absolutePosition">全局坐标位置.</param>
    private void OnMove(Point absolutePosition)
    {
        if (this.DragHelper != null)
        {
            this.DragHelper.Move(absolutePosition.X, absolutePosition.Y);
        }

        if (this.lastDragEventArgs != null)
        {
            this.RaiseDragSourceEvents(this.lastDragEventArgs);
        }

        DependencyObject? currentTarget = this.GetCurrentTarget(absolutePosition);
        this.DoDragOver(currentTarget);
    }

    /// <summary>
    /// 获取当前鼠标相对于根视图的偏移量.
    /// </summary>
    /// <param name="e">鼠标参数.</param>
    /// <returns>返回相对于屏幕根节点的坐标点. </returns>
    private Point GetOffset(MouseEventArgs e)
    {
        if (this.lastDragEventArgs != null)
        {
            this.lastDragEventArgs.MouseEventArgs = e;
        }

        return e.GetPosition(null);
    }

    /// <summary>
    /// 结束拖拽操作，清理资源并执行 Drop 或 Cancel 逻辑.
    /// </summary>
    /// <param name="currentTarget">当前放置目标.</param>
    private void FinishDragOperation(DependencyObject? currentTarget)
    {
        DragAction finalAction = this.OnDefaultQueryContinueDrag(leftButtonIsUp: true, this.escapePressed);
        this.KeyStates &= ~DragDropKeyStates.LeftMouseButton;

        if (DragDropManager.IsDragInProgress)
        {
            if (this.lastDragEventArgs != null)
            {
                this.RaiseDragSourceEvents(this.lastDragEventArgs);
            }
        }
        else
        {
            if (this.lastDragEventArgs != null)
            {
                this.OnDragSourceGiveFeedback(this.lastDragEventArgs);
            }
        }

        this.RemoveHandlersAndHideDragHelper();

        if (finalAction != DragAction.Cancel && currentTarget != null)
        {
            this.lastDragOverElement = currentTarget;
            this.OnDrop(currentTarget);
        }
        else
        {
            this.OnCancel();
        }

        this.Cleanup();
    }

    /// <summary>
    /// 清理拖拽操作占用的所有资源.
    /// </summary>
    private void Cleanup()
    {
        this.RemoveHandlersAndHideDragHelper();
        if (this.DragHelper != null)
        {
            this.DragHelper.Clear();
            this.DragHelper = null;
        }

        this.rootVisual = null;
        this.dragSource = null;
        this.lastDragEventArgs = null;
        this.lastDragOverElement = null;
        this.lastGiveFeedbackEventArgs = null;
        this.LastTouchOffset = new Point(double.NaN, double.NaN);
        DragDropManager.FinishDrag();
    }

    /// <summary>
    /// 移除事件处理器并隐藏拖拽视觉辅助器.
    /// </summary>
    private void RemoveHandlersAndHideDragHelper()
    {
        this.RemoveKeyHandlers();
        if (this.DragHelper != null)
        {
            this.DragHelper.MouseMove -= this.DragSourcePreviewMouseMove;
            this.DragHelper.LostMouseCapture -= this.DragSourceLostMouseCapture;
            this.DragHelper.MouseLeftButtonUp -= this.DragSourceMouseLeftButtonUp;
            this.DragHelper.ReleaseMouseCapture();
            this.DragHelper.Hide();
        }
    }

    /// <summary>
    /// 移除键盘预览事件挂钩.
    /// </summary>
    private void RemoveKeyHandlers()
    {
        IInputElement? targetElement = null;
        if (this.focusedElementWeakRef != null && this.focusedElementWeakRef.IsAlive)
        {
            targetElement = this.focusedElementWeakRef.Target as IInputElement;
        }

        if (targetElement is UIElement ui)
        {
            ui.RemoveHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(this.OnDraggedElementKeyDown));
            ui.RemoveHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(this.OnDraggedElementKeyUp));
        }
        else if (targetElement is ContentElement ce)
        {
            ce.RemoveHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(this.OnDraggedElementKeyDown));
            ce.RemoveHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(this.OnDraggedElementKeyUp));
        }

        this.focusedElementWeakRef = null;
    }

    /// <summary>
    /// 触发拖拽源反馈和状态查询事件.
    /// </summary>
    /// <param name="args">拖拽参数.</param>
    private void RaiseDragSourceEvents(BgControls.Windows.DragDrop.DragEventArgs args)
    {
        this.OnDragSourceGiveFeedback(args);
        this.RaiseQueryContinueDragEvent(new QueryContinueDragEventArgs(DragAction.Continue, this.escapePressed, this.KeyStates));
    }

    /// <summary>
    /// 执行给反馈操作，决定光标和效果样式.
    /// </summary>
    /// <param name="args">拖拽参数.</param>
    private void OnDragSourceGiveFeedback(BgControls.Windows.DragDrop.DragEventArgs args)
    {
        DragDropEffects effects = this.allowedEffects;
        effects = args.Handled ?
            (this.allowedEffects & args.Effects) :
            (this.lastDragOverElement != null ?
                 this.GetDragDropEffects() :
                 DragDropEffects.None);

        this.lastGiveFeedbackEventArgs = new GiveFeedbackEventArgs(effects, useDefaultCursors: false);
        this.RaiseGiveFeedbackEvent(this.lastGiveFeedbackEventArgs);
    }

    /// <summary>
    /// 根据当前按键状态计算推荐的拖拽效果.
    /// </summary>
    /// <returns>返回计算后的 <see cref="DragDropEffects"/>. </returns>
    private DragDropEffects GetDragDropEffects()
    {
        DragDropKeyStates currentKeyStates = this.KeyStates;
        if ((currentKeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey)
        {
            if ((currentKeyStates & DragDropKeyStates.ShiftKey) == DragDropKeyStates.ShiftKey)
            {
                return (this.allowedEffects & DragDropEffects.Link) == DragDropEffects.Link ? DragDropEffects.Link : DragDropEffects.None;
            }

            return (this.allowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy ? DragDropEffects.Copy : DragDropEffects.None;
        }

        if ((this.allowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
        {
            return DragDropEffects.Move;
        }

        return this.allowedEffects;
    }

    /// <summary>
    /// 触发反馈路由事件并更新视觉辅助器.
    /// </summary>
    /// <param name="args">反馈事件参数.</param>
    private void RaiseGiveFeedbackEvent(GiveFeedbackEventArgs args)
    {
        try
        {
            args.RoutedEvent = DragDropManager.PreviewGiveFeedbackEvent;
            this.dragSource?.RaiseEvent(args);
            args.RoutedEvent = DragDropManager.GiveFeedbackEvent;
            if (!args.Handled)
            {
                this.dragSource?.RaiseEvent(args);
            }

            if (!args.Handled)
            {
                DragOperation.OnDefaultGiveFeedback(args);
            }

            if (this.DragHelper != null && this.lastGiveFeedbackEventArgs != null)
            {
                this.DragHelper.DragCueOffset = this.DragCueOffset;
                this.DragHelper.UpdateCursorAndEffects(this.lastGiveFeedbackEventArgs.Effects, args.Cursor, args.UseDefaultCursors);
            }
        }
        catch
        {
            this.Cleanup();
            throw;
        }
    }

    /// <summary>
    /// 触发状态查询事件，决定是否中断、放置或继续拖拽.
    /// </summary>
    /// <param name="args">查询继续拖拽的参数.</param>
    private void RaiseQueryContinueDragEvent(QueryContinueDragEventArgs args)
    {
        try
        {
            DragCuePositionEventArgs cueArgs = new DragCuePositionEventArgs(DragDropManager.DragCuePositionEvent);
            this.dragSource?.RaiseEvent(cueArgs);
            this.DragCueOffset = cueArgs.DragCueOffset;

            args.RoutedEvent = DragDropManager.PreviewQueryContinueDragEvent;
            this.dragSource?.RaiseEvent(args);
            args.RoutedEvent = DragDropManager.QueryContinueDragEvent;
            if (!args.Handled)
            {
                this.dragSource?.RaiseEvent(args);
            }

            if (!args.Handled)
            {
                bool leftButtonIsUp = (this.KeyStates & DragDropKeyStates.LeftMouseButton) != DragDropKeyStates.LeftMouseButton;
                args.Action = this.OnDefaultQueryContinueDrag(leftButtonIsUp, this.escapePressed);
            }

            this.lastQueryContinueDragEventArgs = args;
            DragDropManager.LastQueryContinueAction = args.Action;
        }
        catch
        {
            this.Cleanup();
            throw;
        }
    }

    /// <summary>
    /// 执行默认的状态查询逻辑.
    /// </summary>
    /// <param name="leftButtonIsUp">左键是否已抬起.</param>
    /// <param name="escapeKeyPressed">ESC 键是否被按下.</param>
    /// <returns>返回决定的 <see cref="DragAction"/>. </returns>
    private DragAction OnDefaultQueryContinueDrag(bool leftButtonIsUp, bool escapeKeyPressed)
    {
        if (escapeKeyPressed)
        {
            return DragAction.Cancel;
        }

        if (leftButtonIsUp)
        {
            return (this.lastGiveFeedbackEventArgs == null || this.lastGiveFeedbackEventArgs.Effects != DragDropEffects.None) ? DragAction.Drop : DragAction.Cancel;
        }

        return DragAction.Continue;
    }

    /// <summary>
    /// 命中所给绝对坐标下的有效放置目标.
    /// </summary>
    /// <param name="offset">绝对位置点.</param>
    /// <returns>返回找到的放置目标对象; 若无则返回 null. </returns>
    private DependencyObject? GetCurrentTarget(Point offset)
    {
        if (this.rootVisual == null)
        {
            return null;
        }

        HitTestResult hitResult = VisualTreeHelper.HitTest(this.rootVisual, offset);
        if (hitResult != null)
        {
            DependencyObject visualHit = hitResult.VisualHit;

            // 检查该元素是否允许 Drop.
            if (visualHit != null && (bool)visualHit.GetValue(UIElement.AllowDropProperty))
            {
                return visualHit;
            }
        }

        return null;
    }

    /// <summary>
    /// 处理进入新拖拽目标的逻辑.
    /// </summary>
    /// <param name="sender">新的目标元素.</param>
    private void OnDragEnter(DependencyObject? sender)
    {
        this.OnDragLeave(this.allowedEffects);
        this.lastDragOverElement = sender;
        if (sender != null && this.lastDragEventArgs != null)
        {
            this.lastDragEventArgs.Effects = this.allowedEffects;
            this.RaiseDragEvent(DragDropManager.DragEnterEvent, sender);
        }
    }

    /// <summary>
    /// 处理在目标上悬停移动的逻辑.
    /// </summary>
    /// <param name="sender">当前悬停的目标元素.</param>
    private void OnDragOver(DependencyObject? sender)
    {
        if (this.lastDragOverElement != sender)
        {
            this.OnDragEnter(sender);
        }

        if (sender != null)
        {
            this.RaiseDragEvent(DragDropManager.DragOverEvent, sender);
        }
    }

    /// <summary>
    /// 处理离开拖拽目标的逻辑.
    /// </summary>
    /// <param name="effects">最后的作用效果.</param>
    private void OnDragLeave(DragDropEffects effects)
    {
        if (this.lastDragOverElement != null && this.lastDragEventArgs != null)
        {
            this.lastDragEventArgs.Effects = effects;
            this.RaiseDragEvent(DragDropManager.DragLeaveEvent, this.lastDragOverElement);
            this.lastDragOverElement = null;
        }

        if (this.lastDragEventArgs != null)
        {
            this.lastDragEventArgs.Effects = DragDropEffects.None;
        }
    }

    /// <summary>
    /// 执行放置完成后的数据交换.
    /// </summary>
    /// <param name="source">放置目标源.</param>
    private void OnDrop(DependencyObject source)
    {
        if (this.lastDragEventArgs == null)
        {
            return;
        }

        this.lastDragEventArgs.UpdateAllowedEffects(this.allowedEffects);
        this.lastDragEventArgs.Effects = this.allowedEffects;
        object currentData = this.lastDragEventArgs.Data;

        if (source != null)
        {
            this.RaiseDragEvent(DragDropManager.DropEvent, source);
        }

        this.lastDragOverElement = null;
        this.RaiseDragDropCompleted(this.allowedEffects, currentData);
    }

    /// <summary>
    /// 处理取消操作.
    /// </summary>
    private void OnCancel()
    {
        if (this.lastDragEventArgs == null)
        {
            return;
        }

        this.lastDragEventArgs.UpdateAllowedEffects(DragDropEffects.None);
        this.lastDragEventArgs.Effects = DragDropEffects.None;
        object currentData = this.lastDragEventArgs.Data;
        this.OnDragLeave(DragDropEffects.None);
        this.RaiseDragDropCompleted(DragDropEffects.None, currentData);
    }

    /// <summary>
    /// 触发目标元素的路由事件分发.
    /// </summary>
    /// <param name="dragEvent">路由事件标识.</param>
    /// <param name="target">目标对象.</param>
    private void RaiseDragEvent(RoutedEvent dragEvent, DependencyObject target)
    {
        if (this.lastDragEventArgs == null)
        {
            return;
        }

        var eventArgs = new DragEventArgs(this.lastDragEventArgs);

        // 映射对应的预览事件.
        if (dragEvent == DragDropManager.DragEnterEvent)
        {
            eventArgs.RoutedEvent = DragDropManager.PreviewDragEnterEvent;
        }
        else if (dragEvent == DragDropManager.DragOverEvent)
        {
            eventArgs.RoutedEvent = DragDropManager.PreviewDragOverEvent;
        }
        else if (dragEvent == DragDropManager.DragLeaveEvent)
        {
            eventArgs.RoutedEvent = DragDropManager.PreviewDragLeaveEvent;
        }
        else if (dragEvent == DragDropManager.DropEvent)
        {
            eventArgs.RoutedEvent = DragDropManager.PreviewDropEvent;
        }

        try
        {
            target.RaiseEvent(eventArgs);
            if (!eventArgs.Handled)
            {
                eventArgs.RoutedEvent = dragEvent;
                target.RaiseEvent(eventArgs);
            }

            this.lastDragEventArgs = eventArgs;
        }
        catch
        {
            this.Cleanup();
            throw;
        }
    }

    /// <summary>
    /// 触发拖拽源的完成事件.
    /// </summary>
    /// <param name="effects">执行的效果.</param>
    /// <param name="dataObject">拖拽的数据对象.</param>
    private void RaiseDragDropCompleted(DragDropEffects effects, object dataObject)
    {
        try
        {
            var completedArgs = new DragDropCompletedEventArgs(DragDropManager.DragDropCompletedEvent);
            completedArgs.Effects = effects;
            completedArgs.Data = dataObject;
            this.dragSource?.RaiseEvent(completedArgs);
        }
        catch
        {
            this.Cleanup();
            throw;
        }
    }

    /// <summary>
    /// 处理触控移动触发的拖拽更新.
    /// </summary>
    /// <param name="position">触控当前位置.</param>
    /// <param name="sender">发送源.</param>
    internal void DragSourceTapMove(Point position, object sender)
    {
        DragDropManager.IsInTouchDrag = true;
        this.LastTouchOffset = position;
        if (DragDropManager.IsDragInProgress && this.GetAbsoluteOffset(position, out Point absoluteOffset))
        {
            this.OnMove(absoluteOffset);
        }
    }

    /// <summary>
    /// 处理触控抬起触发的拖拽结束.
    /// </summary>
    /// <param name="position">触控最终位置.</param>
    /// <param name="sender">发送源.</param>
    internal void DragSourceTapUp(Point position, object sender)
    {
        if (this.GetAbsoluteOffset(position, out Point absoluteOffset))
        {
            DependencyObject? currentTarget = this.GetCurrentTarget(absoluteOffset);
            this.FinishDragOperation(currentTarget);
        }
    }

    /// <summary>
    /// 获取触控点相对于指定输入元素的偏移坐标.
    /// </summary>
    /// <param name="relativeTo">参考坐标系的输入元素.</param>
    /// <returns>转换后的坐标点. </returns>
    internal Point GetTouchOffset(IInputElement relativeTo)
    {
        if (this.GetAbsoluteOffset(this.LastTouchOffset, out Point absoluteOffset))
        {
            if (relativeTo is UIElement uiTarget && this.rootVisual != null)
            {
                // 获取相对元素的原点坐标.
                Point targetOrigin = uiTarget.TransformToVisual(this.rootVisual).Transform(default(Point));
                absoluteOffset.X -= targetOrigin.X;
                absoluteOffset.Y -= targetOrigin.Y;
            }

            return absoluteOffset;
        }

        return default(Point);
    }

    /// <summary>
    /// 将本地位置转换为相对于根视觉元素的绝对位置.
    /// </summary>
    /// <param name="localMousePosition">本地坐标位置.</param>
    /// <param name="absoluteOffset">输出转换后的绝对坐标.</param>
    /// <returns>如果转换成功返回 true; 否则返回 false. </returns>
    private bool GetAbsoluteOffset(Point localMousePosition, out Point absoluteOffset)
    {
        absoluteOffset = default(Point);
        if (this.dragSource is UIElement uiSource && this.rootVisual != null)
        {
            // 通过视觉转换器进行坐标换算.
            absoluteOffset = uiSource.TransformToVisual(this.rootVisual).Transform(localMousePosition);
            return true;
        }

        return false;
    }
}