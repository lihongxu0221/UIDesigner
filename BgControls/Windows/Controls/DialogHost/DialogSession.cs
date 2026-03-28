namespace BgControls.Windows.Controls;

/// <summary>
/// 对话框会话类，用于管理对话框的生命周期及内容更新.
/// </summary>
public class DialogSession
{
    // 对话框宿主字段，不以下划线开头.
    private readonly DialogHost owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogSession"/> class.
    /// </summary>
    /// <param name="owner">对话框宿主对象.</param>
    internal DialogSession(DialogHost owner)
    {
        ArgumentNullException.ThrowIfNull(owner, nameof(owner));
        this.owner = owner;
    }

    /// <summary>
    /// Gets a value indicating whether 对话框会话是否已结束.
    /// </summary>
    /// <remarks>
    /// 客户端代码不能直接设置此属性，它由内部管理。要结束对话框会话，请使用 <see cref="DialogSession.Close()" />.
    /// </remarks>
    public bool IsEnded { get; internal set; }

    /// <summary>
    /// Gets or sets 对话框关闭时传递的参数.
    /// </summary>
    /// <remarks>
    /// 该参数传递给 <see cref="DialogHost.CloseDialogCommand" /> 并由 <see cref="DialogHost.ShowAsync(object)" /> 返回.
    /// </remarks>
    internal object? CloseParameter { get; set; }

    /// <summary>
    /// Gets 当前显示的对话框内容.
    /// </summary>
    /// <remarks>
    /// 这可能是一个视图模型（View Model）或一个 UI 元素.
    /// </remarks>
    public object? Content
    {
        get
        {
            // 通过宿主对象获取对话框内容.
            return this.owner.DialogContent;
        }
    }

    /// <summary>
    /// 更新对话框中的当前内容.
    /// </summary>
    /// <param name="content">新的上下文内容.</param>
    public void UpdateContent(object? content)
    {
        // 断言宿主内容是可定位的.
        this.owner.AssertTargetableContent();

        // 设置宿主的新内容.
        this.owner.DialogContent = content;

        // 在后台优先级异步调用宿主的焦点设置方法.
        this.owner.Dispatcher.BeginInvoke(DispatcherPriority.Background, this.owner.FocusPopup);
    }

    /// <summary>
    /// 关闭对话框.
    /// </summary>
    /// <exception cref="InvalidOperationException">如果对话框会话已结束，或者关闭操作正在进行中，则抛出此异常.</exception>
    public void Close()
    {
        // 检查会话是否已经处于结束状态.
        if (this.IsEnded)
        {
            throw new InvalidOperationException("Dialog session has ended.");
        }

        // 调用宿主的内部关闭逻辑，不传递参数.
        this.owner.InternalClose(null);
    }

    /// <summary>
    /// 关闭对话框并传递结果参数.
    /// </summary>
    /// <param name="parameter">结果参数，将返回给关闭事件或 Show 方法.</param>
    /// <exception cref="InvalidOperationException">如果对话框会话已结束，或者关闭操作正在进行中，则抛出此异常.</exception>
    public void Close(object? parameter)
    {
        // 检查会话是否已经处于结束状态.
        if (this.IsEnded)
        {
            throw new InvalidOperationException("Dialog session has ended.");
        }

        // 调用宿主的内部关闭逻辑，传递指定的参数.
        this.owner.InternalClose(parameter);
    }
}