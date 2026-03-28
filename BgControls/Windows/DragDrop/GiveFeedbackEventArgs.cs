namespace BgControls.Windows.DragDrop;

/// <summary>
/// 表示处理拖放反馈（GiveFeedback）路由事件的方法.
/// </summary>
/// <param name="sender">绑定事件处理程序的对象.</param>
/// <param name="e">包含反馈事件数据的参数.</param>
public delegate void GiveFeedbackEventHandler(object sender, GiveFeedbackEventArgs e);

/// <summary>
/// 为提供拖放反馈路由事件提供数据的类.
/// 该类允许拖放源根据当前的拖放效果动态修改光标外观.
/// </summary>
public sealed class GiveFeedbackEventArgs : RoutedEventArgs
{
    private DragDropEffects effects;
    private Cursor? cursor;

    /// <summary>
    /// Initializes a new instance of the <see cref="GiveFeedbackEventArgs"/> class.
    /// </summary>
    /// <param name="e">系统原始的反馈事件参数.</param>
    internal GiveFeedbackEventArgs(System.Windows.GiveFeedbackEventArgs e)
        : base(e.RoutedEvent, e.OriginalSource)
    {
        // 将系统路由事件映射到自定义的拖放管理器事件
        if (e.RoutedEvent == System.Windows.DragDrop.PreviewGiveFeedbackEvent)
        {
            this.RoutedEvent = DragDropManager.PreviewGiveFeedbackEvent;
        }
        else if (e.RoutedEvent == System.Windows.DragDrop.GiveFeedbackEvent)
        {
            this.RoutedEvent = DragDropManager.GiveFeedbackEvent;
        }

        this.effects = e.Effects;
        this.UseDefaultCursors = e.UseDefaultCursors;
        this.Handled = e.Handled;
        this.Source = e.Source;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GiveFeedbackEventArgs"/> class.
    /// </summary>
    /// <param name="effects">当前的拖放操作效果.</param>
    /// <param name="useDefaultCursors">指示是否使用默认光标的初始值.</param>
    internal GiveFeedbackEventArgs(DragDropEffects effects, bool useDefaultCursors)
    {
        this.effects = effects;
        this.UseDefaultCursors = useDefaultCursors;
    }

    /// <summary>
    /// Gets 当前的拖放操作效果.
    /// </summary>
    public DragDropEffects Effects => effects;

    /// <summary>
    /// Gets 已设置的自定义光标.
    /// </summary>
    internal Cursor? Cursor => cursor;

    /// <summary>
    /// Gets or sets a value indicating whether 是否应使用与拖放效果关联的默认系统光标.
    /// </summary>
    public bool UseDefaultCursors { get; set; }

    /// <summary>
    /// 设置在拖放操作期间显示的自定义光标.
    /// </summary>
    /// <param name="cursor">要显示的自定义光标实例.</param>
    public void SetCursor(Cursor cursor)
    {
        this.cursor = cursor;
    }

    /// <summary>
    /// 以特定于类型的方式调用事件处理程序.
    /// 该重写通过消除类型转换开销来提升事件分发效率.
    /// </summary>
    /// <param name="genericHandler">要调用的泛型处理程序.</param>
    /// <param name="genericTarget">在其上调用处理程序的源对象.</param>
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        (genericHandler as BgControls.Windows.DragDrop.GiveFeedbackEventHandler)?.Invoke(genericTarget, this);
    }
}