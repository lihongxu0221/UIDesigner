namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 手势停用令牌类，用于追踪并主动结束特定的手势状态.
/// </summary>
public class GestureDeactivationToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GestureDeactivationToken"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    /// <param name="gestureName">手势名称.</param>
    internal GestureDeactivationToken(UIElement element, string gestureName)
    {
        // 验证输入参数是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        ArgumentNullException.ThrowIfNull(gestureName, nameof(gestureName));

        // 初始化只读属性.
        this.Element = element;
        this.GestureName = gestureName;
    }

    /// <summary>
    /// Gets 关联的 UI 元素.
    /// </summary>
    public UIElement Element { get; }

    /// <summary>
    /// Gets 手势名称.
    /// </summary>
    public string GestureName { get; }

    /// <summary>
    /// 主动停用此令牌关联的手势状态.
    /// </summary>
    public void DeactivateGesture()
    {
        // 调用手势管理器主动释放当前手势.
        GestureManager.DeactivateGestureWillingly(this);
    }
}