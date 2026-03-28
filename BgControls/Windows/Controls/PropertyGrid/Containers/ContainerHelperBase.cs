namespace BgControls.Windows.Controls.PropertyGrid;

/// <summary>
/// 容器辅助基类.
/// </summary>
/// <remarks>
/// 这是一个核心抽象基类，用于协助管理属性容器 (<see cref="IPropertyContainer"/>) 的逻辑.
/// 它负责处理子项的生成、清理、事件转发 (Prepare/Clear)、自定义编辑器查找以及
/// 响应容器属性 (如 Filter, Categorized) 的变化.
/// </remarks>
internal abstract class ContainerHelperBase
{
    /// <summary>
    /// 标识 IsGenerated 附加属性.
    /// </summary>
    /// <remarks>用于指示属性项是自动生成的还是显式定义的.</remarks>
    internal static readonly DependencyProperty IsGeneratedProperty =
        DependencyProperty.RegisterAttached("IsGenerated", typeof(bool), typeof(ContainerHelperBase), new PropertyMetadata(false));

    /// <summary>
    /// 获取一个值，该值指示属性项是否是自动生成的.
    /// </summary>
    /// <param name="obj">属性所属的对象.</param>
    /// <returns>如果是自动生成的则返回 true；否则返回 false.</returns>
    internal static bool GetIsGenerated(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsGeneratedProperty);
    }

    /// <summary>
    /// 设置一个值，该值指示属性项是否是自动生成的.
    /// </summary>
    /// <param name="obj">属性所属的对象.</param>
    /// <param name="value">是否自动生成.</param>
    internal static void SetIsGenerated(DependencyObject obj, bool value)
    {
        obj.SetValue(IsGeneratedProperty, value);
    }

    /// <summary>
    /// 属性容器实例.
    /// </summary>
    private readonly IPropertyContainer container;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerHelperBase"/> class.
    /// </summary>
    /// <param name="propertyContainer">关联的属性容器接口.</param>
    /// <exception cref="ArgumentNullException">propertyContainer 为 null 时抛出.</exception>
    public ContainerHelperBase(IPropertyContainer propertyContainer)
    {
        ArgumentNullException.ThrowIfNull(propertyContainer, nameof(propertyContainer));
        this.container = propertyContainer;

        // 监听容器的属性变化，以便在过滤、分类等设置改变时做出响应
        if (propertyContainer is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += this.OnPropertyContainerPropertyChanged;
        }
    }

    /// <summary>
    /// Gets 关联的属性容器.
    /// </summary>
    protected IPropertyContainer PropertyContainer => this.container;

    /// <summary>
    /// Gets 属性集合 (抽象属性).
    /// </summary>
    public abstract IList Properties { get; }

    /// <summary>
    /// Gets or sets 包含子属性项的 ItemsControl 控件.
    /// </summary>
    /// <remarks>这是实际在 UI 上承载子项的容器.</remarks>
    internal ItemsControl? ChildrenItemsControl { get; set; }

    /// <summary>
    /// Gets a value indicating whether 是否正在运行清理 (Clear) 逻辑.
    /// </summary>
    internal bool IsCleaning { get; private set; }

    /// <summary>
    /// 清理辅助类资源.
    /// </summary>
    /// <remarks>
    /// 移除事件订阅并清空所有生成的子容器. 通常在控件销毁或重置时调用.
    /// </remarks>
    public virtual void ClearHelper()
    {
        this.IsCleaning = true;
        if (this.PropertyContainer is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnPropertyContainerPropertyChanged;
        }

        if (this.ChildrenItemsControl != null)
        {
            // 移除所有 UI 容器
            ((IItemContainerGenerator)ChildrenItemsControl.ItemContainerGenerator).RemoveAll();
        }

        this.IsCleaning = false;
    }

    /// <summary>
    /// 准备子属性项 (生命周期钩子).
    /// </summary>
    /// <param name="propertyItem">生成的属性项容器.</param>
    /// <param name="item">原始数据项.</param>
    /// <remarks>
    /// 设置 ParentNode 并触发 <see cref="PropertyGrid.PreparePropertyItemEvent"/>.
    /// </remarks>
    public virtual void PrepareChildrenPropertyItem(PropertyItemBase propertyItem, object item)
    {
        propertyItem.ParentNode = this.PropertyContainer;
        PropertyGrid.RaisePreparePropertyItemEvent((UIElement)this.PropertyContainer, propertyItem, item);
    }

    /// <summary>
    /// 清除子属性项 (生命周期钩子).
    /// </summary>
    /// <param name="propertyItem">要清除的属性项容器.</param>
    /// <param name="item">原始数据项.</param>
    /// <remarks>
    /// 清空 ParentNode 并触发 <see cref="PropertyGrid.ClearPropertyItemEvent"/>.
    /// </remarks>
    public virtual void ClearChildrenPropertyItem(PropertyItemBase propertyItem, object item)
    {
        propertyItem.ParentNode = null;
        PropertyGrid.RaiseClearPropertyItemEvent((UIElement)this.PropertyContainer, propertyItem, item);
    }

    /// <summary>
    /// 根据类型键生成自定义编辑控件.
    /// </summary>
    /// <param name="definitionKey">定义键 (通常是属性类型 Type).</param>
    /// <param name="propertyItem">属性项.</param>
    /// <returns>生成的 FrameworkElement，如果没有对应的编辑器定义则返回 null.</returns>
    protected FrameworkElement? GenerateCustomEditingElement(Type? definitionKey, PropertyItemBase propertyItem)
    {
        if (this.PropertyContainer.EditorDefinitions == null)
        {
            return null;
        }

        // 递归查找基类的编辑器定义
        return this.CreateCustomEditor(this.PropertyContainer.EditorDefinitions.GetRecursiveBaseTypes(definitionKey), propertyItem);
    }

    /// <summary>
    /// 根据对象键生成自定义编辑控件.
    /// </summary>
    /// <param name="definitionKey">定义键 (通常是属性名称字符串).</param>
    /// <param name="propertyItem">属性项.</param>
    /// <returns>生成的 FrameworkElement，如果没有对应的编辑器定义则返回 null.</returns>
    protected FrameworkElement? GenerateCustomEditingElement(object definitionKey, PropertyItemBase propertyItem)
    {
        if (this.PropertyContainer.EditorDefinitions == null)
        {
            return null;
        }

        return this.CreateCustomEditor(this.PropertyContainer.EditorDefinitions[definitionKey], propertyItem);
    }

    /// <summary>
    /// 使用指定的编辑器定义创建编辑控件.
    /// </summary>
    /// <param name="customEditor">定义编辑器.</param>
    /// <param name="propertyItem">属性项.</param>
    /// <returns>生成的 FrameworkElement，如果没有对应的编辑器定义则返回 null.</returns>
    protected FrameworkElement? CreateCustomEditor(EditorDefinitionBase? customEditor, PropertyItemBase propertyItem)
    {
        return customEditor?.GenerateEditingElementInternal(propertyItem);
    }

    /// <summary>
    /// 当属性容器的属性发生改变时调用.
    /// </summary>
    /// <param name="sender">发起者.</param>
    /// <param name="e">事件信息.</param>
    /// <remarks>
    /// 此方法作为一个中心调度器，根据变化的属性名称调用相应的虚方法 (如 OnFilterChanged).
    /// </remarks>
    protected virtual void OnPropertyContainerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        string? propertyName = e.PropertyName;
        IPropertyContainer? ps = null; // 用于 nameof 获取属性名，实际不引用实例

        if (propertyName == nameof(ps.FilterInfo))
        {
            this.OnFilterChanged();
        }
        else if (propertyName == nameof(ps.IsCategorized))
        {
            this.OnCategorizationChanged();
        }
        else if (propertyName == nameof(ps.CategoryGroupDescription) && this.PropertyContainer.IsCategorized)
        {
            this.OnCategorizationChanged();
        }
        else if (propertyName == nameof(ps.CategoryDefinitions))
        {
            this.OnCategoryDefinitionsChanged();
        }
        else if (propertyName == nameof(ps.AutoGenerateProperties))
        {
            this.OnAutoGeneratePropertiesChanged();
        }
        else if (propertyName == nameof(ps.HideInheritedProperties))
        {
            this.OnHideInheritedPropertiesChanged();
        }
        else if (propertyName == nameof(ps.EditorDefinitions))
        {
            this.OnEditorDefinitionsChanged();
        }
        else if (propertyName == nameof(ps.PropertyDefinitions))
        {
            this.OnPropertyDefinitionsChanged();
        }
    }

    /// <summary>
    /// 当分类模式 (<see cref="IPropertyContainer.IsCategorized"/>) 或分组描述发生改变时触发.
    /// </summary>
    /// <remarks>
    /// 派生类应重写此方法以重新组织视图，例如在“分类视图”和“字母排序视图”之间切换，或者重新应用分组逻辑.
    /// </remarks>
    protected virtual void OnCategorizationChanged()
    {
    }

    /// <summary>
    /// 当过滤条件 (<see cref="IPropertyContainer.FilterInfo"/>) 发生改变时触发.
    /// </summary>
    /// <remarks>
    /// 派生类应重写此方法以应用新的过滤文本，隐藏不匹配的属性项.
    /// </remarks>
    protected virtual void OnFilterChanged()
    {
    }

    /// <summary>
    /// 当自动生成属性的设置 (<see cref="IPropertyContainer.AutoGenerateProperties"/>) 发生改变时触发.
    /// </summary>
    /// <remarks>
    /// 派生类通常在此处清空当前属性列表，并根据新设置重新通过反射生成属性项.
    /// </remarks>
    protected virtual void OnAutoGeneratePropertiesChanged()
    {
    }

    /// <summary>
    /// 当隐藏继承属性的设置 (<see cref="IPropertyContainer.HideInheritedProperties"/>) 发生改变时触发.
    /// </summary>
    /// <remarks>
    /// 派生类应在此处刷新属性列表，以移除或添加来自基类的属性.
    /// </remarks>
    protected virtual void OnHideInheritedPropertiesChanged()
    {
    }

    /// <summary>
    /// 当编辑器定义集合 (<see cref="IPropertyContainer.EditorDefinitions"/>) 发生改变时触发.
    /// </summary>
    /// <remarks>
    /// 当用户在运行时添加或移除自定义编辑器定义时调用. 派生类可能需要重新评估现有属性项应该使用哪个编辑器.
    /// </remarks>
    protected virtual void OnEditorDefinitionsChanged()
    {
    }

    /// <summary>
    /// 当显式属性定义集合 (<see cref="IPropertyContainer.PropertyDefinitions"/>) 发生改变时触发.
    /// </summary>
    /// <remarks>
    /// 当用户手动定义了属性的显示方式（如名称、顺序）时调用. 需要刷新列表以应用这些元数据.
    /// </remarks>
    protected virtual void OnPropertyDefinitionsChanged()
    {
    }

    /// <summary>
    /// 当分类定义集合 (<see cref="IPropertyContainer.CategoryDefinitions"/>) 发生改变时触发.
    /// </summary>
    /// <remarks>
    /// 当分类的排序或属性（如是否展开）发生变化时调用. 视图需要刷新以反映新的分类外观.
    /// </remarks>
    protected virtual void OnCategoryDefinitionsChanged()
    {
    }

    /// <summary>
    /// 当初始化结束时调用.
    /// </summary>
    public virtual void OnEndInit()
    {
    }

    /// <summary>
    /// 通知 EditorDefinitions 集合已更改.
    /// </summary>
    public virtual void NotifyEditorDefinitionsCollectionChanged()
    {
    }

    /// <summary>
    /// 通知 PropertyDefinitions 集合已更改.
    /// </summary>
    public virtual void NotifyPropertyDefinitionsCollectionChanged()
    {
    }

    /// <summary>
    /// 通知 CategoryDefinitions 集合已更改.
    /// </summary>
    public virtual void NotifyCategoryDefinitionsCollectionChanged()
    {
    }

    /// <summary>
    /// 根据数据项获取对应的属性项容器 (抽象方法).
    /// </summary>
    public abstract PropertyItemBase? ContainerFromItem(object item);

    /// <summary>
    /// 根据属性项容器获取对应的数据项 (抽象方法).
    /// </summary>
    public abstract object? ItemFromContainer(PropertyItemBase container);

    /// <summary>
    /// 为子属性项创建默认的数据绑定 (抽象方法).
    /// </summary>
    public abstract Binding CreateChildrenDefaultBinding(PropertyItemBase propertyItem);

    /// <summary>
    /// 强制从源更新值 (抽象方法).
    /// </summary>
    public abstract void UpdateValuesFromSource();

    /// <summary>
    /// 递归设定所有属性的展开状态.
    /// </summary>
    /// <param name="isExpanded">是否展开.</param>
    protected internal virtual void SetPropertiesExpansion(bool isExpanded)
    {
        foreach (object property in Properties)
        {
            // 尝试转换为 PropertyItemBase
            // 确保对象不为空，且允许展开
            if (property is PropertyItemBase item && item.IsExpandable)
            {
                // 如果该项还有子项 (有自己的 Helper)，递归调用
                if (item.ContainerHelper != null)
                {
                    item.ContainerHelper.SetPropertiesExpansion(isExpanded);
                }

                item.IsExpanded = isExpanded;
            }
        }
    }

    /// <summary>
    /// 递归设定指定属性名的展开状态.
    /// </summary>
    /// <param name="propertyName">属性名称.</param>
    /// <param name="isExpanded">是否展开.</param>
    protected internal virtual void SetPropertiesExpansion(string propertyName, bool isExpanded)
    {
        foreach (object property in Properties)
        {
            // 尝试转换为 PropertyItemBase
            // 确保对象不为空，且允许展开
            if (property is PropertyItemBase item && item.IsExpandable)
            {
                if (item.DisplayName == propertyName)
                {
                    // 找到了目标属性，设置状态
                    item.IsExpanded = isExpanded;
                    break;
                }
                else
                {
                    // 没找到：如果有子容器，递归进去找
                    item.ContainerHelper.SetPropertiesExpansion(propertyName, isExpanded);
                }
            }
        }
    }
}