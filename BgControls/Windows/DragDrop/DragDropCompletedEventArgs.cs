namespace BgControls.Windows.DragDrop;

/// <summary>
/// 表示处理拖放操作完成路由事件的方法.
/// </summary>
/// <param name="sender">绑定事件处理程序的对象. </param>
/// <param name="e">包含拖放完成事件数据的参数. </param>
public delegate void DragDropCompletedEventHandler(object sender, DragDropCompletedEventArgs e);

/// <summary>
/// 为拖放操作完成路由事件提供数据的类.
/// </summary>
public sealed class DragDropCompletedEventArgs : RoutedEventArgs
{
    private object? draggedData;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragDropCompletedEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识符. </param>
    internal DragDropCompletedEventArgs(RoutedEvent routedEvent)
        : base(routedEvent)
    {
    }

    /// <summary>
    /// Gets 拖放操作生成的最终效果.
    /// </summary>
    public DragDropEffects Effects { get; internal set; }

    /// <summary>
    /// Gets or sets 正在被拖放的数据对象.
    /// </summary>
    public object? Data
    {
        get { return this.draggedData; }
        set { draggedData = value; }
    }

    /// <summary>
    /// 以特定于类型的方式调用事件处理程序.
    /// 该重写通过消除泛型转换开销来提升事件的分发效率.
    /// </summary>
    /// <param name="genericHandler">要调用的泛型处理程序. </param>
    /// <param name="genericTarget">在其上调用处理程序的源对象. </param>
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        var dragDropCompletedEventHandler = genericHandler as DragDropCompletedEventHandler;
        dragDropCompletedEventHandler?.Invoke(genericTarget, this);
    }
}