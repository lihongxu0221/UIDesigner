namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 触控移动相关的工具类.
/// </summary>
internal static class TouchMoveUtilities
{
    /// <summary>
    /// 触控移动事件辅助对象的附加属性.
    /// </summary>
    private static readonly DependencyProperty TouchMoveEventHelperProperty =
        DependencyProperty.RegisterAttached("TouchMoveEventHelper", typeof(TouchMoveHelper), typeof(TouchMoveUtilities), new PropertyMetadata(null));

    /// <summary>
    /// 当前处于挂起状态的触控移动事件参数.
    /// </summary>
    private static System.Windows.Input.TouchEventArgs? suspendedTouchMoveEventArgs;

    /// <summary>
    /// 获取或创建指定依赖对象的触控移动辅助对象.
    /// </summary>
    /// <param name="obj">目标依赖对象.</param>
    /// <returns>触控移动辅助实例.</returns>
    public static TouchMoveHelper GetOrCreateTouchMoveHelper(DependencyObject obj)
    {
        // 检查参数是否为空.
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));

        // 尝试从附加属性获取辅助对象.
        var moveHelper = (TouchMoveHelper?)obj.GetValue(TouchMoveEventHelperProperty);
        if (moveHelper == null)
        {
            // 如果不存在则创建并设置回附加属性.
            moveHelper = new TouchMoveHelper();
            obj.SetValue(TouchMoveEventHelperProperty, moveHelper);

            // 如果对象是 FrameworkElement，在其卸载时清理字典，释放内存.
            if (obj is FrameworkElement element)
            {
                element.Unloaded += (s, e) =>
                {
                    moveHelper.Clear();
                };
            }
        }

        return moveHelper;
    }

    /// <summary>
    /// 清除指定依赖对象的触控移动辅助对象.
    /// </summary>
    /// <param name="dependencyObject">目标依赖对象.</param>
    public static void ClearTouchMoveHelper(DependencyObject dependencyObject)
    {
        // 检查参数是否为空.
        ArgumentNullException.ThrowIfNull(dependencyObject, nameof(dependencyObject));

        // 清除附加属性值.
        dependencyObject.SetValue(TouchMoveUtilities.TouchMoveEventHelperProperty, null);
    }

    /// <summary>
    /// 触控移动事件的处理逻辑.
    /// </summary>
    /// <param name="sender">事件发送者.</param>
    /// <param name="eventArgs">触控事件参数.</param>
    public static void OnTouchMove(object? sender, System.Windows.Input.TouchEventArgs eventArgs)
    {
        // 检查参数是否为空.
        ArgumentNullException.ThrowIfNull(sender, nameof(sender));
        ArgumentNullException.ThrowIfNull(eventArgs, nameof(eventArgs));

        UIElement uiElement = (UIElement)sender;

        // 验证触控输入相对于原始源是否有效.
        if (!TouchManager.IsValidTouchInput(uiElement, eventArgs.OriginalSource))
        {
            return;
        }

        // 获取当前触控点信息.
        TouchPoint touchPoint = eventArgs.GetTouchPoint(uiElement);
        int touchId = touchPoint.TouchDevice.Id;
        Point currentPosition = touchPoint.Position;

        // 获取或创建移动辅助对象.
        TouchMoveHelper moveHelper = GetOrCreateTouchMoveHelper(uiElement);

        // 如果触控 ID 已被挂起，且不是当前正在处理的事件，则拦截后续逻辑.
        if (TouchManager.FreeUIHelper.IsTouchIdSuspended(touchId) && suspendedTouchMoveEventArgs != eventArgs)
        {
            eventArgs.Handled = true;
            return;
        }

        // 获取上一次记录的移动位置.
        Point? lastPosition = moveHelper.TryGetTouchMovePosition(touchId);

        // 验证移动距离是否超过阈值且不应被抑制.
        if (IsValidTouchMove(lastPosition, currentPosition) && !moveHelper.ShouldSuppressTouchMove(touchId, currentPosition))
        {
            // 标记当前事件为挂起状态，防止重入.
            suspendedTouchMoveEventArgs = eventArgs;

            // 调用 UI 挂起逻辑，并在结束后清除标记.
            TouchManager.FreeUIHelper.SuspendTouchId(touchId, uiElement.Dispatcher, () => suspendedTouchMoveEventArgs = null);

            // 执行辅助对象的移动逻辑.
            moveHelper.OnTouchMove(touchId, currentPosition);

            // 获取包装后的原始源并触发路由事件.
            DependencyObject rawOriginalSource = (DependencyObject)eventArgs.OriginalSource;
            DependencyObject targetOriginalSource = TouchManager.GetOriginalSource(rawOriginalSource);
            TouchManager.RaiseTouchMoveEvent(uiElement, eventArgs, targetOriginalSource, shouldHandleOriginalArgs: true);
        }
        else
        {
            // 如果移动无效，则标记事件已处理，阻止进一步冒泡.
            // 在处理微小震颤时，可能会导致父级容器也无法感知到任何触控动作。
            // eventArgs.Handled = true;
        }
    }

    /// <summary>
    /// 验证两次触控位置之间的移动是否有效.
    /// </summary>
    /// <param name="position1">位置1（可能为空）.</param>
    /// <param name="position2">位置2.</param>
    /// <returns>如果移动有效返回 true; 否则返回 false.</returns>
    private static bool IsValidTouchMove(Point? position1, Point position2)
    {
        // 如果没有历史位置，则视为第一次有效移动.
        if (!position1.HasValue)
        {
            return true;
        }

        return IsValidTouchMove(position1.Value, position2);
    }

    /// <summary>
    /// 验证两个点之间的距离是否达到最小移动阈值.
    /// </summary>
    /// <param name="position1">起始位置.</param>
    /// <param name="position2">结束位置.</param>
    /// <returns>超过阈值返回 true; 否则返回 false.</returns>
    private static bool IsValidTouchMove(Point position1, Point position2)
    {
        // 根据 TouchManager 定义的最小距离判断 X 或 Y 轴上的偏移量.
        return Math.Abs(position1.X - position2.X) >= TouchManager.TouchMoveMinimumDistance ||
               Math.Abs(position1.Y - position2.Y) >= TouchManager.TouchMoveMinimumDistance;
    }
}