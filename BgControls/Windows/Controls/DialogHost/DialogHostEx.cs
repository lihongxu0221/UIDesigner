namespace BgControls.Windows.Controls;

/// <summary>
/// 用于显示对话框的辅助扩展类.
/// </summary>
public static class DialogHostEx
{
    /// <summary>
    /// 在给定窗口中查找第一个 <see cref="DialogHost" /> 并显示对话框.
    /// </summary>
    /// <param name="window">应当显示模态对话框的窗口，该窗口必须包含一个 DialogHost.</param>
    /// <param name="content">要显示的内容，可以是控件或视图模型.</param>
    /// <exception cref="InvalidOperationException">在对视觉树进行深度优先遍历时未找到 DialogHost 时抛出.</exception>
    /// <remarks>
    /// 由于执行的是窗口视觉树的深度优先遍历，在屏幕拥有多个 DialogHost 的情况下使用此方法并不安全.
    /// </remarks>
    /// <returns>异步任务结果为关闭对话框时传递的参数.</returns>
    public static async Task<object?> ShowDialogAsync(this Window window, object content)
    {
        return await window.GetFirstDialogHost().ShowInternalAsync(content, null, null, null);
    }

    /// <summary>
    /// 在给定窗口中查找第一个 <see cref="DialogHost" /> 并显示对话框，同时支持打开事件回调.
    /// </summary>
    /// <param name="window">应当显示模态对话框的窗口.</param>
    /// <param name="content">要显示的内容，可以是控件或视图模型.</param>
    /// <param name="openedEventHandler">允许访问在实例上订阅的打开事件处理程序.</param>
    /// <exception cref="InvalidOperationException">未找到 DialogHost 时抛出.</exception>
    /// <returns>异步任务结果为关闭对话框时传递的参数回传.</returns>
    public static async Task<object?> ShowDialogAsync(this Window window, object content, DialogOpenedEventHandler openedEventHandler)
    {
        return await window.GetFirstDialogHost().ShowInternalAsync(content, openedEventHandler, null, null);
    }

    /// <summary>
    /// 在给定窗口中查找第一个 <see cref="DialogHost" /> 并显示对话框，同时支持正在关闭事件回调.
    /// </summary>
    /// <param name="window">应当显示模态对话框的窗口.</param>
    /// <param name="content">要显示的内容，可以是控件或视图模型.</param>
    /// <param name="closingEventHandler">允许访问在实例上订阅的正在关闭事件处理程序接口.</param>
    /// <exception cref="InvalidOperationException">未找到 DialogHost 时抛出.</exception>
    /// <returns>异步任务结果为关闭对话框时传递的参数.</returns>
    public static async Task<object?> ShowDialogAsync(this Window window, object content, DialogClosingEventHandler closingEventHandler)
    {
        return await window.GetFirstDialogHost().ShowInternalAsync(content, null, closingEventHandler, null);
    }

    /// <summary>
    /// 在给定窗口中查找第一个 <see cref="DialogHost" /> 并显示对话框，支持打开与正在关闭事件.
    /// </summary>
    /// <param name="window">应当显示模态对话框的窗口.</param>
    /// <param name="content">要显示的内容，可以是控件或视图模型.</param>
    /// <param name="openedEventHandler">打开事件处理程序.</param>
    /// <param name="closingEventHandler">正在关闭事件处理程序.</param>
    /// <returns>异步任务结果为关闭对话框时传递的参数.</returns>
    public static async Task<object?> ShowDialogAsync(this Window window, object content, DialogOpenedEventHandler openedEventHandler, DialogClosingEventHandler closingEventHandler)
    {
        return await window.GetFirstDialogHost().ShowInternalAsync(content, openedEventHandler, closingEventHandler, null);
    }

    /// <summary>
    /// 在给定窗口中查找第一个 <see cref="DialogHost" /> 并显示对话框，支持完整生命周期事件回调.
    /// </summary>
    /// <param name="window">应当显示模态对话框的窗口.</param>
    /// <param name="content">要显示的内容，可以是控件或视图模型.</param>
    /// <param name="openedEventHandler">打开事件处理程序.</param>
    /// <param name="closingEventHandler">正在关闭事件处理程序.</param>
    /// <param name="closedEventHandler">已关闭事件处理程序.</param>
    /// <returns>异步任务结果为关闭对话框时传递的参数.</returns>
    public static async Task<object?> ShowDialogAsync(this Window window, object content, DialogOpenedEventHandler openedEventHandler, DialogClosingEventHandler closingEventHandler, DialogClosedEventHandler closedEventHandler)
    {
        return await window.GetFirstDialogHost().ShowInternalAsync(content, openedEventHandler, closingEventHandler, closedEventHandler);
    }

    /// <summary>
    /// 使用给定依赖对象的父级或祖先 <see cref="DialogHost" /> 显示对话框.
    /// </summary>
    /// <param name="childDependencyObject">作为 DialogHost 视觉子级的依赖对象，对话框将在此处显示.</param>
    /// <param name="content">要显示的内容，可以是控件或视图模型.</param>
    /// <exception cref="InvalidOperationException">在溯源视觉树祖先时未找到 DialogHost 时抛出.</exception>
    /// <returns>异步任务结果为关闭对话框时传递的参数.</returns>
    public static async Task<object?> ShowDialogAsync(this DependencyObject childDependencyObject, object content)
    {
        return await childDependencyObject.GetOwningDialogHost().ShowInternalAsync(content, null, null, null);
    }

    /// <summary>
    /// 使用依赖对象的祖先 <see cref="DialogHost" /> 显示对话框，并支持打开事件.
    /// </summary>
    /// <param name="childDependencyObject">子依赖对象.</param>
    /// <param name="content">要显示的内容.</param>
    /// <param name="openedEventHandler">打开事件处理程序.</param>
    /// <returns>异步结果.</returns>
    public static async Task<object?> ShowDialogAsync(this DependencyObject childDependencyObject, object content, DialogOpenedEventHandler openedEventHandler)
    {
        return await childDependencyObject.GetOwningDialogHost().ShowInternalAsync(content, openedEventHandler, null, null);
    }

    /// <summary>
    /// 使用依赖对象的祖先 <see cref="DialogHost" /> 显示对话框，并支持正在关闭事件.
    /// </summary>
    /// <param name="childDependencyObject">子依赖对象.</param>
    /// <param name="content">要显示的内容.</param>
    /// <param name="closingEventHandler">正在关闭事件处理程序.</param>
    /// <returns>异步结果.</returns>
    public static async Task<object?> ShowDialogAsync(this DependencyObject childDependencyObject, object content, DialogClosingEventHandler closingEventHandler)
    {
        return await childDependencyObject.GetOwningDialogHost().ShowInternalAsync(content, null, closingEventHandler, null);
    }

    /// <summary>
    /// 使用依赖对象的祖先 <see cref="DialogHost" /> 显示对话框，支持打开与正在关闭事件.
    /// </summary>
    /// <param name="childDependencyObject">子依赖对象.</param>
    /// <param name="content">要显示的内容.</param>
    /// <param name="openedEventHandler">打开事件处理程序.</param>
    /// <param name="closingEventHandler">正在关闭事件处理程序.</param>
    /// <returns>异步结果.</returns>
    public static async Task<object?> ShowDialogAsync(this DependencyObject childDependencyObject, object content, DialogOpenedEventHandler openedEventHandler, DialogClosingEventHandler closingEventHandler)
    {
        return await childDependencyObject.GetOwningDialogHost().ShowInternalAsync(content, openedEventHandler, closingEventHandler, null);
    }

    /// <summary>
    /// 使用依赖对象的祖先 <see cref="DialogHost" /> 显示对话框，支持完整生命周期事件回调.
    /// </summary>
    /// <param name="childDependencyObject">子依赖对象.</param>
    /// <param name="content">要显示的内容.</param>
    /// <param name="openedEventHandler">打开事件处理程序.</param>
    /// <param name="closingEventHandler">正在关闭事件处理程序.</param>
    /// <param name="closedEventHandler">已关闭事件处理程序.</param>
    /// <returns>异步结果.</returns>
    public static async Task<object?> ShowDialogAsync(this DependencyObject childDependencyObject, object content, DialogOpenedEventHandler openedEventHandler, DialogClosingEventHandler closingEventHandler, DialogClosedEventHandler closedEventHandler)
    {
        return await childDependencyObject.GetOwningDialogHost().ShowInternalAsync(content, openedEventHandler, closingEventHandler, closedEventHandler);
    }

    /// <summary>
    /// 获取指定窗口视觉树中的第一个对话框宿主.
    /// </summary>
    /// <param name="window">目标窗口.</param>
    /// <returns>找到的对话框宿主实例.</returns>
    private static DialogHost GetFirstDialogHost(this Window window)
    {
        ArgumentNullException.ThrowIfNull(window, nameof(window));
        return window.VisualDepthFirstTraversal().OfType<DialogHost>().FirstOrDefault()
               ?? throw new InvalidOperationException("无法在视觉树中找到 DialogHost.");
    }

    /// <summary>
    /// 获取指定子依赖对象所属的对话框宿主.
    /// </summary>
    /// <param name="childDependencyObject">子级依赖对象.</param>
    /// <returns>所属的对话框宿主实例.</returns>
    private static DialogHost GetOwningDialogHost(this DependencyObject childDependencyObject)
    {
        ArgumentNullException.ThrowIfNull(childDependencyObject, nameof(childDependencyObject));
        return childDependencyObject.GetVisualAncestry().OfType<DialogHost>().FirstOrDefault()
               ?? throw new InvalidOperationException("无法在视觉树祖先链中找到 DialogHost.");
    }
}