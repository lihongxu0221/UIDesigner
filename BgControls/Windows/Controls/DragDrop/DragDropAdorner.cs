namespace BgControls.Windows.Controls.DragDrop;

/// <summary>
/// 表示用于拖放操作的装饰器类.
/// </summary>
internal class DragDropAdorner : Adorner, IDisposable
{
    /// <summary>
    /// 视觉子元素集合.
    /// </summary>
    private VisualCollection visualChildren;

    /// <summary>
    /// 拖拽面板.
    /// </summary>
    private Panel dragPanel;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragDropAdorner"/> class.
    /// </summary>
    /// <param name="adornedElement">被装饰的元素.</param>
    /// <param name="dragPanel">承载拖拽内容的面板.</param>
    public DragDropAdorner(UIElement adornedElement, Panel dragPanel)
        : base(adornedElement)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(adornedElement, nameof(adornedElement));
        ArgumentNullException.ThrowIfNull(dragPanel, nameof(dragPanel));

        // 初始化视觉集合并将面板添加为子元素.
        this.visualChildren = new VisualCollection(this);
        this.dragPanel = dragPanel;
        this.visualChildren.Add(this.dragPanel);
    }

    /// <summary>
    /// Gets 获取视觉子元素的数量.
    /// </summary>
    protected override int VisualChildrenCount
    {
        get
        {
            return this.visualChildren.Count;
        }
    }

    /// <summary>
    /// 执行释放资源的操作.
    /// </summary>
    public void Dispose()
    {
        // 清空视觉子元素集合以解除引用.
        this.visualChildren.Clear();
    }

    /// <summary>
    /// 获取指定索引处的视觉子元素.
    /// </summary>
    /// <param name="index">子元素的索引.</param>
    /// <returns>视觉子元素.</returns>
    protected override Visual GetVisualChild(int index)
    {
        // 从集合中返回对应的视觉对象.
        return this.visualChildren[index];
    }

    /// <summary>
    /// 安排子元素的位置并定义装饰器的大小.
    /// </summary>
    /// <param name="finalSize">装饰器最终占用的区域大小.</param>
    /// <returns>实际使用的尺寸.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        // 获取当前装饰器的期望尺寸.
        double desiredWidth = this.DesiredSize.Width;
        double desiredHeight = this.DesiredSize.Height;

        // 安排拖拽面板充满整个装饰器区域.
        this.dragPanel.Arrange(new Rect(0.0, 0.0, desiredWidth, desiredHeight));

        return finalSize;
    }
}