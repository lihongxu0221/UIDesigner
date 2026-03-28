namespace BgControls.Windows.Attach;

/// <summary>
/// 水波纹效果辅助类，提供控制水波纹行为的附加属性.
/// </summary>
public static class RippleAssist
{
    /// <summary>
    /// 标识 ClipToBounds 附加属性.
    /// </summary>
    public static readonly DependencyProperty ClipToBoundsProperty =
        DependencyProperty.RegisterAttached("ClipToBounds", typeof(bool), typeof(RippleAssist), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 IsCentered 附加属性.
    /// </summary>
    public static readonly DependencyProperty IsCenteredProperty =
        DependencyProperty.RegisterAttached("IsCentered", typeof(bool), typeof(RippleAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 IsDisabled 附加属性.
    /// </summary>
    public static readonly DependencyProperty IsDisabledProperty =
        DependencyProperty.RegisterAttached("IsDisabled", typeof(bool), typeof(RippleAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 RippleSizeMultiplier 附加属性.
    /// </summary>
    public static readonly DependencyProperty RippleSizeMultiplierProperty =
        DependencyProperty.RegisterAttached("RippleSizeMultiplier", typeof(double), typeof(RippleAssist), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 Feedback 附加属性.
    /// </summary>
    public static readonly DependencyProperty FeedbackProperty =
        DependencyProperty.RegisterAttached("Feedback", typeof(Brush), typeof(RippleAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 标识 RippleOnTop 附加属性.
    /// </summary>
    public static readonly DependencyProperty RippleOnTopProperty =
        DependencyProperty.RegisterAttached("RippleOnTop", typeof(bool), typeof(RippleAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// 设置一个值，该值指示是否将水波纹限制在边界内.
    /// </summary>
    /// <param name="element">要设置属性的依赖对象.</param>
    /// <param name="value">指示值.</param>
    public static void SetClipToBounds(DependencyObject element, bool value)
    {
        element.SetValue(ClipToBoundsProperty, value);
    }

    /// <summary>
    /// 获取一个值，该值指示是否将水波纹限制在边界内.
    /// </summary>
    /// <param name="element">要读取属性的依赖对象.</param>
    /// <returns>返回是否剪裁到边界的值.</returns>
    public static bool GetClipToBounds(DependencyObject element)
    {
        return (bool)element.GetValue(ClipToBoundsProperty);
    }

    /// <summary>
    /// 设置一个值，该值指示水波纹是否从内容中心开始扩散；否则将从鼠标按下位置开始.
    /// </summary>
    /// <param name="element">要设置属性的依赖对象.</param>
    /// <param name="value">指示值.</param>
    public static void SetIsCentered(DependencyObject element, bool value)
    {
        element.SetValue(IsCenteredProperty, value);
    }

    /// <summary>
    /// 获取一个值，该值指示水波纹是否从内容中心开始扩散.
    /// </summary>
    /// <param name="element">要读取属性的依赖对象.</param>
    /// <returns>返回是否居中的指示值.</returns>
    public static bool GetIsCentered(DependencyObject element)
    {
        return (bool)element.GetValue(IsCenteredProperty);
    }

    /// <summary>
    /// 设置一个值，该值指示是否禁用波纹效果.
    /// </summary>
    /// <param name="element">要设置属性的依赖对象.</param>
    /// <param name="value">指示值.</param>
    public static void SetIsDisabled(DependencyObject element, bool value)
    {
        element.SetValue(IsDisabledProperty, value);
    }

    /// <summary>
    /// 获取一个值，该值指示是否禁用波纹效果.
    /// </summary>
    /// <param name="element">要读取属性的依赖对象.</param>
    /// <returns>返回是否禁用的指示值.</returns>
    public static bool GetIsDisabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsDisabledProperty);
    }

    /// <summary>
    /// 设置波纹大小的倍率.
    /// </summary>
    /// <param name="element">要设置属性的依赖对象.</param>
    /// <param name="value">大小倍率.</param>
    public static void SetRippleSizeMultiplier(DependencyObject element, double value)
    {
        element.SetValue(RippleSizeMultiplierProperty, value);
    }

    /// <summary>
    /// 获取波纹大小的倍率.
    /// </summary>
    /// <param name="element">要读取属性的依赖对象.</param>
    /// <returns>返回波纹大小乘数.</returns>
    public static double GetRippleSizeMultiplier(DependencyObject element)
    {
        return (double)element.GetValue(RippleSizeMultiplierProperty);
    }

    /// <summary>
    /// 设置波纹反馈颜色的画刷.
    /// </summary>
    /// <param name="element">要设置属性的依赖对象.</param>
    /// <param name="value">反馈画刷.</param>
    public static void SetFeedback(DependencyObject element, Brush value)
    {
        element.SetValue(FeedbackProperty, value);
    }

    /// <summary>
    /// 获取波纹反馈颜色的画刷.
    /// </summary>
    /// <param name="element">要读取属性的依赖对象.</param>
    /// <returns>返回波纹反馈画刷.</returns>
    public static Brush GetFeedback(DependencyObject element)
    {
        return (Brush)element.GetValue(FeedbackProperty);
    }

    /// <summary>
    /// 设置一个值，该值指示波纹是否应显示在内容上方.
    /// </summary>
    /// <param name="element">要设置属性的依赖对象.</param>
    /// <param name="value">指示值.</param>
    public static void SetRippleOnTop(DependencyObject element, bool value)
    {
        element.SetValue(RippleOnTopProperty, value);
    }

    /// <summary>
    /// 获取一个值，该值指示波纹是否应显示在内容上方.
    /// </summary>
    /// <param name="element">要读取属性的依赖对象.</param>
    /// <returns>返回波纹是否在顶层的指示值.</returns>
    public static bool GetRippleOnTop(DependencyObject element)
    {
        return (bool)element.GetValue(RippleOnTopProperty);
    }
}