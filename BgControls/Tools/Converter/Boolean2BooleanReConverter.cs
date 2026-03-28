namespace BgControls.Tools.Converter;

/// <summary>
/// 布尔值取反转换器 (Boolean Inverse Converter).
/// </summary>
/// <remarks>
/// <para>
/// 此转换器用于将布尔值进行逻辑翻转 (NOT 操作).
/// </para>
/// <para>
/// <b>主要用途：</b>
/// <list type="bullet">
/// <item>将 <c>true</c> 转换为 <c>false</c>.</item>
/// <item>将 <c>false</c> 转换为 <c>true</c>.</item>
/// </list>
/// </para>
/// <para>
/// <b>常见场景：</b>
/// 例如，将控件的 <c>IsEnabled</c> 属性绑定到 ViewModel 的 <c>IsBusy</c> 属性.
/// 当 <c>IsBusy</c> 为 true 时，控件应被禁用 (IsEnabled = false)，此时就需要此转换器.
/// </para>
/// </remarks>
[Obsolete("建议使用 BgControls.Tools.Converter.Equality2ValueConverter 替代此转换器。")]
public class Boolean2BooleanReConverter : IValueConverter
{
    /// <summary>
    /// 正向转换：将源数据的布尔值取反后传给目标属性.
    /// </summary>
    /// <param name="value">绑定的源值.</param>
    /// <param name="targetType">绑定目标的类型.</param>
    /// <param name="parameter">转换器参数 (未使用).</param>
    /// <param name="culture">区域信息.</param>
    /// <returns>
    /// 如果 <paramref name="value"/> 是 <c>bool</c> 类型，返回其非值 (!value)；
    /// 否则返回原始值.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return value;
    }

    /// <summary>
    /// 反向转换：将目标属性的布尔值取反后回写到源数据.
    /// </summary>
    /// <param name="value">目标属性的值.</param>
    /// <param name="targetType">源数据的类型.</param>
    /// <param name="parameter">转换器参数 (未使用).</param>
    /// <param name="culture">区域信息.</param>
    /// <returns>
    /// 如果 <paramref name="value"/> 是 <c>bool</c> 类型，返回其非值 (!value)；
    /// 否则返回原始值.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return value;
    }
}