namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 点击手势识别器工厂类，用于创建点击手势识别实例并定义手势转换规则.
/// </summary>
public class TapGestureRecognizerFactory : IGestureRecognizerFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TapGestureRecognizerFactory"/> class.
    /// </summary>
    public TapGestureRecognizerFactory()
    {
    }

    /// <summary>
    /// Gets 点击手势识别器的优先级.
    /// </summary>
    public int Priority => TouchManager.TapPriority;

    /// <summary>
    /// 为指定的 UI 元素创建点击手势识别器实例.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    /// <returns>点击手势识别器实例.</returns>
    public GestureRecognizerBase CreateGestureRecognizer(UIElement element)
    {
        // 验证传入的元素是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        return new TapGestureRecognizer(element);
    }

    /// <summary>
    /// 注册手势状态机中的点击手势转换规则.
    /// </summary>
    public void RegisterGestureTransitions()
    {
        // 注册点击待定状态允许转换到的目标手势集合.
        GestureManager.RegisterGestureTransitions(TouchManager.TapPendingGestureName, new HashSet<string>
        {
            TouchManager.TapGestureName,
            TouchManager.TapAndHoldGestureName,
            TouchManager.SwipeGestureName,
            TouchManager.DragGestureName,
        });

        // 注册点击并按住状态允许转换到的目标手势集合.
        GestureManager.RegisterGestureTransitions(TouchManager.TapAndHoldGestureName, new HashSet<string>
        {
            TouchManager.TapHoldAndReleaseGestureName,
            TouchManager.SwipeGestureName,
            TouchManager.DragGestureName,
        });
    }
}