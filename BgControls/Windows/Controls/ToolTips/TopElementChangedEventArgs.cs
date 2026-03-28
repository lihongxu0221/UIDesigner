namespace BgControls.Windows.Controls.ToolTips;

/// <summary>
/// 为顶层元素（Top Element）更改事件提供数据的类.
/// </summary>
internal class TopElementChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TopElementChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldTopElement">更改前的旧顶层元素.</param>
    /// <param name="newTopElement">更改后的新顶层元素.</param>
    public TopElementChangedEventArgs(FrameworkElement? oldTopElement, FrameworkElement newTopElement)
    {
        ArgumentNullException.ThrowIfNull(newTopElement, nameof(newTopElement));
        this.OldTopElement = oldTopElement;
        this.NewTopElement = newTopElement;
    }

    /// <summary>
    /// Gets 更改前的旧顶层元素.
    /// </summary>
    public FrameworkElement? OldTopElement { get; }

    /// <summary>
    /// Gets 更改后的新顶层元素.
    /// </summary>
    public FrameworkElement NewTopElement { get; }
}