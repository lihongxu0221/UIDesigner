using BgControls.Windows.Controls;
using BgControls.Windows.Input.Touch;

namespace BgControls.Windows.DragDrop
{
    /// <summary>
    /// 拖拽初始化器，负责处理 UI 元素的拖拽识别和启动逻辑.
    /// </summary>
    internal class DragInitializer
    {
        private IInputElement? rootVisual;
        private bool mouseCaptured;
        private WeakReference? elementRef;

        /// <summary>
        /// Gets a value indicating whether 是否允许对已捕获鼠标的元素进行拖拽.
        /// </summary>
        public bool AllowCapturedDrag
        {
            get
            {
                if (this.Element != null)
                {
                    return DragDropManager.GetAllowCapturedDrag(this.Element);
                }

                return false;
            }
        }

        /// <summary>
        /// Gets 拖拽开始时的起始坐标点（相对于根视觉元素）.
        /// </summary>
        public Point StartPoint { get; private set; }

        /// <summary>
        /// Gets 拖拽开始时的局部坐标点（相对于当前元素）.
        /// </summary>
        public Point LocalPoint { get; private set; }

        /// <summary>
        /// Gets 关联的 UI 元素.
        /// </summary>
        public UIElement? Element
        {
            get
            {
                if (this.elementRef != null && this.elementRef.IsAlive)
                {
                    return this.elementRef.Target as UIElement;
                }

                return null;
            }
        }

        /// <summary>
        /// 初始化初始化器并绑定指定元素.
        /// </summary>
        /// <param name="uiElement">要绑定的 UI 元素.</param>
        public void Initialize(UIElement uiElement)
        {
            // 验证参数非空.
            ArgumentNullException.ThrowIfNull(uiElement, nameof(uiElement));
            this.elementRef = new WeakReference(uiElement);

            // 确保不会重复挂载事件.
            this.UnhookFromEvents();
            this.HookToEvents();
        }

        /// <summary>
        /// 清理资源并解除事件绑定.
        /// </summary>
        public void Clear()
        {
            this.UnhookFromEvents();
            this.CleanUp(null);
            this.elementRef = null;
        }

        /// <summary>
        /// 启动拖拽操作.
        /// </summary>
        internal void StartDrag()
        {
            DragInitializeEventArgs eventArgs = new DragInitializeEventArgs();
            eventArgs.RelativeStartPoint = this.LocalPoint;

            if (this.Element != null)
            {
                // 触发拖拽初始化事件.
                this.Element.RaiseEvent(eventArgs);
                if (!eventArgs.Cancel)
                {
                    object dragData = eventArgs.Data ?? this.Element;
                    DragDropEffects allowedEffects = eventArgs.AllowedEffectsSet ?
                        eventArgs.AllowedEffects :
                        DragDropManager.DefaultDragDropEffects;

                    // 执行系统的拖拽放置操作.
                    DragDropManager.DoDragDrop(
                        this.Element,
                        dragData,
                        allowedEffects,
                        DragDropKeyStates.LeftMouseButton,
                        eventArgs.DragVisual,
                        eventArgs.RelativeStartPoint,
                        eventArgs.DragVisualOffset);
                }
            }
        }

        /// <summary>
        /// 处理拖拽源的触控按下逻辑.
        /// </summary>
        /// <param name="localPoint">触控局部坐标.</param>
        /// <param name="touchOwner">触控所有者元素.</param>
        internal void DragSourceTapDown(Point localPoint, UIElement touchOwner)
        {
            if (this.Element != null)
            {
                FrameworkElement? rootWindow = Window.GetWindow(this.Element);
                if (rootWindow == null)
                {
                    rootWindow = ApplicationHelper.GetRootVisual(this.Element);
                }

                this.rootVisual = rootWindow;

                // 计算全局起始点并保存局部点.
                this.StartPoint = touchOwner.TransformToVisualSafe(rootWindow).Transform(localPoint);
                this.LocalPoint = localPoint;
                DragDropManager.LastQueryContinueAction = DragAction.Continue;
            }
        }

        /// <summary>
        /// 处理拖拽源的触控抬起逻辑.
        /// </summary>
        /// <param name="touchOwner">触控所有者元素.</param>
        internal void DragSourceTapUp(UIElement? touchOwner)
        {
            this.CleanUp(touchOwner);
        }

        /// <summary>
        /// 处理拖拽源的触控移动逻辑.
        /// </summary>
        /// <param name="localPoint">触控局部坐标.</param>
        /// <param name="touchOwner">触控所有者元素.</param>
        internal void DragSourceOnTapMove(Point localPoint, UIElement touchOwner)
        {
            if (this.rootVisual is Visual visual)
            {
                if (!DragDropManager.IsDragInProgress && DragDropManager.LastQueryContinueAction != DragAction.Cancel)
                {
                    Point currentGlobalPoint = touchOwner.TransformToVisualSafe(visual as UIElement).Transform(localPoint);

                    // 检查是否达到最小拖拽距离阈值.
                    if (Math.Abs(currentGlobalPoint.X - this.StartPoint.X) > DragDropManager.MinimumHorizontalDragDistance ||
                        Math.Abs(currentGlobalPoint.Y - this.StartPoint.Y) > DragDropManager.MinimumVerticalDragDistance)
                    {
                        this.StartDragPrivate(touchOwner);
                    }
                }
                else
                {
                    this.CleanUp(touchOwner);
                }
            }
        }

        /// <summary>
        /// 挂载必要的事件监听程序.
        /// </summary>
        private void HookToEvents()
        {
            if (this.Element != null)
            {
                this.Element.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.DragSourceMouseLeftButtonDown), handledEventsToo: true);
                TouchManager.AddTouchDownEventHandler(this.Element, this.OnDragSourceTouchDown);
                TouchManager.AddDragStartedEventHandler(this.Element, this.OnDragSourceTouchDragStarted);
                TouchManager.AddDragEventHandler(this.Element, this.OnDragSourceTouchDrag);
                TouchManager.AddDragFinishedEventHandler(this.Element, this.OnDragSourceTouchDragFinished);
            }
        }

        /// <summary>
        /// 解除挂载的事件监听程序.
        /// </summary>
        private void UnhookFromEvents()
        {
            if (this.Element != null)
            {
                this.Element.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.DragSourceMouseLeftButtonDown));
                TouchManager.RemoveTouchDownEventHandler(this.Element, this.OnDragSourceTouchDown);
                TouchManager.RemoveDragStartedEventHandler(this.Element, this.OnDragSourceTouchDragStarted);
                TouchManager.RemoveDragEventHandler(this.Element, this.OnDragSourceTouchDrag);
                TouchManager.RemoveDragFinishedEventHandler(this.Element, this.OnDragSourceTouchDragFinished);
            }
        }

        /// <summary>
        /// 鼠标左键按下事件处理程序.
        /// </summary>
        private void DragSourceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement uiElement && this.Element != null)
            {
                // 排除触控引起的鼠标模拟事件.
                if (!TouchManager.IsTouchInput(e.StylusDevice, uiElement, e.OriginalSource as UIElement))
                {
                    this.rootVisual = Window.GetWindow(this.Element) ?? ApplicationHelper.GetRootVisual(this.Element);
                    IInputElement? relativeTo = (this.rootVisual is Popup) ? (sender as IInputElement) : this.rootVisual;

                    this.StartPoint = e.GetPosition(relativeTo);
                    this.LocalPoint = e.GetSafePosition(this.Element);

                    this.RemoveHandlers(uiElement);
                    this.Element.MouseLeave += this.DragSourceMouseLeave;
                    this.Element.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.DragSourceMouseLeftButtonUp), true);

                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        this.Element.AddHandler(UIElement.MouseMoveEvent, new MouseEventHandler(this.DragSourceOnMouseMove), true);
                    }

                    DragDropManager.LastQueryContinueAction = DragAction.Continue;
                }
            }
        }

        /// <summary>
        /// 触控按下事件处理程序.
        /// </summary>
        private void OnDragSourceTouchDown(object sender, BgControls.Windows.Input.Touch.TouchEventArgs e)
        {
            if (sender is UIElement uiElement)
            {
                FrameworkElement? rootWindow = Window.GetWindow(uiElement) ?? ApplicationHelper.GetRootVisual(uiElement);
                Point touchPosition = e.GetTouchPoint(uiElement).Position;

                this.rootVisual = rootWindow;
                this.StartPoint = uiElement.TransformToVisualSafe(rootWindow).Transform(touchPosition);
                this.LocalPoint = touchPosition;
                DragDropManager.LastQueryContinueAction = DragAction.Continue;
                uiElement.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.DragSourceMouseLeftButtonUp), handledEventsToo: true);
            }
        }

        /// <summary>
        /// 鼠标离开元素时的处理逻辑.
        /// </summary>
        private void DragSourceMouseLeave(object sender, MouseEventArgs e)
        {
            bool isLeftButtonPressed = e.LeftButton == MouseButtonState.Pressed;
            bool shouldCleanUp = DragDropManager.IsDragInProgress || !isLeftButtonPressed;

            if (!shouldCleanUp)
            {
                Window? currentWindow = Window.GetWindow(this.Element);
                bool isWindowValid = currentWindow == null || (currentWindow.IsActive && currentWindow.IsMouseOver);
                bool hasMouseMoved = this.Element != null && this.LocalPoint != e.GetPosition(this.Element);

                // 尝试捕获鼠标以继续跟踪移动.
                if (isWindowValid && hasMouseMoved && (Mouse.Captured == null || this.AllowCapturedDrag) && Mouse.Captured != this.Element)
                {
                    this.mouseCaptured = true;
                    this.mouseCaptured = this.Element!.CaptureMouse();
                    shouldCleanUp = !this.mouseCaptured;
                }
                else if (!isWindowValid || !hasMouseMoved)
                {
                    shouldCleanUp = true;
                }
            }

            if (shouldCleanUp)
            {
                this.CleanUp(sender as UIElement);
            }
        }

        /// <summary>
        /// 鼠标移动事件处理程序.
        /// </summary>
        private void DragSourceOnMouseMove(object sender, MouseEventArgs e)
        {
            if (TouchManager.IsTouchInput(e.StylusDevice, sender as UIElement, e.OriginalSource as UIElement))
            {
                return;
            }

            bool isLeftPressed = e.LeftButton == MouseButtonState.Pressed;
            IInputElement? relativeTo = (this.rootVisual is Popup) ? (sender as IInputElement) : this.rootVisual;

            if (!DragDropManager.IsDragInProgress && isLeftPressed && DragDropManager.LastQueryContinueAction != DragAction.Cancel)
            {
                Point currentPos = e.GetPosition(relativeTo);

                // 检查移动距离是否触发拖拽启动.
                if (Math.Abs(currentPos.X - this.StartPoint.X) > DragDropManager.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPos.Y - this.StartPoint.Y) > DragDropManager.MinimumVerticalDragDistance)
                {
                    this.StartDragPrivate(sender as UIElement);
                }
            }
            else
            {
                this.CleanUp(sender as UIElement);
            }
        }

        /// <summary>
        /// 触控拖拽启动事件处理程序.
        /// </summary>
        private void OnDragSourceTouchDragStarted(object sender, BgControls.Windows.Input.Touch.TouchEventArgs e)
        {
            if (this.rootVisual is Visual)
            {
                if (!DragDropManager.IsDragInProgress && DragDropManager.LastQueryContinueAction != DragAction.Cancel)
                {
                    this.StartDragPrivate(this.Element);
                    e.Handled = true;
                }
                else
                {
                    this.CleanUp(this.Element);
                }
            }
        }

        /// <summary>
        /// 触控拖拽过程中事件处理程序.
        /// </summary>
        private void OnDragSourceTouchDrag(object sender, BgControls.Windows.Input.Touch.TouchEventArgs e)
        {
            // 标记已处理以阻止默认行为.
            e.Handled = true;
        }

        /// <summary>
        /// 触控拖拽完成事件处理程序.
        /// </summary>
        private void OnDragSourceTouchDragFinished(object sender, BgControls.Windows.Input.Touch.TouchEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 移除临时挂载的鼠标事件处理程序.
        /// </summary>
        private void RemoveHandlers(UIElement? sender)
        {
            UIElement? targetElement = this.Element ?? sender;
            if (targetElement != null)
            {
                targetElement.MouseLeave -= this.DragSourceMouseLeave;
                targetElement.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.DragSourceMouseLeftButtonUp));
                targetElement.RemoveHandler(UIElement.MouseMoveEvent, new MouseEventHandler(this.DragSourceOnMouseMove));
            }
        }

        /// <summary>
        /// 内部触发拖拽启动逻辑.
        /// </summary>
        private void StartDragPrivate(UIElement? sender)
        {
            this.RemoveHandlers(sender);
            if (!DragDropManager.IsDragInProgress && (this.AllowCapturedDrag || Mouse.Captured == null || Mouse.Captured == sender))
            {
                this.StartDrag();
            }

            this.CleanUp(sender);
        }

        /// <summary>
        /// 鼠标左键抬起事件处理程序.
        /// </summary>
        private void DragSourceMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            this.CleanUp(sender as UIElement);
        }

        /// <summary>
        /// 执行清理操作，释放鼠标捕获并重置坐标点.
        /// </summary>
        private void CleanUp(UIElement? sender)
        {
            UIElement? targetElement = this.Element ?? sender;
            if (this.mouseCaptured)
            {
                targetElement?.ReleaseMouseCapture();
            }

            this.mouseCaptured = false;
            this.rootVisual = null;
            this.ClearPoints();
            this.RemoveHandlers(sender);
            DragDropManager.IsInTouchDrag = false;
        }

        /// <summary>
        /// 将坐标点重置为 NaN 状态.
        /// </summary>
        private void ClearPoints()
        {
            this.StartPoint = new Point(double.NaN, double.NaN);
            this.LocalPoint = new Point(double.NaN, double.NaN);
        }
    }
}