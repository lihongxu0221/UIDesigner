namespace BgControls.Windows.DragDrop;

/// <summary>
/// 表示专门用于列表框（ListBox）拖拽操作的视觉呈现控件.
/// </summary>
public class ListBoxDragVisual : DragVisual
{
    /// <summary>
    /// Initializes static members of the <see cref="ListBoxDragVisual"/> class.
    /// </summary>
    static ListBoxDragVisual()
    {
        // 覆盖默认样式键，以确保控件能够正确加载其在主题资源中定义的模板.
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ListBoxDragVisual),
            new FrameworkPropertyMetadata(typeof(ListBoxDragVisual)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListBoxDragVisual"/> class.
    /// </summary>
    public ListBoxDragVisual()
    {
        // 构造函数初始化逻辑.
    }

    /// <summary>
    /// 当控件进入初始化阶段时执行的逻辑.
    /// </summary>
    /// <param name="e">包含事件数据的 <see cref="EventArgs"/> 对象.</param>
    protected override void OnInitialized(EventArgs e)
    {
        // 调用基类的初始化逻辑，确保标准的 WPF 控件生命周期被正确执行.
        base.OnInitialized(e);

        // 如果需要强制手动应用样式，可以在此处处理.
        // StyleManager.SetDefaultStyleKey(this, typeof(ListBoxDragVisual));
    }
}