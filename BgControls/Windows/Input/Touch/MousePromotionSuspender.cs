namespace BgControls.Windows.Input.Touch;

internal class MousePromotionSuspender
{
    private int touchId;

    private FrameworkElement root;

    private MousePromotionSuspender(FrameworkElement element, int touchId)
    {
        this.touchId = touchId;
        root = Controls.ToolTipService.GetRootVisual(element) ?? element;
    }

    internal static void SuspendMousePromotionUntilTouchUp(FrameworkElement element, int touchId)
    {
        MousePromotionSuspender mousePromotionSuspender = new MousePromotionSuspender(element, touchId);
        mousePromotionSuspender.AttachHandlers();
    }

    private void AttachHandlers()
    {
        root.PreviewMouseDown += Root_PreviewMouseDown;
        root.PreviewMouseMove += Root_PreviewMouseMove;
        root.AddHandler(UIElement.TouchUpEvent, new EventHandler<System.Windows.Input.TouchEventArgs>(Root_TouchUp), handledEventsToo: true);
    }

    private void DetachHandlers()
    {
        root.PreviewMouseDown -= Root_PreviewMouseDown;
        root.PreviewMouseMove -= Root_PreviewMouseMove;
        root.RemoveHandler(UIElement.TouchUpEvent, new EventHandler<System.Windows.Input.TouchEventArgs>(Root_TouchUp));
    }

    private void Root_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (TouchManager.IsTouchInput(e.StylusDevice, sender as UIElement, e.OriginalSource as UIElement) && touchId == e.StylusDevice.Id)
        {
            e.Handled = true;
        }
    }

    private void Root_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (TouchManager.IsTouchInput(e.StylusDevice, sender as UIElement, e.OriginalSource as UIElement) && touchId == e.StylusDevice.Id)
        {
            e.Handled = true;
        }
    }

    private void Root_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        root.RemoveHandler(UIElement.PreviewMouseUpEvent, new MouseButtonEventHandler(Root_PreviewMouseUp));
    }

    private void Root_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
    {
        if (touchId == e.TouchDevice.Id)
        {
            DetachHandlers();
            root.AddHandler(UIElement.PreviewMouseUpEvent, new MouseButtonEventHandler(Root_PreviewMouseUp), handledEventsToo: true);
        }
    }

    internal static void OnShouldSuspendMousePromotionChanged(UIElement element, bool shouldSuspend)
    {
        TouchManager.RemoveTouchDownEventHandler(element, Element_TouchDown_ShouldSuspendMousePromotion);
        element.PreviewMouseMove -= Element_PreviewMouseMove_ShouldSuspendMousePromotion;
        if (shouldSuspend)
        {
            TouchManager.AddTouchDownEventHandler(element, Element_TouchDown_ShouldSuspendMousePromotion);
            element.PreviewMouseMove += Element_PreviewMouseMove_ShouldSuspendMousePromotion;
        }
    }

    private static void Element_TouchDown_ShouldSuspendMousePromotion(object sender, BgControls.Windows.Input.Touch.TouchEventArgs args)
    {
        args.SuspendMousePromotionUntilTouchUp();
    }

    private static void Element_PreviewMouseMove_ShouldSuspendMousePromotion(object sender, MouseEventArgs e)
    {
        if (sender is UIElement element)
        {
            TouchHelper orCreateTouchHelper = TouchManager.GetOrCreateTouchHelper(element);
            if (orCreateTouchHelper.SuspendMouseMovesCount > 0)
            {
                e.Handled = true;
                orCreateTouchHelper.SuspendMouseMovesCount--;
            }
        }
    }
}
