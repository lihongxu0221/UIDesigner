namespace BgControls.Windows.DragDrop;

/// <summary>
/// 为拖拽指示器（Drag Cue）位置更改路由事件提供数据的内部类.
/// </summary>
internal class DragCuePositionEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DragCuePositionEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识符.</param>
    internal DragCuePositionEventArgs(RoutedEvent routedEvent)
        : base(routedEvent)
    {
    }

    /// <summary>
    /// Gets or sets 拖拽指示器相对于鼠标光标的偏移量点.
    /// </summary>
    public Point DragCueOffset { get; internal set; }

    /// <summary>
    /// 以特定于类型的方式调用事件处理程序.
    /// 该重写通过消除泛型转换开销来提升事件的分发效率.
    /// </summary>
    /// <param name="genericHandler">要调用的泛型处理程序.</param>
    /// <param name="genericTarget">在其上调用处理程序的源对象.</param>
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        var eventHandler = genericHandler as EventHandler<DragCuePositionEventArgs>;
        eventHandler?.Invoke(genericTarget, this);
    }
}