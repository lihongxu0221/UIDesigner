namespace BgControls.Windows.Controls;

/// <summary>
/// 支持自动换行的选项卡布局面板.
/// 当选项卡项的总宽度超过可用空间时，该面板会自动将剩余项移至新行.
/// </summary>
public class TabWrapPanel : TabStripPanel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TabWrapPanel"/> class.
    /// </summary>
    public TabWrapPanel()
    {
        this.SizeChanged += this.OnSizeChanged;
    }

    /// <summary>
    /// 计算子元素所需的测量尺寸，并执行换行逻辑计算.
    /// </summary>
    /// <param name="availableSize">父元素可提供的最大可用空间.</param>
    /// <returns>面板测量后的期望尺寸.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        try
        {
            double totalWidth = 0.0;
            double totalHeight = 0.0;
            double maxRowWidth = availableSize.Width;

            if (this.Children.Count == 0)
            {
                return new Size(totalWidth, totalHeight);
            }

            // 初始化行逻辑并处理旋转情况下的顺序
            this.Initialize();
            this.ReverseItemOrderForRotaion();

            foreach (UIElement child in this.Children)
            {
                child.Measure(availableSize);
            }

            // 执行自动换行算法：将现有的行进一步切分为符合宽度的多行
            this.Rows = this.Rows?.SelectMany<List<UIElement>, List<UIElement>>(row =>
            {
                if (double.IsInfinity(maxRowWidth))
                {
                    return new List<UIElement>[] { row };
                }

                double remainingSpace = maxRowWidth;
                IEnumerable<UIElement> remainingElements = row;
                List<List<UIElement>> wrappedRows = new List<List<UIElement>>();

                while (remainingElements.Any())
                {
                    // 获取当前行能容纳的元素集合
                    var currentRowElements = remainingElements.TakeWhile(tab =>
                    {
                        remainingSpace -= tab.DesiredSize.Width;
                        return remainingSpace > 0.0;
                    }).ToList();

                    // 如果一个都放不下，则强行放入第一个元素以防死循环
                    if (currentRowElements.Count == 0)
                    {
                        currentRowElements = remainingElements.Take(1).ToList();
                    }

                    remainingElements = remainingElements.Skip(currentRowElements.Count);
                    remainingSpace = maxRowWidth;
                    wrappedRows.Add(currentRowElements);
                }

                return wrappedRows;
            }).ToList();

            // 计算所有行的高度总和
            totalHeight = this.Rows?.Sum(row => row.Max(item =>
            {
                // 再次测量以确保布局更新
                item.Measure(new Size(availableSize.Width, availableSize.Height));
                return item.DesiredSize.Height;
            })) ?? 0.00;

            if (!double.IsPositiveInfinity(availableSize.Height))
            {
                totalHeight = Math.Min(totalHeight, availableSize.Height);
            }

            // 计算所有行宽度的最大值
            var rowWidths = this.Rows?.Select(row => row.Sum(item => item.DesiredSize.Width));
            totalWidth = rowWidths?.Any() == true ? rowWidths.Max() : 0.0;

            return new Size(totalWidth, totalHeight);
        }
        catch
        {
            return base.MeasureOverride(availableSize);
        }
    }

    /// <summary>
    /// 在指定区域内排列子元素，并根据旋转逻辑和对齐方式调整位置.
    /// </summary>
    /// <param name="finalSize">元素应在其中进行排列的最终区域尺寸.</param>
    /// <returns>排列后的实际尺寸.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        try
        {
            double currentX = 0.0;
            double currentY = 0.0;
            double itemWidth = 0.0;
            double itemHeight = 0.0;
            double accumulatedY = 0.0;

            if (this.Children.Count == 0)
            {
                return base.ArrangeOverride(finalSize);
            }

            if (this.Rows == null || this.Rows.Count == 0)
            {
                this.InvalidateMeasure();
                return finalSize;
            }

            // 处理选定项行置顶/重排逻辑
            this.RearrangeForSelectedItem();

            // 计算缩放比例
            double totalRowsDesiredHeight = this.Rows.Sum(row => row.Max(item => item.DesiredSize.Height));
            double heightScaleFactor = finalSize.Height / totalRowsDesiredHeight;

            // 计算旋转后的实际对齐方式
            TabStripAlign effectiveAlign = this.CalculateAlignForRotation();

            foreach (var row in this.Rows)
            {
                double rowDesiredWidth = row.Sum(item => item.DesiredSize.Width);
                currentX = 0.0;

                // 计算水平偏移量
                if (rowDesiredWidth < finalSize.Width)
                {
                    switch (effectiveAlign)
                    {
                        case TabStripAlign.Right:
                            currentX = finalSize.Width - rowDesiredWidth;
                            break;
                        case TabStripAlign.Center:
                            currentX = (finalSize.Width - rowDesiredWidth) / 2.0;
                            break;
                    }
                }

                // 计算宽度缩放比例（用于两端对齐或超出边界）
                double widthScaleFactor = 1.0;
                if (rowDesiredWidth > finalSize.Width || effectiveAlign == TabStripAlign.Justify)
                {
                    widthScaleFactor = finalSize.Width / rowDesiredWidth;
                }

                double maxRowHeight = row.Max(o => o.DesiredSize.Height) * heightScaleFactor;

                foreach (UIElement item in row)
                {
                    if (this.AllTabsEqualHeight)
                    {
                        currentY = accumulatedY;
                        itemHeight = maxRowHeight;
                    }
                    else
                    {
                        itemHeight = item.DesiredSize.Height * heightScaleFactor;

                        // 顶部停靠时元素底部对齐，否则顶部对齐
                        currentY = (this.TabStripPlacement == Dock.Top) ? (accumulatedY + maxRowHeight - itemHeight) : accumulatedY;
                    }

                    itemWidth = item.DesiredSize.Width * widthScaleFactor;

                    // 确保数值有效
                    itemHeight = double.IsNaN(itemHeight) ? 0.0 : itemHeight;
                    itemWidth = double.IsNaN(itemWidth) ? 0.0 : itemWidth;

                    item.Arrange(new Rect(currentX, currentY, itemWidth, itemHeight));
                    currentX += itemWidth;
                }

                accumulatedY += maxRowHeight;
            }

            // 重置行集合以备下次布局
            this.Rows = new List<List<UIElement>>();
            return finalSize;
        }
        finally
        {
            this.Rows = new List<List<UIElement>>();
        }
    }

    /// <summary>
    /// 处理被移除出视觉树时的情况.
    /// </summary>
    /// <param name="visualAdded">待添加的可视元素.</param>
    /// <param name="visualRemoved">待移除的可视元素.</param>
    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);

        // 当有子元素被移除时，确保清除过时的行缓存引用
        if (visualRemoved != null)
        {
            this.Rows = new List<List<UIElement>>();
        }
    }

    /// <summary>
    /// 处理面板尺寸更改事件，强制重新触发测量流程.
    /// </summary>
    /// <param name="sender">事件源.</param>
    /// <param name="e">尺寸更改参数.</param>
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        this.InvalidateMeasure();
    }

    /// <summary>
    /// 计算由于布局旋转（例如底部停靠）所需的对齐方式修正.
    /// </summary>
    /// <returns>修正后的对齐方式.</returns>
    private TabStripAlign CalculateAlignForRotation()
    {
        TabStripAlign tabStripAlign = this.Align;
        if (this.TabStripPlacement == Dock.Bottom)
        {
            // 底部停靠时，左右对齐方向需要反转
            switch (tabStripAlign)
            {
                case TabStripAlign.Left:
                    tabStripAlign = TabStripAlign.Right;
                    break;
                case TabStripAlign.Right:
                    tabStripAlign = TabStripAlign.Left;
                    break;
            }
        }

        return tabStripAlign;
    }

    /// <summary>
    /// 在特定的停靠模式下（底部或左侧）反转行内元素的顺序，以适应渲染旋转.
    /// </summary>
    private void ReverseItemOrderForRotaion()
    {
        if (this.TabStripPlacement != Dock.Bottom && this.TabStripPlacement != Dock.Left)
        {
            return;
        }

        if (this.Rows != null)
        {
            foreach (var row in this.Rows)
            {
                row.Reverse();
            }
        }
    }
}