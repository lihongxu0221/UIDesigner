namespace BgControls.Windows.DragDrop;

/// <summary>
/// 表示处理下拉菜单（DropDown）相关路由事件的方法.
/// </summary>
/// <param name="sender">绑定事件处理程序的对象.</param>
/// <param name="e">包含下拉菜单事件数据的参数.</param>
public delegate void DropDownEventHandler(object sender, DropDownEventArgs e);

/// <summary>
/// 为下拉菜单路由事件提供数据的类.
/// </summary>
public class DropDownEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识符.</param>
    /// <param name="source">引发事件的源对象.</param>
    /// <param name="dropDownItems">下拉菜单中使用的数据项集合.</param>
    public DropDownEventArgs(RoutedEvent routedEvent, object source, IEnumerable dropDownItems)
        : base(routedEvent, source)
    {
        DropDownItemsSource = dropDownItems;
    }

    /// <summary>
    /// 以特定于类型的方式调用事件处理程序.
    /// 该重写通过消除泛型转换开销来提升事件的分发效率.
    /// </summary>
    /// <param name="genericHandler">要调用的泛型处理程序.</param>
    /// <param name="genericTarget">在其上调用处理程序的源对象.</param>
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        (genericHandler as BgControls.Windows.DragDrop.DropDownEventHandler)?.Invoke(genericTarget, this);
    }

    /// <summary>
    /// Gets or sets 下拉菜单展示的数据项源.
    /// </summary>
    /// <value>
    /// 用于填充下拉列表的 <see cref="IEnumerable"/> 集合.
    /// </value>
    public IEnumerable DropDownItemsSource { get; set; }
}