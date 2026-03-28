namespace BgControls.Windows.DragDrop;

/// <summary>
/// 表示处理拖放路由事件（如 DragEnter、DragOver 等）的方法.
/// </summary>
/// <param name="sender">绑定事件处理程序的对象.</param>
/// <param name="e">包含拖放事件数据的参数.</param>
public delegate void DragEventHandler(object sender, DragEventArgs e);

/// <summary>
/// 为拖放路由事件提供数据的类.
/// 该类对标准的 WPF 拖放事件参数进行了封装，以支持自定义的拖放逻辑和触摸操作.
/// </summary>
public sealed class DragEventArgs : RoutedEventArgs
{
    private object draggedData;
    private System.Windows.DragEventArgs? originalEventArgs;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragEventArgs"/> class.
    /// </summary>
    /// <param name="e">系统原始的拖放事件参数.</param>
    internal DragEventArgs(System.Windows.DragEventArgs e)
        : base(e.RoutedEvent, e.OriginalSource)
    {
        // 将系统的路由事件映射为自定义拖放管理器的路由事件
        if (e.RoutedEvent == System.Windows.DragDrop.PreviewDragEnterEvent)
        {
            this.RoutedEvent = DragDropManager.PreviewDragEnterEvent;
        }
        else if (e.RoutedEvent == System.Windows.DragDrop.PreviewDragLeaveEvent)
        {
            this.RoutedEvent = DragDropManager.PreviewDragLeaveEvent;
        }
        else if (e.RoutedEvent == System.Windows.DragDrop.PreviewDragOverEvent)
        {
            this.RoutedEvent = DragDropManager.PreviewDragOverEvent;
        }
        else if (e.RoutedEvent == System.Windows.DragDrop.PreviewDropEvent)
        {
            this.RoutedEvent = DragDropManager.PreviewDropEvent;
        }
        else if (e.RoutedEvent == System.Windows.DragDrop.DragEnterEvent)
        {
            this.RoutedEvent = DragDropManager.DragEnterEvent;
        }
        else if (e.RoutedEvent == System.Windows.DragDrop.DragLeaveEvent)
        {
            this.RoutedEvent = DragDropManager.DragLeaveEvent;
        }
        else if (e.RoutedEvent == System.Windows.DragDrop.DragOverEvent)
        {
            this.RoutedEvent = DragDropManager.DragOverEvent;
        }
        else if (e.RoutedEvent == System.Windows.DragDrop.DropEvent)
        {
            this.RoutedEvent = DragDropManager.DropEvent;
        }

        this.AllowedEffects = e.AllowedEffects;
        this.MouseEventArgs = null;
        this.Effects = e.Effects;
        this.draggedData = e.Data;
        this.originalEventArgs = e;
        this.Handled = e.Handled;
        this.Source = e.Source;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DragEventArgs"/> class.
    /// </summary>
    /// <param name="e">现有的拖放事件参数实例.</param>
    internal DragEventArgs(DragEventArgs e)
        : this(e.AllowedEffects, e.draggedData, e.Effects, e.MouseEventArgs)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DragEventArgs"/> class.
    /// </summary>
    /// <param name="allowedEffects">当前拖放操作允许的效果.</param>
    /// <param name="data">被拖拽的数据对象.</param>
    /// <param name="effects">当前设置的拖放效果.</param>
    /// <param name="mouseEventArgs">可选的鼠标事件参数.</param>
    internal DragEventArgs(DragDropEffects allowedEffects, object data, DragDropEffects effects, MouseEventArgs? mouseEventArgs)
    {
        this.AllowedEffects = allowedEffects;
        this.MouseEventArgs = mouseEventArgs;
        this.Effects = effects;
        this.draggedData = data;
    }

    /// <summary>
    /// Gets 当前拖拽操作的源所允许的拖放效果.
    /// </summary>
    public DragDropEffects AllowedEffects { get; private set; }

    /// <summary>
    /// Gets or sets 当前拖放操作的目标效果.
    /// </summary>
    public DragDropEffects Effects { get; set; }

    /// <summary>
    /// Gets 正在被拖拽的数据对象.
    /// </summary>
    public object Data
    {
        get { return draggedData; }
        private set { draggedData = value; }
    }

    /// <summary>
    /// Gets or sets 关联的鼠标事件参数.
    /// </summary>
    internal MouseEventArgs? MouseEventArgs { get; set; }

    /// <summary>
    /// 获取拖拽点相对于指定元素的当前坐标.
    /// </summary>
    /// <param name="relativeTo">要在其中计算位置的参考元素.</param>
    /// <returns>拖拽点相对于参考元素的坐标。如果无法计算，则返回 X/Y 为 NaN 的点.</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="relativeTo"/> 为空时抛出.</exception>
    public Point GetPosition(IInputElement relativeTo)
    {
        ArgumentNullException.ThrowIfNull(relativeTo, nameof(relativeTo));

        // 优先处理触摸拖拽相关的偏移逻辑
        if (DragDropManager.DragOperation != null && DragDropManager.DragOperation.IsTouchDrag)
        {
            try
            {
                return DragDropManager.DragOperation.GetTouchOffset(relativeTo);
            }
            catch
            {
                return new Point(double.NaN, double.NaN);
            }
        }

        // 其次通过关联的鼠标事件参数获取位置
        if (MouseEventArgs != null)
        {
            return MouseEventArgs.GetSafePosition(relativeTo);
        }

        // 最后回退到系统原始的拖放事件参数进行位置计算
        if (originalEventArgs != null)
        {
            return originalEventArgs.GetPosition(relativeTo);
        }

        return new Point(double.NaN, double.NaN);
    }

    /// <summary>
    /// 更新允许的拖放效果枚举值.
    /// </summary>
    /// <param name="effects">新的允许效果.</param>
    internal void UpdateAllowedEffects(DragDropEffects effects)
    {
        this.AllowedEffects = effects;
    }

    /// <summary>
    /// 以特定于该类及其委托类型的方式调用事件处理程序.
    /// </summary>
    /// <param name="genericHandler">要调用的泛型处理程序.</param>
    /// <param name="genericTarget">在其上调用处理程序的源对象.</param>
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        var dragEventHandler = genericHandler as BgControls.Windows.DragDrop.DragEventHandler;
        dragEventHandler?.Invoke(genericTarget, this);
    }
}