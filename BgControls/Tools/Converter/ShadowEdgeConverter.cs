using BgControls.Windows.Attach;
using BgControls.Windows.Datas;

namespace BgControls.Tools.Converter;

/// <summary>
/// 阴影边缘剪切转换器，用于根据阴影的模糊半径和指定的显示边缘计算剪切区域.
/// </summary>
public class ShadowEdgeConverter : IMultiValueConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowEdgeConverter"/> class.
    /// </summary>
    public ShadowEdgeConverter()
    {
    }

    /// <summary>
    /// 将多个绑定值转换为用于阴影剪切的画刷.
    /// </summary>
    /// <param name="values">绑定值数组，索引依次为：[0]宽度, [1]高度, [2]海拔/阴影信息, [3](可选)边缘枚举.</param>
    /// <param name="targetType">目标属性类型.</param>
    /// <param name="parameter">转换器参数.</param>
    /// <param name="culture">区域性信息.</param>
    /// <returns>返回配置好的 DrawingBrush 对象；若输入无效则返回 null.</returns>
    public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        // 验证输入数组是否有效且至少包含宽度、高度和阴影信息三个参数.
        if (values != null && values.Length >= 3)
        {
            var validWidth = GetValidSize(values[0]); // 尝试获取并验证宽度值.
            var validHeight = GetValidSize(values[1]); // 尝试获取并验证高度值.
            var shadowEffect = GetDropShadow(values[2]); // 获取阴影效果实例.

            if (validWidth.HasValue && validHeight.HasValue && shadowEffect != null)
            {
                double width = validWidth.GetValueOrDefault();
                double height = validHeight.GetValueOrDefault();
                double blurRadius = shadowEffect.BlurRadius;

                Rect clippingRect;

                // 处理特定边缘显示的逻辑.
                if (values.Length > 3 && values[3] is ShadowEdges shadowEdges && shadowEdges != ShadowEdges.All)
                {
                    // 初始化基础矩形（不带阴影扩展）.
                    clippingRect = new Rect(0.0, 0.0, width, height);

                    // 如果包含左侧边缘，向左扩展矩形范围.
                    if (shadowEdges.HasFlag(ShadowEdges.Left))
                    {
                        Rect tempRect = clippingRect;
                        tempRect.X = 0.0 - blurRadius;
                        tempRect.Width = width + blurRadius;
                        clippingRect = tempRect;
                    }

                    // 如果包含顶侧边缘，向上扩展矩形范围.
                    if (shadowEdges.HasFlag(ShadowEdges.Top))
                    {
                        Rect tempRect = clippingRect;
                        tempRect.Y = 0.0 - blurRadius;
                        tempRect.Height = height + blurRadius;
                        clippingRect = tempRect;
                    }

                    // 如果包含右侧边缘，增加矩形宽度.
                    if (shadowEdges.HasFlag(ShadowEdges.Right))
                    {
                        Rect tempRect = clippingRect;
                        tempRect.Width = clippingRect.Width + blurRadius;
                        clippingRect = tempRect;
                    }

                    // 如果包含底侧边缘，增加矩形高度.
                    if (shadowEdges.HasFlag(ShadowEdges.Bottom))
                    {
                        Rect tempRect = clippingRect;
                        tempRect.Height = clippingRect.Height + blurRadius;
                        clippingRect = tempRect;
                    }
                }
                else
                {
                    // 默认情况下（全部边缘），向四周扩展模糊半径的距离.
                    clippingRect = new Rect(0.0 - blurRadius, 0.0 - blurRadius, width + (blurRadius * 2.0), height + (blurRadius * 2.0));
                }

                // 构建绘图画刷，用于设置不透明掩码或剪裁区域.
                return new DrawingBrush(
                    new GeometryDrawing(Brushes.White, null, new RectangleGeometry(clippingRect)))
                {
                    Stretch = Stretch.None,
                    TileMode = TileMode.None,
                    Viewport = clippingRect,
                    ViewportUnits = BrushMappingMode.Absolute,
                    Viewbox = clippingRect,
                    ViewboxUnits = BrushMappingMode.Absolute,
                };
            }
        }

        return null;
    }

    /// <summary>
    /// 不支持反向转换.
    /// </summary>
    /// <param name="value">目标值.</param>
    /// <param name="targetTypes">源类型数组.</param>
    /// <param name="parameter">转换器参数.</param>
    /// <param name="culture">区域性信息.</param>
    /// <returns>抛出未实现异常.</returns>
    /// <exception cref="NotImplementedException">始终抛出此异常.</exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // 校验参数.
        ArgumentNullException.ThrowIfNull(targetTypes, nameof(targetTypes));
        throw new NotImplementedException();
    }

    /// <summary>
    /// 根据输入对象解析并获取对应的阴影效果.
    /// </summary>
    /// <param name="value">海拔枚举或阴影深度相关的对象.</param>
    /// <returns>返回解析出的 DropShadowEffect 实例，若无效则返回 null.</returns>
    private static DropShadowEffect? GetDropShadow(object? value)
    {
        // 如果输入是海拔枚举，从辅助类获取阴影.
        if (value is Elevation elevation)
        {
            return ElevationAssist.GetDropShadow(elevation);
        }

        return null;
    }

    /// <summary>
    /// 验证并提取有效的双精度尺寸值.
    /// </summary>
    /// <param name="value">绑定的尺寸对象.</param>
    /// <returns>返回有效的双精度值，若为无效数字、无穷大或非 double 类型则返回 null.</returns>
    private static double? GetValidSize(object? value)
    {
        // 判定是否为有效的 double 且非异常数字.
        if (!(value is double doubleValue) || double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
        {
            return null;
        }

        return doubleValue;
    }
}