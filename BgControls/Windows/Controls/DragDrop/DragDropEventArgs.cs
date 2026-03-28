using BgControls.Windows.Controls;
using System.Windows;

namespace BgControls.Windows.Controls.DragDrop;

/// <summary>
/// 表示拖放事件的参数类.
/// </summary>
public class DragDropEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DragDropEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">路由事件标识.</param>
    /// <param name="source">事件源对象.</param>
    /// <param name="options">拖放选项配置.</param>
    public DragDropEventArgs(RoutedEvent? routedEvent, object? source, DragDropOptions? options)
        : base(routedEvent, source)
    {
        // 验证拖放选项参数是否为空.
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        // 赋值属性.
        this.Options = options;
    }

    /// <summary>
    /// Gets 拖放选项配置.
    /// </summary>
    public DragDropOptions Options { get; internal set; }

    /// <summary>
    /// 根据指定的拖拽点坐标获取对应的元素.
    /// </summary>
    /// <typeparam name="T">元素类型，必须继承自 FrameworkElement.</typeparam>
    /// <param name="dragPoint">相对于参考坐标系的拖拽点.</param>
    /// <returns>查找到的类型为 T 的首个元素；如果未找到则返回 null.</returns>
    public T? GetElement<T>(Point dragPoint)
        where T : FrameworkElement
    {
        // 1. 优先尝试通过全局根视觉对象转换坐标并查找.
        if (ApplicationHelper.RootVisual != null)
        {
            // 将点坐标转换为屏幕坐标.
            Point screenPoint = ApplicationHelper.RootVisual.PointToScreen(dragPoint);

            // 从屏幕坐标系中获取符合条件的元素集合并返回第一个.
            return Extensions.GetElementsInScreenCoordinates<T>(screenPoint).FirstOrDefault();
        }

        // 2. 如果全局根对象不可用，则尝试查找相关的参考元素进行坐标转换.
        FrameworkElement? referenceElement = this.GetRelativeElement();
        if (referenceElement != null)
        {
            // 获取参考元素所属的根视觉对象.
            Visual? rootVisual = ApplicationHelper.GetRootVisual(referenceElement);
            if (rootVisual != null)
            {
                // 将点坐标转换为屏幕坐标.
                dragPoint = rootVisual.PointToScreen(dragPoint);
            }

            // 在参考元素所在的屏幕坐标范围内查找元素.
            return referenceElement.GetElementsInScreenCoordinates<T>(dragPoint).FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// 以类型安全的方式调用事件处理程序.
    /// </summary>
    /// <param name="genericHandler">通用的委托处理程序.</param>
    /// <param name="genericTarget">事件触发的目标对象.</param>
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        // 尝试将委托转换为指定的处理程序类型并执行.
        (genericHandler as EventHandler<DragDropEventArgs>)?.Invoke(genericTarget, this);
    }

    /// <summary>
    /// 根据拖放选项获取用于坐标转换或上下文引用的相关框架元素.
    /// </summary>
    /// <returns>查找到的参考元素；如果均不存在则返回 null.</returns>
    private FrameworkElement? GetRelativeElement()
    {
        FrameworkElement? targetElement = null;

        // 优先级 1：优先使用拖拽源元素.
        if (this.Options.Source != null)
        {
            targetElement = this.Options.Source;
        }

        // 优先级 2：如果源不存在，则尝试使用目标元素.
        if (targetElement == null && this.Options.Destination != null)
        {
            targetElement = this.Options.Destination;
        }

        // 优先级 3：如果上述均不存在，则尝试使用参与拖拽的视觉根节点列表中的第一个元素.
        if (targetElement == null && this.Options.ParticipatingVisualRoots != null && this.Options.ParticipatingVisualRoots.Count > 0)
        {
            targetElement = this.Options.ParticipatingVisualRoots.First() as FrameworkElement;
        }

        return targetElement;
    }
}