namespace BgControls.Windows.Attach;

/// <summary>
/// 表示阴影显示的边缘方向枚举，支持通过位运算进行组合.
/// </summary>
[Flags]
public enum ShadowEdges
{
    /// <summary>
    /// 不显示任何边缘的阴影.
    /// </summary>
    None = 0,

    /// <summary>
    /// 显示左侧边缘的阴影.
    /// </summary>
    Left = 1,

    /// <summary>
    /// 显示顶侧边缘的阴影.
    /// </summary>
    Top = 2,

    /// <summary>
    /// 显示右侧边缘的阴影.
    /// </summary>
    Right = 4,

    /// <summary>
    /// 显示底侧边缘的阴影.
    /// </summary>
    Bottom = 8,

    /// <summary>
    /// 显示所有四个边缘的阴影.
    /// </summary>
    All = 0xF
}

/// <summary>
/// 提供阴影效果（Shadow）的附加属性辅助类，支持动态加深动画和缓存模式配置.
/// </summary>
public static class ShadowAssist
{
    /// <summary>
    /// 内部使用的私有类，用于存储阴影的原始状态信息.
    /// </summary>
    private class ShadowLocalInfo
    {
        /// <summary>
        /// 存储阴影的标称不透明度.
        /// </summary>
        private readonly double standardOpacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowLocalInfo"/> class.
        /// </summary>
        /// <param name="standardOpacity">原始不透明度值.</param>
        public ShadowLocalInfo(double standardOpacity)
        {
            this.standardOpacity = standardOpacity;
        }

        /// <summary>
        /// Gets 原始不透明度值.
        /// </summary>
        public double StandardOpacity => this.standardOpacity;
    }

    /// <summary>
    /// 标识 LocalInfo 只读附加属性的键，用于内部存储阴影状态.
    /// </summary>
    private static readonly DependencyPropertyKey LocalInfoPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly("LocalInfo", typeof(ShadowLocalInfo), typeof(ShadowAssist), new PropertyMetadata((object)null));

    /// <summary>
    /// 标识 Darken 附加属性，用于控制是否触发阴影加深动画.
    /// </summary>
    public static readonly DependencyProperty DarkenProperty =
        DependencyProperty.RegisterAttached("Darken", typeof(bool), typeof(ShadowAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, DarkenPropertyChangedCallback));

    /// <summary>
    /// 标识 CacheMode 附加属性，用于配置元素的位图缓存模式以优化性能.
    /// </summary>
    public static readonly DependencyProperty CacheModeProperty =
        DependencyProperty.RegisterAttached("CacheMode", typeof(CacheMode), typeof(ShadowAssist), new FrameworkPropertyMetadata(new BitmapCache
        {
            EnableClearType = true,
            SnapsToDevicePixels = true
        }, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 ShadowEdges 附加属性，用于定义阴影显示的边缘方向.
    /// </summary>
    public static readonly DependencyProperty ShadowEdgesProperty =
        DependencyProperty.RegisterAttached("ShadowEdges", typeof(ShadowEdges), typeof(ShadowAssist), new PropertyMetadata(ShadowEdges.All));

    /// <summary>
    /// 标识 ShadowAnimationDuration 附加属性，定义阴影动画的持续时间.
    /// </summary>
    public static readonly DependencyProperty ShadowAnimationDurationProperty =
        DependencyProperty.RegisterAttached("ShadowAnimationDuration", typeof(TimeSpan), typeof(ShadowAssist), new FrameworkPropertyMetadata(new TimeSpan(0, 0, 0, 0, 180), FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 设置内部使用的 LocalInfo 附加属性值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">阴影状态信息对象.</param>
    private static void SetLocalInfo(DependencyObject element, ShadowLocalInfo? value)
    {
        element.SetValue(LocalInfoPropertyKey, value);
    }

    /// <summary>
    /// 获取内部使用的 LocalInfo 附加属性值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>返回存储的阴影状态信息.</returns>
    private static ShadowLocalInfo? GetLocalInfo(DependencyObject element)
    {
        return (ShadowLocalInfo)element.GetValue(LocalInfoPropertyKey.DependencyProperty);
    }

    /// <summary>
    /// 处理 Darken 属性更改的回调逻辑，执行阴影不透明度动画.
    /// </summary>
    /// <param name="element">发生更改的依赖对象.</param>
    /// <param name="args">事件参数.</param>
    private static void DarkenPropertyChangedCallback(DependencyObject element, DependencyPropertyChangedEventArgs args)
    {
        // 尝试获取元素的 DropShadowEffect 效果
        if ((element as UIElement)?.Effect is not DropShadowEffect shadowEffect)
        {
            return;
        }

        // 如果属性值变为 true，执行加深动画
        if ((bool)args.NewValue)
        {
            // 停止当前动画并记录原始不透明度
            shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, null);
            ShadowAssist.SetLocalInfo(element, new ShadowLocalInfo(shadowEffect.Opacity));

            // 获取定义的动画时长
            TimeSpan duration = ShadowAssist.GetShadowAnimationDuration(element);

            // 构建加深到 1.0 的动画
            DoubleAnimation darkenAnimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new CubicEase(),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.2
            };
            shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, darkenAnimation);
            return;
        }

        // 如果属性值变为 false，还原到原始不透明度
        ShadowLocalInfo? info = ShadowAssist.GetLocalInfo(element);
        if (info != null)
        {
            TimeSpan duration = ShadowAssist.GetShadowAnimationDuration(element);

            // 构建还原动画
            DoubleAnimation restoreAnimation = new DoubleAnimation
            {
                To = info.StandardOpacity,
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new CubicEase(),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.2
            };
            shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, restoreAnimation);
        }
    }

    /// <summary>
    /// 设置 Darken 附加属性的值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">一个值，指示是否加深阴影.</param>
    public static void SetDarken(DependencyObject element, bool value)
    {
        element.SetValue(DarkenProperty, value);
    }

    /// <summary>
    /// 获取 Darken 附加属性的值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>如果开启了加深模式则返回 true.</returns>
    public static bool GetDarken(DependencyObject element)
    {
        return (bool)element.GetValue(DarkenProperty);
    }

    /// <summary>
    /// 设置 CacheMode 附加属性的值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">缓存模式配置.</param>
    public static void SetCacheMode(DependencyObject element, CacheMode value)
    {
        element.SetValue(CacheModeProperty, value);
    }

    /// <summary>
    /// 获取 CacheMode 附加属性的值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>返回当前配置的缓存模式.</returns>
    public static CacheMode GetCacheMode(DependencyObject element)
    {
        return (CacheMode)element.GetValue(CacheModeProperty);
    }

    /// <summary>
    /// 设置 ShadowEdges 附加属性的值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">阴影显示边缘设置.</param>
    public static void SetShadowEdges(DependencyObject element, ShadowEdges value)
    {
        element.SetValue(ShadowEdgesProperty, value);
    }

    /// <summary>
    /// 获取 ShadowEdges 附加属性的值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>返回当前的阴影边缘配置.</returns>
    public static ShadowEdges GetShadowEdges(DependencyObject element)
    {
        return (ShadowEdges)element.GetValue(ShadowEdgesProperty);
    }

    /// <summary>
    /// 获取 ShadowAnimationDuration 附加属性的值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>动画持续时间.</returns>
    public static TimeSpan GetShadowAnimationDuration(DependencyObject element)
    {
        return (TimeSpan)element.GetValue(ShadowAnimationDurationProperty);
    }

    /// <summary>
    /// 设置 ShadowAnimationDuration 附加属性的值.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">动画持续时间值.</param>
    public static void SetShadowAnimationDuration(DependencyObject element, TimeSpan value)
    {
        element.SetValue(ShadowAnimationDurationProperty, value);
    }
}