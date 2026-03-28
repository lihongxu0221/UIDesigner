namespace BgControls.Windows.Controls;

/// <summary>
/// 窗口互操作性辅助类，提供用于存储和获取窗口适配器的附加属性.
/// </summary>
public static class WindowInteropabilityHelper
{
    /// <summary>
    /// 窗口互操作性适配器的附加属性.
    /// </summary>
    public static readonly DependencyProperty WindowInteropabilityAdapterProperty =
        DependencyProperty.RegisterAttached("WindowInteropabilityAdapter", typeof(IWindowInteropabilityAdapter), typeof(WindowInteropabilityHelper), null);

    /// <summary>
    /// 获取指定对象的窗口互操作性适配器.
    /// </summary>
    /// <param name="dependencyObject">目标对象.</param>
    /// <returns>返回实现的 <see cref="IWindowInteropabilityAdapter"/> 接口实例.</returns>
    public static IWindowInteropabilityAdapter GetWindowInteropabilityAdapter(DependencyObject dependencyObject)
    {
        // 验证参数不为空.
        ArgumentNullException.ThrowIfNull(dependencyObject, nameof(dependencyObject));

        // 从依赖对象中获取附加属性值.
        return (IWindowInteropabilityAdapter)dependencyObject.GetValue(WindowInteropabilityHelper.WindowInteropabilityAdapterProperty);
    }

    /// <summary>
    /// 设置指定对象的窗口互操作性适配器.
    /// </summary>
    /// <param name="dependencyObject">目标对象.</param>
    /// <param name="adapterValue">适配器实例.</param>
    public static void SetWindowInteropabilityAdapter(DependencyObject dependencyObject, IWindowInteropabilityAdapter adapterValue)
    {
        // 验证参数不为空.
        ArgumentNullException.ThrowIfNull(dependencyObject, nameof(dependencyObject));

        // 将适配器实例设置到附加属性中.
        dependencyObject.SetValue(WindowInteropabilityHelper.WindowInteropabilityAdapterProperty, adapterValue);
    }

    /// <summary>
    /// 查找指定对象在视觉树或逻辑树中的父级窗口互操作性适配器.
    /// </summary>
    /// <param name="dependencyObject">起始查找对象.</param>
    /// <returns>找到的第一个适配器，若未找到则返回 null.</returns>
    internal static IWindowInteropabilityAdapter? GetParentWindowInteropabilityAdapter(DependencyObject dependencyObject)
    {
        // 验证参数不为空.
        ArgumentNullException.ThrowIfNull(dependencyObject, nameof(dependencyObject));

        // 获取所有父级序列，并筛选出第一个配置了适配器的节点.
        return ApplicationHelper.GetParents(dependencyObject)
                                .Select(parent => GetWindowInteropabilityAdapter(parent))
                                .OfType<IWindowInteropabilityAdapter>()
                                .FirstOrDefault();
    }
}