namespace BgControls.Windows.Attach;

/// <summary>
/// 验证辅助类，提供用于控制验证 UI 显示行为的附加属性.
/// </summary>
public static class ValidationAssist
{
    /// <summary>
    /// 标识 OnlyShowOnFocus 附加属性.
    /// </summary>
    public static readonly DependencyProperty OnlyShowOnFocusProperty =
        DependencyProperty.RegisterAttached("OnlyShowOnFocus", typeof(bool), typeof(ValidationAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 UsePopup 附加属性.
    /// </summary>
    public static readonly DependencyProperty UsePopupProperty =
        DependencyProperty.RegisterAttached("UsePopup", typeof(bool), typeof(ValidationAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 PopupPlacement 附加属性.
    /// </summary>
    public static readonly DependencyProperty PopupPlacementProperty =
        DependencyProperty.RegisterAttached("PopupPlacement", typeof(PlacementMode), typeof(ValidationAssist), new FrameworkPropertyMetadata(PlacementMode.Bottom, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 Suppress 附加属性。仅限框架内部使用.
    /// </summary>
    public static readonly DependencyProperty SuppressProperty =
        DependencyProperty.RegisterAttached("Suppress", typeof(bool), typeof(ValidationAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 Background 附加属性.
    /// </summary>
    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.RegisterAttached("Background", typeof(Brush), typeof(ValidationAssist), new PropertyMetadata(null));

    /// <summary>
    /// 标识 FontSize 附加属性.
    /// </summary>
    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.RegisterAttached("FontSize", typeof(double), typeof(ValidationAssist), new PropertyMetadata(10.0));

    /// <summary>
    /// 标识 HasError 附加属性.
    /// </summary>
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.RegisterAttached("HasError", typeof(bool), typeof(ValidationAssist), new PropertyMetadata(false));

    /// <summary>
    /// 标识 HorizontalAlignment 附加属性.
    /// </summary>
    public static readonly DependencyProperty HorizontalAlignmentProperty =
        DependencyProperty.RegisterAttached("HorizontalAlignment", typeof(HorizontalAlignment), typeof(ValidationAssist), new PropertyMetadata(HorizontalAlignment.Left));

    /// <summary>
    /// 获取一个值，该值指示是否仅在获得焦点时显示验证信息.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>如果是，则返回 true；否则返回 false.</returns>
    public static bool GetOnlyShowOnFocus(DependencyObject element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (bool)element.GetValue(OnlyShowOnFocusProperty);
    }

    /// <summary>
    /// 设置一个值，该值指示是否仅在获得焦点时显示验证信息.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">指示值.</param>
    public static void SetOnlyShowOnFocus(DependencyObject element, bool value)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(OnlyShowOnFocusProperty, value);
    }

    /// <summary>
    /// 获取一个值，该值指示是否使用弹出框显示验证信息.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>如果是，则返回 true；否则返回 false.</returns>
    public static bool GetUsePopup(DependencyObject element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (bool)element.GetValue(UsePopupProperty);
    }

    /// <summary>
    /// 设置一个值，该值指示是否使用弹出框显示验证信息.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">指示值.</param>
    public static void SetUsePopup(DependencyObject element, bool value)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(UsePopupProperty, value);
    }

    /// <summary>
    /// 获取验证弹出框的放置位置.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>返回位置枚举值.</returns>
    public static PlacementMode GetPopupPlacement(DependencyObject element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (PlacementMode)element.GetValue(PopupPlacementProperty);
    }

    /// <summary>
    /// 设置验证弹出框的放置位置.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">位置枚举值.</param>
    public static void SetPopupPlacement(DependencyObject element, PlacementMode value)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(PopupPlacementProperty, value);
    }

    /// <summary>
    /// 设置一个值，该值指示是否抑制验证显示。仅限框架内部使用.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">指示值.</param>
    public static void SetSuppress(DependencyObject element, bool value)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(SuppressProperty, value);
    }

    /// <summary>
    /// 获取一个值，该值指示是否抑制验证显示。仅限框架内部使用.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>如果是，则返回 true；否则返回 false.</returns>
    public static bool GetSuppress(DependencyObject element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (bool)element.GetValue(SuppressProperty);
    }

    /// <summary>
    /// 设置验证信息的背景画刷.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">画刷对象.</param>
    public static void SetBackground(DependencyObject element, Brush value)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// 获取验证信息的背景画刷.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>返回背景画刷.</returns>
    public static Brush GetBackground(DependencyObject element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (Brush)element.GetValue(BackgroundProperty);
    }

    /// <summary>
    /// 设置验证信息的字体大小.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">字体大小值.</param>
    public static void SetFontSize(DependencyObject element, double value)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// 获取验证信息的字体大小.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>返回字体大小值.</returns>
    public static double GetFontSize(DependencyObject element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (double)element.GetValue(FontSizeProperty);
    }

    /// <summary>
    /// 设置一个值，该值指示当前元素是否存在验证错误.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">指示值.</param>
    public static void SetHasError(DependencyObject element, bool value)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(HasErrorProperty, value);
    }

    /// <summary>
    /// 获取一个值，该值指示当前元素是否存在验证错误.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>如果是，则返回 true；否则返回 false.</returns>
    public static bool GetHasError(DependencyObject element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (bool)element.GetValue(HasErrorProperty);
    }

    /// <summary>
    /// 设置验证信息的水平对齐方式.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="value">水平对齐枚举值.</param>
    public static void SetHorizontalAlignment(DependencyObject element, HorizontalAlignment value)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(HorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// 获取验证信息的水平对齐方式.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <returns>返回水平对齐方式.</returns>
    public static HorizontalAlignment GetHorizontalAlignment(DependencyObject element)
    {
        // 验证参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (HorizontalAlignment)element.GetValue(HorizontalAlignmentProperty);
    }
}