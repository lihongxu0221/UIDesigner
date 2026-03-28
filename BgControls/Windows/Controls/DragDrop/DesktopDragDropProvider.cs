using BgControls.Windows.Controls;
using BgControls.Windows.Controls.DragDrop.NativeWrappers;
using BgControls.Windows.DragDrop;

namespace BgControls.Windows.Controls.DragDrop;

internal class DesktopDragDropProvider : DragDropProviderBase
{
    private Dictionary<IScrollViewerAdapter, GeneralTransform> transformationsFromRootElement;

    private bool finishingDrag;

    private bool isDropPossible;

    private bool dragInitiated;

    private Dictionary<int, bool> approvedDestinations;

    private Point LastAdjustedPosition { get; set; }

    private Dictionary<IScrollViewerAdapter, GeneralTransform> TransformationsFromRootElement
    {
        get
        {
            if (transformationsFromRootElement == null)
            {
                transformationsFromRootElement = new Dictionary<IScrollViewerAdapter, GeneralTransform>();
            }
            return transformationsFromRootElement;
        }
    }

    internal UndetectableWindow DragPopup { get; set; }

    internal UndetectableWindow ArrowPopup { get; set; }

    internal UIElement CurrentWindowHost { get; set; }

    internal Point AbsoluteClickPoint { get; set; }

    internal Point RelativeClickPoint { get; set; }

    internal FrameworkElement LastClickedElement { get; set; }

    private Point RelativeHostClickPoint { get; set; }

    private bool IsCancellingDrag { get; set; }

    private bool IsDragCuePositioned { get; set; }

    private UIElement CurrentRootForAdjustment { get; set; }

    private Dictionary<UIElement, Point> PreviousRelativeDragPoints { get; set; }

    private List<UIElement> RootElements { get; set; }

    private Point InitialRelativeDragPosition { get; set; }

    private Point LastFeedBackPoint { get; set; }

    private Point LastDragOverPoint { get; set; }

    private bool IsDraggingFromExternalProgram
    {
        get
        {
            if (base.Options.Source == null)
            {
                return base.Options.Destination != null;
            }

            return false;
        }
    }

    public virtual bool ShouldRefreshAutoScroll(Point newMousePosition)
    {
        return false;
    }

    internal IList<UIElement> GetVisibleWindowHosts()
    {
        IList<UIElement> list = new List<UIElement>();
        if (base.Options.ParticipatingVisualRoots != null && base.Options.ParticipatingVisualRoots.Count > 0)
        {
            list = base.Options.ParticipatingVisualRoots.Where((UIElement c) => c.IsVisible).ToList();
        }

        if (CurrentWindowHost != null && CurrentWindowHost.IsVisible && !list.Contains(CurrentWindowHost))
        {
            list.Add(CurrentWindowHost);
        }

        return list;
    }

    internal override void OnAdjustingScrollViewers()
    {
        if (base.IsDragging && base.ScrollViewersToAdjust != null)
        {
            base.PreviousScrollAdjustPosition = base.Options.CurrentDragPoint;
            {
                foreach (IScrollViewerAdapter item in base.ScrollViewersToAdjust)
                {
                    AdjustScrollViewer(item, base.PreviousScrollAdjustPosition);
                }

                return;
            }
        }

        base.ScrollViewerScrollTimer.Stop();
        base.ScrollViewersToAdjust.Clear();
    }

    protected override void AdjustScrollViewer(IScrollViewerAdapter viewer, Point currentPoint)
    {
        GeneralTransform generalTransform = TransformationsFromRootElement[viewer];
        Point topLeftViewerPoint = generalTransform.Transform(new Point(0.0, 0.0));
        AdjustScrollViewer(viewer, topLeftViewerPoint, currentPoint);
    }

    protected void FindScrollViewers()
    {
        if (base.ScrollViewersToAdjust == null)
        {
            base.ScrollViewersToAdjust = new List<IScrollViewerAdapter>();
        }

        RootElements = new List<UIElement>();
        RootElements.Add(CurrentWindowHost);
        if (base.Options.ParticipatingVisualRoots != null)
        {
            foreach (UIElement visibleWindowHost in GetVisibleWindowHosts())
            {
                if (!RootElements.Contains(visibleWindowHost))
                {
                    RootElements.Add(visibleWindowHost);
                }
            }
        }

        foreach (UIElement item in RootElements)
        {
            Point position = NativeMouseWrapper.GetPosition(item);
            UIElement reference = item;
            HitTestResultCallback resultCallback = delegate (HitTestResult el)
            {
                AddScrollViewerAdapters(el, item);
                return HitTestResultBehavior.Continue;
            };
            VisualTreeHelper.HitTest(reference, null, resultCallback, new PointHitTestParameters(position));
        }
    }

    private void AddScrollViewerAdapters(HitTestResult hitTest, UIElement currentRoot)
    {
        FrameworkElement frameworkElement = hitTest.VisualHit as FrameworkElement;
        IScrollViewerAdapter scrollViewerAdapter = null;
        if (frameworkElement is ScrollViewer)
        {
            scrollViewerAdapter = new DefaultScrollViewerAdapter((ScrollViewer)frameworkElement);
        }
        else if (frameworkElement is IScrollViewerAdapter)
        {
            scrollViewerAdapter = (IScrollViewerAdapter)frameworkElement;
        }

        if (scrollViewerAdapter != null && frameworkElement.IsHitTestVisible && frameworkElement.IsVisible && !base.ScrollViewersToAdjust.Contains(scrollViewerAdapter))
        {
            base.ScrollViewersToAdjust.Add(scrollViewerAdapter);
            if (!TransformationsFromRootElement.ContainsKey(scrollViewerAdapter))
            {
                TransformationsFromRootElement.Add(scrollViewerAdapter, scrollViewerAdapter.OriginalUIElement.TransformToVisual(currentRoot));
            }
        }
    }

    private void UpdateScrollViewers(Point newMousePosition)
    {
        if (ShouldRefreshAutoScroll(newMousePosition))
        {
            if (base.ScrollViewerScrollTimer != null && !base.ScrollViewerScrollTimer.IsEnabled)
            {
                base.ScrollViewerScrollTimer.Start();
            }

            LastAdjustedPosition = newMousePosition;
            FindScrollViewers();
            OnAdjustingScrollViewers();
        }
    }

    public override void SetAllowDrag(DependencyObject obj, bool value)
    {
        FrameworkElement frameworkElement = obj as FrameworkElement;
        if (frameworkElement != null && value)
        {
            EnsureDragCuePopup(frameworkElement);
            frameworkElement.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnLeftMouseButtonDown));
            frameworkElement.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnLeftMouseButtonUp));
            frameworkElement.Unloaded -= OnFrameworkElementUnloaded;
            frameworkElement.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnLeftMouseButtonDown), handledEventsToo: true);
            frameworkElement.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnLeftMouseButtonUp), handledEventsToo: true);
            frameworkElement.Unloaded += OnFrameworkElementUnloaded;
        }
        else if (frameworkElement != null)
        {
            frameworkElement.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnLeftMouseButtonDown));
            frameworkElement.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnLeftMouseButtonUp));
        }
    }

    public override void SetAllowDrop(DependencyObject obj, bool value)
    {
        FrameworkElement frameworkElement = obj as FrameworkElement;
        if (frameworkElement != null && value)
        {
            frameworkElement.AllowDrop = value;
            System.Windows.DragDrop.RemoveDragOverHandler(frameworkElement, OnElementDragOver);
            System.Windows.DragDrop.RemoveDragLeaveHandler(frameworkElement, OnElementDragLeave);
            System.Windows.DragDrop.RemoveDropHandler(frameworkElement, OnElementDrop);
            frameworkElement.Unloaded -= OnFrameworkElementUnloaded;
            System.Windows.DragDrop.AddDragOverHandler(frameworkElement, OnElementDragOver);
            System.Windows.DragDrop.AddDragLeaveHandler(frameworkElement, OnElementDragLeave);
            System.Windows.DragDrop.AddDropHandler(frameworkElement, OnElementDrop);
            frameworkElement.Unloaded += OnFrameworkElementUnloaded;
        }
        else if (frameworkElement != null)
        {
            System.Windows.DragDrop.RemoveDragOverHandler(frameworkElement, OnElementDragOver);
            System.Windows.DragDrop.RemoveDragLeaveHandler(frameworkElement, OnElementDragLeave);
            System.Windows.DragDrop.RemoveDropHandler(frameworkElement, OnElementDrop);
        }
    }

    public override void StartDrag(FrameworkElement dragSource, object payload, object dragCue)
    {
        PrepareDrag(dragSource);
        DragInternal(dragSource, payload, dragCue, dragStartApproved: true);
    }

    public override void CancelDrag()
    {
        CancelDragging();
        Reset();
    }

    public override void Dispose()
    {
        CancelDrag();
        if (ArrowPopup != null)
        {
            ArrowPopup.Dispose();
        }
        if (DragPopup != null)
        {
            DragPopup.Dispose();
        }
    }

    internal void DragInternal(FrameworkElement dragElement, object payload, object dragCue, bool dragStartApproved)
    {
        FrameworkElement frameworkElement = ((base.Options.Source == null) ? dragElement : base.Options.Source);
        if (!frameworkElement.IsVisible)
        {
            Reset();
            return;
        }
        Point position = NativeMouseWrapper.GetPosition(null);
        InitialRelativeDragPosition = frameworkElement.PointFromScreen(position);
        EnsureWindowHost((base.Options.Source == null) ? dragElement : base.Options.Source);
        Point currentPos = CurrentWindowHost.PointFromScreen(position);
        InitDragOptions(currentPos);
        if (payload != null)
        {
            base.Options.Payload = payload;
        }
        if (dragCue != null)
        {
            base.Options.DragCue = dragCue;
        }
        bool? flag = dragStartApproved;
        if (!dragStartApproved)
        {
            flag = RaiseDragQuery().QueryResult;
        }
        if (flag.HasValue && flag.Value)
        {
            base.Options.Status = DragStatus.DragInProgress;
            RaiseDragInfo();
            _ = base.AutoBringIntoView;
            if (base.Options.DragCue != null)
            {
                EnsureDragCuePopup(frameworkElement, base.Options.DragCue as FrameworkElement);
            }
            if (base.Options.ArrowCue != null)
            {
                ArrowPopup = new UndetectableWindow(CurrentWindowHost as Window);
                ArrowPopup.CurrentDisplayBehavior = base.Options.ArrowCueDisplayBehavior;
                ArrowPopup.Child = base.Options.ArrowCue;
                ArrowPopup.Top = 0.0;
                ArrowPopup.Left = 0.0;
                ArrowPopup.Height = SystemParameters.PrimaryScreenHeight;
                ArrowPopup.Width = SystemParameters.PrimaryScreenWidth;
                ArrowPopup.Child.Opacity = 0.0;
                ArrowPopup.IsOpen = true;
            }
            DragDrop.AddGiveFeedbackHandler(dragElement, OnGiveFeedBack);
            DragDrop.AddQueryContinueDragHandler(dragElement, OnQueryContinue);
            DataObject dataObject = base.Options.DropDataObject;
            if (dataObject == null)
            {
                dataObject = new DataObject(dragElement.ToString());
            }
            DragDropEffects allowedEffects = base.Options.Effects ?? DragDropEffects.All;
            DragDrop.DoDragDrop(dragElement, dataObject, allowedEffects);
            FinishDrag();
        }
        else
        {
            Reset();
        }
    }

    internal void PrepareDrag(FrameworkElement dragElement, Point windowHostClickPoint)
    {
        if (dragElement != null && dragElement.IsVisible)
        {
            EnsureWindowHost(dragElement);
            base.IsDragging = true;
            base.Options = GetDefaultOptions();
            base.Options.Source = dragElement;
            AbsoluteClickPoint = CurrentWindowHost.PointToScreen(windowHostClickPoint);
            RelativeHostClickPoint = CurrentWindowHost.PointFromScreen(AbsoluteClickPoint);
            base.Options.CurrentDragPoint = RelativeHostClickPoint;
            base.Options.MouseClickPoint = RelativeHostClickPoint;
            RelativeClickPoint = dragElement.PointFromScreen(AbsoluteClickPoint);
        }
    }

    internal void PrepareDrag(FrameworkElement dragElement)
    {
        PrepareDrag(dragElement, Mouse.GetPosition(ApplicationHelper.GetRootVisual(dragElement)));
    }

    internal override void Initialize()
    {
        base.Initialize();
        PreviousRelativeDragPoints = new Dictionary<UIElement, Point>();
        approvedDestinations = new Dictionary<int, bool>();
    }

    internal override void Reset()
    {
        if (CurrentWindowHost != null)
        {
            CurrentWindowHost.RemoveHandler(UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }
        base.Reset();
        if (DragPopup != null)
        {
            DragPopup.Dispatcher.BeginInvoke((Action)delegate
            {
                DragPopup.Child = null;
                DragPopup.UpdatePosition(refresh: true);
                DragPopup.IsOpen = false;
            });
        }
        if (ArrowPopup != null)
        {
            ArrowPopup.Dispatcher.BeginInvoke((Action)delegate
            {
                ArrowPopup.Child = null;
                ArrowPopup.IsOpen = false;
                ArrowPopup.Close();
            });
        }
        UnsubscribeFromElementDragEvents();
        RootElements = null;
        CurrentWindowHost = null;
        IsDragCuePositioned = false;
        LastClickedElement = null;
        LastFeedBackPoint = default(Point);
        LastDragOverPoint = default(Point);
        LastAdjustedPosition = default(Point);
        if (base.ScrollViewerScrollTimer != null)
        {
            base.ScrollViewerScrollTimer.Stop();
        }
        approvedDestinations.Clear();
    }

    internal override void SetCursor()
    {
        Mouse.OverrideCursor = base.Options.CurrentCursor;
    }

    internal override void ResetCursor()
    {
        Mouse.OverrideCursor = null;
    }

    internal void ProcessMouseLeftButtonDown(object sender)
    {
        if (LastClickedElement == null || (LastClickedElement != null && !LastClickedElement.IsVisible))
        {
            LastClickedElement = sender as FrameworkElement;
        }
        if (LastClickedElement != null)
        {
            PrepareDrag(LastClickedElement);
        }
    }

    internal void PositionDragPopup(Point absolutePoint)
    {
        PositionDragPopup(absolutePoint, useNativeUpdate: true);
    }

    internal void PositionDragPopup(Point absolutePoint, bool useNativeUpdate)
    {
        if (DragPopup == null)
        {
            return;
        }
        if (!IsDragCuePositioned && DragPopup.Child != null && DragPopup.Child.ActualHeight != 0.0 && DragPopup.Child.ActualWidth != 0.0 && DragPopup.Child.IsLoaded)
        {
            IsDragCuePositioned = true;
            Point initialRelativeDragPosition = InitialRelativeDragPosition;
            RelativeClickPoint = GetRelativeDragCuePosition(RelativeClickPoint, initialRelativeDragPosition, DragPopup.Child);
        }
        if (IsDragCuePositioned)
        {
            DragPopup.VerticalOffset = absolutePoint.Y - RelativeClickPoint.Y;
            DragPopup.HorizontalOffset = absolutePoint.X - RelativeClickPoint.X;
            if (useNativeUpdate)
            {
                DragPopup.UpdatePosition(refresh: false);
            }
            else
            {
                DragPopup.Top = DragPopup.HorizontalOffset;
                DragPopup.Left = DragPopup.HorizontalOffset;
            }
            if (DragPopup.Child != null && IsDragCuePositioned)
            {
                DragPopup.Child.Opacity = 1.0;
            }
        }
    }

    internal override DragDropOptions GetDefaultOptions()
    {
        DragDropOptions defaultOptions = base.GetDefaultOptions();
        defaultOptions.DragCueDisplayBehavior = VisualCueBehavior.ShowsOnTop;
        defaultOptions.ArrowCueDisplayBehavior = VisualCueBehavior.ShowsOnTop;
        return defaultOptions;
    }

    internal void PositionArrow()
    {
        if (ArrowPopup == null || ArrowPopup.Child == null)
        {
            return;
        }
        ContentControl contentControl = ArrowPopup.Child as ContentControl;
        double num = base.Options.MouseClickPoint.X - base.Options.CurrentDragPoint.X;
        double num2 = base.Options.MouseClickPoint.Y - base.Options.CurrentDragPoint.Y;
        double num3 = Math.Sqrt(num * num + num2 * num2);
        if (num3 > base.ArrowVisibilityMinimumThreshold)
        {
            TransformGroup transformGroup = contentControl.RenderTransform as TransformGroup;
            DragArrowAdjustingEventArgs e = RaiseDragArrowAdjusting(transformGroup);
            Point point = new Point(Math.Abs(AbsoluteClickPoint.X - RelativeHostClickPoint.X), Math.Abs(AbsoluteClickPoint.Y - RelativeHostClickPoint.Y));
            if (!e.Cancel)
            {
                ScaleTransform scaleTransform = transformGroup.Children[0] as ScaleTransform;
                RotateTransform rotateTransform = transformGroup.Children[1] as RotateTransform;
                TranslateTransform translateTransform = transformGroup.Children[2] as TranslateTransform;
                translateTransform.X = AbsoluteClickPoint.X;
                translateTransform.Y = AbsoluteClickPoint.Y;
                contentControl.Width = num3;
                if (num != 0.0)
                {
                    rotateTransform.Angle = Math.Atan(num2 / num) * 180.0 / Math.PI;
                }
                else
                {
                    rotateTransform.Angle = ((num2 < 0.0) ? 90 : (-90));
                }
                if (num > 0.0)
                {
                    rotateTransform.Angle += 180.0;
                    scaleTransform.ScaleY = -1.0;
                }
                else
                {
                    scaleTransform.ScaleY = 1.0;
                }
            }
            else
            {
                TranslateTransform translateTransform2 = (from c in e.ArrowTransformation.Children
                                                          where c is TranslateTransform
                                                          select (TranslateTransform)c).FirstOrDefault();
                if (translateTransform2 == null)
                {
                    translateTransform2 = new TranslateTransform(point.X, point.Y);
                    e.ArrowTransformation.Children.Add(translateTransform2);
                }
                else
                {
                    translateTransform2.X += point.X;
                    translateTransform2.Y += point.Y;
                }
            }
            contentControl.RenderTransform = e.ArrowTransformation;
            contentControl.Visibility = Visibility.Visible;
        }
        else
        {
            contentControl.Visibility = Visibility.Collapsed;
        }
    }

    private void EnsureDragCuePopup(FrameworkElement element)
    {
        EnsureDragCuePopup(element, null);
        DragPopup.Loaded += DragPopup_Loaded;
    }

    private void DragPopup_Loaded(object sender, RoutedEventArgs e)
    {
        DragPopup.Loaded -= DragPopup_Loaded;
        if (!base.IsDragging)
        {
            DragPopup.IsOpen = false;
        }
    }

    private void EnsureDragCuePopup(FrameworkElement element, FrameworkElement dragCue)
    {
        if (CurrentWindowHost == null)
        {
            EnsureWindowHost(element);
        }
        if (DragPopup == null || (DragPopup != null && (!DragPopup.CanManage(element.Dispatcher.Thread) || DragPopup.Disposed)))
        {
            DragPopup = new UndetectableWindow(CurrentWindowHost as Window);
            if (!DragPopup.IsOpen && !element.IsLoaded)
            {
                element.Loaded += FirstDraggableElementLoaded;
            }
            else if (!DragPopup.IsOpen)
            {
                DragPopup.IsOpen = true;
            }
        }
        UpdateDragCueWindowState(dragCue);
    }

    private void FirstDraggableElementLoaded(object sender, EventArgs args)
    {
        ((FrameworkElement)sender).Loaded -= FirstDraggableElementLoaded;
        DragPopup.IsOpen = true;
    }

    private void UpdateDragCueWindowState(FrameworkElement dragCue)
    {
        if (CurrentWindowHost != null && CurrentWindowHost is Window)
        {
            DragPopup.ParentWindow = (Window)CurrentWindowHost;
        }
        DragPopup.CurrentDisplayBehavior = base.Options.DragCueDisplayBehavior;
        DragPopup.FitToChildDimensions = true;
        DragPopup.AutoCloseCondition = () => !base.IsDragging;
        DragPopup.Width = 0.0;
        DragPopup.Height = 0.0;
        if (dragCue != null)
        {
            dragCue.IsHitTestVisible = false;
            dragCue.Opacity = 0.0;
            DragPopup.Child = dragCue;
        }
    }

    private void OnFrameworkElementUnloaded(object sender, RoutedEventArgs e)
    {
    }

    private void DragInternal(FrameworkElement dragElement)
    {
        DragInternal(dragElement, null, null, dragStartApproved: false);
    }

    private void FinishDrag()
    {
        finishingDrag = true;
        if ((base.Options.Destination != null && approvedDestinations.ContainsKey(base.Options.Destination.GetHashCode())) ? approvedDestinations[base.Options.Destination.GetHashCode()] : isDropPossible)
        {
            base.Options.Status = DragStatus.DragComplete;
            RaiseDragInfo();
            base.Options.Status = DragStatus.DropComplete;
            RaiseDropInfo();
        }
        else if (base.IsDragging)
        {
            CancelDragging();
        }
        finishingDrag = false;
        Reset();
    }

    private void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            ProcessMouseLeftButtonDown(sender);
        }
    }

    private void OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        ProcessMouseLeftButtonUp();
    }

    private void ProcessMouseLeftButtonUp()
    {
        Reset();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && base.IsDragging && LastClickedElement != null && !dragInitiated)
        {
            dragInitiated = true;
            Point position = Mouse.GetPosition(CurrentWindowHost);
            if (Math.Abs(position.X - RelativeHostClickPoint.X) > base.DragStartThreshold || Math.Abs(position.Y - RelativeHostClickPoint.Y) > base.DragStartThreshold)
            {
                DragInternal(LastClickedElement);
            }
            dragInitiated = false;
        }
        else if (e.LeftButton != MouseButtonState.Pressed && base.IsDragging && !finishingDrag)
        {
            base.IsDragging = false;
            FinishDrag();
        }
    }

    private void OnGiveFeedBack(object sender, GiveFeedbackEventArgs e)
    {
        Point position = NativeMouseWrapper.GetPosition(null);
        if (CurrentWindowHost != null && CurrentWindowHost.IsVisible)
        {
            base.Options.CurrentDragPoint = CurrentWindowHost.PointFromScreen(position);
        }
        if (!DragDropProviderBase.ArePointsNear(position, LastFeedBackPoint, 1.0))
        {
            LastFeedBackPoint = position;
            PositionDragPopup(position);
            if (base.AutoBringIntoView)
            {
                UpdateScrollViewers(position);
            }
        }
        PositionArrow();
        if (base.Options.CurrentCursor != null)
        {
            SetCursor();
        }
        else if (base.Options.Effects.HasValue)
        {
            e.UseDefaultCursors = true;
            ResetCursor();
        }
        else
        {
            e.UseDefaultCursors = false;
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        if (DragPopup != null && DragPopup.Child != null && DragPopup.Child.IsLoaded && !DragPopup.IsOpen)
        {
            DragPopup.IsOpen = true;
            PositionDragPopup(position);
            DragPopup.Child.Opacity = 1.0;
        }
        if (ArrowPopup != null && ArrowPopup.Child != null)
        {
            ArrowPopup.Child.Opacity = 1.0;
        }
        e.Handled = true;
    }

    private void OnQueryContinue(object sender, QueryContinueDragEventArgs args)
    {
        if (IsCancellingDrag)
        {
            args.Action = DragAction.Cancel;
        }
    }

    private void OnElementDragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (base.Options.Effects.HasValue)
        {
            e.Effects = base.Options.Effects.Value;
        }

        LastDragOverPoint = base.Options.CurrentDragPoint;
        DragStatus status = base.Options.Status;
        base.Options.Destination = sender as FrameworkElement;
        base.Options.Status = DragStatus.DropDestinationQuery;
        if (base.Options.Destination != null)
        {
            base.Options.RelativeDragPoint = NativeMouseWrapper.GetPosition(base.Options.Destination);
        }

        if (IsDraggingFromExternalProgram)
        {
            base.Options.DropDataObject = e.Data as DataObject;
        }

        var e2 = RaiseDropQuery();
        bool? queryResult = e2.QueryResult;
        bool? flag = null;
        if (queryResult == true)
        {
            base.Options.Status = DragStatus.DropSourceQuery;
            flag = RaiseDragQuery().QueryResult;
        }
        bool? flag2 = queryResult;
        if (flag2 == true && flag2.HasValue && (flag == true || base.Options.Source == null))
        {
            base.Options.Status = DragStatus.DropPossible;
            RaiseDropInfo();
            RaiseDragInfo();
            OnDropPossible();
            isDropPossible = true;
            if (approvedDestinations.ContainsKey(base.Options.Destination.GetHashCode()))
            {
                approvedDestinations[base.Options.Destination.GetHashCode()] = true;
            }
            else
            {
                approvedDestinations.Add(base.Options.Destination.GetHashCode(), value: true);
            }
        }
        else
        {
            base.Options.Status = DragStatus.DropImpossible;
            isDropPossible = false;
            if (approvedDestinations.ContainsKey(base.Options.Destination.GetHashCode()))
            {
                approvedDestinations[base.Options.Destination.GetHashCode()] = false;
            }
            else
            {
                approvedDestinations.Add(base.Options.Destination.GetHashCode(), value: false);
            }
            if (status != DragStatus.DropImpossible)
            {
                NotifyPreviousApprovedDestination();
            }
        }
        e.Handled = e2.Handled;
    }

    private void OnElementDragLeave(object sender, System.Windows.DragEventArgs e)
    {
        OnDropImpossible();
        isDropPossible = false;
        if (approvedDestinations.ContainsKey(base.Options.Destination.GetHashCode()))
        {
            approvedDestinations[base.Options.Destination.GetHashCode()] = false;
        }
        else
        {
            approvedDestinations.Add(base.Options.Destination.GetHashCode(), value: false);
        }
    }

    private void OnElementDrop(object sender, System.Windows.DragEventArgs e)
    {
        base.Options.Destination = sender as FrameworkElement;
        e.Handled = true;
        if (IsDraggingFromExternalProgram)
        {
            base.Options.DropDataObject = e.Data as DataObject;
            FinishDrag();
        }
    }

    private void EnsureWindowHost(UIElement element)
    {
        UIElement currentWindowHost = CurrentWindowHost;
        if (element != null)
        {
            CurrentWindowHost = Window.GetWindow(element);
        }

        if (CurrentWindowHost == null && RadControl.IsInElementHost(element))
        {
            PresentationSource presentationSource = PresentationSource.FromVisual(element);
            CurrentWindowHost = presentationSource.RootVisual as FrameworkElement;
        }

        if (CurrentWindowHost != null)
        {
            currentWindowHost?.RemoveHandler(UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
            CurrentWindowHost.AddHandler(UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove), handledEventsToo: true);
        }
    }

    private void InitDragOptions(Point currentPos)
    {
        base.Options.CurrentDragPoint = currentPos;
        base.Options.ArrowCue = null;
        base.Options.DragCue = null;
        base.Options.Status = DragStatus.DragQuery;
    }

    private void CancelDragging()
    {
        if (base.IsDragging)
        {
            base.IsDragging = false;
            NotifyPreviousApprovedDestination();
            base.Options.Status = DragStatus.DragCancel;
            RaiseDragInfo();
            base.Options.Status = DragStatus.None;
            IsCancellingDrag = true;
            UnsubscribeFromElementDragEvents();
        }
    }

    private void UnsubscribeFromElementDragEvents()
    {
        if (base.Options.Source != null)
        {
            System.Windows.DragDrop.RemoveGiveFeedbackHandler(base.Options.Source, OnGiveFeedBack);
            System.Windows.DragDrop.RemoveQueryContinueDragHandler(base.Options.Source, OnQueryContinue);
        }
    }
}
