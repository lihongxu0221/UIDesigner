using BgControls.Collections.Generic;
using BgControls.Windows.Controls;
using BgControls.Windows.Controls.DragDrop.NativeWrappers;
using BgControls.Windows.Input.Touch;
using Standard;

namespace BgControls.Windows.DragDrop;

/// <summary>
/// 拖拽放置管理器，负责协调原生拖拽、自定义拖拽以及触控拖拽逻辑.
/// </summary>
public static class DragDropManager
{
    /// <summary>
    /// 内部委托辅助类，用于管理原生拖拽事件与自定义事件处理程序的映射.
    /// </summary>
    private class DelegateHelper
    {
        /// <summary>
        /// 委托辅助对象的附加属性标识.
        /// </summary>
        private static readonly DependencyProperty DelegateHelperProperty = DependencyProperty.RegisterAttached("DelegateHelper", typeof(DelegateHelper), typeof(DragDropManager), new PropertyMetadata(null));

        private WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler> dropHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler> previewDropHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler> enterHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler> previewEnterHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler> leaveHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler> previewLeaveHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler> overHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler> previewOverHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.DragEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.GiveFeedbackEventHandler> giveFeedbackHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.GiveFeedbackEventHandler>();
        private WeakReferenceList<BgControls.Windows.DragDrop.GiveFeedbackEventHandler> previewGiveFeedbackHandlers = new WeakReferenceList<BgControls.Windows.DragDrop.GiveFeedbackEventHandler>();
        private WeakReferenceList<QueryContinueDragEventHandler> queryContinueHandlers = new WeakReferenceList<QueryContinueDragEventHandler>();
        private WeakReferenceList<QueryContinueDragEventHandler> previewQueryContinueHandlers = new WeakReferenceList<QueryContinueDragEventHandler>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateHelper"/> class.
        /// </summary>
        public DelegateHelper()
        {
            // 构造辅助类实例.
        }

        /// <summary>
        /// Gets a value indicating whether 是否具有原生放置事件处理程序.
        /// </summary>
        private bool HasNativeDropHandler => this.dropHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生预览放置事件处理程序.
        /// </summary>
        private bool HasNativePreviewDropHandler => this.previewDropHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生进入事件处理程序.
        /// </summary>
        private bool HasNativeEnterHandler => this.enterHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生预览进入事件处理程序.
        /// </summary>
        private bool HasNativePreviewEnterHandler => this.previewEnterHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生离开事件处理程序.
        /// </summary>
        private bool HasNativeLeaveHandler => this.leaveHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生预览离开事件处理程序.
        /// </summary>
        private bool HasNativePreviewLeaveHandler => this.previewLeaveHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生悬停事件处理程序.
        /// </summary>
        private bool HasNativeOverHandler => this.overHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生预览悬停事件处理程序.
        /// </summary>
        private bool HasNativePreviewOverHandler => this.previewOverHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生反馈事件处理程序.
        /// </summary>
        private bool HasNativeGiveFeedbackHandler => this.giveFeedbackHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生预览反馈事件处理程序.
        /// </summary>
        private bool HasNativePreviewGiveFeedbackHandler => this.previewGiveFeedbackHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生查询继续事件处理程序.
        /// </summary>
        private bool HasNativeQueryContinueHandler => this.queryContinueHandlers.Count > 0;

        /// <summary>
        /// Gets a value indicating whether 是否具有原生预览查询继续事件处理程序.
        /// </summary>
        private bool HasNativePreviewQueryContinueHandler => this.previewQueryContinueHandlers.Count > 0;

        /// <summary>
        /// 添加放置事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddDropHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativeDropHandler)
            {
                // 如果是首次添加，则挂载原生 WPF 事件.
                element.AddHandler(System.Windows.DragDrop.DropEvent, new System.Windows.DragEventHandler(helper.OnDrop), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.dropHandlers);
        }

        /// <summary>
        /// 移除放置事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemoveDropHandler(DependencyObject element, DragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.dropHandlers);
                if (!helper.HasNativeDropHandler)
                {
                    // 如果已无处理程序，则卸载原生 WPF 事件.
                    System.Windows.DragDrop.RemoveDropHandler(element, helper.OnDrop);
                }
            }
        }

        /// <summary>
        /// 添加预览放置事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddPreviewDropHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativePreviewDropHandler)
            {
                element.AddHandler(System.Windows.DragDrop.PreviewDropEvent, new System.Windows.DragEventHandler(helper.OnPreviewDrop), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.previewDropHandlers);
        }

        /// <summary>
        /// 移除预览放置事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemovePreviewDropHandler(DependencyObject element, DragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.previewDropHandlers);
                if (!helper.HasNativePreviewDropHandler)
                {
                    System.Windows.DragDrop.RemovePreviewDropHandler(element, helper.OnPreviewDrop);
                }
            }
        }

        /// <summary>
        /// 添加拖拽进入事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddDragEnterHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativeEnterHandler)
            {
                element.AddHandler(System.Windows.DragDrop.DragEnterEvent, new System.Windows.DragEventHandler(helper.OnEnter), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.enterHandlers);
        }

        /// <summary>
        /// 移除拖拽进入事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemoveDragEnterHandler(DependencyObject element, DragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.enterHandlers);
                if (!helper.HasNativeEnterHandler)
                {
                    System.Windows.DragDrop.RemoveDragEnterHandler(element, helper.OnEnter);
                }
            }
        }

        /// <summary>
        /// 添加预览拖拽进入事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddPreviewDragEnterHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativePreviewEnterHandler)
            {
                element.AddHandler(System.Windows.DragDrop.PreviewDragEnterEvent, new System.Windows.DragEventHandler(helper.OnPreviewEnter), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.previewEnterHandlers);
        }

        /// <summary>
        /// 移除预览拖拽进入事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemovePreviewDragEnterHandler(DependencyObject element, DragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.previewEnterHandlers);
                if (!helper.HasNativePreviewEnterHandler)
                {
                    System.Windows.DragDrop.RemovePreviewDragEnterHandler(element, helper.OnPreviewEnter);
                }
            }
        }

        /// <summary>
        /// 添加拖拽离开事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddDragLeaveHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativeLeaveHandler)
            {
                element.AddHandler(System.Windows.DragDrop.DragLeaveEvent, new System.Windows.DragEventHandler(helper.OnLeave), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.leaveHandlers);
        }

        /// <summary>
        /// 移除拖拽离开事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemoveDragLeaveHandler(DependencyObject element, DragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.leaveHandlers);
                if (!helper.HasNativeLeaveHandler)
                {
                    System.Windows.DragDrop.RemoveDragLeaveHandler(element, helper.OnLeave);
                }
            }
        }

        /// <summary>
        /// 添加预览拖拽离开事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddPreviewDragLeaveHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativePreviewLeaveHandler)
            {
                element.AddHandler(System.Windows.DragDrop.PreviewDragLeaveEvent, new System.Windows.DragEventHandler(helper.OnPreviewLeave), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.previewLeaveHandlers);
        }

        /// <summary>
        /// 移除预览拖拽离开事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemovePreviewDragLeaveHandler(DependencyObject element, DragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.previewLeaveHandlers);
                if (!helper.HasNativePreviewLeaveHandler)
                {
                    System.Windows.DragDrop.RemovePreviewDragLeaveHandler(element, helper.OnPreviewLeave);
                }
            }
        }

        /// <summary>
        /// 添加拖拽悬停事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddDragOverHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativeOverHandler)
            {
                element.AddHandler(System.Windows.DragDrop.DragOverEvent, new System.Windows.DragEventHandler(helper.OnOver), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.overHandlers);
        }

        /// <summary>
        /// 移除拖拽悬停事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemoveDragOverHandler(DependencyObject element, DragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.overHandlers);
                if (!helper.HasNativeOverHandler)
                {
                    System.Windows.DragDrop.RemoveDragOverHandler(element, helper.OnOver);
                }
            }
        }

        /// <summary>
        /// 添加预览拖拽悬停事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddPreviewDragOverHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativePreviewOverHandler)
            {
                element.AddHandler(System.Windows.DragDrop.PreviewDragOverEvent, new System.Windows.DragEventHandler(helper.OnPreviewOver), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.previewOverHandlers);
        }

        /// <summary>
        /// 移除预览拖拽悬停事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemovePreviewDragOverHandler(DependencyObject element, DragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.previewOverHandlers);
                if (!helper.HasNativePreviewOverHandler)
                {
                    System.Windows.DragDrop.RemovePreviewDragOverHandler(element, helper.OnPreviewOver);
                }
            }
        }

        /// <summary>
        /// 添加提供反馈事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativeGiveFeedbackHandler)
            {
                element.AddHandler(System.Windows.DragDrop.GiveFeedbackEvent, new System.Windows.GiveFeedbackEventHandler(helper.OnGiveFeedback), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.giveFeedbackHandlers);
        }

        /// <summary>
        /// 移除提供反馈事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemoveGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.giveFeedbackHandlers);
                if (!helper.HasNativeGiveFeedbackHandler)
                {
                    System.Windows.DragDrop.RemoveGiveFeedbackHandler(element, helper.OnGiveFeedback);
                }
            }
        }

        /// <summary>
        /// 添加预览提供反馈事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddPreviewGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativePreviewGiveFeedbackHandler)
            {
                element.AddHandler(System.Windows.DragDrop.PreviewGiveFeedbackEvent, new System.Windows.GiveFeedbackEventHandler(helper.OnPreviewGiveFeedback), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.previewGiveFeedbackHandlers);
        }

        /// <summary>
        /// 移除预览提供反馈事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemovePreviewGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.previewGiveFeedbackHandlers);
                if (!helper.HasNativePreviewGiveFeedbackHandler)
                {
                    System.Windows.DragDrop.RemovePreviewGiveFeedbackHandler(element, helper.OnPreviewGiveFeedback);
                }
            }
        }

        /// <summary>
        /// 添加查询继续拖拽事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativeQueryContinueHandler)
            {
                element.AddHandler(System.Windows.DragDrop.QueryContinueDragEvent, new System.Windows.QueryContinueDragEventHandler(helper.OnQueryContinue), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.queryContinueHandlers);
        }

        /// <summary>
        /// 移除查询继续拖拽事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemoveQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.queryContinueHandlers);
                if (!helper.HasNativeQueryContinueHandler)
                {
                    System.Windows.DragDrop.RemoveQueryContinueDragHandler(element, helper.OnQueryContinue);
                }
            }
        }

        /// <summary>
        /// 添加预览查询继续拖拽事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
        internal static void AddPreviewQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler, bool handledEventsToo)
        {
            DelegateHelper helper = DelegateHelper.GetOrCreateDelegateHelper(element);
            if (!helper.HasNativePreviewQueryContinueHandler)
            {
                element.AddHandler(System.Windows.DragDrop.PreviewQueryContinueDragEvent, new System.Windows.QueryContinueDragEventHandler(helper.OnPreviewQueryContinue), handledEventsToo);
            }

            DelegateHelper.AddHandler(handler, helper.previewQueryContinueHandlers);
        }

        /// <summary>
        /// 移除预览查询继续拖拽事件处理程序.
        /// </summary>
        /// <param name="element">目标元素.</param>
        /// <param name="handler">处理程序.</param>
        internal static void RemovePreviewQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
        {
            DelegateHelper helper = DelegateHelper.GetDelegateHelper(element);
            if (helper != null)
            {
                DelegateHelper.RemoveHandler(handler, helper.previewQueryContinueHandlers);
                if (!helper.HasNativePreviewQueryContinueHandler)
                {
                    System.Windows.DragDrop.RemovePreviewQueryContinueDragHandler(element, helper.OnPreviewQueryContinue);
                }
            }
        }

        /// <summary>
        /// 获取指定对象的委托辅助器.
        /// </summary>
        /// <param name="obj">目标依赖对象.</param>
        /// <returns>委托辅助器实例. </returns>
        private static DelegateHelper GetDelegateHelper(DependencyObject obj)
        {
            return (DelegateHelper)obj.GetValue(DelegateHelper.DelegateHelperProperty);
        }

        /// <summary>
        /// 获取或创建指定对象的委托辅助器.
        /// </summary>
        /// <param name="obj">目标依赖对象.</param>
        /// <returns>委托辅助器实例. </returns>
        private static DelegateHelper GetOrCreateDelegateHelper(DependencyObject obj)
        {
            DelegateHelper helper = (DelegateHelper)obj.GetValue(DelegateHelper.DelegateHelperProperty);
            if (helper == null)
            {
                helper = new DelegateHelper();
                DelegateHelper.SetDelegateHelper(obj, helper);
            }

            return helper;
        }

        /// <summary>
        /// 设置指定对象的委托辅助器.
        /// </summary>
        /// <param name="obj">目标依赖对象.</param>
        /// <param name="value">辅助器实例.</param>
        private static void SetDelegateHelper(DependencyObject obj, DelegateHelper value)
        {
            obj.SetValue(DelegateHelper.DelegateHelperProperty, value);
        }

        /// <summary>
        /// 通用的处理程序添加方法.
        /// </summary>
        private static void AddHandler<T>(T handler, WeakReferenceList<T> list)
            where T : class
        {
            list.Add(handler);
        }

        /// <summary>
        /// 通用的处理程序移除方法.
        /// </summary>
        private static void RemoveHandler<T>(T handler, WeakReferenceList<T> list)
            where T : class
        {
            list.Remove(handler);
        }

        /// <summary>
        /// 处理原生拖拽事件并转发到自定义路由事件.
        /// </summary>
        private void OnDragEventHandler(object sender, System.Windows.DragEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                // 包装原生参数并触发内部定义的路由事件.
                DragEventArgs customArgs = new DragEventArgs(e);
                source.RaiseEvent(customArgs);

                // 将处理状态和效果同步回原生参数.
                e.Handled = customArgs.Handled;
                e.Effects = customArgs.Effects;
            }
        }

        /// <summary>
        /// 处理原生提供反馈事件并转发.
        /// </summary>
        private void OnGiveFeedbackEventHandler(object sender, System.Windows.GiveFeedbackEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                GiveFeedbackEventArgs customArgs = new GiveFeedbackEventArgs(e);
                source.RaiseEvent(customArgs);
                e.Handled = customArgs.Handled;
                e.UseDefaultCursors = customArgs.UseDefaultCursors;

                // 更新视觉窗口的效果表现.
                IEffectsPresenter? presenter = DragDropManager.dragDropWindow?.Content as IEffectsPresenter;
                if (presenter != null)
                {
                    presenter.Effects = e.Effects;
                }

                // 如果不使用默认光标，则覆盖系统光标.
                if (!e.UseDefaultCursors && Mouse.OverrideCursor != customArgs.Cursor)
                {
                    Mouse.OverrideCursor = customArgs.Cursor;
                }
            }
        }

        /// <summary>
        /// 处理原生查询继续拖拽事件并转发.
        /// </summary>
        private void OnQueryContinueDragEventHandler(object sender, System.Windows.QueryContinueDragEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                QueryContinueDragEventArgs customArgs = new QueryContinueDragEventArgs(e);
                source.RaiseEvent(customArgs);
                e.Handled = customArgs.Handled;
                e.Action = customArgs.Action;
                DragDropManager.LastQueryContinueAction = customArgs.Action;
            }
        }

        /// <summary>
        /// 放置事件的回调入口.
        /// </summary>
        private void OnDrop(object sender, System.Windows.DragEventArgs e)
        {
            // 放置完成后销毁拖拽视觉窗口.
            DragDropManager.DestroyDragDropWindow();
            this.OnDragEventHandler(sender, e);
        }

        /// <summary>
        /// 预览放置事件的回调入口.
        /// </summary>
        private void OnPreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            DragDropManager.DestroyDragDropWindow();
            this.OnDragEventHandler(sender, e);
        }

        /// <summary>
        /// 拖拽进入事件的回调.
        /// </summary>
        private void OnEnter(object sender, System.Windows.DragEventArgs e)
        {
            this.OnDragEventHandler(sender, e);
        }

        /// <summary>
        /// 预览拖拽进入事件的回调.
        /// </summary>
        private void OnPreviewEnter(object sender, System.Windows.DragEventArgs e)
        {
            this.OnDragEventHandler(sender, e);
        }

        /// <summary>
        /// 拖拽离开事件的回调.
        /// </summary>
        private void OnLeave(object sender, System.Windows.DragEventArgs e)
        {
            this.OnDragEventHandler(sender, e);
        }

        /// <summary>
        /// 预览拖拽离开事件的回调.
        /// </summary>
        private void OnPreviewLeave(object sender, System.Windows.DragEventArgs e)
        {
            this.OnDragEventHandler(sender, e);
        }

        /// <summary>
        /// 拖拽悬停事件的回调.
        /// </summary>
        private void OnOver(object sender, System.Windows.DragEventArgs e)
        {
            this.OnDragEventHandler(sender, e);
        }

        /// <summary>
        /// 预览拖拽悬停事件的回调.
        /// </summary>
        private void OnPreviewOver(object sender, System.Windows.DragEventArgs e)
        {
            this.OnDragEventHandler(sender, e);
        }

        /// <summary>
        /// 查询继续拖拽事件的回调.
        /// </summary>
        private void OnQueryContinue(object sender, System.Windows.QueryContinueDragEventArgs e)
        {
            this.OnQueryContinueDragEventHandler(sender, e);
        }

        /// <summary>
        /// 预览查询继续拖拽事件的回调.
        /// </summary>
        private void OnPreviewQueryContinue(object sender, System.Windows.QueryContinueDragEventArgs e)
        {
            this.OnQueryContinueDragEventHandler(sender, e);
        }

        /// <summary>
        /// 提供反馈事件的回调.
        /// </summary>
        private void OnGiveFeedback(object sender, System.Windows.GiveFeedbackEventArgs e)
        {
            this.OnGiveFeedbackEventHandler(sender, e);
        }

        /// <summary>
        /// 预览提供反馈事件的回调.
        /// </summary>
        private void OnPreviewGiveFeedback(object sender, System.Windows.GiveFeedbackEventArgs e)
        {
            this.OnGiveFeedbackEventHandler(sender, e);
        }
    }

    /// <summary>
    /// 拖拽初始化器的附加属性标识.
    /// </summary>
    private static readonly DependencyProperty DragInitializerProperty = DependencyProperty.RegisterAttached("DragInitializer", typeof(DragInitializer), typeof(DragDropManager), new PropertyMetadata(null));

    /// <summary>
    /// 是否允许捕获拖拽的附加属性标识.
    /// </summary>
    public static readonly DependencyProperty AllowCapturedDragProperty = DependencyProperty.RegisterAttached("AllowCapturedDrag", typeof(bool), typeof(DragDropManager), new PropertyMetadata(false, DragDropManager.OnAllowCapturedDragPropertyChanged));

    /// <summary>
    /// 是否允许拖拽的附加属性标识.
    /// </summary>
    public static readonly DependencyProperty AllowDragProperty = DependencyProperty.RegisterAttached("AllowDrag", typeof(bool), typeof(DragDropManager), new PropertyMetadata(false, DragDropManager.OnAllowDragPropertyChanged));

    /// <summary>
    /// 触控拖拽触发器的附加属性标识.
    /// </summary>
    [Obsolete("Obsoleted with 2017 R1. Use BgControls.Windows.Input.Touch.TouchManager's DragStartTrigger instead.", false)]
    public static readonly DependencyProperty TouchDragTriggerProperty = DependencyProperty.RegisterAttached("TouchDragTrigger", typeof(TouchDragTrigger), typeof(DragDropManager), new PropertyMetadata(TouchDragTrigger.TapDown, DragDropManager.OnTouchDragTriggerPropertyChanged));

    /// <summary>
    /// 拖拽进入路由事件.
    /// </summary>
    internal static readonly RoutedEvent DragEnterEvent = EventManager.RegisterRoutedEvent("DragEnter", RoutingStrategy.Bubble, typeof(DragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 拖拽离开路由事件.
    /// </summary>
    internal static readonly RoutedEvent DragLeaveEvent = EventManager.RegisterRoutedEvent("DragLeave", RoutingStrategy.Bubble, typeof(DragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 拖拽悬停路由事件.
    /// </summary>
    internal static readonly RoutedEvent DragOverEvent = EventManager.RegisterRoutedEvent("DragOver", RoutingStrategy.Bubble, typeof(DragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 放置路由事件.
    /// </summary>
    internal static readonly RoutedEvent DropEvent = EventManager.RegisterRoutedEvent("Drop", RoutingStrategy.Bubble, typeof(DragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 提供反馈路由事件.
    /// </summary>
    internal static readonly RoutedEvent GiveFeedbackEvent = EventManager.RegisterRoutedEvent("GiveFeedback", RoutingStrategy.Bubble, typeof(GiveFeedbackEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 预览拖拽进入路由事件.
    /// </summary>
    internal static readonly RoutedEvent PreviewDragEnterEvent = EventManager.RegisterRoutedEvent("PreviewDragEnter", RoutingStrategy.Tunnel, typeof(DragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 预览拖拽离开路由事件.
    /// </summary>
    internal static readonly RoutedEvent PreviewDragLeaveEvent = EventManager.RegisterRoutedEvent("PreviewDragLeave", RoutingStrategy.Tunnel, typeof(DragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 预览拖拽悬停路由事件.
    /// </summary>
    internal static readonly RoutedEvent PreviewDragOverEvent = EventManager.RegisterRoutedEvent("PreviewDragOver", RoutingStrategy.Tunnel, typeof(DragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 预览放置路由事件.
    /// </summary>
    internal static readonly RoutedEvent PreviewDropEvent = EventManager.RegisterRoutedEvent("PreviewDrop", RoutingStrategy.Tunnel, typeof(DragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 预览提供反馈路由事件.
    /// </summary>
    internal static readonly RoutedEvent PreviewGiveFeedbackEvent = EventManager.RegisterRoutedEvent("PreviewGiveFeedback", RoutingStrategy.Tunnel, typeof(GiveFeedbackEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 预览查询继续拖拽路由事件.
    /// </summary>
    internal static readonly RoutedEvent PreviewQueryContinueDragEvent = EventManager.RegisterRoutedEvent("PreviewQueryContinueDrag", RoutingStrategy.Tunnel, typeof(BgControls.Windows.DragDrop.QueryContinueDragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 查询继续拖拽路由事件.
    /// </summary>
    internal static readonly RoutedEvent QueryContinueDragEvent = EventManager.RegisterRoutedEvent("QueryContinueDrag", RoutingStrategy.Bubble, typeof(BgControls.Windows.DragDrop.QueryContinueDragEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 拖拽放置完成路由事件.
    /// </summary>
    internal static readonly RoutedEvent DragDropCompletedEvent = EventManager.RegisterRoutedEvent("DragDropCompleted", RoutingStrategy.Bubble, typeof(DragDropCompletedEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 拖拽初始化路由事件.
    /// </summary>
    internal static readonly RoutedEvent DragInitializeEvent = EventManager.RegisterRoutedEvent("DragInitialize", RoutingStrategy.Bubble, typeof(DragInitializeEventHandler), typeof(DragDropManager));

    /// <summary>
    /// 拖拽提示位置变更路由事件.
    /// </summary>
    internal static readonly RoutedEvent DragCuePositionEvent = EventManager.RegisterRoutedEvent("DragCuePosition", RoutingStrategy.Direct, typeof(EventHandler<DragCuePositionEventArgs>), typeof(DragDropManager));

    /// <summary>
    /// Initializes static members of the <see cref="DragDropManager"/> class.
    /// </summary>
    static DragDropManager()
    {
        // 设置默认的最小拖拽启动距离.
        DragDropManager.MinimumHorizontalDragDistance = 4.0;
        DragDropManager.MinimumVerticalDragDistance = 4.0;
    }

    private static bool isInNativeDrag;
    private static Window? dragDropWindow;
    private static Cursor? overrideCursor;

    /// <summary>
    /// Gets or sets 初始拖拽坐标位置.
    /// </summary>
    private static Point InitialDragPosition { get; set; }

    /// <summary>
    /// Gets or sets 拖拽视觉对象的偏移量.
    /// </summary>
    private static Point DragVisualOffset { get; set; }

    /// <summary>
    /// Gets or sets 拖拽提示信息的偏移量.
    /// </summary>
    private static Point DragCueOffset { get; set; }

    /// <summary>
    /// Gets a value indicating whether 当前是否正在进行拖拽操作.
    /// </summary>
    public static bool IsDragInProgress => DragDropManager.DragOperation != null || DragDropManager.isInNativeDrag;

    /// <summary>
    /// Gets a value indicating whether 当前运行环境是否为完全信任模式.
    /// </summary>
    public static bool IsFullTrust => ApplicationHelper.IsFullTrust;

    /// <summary>
    /// Gets or sets 允许启动拖拽的最小水平位移.
    /// </summary>
    public static double MinimumHorizontalDragDistance { get; set; }

    /// <summary>
    /// Gets or sets 允许启动拖拽的最小垂直位移.
    /// </summary>
    public static double MinimumVerticalDragDistance { get; set; }

    /// <summary>
    /// Gets or sets 拖拽放置操作的默认效果标志.
    /// </summary>
    public static DragDropEffects DefaultDragDropEffects { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 是否在装饰层（AdornerLayer）中显示拖拽视觉效果.
    /// </summary>
    public static bool UseAdornerLayer { get; set; }

    /// <summary>
    /// Gets or sets 内部记录的最后一次查询继续拖拽的行为.
    /// </summary>
    internal static DragAction LastQueryContinueAction { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 是否处于触控拖拽模式中.
    /// </summary>
    internal static bool IsInTouchDrag { get; set; }

    /// <summary>
    /// Gets or sets 当前托管的拖拽操作实例.
    /// </summary>
    internal static DragOperation? DragOperation { get; set; }

    /// <summary>
    /// 为指定元素添加拖拽初始化事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddDragInitializeHandler(DependencyObject element, DragInitializeEventHandler handler)
    {
        DragDropManager.AddDragInitializeHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加拖拽初始化事件处理程序，支持高级配置.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddDragInitializeHandler(DependencyObject element, DragInitializeEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.DragInitializeEvent, handler, handledEventsToo);
    }

    /// <summary>
    /// 移除指定元素的拖拽初始化事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemoveDragInitializeHandler(DependencyObject element, DragInitializeEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.DragInitializeEvent, handler);
    }

    /// <summary>
    /// 为指定元素添加拖拽进入事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddDragEnterHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.AddDragEnterHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加拖拽进入事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddDragEnterHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.DragEnterEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddDragEnterHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加预览拖拽进入事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddPreviewDragEnterHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.AddPreviewDragEnterHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加预览拖拽进入事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddPreviewDragEnterHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.PreviewDragEnterEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddPreviewDragEnterHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加拖拽离开事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddDragLeaveHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.AddDragLeaveHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加拖拽离开事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddDragLeaveHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.DragLeaveEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddDragLeaveHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加预览拖拽离开事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddPreviewDragLeaveHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.AddPreviewDragLeaveHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加预览拖拽离开事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddPreviewDragLeaveHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.PreviewDragLeaveEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddPreviewDragLeaveHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加拖拽悬停事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddDragOverHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.AddDragOverHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加拖拽悬停事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddDragOverHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.DragOverEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddDragOverHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加预览拖拽悬停事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddPreviewDragOverHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.AddPreviewDragOverHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加预览拖拽悬停事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddPreviewDragOverHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.PreviewDragOverEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddPreviewDragOverHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加放置事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddDropHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.AddDropHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加放置事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddDropHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.DropEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddDropHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加预览放置事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddPreviewDropHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.AddPreviewDropHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加预览放置事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddPreviewDropHandler(DependencyObject element, DragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.PreviewDropEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddPreviewDropHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加反馈事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
    {
        DragDropManager.AddGiveFeedbackHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加反馈事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.GiveFeedbackEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddGiveFeedbackHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加预览反馈事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddPreviewGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
    {
        DragDropManager.AddPreviewGiveFeedbackHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加预览反馈事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddPreviewGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.PreviewGiveFeedbackEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddPreviewGiveFeedbackHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加查询继续拖拽事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
    {
        DragDropManager.AddQueryContinueDragHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加查询继续拖拽事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.QueryContinueDragEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddQueryContinueDragHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 为指定元素添加预览查询继续拖拽事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddPreviewQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
    {
        DragDropManager.AddPreviewQueryContinueDragHandler(element, handler, false);
    }

    /// <summary>
    /// 为指定元素添加预览查询继续拖拽事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddPreviewQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.PreviewQueryContinueDragEvent, handler, handledEventsToo);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.AddPreviewQueryContinueDragHandler(element, handler, handledEventsToo);
        }
    }

    /// <summary>
    /// 添加拖拽完成事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void AddDragDropCompletedHandler(DependencyObject element, DragDropCompletedEventHandler handler)
    {
        DragDropManager.AddDragDropCompletedHandler(element, handler, false);
    }

    /// <summary>
    /// 添加拖拽完成事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    /// <param name="handledEventsToo">是否处理已标记为已处理的事件.</param>
    public static void AddDragDropCompletedHandler(DependencyObject element, DragDropCompletedEventHandler handler, bool handledEventsToo)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.DragDropCompletedEvent, handler, handledEventsToo);
    }

    /// <summary>
    /// 移除拖拽完成事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemoveDragDropCompletedHandler(DependencyObject element, DragDropCompletedEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.DragDropCompletedEvent, handler);
    }

    /// <summary>
    /// 移除拖拽进入事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemoveDragEnterHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.DragEnterEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemoveDragEnterHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除预览拖拽进入事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemovePreviewDragEnterHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.PreviewDragEnterEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemovePreviewDragEnterHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除拖拽离开事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemoveDragLeaveHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.DragLeaveEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemoveDragLeaveHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除预览拖拽离开事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemovePreviewDragLeaveHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.PreviewDragLeaveEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemovePreviewDragLeaveHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除拖拽悬停事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemoveDragOverHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.DragOverEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemoveDragOverHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除预览拖拽悬停事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemovePreviewDragOverHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.PreviewDragOverEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemovePreviewDragOverHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除放置事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemoveDropHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.DropEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemoveDropHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除预览放置事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemovePreviewDropHandler(DependencyObject element, DragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.PreviewDropEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemovePreviewDropHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除提供反馈事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemoveGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.GiveFeedbackEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemoveGiveFeedbackHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除预览提供反馈事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemovePreviewGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.PreviewGiveFeedbackEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemovePreviewGiveFeedbackHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除查询继续拖拽事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemoveQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.QueryContinueDragEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemoveQueryContinueDragHandler(element, handler);
        }
    }

    /// <summary>
    /// 移除预览查询继续拖拽事件处理程序.
    /// </summary>
    /// <param name="element">目标依赖对象.</param>
    /// <param name="handler">处理程序委托.</param>
    public static void RemovePreviewQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.PreviewQueryContinueDragEvent, handler);
        if (DragDropManager.IsFullTrust)
        {
            DelegateHelper.RemovePreviewQueryContinueDragHandler(element, handler);
        }
    }

    /// <summary>
    /// Gets 是否允许启动拖拽操作.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <returns>返回是否允许拖拽. </returns>
    public static bool GetAllowDrag(DependencyObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        return (bool)obj.GetValue(DragDropManager.AllowDragProperty);
    }

    /// <summary>
    /// Sets 是否允许启动拖拽操作.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <param name="value">值.</param>
    public static void SetAllowDrag(DependencyObject obj, bool value)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        obj.SetValue(DragDropManager.AllowDragProperty, value);
    }

    /// <summary>
    /// Gets 触控拖拽的触发模式.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <returns>返回触发器枚举. </returns>
    [Obsolete("Obsoleted with 2017 R1. Use BgControls.Windows.Input.Touch.TouchManager's DragStartTrigger instead.", false)]
    public static TouchDragTrigger GetTouchDragTrigger(DependencyObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        return (TouchDragTrigger)obj.GetValue(DragDropManager.TouchDragTriggerProperty);
    }

    /// <summary>
    /// Sets 触控拖拽的触发模式.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <param name="value">值.</param>
    [Obsolete("Obsoleted with 2017 R1. Use BgControls.Windows.Input.Touch.TouchManager's DragStartTrigger instead.", false)]
    public static void SetTouchDragTrigger(DependencyObject obj, TouchDragTrigger value)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        obj.SetValue(DragDropManager.TouchDragTriggerProperty, value);
    }

    /// <summary>
    /// Gets 是否允许对已捕获触控的对象启动拖拽.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <returns>返回是否允许. </returns>
    public static bool GetAllowCapturedDrag(DependencyObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        return (bool)obj.GetValue(DragDropManager.AllowCapturedDragProperty);
    }

    /// <summary>
    /// Sets 是否允许对已捕获触控的对象启动拖拽.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <param name="value">值.</param>
    public static void SetAllowCapturedDrag(DependencyObject obj, bool value)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        obj.SetValue(DragDropManager.AllowCapturedDragProperty, value);
    }

    /// <summary>
    /// 执行拖拽放置操作（默认无视觉偏移）.
    /// </summary>
    /// <param name="dragSource">拖拽源.</param>
    /// <param name="data">数据.</param>
    /// <param name="allowedEffects">允许的效果.</param>
    /// <param name="initialKeyState">初始按键状态.</param>
    public static void DoDragDrop(DependencyObject dragSource, object data, DragDropEffects allowedEffects, DragDropKeyStates initialKeyState)
    {
        DragDropManager.DoDragDrop(dragSource, data, allowedEffects, initialKeyState, null, default(Point), default(Point));
    }

    /// <summary>
    /// 执行拖拽放置操作，支持自定义视觉对象和坐标偏移.
    /// </summary>
    /// <param name="dragSource">拖拽源.</param>
    /// <param name="data">数据.</param>
    /// <param name="allowedEffects">允许的效果.</param>
    /// <param name="initialKeyState">初始按键状态.</param>
    /// <param name="dragVisual">拖拽视觉对象.</param>
    /// <param name="relativeStartPoint">相对起始点.</param>
    /// <param name="dragVisualOffset">拖拽视觉偏移.</param>
    public static void DoDragDrop(DependencyObject dragSource, object data, DragDropEffects allowedEffects, DragDropKeyStates initialKeyState, object? dragVisual, Point relativeStartPoint, Point dragVisualOffset)
    {
        // 验证拖拽源是否有效.
        if (dragSource is not IInputElement)
        {
            throw new ArgumentException("dragSource should be IInputElement.");
        }

        // 如果环境支持完全信任且不强制使用装饰层，则尝试执行系统原生拖拽.
        if (DragDropManager.IsFullTrust && !DragDropManager.UseAdornerLayer)
        {
            try
            {
                // 挂载原生查询事件以实时更新视觉窗口位置.
                dragSource.AddHandler(System.Windows.DragDrop.PreviewQueryContinueDragEvent, new System.Windows.QueryContinueDragEventHandler(DragDropManager.PreviewQueryContinueDragWindow), true);
                DragDropManager.isInNativeDrag = true;
                DragDropManager.InitialDragPosition = relativeStartPoint;
                DragDropManager.DragVisualOffset = dragVisualOffset;

                // 创建并显示透明拖拽窗口.
                DragDropManager.CreateDragDropWindow(dragVisual);
                DragDropManager.dragDropWindow?.Show();
                DragDropManager.overrideCursor = Mouse.OverrideCursor;

                // 调用系统底层的拖拽接口.
                DragDropEffects effects = System.Windows.DragDrop.DoDragDrop(dragSource, data, allowedEffects);

                // 拖拽结束后清理资源.
                System.Windows.DragDrop.RemovePreviewQueryContinueDragHandler(dragSource, DragDropManager.PreviewQueryContinueDragWindow);
                DragDropManager.DestroyDragDropWindow();

                // 触发拖拽完成事件.
                DragDropCompletedEventArgs completeArgs = new DragDropCompletedEventArgs(DragDropManager.DragDropCompletedEvent);
                completeArgs.Data = data;
                completeArgs.Effects = effects;
                dragSource.RaiseEvent(completeArgs);
                return;
            }
            catch (COMException)
            {
                // 捕获 COM 异常并标记拖拽为失败.
                System.Windows.DragDrop.RemovePreviewQueryContinueDragHandler(dragSource, DragDropManager.PreviewQueryContinueDragWindow);
                DragDropManager.DestroyDragDropWindow();
                DragDropCompletedEventArgs errorArgs = new DragDropCompletedEventArgs(DragDropManager.DragDropCompletedEvent);
                errorArgs.Data = data;
                errorArgs.Effects = DragDropEffects.None;
                dragSource.RaiseEvent(errorArgs);
                return;
            }
            finally
            {
                // 确保不论成败都重置状态标记.
                DragDropManager.isInNativeDrag = false;
                DragDropManager.DestroyDragDropWindow();
                DragDropManager.FinishDrag();
            }
        }

        // 环境不支持系统原生拖拽时，回退到自定义实现的 DragOperation.
        DragDropManager.overrideCursor = Mouse.OverrideCursor;
        DragDropManager.DragOperation = DragOperation.DoDragDrop(dragSource, data, allowedEffects, initialKeyState, dragVisual, relativeStartPoint, dragVisualOffset);
    }

    /// <summary>
    /// 添加拖拽提示位置变更的事件处理程序.
    /// </summary>
    /// <param name="element">依赖对象.</param>
    /// <param name="handler">处理程序.</param>
    internal static void AddDragCuePositionEventHandler(DependencyObject element, EventHandler<DragCuePositionEventArgs> handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.AddHandler(DragDropManager.DragCuePositionEvent, handler, true);
    }

    /// <summary>
    /// 移除拖拽提示位置变更的事件处理程序.
    /// </summary>
    /// <param name="element">依赖对象.</param>
    /// <param name="handler">处理程序.</param>
    internal static void RemoveDragCuePositionEventHandler(DependencyObject element, EventHandler<DragCuePositionEventArgs> handler)
    {
        DragDropManager.CheckNotNull(element, handler);
        element.RemoveHandler(DragDropManager.DragCuePositionEvent, handler);
    }

    /// <summary>
    /// 验证拖拽动作枚举值是否合法.
    /// </summary>
    /// <param name="dragAction">拖拽动作.</param>
    /// <returns>返回是否合法. </returns>
    internal static bool IsValidDragAction(DragAction dragAction)
    {
        return dragAction == DragAction.Continue || dragAction == DragAction.Drop || dragAction == DragAction.Cancel;
    }

    /// <summary>
    /// 结束整个拖拽流程，恢复光标并清空坐标状态.
    /// </summary>
    internal static void FinishDrag()
    {
        // 恢复鼠标光标.
        Mouse.OverrideCursor = DragDropManager.overrideCursor;
        DragDropManager.overrideCursor = null;

        // 重置坐标数据.
        DragDropManager.InitialDragPosition = default(Point);
        DragDropManager.DragVisualOffset = default(Point);
        DragDropManager.DragCueOffset = default(Point);

        // 重置状态位.
        DragDropManager.IsInTouchDrag = false;
        DragDropManager.DragOperation = null;
    }

    /// <summary>
    /// 获取元素的拖拽初始化器.
    /// </summary>
    /// <param name="element">元素.</param>
    /// <returns>初始化器对象. </returns>
    internal static DragInitializer? GetDragInitializer(UIElement element)
    {
        return element.GetValue(DragDropManager.DragInitializerProperty) as DragInitializer;
    }

    /// <summary>
    /// 设置元素的拖拽初始化器.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <param name="value">初始化器.</param>
    internal static void SetDragInitializer(DependencyObject obj, DragInitializer value)
    {
        obj.SetValue(DragDropManager.DragInitializerProperty, value);
    }

    /// <summary>
    /// 获取依赖对象的拖拽初始化器（强转版本）.
    /// </summary>
    /// <param name="obj">依赖对象.</param>
    /// <returns>初始化器对象. </returns>
    internal static DragInitializer? GetDragInitializer(DependencyObject obj)
    {
        return (DragInitializer?)obj.GetValue(DragDropManager.DragInitializerProperty);
    }

    /// <summary>
    /// 当 AllowDrag 属性更改时的回调逻辑.
    /// </summary>
    private static void OnAllowDragPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        bool isAllowed = (bool)e.NewValue;
        DragInitializer? initializer = DragDropManager.GetDragInitializer(d);

        if (isAllowed)
        {
            // 如果允许拖拽且初始化器不存在，则创建并初始化.
            if (initializer == null)
            {
                initializer = new DragInitializer();
            }

            initializer.Initialize(element);
            DragDropManager.SetDragInitializer(element, initializer);
        }
        else
        {
            // 如果禁止拖拽，清理已有的初始化器.
            initializer?.Clear();
            element.ClearValue(DragDropManager.DragInitializerProperty);
        }
    }

    /// <summary>
    /// 处理过时的触控拖拽触发器属性变更，映射到现代触控管理器.
    /// </summary>
    private static void OnTouchDragTriggerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && (DragDropManager.GetAllowDrag(element) || DragDropManager.GetAllowCapturedDrag(element)))
        {
            // 确保初始化器已就绪.
            DragInitializer? initializer = DragDropManager.GetDragInitializer(d);
            if (initializer == null)
            {
                initializer = new DragInitializer();
                DragDropManager.SetDragInitializer(element, initializer);
            }

            initializer.Initialize(element);
        }

        // 根据旧版枚举值设置新版触控管理器的触发策略.
        switch ((TouchDragTrigger)e.NewValue)
        {
            case TouchDragTrigger.TapDown:
                TouchManager.SetDragStartTrigger(d, TouchDragStartTrigger.TouchMove);
                break;
            case TouchDragTrigger.TapAndHold:
                TouchManager.SetDragStartTrigger(d, TouchDragStartTrigger.TapHoldAndMove);
                break;
        }
    }

    /// <summary>
    /// 当允许捕获拖拽属性变更时同步到普通拖拽属性.
    /// </summary>
    private static void OnAllowCapturedDragPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement)
        {
            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;
            if (newValue != oldValue)
            {
                DragDropManager.SetAllowDrag(d, newValue);
            }
        }
    }

    /// <summary>
    /// 内部非空检查辅助方法.
    /// </summary>
    private static void CheckNotNull(DependencyObject element, Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
    }

    /// <summary>
    /// 创建系统原生拖拽时显示的透明跟随窗口.
    /// </summary>
    /// <param name="content">要显示的内容对象.</param>
    private static void CreateDragDropWindow(object? content)
    {
        Window window = new Window
        {
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            AllowDrop = false,
            Background = null,
            IsHitTestVisible = false,
            SizeToContent = SizeToContent.WidthAndHeight,
            Topmost = true,
            ShowInTaskbar = false,
        };

        DragDropManager.dragDropWindow = window;

        // 在窗口句柄初始化完成后，设置为鼠标穿透透明样式.
        EventHandler? sourceInitializedHandler = null;
        sourceInitializedHandler = (sender, args) =>
        {
            if (DragDropManager.dragDropWindow != null)
            {
                DragDropManager.dragDropWindow.SourceInitialized -= sourceInitializedHandler;
                NativeWindowStyleWrapper.SetTransparentStyle(DragDropManager.dragDropWindow);
                DragDropManager.UpdateWindowLocation();
            }
        };

        DragDropManager.dragDropWindow.SourceInitialized += sourceInitializedHandler;
        DragDropManager.dragDropWindow.Content = content;
        // 执行初次位置同步.
        DragDropManager.UpdateWindowLocation();
    }

    /// <summary>
    /// 同步视觉跟随窗口到当前的鼠标坐标位置.
    /// </summary>
    private static void UpdateWindowLocation()
    {
        if (DragDropManager.dragDropWindow != null && NativeMouseWrapper.GetCursorPos(out Win32Point screenPoint))
        {
            // 考虑 DPI 缩放换算坐标.
            double scale = DpiHelper.GetPerMonitorDPIAwareScaleFactor(DragDropManager.dragDropWindow);
            Point logicalPoint = DpiHelper.ScaledDevicePixelsToLogical(new Point(screenPoint.X, screenPoint.Y), scale);

            // 根据偏移量计算最终窗口位置.
            DragDropManager.dragDropWindow.Left = logicalPoint.X + DragDropManager.DragVisualOffset.X + DragDropManager.DragCueOffset.X - DragDropManager.InitialDragPosition.X;
            DragDropManager.dragDropWindow.Top = logicalPoint.Y + DragDropManager.DragVisualOffset.Y + DragDropManager.DragCueOffset.Y - DragDropManager.InitialDragPosition.Y;
        }
    }

    /// <summary>
    /// 彻底关闭并注销拖拽跟随窗口.
    /// </summary>
    private static void DestroyDragDropWindow()
    {
        if (DragDropManager.dragDropWindow != null && !DragDropManager.IsDragInProgress)
        {
            DragDropManager.dragDropWindow.Close();
            DragDropManager.dragDropWindow = null;
        }
    }

    /// <summary>
    /// 在原生拖拽过程中实时更新跟随窗口的位置.
    /// </summary>
    private static void PreviewQueryContinueDragWindow(object sender, System.Windows.QueryContinueDragEventArgs e)
    {
        if (sender is DependencyObject dependencyObject)
        {
            // 触发提示位置变更事件并刷新窗口定位.
            DragCuePositionEventArgs cueArgs = new DragCuePositionEventArgs(DragDropManager.DragCuePositionEvent);
            dependencyObject.RaiseEvent(cueArgs);
            DragDropManager.DragCueOffset = cueArgs.DragCueOffset;
            DragDropManager.UpdateWindowLocation();
        }
    }
}