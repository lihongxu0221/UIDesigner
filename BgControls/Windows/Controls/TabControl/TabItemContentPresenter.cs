namespace BgControls.Windows.Controls;

/// <summary>
/// 选项卡项的内容呈现器.
/// 该类已过时，建议使用视觉状态（Visual States）替代其功能.
/// </summary>
[Obsolete("此类将被移除。请改用视觉状态进行替代。", false)]
public class TabItemContentPresenter : ContentControl
{
    /// <summary>
    /// 存储所有者对象的弱引用，以避免内存泄漏.
    /// </summary>
    private WeakReference? ownerReference;

    /// <summary>
    /// Gets or sets 该内容呈现器所属的选项卡项（BgTabItem）所有者.
    /// </summary>
    internal BgTabItem? TabItemOwner
    {
        get
        {
            if (ownerReference != null)
            {
                return ownerReference.Target as BgTabItem;
            }

            return null;
        }

        set
        {
            ownerReference = new WeakReference(value);
        }
    }

    /// <summary>
    /// Gets 一个值，该值指示当前是否存在有效的所有者控件.
    /// </summary>
    private bool HasOwner => Owner != null;

    /// <summary>
    /// Gets 与该呈现器关联的选项卡控件（RadTabControl）实例.
    /// </summary>
    private BgTabControl Owner
    {
        get
        {
            if (TabItemOwner == null)
            {
                return null;
            }

            return TabItemOwner.Owner;
        }
    }

    /// <summary>
    /// 获取当前的排列方向.
    /// 如果未关联所有者，则默认为水平方向.
    /// </summary>
    private Orientation Orientation
    {
        get
        {
            if (!HasOwner)
            {
                return Orientation.Horizontal;
            }

            return Owner.TabOrientation;
        }
    }

    static TabItemContentPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TabItemContentPresenter), new FrameworkPropertyMetadata(typeof(ContentControl)));
    }

    /// <summary>
    /// 参与排列阶段的布局处理.
    /// 在垂直排列模式下，会对内容应用旋转变换.
    /// </summary>
    /// <param name="arrangeBounds">该元素在布局中应占用的区域尺寸.</param>
    /// <returns>元素排列后的实际尺寸.</returns>
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        this.RenderTransformOrigin = new Point(0.0, 0.0);
        TransformGroup transformGroup = new TransformGroup();

        // 如果是垂直方向，则交换宽高进行排列，然后再交换回来
        Size result = (Orientation != Orientation.Vertical) ?
            base.ArrangeOverride(arrangeBounds) :
            base.ArrangeOverride(arrangeBounds.Swap()).Swap();

        if (Orientation == Orientation.Vertical)
        {
            // 应用 90 度旋转变换
            transformGroup.Children.Add(new RotateTransform
            {
                Angle = -90.0,
                CenterY = 0.0,
                CenterX = 0.0,
            });

            // 应用偏移变换以纠正旋转后的位置
            transformGroup.Children.Add(new TranslateTransform
            {
                Y = result.Height,
            });
        }

        RenderTransform = transformGroup;
        return result;
    }

    /// <summary>
    /// 参与测量阶段的布局处理.
    /// 根据排列方向计算所需的尺寸.
    /// </summary>
    /// <param name="constraint">可分配的最大可用空间尺寸.</param>
    /// <returns>该元素在排列阶段所需的尺寸.</returns>
    protected override Size MeasureOverride(Size constraint)
    {
        if (Orientation == Orientation.Vertical)
        {
            // 在垂直模式下，交换约束尺寸进行测量，并返回交换后的期望尺寸
            return base.MeasureOverride(constraint.Swap()).Swap();
        }

        return base.MeasureOverride(constraint);
    }
}