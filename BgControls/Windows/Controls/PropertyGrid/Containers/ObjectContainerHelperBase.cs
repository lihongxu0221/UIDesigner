using BgControls.Core.Utilities;
using BgControls.Windows.Controls.PropertyGrid.Attributes;
using BgControls.Windows.Controls.PropertyGrid.Editors;

namespace BgControls.Windows.Controls.PropertyGrid;

/// <summary>
/// 对象容器辅助类的基类 (内部使用).
/// </summary>
/// <remarks>
/// <para>
/// 此类继承自 <see cref="ContainerHelperBase"/>，专门用于处理 <see cref="ItemsControl"/> 中的数据项是纯对象 (Data Object) 的情况.
/// </para>
/// <para>
/// 与直接包含 UI 元素的情况不同，当项是对象时，必须由 <see cref="IItemContainerGenerator"/> 创建对应的容器控件 (例如 ListBoxItem).
/// 此帮助类封装了与生成器交互、创建新容器以及将数据项绑定到容器的核心逻辑.
/// </para>
/// </remarks>
internal abstract class ObjectContainerHelperBase : ContainerHelperBase
{
    private readonly PropertyItemCollection propertyItemCollection;
    private volatile bool isPreparingItemFlag;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectContainerHelperBase"/> class.
    /// </summary>
    /// <param name="propertyContainer">属性容器.</param>
    public ObjectContainerHelperBase(IPropertyContainer propertyContainer)
        : base(propertyContainer)
    {
        this.propertyItemCollection = new PropertyItemCollection(new ObservableCollection<PropertyItem>());
        this.UpdateFilter();
        this.UpdateCategorization(false);
    }

    /// <inheritdoc/>
    public override IList Properties => this.propertyItemCollection;

    private PropertyItem? DefaultProperty
    {
        get
        {
            PropertyItem? defaultProperty = null;
            string defaultName = this.GetDefaultPropertyName();
            if (defaultName != null)
            {
                defaultProperty = propertyItemCollection.FirstOrDefault(
                    prop => object.Equals(defaultName, prop.PropertyDescriptor.Name));
            }

            return defaultProperty;
        }
    }

    protected PropertyItemCollection PropertyItems => this.propertyItemCollection;

    public event EventHandler? ObjectsGenerated;

    /// <inheritdoc/>
    public override PropertyItemBase? ContainerFromItem(object item)
    {
        if (item == null)
        {
            return null;
        }

        Debug.Assert(item is PropertyItem || item is string);

        if (item is PropertyItem propertyItem)
        {
            return propertyItem;
        }

        var propertyStr = item as string;
        if (propertyStr != null)
        {
            return PropertyItems.FirstOrDefault(prop => propertyStr == prop.PropertyDescriptor.Name);
        }

        return null;
    }

    /// <inheritdoc/>
    public override object? ItemFromContainer(PropertyItemBase container)
    {
        var propertyItem = container as PropertyItem;
        if (propertyItem == null)
        {
            return null;
        }

        return propertyItem.PropertyDescriptor.Name;
    }

    /// <inheritdoc/>
    public override void UpdateValuesFromSource()
    {
        foreach (PropertyItem propertyItem in PropertyItems)
        {
            propertyItem.DescriptorDefinition.UpdateValueFromSource();
            propertyItem.ContainerHelper.UpdateValuesFromSource();
        }
    }

    public void GenerateProperties()
    {
        if (this.PropertyItems.Count == 0 || this.ShouldRegenerateProperties())
        {
            this.RegenerateProperties();
        }
    }

    /// <inheritdoc/>
    protected override void OnFilterChanged()
    {
        this.UpdateFilter();
    }

    /// <inheritdoc/>
    protected override void OnCategorizationChanged()
    {
        this.UpdateCategorization(updateSubPropertiesCategorization: true);
    }

    /// <inheritdoc/>
    protected override void OnAutoGeneratePropertiesChanged()
    {
        this.RegenerateProperties();
    }

    /// <inheritdoc/>
    protected override void OnHideInheritedPropertiesChanged()
    {
        this.RegenerateProperties();
    }

    /// <inheritdoc/>
    protected override void OnEditorDefinitionsChanged()
    {
        this.RegenerateProperties();
    }

    /// <inheritdoc/>
    protected override void OnPropertyDefinitionsChanged()
    {
        this.RegenerateProperties();
    }

    /// <inheritdoc/>
    protected override void OnCategoryDefinitionsChanged()
    {
        this.RegenerateProperties();
    }

    /// <inheritdoc/>
    public override void NotifyCategoryDefinitionsCollectionChanged()
    {
        this.RegenerateProperties();
    }

    protected internal virtual bool ShouldRegenerateProperties()
    {
        return false;
    }

    /// <summary>
    /// Gets a value indicating whether 是否展开非基元类型属性.
    /// </summary>
    /// <returns>是否展开非基元类型属性.</returns>
    internal bool IsExpandingNonPrimitiveTypes()
    {
        if (this.PropertyContainer == null)
        {
            return false;
        }

        return this.PropertyContainer.IsExpandingNonPrimitiveTypes;
    }

    /// <inheritdoc/>
    protected internal override void SetPropertiesExpansion(bool isExpanded)
    {
        if (this.Properties.Count == 0)
        {
            this.GenerateProperties();
        }

        base.SetPropertiesExpansion(isExpanded);
    }

    /// <inheritdoc/>
    protected internal override void SetPropertiesExpansion(string propertyName, bool isExpanded)
    {
        if (this.Properties.Count == 0)
        {
            this.GenerateProperties();
        }

        base.SetPropertiesExpansion(propertyName, isExpanded);
    }

    private void UpdateFilter()
    {
        FilterInfo filterInfo = this.PropertyContainer.FilterInfo;
        this.PropertyItems.FilterPredicate = filterInfo.Predicate ?? PropertyItemCollection.CreateFilter(filterInfo.InputString, PropertyItems, PropertyContainer);
    }

    private void UpdateCategorization(bool updateSubPropertiesCategorization)
    {
        this.propertyItemCollection.UpdateCategorization(
            this.ComputeCategoryGroupDescription(),
            this.PropertyContainer.IsCategorized,
            this.PropertyContainer.IsSortedAlphabetically);
        if (updateSubPropertiesCategorization && this.propertyItemCollection.Count > 0)
        {
            foreach (PropertyItem propertyItem in this.propertyItemCollection)
            {
                if (propertyItem.Properties is PropertyItemCollection propertyItemCollection)
                {
                    propertyItemCollection.UpdateCategorization(
                        this.ComputeCategoryGroupDescription(),
                        this.PropertyContainer.IsCategorized,
                        this.PropertyContainer.IsSortedAlphabetically);
                }
            }
        }
    }

    private GroupDescription? ComputeCategoryGroupDescription()
    {
        if (this.PropertyContainer.IsCategorized)
        {
            return this.PropertyContainer.CategoryGroupDescription ??
                new PropertyGroupDescription(PropertyItemCollection.CategoryPropertyName);
        }

        return null;
    }

    private string GetCategoryGroupingPropertyName()
    {
        var propGroup = this.ComputeCategoryGroupDescription() as PropertyGroupDescription;
        return propGroup != null ? propGroup.PropertyName : string.Empty;
    }

    private void OnChildrenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == PropertyItemCollection.DisplayNamePropertyName ||
            e.PropertyName == PropertyItemCollection.CategoryOrderPropertyName ||
            e.PropertyName == PropertyItemCollection.DisplayNamePropertyName ||
            e.PropertyName == this.GetCategoryGroupingPropertyName())
        {
            if (this.ChildrenItemsControl != null &&
                this.ChildrenItemsControl.ItemContainerGenerator.Status != GeneratorStatus.GeneratingContainers &&
                this.isPreparingItemFlag == false)
            {
                this.PropertyItems.RefreshView();
            }
        }
    }

    protected abstract string GetDefaultPropertyName();

    protected abstract void GenerateSubPropertiesCore(Action<IEnumerable<PropertyItem>> updatePropertyItemsCallback);

    private void RegenerateProperties()
    {
        this.GenerateSubPropertiesCore(this.UpdatePropertyItemsCallback);
    }

    protected internal virtual void UpdatePropertyItemsCallback(IEnumerable<PropertyItem> subProperties)
    {
        foreach (var propertyItem in subProperties)
        {
            this.InitializePropertyItem(propertyItem);
        }

        foreach (var propertyItem in this.PropertyItems)
        {
            propertyItem.PropertyChanged -= this.OnChildrenPropertyChanged;
        }

        PropertyGrid? propertyGrid = null;
        if (this.PropertyContainer is PropertyGrid containerPropertyGrid)
        {
            propertyGrid = containerPropertyGrid;
        }
        else if (this.PropertyContainer is PropertyItemBase containerPropertyItem)
        {
            // 类中类
            PropertyGrid? parentElement = null;
            while (containerPropertyItem.ParentElement != null)
            {
                if (containerPropertyItem.ParentElement is PropertyItemBase propertyItemBase)
                {
                    containerPropertyItem = propertyItemBase;
                    continue;
                }

                parentElement = containerPropertyItem.ParentElement as PropertyGrid;
                if (parentElement != null)
                {
                    break;
                }
            }
            propertyGrid = parentElement;
        }

        // TO DO,权限校验和过滤
        if (propertyGrid != null && propertyGrid.FilterAdvance != null)
        {
            List<PropertyItem> propertyItems = new List<PropertyItem>();
            foreach (var propertyItem in subProperties)
            {
                if (propertyGrid.FilterAdvance.Invoke(propertyGrid.SelectedObjectName, propertyItem))
                {
                    propertyItems.Add(propertyItem);
                }
            }

            this.PropertyItems.UpdateItems(propertyItems);
        }
        else
        {
            this.PropertyItems.UpdateItems(subProperties);
        }

        foreach (var propertyItem in this.PropertyItems)
        {
            propertyItem.PropertyChanged += this.OnChildrenPropertyChanged;
        }

        if (propertyGrid != null)
        {
            propertyGrid.SelectedPropertyItem = DefaultProperty;
        }

        this.ObjectsGenerated?.Invoke(this, EventArgs.Empty);
    }

    protected bool GetIsExpanded(PropertyDescriptor propertyDescriptor)
    {
        if (propertyDescriptor == null)
        {
            return false;
        }

        return propertyDescriptor.GetAttribute<ExpandableObjectAttribute>()?.IsExpanded ?? false;
    }

    protected bool GetWillRefreshPropertyGrid(PropertyDescriptor propertyDescriptor)
    {
        if (propertyDescriptor == null)
        {
            return false;
        }

        var attribute = propertyDescriptor.GetAttribute<RefreshPropertiesAttribute>();
        if (attribute != null)
        {
            return attribute.RefreshProperties != RefreshProperties.None;
        }

        return false;
    }

    internal void InitializeDescriptorDefinition(
        DescriptorPropertyDefinitionBase descriptorDef,
        PropertyDefinition propertyDefinition)
    {
        ArgumentNullException.ThrowIfNull(descriptorDef, nameof(descriptorDef));
        if (propertyDefinition == null)
        {
            return;
        }

        if (propertyDefinition != null)
        {
            if (propertyDefinition.Category != null)
            {
                descriptorDef.Category = propertyDefinition.Category;
                descriptorDef.CategoryValue = propertyDefinition.Category;
            }

            if (propertyDefinition.Description != null)
            {
                descriptorDef.Description = propertyDefinition.Description;
            }

            if (propertyDefinition.DisplayName != null)
            {
                descriptorDef.DisplayName = propertyDefinition.DisplayName;
            }

            if (propertyDefinition.DisplayOrder.HasValue)
            {
                descriptorDef.DisplayOrder = propertyDefinition.DisplayOrder.Value;
            }

            if (propertyDefinition.IsExpandable.HasValue)
            {
                descriptorDef.ExpandableAttribute = propertyDefinition.IsExpandable.Value;
            }
        }
    }

    private void InitializePropertyItem(PropertyItem propertyItem)
    {
        var pd = propertyItem.DescriptorDefinition;
        propertyItem.PropertyDescriptor = pd.PropertyDescriptor;
        propertyItem.IsReadOnly = pd.IsReadOnly;
        propertyItem.DisplayName = pd.DisplayName;
        propertyItem.Description = pd.Description;
        propertyItem.DefinitionKey = pd.DefinitionKey;
        propertyItem.DecimalPlaces = pd.DecimalPlaces;
        propertyItem.Increment = pd.Increment;
        propertyItem.Maximum = pd.Maximum;
        propertyItem.Minimum = pd.Minimum;

        if (pd.DependsOnPropertyItemNames != null)
        {
            foreach (string dependsOnPropertyItemName in pd.DependsOnPropertyItemNames)
            {
                PropertyContainer.DependsOnPropertyItemsList.Add(new KeyValuePair<string, PropertyItem>(dependsOnPropertyItemName, propertyItem));
            }
        }

        propertyItem.Category = pd.Category;
        propertyItem.PropertyOrder = pd.DisplayOrder;
        if (pd.PropertyDescriptor.Converter is ExpandableObjectConverter)
        {
            propertyItem.IsExpandable = true;
        }
        else
        {
            this.SetupDefinitionBinding(propertyItem, PropertyItemBase.IsExpandableProperty, pd, nameof(pd.IsExpandable), BindingMode.OneWay);
        }

        this.SetupDefinitionBinding(propertyItem, PropertyItemBase.AdvancedOptionsIconProperty, pd, nameof(pd.AdvancedOptionsIcon), BindingMode.OneWay);
        this.SetupDefinitionBinding(propertyItem, PropertyItemBase.AdvancedOptionsTooltipProperty, pd, nameof(pd.AdvancedOptionsTooltip), BindingMode.OneWay);
        this.SetupDefinitionBinding(propertyItem, CustomPropertyItem.ValueProperty, pd, nameof(pd.Value), BindingMode.TwoWay);

        if (pd.CommandBindings != null)
        {
            foreach (CommandBinding commandBinding in pd.CommandBindings)
            {
                propertyItem.CommandBindings.Add(commandBinding);
            }
        }
    }

    private object? GetTypeDefaultValue(Type? type)
    {
        if (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetProperty("Value")?.PropertyType;
            }

            if (type != null && type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
        }

        return null;
    }

    private void SetupDefinitionBinding(
        PropertyItem propertyItem,
        DependencyProperty itemProperty,
        DescriptorPropertyDefinitionBase pd,
        string definitionPropertyName,
        BindingMode bindingMode)
    {
        propertyItem.SetBinding(
            itemProperty,
            new Binding()
            {
                Path = new PropertyPath(definitionPropertyName),
                Source = pd,
                Mode = bindingMode,
            });
    }

    internal FrameworkElement? GenerateChildrenEditorElement(PropertyItem propertyItem)
    {
        DescriptorPropertyDefinitionBase? pd = propertyItem.DescriptorDefinition;
        object? definitionKey = propertyItem.DefinitionKey;
        Type? definitionKeyType = definitionKey as Type;
        FrameworkElement? editorElement = null;
        ITypeEditor? editor = null;
        if (editor == null)
        {
            editor = pd.CreateAttributeEditor();
        }

        if (editor != null)
        {
            editorElement = editor.ResolveEditor(propertyItem);
        }

        if (editorElement == null && definitionKey != null)
        {
            editorElement = this.GenerateCustomEditingElement(definitionKey, propertyItem);
        }

        if (editorElement == null && definitionKeyType != null)
        {
            editorElement = this.GenerateCustomEditingElement(definitionKeyType, propertyItem);
        }

        if (editorElement == null && definitionKey == null && propertyItem.PropertyDescriptor != null)
        {
            editorElement = this.GenerateCustomEditingElement(propertyItem.PropertyDescriptor.Name, propertyItem);
        }

        if (editorElement == null && definitionKeyType == null)
        {
            editorElement = this.GenerateCustomEditingElement(propertyItem.PropertyType, propertyItem);
        }

        if (editorElement == null)
        {
            if (propertyItem.IsReadOnly &&
                !ListUtilities.IsListOfItems(propertyItem.PropertyType) &&
                !ListUtilities.IsCollectionOfItems(propertyItem.PropertyType) &&
                !ListUtilities.IsDictionaryOfItems(propertyItem.PropertyType))
            {
                editor = new TextBlockEditor(propertyItem.PropertyDescriptor?.Converter);
            }

            if (editor == null)
            {
                editor = (definitionKeyType != null) ?
                    propertyItem.CreateDefaultEditor(definitionKeyType, null) :
                    pd.CreateDefaultEditor(propertyItem);
            }

            editorElement = editor?.ResolveEditor(propertyItem);
        }

        return editorElement;
    }

    internal PropertyDefinition? GetPropertyDefinition(PropertyDescriptor descriptor)
    {
        PropertyDefinition? def = null;
        var propertyDefs = this.PropertyContainer.PropertyDefinitions;
        if (propertyDefs != null)
        {
            def = propertyDefs[descriptor.Name];
            if (def == null)
            {
                def = propertyDefs.GetRecursiveBaseTypes(descriptor.PropertyType);
            }
        }

        return def;
    }

    internal CategoryDefinition? GetCategoryDefinition(object categoryValue)
    {
        return this.PropertyContainer.CategoryDefinitions?[categoryValue];
    }

    /// <inheritdoc/>
    public override void PrepareChildrenPropertyItem(PropertyItemBase propertyItem, object item)
    {
        isPreparingItemFlag = true;
        base.PrepareChildrenPropertyItem(propertyItem, item);
        if (propertyItem.Editor == null)
        {
            var editor = this.GenerateChildrenEditorElement((PropertyItem)propertyItem);
            if (editor != null)
            {
                SetIsGenerated(editor, true);
                propertyItem.Editor = editor;
            }
        }

        isPreparingItemFlag = false;
    }

    /// <inheritdoc/>
    public override void ClearChildrenPropertyItem(PropertyItemBase propertyItem, object item)
    {
        if (propertyItem.Editor != null && GetIsGenerated(propertyItem.Editor))
        {
            propertyItem.Editor = null;
        }

        base.ClearChildrenPropertyItem(propertyItem, item);
    }

    /// <inheritdoc/>
    public override Binding CreateChildrenDefaultBinding(PropertyItemBase propertyItem)
    {
        return new Binding()
        {
            Path = new PropertyPath("Value"),
            Mode = ((PropertyItem)propertyItem).IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
        };
    }

    /// <summary>
    /// get the instance object DefaultProperty name.
    /// </summary>
    /// <param name="instance">the instance object.</param>
    /// <returns>return the instance object DefaultPropertyName. </returns>
    protected static string GetDefaultPropertyName(object instance)
    {
        // 1. 如果对象为空，直接返回 null
        if (instance != null)
        {
            // 2. 获取该对象的所有特性 (Attributes)
            // 使用 TypeDescriptor 是为了支持 ICustomTypeDescriptor (动态属性)
            var attributes = TypeDescriptor.GetAttributes(instance);

            // 3. 尝试从中取出 [DefaultProperty] 特性
            // 这里的 is 模式匹配写法比原来的强制转换更安全、更易读
            if (attributes[typeof(DefaultPropertyAttribute)] is DefaultPropertyAttribute defaultAttr)
            {
                // 4. 如果找到了，返回它指定的名称 (例如 "Name")
                return defaultAttr.Name ?? string.Empty;
            }
        }

        return string.Empty;
    }
}