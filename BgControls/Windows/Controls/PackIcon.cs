namespace BgControls.Windows.Controls;

/// <summary>
/// 图标展示控件，通过高效的索引映射机制从资源字典中检索几何数据.
/// </summary>
public class PackIcon : Control
{
    // 资源字典实例，不以下划线开头.
    private static readonly ResourceDictionary IconResources;
    private static readonly Lazy<Dictionary<int, string>> ResourceValueKeyMap = new Lazy<Dictionary<int, string>>(
        BuildResourceValueKeyMap,
        LazyThreadSafetyMode.ExecutionAndPublication);

    // 标识 Data 依赖属性的 Key，设置为只读.
    private static readonly DependencyPropertyKey DataPropertyKey =
        DependencyProperty.RegisterReadOnly("Data", typeof(Geometry), typeof(PackIcon), new PropertyMetadata(null));

    /// <summary>
    /// 标识 Kind 依赖属性.
    /// </summary>
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register("Kind", typeof(PackIconKind), typeof(PackIcon), new PropertyMetadata(PackIconKind.Abacus, KindPropertyChangedCallback));

    /// <summary>
    /// 标识 Data 依赖属性.
    /// </summary>
    public static readonly DependencyProperty DataProperty = DataPropertyKey.DependencyProperty;

    /// <summary>
    /// Initializes static members of the <see cref="PackIcon"/> class.
    /// </summary>
    static PackIcon()
    {
        // 覆盖默认样式键，以便控件可以正确加载通用样式.
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PackIcon), new FrameworkPropertyMetadata(typeof(PackIcon)));

        // 加载资源字典。注意：此时并不会立即解析所有的 Geometry 具体的 PathData.
        IconResources = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/BgControls;component/Assets/Style/PackIcon.xaml", UriKind.Absolute),
        };

        // 可选：在UI线程空闲时预热
        Dispatcher.CurrentDispatcher.BeginInvoke(
            () =>
            {
                try
                {
                    _ = ResourceValueKeyMap.Value;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"预热失败: {ex.Message}");
                }
            },
            DispatcherPriority.Background);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PackIcon"/> class.
    /// </summary>
    public PackIcon()
    {
    }

    /// <summary>
    /// Gets or sets 要显示的图标种类.
    /// </summary>
    public PackIconKind Kind
    {
        get { return (PackIconKind)this.GetValue(KindProperty); }
        set { this.SetValue(KindProperty, value); }
    }

    /// <summary>
    /// Gets 当前图标对应的矢量路径数据.
    /// </summary>
    public Geometry? Data
    {
        get { return (Geometry?)this.GetValue(DataProperty); }
        private set { this.SetValue(DataPropertyKey, value); }
    }

    /// <summary>
    /// 在应用控件模板时执行，用于初始化图标数据.
    /// </summary>
    public override void OnApplyTemplate()
    {
        // 调用基类方法.
        base.OnApplyTemplate();

        // 初始化更新一次图标路径.
        this.UpdateData();
    }

    /// <summary>
    /// 根据 Kind 属性，通过数值映射机制检索资源.
    /// </summary>
    private void UpdateData()
    {
        // 将枚举转换为整数值，这是处理所有别名（如 User, Person, Account）的统一标识.
        int enumValue = (int)this.Kind;

        if (ResourceValueKeyMap.Value.TryGetValue(enumValue, out var resKey))
        {
            this.SetGeometry(resKey);
            return;
        }

        this.Data = null;
    }

    private static Dictionary<int, string> BuildResourceValueKeyMap()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var enumType = typeof(PackIconKind);
            var resourceKeys = IconResources.Keys.OfType<string>().ToList();
            var resultMap = new Dictionary<int, string>(resourceKeys.Count);
            foreach (string resourceKey in resourceKeys)
            {
                if (Enum.TryParse(resourceKey, out PackIconKind kind))
                {
                    int enumValue = (int)kind;
                    _ = resultMap.TryAdd(enumValue, resourceKey);
                }
            }

            return resultMap;
        }
        finally
        {
            stopwatch.Stop();
            Debug.WriteLine($"PackIcon 映射表构建耗时: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    /// <summary>
    /// 从资源中提取并冻结几何图形.
    /// </summary>
    private void SetGeometry(string key)
    {
        if (IconResources[key] is Geometry geometry)
        {
            if (geometry.CanFreeze && !geometry.IsFrozen)
            {
                geometry.Freeze();
            }

            this.Data = geometry;
        }
    }

    /// <summary>
    /// Kind 属性更改时的静态回调函数.
    /// </summary>
    /// <param name="dependencyObject">发生更改的依赖对象.</param>
    /// <param name="eventArgs">包含新旧值的事件参数.</param>
    private static void KindPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
        // 如果对象是 PackIcon 类型，则通知其更新内部数据.
        if (dependencyObject is PackIcon iconInstance)
        {
            iconInstance.UpdateData();
        }
    }
}