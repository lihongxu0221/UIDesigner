namespace BgControls.Windows.DragDrop;

/// <summary>
/// 提供针对 <see cref="MouseEventArgs"/> 的扩展方法辅助类.
/// </summary>
internal static class MouseEventArgsExtensions
{
    /// <summary>
    /// 安全地获取鼠标相对于指定元素的当前位置.
    /// 该方法通过捕获可能的异常（例如当参考元素未关联到视觉树时）并返回默认坐标值来确保调用的安全性.
    /// </summary>
    /// <param name="mouseEventArgs">鼠标事件参数实例.</param>
    /// <param name="relativeTo">要在其中计算位置的参考元素.</param>
    /// <returns>鼠标相对于参考元素的坐标点；如果获取失败，则返回原点坐标 (0,0).</returns>
    public static Point GetSafePosition(this MouseEventArgs mouseEventArgs, IInputElement relativeTo)
    {
        try
        {
            // 尝试获取标准位置
            return mouseEventArgs.GetPosition(relativeTo);
        }
        catch
        {
            // 如果元素无效或已断开连接，则回退到安全默认值
            return new Point(0.0, 0.0);
        }
    }
}