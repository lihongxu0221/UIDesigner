using BgControls.Windows.Attach;
using BgControls.Windows.Datas;

namespace BgControls.Tools.Converter;

/// <summary>
/// 阴影转换器，用于根据 <see cref="Elevation"/> 枚举值创建并返回新的 <see cref="DropShadowEffect"/> 实例.
/// </summary>
public class ShadowConverter : IValueConverter
{
    /// <summary>
    /// 获取 <see cref="ShadowConverter"/> 的静态实例，用于 XAML 中的快速引用.
    /// </summary>
    public static readonly ShadowConverter Instance = new ShadowConverter();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowConverter"/> class.
    /// </summary>
    public ShadowConverter()
    {
    }

    /// <summary>
    /// 将 <see cref="Elevation"/> 转换成一个新的 <see cref="DropShadowEffect"/> 对象.
    /// </summary>
    /// <param name="value">绑定的源数据，预期为 <see cref="Elevation"/> 枚举类型.</param>
    /// <param name="targetType">绑定目标属性的类型.</param>
    /// <param name="parameter">转换器参数.</param>
    /// <param name="culture">区域性信息.</param>
    /// <returns>返回转换后的阴影效果实例；如果输入无效则返回 null.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 判定输入值是否为 Elevation 枚举
        if (value is Elevation elevationValue)
        {
            // 通过辅助类获取预定义的阴影效果模板
            DropShadowEffect? sourceEffect = ElevationAssist.GetDropShadow(elevationValue);

            // 如果模板不为空，则克隆一个新实例返回（避免多处引用同一个 Effect 对象导致的渲染冲突）
            if (sourceEffect != null)
            {
                return new DropShadowEffect
                {
                    BlurRadius = sourceEffect.BlurRadius,
                    Color = sourceEffect.Color,
                    Direction = sourceEffect.Direction,
                    Opacity = sourceEffect.Opacity,
                    RenderingBias = sourceEffect.RenderingBias,
                    ShadowDepth = sourceEffect.ShadowDepth,
                };
            }
        }

        // 如果不匹配或无对应效果，则返回空
        return null;
    }

    /// <summary>
    /// 此转换器不支持从阴影效果还原回海拔枚举.
    /// </summary>
    /// <param name="value">来自目标的阴影效果值.</param>
    /// <param name="targetType">要转换回的源类型.</param>
    /// <param name="parameter">转换器参数.</param>
    /// <param name="culture">区域性信息.</param>
    /// <returns>不返回任何结果.</returns>
    /// <exception cref="NotImplementedException">始终抛出此异常.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 抛出未实现异常，因为阴影效果无法唯一对应回特定的海拔枚举
        throw new NotImplementedException();
    }
}