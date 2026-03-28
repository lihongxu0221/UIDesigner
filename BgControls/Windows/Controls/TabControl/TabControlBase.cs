using BgControls.Tools.Helpers;
using BgControls.Windows.DragDrop;

namespace BgControls.Windows.Controls;

/// <summary>
/// 选项卡控件的基类，继承自 <see cref="MultiSelector"/>.
/// 提供选项卡选择、内容呈现、内容保留以及布局管理的核心逻辑.
/// </summary>
public abstract class TabControlBase : MultiSelector
{
    /// <summary>
    /// 内部类，用于存储和管理选项卡控件的状态标识.
    /// </summary>
    protected class TabControlState
    {
        private bool isMousePressed;

        /// <summary>
        /// Gets or sets a value indicating whether 在拖拽过程中是否抑制选择通知.
        /// </summary>
        internal bool SupressSelectionNotificationsWhileDragging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 当前是否正在执行选择操作.
        /// </summary>
        internal bool IsSelectionInProgress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 控件是否已加载.
        /// </summary>
        internal bool IsLoaded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 选项卡条（TabStrip）是否已更新.
        /// </summary>
        internal bool IsTabStripUpdated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 鼠标是否处于按下状态.
        /// </summary>
        internal bool IsMousePressed
        {
            get => this.isMousePressed;
            set => this.isMousePressed = value;
        }
    }

    /// <summary>
    /// 标识 <see cref="BackgroundVisibility"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty BackgroundVisibilityProperty =
        DependencyProperty.Register("BackgroundVisibility", typeof(Visibility), typeof(TabControlBase), null);

    /// <summary>
    /// 标识 <see cref="HeaderBackground"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty HeaderBackgroundProperty =
        DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(TabControlBase), new PropertyMetadata(Brushes.Transparent, null));

    /// <summary>
    /// 标识 <see cref="ReorderTabRows"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty ReorderTabRowsProperty =
        DependencyProperty.Register("ReorderTabRows", typeof(bool), typeof(TabControlBase), new PropertyMetadata(true, OnReorderTabRowsPropertyChanged));

    /// <summary>
    /// 标识 <see cref="SelectedContent"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty SelectedContentProperty =
        DependencyProperty.Register("SelectedContent", typeof(object), typeof(TabControlBase), null);

    /// <summary>
    /// 标识 <see cref="SelectedContentTemplate"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty SelectedContentTemplateProperty =
        DependencyProperty.Register("SelectedContentTemplate", typeof(DataTemplate), typeof(TabControlBase), null);

    /// <summary>
    /// 标识 <see cref="SelectedContentTemplateSelector"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty SelectedContentTemplateSelectorProperty =
        DependencyProperty.Register("SelectedContentTemplateSelector", typeof(DataTemplateSelector), typeof(TabControlBase), null);

    /// <summary>
    /// 标识 <see cref="ContentTemplate"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(TabControlBase), null);

    /// <summary>
    /// 标识 <see cref="ContentTemplateSelector"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty ContentTemplateSelectorProperty =
        DependencyProperty.Register("ContentTemplateSelector", typeof(DataTemplateSelector), typeof(TabControlBase), null);

    /// <summary>
    /// 标识 <see cref="IsContentPreserved"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsContentPreservedProperty =
        DependencyProperty.Register("IsContentPreserved", typeof(bool), typeof(TabControlBase), new PropertyMetadata(false, OnIsContentPreservedPropertyChanged));

    /// <summary>
    /// 标识 <see cref="PropagateItemDataContextToContent"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty PropagateItemDataContextToContentProperty =
        DependencyProperty.Register("PropagateItemDataContextToContent", typeof(bool), typeof(TabControlBase), new PropertyMetadata(true));

    /// <summary>
    /// 标识 <see cref="SelectedItemRemoveBehaviour"/> 依赖属性.
    /// </summary>
    public static readonly DependencyProperty SelectedItemRemoveBehaviourProperty =
        DependencyProperty.Register("SelectedItemRemoveBehaviour", typeof(SelectedItemRemoveBehaviour), typeof(TabControlBase), new PropertyMetadata(SelectedItemRemoveBehaviour.SelectNext));

    /// <summary>
    /// 标识 PreviewSelectionChanged 路由事件.
    /// </summary>
    public static readonly RoutedEvent PreviewSelectionChangedEvent =
        EventManager.RegisterRoutedEvent("PreviewSelectionChanged", RoutingStrategy.Tunnel, typeof(SelectionChangedEventHandler), typeof(TabControlBase));

    /// <summary>
    /// 标识 SelectionChanged 路由事件，隐藏基类事件以支持特定的处理程序类型.
    /// </summary>
    public new static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(TabControlBase));

    /// <summary>
    /// 标识 DropDownOpened 路由事件.
    /// </summary>
    public static readonly RoutedEvent DropDownOpenedEvent =
        EventManager.RegisterRoutedEvent("DropDownOpened", RoutingStrategy.Bubble, typeof(DropDownEventHandler), typeof(TabControlBase));

    /// <summary>
    /// 标识 DropDownClosed 路由事件.
    /// </summary>
    public static readonly RoutedEvent DropDownClosedEvent =
        EventManager.RegisterRoutedEvent("DropDownClosed", RoutingStrategy.Bubble, typeof(DropDownEventHandler), typeof(TabControlBase));

    /// <summary>
    /// Initializes a new instance of the <see cref="TabControlBase"/> class.
    /// </summary>
    protected TabControlBase()
    {
        this.InitializeFields();
        this.Loaded += RadTabControl_Loaded;
        this.SelectionChanged += OnSelectionChanged;
    }

    /// <summary>
    /// Gets or sets 背景的可见性.
    /// </summary>
    [Category("AppearanceCategory")]
    [Description("TabControlBackgroundOpacityDescription")]
    [DefaultValue(Visibility.Visible)]
    public virtual Visibility BackgroundVisibility
    {
        get => (Visibility)GetValue(BackgroundVisibilityProperty);
        set => SetValue(BackgroundVisibilityProperty, value);
    }

    /// <summary>
    /// Gets 选定项集合. 在该控件中此属性不被使用并返回 null.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new IList? SelectedItems => null;

    /// <summary>
    /// Gets or sets 选项卡标头区域的背景画刷.
    /// </summary>
    [Category("AppearanceCategory")]
    public virtual Brush HeaderBackground
    {
        get => (Brush)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否根据选定项重新排列选项卡行.
    /// </summary>
    [DefaultValue(true)]
    [Description("TabControlReorderTabRowsDescription")]
    [Category("BehaviourCategory")]
    public virtual bool ReorderTabRows
    {
        get => (bool)GetValue(ReorderTabRowsProperty);
        set => SetValue(ReorderTabRowsProperty, value);
    }

    /// <summary>
    /// Gets or sets 当前选定项的内容.
    /// </summary>
    [DefaultValue(null)]
    [Browsable(false)]
    public virtual object SelectedContent
    {
        get => GetValue(SelectedContentProperty);
        set => SetValue(SelectedContentProperty, value);
    }

    /// <summary>
    /// Gets or sets 用于显示选定项内容的 <see cref="DataTemplate"/>.
    /// </summary>
    [DefaultValue(null)]
    [Browsable(false)]
    public virtual DataTemplate? SelectedContentTemplate
    {
        get => (DataTemplate?)GetValue(SelectedContentTemplateProperty);
        set => SetValue(SelectedContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets 用于选择选定项内容模板的选择器.
    /// </summary>
    [Browsable(false)]
    [DefaultValue(null)]
    [Category("AppearanceCategory")]
    public virtual DataTemplateSelector? SelectedContentTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(SelectedContentTemplateSelectorProperty);
        set => SetValue(SelectedContentTemplateSelectorProperty, value);
    }

    /// <summary>
    /// Gets or sets 用于所有项内容的默认 <see cref="DataTemplate"/>.
    /// </summary>
    [Category("AppearanceCategory")]
    public virtual DataTemplate ContentTemplate
    {
        get => (DataTemplate)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets 用于所有项内容的默认模板选择器.
    /// </summary>
    [Category("AppearanceCategory")]
    public virtual DataTemplateSelector ContentTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty);
        set => SetValue(ContentTemplateSelectorProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否保留选项卡的内容视觉状态（不重新生成内容）.
    /// </summary>
    public virtual bool IsContentPreserved
    {
        get => (bool)GetValue(IsContentPreservedProperty);
        set => SetValue(IsContentPreservedProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否将项的数据上下文（DataContext）传播到内容区域.
    /// </summary>
    public virtual bool PropagateItemDataContextToContent
    {
        get => (bool)GetValue(PropagateItemDataContextToContentProperty);
        set => SetValue(PropagateItemDataContextToContentProperty, value);
    }

    /// <summary>
    /// Gets or sets 当选定项被移除时的处理行为.
    /// </summary>
    public SelectedItemRemoveBehaviour SelectedItemRemoveBehaviour
    {
        get => (SelectedItemRemoveBehaviour)GetValue(SelectedItemRemoveBehaviourProperty);
        set => SetValue(SelectedItemRemoveBehaviourProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否抑制重新应用选定内容模板的操作.
    /// </summary>
    public virtual bool SupressSelectedContentTemplateReapplying { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 默认项是否已被选中.
    /// </summary>
    public bool IsDefaultItemSelected { get; set; }

    /// <summary>
    /// Gets 下拉菜单显示的项集合.
    /// </summary>
    internal ObservableCollection<object> DropDownItems { get; private set; }

    /// <summary>
    /// Gets 项与内容呈现器（ContentPresenter）之间的映射字典，用于内容保留模式.
    /// </summary>
    internal Dictionary<object, ContentPresenter> ItemToContentMap { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether 当前的选定操作是否由用户发起.
    /// </summary>
    internal bool IsUserInitiatedSelection { get; set; }

    /// <summary>
    /// Gets or sets 承载选项卡头的面板（TabStrip）.
    /// </summary>
    protected internal Panel? TabStrip { get; protected set; }

    /// <summary>
    /// Gets or sets 当前显示内容的呈现器实例.
    /// </summary>
    protected internal ContentPresenter? ContentElement { get; set; }

    /// <summary>
    /// Gets or sets 在内容保留模式下用于承载所有内容呈现器的容器面板（Grid）.
    /// </summary>
    protected internal Grid? ContentElementsPanel { get; set; }

    /// <summary>
    /// Gets 控件的状态标识对象.
    /// </summary>
    protected TabControlState TabFlags { get; private set; }

    /// <summary>
    /// 根据索引获取对应的容器项（ITabItem）.
    /// </summary>
    /// <param name="index">索引位置.</param>
    /// <returns>容器项.</returns>
    public virtual ITabItem? this[int index] => this.ContainerFromIndex(index);

    /// <summary>
    /// 当选定项即将更改时发生.
    /// </summary>
    public event SelectionChangedEventHandler PreviewSelectionChanged
    {
        add { AddHandler(PreviewSelectionChangedEvent, value); }
        remove { RemoveHandler(PreviewSelectionChangedEvent, value); }
    }

    /// <summary>
    /// 当选定项已更改时发生.
    /// </summary>
    [DefaultValue("TabControlSelectionChangedDescription")]
    public new event SelectionChangedEventHandler SelectionChanged
    {
        add { AddHandler(SelectionChangedEvent, value); }
        remove { RemoveHandler(SelectionChangedEvent, value); }
    }

    /// <summary>
    /// 当下拉菜单打开时发生.
    /// </summary>
    public virtual event DropDownEventHandler DropDownOpened
    {
        add { AddHandler(DropDownOpenedEvent, value); }
        remove { RemoveHandler(DropDownOpenedEvent, value); }
    }

    /// <summary>
    /// 当下拉菜单关闭时发生.
    /// </summary>
    public virtual event DropDownEventHandler DropDownClosed
    {
        add { AddHandler(DropDownClosedEvent, value); }
        remove { RemoveHandler(DropDownClosedEvent, value); }
    }

    /// <summary>
    /// 根据屏幕绝对坐标查找对应的重排目标项.
    /// </summary>
    /// <param name="absolutePoint">相对于窗口的绝对点坐标.</param>
    /// <returns>找到的项元素；如果未找到则返回 null.</returns>
    private FrameworkElement? FindReoderDestinationItem(Point absolutePoint)
    {
        if (TabStrip == null)
        {
            return null;
        }

        try
        {
            Point point = Window.GetWindow(this).TransformToVisual(TabStrip).Transform(absolutePoint);
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (this.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement frameworkElement &&
                    LayoutInformation.GetLayoutSlot(frameworkElement).Contains(point))
                {
                    return frameworkElement;
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// 执行由用户发起的选定操作.
    /// </summary>
    /// <param name="selectedIndex">目标索引.</param>
    internal void UserInitiatedSelection(int selectedIndex)
    {
        this.IsUserInitiatedSelection = true;
        this.SelectedIndex = selectedIndex;
        this.IsUserInitiatedSelection = false;
    }

    /// <summary>
    /// 计算并获取控件的最终可见性.
    /// </summary>
    /// <returns>可见性枚举值.</returns>
    internal virtual Visibility GetComputedVisibility()
    {
        return Visibility.Collapsed;
    }

    /// <summary>
    /// 将选定项带入可视区域.
    /// </summary>
    internal virtual void BringSelectedItemIntoView()
    {
    }

    /// <summary>
    /// 更新控件的视觉状态.
    /// </summary>
    /// <param name="useTransitions">是否使用过渡动画.</param>
    protected internal virtual void ChangeVisualState(bool useTransitions)
    {
    }

    /// <summary>
    /// 当选定项更改后更新焦点位置.
    /// </summary>
    protected internal virtual void UpdateFocusOnSelectionChange()
    {
        if (!IsUserInitiatedSelection)
        {
            return;
        }

        var selectedItemContainer = this.GetSelectedContainer();
        if (selectedItemContainer == null)
        {
            return;
        }

        var dependencyObject = this.GetFocusedElement() as DependencyObject;
        var currentContentElement = GetCurrentContentElement();
        bool shouldFocusContent = dependencyObject == null ||
                                  (currentContentElement != null && currentContentElement.IsKeyboardFocusWithin) ||
                                  !this.IsKeyboardFocusWithin;

        this.Dispatcher.BeginInvoke(
            () =>
            {
                if (shouldFocusContent)
                {
                    if (currentContentElement != null)
                    {
                        currentContentElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                    else
                    {
                        (SelectedContent as FrameworkElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                }
                else
                {
                    selectedItemContainer.Control.Focus();
                }
            },
            DispatcherPriority.ApplicationIdle);
    }

    /// <summary>
    /// 获取当前选定项对应的容器对象（ITabItem）.
    /// </summary>
    /// <returns>选定项容器.</returns>
    internal ITabItem? GetSelectedContainer()
    {
        ITabItem? radTabItem = null;
        if (this.ItemContainerGenerator != null)
        {
            radTabItem = this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem) as ITabItem;
        }

        if (radTabItem == null)
        {
            radTabItem = this.SelectedItem as ITabItem;
        }

        return radTabItem;
    }

    /// <summary>
    /// 根据索引获取对应的容器项.
    /// </summary>
    /// <param name="index">索引.</param>
    /// <returns>容器项.</returns>
    internal ITabItem? ContainerFromIndex(int index)
    {
        if (this.ItemContainerGenerator != null)
        {
            return this.ItemContainerGenerator.ContainerFromIndex(index) as ITabItem;
        }

        return null;
    }

    /// <summary>
    /// 当子项内容发生更改时通知父控件，触发选定内容属性及显示的更新.
    /// </summary>
    internal void NotifyChildContentChanged()
    {
        this.UpdateSelectedContentProperties();
        this.UpdateContentSafely();
    }

    /// <summary>
    /// 触发选定项更改预览事件，并判断事件是否已被处理.
    /// </summary>
    /// <param name="removedItems">被移除的项.</param>
    /// <param name="addedItems">被添加的项.</param>
    /// <returns>如果事件已被处理（Handled 为 true）则返回 true.</returns>
    internal bool IsPreviewSelectionChangedEventHandeled(IList removedItems, IList addedItems)
    {
        if (this.TabFlags.SupressSelectionNotificationsWhileDragging)
        {
            return false;
        }

        var e = new SelectionChangedEventArgs(PreviewSelectionChangedEvent, this, removedItems, addedItems);
        RaiseEvent(e);
        return e.Handled;
    }

    /// <summary>
    /// 获取当前选定项的内容呈现器.
    /// </summary>
    /// <returns>内容呈现器实例.</returns>
    internal ContentPresenter? GetCurrentContentElement()
    {
        ContentPresenter? result = null;
        if (IsContentPreserved)
        {
            if (this.SelectedItem != null &&
                this.ItemToContentMap.TryGetValue(this.SelectedItem, out ContentPresenter? contentPresenter) &&
                contentPresenter != null)
            {
                if (this.ContentElementsPanel != null && this.ContentElementsPanel.Children.Contains(contentPresenter))
                {
                    result = contentPresenter;
                }
            }
        }
        else
        {
            result = this.ContentElement;
        }

        return result;
    }

    /// <summary>
    /// 获取指定数据项对应的内容呈现器.
    /// </summary>
    /// <param name="item">数据项.</param>
    /// <returns>内容呈现器.</returns>
    internal ContentPresenter? GetContentElementForItem(object item)
    {
        ContentPresenter? result = null;
        if (item != null)
        {
            if (this.IsContentPreserved)
            {
                if (this.ItemToContentMap.TryGetValue(item, out ContentPresenter? contentPresenter) &&
                    contentPresenter != null)
                {
                    if (this.ContentElementsPanel != null && this.ContentElementsPanel.Children.Contains(contentPresenter))
                    {
                        result = contentPresenter;
                    }
                }
            }
            else
            {
                result = this.ContentElement;
            }
        }

        return result;
    }

    /// <summary>
    /// 重新安排选项卡布局.
    /// </summary>
    internal void RearrangeTabs()
    {
        if (this.TabStrip != null)
        {
            this.TabStrip.InvalidateMeasure();
        }
    }

    /// <summary>
    /// 当选定操作执行后，更新焦点是否应移动到内容区域.
    /// </summary>
    /// <param name="shouldFocusContent">是否应将焦点移动到内容.</param>
    internal virtual void UpdateFocusOnSelectionChange(bool shouldFocusContent)
    {
        if (!IsUserInitiatedSelection)
        {
            return;
        }

        var currentContentElement = this.GetCurrentContentElement();
        var selectedContainer = this.GetSelectedContainer();
        if (currentContentElement == null || selectedContainer == null)
        {
            return;
        }

        this.Dispatcher.BeginInvoke(
            () =>
            {
                if (shouldFocusContent)
                {
                    var control = currentContentElement.ChildrenOfType<Control>()?.FirstOrDefault(c => c.IsTabStop);
                    if (control != null)
                    {
                        control.Focus();
                    }
                    else if (IsUserInitiatedSelection)
                    {
                        selectedContainer.Control.Focus();
                    }
                }
                else
                {
                    selectedContainer.Control.Focus();
                }
            },
            DispatcherPriority.ApplicationIdle);
    }

    /// <summary>
    /// 以安全的方式更新内容区域的显示状态.
    /// 处理内容保留逻辑、模板重新应用以及隐藏非选定项的内容.
    /// </summary>
    internal void UpdateContentSafely()
    {
        var contentPresenter = this.GetCurrentContentElement();
        if (this.IsContentPreserved)
        {
            if (ContentElementsPanel == null)
            {
                return;
            }

            if (this.SelectedItem == null)
            {
                foreach (ContentPresenter item in this.ContentElementsPanel.Children.OfType<ContentPresenter>())
                {
                    item.Visibility = Visibility.Collapsed;
                }

                return;
            }

            if (contentPresenter == null)
            {
                contentPresenter = new ContentPresenter();
                this.ItemToContentMap[this.SelectedItem] = contentPresenter;
                this.ContentElementsPanel.Children.Add(contentPresenter);
            }

            this.FillContentInternal(contentPresenter);
            if (contentPresenter.ContentTemplate != this.SelectedContentTemplate)
            {
                contentPresenter.ContentTemplate = this.SelectedContentTemplate;
            }

            if (contentPresenter.ContentTemplateSelector != this.SelectedContentTemplateSelector)
            {
                contentPresenter.ContentTemplateSelector = this.SelectedContentTemplateSelector;
            }

            foreach (ContentPresenter item2 in this.ContentElementsPanel.Children.OfType<ContentPresenter>())
            {
                if (item2 != contentPresenter)
                {
                    item2.Visibility = Visibility.Collapsed;
                }
            }

            contentPresenter.Visibility = Visibility.Visible;
        }
        else if (contentPresenter != null)
        {
            this.FillContentInternal(contentPresenter);
        }
    }

    /// <summary>
    /// 处理内部 SelectionChanged 事件，并同步更新容器生成和内容显示.
    /// </summary>
    /// <param name="sender">事件源.</param>
    /// <param name="e">选择更改参数.</param>
    protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e == null || e.OriginalSource != this)
        {
            return;
        }

        var selectedContainer = this.GetSelectedContainer();
        if (selectedContainer == null && this.SelectedIndex > -1)
        {
            this.DelaySelectionAfterContainersGeneration(() =>
            {
                this.HandleSelectionChanged(e.RemovedItems, e.AddedItems);
            });
        }
        else
        {
            this.HandleSelectionChanged(e.RemovedItems, e.AddedItems);
        }

        e.Handled = true;
    }

    /// <summary>
    /// 根据配置更新选项卡行的布局.
    /// </summary>
    protected virtual void UpdateTabRows()
    {
        if (this.ReorderTabRows)
        {
            this.RearrangeTabs();
        }
    }

    /// <summary>
    /// 更新与选定内容相关的属性.
    /// </summary>
    protected virtual void UpdateSelectedContentProperties()
    {
    }

    /// <summary>
    /// 获取项容器（在此基类中需由派生类具体实现）.
    /// </summary>
    /// <returns>项容器.</returns>
    protected virtual ITabItem? GetContainer()
    {
        return null;
    }

    /// <summary>
    /// 切换控件到指定的视觉状态.
    /// </summary>
    /// <param name="useTransitions">是否使用平滑过渡动画.</param>
    /// <param name="stateNames">状态名称数组，将按顺序尝试进入第一个可用的状态.</param>
    protected void GoToState(bool useTransitions, params string[] stateNames)
    {
        if (stateNames == null)
        {
            return;
        }

        foreach (string stateName in stateNames)
        {
            if (VisualStateManager.GoToState(this, stateName, useTransitions))
            {
                break;
            }
        }
    }

    /// <summary>
    /// 引发 SelectionChanged 事件.
    /// </summary>
    /// <param name="e">事件参数.</param>
    protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        RaiseEvent(e);
    }

    /// <summary>
    /// 引发 PreviewSelectionChanged 事件.
    /// </summary>
    /// <param name="e">事件参数.</param>
    protected virtual void OnPreviewSelectionChanged(SelectionChangedEventArgs e)
    {
        RaiseEvent(e);
    }

    /// <summary>
    /// 为指定的项创建一个容器元素.
    /// </summary>
    /// <returns>新容器元素的依赖对象标识符.</returns>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return this.GetContainer()?.Control;
    }

    /// <summary>
    /// 确定指定项是否是其自己的容器.
    /// </summary>
    /// <param name="item">要检查的项.</param>
    /// <returns>如果项是容器则返回 true.</returns>
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ITabItem;
    }

    /// <summary>
    /// 确定选择属性（SelectedIndex 或 SelectedItem）在客户端是否未进行数据绑定.
    /// </summary>
    /// <returns>如果未绑定则返回 true.</returns>
    protected virtual bool IsSelectionNonBoundAtClientSide()
    {
        if (GetBindingExpression(Selector.SelectedIndexProperty) == null)
        {
            return GetBindingExpression(Selector.SelectedItemProperty) == null;
        }

        return false;
    }

    /// <summary>
    /// 清理内容区域的引用及视觉子元素，以防内存泄漏.
    /// </summary>
    protected void ClearContentSafely()
    {
        if (this.ContentElementsPanel != null)
        {
            foreach (ContentPresenter item in this.ContentElementsPanel.Children.OfType<ContentPresenter>())
            {
                item.ClearValue(ContentPresenter.ContentProperty);
                item.ClearValue(FrameworkElement.DataContextProperty);
            }

            this.ContentElementsPanel.Children.Clear();
            this.ItemToContentMap.Clear();
        }

        if (this.ContentElement != null)
        {
            this.ContentElement.ClearValue(ContentPresenter.ContentProperty);
            this.ContentElement.ClearValue(FrameworkElement.DataContextProperty);
        }
    }

    /// <summary>
    /// 初始化内部字段及其默认值.
    /// </summary>
    private void InitializeFields()
    {
        this.IsDefaultItemSelected = true;
        this.TabFlags = new TabControlState();
        this.ItemToContentMap = new Dictionary<object, ContentPresenter>();
        this.DropDownItems = new ObservableCollection<object>();
        this.IsUserInitiatedSelection = false;
    }

    /// <summary>
    /// 当控件加载完成时触发的回调.
    /// </summary>
    private void RadTabControl_Loaded(object sender, RoutedEventArgs e)
    {
        SetSelectedindexOnStartUp();
    }

    /// <summary>
    /// 在控件启动时设置初始选定索引（如果未绑定选择属性且已启用默认选择）.
    /// </summary>
    private void SetSelectedindexOnStartUp()
    {
        Action action = new Action(() =>
        {
            if (this.SelectedIndex == -1 &&
                this.Items.Count > 0 &&
                this.IsSelectionNonBoundAtClientSide() &&
                this.IsDefaultItemSelected)
            {
                this.SelectedIndex = 0;
            }
        });

        if (this.Items.Count > 0 && ReadLocalValue(ItemsControl.ItemsSourceProperty) != DependencyProperty.UnsetValue)
        {
            this.DelaySelectionAfterContainersGeneration(action);
        }
        else
        {
            action();
        }
    }

    /// <summary>
    /// 延迟执行选择逻辑，直到项容器生成器完成容器生成工作.
    /// </summary>
    /// <param name="actionToDelay">待延迟执行的操作.</param>
    private void DelaySelectionAfterContainersGeneration(Action actionToDelay)
    {
        EventHandler? statusChangedHandler = null;
        statusChangedHandler = new EventHandler((s, e) =>
        {
            if (this.ItemContainerGenerator != null &&
                this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated &&
                (this.GetSelectedContainer() != null || this.SelectedIndex <= -1))
            {
                this.ItemContainerGenerator.StatusChanged -= statusChangedHandler;
                actionToDelay.Invoke();
            }
        });

        if ((this.ItemContainerGenerator != null && this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) ||
            (this.GetSelectedContainer() == null && this.SelectedIndex > -1))
        {
            if (this.ItemContainerGenerator != null)
            {
                this.ItemContainerGenerator.StatusChanged += statusChangedHandler;
            }
        }
        else
        {
            actionToDelay.Invoke();
        }
    }

    /// <summary>
    /// 核心选择更改处理逻辑，负责同步内容区域、处理预览事件及更新焦点.
    /// </summary>
    /// <param name="removedItems">取消选定的项.</param>
    /// <param name="addedItems">新选定的项.</param>
    private void HandleSelectionChanged(IList removedItems, IList addedItems)
    {
        if (this.TabFlags.IsSelectionInProgress)
        {
            return;
        }

        this.TabFlags.IsSelectionInProgress = true;
        if (this.IsPreviewSelectionChangedEventHandeled(removedItems, addedItems))
        {
            this.RevertLastSelectionChanges(removedItems);
            this.TabFlags.IsSelectionInProgress = false;
            return;
        }

        var removedItem = removedItems?.OfType<object>().FirstOrDefault();
        bool shouldFocusContent = false;
        if (removedItem != null)
        {
            shouldFocusContent = this.ShouldMoveFocusToContent(removedItem);
        }

        this.UpdateSelectedContentProperties();
        this.UpdateContentSafely();
        this.EnsureContainer_IsSelectedProperty();
        this.BringSelectedItemIntoView();
        this.UpdateFocusOnSelectionChange(shouldFocusContent);
        this.UpdateTabRows();
        TabFlags.IsSelectionInProgress = false;
        if (!TabFlags.SupressSelectionNotificationsWhileDragging)
        {
            this.OnSelectionChanged(new SelectionChangedEventArgs(SelectionChangedEvent, this, removedItems, addedItems));
        }
    }

    /// <summary>
    /// 判断是否需要将焦点移动到内容区域.
    /// 通常在旧焦点位于被移除项的内容中时执行.
    /// </summary>
    /// <param name="removedItem">被移除的项.</param>
    /// <returns>如果需要移动焦点则返回 true.</returns>
    private bool ShouldMoveFocusToContent(object removedItem)
    {
        bool result = false;
        var selectedContainer = this.GetSelectedContainer();
        if (selectedContainer != null)
        {
            var uIElement = this.GetFocusedElement() as UIElement;
            var contentElementForItem = this.GetContentElementForItem(removedItem);
            result = uIElement == null ||
                     (contentElementForItem != null && contentElementForItem.IsAncestorOf(uIElement)) ||
                     !this.IsAncestorOf(uIElement);
        }

        return result;
    }

    /// <summary>
    /// 撤销上一次的选择更改（通常在预览事件被拦截时调用）.
    /// </summary>
    /// <param name="removedItems">之前选中的项集合.</param>
    private void RevertLastSelectionChanges(IList removedItems)
    {
        if (removedItems != null && removedItems.Count >= 1)
        {
            this.SelectedItem = removedItems[0];
        }
        else
        {
            this.SelectedItem = null;
        }
    }

    /// <summary>
    /// 为内容呈现器填充选定内容及数据上下文.
    /// </summary>
    /// <param name="presenterToFill">目标内容呈现器.</param>
    private void FillContentInternal(ContentPresenter presenterToFill)
    {
        if (presenterToFill == null)
        {
            return;
        }

        if ((this.SelectedContent is not UIElement reference || VisualTreeHelper.GetParent(reference) == null) &&
            presenterToFill.Content != this.SelectedContent)
        {
            presenterToFill.Content = this.SelectedContent;
            var owner = this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem);
            if (owner is BgTabItem tabItem)
            {
                presenterToFill.ContentStringFormat = tabItem.ContentStringFormat;
            }
        }

        if (this.PropagateItemDataContextToContent)
        {
            var obj = this.SelectedItem == null ?
                null :
                (this.SelectedItem is FrameworkElement framework) ?
                    framework.DataContext :
                    !(this.SelectedItem is UIElement) ?
                            this.SelectedItem :
                            null;
            if (presenterToFill.DataContext != obj)
            {
                presenterToFill.DataContext = obj;
            }
        }
    }

    /// <summary>
    /// 确保项容器的 IsSelected 属性与选择状态同步.
    /// </summary>
    private void EnsureContainer_IsSelectedProperty()
    {
        var selectedContainer = GetSelectedContainer();
        if (selectedContainer != null && !selectedContainer.IsSelected)
        {
            selectedContainer.IsSelected = true;
        }
    }

    /// <summary>
    /// 为指定元素添加下拉菜单打开事件的处理程序.
    /// </summary>
    /// <param name="target">触发事件源.</param>
    /// <param name="handler">触发事件.</param>
    public static void AddDropDownOpenedHandler(UIElement target, DropDownEventHandler handler)
    {
        target.AddHandler(DropDownOpenedEvent, handler);
    }

    /// <summary>
    /// 从指定元素移除下拉菜单打开事件的处理程序.
    /// </summary>
    /// <param name="target">触发事件源.</param>
    /// <param name="handler">触发事件.</param>
    public static void RemoveDropDownOpenedHandler(UIElement target, DropDownEventHandler handler)
    {
        target.RemoveHandler(DropDownOpenedEvent, handler);
    }

    /// <summary>
    /// 为指定元素添加下拉菜单关闭事件的处理程序.
    /// </summary>
    /// <param name="target">触发事件源.</param>
    /// <param name="handler">触发事件.</param>
    public static void AddDropDownClosedHandler(UIElement target, DropDownEventHandler handler)
    {
        target.AddHandler(DropDownClosedEvent, handler);
    }

    /// <summary>
    /// 从指定元素移除下拉菜单关闭事件的处理程序.
    /// </summary>
    /// <param name="target">触发事件源.</param>
    /// <param name="handler">触发事件.</param>
    public static void RemoveDropDownClosedHandler(UIElement target, DropDownEventHandler handler)
    {
        target.RemoveHandler(DropDownClosedEvent, handler);
    }

    /// <summary>
    /// 当 ReorderTabRows 属性更改时的静态回调，用于强制更新布局.
    /// </summary>
    private static void OnReorderTabRowsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var tabControlBase = sender as TabControlBase;
        if (tabControlBase?.TabStrip is TabStripPanel tabStripPanel)
        {
            tabStripPanel.RearrangeTabs = (bool)e.NewValue;
            tabStripPanel.InvalidateMeasure();
        }
    }

    /// <summary>
    /// 当 IsContentPreserved 属性更改时的静态回调，用于清理并更新内容呈现状态.
    /// </summary>
    private static void OnIsContentPreservedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TabControlBase radTabControlBase)
        {
            radTabControlBase.ClearContentSafely();
            radTabControlBase.UpdateContentSafely();
        }
    }

    /// <summary>
    /// 处理键盘导航键，预测并移动焦点到指定方向的元素.
    /// </summary>
    private static bool HandleNavigationKey(Control focusedItem, FocusNavigationDirection focusNavigationDirection)
    {
        if (focusedItem.PredictFocus(focusNavigationDirection) is FrameworkElement element)
        {
            Keyboard.Focus(element);
            return true;
        }

        return false;
    }
}