namespace BgControls.Windows.Controls.ToolTips;

/// <summary>
/// 维护一个视觉元素栈，用于管理和跟踪当前处于激活状态的元素层次结构.
/// </summary>
internal class VisualElementsStack
{
    private FrameworkElement? topElement;
    private LinkedList<FrameworkElement> orderedElements = new LinkedList<FrameworkElement>();
    private HashSet<FrameworkElement> orderedElementsSet = new HashSet<FrameworkElement>();

    /// <summary>
    /// Gets 当前位于栈顶（视觉层级最深）的元素.
    /// </summary>
    public FrameworkElement? TopElement => this.topElement;

    /// <summary>
    /// 当栈顶元素发生更改时触发的事件.
    /// </summary>
    public event EventHandler<TopElementChangedEventArgs>? TopElementChanged;

    /// <summary>
    /// 向栈中添加一个元素，并根据其在视觉树中的位置进行排序.
    /// </summary>
    /// <param name="element">要添加的框架元素.</param>
    public void Add(FrameworkElement element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        // 如果集合为空，直接作为第一个元素添加.
        if (this.orderedElements.Count == 0)
        {
            this.orderedElements.AddFirst(element);
            this.orderedElementsSet.Add(element);
        }
        else
        {
            // 根据层级逻辑寻找合适的位置插入.
            this.InsertElement(element);
        }

        // 更新并验证当前的顶层元素.
        this.UpdateTopElement();
    }

    /// <summary>
    /// 从栈中移除指定的元素，并同步更新状态.
    /// </summary>
    /// <param name="element">要移除的框架元素.</param>
    public void Remove(FrameworkElement element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        // 从链表和哈希表中同步移除.
        this.orderedElements.Remove(element);
        this.orderedElementsSet.Remove(element);

        // 移除后重新计算顶层元素.
        this.UpdateTopElement();
    }

    /// <summary>
    /// 核心逻辑方法：根据视觉树的父子关系将元素插入到有序链表中.
    /// </summary>
    /// <param name="element">待插入的元素.</param>
    private void InsertElement(FrameworkElement element)
    {
        // 此处假设 element 不为 null，外部 Add 方法已校验.
        FrameworkElement? currentAncestor = element;

        // 向上遍历视觉树，寻找第一个已经存在于当前栈中的祖先节点.
        while (true)
        {
            if (currentAncestor == null)
            {
                // 如果没有任何祖先在栈中，说明是新的独立分支，插入到链表头部.
                this.orderedElements.AddFirst(element);
                this.orderedElementsSet.Add(element);
                return;
            }

            // 检查当前遍历到的节点是否已在集合中.
            if (this.orderedElementsSet.Contains(currentAncestor))
            {
                break;
            }

            // 继续向上寻找父级.
            currentAncestor = VisualTreeHelper.GetParent(currentAncestor) as FrameworkElement;
        }

        // 逻辑：将当前元素插入到其最近祖先节点的后面，以保持“父在前，子在后”的顺序.
        var node = this.orderedElements.Find(currentAncestor);
        if (node != null)
        {
            this.orderedElements.AddAfter(node, element);
            this.orderedElementsSet.Add(element);
        }
    }

    /// <summary>
    /// 更新顶层元素标识，并在发生更改时引发通知事件.
    /// </summary>
    private void UpdateTopElement()
    {
        // 链表的最后一个元素被定义为视觉上的“最顶层”元素.
        var newTop = this.orderedElements.Count > 0 ? this.orderedElements.Last?.Value : null;

        if (this.topElement != newTop)
        {
            FrameworkElement? oldTop = this.topElement;
            this.topElement = newTop;

            // 触发事件通知订阅者.
            this.RaiseTopElementChanged(oldTop);
        }
    }

    /// <summary>
    /// 引发顶层元素更改事件的内部方法.
    /// </summary>
    /// <param name="oldTopElement">更改前的旧顶层元素.</param>
    private void RaiseTopElementChanged(FrameworkElement? oldTopElement)
    {
        // 仅在事件已订阅且新顶层元素不为空时引发.
        if (this.TopElementChanged != null && this.topElement != null)
        {
            this.TopElementChanged.Invoke(this, new TopElementChangedEventArgs(oldTopElement, this.topElement));
        }
    }
}