using BgControls.Windows.Datas;

namespace BgControls.Windows.Attach;

/// <summary>
/// 提供阴影深度（Elevation）的附加属性辅助类.
/// </summary>
public static class ElevationAssist
{
    /// <summary>
    /// 标识 Elevation 附加属性.
    /// </summary>
    public static readonly DependencyProperty ElevationProperty = DependencyProperty.RegisterAttached(
        "Elevation",
        typeof(Elevation),
        typeof(ElevationAssist),
        new FrameworkPropertyMetadata(Elevation.Dp0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// 设置指定元素的 Elevation 附加属性值.
    /// </summary>
    /// <param name="element">要设置属性的依赖对象.</param>
    /// <param name="value">要应用的阴影深度枚举值.</param>
    public static void SetElevation(DependencyObject element, Elevation value)
    {
        // 验证参数是否为空
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        // 设置属性值
        element.SetValue(ElevationProperty, value);
    }

    /// <summary>
    /// 获取指定元素的 Elevation 附加属性值.
    /// </summary>
    /// <param name="element">要获取属性的依赖对象.</param>
    /// <returns>当前的阴影深度枚举值.</returns>
    public static Elevation GetElevation(DependencyObject element)
    {        // 获取并转换属性值
        return (Elevation)element.GetValue(ElevationProperty);
    }

    /// <summary>
    /// 根据阴影深度获取对应的 DropShadowEffect 实例.
    /// </summary>
    /// <param name="elevation">阴影深度枚举值.</param>
    /// <returns>对应的阴影效果实例；如果没有则返回 null.</returns>
    public static DropShadowEffect? GetDropShadow(Elevation elevation)
    {
        // 调用内部信息类获取阴影效果
        return ElevationInfo.GetDropShadow(elevation);
    }
}

/// <summary>
/// 内部辅助类，负责加载资源字典并维护阴影效果的映射关系.
/// </summary>
internal static class ElevationInfo
{
    /// <summary>
    /// 存储阴影深度与阴影效果映射关系的字典.
    /// </summary>
    private static readonly Dictionary<Elevation, DropShadowEffect?> ShadowsDictionary;

    /// <summary>
    /// Initializes static members of the <see cref="ElevationInfo"/> class.
    /// </summary>
    static ElevationInfo()
    {
        // 初始化资源字典并加载指定路径的 XAML 文件
        ResourceDictionary shadowResources = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/BgControls.Wpf;component/Assets/Themes/Shadows.xaml", UriKind.Absolute)
        };

        // 构建阴影深度到资源 key 的映射字典
        ShadowsDictionary = new Dictionary<Elevation, DropShadowEffect?>
        {
            { Elevation.Dp0, null },
            { Elevation.Dp1, shadowResources["MaterialDesignElevationShadow1"] as DropShadowEffect },
            { Elevation.Dp2, shadowResources["MaterialDesignElevationShadow2"] as DropShadowEffect },
            { Elevation.Dp3, shadowResources["MaterialDesignElevationShadow3"] as DropShadowEffect },
            { Elevation.Dp4, shadowResources["MaterialDesignElevationShadow4"] as DropShadowEffect },
            { Elevation.Dp5, shadowResources["MaterialDesignElevationShadow5"] as DropShadowEffect },
            { Elevation.Dp6, shadowResources["MaterialDesignElevationShadow6"] as DropShadowEffect },
            { Elevation.Dp7, shadowResources["MaterialDesignElevationShadow7"] as DropShadowEffect },
            { Elevation.Dp8, shadowResources["MaterialDesignElevationShadow8"] as DropShadowEffect },
            { Elevation.Dp12, shadowResources["MaterialDesignElevationShadow12"] as DropShadowEffect },
            { Elevation.Dp16, shadowResources["MaterialDesignElevationShadow16"] as DropShadowEffect },
            { Elevation.Dp24, shadowResources["MaterialDesignElevationShadow24"] as DropShadowEffect }
        };
    }

    /// <summary>
    /// 从静态字典中检索指定的阴影效果.
    /// </summary>
    /// <param name="elevation">阴影深度枚举值.</param>
    /// <returns>对应的 DropShadowEffect 实例.</returns>
    public static DropShadowEffect? GetDropShadow(Elevation elevation)
    {
        // 如果字典中包含该 key 则返回，否则返回 null
        if (ElevationInfo.ShadowsDictionary.TryGetValue(elevation, out DropShadowEffect? effect))
        {
            return effect;
        }

        return null;
    }
}