namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 请求停止手势时提供的事件参数类.
/// </summary>
internal class CeaseGesturesRequestedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CeaseGesturesRequestedEventArgs"/> class.
    /// </summary>
    /// <param name="root">请求停止手势关联的根视觉元素.</param>
    public CeaseGesturesRequestedEventArgs(FrameworkElement root)
    {
        // 将传入的根元素引用赋值给只读属性.
        this.Root = root;
    }

    /// <summary>
    /// Gets 请求停止手势关联的根视觉元素.
    /// </summary>
    public FrameworkElement Root { get; }
}