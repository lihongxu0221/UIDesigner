using BgCommon.Localization.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BgControls.Windows.Controls;

/// <summary>
/// 指定选项卡栏（TabStrip）在选项卡控件中的对齐方式.
/// </summary>
[TypeConverter(typeof(EnumLocalizationConverter))]
public enum TabStripAlign
{
    /// <summary>
    /// 两端对齐. 选项卡项将拉伸或均匀分布以填满可用空间.
    /// </summary>
    [Display(Name= "两端对齐")]
    Justify,

    /// <summary>
    /// 居中对齐. 选项卡项集合将在可用空间内水平或垂直居中.
    /// </summary>
    [Display(Name = "居中对齐")]
    Center,

    /// <summary>
    /// 左对齐（在垂直模式下为顶端对齐）. 选项卡项将向起始位置靠拢.
    /// </summary>
    [Display(Name = "左对齐")]
    Left,

    /// <summary>
    /// 右对齐（在垂直模式下为底端对齐）. 选项卡项将向结束位置靠拢.
    /// </summary>
    [Display(Name = "右对齐")]
    Right,
}