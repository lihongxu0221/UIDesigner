namespace BgControls.Windows.DragDrop;

/// <summary>
/// 表示拖拽过程中的视觉呈现控件.
/// </summary>
public class DragVisual : ContentControl, IEffectsPresenter
{
    /// <summary>
    /// 标识 Effects 依赖属性.
    /// </summary>
    public static readonly DependencyProperty EffectsProperty = DependencyProperty.Register(
        "Effects",
        typeof(DragDropEffects),
        typeof(DragVisual),
        new PropertyMetadata(DragDropEffects.All, OnEffectsChanged));

    /// <summary>
    /// 标识 IsDraggingMultipleItems 依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsDraggingMultipleItemsProperty = DependencyProperty.Register(
        "IsDraggingMultipleItems",
        typeof(bool),
        typeof(DragVisual),
        null);

    /// <summary>
    /// Initializes static members of the <see cref="DragVisual"/> class.
    /// </summary>
    static DragVisual()
    {
        // 覆盖默认样式键以关联 Generic.xaml 中的样式.
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DragVisual),
            new FrameworkPropertyMetadata(typeof(DragVisual)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DragVisual"/> class.
    /// </summary>
    public DragVisual()
    {
        // 构造函数逻辑.
    }

    /// <summary>
    /// Gets or sets 拖放效果.
    /// </summary>
    public DragDropEffects Effects
    {
        get { return (DragDropEffects)this.GetValue(EffectsProperty); }
        set { this.SetValue(EffectsProperty, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否正在拖动多个项目.
    /// </summary>
    public bool IsDraggingMultipleItems
    {
        get { return (bool)this.GetValue(IsDraggingMultipleItemsProperty); }
        set { this.SetValue(IsDraggingMultipleItemsProperty, value); }
    }

    /// <summary>
    /// 应用模板时的处理逻辑.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 模板应用后更新初始视觉状态.
        this.UpdateVisualState();
    }

    /// <summary>
    /// 控件初始化逻辑.
    /// </summary>
    /// <param name="e">事件参数.</param>
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        // 此处可设置默认样式键.
    }

    /// <summary>
    /// 当 Effects 属性更改时的回调处理.
    /// </summary>
    /// <param name="dependencyObject">依赖对象实例.</param>
    /// <param name="eventArgs">事件参数.</param>
    private static void OnEffectsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
        if (dependencyObject is DragVisual dragVisualInstance)
        {
            // 通知实例更新视觉状态.
            dragVisualInstance.UpdateVisualState();
        }
    }

    /// <summary>
    /// 根据当前的拖放效果更新视觉状态.
    /// </summary>
    private void UpdateVisualState()
    {
        // 检查是否包含 Move 效果.
        if ((this.Effects & DragDropEffects.Move) == DragDropEffects.Move)
        {
            VisualStateManager.GoToState(this, DragDropEffects.Move.ToString(), false);
            return;
        }

        // 检查是否包含 Copy 效果.
        if ((this.Effects & DragDropEffects.Copy) == DragDropEffects.Copy)
        {
            VisualStateManager.GoToState(this, DragDropEffects.Copy.ToString(), false);
            return;
        }

        // 检查是否包含 Link 效果.
        if ((this.Effects & DragDropEffects.Link) == DragDropEffects.Link)
        {
            VisualStateManager.GoToState(this, DragDropEffects.Link.ToString(), false);
            return;
        }

        // 检查是否包含 Scroll 效果.
        if ((this.Effects & DragDropEffects.Scroll) == DragDropEffects.Scroll)
        {
            VisualStateManager.GoToState(this, DragDropEffects.Scroll.ToString(), false);
            return;
        }

        // 默认回退到 None 状态.
        // _ = this.Effects;
        // if (true)
        {
            VisualStateManager.GoToState(this, DragDropEffects.None.ToString(), false);
        }
    }
}