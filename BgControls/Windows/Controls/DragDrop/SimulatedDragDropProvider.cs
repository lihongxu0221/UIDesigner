namespace BgControls.Windows.Controls.DragDrop;

internal abstract class SimulatedDragDropProvider : DragDropProviderBase
{
    private WeakReference coverRectangle = new WeakReference(null);

    private WeakReference rootVisual = new WeakReference(null);

    private bool isMouseDown;

    private bool isDragCuePositioned;

    internal Grid? CoverRectangle
    {
        get
        {
            return coverRectangle.Target as Grid;
        }

        set
        {
            coverRectangle = new WeakReference(value);
        }
    }

    internal Point MouseClickPoint { get; set; }

    internal Point RelativeClick { get; set; }

    internal virtual bool IsInitialized { get; set; }

    internal virtual bool IsMouseDown
    {
        get
        {
            return isMouseDown;
        }

        set
        {
            isMouseDown = value;
        }
    }

    internal FrameworkElement? RootVisual
    {
        get
        {
            return rootVisual.Target as FrameworkElement;
        }

        set
        {
            if (rootVisual.Target != null && rootVisual.IsAlive)
            {
                RemoveRootVisualHandlers(rootVisual.Target as FrameworkElement);
            }

            rootVisual = new WeakReference(value);
            if (value != null)
            {
                AddRootVisualHandlers(value);
            }
        }
    }

    protected Point InitialRelativeDragPosition { get; set; }

    protected List<UIElement>? ElementsUnderTheMouse { get; set; }

    public SimulatedDragDropProvider()
    {
    }

    public override void SetAllowDrag(DependencyObject obj, bool value)
    {
        if (!RadControl.IsInDesignMode)
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            UIElement uIElement = obj as UIElement;
            if (value)
            {
                uIElement.LostMouseCapture -= OnSourceLostMouseCapture;
                uIElement.LostMouseCapture += OnSourceLostMouseCapture;
            }
            else
            {
                uIElement.LostMouseCapture -= OnSourceLostMouseCapture;
            }
        }
    }

    public override void SetAllowDrop(DependencyObject obj, bool value)
    {
    }

    internal override void Initialize()
    {
        if (!RadControl.IsInDesignMode)
        {
            base.Initialize();
            FindRootVisual();
            IsInitialized = true;
        }
    }

    protected virtual void OnSourceLostMouseCaptureInternal(object sender, MouseEventArgs e)
    {
        if (this.IsDragging)
        {
            CancelDrag();
        }
    }

    protected virtual void AddRootVisualHandlers(FrameworkElement root)
    {
        if (root != null)
        {
            root.KeyDown += OnRootVisualKeyDown;
            root.KeyUp += OnRootVisualKeyUp;
            root.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnCoverRectangleMouseLeftButtonUp), handledEventsToo: true);
        }
    }

    protected virtual void RemoveRootVisualHandlers(FrameworkElement root)
    {
        root.KeyDown -= OnRootVisualKeyDown;
        root.KeyUp -= OnRootVisualKeyUp;
        root.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnCoverRectangleMouseLeftButtonUp));
    }

    internal override void Reset()
    {
        if (this.Options.ParticipatingVisualRoots != null)
        {
            foreach (UIElement participatingVisualRoot in base.Options.ParticipatingVisualRoots)
            {
                if (participatingVisualRoot != null)
                {
                    participatingVisualRoot.MouseMove -= OnCoverRectangleMouseMove;
                }
            }

            this.Options.ParticipatingVisualRoots.Clear();
        }

        ElementsUnderTheMouse = null;
        VisualCueHelper.RemovePreviousArrowCue();
        isDragCuePositioned = false;
        base.Reset();
    }

    public override void CancelDrag()
    {
        if (this.IsDragging)
        {
            CancelDragging();
        }
        else
        {
            Reset();
        }
    }

    public override void StartDrag(FrameworkElement dragSource, object payload, object dragCue)
    {
        if (!this.IsDragging)
        {
            ArgumentNullException.ThrowIfNull(dragSource, nameof(dragSource));

            if (!IsInitialized)
            {
                Initialize();
            }

            this.Options.Source = dragSource;
            this.Options.Payload = payload;
            this.Options.DragCue = dragCue;
            StartDragging();
        }
    }

    protected void OnCoverRectangleMouseLeftButtonUpInternal()
    {
        if (this.Options.Status != DragStatus.DropPossible)
        {
            if (this.IsDragging)
            {
                CancelDragging();
            }
            else
            {
                Reset();
            }
        }
        else
        {
            OnDrop();
        }
    }

    private void OnSourceLostMouseCapture(object sender, MouseEventArgs e)
    {
        OnSourceLostMouseCaptureInternal(sender, e);
    }

    internal virtual void OnTrackedPartMouseLeftButtonDownInternal(object sender, IMouseButtonEventArgs e)
    {
        var frameworkElement = e.OriginalSource as FrameworkElement;
        this.Options = GetDefaultOptions();
        if (frameworkElement != null && RadDragAndDropManager.GetAllowDrag(frameworkElement))
        {
            this.Options.Source = (FrameworkElement)e.OriginalSource;
        }
        else if (e.OriginalSource is DependencyObject)
        {
            this.Options.Source = GetFirstDraggableParent((DependencyObject)e.OriginalSource);
        }

        UpdateMousePointInformation(e, this.Options.Source);
    }

    private static FrameworkElement? GetFirstDraggableParent(DependencyObject originalSource)
    {
        if (originalSource == null)
        {
            return null;
        }

        if (VisualTreeHelper.GetParent(originalSource) is not FrameworkElement frameworkElement)
        {
            return null;
        }

        if ((bool)frameworkElement.GetValue(RadDragAndDropManager.AllowDragProperty))
        {
            return frameworkElement;
        }

        return GetFirstDraggableParent(frameworkElement);
    }

    protected void UpdateMousePointInformation(IMouseButtonEventArgs e, FrameworkElement element)
    {
        try
        {
            MouseClickPoint = e.GetPosition(ApplicationHelper.GetRootVisual(element));
            RelativeClick = e.GetPosition(element);
        }
        catch
        {
            Reset();
        }

        this.Options.CurrentDragPoint = MouseClickPoint;
    }

    private void OnRootVisualKeyUp(object sender, KeyEventArgs e)
    {
        if (this.IsDragging)
        {
            e.Handled = true;
        }
    }

    private void OnRootVisualKeyDown(object sender, KeyEventArgs e)
    {
        if (this.IsDragging)
        {
            if (e.Key == Key.Escape)
            {
                CancelDragging();
            }

            e.Handled = true;
        }
    }

    private void OnCoverRectangleMouseLeftButtonUp(object sender, EventArgs e)
    {
        OnCoverRectangleMouseLeftButtonUpInternal();
    }

    internal virtual void OnDrop()
    {
        this.Options.Status = DragStatus.DragComplete;
        RaiseDragInfo();
        this.Options.Status = DragStatus.DropComplete;
        RaiseDropInfo();
        this.IsDragging = false;
        this.Options.Status = DragStatus.None;
        Reset();
    }

    internal virtual void OnCoverRectangleMouseMoveInternal(IMouseEventArgs e)
    {
        if (IsMouseDown && this.Options.Source != null)
        {
            if (this.IsDragging)
            {
                OnRealDrag(e);
            }
            else
            {
                OnTrackedElementMouseMoveInternal(e);
            }
        }
        else
        {
            Reset();
        }
    }

    private void OnCoverRectangleMouseMove(object sender, MouseEventArgs e)
    {
        OnCoverRectangleMouseMoveInternal(new TestableMouseEventArgs(e));
    }

    protected void OnTrackedElementMouseMoveInternal(IMouseEventArgs e)
    {
        if (CanStartDrag() && IsMouseDown && !base.IsDragging)
        {
            TryStartDrag(e);
        }
    }

    internal abstract bool CanStartDrag();

    internal virtual void TryStartDrag(IMouseEventArgs e)
    {
        if (!this.IsDragging)
        {
            isDragCuePositioned = false;
            this.Options.CurrentDragPoint = e.GetPosition(null);
            if (this.Options.Source != null &&
                (this.Options.Source == null || !ArePointsNear(e.GetPosition(this.Options.Source), RelativeClick)))
            {
                this.Options.MouseClickPoint = MouseClickPoint;
                InitialRelativeDragPosition = e.GetPosition(this.Options.Source);
                this.Options.ArrowCue = null;
                this.Options.DragCue = null;
                this.Options.Status = DragStatus.DragQuery;
            }
        }
    }

    protected bool ArePointsNear(Point currentRelativeMousePoint)
    {
        return ArePointsNear(currentRelativeMousePoint, RelativeClick);
    }

    protected void StartDragging()
    {
        this.IsDragging = true;
        this.Options.Status = DragStatus.DragInProgress;
        RaiseDragInfo();
        StartDraggingInternal();
    }

    internal abstract void StartDraggingInternal();

    internal virtual void FindRootVisual()
    {
    }

    protected abstract void CancelDragging();

    private void OnRealDrag(IMouseEventArgs e)
    {
        DragStatus status = this.Options.Status;
        this.Options.CurrentDragPoint = e.GetPosition(null);
        PositionPopup();
        PositionArrow();
        FindDropZones(out var dropZones);
        if (this.Options.CurrentCursor != null)
        {
            SetCursor();
        }

        bool flag = false;
        foreach (FrameworkElement item in dropZones)
        {
            this.Options.Status = DragStatus.DropDestinationQuery;
            this.Options.Destination = item;
            this.Options.RelativeDragPoint = e.GetPosition(item);
            bool? queryResult = RaiseDropQuery().QueryResult;
            bool? flag2 = null;
            if (queryResult == true)
            {
                this.Options.Status = DragStatus.DropSourceQuery;
                flag2 = RaiseDragQuery().QueryResult;
            }

            bool? flag3 = queryResult;
            if (flag3 == true && flag3.HasValue && flag2 == true)
            {
                this.Options.Status = DragStatus.DropPossible;
                RaiseDropInfo();
                RaiseDragInfo();
                OnDropPossible();
                flag = true;
                break;
            }

            this.Options.Status = DragStatus.DropImpossible;
            if (status != DragStatus.DropImpossible)
            {
                OnDropImpossible();
            }

            if (queryResult == false || flag2 == false)
            {
                break;
            }
        }

        foreach (IScrollViewerAdapter item2 in this.ScrollViewersToAdjust)
        {
            AdjustScrollViewer(item2, this.Options.CurrentDragPoint);
        }

        if (this.ScrollViewersToAdjust.Any())
        {
            this.PreviousScrollAdjustPosition = this.Options.CurrentDragPoint;
            if (this.ScrollViewerScrollTimer != null)
            {
                this.ScrollViewerScrollTimer.Start();
            }
        }
        else
        {
            this.ScrollViewerScrollTimer.Stop();
        }

        if (!flag)
        {
            OnDropImpossible();
        }
    }

    protected override void NotifyNotApprovedDestination()
    {
        var item = this.LastNotApprovedDestination ?? this.Options.Destination;
        if (ElementsUnderTheMouse != null && !ElementsUnderTheMouse.Contains(item))
        {
            base.NotifyNotApprovedDestination();
        }
    }

    protected override void AdjustScrollViewer(IScrollViewerAdapter viewer, Point currentPoint)
    {
        if (CanManage(RootVisual, viewer.OriginalUIElement.Dispatcher.Thread))
        {
            GeneralTransform generalTransform = viewer.OriginalUIElement.TransformToVisualSafe(RootVisual);
            Point topLeftViewerPoint = generalTransform.Transform(new Point(0.0, 0.0));
            AdjustScrollViewer(viewer, topLeftViewerPoint, currentPoint);
        }
    }

    internal static bool CanManage(DependencyObject obj, Thread thread)
    {
        return obj.Dispatcher.Thread.ManagedThreadId == thread.ManagedThreadId;
    }

    protected void FindDropZones(out IList<FrameworkElement> dropZones)
    {
        ElementsUnderTheMouse = GetElementsUnderMouse();
        List<FrameworkElement> list = new List<FrameworkElement>(4);
        this.ScrollViewersToAdjust?.Clear();
        foreach (UIElement item in ElementsUnderTheMouse)
        {
            if (RadDragAndDropManager.GetAllowDrop(item))
            {
                list.Add(item as FrameworkElement);
            }

            ScrollViewer scrollViewer = item as ScrollViewer;
            if (this.AutoBringIntoView && scrollViewer != null)
            {
                this.ScrollViewersToAdjust.Add(new DefaultScrollViewerAdapter(scrollViewer));
            }

            IScrollViewerAdapter scrollViewerAdapter = item as IScrollViewerAdapter;
            if (this.AutoBringIntoView && scrollViewerAdapter != null)
            {
                this.ScrollViewersToAdjust.Add(scrollViewerAdapter);
            }

            if (item.GetType().Name == "RadWindow")
            {
                break;
            }
        }

        dropZones = list;
    }

    protected abstract List<UIElement> GetElementsUnderMouse();

    internal void PositionArrow()
    {
        if (this.Options.ArrowCue == null || CoverRectangle == null)
        {
            return;
        }

        ContentControl arrowCue = this.Options.ArrowCue;
        if (!CoverRectangle.Children.Contains(arrowCue))
        {
            CoverRectangle.Children.Add(arrowCue);
        }

        double num = MouseClickPoint.X - this.Options.CurrentDragPoint.X;
        double num2 = MouseClickPoint.Y - this.Options.CurrentDragPoint.Y;
        double num3 = Math.Sqrt(num * num + num2 * num2);
        if (num3 > this.ArrowVisibilityMinimumThreshold)
        {
            var transformGroup = arrowCue.RenderTransform as TransformGroup;
            var e = RaiseDragArrowAdjusting(transformGroup);
            if (!e.Cancel)
            {
                var scaleTransform = transformGroup.Children[0] as ScaleTransform;
                var rotateTransform = transformGroup.Children[1] as RotateTransform;
                var translateTransform = transformGroup.Children[2] as TranslateTransform;
                if (translateTransform != null)
                {
                    translateTransform.X = MouseClickPoint.X;
                    translateTransform.Y = MouseClickPoint.Y;
                }

                arrowCue.Width = num3;
                if (rotateTransform != null)
                {
                    if (num != 0.0)
                    {
                        rotateTransform.Angle = Math.Atan(num2 / num) * 180.0 / Math.PI;
                    }
                    else
                    {
                        rotateTransform.Angle = (num2 < 0.0) ? 90 : -90;
                    }
                }

                if (num > 0.0)
                {
                    if (rotateTransform != null)
                    {
                        rotateTransform.Angle += 180.0;
                    }

                    if (scaleTransform != null)
                    {
                        scaleTransform.ScaleY = -1.0;
                    }
                }
                else
                {
                    if (scaleTransform != null)
                    {
                        scaleTransform.ScaleY = 1.0;
                    }
                }
            }

            arrowCue.RenderTransform = e.ArrowTransformation;
            arrowCue.Visibility = Visibility.Visible;
        }
        else
        {
            arrowCue.Visibility = Visibility.Collapsed;
        }
    }

    protected void PositionPopup()
    {
        FrameworkElement dragCueHost = GetDragCueHost();
        if (dragCueHost != null)
        {
            if (!isDragCuePositioned && dragCueHost.ActualHeight != 0.0 && dragCueHost.ActualWidth != 0.0)
            {
                isDragCuePositioned = true;
                RelativeClick = GetRelativeDragCuePosition(RelativeClick, InitialRelativeDragPosition, dragCueHost);
                dragCueHost.Opacity = 1.0;
            }

            var translateTransform = dragCueHost.RenderTransform as TranslateTransform;
            if (translateTransform == null)
            {
                translateTransform = (TranslateTransform)(dragCueHost.RenderTransform = new TranslateTransform());
            }

            translateTransform.Y = this.Options.CurrentDragPoint.Y - RelativeClick.Y;
            translateTransform.X = this.Options.CurrentDragPoint.X - RelativeClick.X;
        }
    }

    internal abstract FrameworkElement? GetDragCueHost();
}
