namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 手势识别器基类，定义了手势识别的基本接口和属性.
/// </summary>
public abstract class GestureRecognizerBase
{
    private bool hasGestureHandlers;
    private UIElement element;
    private TouchHelper? touchHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GestureRecognizerBase"/> class.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    protected GestureRecognizerBase(UIElement element)
    {
        // 设置关联的 UI 元素.
        this.element = element;
    }

    /// <summary>
    /// Gets 关联的 UI 元素.
    /// </summary>
    public UIElement Element => this.element;

    /// <summary>
    /// Gets or sets 触控辅助对象.
    /// </summary>
    internal TouchHelper? TouchHelper
    {
        get { return this.touchHelper; }
        set { this.touchHelper = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否具有手势处理程序.
    /// </summary>
    public bool HasGestureHandlers
    {
        get
        {
            return this.hasGestureHandlers;
        }

        set
        {
            if (this.hasGestureHandlers != value)
            {
                this.hasGestureHandlers = value;

                // 当状态改变时触发通知逻辑.
                this.OnHasGestureHandlersChanged();
            }
        }
    }

    /// <summary>
    /// 触控进入时的抽象处理方法.
    /// </summary>
    /// <param name="args">事件参数.</param>
    public abstract void OnTouchEnter(GestureRecognizerEventArgs args);

    /// <summary>
    /// 触控按下时的抽象处理方法.
    /// </summary>
    /// <param name="args">事件参数.</param>
    public abstract void OnTouchDown(GestureRecognizerEventArgs args);

    /// <summary>
    /// 触控移动时的抽象处理方法.
    /// </summary>
    /// <param name="args">事件参数.</param>
    public abstract void OnTouchMove(GestureRecognizerEventArgs args);

    /// <summary>
    /// 触控抬起时的抽象处理方法.
    /// </summary>
    /// <param name="args">事件参数.</param>
    public abstract void OnTouchUp(GestureRecognizerEventArgs args);

    /// <summary>
    /// 触控离开时的抽象处理方法.
    /// </summary>
    /// <param name="args">事件参数.</param>
    public abstract void OnTouchLeave(GestureRecognizerEventArgs args);

    /// <summary>
    /// 当请求停止手势识别时触发的虚拟方法.
    /// </summary>
    public virtual void OnCeaseGesturesRequested()
    {
    }

    /// <summary>
    /// 验证 UI 元素是否匹配.
    /// </summary>
    /// <param name="element">预期的 UI 元素.</param>
    /// <param name="sender">实际触发事件的 UI 元素.</param>
    /// <exception cref="InvalidOperationException">当元素不匹配时抛出.</exception>
    internal static void VerifyElementsMismatch(UIElement element, UIElement sender)
    {
        if (element != sender)
        {
            // 如果当前识别器关联的元素与发送者不一致，则抛出异常.
            throw new InvalidOperationException("Arguments mismatch. The recognizer is not responsible for the sender element, but a different UI element.");
        }
    }

    /// <summary>
    /// 当 HasGestureHandlers 属性值更改时调用的内部处理逻辑.
    /// </summary>
    internal void OnHasGestureHandlersChanged()
    {
        if (this.touchHelper != null)
        {
            // 如果存在触控辅助对象，则尝试建立触控订阅.
            this.touchHelper.GetOrCreateGestureHelper()?.EstablishTouchSubscription();
        }
    }
}