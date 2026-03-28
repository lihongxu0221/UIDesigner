namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 默认的滑动惯性提供者，通过计算触控轨迹的速度和方向来生成惯性参数.
/// </summary>
internal class DefaultSwipeInertiaProvider : ISwipeInertiaProvider
{
    /// <summary>
    /// 判定显著点的时间阈值（毫秒）.
    /// </summary>
    private static double maxTimeThreshold = 50.0;

    /// <summary>
    /// 判定手指在终点附近停留的距离阈值.
    /// </summary>
    private static double quitInertiaVicinityThreshold = 8.0;

    /// <summary>
    /// 判定放弃惯性计算的停留时间阈值.
    /// </summary>
    private static double quitInertiaTimeThreshold = 100.0;

    /// <summary>
    /// 获取在指定时间阈值内的显著点数据，用于计算瞬时速度.
    /// </summary>
    /// <param name="timePoints">所有原始触摸点列表.</param>
    /// <param name="currentTime">当前计算的参考时间.</param>
    /// <returns>显著的触摸点列表.</returns>
    private static List<Tuple<DateTime, Point>> GetSignificantPoints(IList<Tuple<DateTime, Point>> timePoints, DateTime currentTime)
    {
        List<Tuple<DateTime, Point>> filteredPoints = new List<Tuple<DateTime, Point>>();

        // 遍历所有点，筛选出距离当前时间较近的点.
        for (int index = 0; index < timePoints.Count; index++)
        {
            double elapsedMilliseconds = (currentTime - timePoints[index].Item1).TotalMilliseconds;
            if (elapsedMilliseconds < DefaultSwipeInertiaProvider.maxTimeThreshold)
            {
                filteredPoints.Add(timePoints[index]);
            }
        }

        // 如果只筛选出一个点，则补入倒数第二个点以保证后续计算速度所需的最小点数.
        if (filteredPoints.Count == 1 && timePoints.Count >= 2)
        {
            filteredPoints.Insert(0, timePoints[timePoints.Count - 2]);
        }

        return filteredPoints;
    }

    /// <summary>
    /// 计算手指在终点位置附近（邻域内）停留的时间.
    /// </summary>
    /// <param name="timePoints">触摸点列表.</param>
    /// <param name="currentTime">当前时间.</param>
    /// <returns>在邻域内停留的总毫秒数.</returns>
    private static double GetTimeSpentInVicinity(IList<Tuple<DateTime, Point>> timePoints, DateTime currentTime)
    {
        Point lastPosition = timePoints[timePoints.Count - 1].Item2;
        Tuple<DateTime, Point> vicinityStartPoint = timePoints[timePoints.Count - 1];

        // 从后往前查找离开邻域的临界点.
        for (int index = timePoints.Count - 2; index >= 0; index--)
        {
            Point currentPosition = timePoints[index].Item2;
            double distance = MathUtilities.CalculateDistance(lastPosition, currentPosition);

            // 如果距离在阈值内，则更新邻域起始点.
            if (distance <= DefaultSwipeInertiaProvider.quitInertiaVicinityThreshold)
            {
                vicinityStartPoint = timePoints[index];
            }
        }

        return (currentTime - vicinityStartPoint.Item1).TotalMilliseconds;
    }

    /// <summary>
    /// 计算虚拟的轨迹尾部点，用于平滑整体滑动速度的计算.
    /// </summary>
    /// <param name="timePoints">显著触摸点列表.</param>
    /// <returns>计算出的虚拟起点数据.</returns>
    private static Tuple<DateTime, Point> GetTail(IList<Tuple<DateTime, Point>> timePoints)
    {
        Tuple<DateTime, Point> lastPoint = timePoints[timePoints.Count - 1];
        Tuple<DateTime, Point> secondLastPoint = timePoints[timePoints.Count - 2];

        // 计算整条轨迹的总长度.
        double totalRouteLength = DefaultSwipeInertiaProvider.GetTotalRouteLength(timePoints);
        Point endPosition = lastPoint.Item2;
        Point prevPosition = secondLastPoint.Item2;

        // 根据总长度推算虚拟的起始位置.
        Point virtualStartPosition = MathUtilities.GetPoint(endPosition, prevPosition, totalRouteLength);
        return new Tuple<DateTime, Point>(timePoints[0].Item1, virtualStartPosition);
    }

    /// <summary>
    /// 计算给定触摸点序列的总路径长度.
    /// </summary>
    /// <param name="timePoints">触摸点列表.</param>
    /// <returns>总距离长度.</returns>
    private static double GetTotalRouteLength(IList<Tuple<DateTime, Point>> timePoints)
    {
        double totalLength = 0.0;
        Tuple<DateTime, Point>? previousPoint = null;

        foreach (Tuple<DateTime, Point> currentPoint in timePoints)
        {
            if (previousPoint != null)
            {
                // 累加相邻两点间的欧几里得距离.
                double segmentLength = MathUtilities.CalculateDistance(currentPoint.Item2, previousPoint.Item2);
                totalLength += segmentLength;
            }

            previousPoint = currentPoint;
        }

        return totalLength;
    }

    /// <summary>
    /// 根据触控轨迹计算滑动惯性参数.
    /// </summary>
    /// <param name="timePoints">触控移动产生的时间点集合.</param>
    /// <returns>滑动惯性参数实例，若不满足惯性触发条件则返回 null.</returns>
    public SwipeInertiaParams? GetInertiaParams(IList<Tuple<DateTime, Point>> timePoints)
    {
        // 验证输入参数不为空.
        ArgumentNullException.ThrowIfNull(timePoints, nameof(timePoints));

        DateTime now = DateTime.Now;
        if (timePoints.Count < 2)
        {
            return null;
        }

        // 1. 检查手指在终点附近的停留时间，若停顿太久则不触发惯性.
        double vicinityTime = DefaultSwipeInertiaProvider.GetTimeSpentInVicinity(timePoints, now);
        if (DefaultSwipeInertiaProvider.quitInertiaTimeThreshold < vicinityTime)
        {
            return null;
        }

        // 2. 提取最近一段时间内的显著移动点.
        List<Tuple<DateTime, Point>> significantPoints = DefaultSwipeInertiaProvider.GetSignificantPoints(timePoints, now);
        if (significantPoints.Count < 2)
        {
            return null;
        }

        // 3. 基于显著点和轨迹长度计算虚拟尾部并得出最终速度.
        Tuple<DateTime, Point> virtualTail = DefaultSwipeInertiaProvider.GetTail(significantPoints);
        Tuple<DateTime, Point> finalPoint = significantPoints[significantPoints.Count - 1];

        double totalElapsedMs = (finalPoint.Item1 - virtualTail.Item1).TotalMilliseconds;

        // 避免除以零的情况.
        if (totalElapsedMs <= 0)
        {
            return null;
        }

        // 根据时间比例计算水平和垂直速度.
        double velocityFactor = SwipeInertiaHelper.SpeedBaseDuration / totalElapsedMs;
        double horizontalVelocity = (finalPoint.Item2.X - virtualTail.Item2.X) * velocityFactor;
        double verticalVelocity = (finalPoint.Item2.Y - virtualTail.Item2.Y) * velocityFactor;

        // 返回包含速度和缓动函数的惯性参数.
        return new SwipeInertiaParams(horizontalVelocity, verticalVelocity, new ExponentialEase());
    }
}