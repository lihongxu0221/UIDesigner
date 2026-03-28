using HandyControl.Controls;

namespace BgControls.Windows.Controls;

/// <summary>
/// 用于选项卡项布局的自定义面板.
/// 该面板支持多行排列、对齐方式设置以及基于选定项的行重排功能.
/// </summary>
public class TabStripPanel : Panel
{
    /// <summary>
    /// 标识 <see cref="RearrangeTabs"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty RearrangeTabsProperty =
        DependencyProperty.Register("RearrangeTabs", typeof(bool), typeof(TabStripPanel), new PropertyMetadata(true, OnPanelPropertyChnaged));

    /// <summary>
    /// 标识 <see cref="TabStripPlacement"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty TabStripPlacementProperty =
        DependencyProperty.Register("TabStripPlacement", typeof(Dock), typeof(TabStripPanel), new PropertyMetadata(Dock.Top, OnPanelPropertyChnaged));

    /// <summary>
    /// 标识 <see cref="AllTabsEqualHeight"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty AllTabsEqualHeightProperty =
        DependencyProperty.Register("AllTabsEqualHeight", typeof(bool), typeof(TabStripPanel), new PropertyMetadata(true, OnPanelPropertyChnaged));

    /// <summary>
    /// 标识 <see cref="Align"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty AlignProperty =
        DependencyProperty.Register("Align", typeof(TabStripAlign), typeof(TabStripPanel), new PropertyMetadata(TabStripAlign.Justify, null));

    /// <summary>
    /// 存储子元素的行集合.
    /// </summary>
    private IList<List<UIElement>>? rows;

    /// <summary>
    /// Gets or sets 选项卡条相对于内容的停靠位置.
    /// </summary>
    public Dock TabStripPlacement
    {
        get { return (Dock)GetValue(TabStripPlacementProperty); }
        set { SetValue(TabStripPlacementProperty, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether 同一行中的所有选项卡是否应具有相同的高度（或在垂直模式下具有相同的宽度）.
    /// </summary>
    public bool AllTabsEqualHeight
    {
        get { return (bool)GetValue(AllTabsEqualHeightProperty); }
        set { SetValue(AllTabsEqualHeightProperty, value); }
    }

    /// <summary>
    /// Gets or sets 选项卡项在面板内的对齐方式.
    /// </summary>
    public TabStripAlign Align
    {
        get { return (TabStripAlign)GetValue(AlignProperty); }
        set { SetValue(AlignProperty, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否根据选定项重新排列选项卡行的顺序（通常使选定行处于最底层或最顶层）.
    /// </summary>
    public bool RearrangeTabs
    {
        get { return (bool)GetValue(RearrangeTabsProperty); }
        set { SetValue(RearrangeTabsProperty, value); }
    }

    /// <summary>
    /// Gets or sets 内部的行逻辑集合.
    /// </summary>
    internal IList<List<UIElement>>? Rows
    {
        get { return this.rows; }
        set { this.rows = value; }
    }

    /// <summary>
    /// 初始化行集合，根据子元素的换行属性（IsBreak）将子元素分配到不同的行中.
    /// </summary>
    internal void Initialize()
    {
        this.rows = new List<List<UIElement>>(2);
        this.rows.Add(new List<UIElement>(8));
        int currentRowIndex = 0;

        foreach (UIElement child in this.Children)
        {
            if (child != null)
            {
                this.rows[currentRowIndex].Add(child);
            }

            if (child is BgTabItem tabItem && tabItem.IsBreak)
            {
                currentRowIndex++;
                this.rows.Add(new List<UIElement>(8));
            }
        }

        if (this.rows.Last().Count == 0)
        {
            this.rows.Remove(this.rows.Last());
        }
    }

    /// <summary>
    /// 为选定项重新排列行顺序.
    /// 将包含选定选项卡的行移至集合末尾，以便在视觉上优先显示.
    /// </summary>
    internal void RearrangeForSelectedItem()
    {
        if (this.rows != null)
        {
            if (this.RearrangeTabs && this.rows.Any())
            {
                var selectedRow = this.rows.FirstOrDefault(row => row.OfType<BgTabItem>().Any(tab => tab.IsSelected));
                if (selectedRow != null)
                {
                    this.rows.Remove(selectedRow);
                    this.rows.Add(selectedRow);
                }
            }

            this.rows = this.rows.Where(r => r != null).ToList();
        }
    }

    /// <summary>
    /// 计算子元素所需的测量尺寸.
    /// 根据停靠位置和行逻辑计算面板的期望大小.
    /// </summary>
    /// <param name="availableSize">父元素可提供的最大可用空间.</param>
    /// <returns>面板测量后的期望尺寸.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        double totalDesiredWidth = 0.0;
        double totalDesiredHeight = 0.0;

        if (this.Children.Count == 0)
        {
            return new Size(totalDesiredWidth, totalDesiredHeight);
        }

        this.Initialize();
        if (this.rows != null)
        {
            // 横向布局（Top/Bottom）
            if (this.TabStripPlacement == Dock.Top || this.TabStripPlacement == Dock.Bottom)
            {
                totalDesiredHeight = this.rows.Sum(row => row.Max(item =>
                {
                    item.Measure(new Size(availableSize.Width, availableSize.Height));
                    return item.DesiredSize.Height;
                }));

                if (!double.IsPositiveInfinity(availableSize.Height))
                {
                    totalDesiredHeight = Math.Min(totalDesiredHeight, availableSize.Height);
                }

                var rowWidths = this.rows.Select(row => row.Sum(item =>
                {
                    item.Measure(new Size(availableSize.Width, availableSize.Height));
                    return item.DesiredSize.Width;
                })).ToList();

                totalDesiredWidth = rowWidths.Count > 0 ? rowWidths.Max() : 0.0;

                // 缩放处理：如果某行宽度超过可用宽度，则对该行项进行压缩测量
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rowWidths[i] > availableSize.Width)
                    {
                        double horizontalShrinkFactor = availableSize.Width / rowWidths[i];
                        this.rows[i].ForEach(el =>
                        {
                            el.Measure(new Size(el.DesiredSize.Width * horizontalShrinkFactor, availableSize.Height));
                        });
                    }
                }
            }
            else
            {
                // 纵向布局（Left/Right）
                totalDesiredWidth = this.rows.Sum(row => row.Max(item =>
                {
                    item.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                    return item.DesiredSize.Width;
                }));

                if (!double.IsPositiveInfinity(availableSize.Width))
                {
                    totalDesiredWidth = Math.Min(totalDesiredWidth, availableSize.Width);
                }

                var rowHeights = rows.Select(row => row.Sum(item =>
                {
                    item.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                    return item.DesiredSize.Height;
                })).ToList();

                totalDesiredHeight = rowHeights.Count > 0 ? rowHeights.Max() : 0.0;

                for (int i = 0; i < rows.Count; i++)
                {
                    if (rowHeights[i] > availableSize.Height)
                    {
                        double verticalShrinkFactor = availableSize.Height / rowHeights[i];
                        rows[i].ForEach(el =>
                        {
                            el.Measure(new Size(availableSize.Width, el.DesiredSize.Height * verticalShrinkFactor));
                        });
                    }
                }
            }
        }

        this.rows = null;
        return new Size(Math.Min(availableSize.Width, totalDesiredWidth), Math.Min(availableSize.Height, totalDesiredHeight));
    }

    /// <summary>
    /// 在指定区域内排列子元素并应用布局逻辑.
    /// 包括处理选定行重排、对齐方式以及两端对齐（Justify）的拉伸计算.
    /// </summary>
    /// <param name="finalSize">元素应在其中进行排列的最终区域尺寸.</param>
    /// <returns>排列后的实际尺寸.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        this.Initialize();
        this.RearrangeForSelectedItem();

        double currentX = 0.0;
        double currentY = 0.0;
        double itemWidth = 0.0;
        double itemHeight = 0.0;
        double accumulatedX = 0.0;
        double accumulatedY = 0.0;

        if (this.Children.Count == 0)
        {
            return base.ArrangeOverride(finalSize);
        }

        if (this.rows != null)
        {
            // 横向布局安排
            if (this.TabStripPlacement == Dock.Top || this.TabStripPlacement == Dock.Bottom)
            {
                double totalRowsDesiredHeight = this.rows.Sum(row => row.Max(item => item.DesiredSize.Height));
                double heightScaleFactor = finalSize.Height / totalRowsDesiredHeight;

                foreach (List<UIElement> row in this.rows)
                {
                    double rowDesiredWidth = row.Sum(item => item.DesiredSize.Width);
                    currentX = 0.0;

                    // 根据对齐方式计算初始偏移
                    if (rowDesiredWidth < finalSize.Width)
                    {
                        if (this.Align == TabStripAlign.Right)
                        {
                            currentX = finalSize.Width - rowDesiredWidth;
                        }
                        else if (this.Align == TabStripAlign.Center)
                        {
                            currentX = (finalSize.Width - rowDesiredWidth) / 2.0;
                        }
                    }

                    // 计算宽度缩放比例（用于两端对齐或超出边界时）
                    double widthScaleFactor = 1.0;
                    if (rowDesiredWidth > finalSize.Width || this.Align == TabStripAlign.Justify)
                    {
                        widthScaleFactor = finalSize.Width / rowDesiredWidth;
                    }

                    double maxRowHeight = row.Max(o => o.DesiredSize.Height) * heightScaleFactor;

                    foreach (UIElement item in row)
                    {
                        if (AllTabsEqualHeight)
                        {
                            currentY = accumulatedY;
                            itemHeight = maxRowHeight;
                        }
                        else
                        {
                            itemHeight = item.DesiredSize.Height * heightScaleFactor;

                            // 如果是顶部停靠，则底部对齐行内元素；否则顶部对齐
                            currentY = (TabStripPlacement == Dock.Top) ? (accumulatedY + maxRowHeight - itemHeight) : accumulatedY;
                        }

                        itemWidth = item.DesiredSize.Width * widthScaleFactor;

                        // 防止无效数值
                        itemHeight = double.IsNaN(itemHeight) ? 0.0 : itemHeight;
                        itemWidth = double.IsNaN(itemWidth) ? 0.0 : itemWidth;

                        item.Arrange(new Rect(currentX, currentY, itemWidth, itemHeight));
                        currentX += itemWidth;
                    }

                    accumulatedY += maxRowHeight;
                }
            }
            else
            {
                // 纵向布局安排
                double totalColumnsDesiredWidth = rows.Sum(row => row.Max(item => item.DesiredSize.Width));
                double widthScaleFactor = finalSize.Width / totalColumnsDesiredWidth;

                if (double.IsNaN(widthScaleFactor) || double.IsInfinity(widthScaleFactor))
                {
                    widthScaleFactor = 1.0;
                }

                foreach (List<UIElement> row in rows)
                {
                    double columnDesiredHeight = row.Sum(item => item.DesiredSize.Height);
                    currentY = 0.0;

                    if (columnDesiredHeight < finalSize.Height)
                    {
                        if (Align == TabStripAlign.Right)
                        {
                            currentY = finalSize.Height - columnDesiredHeight;
                        }
                        else if (Align == TabStripAlign.Center)
                        {
                            currentY = (finalSize.Height - columnDesiredHeight) / 2.0;
                        }
                    }

                    double heightScaleFactor = 1.0;
                    if (columnDesiredHeight > finalSize.Height || Align == TabStripAlign.Justify)
                    {
                        heightScaleFactor = finalSize.Height / columnDesiredHeight;
                    }

                    double maxColumnWidth = row.Max(o => o.DesiredSize.Width) * widthScaleFactor;

                    foreach (UIElement item in row)
                    {
                        if (AllTabsEqualHeight)
                        {
                            currentX = accumulatedX;
                            itemWidth = maxColumnWidth;
                        }
                        else
                        {
                            itemWidth = item.DesiredSize.Width * widthScaleFactor;
                            currentX = (this.TabStripPlacement == Dock.Left) ? (accumulatedX + maxColumnWidth - itemWidth) : accumulatedX;
                        }

                        itemHeight = item.DesiredSize.Height * heightScaleFactor;
                        item.Arrange(new Rect(currentX, currentY, itemWidth, itemHeight));
                        currentY += itemHeight;
                    }

                    accumulatedX += maxColumnWidth;
                }
            }
        }

        this.rows = null;
        return finalSize;
    }

    /// <summary>
    /// 当面板的相关依赖属性发生更改时触发的回调函数.
    /// 用于强制重新测量和排列布局.
    /// </summary>
    /// <param name="sender">发生更改的属性所有者.</param>
    /// <param name="e">包含属性更改信息的事件参数.</param>
    private static void OnPanelPropertyChnaged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var tabStripPanel = sender as TabStripPanel;
        if (tabStripPanel != null)
        {
            tabStripPanel.InvalidateMeasure();
            tabStripPanel.InvalidateArrange();
        }
    }
}