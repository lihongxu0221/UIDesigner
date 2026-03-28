namespace BgControls.Windows.Controls;

/// <summary>
/// 消息条消息队列接口，用于管理通知消息的排队与显示逻辑.
/// </summary>
public interface ISnackbarMessageQueue
{
    /// <summary>
    /// 将通知消息加入队列以在消息条中显示.
    /// </summary>
    /// <param name="content">要显示的消息内容.</param>
    void Enqueue(object content);

    /// <summary>
    /// 将通知消息加入队列以在消息条中显示.
    /// </summary>
    /// <param name="content">要显示的消息内容.</param>
    /// <param name="actionContent">操作按钮上显示的内容.</param>
    /// <param name="actionHandler">用户点击操作按钮时执行的回调函数.</param>
    void Enqueue(object content, object? actionContent, Action? actionHandler);

    /// <summary>
    /// 将通知消息加入队列以在消息条中显示.
    /// </summary>
    /// <typeparam name="TArgument">操作参数的类型.</typeparam>
    /// <param name="content">要显示的消息内容.</param>
    /// <param name="actionContent">操作按钮上显示的内容.</param>
    /// <param name="actionHandler">用户点击操作按钮时执行的回调函数.</param>
    /// <param name="actionArgument">传递给 <paramref name="actionHandler" /> 的参数.</param>
    void Enqueue<TArgument>(object content, object? actionContent, Action<TArgument?>? actionHandler, TArgument? actionArgument);

    /// <summary>
    /// 将通知消息加入队列以在消息条中显示.
    /// </summary>
    /// <param name="content">要显示的消息内容.</param>
    /// <param name="neverConsiderToBeDuplicate">如果设为 <c>true</c>，则跳过重复检查，确保消息始终显示.</param>
    void Enqueue(object content, bool neverConsiderToBeDuplicate);

    /// <summary>
    /// 将通知消息加入队列以在消息条中显示.
    /// </summary>
    /// <param name="content">要显示的消息内容.</param>
    /// <param name="actionContent">操作按钮上显示的内容.</param>
    /// <param name="actionHandler">用户点击操作按钮时执行的回调函数.</param>
    /// <param name="promote">如果设为 <c>true</c>，该消息将被提升至队列前端显示.</param>
    void Enqueue(object content, object? actionContent, Action? actionHandler, bool promote);

    /// <summary>
    /// 将通知消息加入队列以在消息条中显示.
    /// </summary>
    /// <typeparam name="TArgument">操作参数的类型.</typeparam>
    /// <param name="content">要显示的消息内容.</param>
    /// <param name="actionContent">操作按钮上显示的内容.</param>
    /// <param name="actionHandler">用户点击操作按钮时执行的回调函数.</param>
    /// <param name="actionArgument">传递给 <paramref name="actionHandler" /> 的参数.</param>
    /// <param name="promote">如果设为 <c>true</c>，该消息将被提升至队列前端且不被视为重复项.</param>
    void Enqueue<TArgument>(object content, object? actionContent, Action<TArgument?>? actionHandler, TArgument? actionArgument, bool promote);

    /// <summary>
    /// 将通知消息加入队列以在消息条中显示.
    /// </summary>
    /// <typeparam name="TArgument">操作参数的类型.</typeparam>
    /// <param name="content">要显示的消息内容.</param>
    /// <param name="actionContent">操作按钮上显示的内容.</param>
    /// <param name="actionHandler">用户点击操作按钮时执行的回调函数.</param>
    /// <param name="actionArgument">传递给 <paramref name="actionHandler" /> 的参数.</param>
    /// <param name="promote">如果设为 <c>true</c>，该消息将被提升至队列前端.</param>
    /// <param name="neverConsiderToBeDuplicate">如果设为 <c>true</c>，该消息将永不被视为重复项.</param>
    /// <param name="durationOverride">消息显示的覆盖时长.</param>
    void Enqueue<TArgument>(object content, object? actionContent, Action<TArgument?>? actionHandler, TArgument? actionArgument, bool promote, bool neverConsiderToBeDuplicate, TimeSpan? durationOverride = null);

    /// <summary>
    /// 将通知消息加入队列以在消息条中显示.
    /// </summary>
    /// <param name="content">要显示的消息内容.</param>
    /// <param name="actionContent">操作按钮上显示的内容.</param>
    /// <param name="actionHandler">用户点击操作按钮时执行的回调函数.</param>
    /// <param name="actionArgument">传递给 <paramref name="actionHandler" /> 的参数.</param>
    /// <param name="promote">如果设为 <c>true</c>，该消息将被提升至队列前端.</param>
    /// <param name="neverConsiderToBeDuplicate">如果设为 <c>true</c>，该消息将永不被视为重复项.</param>
    /// <param name="durationOverride">消息显示的覆盖时长.</param>
    void Enqueue(object content, object? actionContent, Action<object?>? actionHandler, object? actionArgument, bool promote, bool neverConsiderToBeDuplicate, TimeSpan? durationOverride = null);
}