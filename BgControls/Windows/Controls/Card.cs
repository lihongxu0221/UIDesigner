namespace BgControls.Windows.Controls;

/// <summary>
/// 表示一个卡片控件，支持自定义圆角以及内容剪裁功能.
/// </summary>
[TemplatePart(Name = ClipBorderPartName, Type = typeof(Border))]
public class Card : ContentControl
{
    private const double DefaultUniformCornerRadiusValue = 4.0;
    private const string ClipBorderPartName = "PART_ClipBorder";

    /// <summary>
    /// 标识 UniformCornerRadius 依赖属性.
    /// </summary>
    public static readonly DependencyProperty UniformCornerRadiusProperty =
        DependencyProperty.Register("UniformCornerRadius", typeof(double), typeof(Card), new FrameworkPropertyMetadata(DefaultUniformCornerRadiusValue, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// 标识 ContentClip 只读依赖属性的键.
    /// </summary>
    private static readonly DependencyPropertyKey ContentClipPropertyKey =
        DependencyProperty.RegisterReadOnly("ContentClip", typeof(Geometry), typeof(Card), new PropertyMetadata(null));

    /// <summary>
    /// 标识 ClipContent 依赖属性.
    /// </summary>
    public static readonly DependencyProperty ClipContentProperty =
        DependencyProperty.Register("ClipContent", typeof(bool), typeof(Card), new PropertyMetadata(false));

    /// <summary>
    /// 标识 ContentClip 依赖属性.
    /// </summary>
    public static readonly DependencyProperty ContentClipProperty = ContentClipPropertyKey.DependencyProperty;

    static Card()
    {
        // 覆盖默认样式键，以确保控件使用自定义的样式模板
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
    }

    private Border? clipBorder;

    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> class.
    /// </summary>
    public Card()
    {
    }

    /// <summary>
    /// Gets or sets 统一的圆角半径.
    /// </summary>
    public double UniformCornerRadius
    {
        get { return (double)this.GetValue(UniformCornerRadiusProperty); }
        set { this.SetValue(UniformCornerRadiusProperty, value); }
    }

    /// <summary>
    /// Gets 内容剪裁几何图形.
    /// </summary>
    public Geometry? ContentClip
    {
        get { return (Geometry)this.GetValue(ContentClipProperty); }
        private set { this.SetValue(ContentClipPropertyKey, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否对内容进行剪裁.
    /// </summary>
    public bool ClipContent
    {
        get { return (bool)this.GetValue(ClipContentProperty); }
        set { this.SetValue(ClipContentProperty, value); }
    }

    /// <summary>
    /// 在应用控件模板时调用，用于获取模板中的特定部件.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 获取用于计算剪裁区域的内部 Border 部件
        this.clipBorder = this.Template.FindName(ClipBorderPartName, this) as Border;
    }

    /// <summary>
    /// 当控件的渲染大小发生变化时调用，用于更新剪裁几何图形.
    /// </summary>
    /// <param name="sizeInfo">包含尺寸改变信息的参数.</param>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        // 如果获取到了模板中的剪裁边框，则根据其当前大小计算剪裁矩形
        if (this.clipBorder != null)
        {
            double actualWidth = Math.Max(0.0, this.clipBorder.ActualWidth);
            double actualHeight = Math.Max(0.0, this.clipBorder.ActualHeight);

            // 创建表示内容边界的矩形
            Rect contentBounds = new Rect(new Point(0.0, 0.0), new Point(actualWidth, actualHeight));

            // 更新 ContentClip 属性，生成带圆角的矩形几何图形
            this.ContentClip = new RectangleGeometry(contentBounds, this.UniformCornerRadius, this.UniformCornerRadius);
        }
    }
}