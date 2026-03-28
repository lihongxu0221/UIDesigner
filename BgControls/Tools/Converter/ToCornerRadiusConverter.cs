namespace BgControls.Tools.Converter;

/// <summary>
/// 将各种类型（数字、格式化字符串）转换为 <see cref="CornerRadius"/> 结构的通用转换器.
/// </summary>
public class ToCornerRadiusConverter : IValueConverter
{
    /// <summary>
    /// 获取 <see cref="ToCornerRadiusConverter"/> 的静态实例，方便在 XAML 中直接引用.
    /// </summary>
    public static readonly ToCornerRadiusConverter Instance = new ToCornerRadiusConverter();

    /// <summary>
    /// Initializes a new instance of the <see cref="ToCornerRadiusConverter"/> class.
    /// </summary>
    public ToCornerRadiusConverter()
    {
    }

    /// <summary>
    /// 执行从源数据到 <see cref="CornerRadius"/> 的转换逻辑.
    /// </summary>
    /// <param name="value">输入对象，支持数值类型或特定格式的字符串（如 "a", "a,b", "a b c d"）.</param>
    /// <param name="targetType">绑定目标属性的类型.</param>
    /// <param name="parameter">要使用的转换器参数.</param>
    /// <param name="culture">要在转换器中使用的区域性信息.</param>
    /// <returns>转换后的圆角对象；如果解析失败则返回 0 值的圆角.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 1. 如果输入值为 null，直接返回 0 圆角.
        if (value == null)
        {
            return new CornerRadius(0);
        }

        // 2. 处理数值类型：整数、浮点数等. 此时四个方向的圆角使用同一个数值.
        if (value is double || value is int || value is float || value is decimal || value is long || value is short)
        {
            // 将各类数字统一转换为双精度浮点数.
            double uniformRadius = System.Convert.ToDouble(value);
            return new CornerRadius(Math.Max(0, uniformRadius));
        }

        // 3. 处理字符串格式. 支持多种分隔符及多值定义.
        if (value is string textValue)
        {
            // 使用逗号或空格作为分隔符进行分割，并剔除空字符串片段.
            string[] rawSegments = textValue.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // 尝试解析分割出的所有数字片段. 如果片段解析失败，则该片段默认为 0.
            double[] parsedRadii = rawSegments
                .Select(segment => double.TryParse(segment, NumberStyles.Any, culture, out double result) ? result : 0.0)
                .ToArray();

            switch (parsedRadii.Length)
            {
                // 情况 1: 字符串包含 1 个值 "a"，对应 CornerRadius(a, a, a, a).
                case 1:
                    return new CornerRadius(parsedRadii[0]);

                // 情况 2: 字符串包含 2 个值 "a,b"，对应 CornerRadius(a, b, a, b).
                case 2:
                    return new CornerRadius(parsedRadii[0], parsedRadii[1], parsedRadii[0], parsedRadii[1]);

                // 情况 3: 字符串包含 4 个值 "a,b,c,d"，对应 CornerRadius(a, b, c, d).
                case 4:
                    return new CornerRadius(parsedRadii[0], parsedRadii[1], parsedRadii[2], parsedRadii[3]);

                // 字符串长度不符合规格，回退到 0 圆角.
                default:
                    return new CornerRadius(0);
            }
        }

        // 4. 以上类型均不满足时，返回默认圆角值.
        return new CornerRadius(0);
    }

    /// <summary>
    /// 尚未实现从 <see cref="CornerRadius"/> 到原始数据的回传转换.
    /// </summary>
    /// <param name="value">绑定目标产生的值.</param>
    /// <param name="targetType">要转换回的源类型.</param>
    /// <param name="parameter">要使用的转换器参数.</param>
    /// <param name="culture">要在转换器中使用的区域性信息.</param>
    /// <returns>无返回值.</returns>
    /// <exception cref="NotImplementedException">始终抛出此异常.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}