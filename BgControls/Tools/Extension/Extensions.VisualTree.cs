namespace BgControls;

/// <summary>
/// DependencyObject 帮助类.
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// 获取可视化树父级.
    /// </summary>
    /// <param name="child">子控件.</param>
    /// <param name="targetTypes">目标父级类型.</param>
    /// <returns>可视化树父级.</returns>
    private static DependencyObject? GetParent(this DependencyObject child, params Type[] targetTypes)
    {
        DependencyObject? parent = null;
        if (child is Visual)
        {
            parent = VisualTreeHelper.GetParent(child);

            // 递归向上查找，直到找到目标类型或没有父级
            while (parent != null)
            {
                if (targetTypes.Length > 0 && targetTypes.Any(t => t.IsInstanceOfType(parent)))
                {
                    return parent;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        return parent;
    }

    /// <summary>
    /// 获取逻辑树父级.
    /// </summary>
    /// <param name="child">子控件.</param>
    /// <param name="targetTypes">目标父级类型.</param>
    /// <returns>逻辑树父级.</returns>
    private static DependencyObject? GetLogicalParent(this DependencyObject child, params Type[] targetTypes)
    {
        DependencyObject? parent = LogicalTreeHelper.GetParent(child);

        // 递归向上查找，直到找到目标类型或没有父级
        while (parent != null)
        {
            if (targetTypes.Length > 0 && targetTypes.Any(t => t.IsInstanceOfType(parent)))
            {
                return parent;
            }

            parent = LogicalTreeHelper.GetParent(parent);
        }

        return parent;
    }

    /// <summary>
    /// 查找指定类型的父级.
    /// </summary>
    /// <param name="child">子控件.</param>
    /// <param name="targetTypes">目标父级类型.</param>
    /// <returns>父级别.</returns>
    public static DependencyObject? FindParent(this DependencyObject child, params Type[] targetTypes)
    {
        // 先尝试从可视化树查找
        DependencyObject? visualParent = child.GetParent(targetTypes);
        if (visualParent != null)
        {
            return visualParent;
        }

        // 如果可视化树未找到，再从逻辑树查找
        return child.GetLogicalParent(targetTypes);
    }

    /// <summary>
    /// 获取可视化树子控件.
    /// </summary>
    /// <typeparam name="T">子控件类型.</typeparam>
    /// <param name="parent">父控件.</param>
    /// <param name="name">子控件名称.</param>
    /// <returns>子控件.</returns>
    public static T? FindVisualChild<T>(this DependencyObject parent, string? name = null)
        where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result &&
               (string.IsNullOrEmpty(name) || (result as FrameworkElement)?.Name == name))
            {
                return result;
            }
            else
            {
                T? descendant = FindVisualChild<T>(child, name);
                if (descendant != null)
                {
                    return descendant;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 获取可视化树子控件.
    /// </summary>
    /// <param name="parent">父控件.</param>
    /// <param name="childType">子控件类型.</param>
    /// <param name="name">子控件名称.</param>
    /// <returns>子控件.</returns>
    public static DependencyObject? FindVisualChild(this DependencyObject parent, Type childType, string? name = null)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child.GetType() == childType &&
               (string.IsNullOrEmpty(name) || (child as FrameworkElement)?.Name == name))
            {
                return child;
            }
            else
            {
                DependencyObject? descendant = child.FindVisualChild(childType, name);
                if (descendant != null)
                {
                    return descendant;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 在可视化树中查找指定类型的祖先节点.
    /// </summary>
    /// <typeparam name="T">祖先节点的类型.</typeparam>
    /// <param name="child">起始子项实例.</param>
    /// <returns>找到的匹配祖先节点实例.</returns>
    public static T? FindAncestor<T>(this DependencyObject child)
        where T : DependencyObject
    {
        DependencyObject? current = child;
        while (current != null)
        {
            if (current is T ancestor)
            {
                return ancestor;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    /// <summary>
    /// 获取当前元素下所有指定类型的后代元素集合.
    /// </summary>
    /// <typeparam name="T">目标后代元素的类型.</typeparam>
    /// <param name="element">起始元素.</param>
    /// <returns>匹配类型的后代元素枚举集合.</returns>
    public static IEnumerable<T>? ChildrenOfType<T>(this DependencyObject element)
        where T : DependencyObject
    {
        return GetChildrenRecursive(element).OfType<T>();
    }

    /// <summary>
    /// 查找当前元素下第一个匹配指定类型的后代元素.
    /// </summary>
    /// <typeparam name="T">目标后代元素的类型.</typeparam>
    /// <param name="element">起始元素.</param>
    /// <returns>第一个匹配的后代元素实例.</returns>
    public static T? FindChildByType<T>(this DependencyObject element)
        where T : DependencyObject
    {
        return ChildrenOfType<T>(element)?.FirstOrDefault();
    }

    /// <summary>
    /// 查找当前元素下所有匹配指定类型的后代元素集合（内部使用）.
    /// </summary>
    /// <typeparam name="T">目标元素的类型.</typeparam>
    /// <param name="element">起始元素.</param>
    /// <returns>匹配类型的元素的集合.</returns>
    internal static IEnumerable<T>? FindChildrenByType<T>(this DependencyObject element)
        where T : DependencyObject
    {
        return ChildrenOfType<T>(element);
    }

    /// <summary>
    /// 根据名称查找子元素，优先使用名称索引查找，其次搜索后代集合.
    /// </summary>
    /// <param name="element">起始元素.</param>
    /// <param name="name">后代元素的名称.</param>
    /// <returns>匹配类型的元素.</returns>
    internal static FrameworkElement? GetChildByName(this FrameworkElement element, string name)
    {
        return ((FrameworkElement?)element.FindName(name)) ??
            ChildrenOfType<FrameworkElement>(element)?.FirstOrDefault(c => c.Name == name);
    }

    /// <summary>
    /// 获取当前元素或其后代中第一个匹配指定类型的元素.
    /// </summary>
    /// <typeparam name="T">目标元素的类型.</typeparam>
    /// <param name="target">起始元素.</param>
    /// <returns>匹配类型的元素.</returns>
    internal static T? GetFirstDescendantOfType<T>(this DependencyObject target)
        where T : DependencyObject
    {
        return (target as T) ??
            ChildrenOfType<T>(target)?.FirstOrDefault();
    }

    /// <summary>
    /// 获取当前父元素下所有指定类型的框架元素（FrameworkElement）后代.
    /// </summary>
    /// <typeparam name="T">目标元素的类型.</typeparam>
    /// <param name="parent">起始元素.</param>
    /// <returns>匹配类型的后代元素集合.</returns>
    internal static IEnumerable<T>? GetChildren<T>(this DependencyObject parent)
        where T : FrameworkElement
    {
        return GetChildrenRecursive(parent).OfType<T>();
    }

    /// <summary>
    /// 获取当前元素下所有指定类型的后代元素，但跳过指定类型的子树搜索.
    /// </summary>
    /// <typeparam name="T">目标后代元素的类型.</typeparam>
    /// <param name="element">起始元素.</param>
    /// <param name="typeWhichChildrenShouldBeSkipped">需要跳过其子树搜索的类型.</param>
    /// <returns>匹配类型的后代元素集合.</returns>
    internal static IEnumerable<T>? ChildrenOfType<T>(this DependencyObject element, Type typeWhichChildrenShouldBeSkipped)
    {
        return GetChildrenOfType(element, typeWhichChildrenShouldBeSkipped)?.OfType<T>();
    }

    /// <summary>
    /// 以递归方式获取可视化树中的所有后代节点.
    /// </summary>
    /// <param name="element">起始元素.</param>
    /// <returns>所有后代节点的延迟加载枚举集合.</returns>
    private static IEnumerable<DependencyObject> GetChildrenRecursive(this DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            if (child == null)
            {
                continue;
            }

            yield return child;

            foreach (DependencyObject descendant in GetChildrenRecursive(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// 递归获取所有后代节点，但跳过指定类型节点的子级搜索.
    /// </summary>
    /// <param name="element">起始元素.</param>
    /// <param name="skipType">遇到此类型时停止向下搜索其子级.</param>
    /// <returns>后代节点的延迟加载枚举集合.</returns>
    private static IEnumerable<DependencyObject> GetChildrenOfType(this DependencyObject element, Type skipType)
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            yield return child;

            if (skipType.IsAssignableFrom(child.GetType()))
            {
                continue;
            }

            foreach (var descendant in GetChildrenOfType(child, skipType))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// 向上查找并返回第一个匹配指定类型的父级节点.
    /// </summary>
    /// <typeparam name="T">要查找的父级类型.</typeparam>
    /// <param name="element">起始依赖对象.</param>
    /// <returns>找到的第一个匹配类型的父级实例；如果未找到则返回 null.</returns>
    public static T? ParentOfType<T>(this DependencyObject element)
        where T : DependencyObject
    {
        if (element == null)
        {
            return null;
        }

        // 循环向上查找以提高性能，避免生成不必要的迭代器对象.
        DependencyObject? current = element;
        while ((current = GetParent(current)) != null)
        {
            if (current is T typedParent)
            {
                return typedParent;
            }
        }

        return null;
    }

    public static T? ParentOfType1<T>(this DependencyObject element)
        where T : DependencyObject
    {
        if (element == null)
        {
            return null;
        }

        return GetParents(element).OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// 确定当前元素是否是指定后代元素的祖先.
    /// </summary>
    /// <param name="element">作为潜在祖先的元素.</param>
    /// <param name="descendant">作为潜在后代的元素.</param>
    /// <returns>如果是祖先关系（或二者为同一对象）则返回 true；否则返回 false.</returns>
    public static bool IsAncestorOf(this DependencyObject element, DependencyObject descendant)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        ArgumentNullException.ThrowIfNull(descendant, nameof(descendant));

        // 逐级向上对比引用，避免将整条链路存入内存.
        DependencyObject? current = descendant;
        while (current != null)
        {
            if (current == element)
            {
                return true;
            }

            current = GetParent(current);
        }

        return false;
    }

    /// <summary>
    /// 判断一个节点是否是另一个指定祖先节点的后代.
    /// </summary>
    /// <param name="leaf">待检测的子节点.</param>
    /// <param name="ancestor">指定的祖先节点.</param>
    /// <returns>如果是后代则返回 true；否则返回 false.</returns>
    public static bool IsDescendantOf(this DependencyObject? leaf, DependencyObject? ancestor)
    {
        DependencyObject? lastTrackedNode = null;

        // 首先在视觉树祖先链中进行匹配.
        foreach (DependencyObject currentNode in GetVisualAncestry(leaf))
        {
            if (object.Equals(currentNode, ancestor))
            {
                return true;
            }

            // 记录最后一个访问到的视觉节点.
            lastTrackedNode = currentNode;
        }

        // 如果视觉树未找到，则尝试从最后的视觉节点开始在逻辑树中查找.
        return lastTrackedNode?.GetLogicalAncestry()?.Contains(ancestor) ?? false;
    }

    /// <summary>
    /// 获取可视化树中的父级节点（语义化别名）.
    /// </summary>
    /// <typeparam name="T">目标父级类型.</typeparam>
    /// <param name="element">当前元素.</param>
    /// <returns>匹配类型的父级实例.</returns>
    public static T? GetVisualParent<T>(this DependencyObject element)
        where T : DependencyObject
    {
        return ParentOfType<T>(element);
    }

    /// <summary>
    /// 获取当前元素的所有祖先节点集合.
    /// </summary>
    /// <typeparam name="T">要过滤的祖先类型.</typeparam>
    /// <param name="element">当前元素.</param>
    /// <returns>匹配类型的祖先节点枚举集合.</returns>
    internal static IEnumerable<T> GetAncestors<T>(this DependencyObject element)
        where T : class
    {
        return GetParents(element).OfType<T>();
    }

    /// <summary>
    /// 获取指定类型的直接或间接父级节点.
    /// </summary>
    /// <typeparam name="T">父级框架元素类型.</typeparam>
    /// <param name="element">当前元素.</param>
    /// <returns>匹配类型的父级实例.</returns>
    internal static T? GetParent<T>(this DependencyObject element)
        where T : FrameworkElement
    {
        return ParentOfType<T>(element);
    }

    /// <summary>
    /// 以延迟加载方式获取当前元素的所有父级节点序列.
    /// </summary>
    /// <param name="element">起始元素.</param>
    /// <returns>从直接父级开始逐级向上的依赖对象枚举序列.</returns>
    public static IEnumerable<DependencyObject> GetParents(this DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        DependencyObject? current = element;
        while ((current = GetParent(current)) != null)
        {
            yield return current;
        }
    }

    /// <summary>
    /// 获取指定元素的父级节点，优先搜索可视化树，其次搜索逻辑树.
    /// </summary>
    /// <param name="element">当前元素.</param>
    /// <returns>父级依赖对象；如果已到达顶端则返回 null.</returns>
    private static DependencyObject? GetParent(this DependencyObject element)
    {
        if (element == null)
        {
            return null;
        }

        // 仅对视觉对象尝试获取可视化树父级，以避免不必要的异常.
        DependencyObject? parent = null;
        if (element is Visual || element is Visual3D)
        {
            try
            {
                parent = VisualTreeHelper.GetParent(element);
            }
            catch (InvalidOperationException)
            {
                parent = null;
            }
        }

        // 如果可视化树中未找到父级，则尝试通过逻辑树或属性所有权查找.
        if (parent == null)
        {
            if (element is FrameworkElement frameworkElement)
            {
                parent = frameworkElement.Parent;
            }
            else if (element is FrameworkContentElement frameworkContentElement)
            {
                parent = frameworkContentElement.Parent;
            }
        }

        return parent;
    }



    /// <summary>
    /// 对视觉树进行真正的广度优先遍历.
    /// </summary>
    /// <param name="node">开始遍历的起始节点. </param>
    /// <returns>广度优先遍历的节点枚举序列. </returns>
    public static IEnumerable<DependencyObject> VisualBreadthFirstTraversal(this DependencyObject node)
    {
        // 检查起始节点是否为空.
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        // 使用队列存储待访问的节点，实现先入先出（层级遍历）.
        Queue<DependencyObject> pendingNodes = new Queue<DependencyObject>();

        // 将起始节点加入队列.
        pendingNodes.Enqueue(node);

        // 当队列不为空时，持续进行处理.
        while (pendingNodes.Count > 0)
        {
            // 从队列中取出当前节点并返回.
            DependencyObject currentNode = pendingNodes.Dequeue();
            yield return currentNode;

            // 获取当前节点在视觉树中的子节点数量.
            int childrenCount = VisualTreeHelper.GetChildrenCount(currentNode);

            // 遍历所有直接子节点并加入队列.
            for (int i = 0; i < childrenCount; i++)
            {
                // 获取索引位置的子节点.
                DependencyObject childNode = VisualTreeHelper.GetChild(currentNode, i);

                // 将子节点入队，等待下一轮处理.
                pendingNodes.Enqueue(childNode);
            }
        }
    }

    /// <summary>
    /// 对视觉树进行非递归的深度优先遍历.
    /// </summary>
    /// <param name="node">开始遍历的起始节点. </param>
    /// <returns>深度优先遍历的节点枚举序列. </returns>
    public static IEnumerable<DependencyObject> VisualDepthFirstTraversal(this DependencyObject node)
    {
        // 验证起始节点是否为空.
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        // 使用栈结构模拟递归调用栈，避免深层树结构导致栈溢出.
        Stack<DependencyObject> stack = new Stack<DependencyObject>();

        // 将起始节点压入栈中.
        stack.Push(node);

        // 当栈中仍有节点时持续进行迭代.
        while (stack.Count > 0)
        {
            // 弹出当前待访问的节点.
            DependencyObject currentNode = stack.Pop();

            // 返回当前节点.
            yield return currentNode;

            // 获取当前节点的子节点总数.
            int childrenCount = VisualTreeHelper.GetChildrenCount(currentNode);

            // 为了保持与递归遍历相同的顺序（先访问第一个子节点），需要逆序将子节点压入栈中.
            // 这样出栈时，索引较小的子节点会先被弹出.
            for (int i = childrenCount - 1; i >= 0; i--)
            {
                // 获取指定索引位置的子节点.
                DependencyObject childNode = VisualTreeHelper.GetChild(currentNode, i);

                // 将子节点压入栈顶.
                stack.Push(childNode);
            }
        }
    }

    /// <summary>
    /// 获取完整的视觉祖先链，从叶子节点开始向上追踪.
    /// </summary>
    /// <remarks>
    /// 如果元素不是 Visual 或 Visual3D 类型，则尝试使用逻辑树溯源.
    /// </remarks>
    /// <param name="leaf">开始溯源的叶子节点.</param>
    /// <returns>视觉祖先节点序列.</returns>
    public static IEnumerable<DependencyObject> GetVisualAncestry(this DependencyObject? leaf)
    {
        // 当节点不为空时持续向上寻找父级.
        while (leaf != null)
        {
            yield return leaf;

            // 判断当前节点是否属于视觉对象，若是则通过视觉树查找，否则通过逻辑树查找.
            leaf = (leaf is Visual || leaf is Visual3D)
                ? VisualTreeHelper.GetParent(leaf)
                : LogicalTreeHelper.GetParent(leaf);
        }
    }

    /// <summary>
    /// 获取完整的逻辑祖先链.
    /// </summary>
    /// <param name="leaf">开始溯源的叶子节点.</param>
    /// <returns>逻辑祖先节点序列.</returns>
    public static IEnumerable<DependencyObject?> GetLogicalAncestry(this DependencyObject leaf)
    {
        // 验证起始节点是否为空.
        ArgumentNullException.ThrowIfNull(leaf, nameof(leaf));

        DependencyObject? current = leaf;

        // 持续查找逻辑父级直至根节点.
        while (current != null)
        {
            yield return current;
            current = LogicalTreeHelper.GetParent(current);
        }
    }

    /// <summary>
    /// 为指定的依赖对象添加路由事件处理程序.
    /// </summary>
    /// <param name="dependencyObject">目标依赖对象，必须为 <see cref="UIElement"/>、<see cref="ContentElement"/> 或 <see cref="UIElement3D"/>.</param>
    /// <param name="routedEvent">要处理的路由事件标识符.</param>
    /// <param name="handler">事件处理程序委派.</param>
    /// <param name="handledEventsToo">如果为 <see langword="true"/>，则即使事件已在路由中标记为已处理，也会调用该处理程序.</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="dependencyObject"/> 为空时抛出.</exception>
    /// <exception cref="ArgumentException">当对象类型不支持路由事件处理程序时抛出.</exception>
    public static void AddHandler(this DependencyObject dependencyObject, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
    {
        ArgumentNullException.ThrowIfNull(dependencyObject, nameof(dependencyObject));

        if (dependencyObject is UIElement uiElement)
        {
            uiElement.AddHandler(routedEvent, handler, handledEventsToo);
        }
        else if (dependencyObject is ContentElement contentElement)
        {
            contentElement.AddHandler(routedEvent, handler, handledEventsToo);
        }
        else if (dependencyObject is UIElement3D uiElement3D)
        {
            uiElement3D.AddHandler(routedEvent, handler, handledEventsToo);
        }
        else
        {
            throw new ArgumentException("目标对象必须是支持路由事件的类型（如 UIElement 或 ContentElement）.", nameof(dependencyObject));
        }
    }

    /// <summary>
    /// 从指定的依赖对象中移除路由事件处理程序.
    /// </summary>
    /// <param name="dependencyObject">目标依赖对象.</param>
    /// <param name="routedEvent">路由事件标识符.</param>
    /// <param name="handler">要移除的事件处理程序委派.</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="dependencyObject"/> 为空时抛出.</exception>
    /// <exception cref="ArgumentException">当对象类型不支持路由事件处理程序时抛出.</exception>
    public static void RemoveHandler(this DependencyObject dependencyObject, RoutedEvent routedEvent, Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(dependencyObject, nameof(dependencyObject));

        if (dependencyObject is UIElement uiElement)
        {
            uiElement.RemoveHandler(routedEvent, handler);
        }
        else if (dependencyObject is ContentElement contentElement)
        {
            contentElement.RemoveHandler(routedEvent, handler);
        }
        else if (dependencyObject is UIElement3D uiElement3D)
        {
            uiElement3D.RemoveHandler(routedEvent, handler);
        }
        else
        {
            throw new ArgumentException("目标对象必须是支持路由事件的类型（如 UIElement 或 ContentElement）.", nameof(dependencyObject));
        }
    }

    /// <summary>
    /// 在指定的依赖对象上引发路由事件.
    /// </summary>
    /// <param name="dependencyObject">引发事件的依赖对象.</param>
    /// <param name="routedEventArgs">包含事件数据的参数对象.</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="dependencyObject"/> 为空时抛出.</exception>
    /// <exception cref="ArgumentException">当对象未实现 <see cref="IInputElement"/> 接口时抛出.</exception>
    public static void RaiseEvent(this DependencyObject dependencyObject, RoutedEventArgs routedEventArgs)
    {
        ArgumentNullException.ThrowIfNull(dependencyObject, nameof(dependencyObject));

        if (dependencyObject is IInputElement inputElement)
        {
            inputElement.RaiseEvent(routedEventArgs);
        }
        else
        {
            throw new ArgumentException("目标对象必须实现 IInputElement 接口才能引发路由事件.", nameof(dependencyObject));
        }
    }
}