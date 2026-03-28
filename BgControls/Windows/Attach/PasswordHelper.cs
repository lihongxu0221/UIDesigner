using BgControls.Windows.Datas;

namespace BgControls.Windows.Attach;

/// <summary>
/// 密码帮助类，提供附加属性用于解决PasswordBox控件的密码绑定问题.
/// </summary>
public static class PasswordHelper
{
    /// <summary>
    /// 密码属性，用于双向绑定密码值.
    /// </summary>
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordHelper), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

    /// <summary>
    /// 附加属性，用于控制是否附加密码变更事件.
    /// </summary>
    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, OnAttachPropertyChanged));

    /// <summary>
    /// 附加属性，更新标记属性，用于防止循环更新.
    /// </summary>
    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordHelper));

    /// <summary>
    /// 附加属性，密码长度.
    /// </summary>
    public static readonly DependencyProperty PasswordLengthProperty =
        DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordHelper), new PropertyMetadata(ValueBoxes.Int0Box));

    /// <summary>
    /// 附加属性，是否监测.
    /// </summary>
    public static readonly DependencyProperty IsMonitoringProperty =
        DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordHelper), new FrameworkPropertyMetadata(ValueBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits, OnIsMonitoringChanged));

    /// <summary>
    /// 获取密码值.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <returns>密码字符串.</returns>
    public static string GetPassword(DependencyObject obj)
    {
        return (string)obj.GetValue(PasswordProperty);
    }

    /// <summary>
    /// 设置密码值.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <param name="value">密码字符串.</param>
    public static void SetPassword(DependencyObject obj, string value)
    {
        obj.SetValue(PasswordProperty, value);
    }

    /// <summary>
    /// 设置附加状态.
    /// </summary>
    /// <param name="dp">依赖对象.</param>
    /// <param name="value">是否附加.</param>
    public static void SetAttach(DependencyObject dp, bool value)
    {
        dp.SetValue(AttachProperty, value);
    }

    /// <summary>
    /// 获取附加状态.
    /// </summary>
    /// <param name="dp">依赖对象.</param>
    /// <returns>是否附加.</returns>
    public static bool GetAttach(DependencyObject dp)
    {
        return (bool)dp.GetValue(AttachProperty);
    }

    /// <summary>
    /// 设置更新标记.
    /// </summary>
    /// <param name="dp">依赖对象.</param>
    /// <param name="value">是否正在更新.</param>
    private static void SetIsUpdating(DependencyObject dp, bool value)
    {
        dp.SetValue(IsUpdatingProperty, value);
    }

    /// <summary>
    /// 获取更新标记.
    /// </summary>
    /// <param name="dp">依赖对象.</param>
    /// <returns>是否正在更新.</returns>
    private static bool GetIsUpdating(DependencyObject dp)
    {
        return (bool)dp.GetValue(IsUpdatingProperty);
    }

    /// <summary>
    /// 将光标移动到密码框末尾.可能有兼容性问题，需要测试.
    /// </summary>
    /// <param name="passwordBox">密码框.</param>
    private static void MoveCursorToEnd(PasswordBox passwordBox)
    {
        // 使用反射调用 PasswordBox 的 Select 方法
        Type type = passwordBox.GetType();
        MethodInfo? method = type.GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic);
        _ = method?.Invoke(passwordBox, new object[] { passwordBox.Password.Length, 0 });
    }

    /// <summary>
    /// 密码属性变更时的回调方法.
    /// </summary>
    /// <param name="d">依赖对象.</param>
    /// <param name="e">依赖属性变更事件参数.</param>
    private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            // 防止循环更新
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            if (!GetIsUpdating(passwordBox))
            {
                Debug.WriteLine($"Password updated: {e.NewValue}");
                passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
                MoveCursorToEnd(passwordBox); // 将光标移动到末尾
            }

            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }
    }

    /// <summary>
    /// 设置密码长度.
    /// </summary>
    /// <param name="element">依赖对象.</param>
    /// <param name="value">长度值.</param>
    public static void SetPasswordLength(DependencyObject element, int value) => element.SetValue(PasswordLengthProperty, value);

    /// <summary>
    /// 获取密码长度.
    /// </summary>
    /// <param name="element">依赖对象.</param>
    /// <returns>返回密码长度.</returns>
    public static int GetPasswordLength(DependencyObject element) => (int)element.GetValue(PasswordLengthProperty);

    /// <summary>
    /// 设置是否监测.
    /// </summary>
    /// <param name="element">依赖对象.</param>
    /// <param name="value">是否监控.</param>
    public static void SetIsMonitoring(DependencyObject element, bool value) => element.SetValue(IsMonitoringProperty, ValueBoxes.BooleanBox(value));

    /// <summary>
    /// 获取是否监测.
    /// </summary>
    /// <param name="element">依赖对象.</param>
    /// <returns>返回是否监测.</returns>
    public static bool GetIsMonitoring(DependencyObject element) => (bool)element.GetValue(IsMonitoringProperty);

    /// <summary>
    /// 附加属性变更时的回调方法.
    /// </summary>
    /// <param name="d">依赖对象.</param>
    /// <param name="e">依赖属性变更事件参数.</param>
    private static void OnAttachPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
            else
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }
    }

    private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is System.Windows.Controls.PasswordBox passwordBox)
        {
            if (e.NewValue is bool boolValue)
            {
                if (boolValue)
                {
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }
            }
        }
    }

    /// <summary>
    /// 密码框密码变更事件处理方法.
    /// </summary>
    /// <param name="sender">事件发送者.</param>
    /// <param name="e">路由事件参数.</param>
    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            SetIsUpdating(passwordBox, true);

            SetPassword(passwordBox, passwordBox.Password);
            bool isMonitoring = GetIsMonitoring(passwordBox);
            if (isMonitoring)
            {
                SetPasswordLength(passwordBox, passwordBox.Password.Length);
            }

            SetIsUpdating(passwordBox, false);
        }
    }
}