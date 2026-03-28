namespace BgControls.Windows.Attach;

/// <summary>
/// 扩展器（Expander）控件的辅助类，提供标题边距、字体大小及背景色等附加属性.
/// </summary>
public static class ExpanderAssist
{
    // 默认水平标题边距字段.
    private static readonly Thickness DefaultHorizontalHeaderPadding = new Thickness(24.0, 12.0, 24.0, 12.0);

    // 默认垂直标题边距字段.
    private static readonly Thickness DefaultVerticalHeaderPadding = new Thickness(12.0, 24.0, 12.0, 24.0);

    /// <summary>
    /// 标识 HorizontalHeaderPadding 附加属性.
    /// </summary>
    public static readonly DependencyProperty HorizontalHeaderPaddingProperty =
        DependencyProperty.RegisterAttached("HorizontalHeaderPadding", typeof(Thickness), typeof(ExpanderAssist), new FrameworkPropertyMetadata(DefaultHorizontalHeaderPadding, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 VerticalHeaderPadding 附加属性.
    /// </summary>
    public static readonly DependencyProperty VerticalHeaderPaddingProperty =
        DependencyProperty.RegisterAttached("VerticalHeaderPadding", typeof(Thickness), typeof(ExpanderAssist), new FrameworkPropertyMetadata(DefaultVerticalHeaderPadding, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 HeaderFontSize 附加属性.
    /// </summary>
    public static readonly DependencyProperty HeaderFontSizeProperty =
        DependencyProperty.RegisterAttached("HeaderFontSize", typeof(double), typeof(ExpanderAssist), new FrameworkPropertyMetadata(15.0));

    /// <summary>
    /// 标识 HeaderBackground 附加属性.
    /// </summary>
    public static readonly DependencyProperty HeaderBackgroundProperty =
        DependencyProperty.RegisterAttached("HeaderBackground", typeof(Brush), typeof(ExpanderAssist));

    /// <summary>
    /// 获取水平布局时的标题边距.
    /// </summary>
    /// <param name="element">扩展器元素.</param>
    /// <returns>返回标题边距值.</returns>
    public static Thickness GetHorizontalHeaderPadding(Expander element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (Thickness)element.GetValue(HorizontalHeaderPaddingProperty);
    }

    /// <summary>
    /// 设置水平布局时的标题边距.
    /// </summary>
    /// <param name="element">扩展器元素.</param>
    /// <param name="value">边距值.</param>
    public static void SetHorizontalHeaderPadding(Expander element, Thickness value)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(HorizontalHeaderPaddingProperty, value);
    }

    /// <summary>
    /// 获取垂直布局时的标题边距.
    /// </summary>
    /// <param name="element">扩展器元素.</param>
    /// <returns>返回标题边距值.</returns>
    public static Thickness GetVerticalHeaderPadding(Expander element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (Thickness)element.GetValue(VerticalHeaderPaddingProperty);
    }

    /// <summary>
    /// 设置垂直布局时的标题边距.
    /// </summary>
    /// <param name="element">扩展器元素.</param>
    /// <param name="value">边距值.</param>
    public static void SetVerticalHeaderPadding(Expander element, Thickness value)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(VerticalHeaderPaddingProperty, value);
    }

    /// <summary>
    /// 获取标题的字体大小.
    /// </summary>
    /// <param name="element">扩展器元素.</param>
    /// <returns>返回字体大小值.</returns>
    public static double GetHeaderFontSize(Expander element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (double)element.GetValue(HeaderFontSizeProperty);
    }

    /// <summary>
    /// 设置标题的字体大小.
    /// </summary>
    /// <param name="element">扩展器元素.</param>
    /// <param name="value">字体大小值.</param>
    public static void SetHeaderFontSize(Expander element, double value)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(HeaderFontSizeProperty, value);
    }

    /// <summary>
    /// 获取标题的背景色画刷.
    /// </summary>
    /// <param name="element">扩展器元素.</param>
    /// <returns>返回背景画刷.</returns>
    public static Brush? GetHeaderBackground(Expander element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        return (Brush?)element.GetValue(HeaderBackgroundProperty);
    }

    /// <summary>
    /// 设置标题的背景色画刷.
    /// </summary>
    /// <param name="element">扩展器元素.</param>
    /// <param name="value">背景画刷值.</param>
    public static void SetHeaderBackground(Expander element, Brush? value)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        element.SetValue(HeaderBackgroundProperty, value);
    }
}