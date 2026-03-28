namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 提供触控计算相关的数学工具类.
/// </summary>
internal static class MathUtilities
{
    /// <summary>
    /// 计算两个坐标点之间的欧几里得距离.
    /// </summary>
    /// <param name="point1">第一个坐标点.</param>
    /// <param name="point2">第二个坐标点.</param>
    /// <returns>两点之间的距离.</returns>
    internal static double CalculateDistance(Point point1, Point point2)
    {
        // 计算 X轴 和 Y轴 的差值.
        double deltaX = point1.X - point2.X;
        double deltaY = point1.Y - point2.Y;

        // 使用勾股定理计算距离.
        return Math.Sqrt(Math.Pow(deltaX, 2.0) + Math.Pow(deltaY, 2.0));
    }

    /// <summary>
    /// 根据给定的长度比例，在两点确定的直线上计算一个新的坐标点.
    /// </summary>
    /// <param name="p1">起始参考点.</param>
    /// <param name="p2">方向参考点.</param>
    /// <param name="length">延伸的长度值.</param>
    /// <returns>计算出的新坐标点.</returns>
    internal static Point GetPoint(Point p1, Point p2, double length)
    {
        // 首先计算两参考点之间的基础距离.
        double baseDistance = MathUtilities.CalculateDistance(p1, p2);

        // 计算延伸长度与基础距离的比率.
        double scaleRatio = length / baseDistance;

        // 根据比率线性插值计算新的 X 和 Y 坐标.
        double resultX = p1.X + (scaleRatio * (p2.X - p1.X));
        double resultY = p1.Y + (scaleRatio * (p2.Y - p1.Y));

        return new Point(resultX, resultY);
    }

    /// <summary>
    /// 验证指定的双精度浮点数是否为有效数字（非无穷大且在有效范围内）.
    /// </summary>
    /// <param name="value">待验证的数值.</param>
    /// <returns>如果数值有效则返回 true; 否则返回 false.</returns>
    internal static bool IsValidNumber(double value)
    {
        // 检查数值是否在双精度浮点数的可表示范围内.
        return value >= double.MinValue && value < double.MaxValue;
    }
}