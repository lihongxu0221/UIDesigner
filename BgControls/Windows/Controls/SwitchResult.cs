namespace BgControls.Windows.Controls;

/// <summary>
/// 表示项或选项卡切换结果的内部类.
/// </summary>
internal class SwitchResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchResult"/> class.
    /// </summary>
    /// <param name="isSwiched">指示切换是否成功的初始状态值.</param>
    internal SwitchResult(bool isSwiched)
    {
        this.IsSwiched = isSwiched;
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否已成功执行切换操作.
    /// </summary>
    internal bool IsSwiched { get; set; }

    /// <summary>
    /// Gets or sets 执行切换操作前的原始索引位置.
    /// </summary>
    internal int OldIndex { get; set; }

    /// <summary>
    /// Gets or sets 执行切换操作后的新索引位置.
    /// </summary>
    internal int NewIndex { get; set; }
}