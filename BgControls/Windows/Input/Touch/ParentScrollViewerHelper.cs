namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 父级滚动视图辅助类，用于处理 UI 元素与其视觉树中的父级 ScrollViewer 之间的关联.
/// </summary>
internal class ParentScrollViewerHelper : ScrollViewerHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParentScrollViewerHelper"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    internal ParentScrollViewerHelper(UIElement element)
        : base(element)
    {
        // 构造函数逻辑通过基类 base(element) 完成初始化.
    }

    /// <summary>
    /// Gets a value indicating whether 父级滚动视图是否支持惯性.
    /// </summary>
    /// <remarks>
    /// 在该辅助类中固定返回 false，以防止与父级容器的原生惯性逻辑冲突.
    /// </remarks>
    public override bool SupportsInertia => false;

    /// <summary>
    /// 获取指定 UI 元素在视觉树中的父级滚动视图.
    /// </summary>
    /// <param name="uiElement">目标 UI 元素.</param>
    /// <returns>在视觉树中找到的第一个父级 ScrollViewer 实例; 若未找到则返回 null.</returns>
    public override ScrollViewer? GetScrollViewer(UIElement uiElement)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(uiElement, nameof(uiElement));

        // 向上查找视觉树中的父级滚动视图.
        return uiElement.GetVisualParent<ScrollViewer>();
    }
}