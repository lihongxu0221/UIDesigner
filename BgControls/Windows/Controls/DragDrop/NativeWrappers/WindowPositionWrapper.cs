namespace BgControls.Windows.Controls.DragDrop.NativeWrappers;

/// <summary>
/// 窗口位置操作包装类，提供移动窗口位置的功能.
/// </summary>
internal static class WindowPositionWrapper
{
    /// <summary>
    /// 原生方法定义类.
    /// </summary>
    private static class NativeMethods
    {
        /// <summary>
        /// 改变指定窗口的位置和尺寸.
        /// </summary>
        /// <param name="handle">窗口句柄.</param>
        /// <param name="x">新左侧位置.</param>
        /// <param name="y">新顶部位置.</param>
        /// <param name="width">新宽度.</param>
        /// <param name="height">新高度.</param>
        /// <param name="repaint">是否重绘窗口.</param>
        /// <returns>操作成功返回 true. </returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, [MarshalAs(UnmanagedType.Bool)] bool repaint);
    }

    /// <summary>
    /// 移动指定的窗口到新的坐标和尺寸.
    /// </summary>
    /// <param name="handle">窗口句柄.</param>
    /// <param name="xPosition">水平位置.</param>
    /// <param name="yPosition">垂直位置.</param>
    /// <param name="newWidth">窗口宽度.</param>
    /// <param name="newHeight">窗口高度.</param>
    /// <param name="shouldRefresh">是否刷新界面.</param>
    /// <returns>移动是否成功. </returns>
    public static bool MoveWindow(IntPtr handle, int xPosition, int yPosition, int newWidth, int newHeight, bool shouldRefresh)
    {
        // 调用底层的移动窗口接口.
        return NativeMethods.MoveWindow(handle, xPosition, yPosition, newWidth, newHeight, shouldRefresh);
    }
}