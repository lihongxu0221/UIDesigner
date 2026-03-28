namespace BgControls.Windows.Controls;

/// <summary>
/// 为预览选项卡更改（PreviewTabChanged）路由事件提供数据的类.
/// 该类允许订阅者在选项卡切换完成前拦截并取消该操作.
/// </summary>
public class PreviewTabChangedEventArgs : TabChangedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewTabChangedEventArgs"/> class.
    /// </summary>
    /// <param name="tabItem">即将被选中的目标选项卡项.</param>
    /// <param name="viewModel">与该选项卡关联的视图模型对象.</param>
    /// <param name="routedEvent">关联的路由事件标识符.</param>
    /// <param name="source">引发事件的源对象.</param>
    public PreviewTabChangedEventArgs(BgTabItem tabItem, object viewModel, RoutedEvent routedEvent, object source)
        : base(tabItem, viewModel, routedEvent, source)
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否应取消当前的选项卡更改操作.
    /// </summary>
    /// <value>
    /// 如果应取消切换操作，则为 <see langword="true"/>；否则为 <see langword="false"/>.
    /// </value>
    public bool Cancel { get; set; }
}