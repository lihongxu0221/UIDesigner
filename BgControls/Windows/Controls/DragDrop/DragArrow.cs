namespace BgControls.Windows.Controls.DragDrop;

/// <summary>
/// 表示用于拖拽指示的箭头控件.
/// </summary>
public class DragArrow : ContentControl
{
    /// <summary>
    /// Initializes static members of the <see cref="DragArrow"/> class.
    /// </summary>
    static DragArrow()
    {
        // 覆盖默认样式键，以便控件能够从通用样式资源（如 Generic.xaml）中加载其模板.
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DragArrow),
            new FrameworkPropertyMetadata(typeof(DragArrow)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DragArrow"/> class.
    /// </summary>
    public DragArrow()
    {
        // 构造函数逻辑.
    }

    /// <summary>
    /// 当控件进入初始化阶段时触发的逻辑.
    /// </summary>
    /// <param name="e">包含事件数据的 <see cref="EventArgs"/> 对象.</param>
    protected override void OnInitialized(EventArgs e)
    {
        // 调用基类的初始化方法以确保标准生命周期逻辑执行.
        base.OnInitialized(e);

        // 如果需要手动强制指定样式键，可以在此处取消注释.
        // StyleManager.SetDefaultStyleKey(this, typeof(DragArrow));
    }
}