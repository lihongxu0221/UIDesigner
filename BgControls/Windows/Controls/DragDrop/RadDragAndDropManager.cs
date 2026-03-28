namespace BgControls.Windows.Controls.DragDrop;

[Obsolete("Obsoleted due to the replacement with DragDropManager", false)]
public static class RadDragAndDropManager
{
    public static readonly DependencyProperty AllowDragProperty = DependencyProperty.RegisterAttached("AllowDrag", typeof(bool), typeof(RadDragAndDropManager), new PropertyMetadata(false, OnAllowDragChanged));

    public static readonly DependencyProperty AllowDropProperty = DependencyProperty.RegisterAttached("AllowDrop", typeof(bool), typeof(RadDragAndDropManager), new PropertyMetadata(false, OnAllowDropChanged));

    public static readonly DependencyProperty AutoDragProperty = DependencyProperty.RegisterAttached("AutoDrag", typeof(bool), typeof(RadDragAndDropManager), new PropertyMetadata(false));

    public static readonly RoutedEvent DragInfoEvent = EventManager.RegisterRoutedEvent("DragInfo", RoutingStrategy.Bubble, typeof(EventHandler<DragDropEventArgs>), typeof(RadDragAndDropManager));

    public static readonly RoutedEvent DropInfoEvent = EventManager.RegisterRoutedEvent("DropInfo", RoutingStrategy.Bubble, typeof(EventHandler<DragDropEventArgs>), typeof(RadDragAndDropManager));

    public static readonly RoutedEvent DragQueryEvent = EventManager.RegisterRoutedEvent("DragQuery", RoutingStrategy.Bubble, typeof(EventHandler<DragDropQueryEventArgs>), typeof(RadDragAndDropManager));

    public static readonly RoutedEvent DropQueryEvent = EventManager.RegisterRoutedEvent("DropQuery", RoutingStrategy.Bubble, typeof(EventHandler<DragDropQueryEventArgs>), typeof(RadDragAndDropManager));

    public static readonly RoutedEvent DragArrowAdjustingEvent = EventManager.RegisterRoutedEvent("DragArrowAdjusting", RoutingStrategy.Bubble, typeof(EventHandler<DragArrowAdjustingEventArgs>), typeof(RadDragAndDropManager));

    private static bool enableNativeDrag;

    private static DragDropProviderBase dragDropProvider;

    public static bool IsDragging
    {
        get
        {
            return DragDropProvider.IsDragging;
        }

        internal set
        {
            DragDropProvider.IsDragging = value;
        }
    }

    public static DragExecutionMode ExecutionMode { get; set; }

    public static bool EnableNativeDrag
    {
        get
        {
            return enableNativeDrag;
        }

        set
        {
            bool flag = enableNativeDrag != value;
            enableNativeDrag = value;
            if (flag)
            {
                Initialize();
            }
        }
    }

    public static DragDropOptions Options => DragDropProvider.Options;

    public static bool AutoBringIntoView
    {
        get
        {
            return DragDropProvider.AutoBringIntoView;
        }

        set
        {
            DragDropProvider.AutoBringIntoView = value;
        }
    }

    public static double ArrowVisibilityMinimumThreshold
    {
        get
        {
            return DragDropProvider.ArrowVisibilityMinimumThreshold;
        }

        set
        {
            DragDropProvider.ArrowVisibilityMinimumThreshold = value;
        }
    }

    internal static DragDropProviderBase DragDropProvider
    {
        get
        {
            if (dragDropProvider == null)
            {
                Initialize();
            }

            return dragDropProvider;
        }

        set
        {
            dragDropProvider = value;
        }
    }

    public static double DragStartThreshold
    {
        get
        {
            return DragDropProvider.DragStartThreshold;
        }

        set
        {
            if (value < 0.0)
            {
                throw new ArgumentOutOfRangeException("value", "DragStartThreshold cannot be smaller than 0");
            }

            DragDropProvider.DragStartThreshold = value;
        }
    }

    public static Point DragCueOffset
    {
        get
        {
            return DragDropProvider.DragCueOffset;
        }

        set
        {
            DragDropProvider.DragCueOffset = value;
        }
    }

    public static bool GetAllowDrag(DependencyObject obj)
    {
        return (bool)obj.GetValue(AllowDragProperty);
    }

    public static void SetAllowDrag(DependencyObject obj, bool value)
    {
        obj.SetValue(AllowDragProperty, value);
    }

    public static bool GetAllowDrop(DependencyObject obj)
    {
        return (bool)obj.GetValue(AllowDropProperty);
    }

    public static void SetAllowDrop(DependencyObject obj, bool value)
    {
        obj.SetValue(AllowDropProperty, value);
    }

    public static bool GetAutoDrag(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoDragProperty);
    }

    public static void SetAutoDrag(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoDragProperty, value);
    }

    public static void Initialize()
    {
        if (dragDropProvider != null)
        {
            UnsubscribeFromProviderEvents(dragDropProvider);
            dragDropProvider.Dispose();
        }

        bool flag = EnableNativeDrag;
        dragDropProvider = DragDropProviderBase.Create(flag, ExecutionMode);
        SubscribeToProviderEvents(dragDropProvider);
    }

    private static void DragDropProvider_DragQuery(object sender, DragDropQueryEventArgs e)
    {
        Options.Source.RaiseEvent(e);
    }

    private static void DragDropProvider_DragInfo(object sender, DragDropEventArgs e)
    {
        Options.Source.RaiseEvent(e);
    }

    private static void DragDropProvider_DropQuery(object sender, DragDropEventArgs e)
    {
        Options.Destination.RaiseEvent(e);
    }

    private static void DragDropProvider_DropInfo(object sender, DragDropEventArgs e)
    {
        Options.Destination.RaiseEvent(e);
    }

    private static void DragDropProvider_DragArrowAdjusting(object sender, DragArrowAdjustingEventArgs e)
    {
        if (Options.Source != null)
        {
            Options.Source.RaiseEvent(e);
        }
    }

    internal static void SubscribeToProviderEvents(DragDropProviderBase dragProvider)
    {
        dragProvider.DragInfo += DragDropProvider_DragInfo;
        dragProvider.DragQuery += DragDropProvider_DragQuery;
        dragProvider.DropQuery += DragDropProvider_DropQuery;
        dragProvider.DropInfo += DragDropProvider_DropInfo;
        dragProvider.DragArrowAdjusting += DragDropProvider_DragArrowAdjusting;
    }

    internal static void UnsubscribeFromProviderEvents(DragDropProviderBase dragProvider)
    {
        dragProvider.DragInfo -= DragDropProvider_DragInfo;
        dragProvider.DragQuery -= DragDropProvider_DragQuery;
        dragProvider.DropQuery -= DragDropProvider_DropQuery;
        dragProvider.DropInfo -= DragDropProvider_DropInfo;
        dragProvider.DragArrowAdjusting -= DragDropProvider_DragArrowAdjusting;
    }

    public static DragVisualCue GenerateVisualCue()
    {
        return GenerateVisualCue(null);
    }

    public static DragVisualCue GenerateVisualCue(FrameworkElement source)
    {
        return VisualCueHelper.GenerateVisualCue(source);
    }

    public static ContentControl GenerateArrowCue()
    {
        return VisualCueHelper.GenerateArrowCue();
    }

    public static void AddDragQueryHandler(DependencyObject target, EventHandler<DragDropQueryEventArgs> handler)
    {
        (target as UIElement).AddHandler(DragQueryEvent, handler);
    }

    public static void RemoveDragQueryHandler(DependencyObject target, EventHandler<DragDropQueryEventArgs> handler)
    {
        (target as UIElement).RemoveHandler(DragQueryEvent, handler);
    }

    public static void AddDropQueryHandler(DependencyObject target, EventHandler<DragDropQueryEventArgs> handler)
    {
        (target as UIElement).AddHandler(DropQueryEvent, handler);
    }

    public static void RemoveDropQueryHandler(DependencyObject target, EventHandler<DragDropQueryEventArgs> handler)
    {
        (target as UIElement).RemoveHandler(DropQueryEvent, handler);
    }

    public static void AddDragInfoHandler(DependencyObject target, EventHandler<DragDropEventArgs> handler)
    {
        (target as UIElement).AddHandler(DragInfoEvent, handler);
    }

    public static void RemoveDragInfoHandler(DependencyObject target, EventHandler<DragDropEventArgs> handler)
    {
        (target as UIElement).RemoveHandler(DragInfoEvent, handler);
    }

    public static void AddDropInfoHandler(DependencyObject target, EventHandler<DragDropEventArgs> handler)
    {
        (target as UIElement).AddHandler(DropInfoEvent, handler);
    }

    public static void RemoveDropInfoHandler(DependencyObject target, EventHandler<DragDropEventArgs> handler)
    {
        (target as UIElement).RemoveHandler(DropInfoEvent, handler);
    }

    public static void AddDragArrowAdjustingHandler(DependencyObject target, EventHandler<DragArrowAdjustingEventArgs> handler)
    {
        (target as UIElement).AddHandler(DragArrowAdjustingEvent, handler);
    }

    public static void RemoveDragArrowAdjustingHandler(DependencyObject target, EventHandler<DragArrowAdjustingEventArgs> handler)
    {
        (target as UIElement).RemoveHandler(DragArrowAdjustingEvent, handler);
    }

    public static void StartDrag(FrameworkElement dragSource, object payload, object dragCue)
    {
        DragDropProvider.StartDrag(dragSource, payload, dragCue);
    }

    public static void CancelDrag()
    {
        DragDropProvider.CancelDrag();
    }

    internal static void OnAllowDragChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        DragDropProvider.SetAllowDrag(sender, (bool)e.NewValue);
    }

    internal static void OnAllowDropChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        DragDropProvider.SetAllowDrop(sender, (bool)e.NewValue);
    }
}
