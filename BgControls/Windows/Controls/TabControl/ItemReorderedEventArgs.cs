namespace BgControls.Windows.Controls;

/// <summary>
/// 表示处理项重排路由事件的方法.
/// </summary>
/// <param name="sender">事件绑定的对象.</param>
/// <param name="e">包含项重排事件数据的参数.</param>
public delegate void ItemReorderedEventHandler(object sender, ItemReorderedEventArgs e);

/// <summary>
/// 为项重排（Reordered）事件提供数据的类.
/// </summary>
public class ItemReorderedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemReorderedEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识符.</param>
    /// <param name="source">引发事件的源对象.</param>
    /// <param name="oldIndex">项在重排前的原始索引位置.</param>
    /// <param name="newIndex">项在重排后的新索引位置.</param>
    internal ItemReorderedEventArgs(RoutedEvent routedEvent, object source, int oldIndex, int newIndex)
        : base(routedEvent, source)
    {
        this.NewIndex = newIndex;
        this.OldIndex = oldIndex;
    }

    /// <summary>
    /// Gets 项在重排前的原始索引位置.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// Gets 项在重排后的新索引位置.
    /// </summary>
    public int NewIndex { get; }
}