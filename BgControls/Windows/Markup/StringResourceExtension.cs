using BgCommon.Localization;
using System.Xaml;

namespace BgControls.Windows.Markup;

/// <summary>
/// 嵌入资源字符串绑定扩展.
/// </summary>
internal class StringResourceExtension :
    MarkupExtension,
    INotifyPropertyChanged,
    INotifyPropertyChanging
{
    private Assembly? assembly;
    private object? value;
    private string key = string.Empty;
    private string stringFormat = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringResourceExtension"/> class.
    /// </summary>
    public StringResourceExtension()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// Gets 调用的程序集.
    /// </summary>
    public Assembly? ExecuteAssembly => this.assembly;

    /// <summary>
    /// Gets or sets 绑定的键.
    /// </summary>
    [ConstructorArgument("key")]
    public string Key
    {
        get => this.key;
        set => _ = this.SetProperty(ref this.key, value);
    }

    /// <summary>
    /// Gets or sets 绑定的值.
    /// </summary>
    public object? Value
    {
        get => this.value;
        set => _ = this.SetProperty(ref this.value, value);
    }

    /// <summary>
    /// Gets or sets 字符串格式化.
    /// </summary>
    public string StringFormat
    {
        get => this.stringFormat;
        set => _ = this.SetProperty(ref this.stringFormat, value);
    }

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringResourceExtension"/> class.
    /// </summary>
    /// <param name="key">绑定的键.</param>
    public StringResourceExtension(string key)
    {
        this.Key = key;
    }

    /// <summary>
    /// 设置值.
    /// </summary>
    protected virtual void SetValue()
    {
        if (this.ExecuteAssembly == null)
        {
            this.Value = this.key;
            return;
        }

        string value = LocalizationProviderFactory.GetString(
            assemblyName: this.ExecuteAssembly?.GetName().Name,
            key: this.Key);

        if (!string.IsNullOrEmpty(this.StringFormat))
        {
            this.Value = string.Format(this.StringFormat, value);
        }
        else
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// 设置容器.<br/>
    /// 获取使用到的XAML的程序集.<br/>
    /// </summary>
    /// <param name="serviceProvider">serviceProvider.</param>
    protected virtual void Initial(IServiceProvider serviceProvider)
    {
        // 获取根对象（如 Window、UserControl）
        if (serviceProvider.GetService(typeof(IRootObjectProvider)) is IRootObjectProvider rootProvider)
        {
            // 获取绑定的XAML所在程序集
            Type type = rootProvider.RootObject.GetType();
            this.assembly = type.Assembly;
        }
    }

    /// <inheritdoc/>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        // 设置容器
        this.Initial(serviceProvider);

        // 设置初始值
        this.SetValue();
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget target ||
            target?.TargetObject is not Setter)
        {
            Binding binding = new(nameof(this.Value))
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };

            return binding.ProvideValue(serviceProvider);
        }
        else
        {
            return new Binding(nameof(this.Value))
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
        }
    }

    /// <summary>
    /// 触发属性变更事件.
    /// </summary>
    /// <param name="e">属性变更事件参数.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e, nameof(e));
        this.PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 触发属性变更事件.
    /// </summary>
    /// <param name="e">属性变更事件参数.</param>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e, nameof(e));
        this.PropertyChanging?.Invoke(this, e);
    }

    /// <summary>
    /// 触发属性变更事件.
    /// </summary>
    /// <param name="propertyName">属性名.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
        this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 触发属性变更事件.
    /// </summary>
    /// <param name="propertyName">属性名.</param>
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
        this.OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
    }

    /// <summary>
    /// 设置属性值.
    /// </summary>
    /// <typeparam name="T">属性类型.</typeparam>
    /// <param name="field">属性字段引用.</param>
    /// <param name="newValue">新值.</param>
    /// <param name="propertyName">属性名.</param>
    /// <returns>如果值发生改变则返回 true，否则返回 false.</returns>
    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        this.OnPropertyChanging(propertyName);
        field = newValue;
        this.OnPropertyChanged(propertyName);
        return true;
    }
}