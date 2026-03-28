namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 定义触控拖拽操作的触发方式枚举.
/// </summary>
public enum TouchDragStartTrigger
{
    /// <summary>
    /// 通过点击、按住并开始移动来触发拖拽操作.
    /// </summary>
    TapHoldAndMove,

    /// <summary>
    /// 仅通过触控移动即可触发拖拽操作.
    /// </summary>
    TouchMove,

    /// <summary>
    /// 通过点击并按住（长按）一定时间来触发拖拽操作.
    /// </summary>
    TapAndHold,
}