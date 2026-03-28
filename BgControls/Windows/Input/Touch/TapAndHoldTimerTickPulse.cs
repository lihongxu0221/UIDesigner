namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 点击并按住计时器触发的脉冲信息类.
/// </summary>
internal class TapAndHoldTimerTickPulse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TapAndHoldTimerTickPulse"/> class.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    public TapAndHoldTimerTickPulse(int touchId)
    {
        // 设置当前脉冲关联的触控 ID.
        this.TouchId = touchId;
    }

    /// <summary>
    /// Gets 触控 ID.
    /// </summary>
    public int TouchId { get; }

    /// <summary>
    /// Gets or sets a value indicating whether 点击并按住操作是否已被处理.
    /// </summary>
    public bool TapAndHoldHandled { get; set; }
}