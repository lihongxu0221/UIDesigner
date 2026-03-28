namespace BgControls.Windows.Controls.DragDrop;

/// <summary>
/// 表示拖拽执行模式的枚举.
/// </summary>
public enum DragExecutionMode
{
    /// <summary>
    /// 默认执行模式.
    /// </summary>
    Default,

    /// <summary>
    /// 兼容旧版本的执行模式.
    /// </summary>
    Legacy,

    /// <summary>
    /// 禁用拖拽执行逻辑.
    /// </summary>
    Disabled,
}