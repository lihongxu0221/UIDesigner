namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 触控移动辅助类，用于跟踪和管理触控点的位置状态.
/// </summary>
internal class TouchMoveHelper
{
    /// <summary>
    /// 比较浮点数时允许的最小误差阈值.
    /// </summary>
    private const double Epsilon = 0.00001;

    /// <summary>
    /// 记录触控 ID 与当前移动位置的映射字典.
    /// </summary>
    private readonly Dictionary<int, Point> touchIdToMovePosition;

    /// <summary>
    /// 记录触控 ID 与按下时初始位置的映射字典.
    /// </summary>
    private readonly Dictionary<int, Point> touchIdToDownPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="TouchMoveHelper"/> class.
    /// </summary>
    public TouchMoveHelper()
    {
        // 初始化存储触控位置信息的字典.
        this.touchIdToMovePosition = new Dictionary<int, Point>();
        this.touchIdToDownPosition = new Dictionary<int, Point>();
    }

    /// <summary>
    /// 清空所有记录的触控点状态，防止内存残留.
    /// </summary>
    public void Clear()
    {
        this.touchIdToMovePosition.Clear();
        this.touchIdToDownPosition.Clear();
    }

    /// <summary>
    /// 当触控按下时记录初始位置.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <param name="position">按下的坐标点信号.</param>
    public void OnTouchDown(int touchId, Point position)
    {
        // 清除旧的移动记录并更新按下记录.
        this.touchIdToMovePosition.Remove(touchId);
        this.touchIdToDownPosition[touchId] = position;
    }

    /// <summary>
    /// 当触控移动时更新当前位置.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <param name="position">当前的坐标点信号.</param>
    public void OnTouchMove(int touchId, Point position)
    {
        // 更新指定触控 ID 的最新移动位置.
        this.touchIdToMovePosition[touchId] = position;
    }

    /// <summary>
    /// 当触控抬起时清除对应的位置记录.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    public void OnTouchUp(int touchId)
    {
        // 移除该触控 ID 关联的所有位置信息.
        this.touchIdToMovePosition.Remove(touchId);
        this.touchIdToDownPosition.Remove(touchId);
    }

    /// <summary>
    /// 判断是否应该抑制移动. 使用模糊匹配以处理硬件微小抖动.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <param name="newPosition">新的坐标点.</param>
    /// <returns>如果应该抑制则返回 true; 否则返回 false.</returns>
    public bool ShouldSuppressTouchMove(int touchId, Point newPosition)
    {
        if (this.touchIdToMovePosition.ContainsKey(touchId))
        {
            return false;
        }

        var downPosition = this.TryGetTouchDownPosition(touchId);
        if (downPosition.HasValue)
        {
            // 采用坐标差值的绝对值判断，而非直接使用 == 比较.
            // return downPosition.Value == newPosition;
            return this.IsPointCloseEnough(downPosition.Value, newPosition);
        }

        return false;
    }

    /// <summary>
    /// 检查最后记录位置与抬起位置是否在视觉上一致.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <param name="touchUpPosition">抬起时的坐标点.</param>
    /// <returns>需要触发则返回 true; 否则返回 false.</returns>
    public bool ShouldRaiseLastTouchMoveBeforeTouchUp(int touchId, Point touchUpPosition)
    {
        var lastMove = this.TryGetTouchMovePosition(touchId);
        if (lastMove.HasValue)
        {
            // return lastMove.Value != touchUpPosition;
            return !this.IsPointCloseEnough(lastMove.Value, touchUpPosition);
        }

        var initialDown = this.TryGetTouchDownPosition(touchId);
        if (initialDown.HasValue)
        {
            // return initialDown.Value != touchUpPosition;
            return !this.IsPointCloseEnough(initialDown.Value, touchUpPosition);
        }

        return false;
    }

    /// <summary>
    /// 尝试获取指定触控 ID 的最后移动位置.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <returns>移动位置的坐标点，若不存在则返回 null.</returns>
    public Point? TryGetTouchMovePosition(int touchId)
    {
        // 从移动位置映射表中提取数据.
        if (this.touchIdToMovePosition.TryGetValue(touchId, out var currentPosition))
        {
            return currentPosition;
        }

        return null;
    }

    /// <summary>
    /// 尝试获取指定触控 ID 的按下位置.
    /// </summary>
    /// <param name="touchId">触控 ID.</param>
    /// <returns>按下的坐标点，若不存在则返回 null.</returns>
    private Point? TryGetTouchDownPosition(int touchId)
    {
        // 从按下位置映射表中提取数据.
        if (this.touchIdToDownPosition.TryGetValue(touchId, out var initialPosition))
        {
            return initialPosition;
        }

        return null;
    }

    /// <summary>
    /// 内部辅助方法：判断两个点在指定的误差范围内是否相等.
    /// </summary>
    private bool IsPointCloseEnough(Point point1, Point point2)
    {
        // 只有当 X 和 Y 的偏移量都小于极小值时，才认为两个坐标点相同.
        return Math.Abs(point1.X - point2.X) < Epsilon &&
               Math.Abs(point1.Y - point2.Y) < Epsilon;
    }
}