namespace BgControls.Windows.Controls.DragDrop;

/// <summary>
/// 表示视觉提示（如拖拽内容镜像或箭头提示）的显示行为枚举.
/// </summary>
public enum VisualCueBehavior
{
    /// <summary>
    /// 视觉提示始终显示在所有内容的顶层.
    /// </summary>
    ShowsOnTop,

    /// <summary>
    /// 视觉提示保持其当前相对于鼠标或源元素的位置.
    /// </summary>
    KeepCurrentPosition,
}