namespace BgControls.Windows.Controls;

/// <summary>
/// 指定当当前选定项被移除时，集合控件（如 TabControl 或 ListBox）所采取的选择行为.
/// </summary>
public enum SelectedItemRemoveBehaviour
{
    /// <summary>
    /// 移除后不选择任何项.
    /// </summary>
    SelectNone,

    /// <summary>
    /// 移除后自动选择集合中的第一个项.
    /// </summary>
    SelectFirst,

    /// <summary>
    /// 移除后自动选择集合中的最后一个项.
    /// </summary>
    SelectLast,

    /// <summary>
    /// 移除后选择紧邻原选定项之前的项.
    /// </summary>
    SelectPrevious,

    /// <summary>
    /// 移除后选择紧邻原选定项之后的项.
    /// </summary>
    SelectNext,
}