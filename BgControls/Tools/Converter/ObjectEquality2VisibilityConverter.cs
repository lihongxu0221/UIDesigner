namespace BgControls.Tools.Converter;

/// <summary>
/// 对象相等性到可见性的转换器.
/// </summary>
/// <remarks>
/// <para>
/// 将绑定的值 (<c>value</c>) 与转换器参数 (<c>parameter</c>) 进行比较.
/// </para>
/// <para>
/// 如果相等，返回 <see cref="EqualsVisibility"/> (默认 Visible)；
/// 如果不相等，返回 <see cref="NotEqualsVisibility"/> (默认 Collapsed).
/// </para>
/// </remarks>
[Obsolete("建议使用 BgControls.Tools.Converter.Equality2ValueConverter，以获得更好的类型安全性和性能。")]
public class ObjectEquality2VisibilityConverter : IValueConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectEquality2VisibilityConverter" /> class.
    /// </summary>
    public ObjectEquality2VisibilityConverter()
    {
        this.EqualsVisibility = Visibility.Visible;
        this.NotEqualsVisibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Gets or sets 当值与参数相等时返回的可见性.
    /// </summary>
    /// <value>默认为 <see cref="Visibility.Visible"/>.</value>
    public Visibility EqualsVisibility { get; set; }

    /// <summary>
    /// Gets or sets the visibility to return when the value to convert does not equal the converter parameter.
    /// </summary>
    /// <value>The not equals visibility.</value>
    public Visibility NotEqualsVisibility { get; set; }

    /// <summary>
    /// 将值转换为可见性.
    /// </summary>
    /// <param name="value">绑定源产生的值.</param>
    /// <param name="targetType">绑定目标属性的类型.</param>
    /// <param name="parameter">要比较的参数.</param>
    /// <param name="culture">区域信息.</param>
    /// <returns>转换后的 <see cref="Visibility"/> 值.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 1. 处理双 null 情况 (视为相等)
        if (value == null && parameter == null)
        {
            return this.EqualsVisibility;
        }

        // 2. 处理单 null 情况 (视为不相等)
        if (value == null || parameter == null)
        {
            return this.NotEqualsVisibility;
        }

        // 3. 尝试直接比较 (处理相同类型或实现了 Equals 的情况)
        if (object.Equals(value, parameter))
        {
            return this.EqualsVisibility;
        }

        // 4. 类型不同时的兼容处理
        // (例如：ViewModel 是 int 类型，但 XAML 传入的 CommandParameter 是 string 类型)
        try
        {
            // 尝试将参数转换为值的类型进行比较
            // 注意：如果 value 是 Enum，ChangeType 也能处理整数或字符串到枚举的转换
            var convertedParameter = System.Convert.ChangeType(parameter, value.GetType(), culture);
            if (value.Equals(convertedParameter))
            {
                return this.EqualsVisibility;
            }
            else
            {
                return this.NotEqualsVisibility;
            }
        }
        catch
        {
        }

        // 5. 最后的手段：比较字符串形式 (处理复杂的 Enum 或自定义类型)
        // 使用 StringComparison.Ordinal 提高性能，或者根据需求使用 CurrentCulture
        string? valueString = value.ToString();
        string? paramString = parameter.ToString();

        if (string.Equals(valueString, paramString, StringComparison.Ordinal))
        {
            return EqualsVisibility;
        }

        return this.NotEqualsVisibility;
    }

    /// <summary>
    /// 反向转换 (通常用于 RadioButton 样式的绑定).
    /// </summary>
    /// <param name="value">绑定目标产生的值 (可见性).</param>
    /// <param name="targetType">要转换到的类型.</param>
    /// <param name="parameter">转换器参数.</param>
    /// <param name="culture">区域信息.</param>
    /// <returns>
    /// 如果可见性等于 <see cref="EqualsVisibility"/> (通常为 Visible)，则返回 <paramref name="parameter"/>；
    /// 否则返回 <see cref="Binding.DoNothing"/>.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 只有当 UI 状态变为 "Visible" 时，才将参数值写回源属性.
        // 这模仿了 RadioButton 的行为：只有被选中的项才会更新绑定的 Enum 值.
        if (value is Visibility visibility && visibility == EqualsVisibility)
        {
            // 尝试将参数转换回目标类型，以支持 TwoWay 绑定
            try
            {
                return System.Convert.ChangeType(parameter, targetType, culture);
            }
            catch
            {
                return parameter;
            }
        }

        return Binding.DoNothing;
    }
}