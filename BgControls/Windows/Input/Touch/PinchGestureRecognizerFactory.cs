namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 捏合手势识别器工厂类，用于创建和管理捏合手势识别实例.
/// </summary>
public class PinchGestureRecognizerFactory : IGestureRecognizerFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PinchGestureRecognizerFactory"/> class.
    /// </summary>
    public PinchGestureRecognizerFactory()
    {
    }

    /// <summary>
    /// Gets 捏合手势识别器的优先级.
    /// </summary>
    public int Priority => TouchManager.PinchPriority;

    /// <summary>
    /// 为指定的 UI 元素创建一个新的捏合手势识别器.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    /// <returns>捏合手势识别器实例.</returns>
    public GestureRecognizerBase CreateGestureRecognizer(UIElement element)
    {
        // 验证传入的 UI 元素是否为空.
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        // 实例化并返回特定于元素的捏合手势识别器.
        return new PinchGestureRecognizer(element);
    }

    /// <summary>
    /// 注册手势转换的相关规则.
    /// </summary>
    public void RegisterGestureTransitions()
    {
        // 按照接口要求实现，当前逻辑无需执行特定转换注册.
    }
}