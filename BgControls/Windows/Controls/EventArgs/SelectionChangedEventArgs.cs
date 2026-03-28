namespace BgControls.Windows.Controls;

/// <summary>
/// 表示处理选择项更改（SelectionChanged）路由事件的方法.
/// </summary>
/// <param name="sender">绑定事件处理程序的对象.</param>
/// <param name="e">包含选择更改数据的事件参数.</param>
public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);

/// <summary>
/// 为选择项更改路由事件提供数据的类.
/// 该类扩展了标准的 <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> 以支持特定的事件路由需求.
/// </summary>
public class SelectionChangedEventArgs : System.Windows.Controls.SelectionChangedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionChangedEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识符.</param>
    /// <param name="source">引发事件的源对象.</param>
    /// <param name="removedItems">在此次事件中取消选中的项集合.</param>
    /// <param name="addedItems">在此次事件中新选中的项集合.</param>
    public SelectionChangedEventArgs(RoutedEvent routedEvent, object source, IList? removedItems, IList? addedItems)
        : base(routedEvent, removedItems, addedItems)
    {
        this.Source = source;
    }

    /// <summary>
    /// 以特定于此委托类型的方式调用事件处理程序.
    /// 该重写通过减少类型转换开销来提高事件分发的效率.
    /// </summary>
    /// <param name="genericHandler">要调用的泛型处理程序实例.</param>
    /// <param name="genericTarget">在其上调用处理程序的源对象.</param>
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        var radSelectionChangedEventHandler = (SelectionChangedEventHandler?)genericHandler;
        radSelectionChangedEventHandler?.Invoke(genericTarget, this);
    }
}