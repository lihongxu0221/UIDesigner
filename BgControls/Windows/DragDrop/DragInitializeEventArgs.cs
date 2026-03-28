namespace BgControls.Windows.DragDrop;

/// <summary>
/// 表示处理拖拽初始化事件的方法.
/// </summary>
/// <param name="sender">事件发送者.</param>
/// <param name="e">包含拖拽初始化事件数据的参数.</param>
public delegate void DragInitializeEventHandler(object sender, DragInitializeEventArgs e);

/// <summary>
/// 为拖拽初始化路由事件提供数据的类.
/// </summary>
public sealed class DragInitializeEventArgs : RoutedEventArgs
{
    private DragDropEffects allowedEffects;

    private bool allowedEffectsSet;

    private object? draggedData;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragInitializeEventArgs"/> class.
    /// </summary>
    internal DragInitializeEventArgs()
        : base(DragDropManager.DragInitializeEvent)
    {
        // 调用基类构造函数并关联拖拽初始化路由事件.
    }

    /// <summary>
    /// Gets or sets 被拖拽的数据对象.
    /// </summary>
    public object? Data
    {
        get { return draggedData; }
        set { draggedData = value; }
    }

    /// <summary>
    /// Gets or sets 拖拽操作所允许的效果.
    /// </summary>
    public DragDropEffects AllowedEffects
    {
        get
        {
            return allowedEffects;
        }

        set
        {
            // 标记允许的效果已被显式设置.
            allowedEffectsSet = true;
            allowedEffects = value;
        }
    }

    /// <summary>
    /// Gets 拖拽开始时相对于触发元素的起始坐标点.
    /// </summary>
    public Point RelativeStartPoint { get; internal set; }

    /// <summary>
    /// Gets or sets 拖拽视觉对象相对于鼠标光标的偏移量.
    /// </summary>
    public Point DragVisualOffset { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 是否取消当前的拖拽初始化操作.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets or sets 拖拽过程中显示的视觉提示对象.
    /// </summary>
    public object? DragVisual { get; set; }

    /// <summary>
    /// Gets a value indicating whether 是否已经设置了允许的拖拽效果.
    /// </summary>
    internal bool AllowedEffectsSet => allowedEffectsSet;

    /// <summary>
    /// 以特定于类型的方式调用事件处理程序.
    /// </summary>
    /// <param name="genericHandler">要调用的泛型处理程序.</param>
    /// <param name="genericTarget">在其上调用处理程序的源对象.</param>
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        // 将通用的委派转换为特定的拖拽初始化事件处理程序并执行.
        var handler = genericHandler as DragInitializeEventHandler;
        handler?.Invoke(genericTarget, this);
    }
}