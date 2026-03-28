using BgCommon;
using BgCommon.Collections;
using BgCommon.Localization;
using BgControls.Windows.Datas;

namespace BgControls.Windows.Controls;

/// <summary>
/// 继承自ContentControl，并将其默认样式键与自身类型关联.
/// </summary>
[TemplatePart(Name = PartDrivesTree, Type = typeof(TreeView))]
[TemplatePart(Name = PartItemsList, Type = typeof(ListView))]
public class FileExplorer : Control
{
    // --- 模板部件名称常量 ---
    private const string PartDrivesTree = "PART_DrivesTree";
    private const string PartItemsList = "PART_ItemsList";

    private static readonly DependencyPropertyKey DrivesPropertyKey =
    DependencyProperty.RegisterReadOnly(nameof(Drives), typeof(ObservableRangeCollection<TreeViewItem>), typeof(FileExplorer), new PropertyMetadata(new ObservableRangeCollection<TreeViewItem>()));

    private static readonly DependencyPropertyKey FileSystemItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FileSystemItems), typeof(ObservableRangeCollection<FileSystemItemInfo>), typeof(FileExplorer), new PropertyMetadata(new ObservableRangeCollection<FileSystemItemInfo>()));

    private static readonly DependencyPropertyKey IsLoadingPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLoading), typeof(bool), typeof(FileExplorer), new PropertyMetadata(false));

    private static readonly DependencyPropertyKey SelectedItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedItems), typeof(ObservableRangeCollection<FileSystemItemInfo>), typeof(FileExplorer), new PropertyMetadata(new ObservableRangeCollection<FileSystemItemInfo>()));

    /// <summary>
    /// 标识 Drives 只读依赖属性.
    /// </summary>
    public static readonly DependencyProperty DrivesProperty = DrivesPropertyKey.DependencyProperty;

    /// <summary>
    /// 标识 FileSystemItems 只读依赖属性.
    /// </summary>
    public static readonly DependencyProperty FileSystemItemsProperty = FileSystemItemsPropertyKey.DependencyProperty;

    /// <summary>
    /// 标识 IsLoading 只读依赖属性.
    /// </summary>
    public static readonly DependencyProperty IsLoadingProperty = IsLoadingPropertyKey.DependencyProperty;

    /// <summary>
    /// 标识 SelectedItems 只读依赖属性.
    /// </summary>
    public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;

    /// <summary>
    /// 标识 FolderContentFilter 依赖属性.
    /// 这个属性用于过滤文件夹，只显示那些包含指定扩展名文件的文件夹.
    /// </summary>
    public static readonly DependencyProperty IsFilterFolderProperty =
        DependencyProperty.Register(nameof(IsFilterFolder), typeof(bool), typeof(FileExplorer), new PropertyMetadata(true, OnFilterOrPathChanged)); // 重用回调，因为改变它也需要刷新列表

    public static readonly DependencyProperty FileFilterProperty =
        DependencyProperty.Register(nameof(FileFilter), typeof(string), typeof(FileExplorer), new PropertyMetadata("*.*", OnFilterOrPathChanged));

    public static readonly DependencyProperty CurrentPathProperty =
        DependencyProperty.Register(nameof(CurrentPath), typeof(string), typeof(FileExplorer), new PropertyMetadata(null, OnFilterOrPathChanged));

    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(SelectionMode), typeof(FileExplorer), new PropertyMetadata(SelectionMode.Single, OnSelectionModeChanged));

    /// <summary>
    /// 标识 FileDoubleClickCommand 依赖属性.
    /// </summary>
    public static readonly DependencyProperty FileDoubleClickCommandProperty =
        DependencyProperty.Register(nameof(FileDoubleClickCommand), typeof(ICommand), typeof(FileExplorer), new PropertyMetadata(null));

    /// <summary>
    /// 标识 SelectionChangedCommand 依赖属性.
    /// </summary>
    public static readonly DependencyProperty SelectionChangedCommandProperty =
        DependencyProperty.Register(nameof(SelectionChangedCommand), typeof(ICommand), typeof(FileExplorer), new PropertyMetadata(null));

    /// <summary>
    /// 标识 FileDoubleClick 路由事件.
    /// </summary>
    public static readonly RoutedEvent FileDoubleClickEvent =
        EventManager.RegisterRoutedEvent(nameof(FileDoubleClick), RoutingStrategy.Bubble, typeof(FileDoubleClickHandler), typeof(FileExplorer));

    /// <summary>
    /// 标识 SelectionChanged 路由事件.
    /// </summary>
    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(FileSelectionChangedHandler), typeof(FileExplorer));

    static FileExplorer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FileExplorer), new FrameworkPropertyMetadata(typeof(FileExplorer)));
    }

    private bool isLoaded = false;
    private TreeView? drivesTree = null;
    private ListView? itemsList = null;
    private HwndSource? hwndSource = null; // 用于管理窗口消息钩子
    private CancellationTokenSource? cts = null; // 用于管理取消操作

    // Key: 父目录的完整路径
    // Value: 该父目录下所有有效的、应该被显示的直接子目录列表
    private readonly Dictionary<string, List<DirectoryInfo>> folderCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="FileExplorer"/> class.
    /// </summary>
    public FileExplorer()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否根据文件扩展名过滤文件夹.
    /// </summary>
    public bool IsFilterFolder
    {
        get { return (bool)GetValue(IsFilterFolderProperty); }
        set { SetValue(IsFilterFolderProperty, value); }
    }

    public string FileFilter
    {
        get { return (string)GetValue(FileFilterProperty); }
        set { SetValue(FileFilterProperty, value); }
    }

    public string CurrentPath
    {
        get { return (string)GetValue(CurrentPathProperty); }
        set { SetValue(CurrentPathProperty, value); }
    }

    public ObservableRangeCollection<TreeViewItem> Drives
    {
        get { return (ObservableRangeCollection<TreeViewItem>)GetValue(DrivesProperty); }
        private set { SetValue(DrivesPropertyKey, value); }
    }

    public ObservableRangeCollection<FileSystemItemInfo> FileSystemItems
    {
        get { return (ObservableRangeCollection<FileSystemItemInfo>)GetValue(FileSystemItemsProperty); }
        private set { SetValue(FileSystemItemsPropertyKey, value); }
    }

    /// <summary>
    /// Gets 当前被选中的项的集合.这是一个只读属性.
    /// </summary>
    public ObservableRangeCollection<FileSystemItemInfo> SelectedItems
    {
        get => (ObservableRangeCollection<FileSystemItemInfo>)GetValue(SelectedItemsProperty);
        private set => SetValue(SelectedItemsPropertyKey, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        private set => SetValue(IsLoadingPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets 文件列表的项选择模式.
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets 在文件项被双击时要执行的命令.
    /// 此命令将接收被双击的 FileSystemItemInfo 作为参数.
    /// </summary>
    public ICommand FileDoubleClickCommand
    {
        get => (ICommand)GetValue(FileDoubleClickCommandProperty);
        set => SetValue(FileDoubleClickCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets  在选择项发生变化时执行的命令.
    /// 此命令将接收一个 IList<FileSystemItemInfo> 作为参数.
    /// </summary>
    public ICommand SelectionChangedCommand
    {
        get => (ICommand)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    /// <summary>
    /// 当文件项被双击时发生.
    /// </summary>
    public event FileDoubleClickHandler FileDoubleClick
    {
        add { AddHandler(FileDoubleClickEvent, value); }
        remove { RemoveHandler(FileDoubleClickEvent, value); }
    }

    /// <summary>
    /// 当文件列表中的选择项发生变化时发生.
    /// </summary>
    public event FileSelectionChangedHandler SelectionChanged
    {
        add { AddHandler(SelectionChangedEvent, value); }
        remove { RemoveHandler(SelectionChangedEvent, value); }
    }

    private string ThisComputerText => LocalizationProviderFactory.GetString("此电脑");

    private string DriverText => LocalizationProviderFactory.GetString("驱动器");

    private string FolderText => LocalizationProviderFactory.GetString("文件夹");

    private string FileText => LocalizationProviderFactory.GetString("文件");

    /// <summary>
    /// 当控件应用新模板时调用.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解除旧模板部件的事件订阅
        if (drivesTree != null)
        {
            drivesTree.SelectedItemChanged -= DrivesTree_SelectedItemChanged;

            // 移除 Expanded 事件处理器
            drivesTree.RemoveHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(DrivesTree_Expanded));
        }

        if (itemsList != null)
        {
            itemsList.SelectionMode = SelectionMode;
            itemsList.SelectionChanged -= ItemsList_SelectionChanged;
            itemsList.MouseDoubleClick -= ItemsList_MouseDoubleClick;
        }

        // 获取新模板中的部件
        drivesTree = GetTemplateChild(PartDrivesTree) as TreeView;
        itemsList = GetTemplateChild(PartItemsList) as ListView;
        if (drivesTree != null)
        {
            drivesTree.SelectedItemChanged += DrivesTree_SelectedItemChanged;

            // 在这里订阅 Expanded 事件
            // 我们订阅 TreeView 上的事件，它会捕获所有子 TreeViewItem 冒泡上来的 Expanded 事件.
            // 使用 AddHandler 并设置 handledEventsToo: true 是一个好习惯，
            // 确保即使某个子项将事件标记为已处理，我们的处理器仍然会被调用.
            drivesTree.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(DrivesTree_Expanded), true);
        }

        if (itemsList != null)
        {
            itemsList.SelectionMode = SelectionMode;
            itemsList.SelectionChanged += ItemsList_SelectionChanged;
            itemsList.MouseDoubleClick += ItemsList_MouseDoubleClick;
        }

        // 模板应用后 + 加载完毕后，根据当前路径加载一次内容
        if (isLoaded)
        {
            LoadDirectoryAsync(CurrentPath);
        }
    }

    /// <summary>
    /// 根据一个已知的文件夹 GUID 创建一个 TreeViewItem 节点.
    /// </summary>
    /// <param name="knownFolderId">已知文件夹的 GUID.</param>
    /// <param name="fallbackName">如果无法获取本地化名称时使用的备用名称.</param>
    /// <returns>一个配置好的 TreeViewItem，如果文件夹不存在或无法访问则返回 null.</returns>
    private TreeViewItem? CreateKnownFolderNode(Guid knownFolderId, string fallbackName)
    {
        try
        {
            // 步骤 1: 获取文件夹的实际文件系统路径
            int result = NativeMethods.SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out IntPtr pathPtr);
            if (result != 0) // S_OK 的值为 0
            {
                return null;
            }

            string? path = Marshal.PtrToStringUni(pathPtr);
            NativeMethods.CoTaskMemFree(pathPtr); // 重要：释放由 API 分配的内存

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return null;
            }

            // 步骤 2: 获取文件夹的专用图标和本地化显示名称
            ImageSource? icon = IconManager.GetIconFromKnownFolder(knownFolderId);
            string displayName = fallbackName;

            // // 使用 PIDL 来获取 Shell 的官方显示名称，这是最可靠的方式
            // if (NativeMethods.SHGetKnownFolderIDList(ref knownFolderId, 0, IntPtr.Zero, out IntPtr pidl) == 0)
            // {
            //     try
            //     {
            //         SHFileInfo shfi = new();
            //         SHGFI flags = SHGFI.PIDL | SHGFI.DisplayName | SHGFI.SmallIcon;
            //         if (NativeMethods.SHGetFileInfo(pidl, 0, ref shfi, (uint)Marshal.SizeOf(shfi), flags) != IntPtr.Zero && !string.IsNullOrEmpty(shfi.DisplayName))
            //         {
            //             displayName = shfi.DisplayName;
            //         }
            //     }
            //     finally
            //     {
            //         if (pidl != IntPtr.Zero)
            //         {
            //             NativeMethods.CoTaskMemFree(pidl); // 释放 PIDL 内存
            //         }
            //     }
            // }

            // 步骤 3: 创建 TreeViewItem
            var node = new TreeViewItem
            {
                Header = CreateHeader(displayName, icon),
                Tag = path // Tag 存储其实际路径，用于后续导航
            };

            // 添加一个 null 子项，使其可以被展开（按需加载）
            _ = node.Items.Add(null);

            return node;
        }
        catch (Exception ex)
        {
            // 记录异常，以防万一
            System.Diagnostics.Debug.WriteLine($"创建已知文件夹节点 '{fallbackName}' 失败: {ex.Message}");
            return null;
        }
    }

    private async void LoadDrives()
    {
        // 当驱动器列表刷新时（例如U盘插拔），清空所有旧的文件夹缓存是安全的
        folderCache.Clear();

        var drivesCollection = new ObservableRangeCollection<TreeViewItem>();
        var thisComputerNode = new TreeViewItem
        {
            Header = CreateHeader(ThisComputerText, IconManager.GetComputerIcon()),
            Tag = "MyComputer"
        };

        // --- 1. 首先添加特殊文件夹 ---
        var specialFolders = new List<(Guid, string)>
        {
            (NativeMethods.FOLDERID_Desktop, LocalizationProviderFactory.GetString("桌面")),
            (NativeMethods.FOLDERID_Documents, LocalizationProviderFactory.GetString("文档")),
            (NativeMethods.FOLDERID_Downloads, LocalizationProviderFactory.GetString("下载")),
            (NativeMethods.FOLDERID_Music, LocalizationProviderFactory.GetString("音乐")),
            (NativeMethods.FOLDERID_Pictures, LocalizationProviderFactory.GetString("图片")),
            (NativeMethods.FOLDERID_Videos, LocalizationProviderFactory.GetString("视频")),
        };

        foreach (var (id, name) in specialFolders)
        {
            TreeViewItem? node = CreateKnownFolderNode(id, name);
            if (node != null)
            {
                _ = thisComputerNode.Items.Add(node);
            }
        }

        // --- 2. 然后添加驱动器 --
        TreeViewItem? removableDriveToSelect = null;
        TreeViewItem? firstFixedDriveToSelect = null;

        IEnumerable<DriveInfo> drives = DriveInfo.GetDrives().Where(d => d.IsReady);
        foreach (DriveInfo drive in drives)
        {
            string name = string.IsNullOrEmpty(drive.VolumeLabel)
                ? $"{DriverText} ({drive.Name.TrimEnd('\\')})"
                : $"{drive.VolumeLabel} ({drive.Name.TrimEnd('\\')})";

            var driveNode = new TreeViewItem
            {
                Header = CreateHeader(name, IconManager.GetDriveIcon(drive)),
                Tag = drive.Name
            };

            // 智能选择逻辑
            if (drive.DriveType == DriveType.Removable && removableDriveToSelect == null)
            {
                removableDriveToSelect = driveNode; // 找到第一个U盘
            }
            else if (drive.DriveType == DriveType.Fixed && firstFixedDriveToSelect == null)
            {
                firstFixedDriveToSelect = driveNode; // 找到第一个本地磁盘
            }

            _ = driveNode.Items.Add(null);

            // driveNode.Expanded += DrivesTree_Expanded;
            _ = thisComputerNode.Items.Add(driveNode);
        }

        drivesCollection.Add(thisComputerNode);
        thisComputerNode.IsExpanded = true;

        // 使用只读依赖属性的 SetValue 方法来更新集合，这是最佳实践
        SetValue(DrivesPropertyKey, drivesCollection);

        // --- 3. 更新默认选中项的逻辑 ---
        // 优先选择U盘，其次是本地磁盘，最后是第一个特殊文件夹
        TreeViewItem? itemToSelect = removableDriveToSelect ?? firstFixedDriveToSelect;
        if (itemToSelect != null)
        {
            // 使用 Dispatcher.BeginInvoke 确保这些UI操作在WPF布局和渲染周期之后执行，
            // 这样可以避免在树刚刚构建时设置 IsSelected 可能出现的竞争条件或失败.
            // 这是处理 TreeViewItem 选中的一个非常健壮的技巧.
            await Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        itemToSelect.IsSelected = true;

                        // 如果是驱动器，则展开它
                        if (itemToSelect.Tag is string tag && (tag.EndsWith(":\\") || tag.EndsWith(":/")))
                        {
                            itemToSelect.IsExpanded = true;
                        }

                        // 如果需要，可以滚动到选中项
                        itemToSelect.BringIntoView();
                    }),
                    DispatcherPriority.Loaded
            );
        }
        else
        {
            // 如果没有任何可用的驱动器，清空右侧列表
            FileSystemItems.Clear();
        }

        //// 当驱动器列表刷新时（例如U盘插拔），清空所有旧的文件夹缓存是安全的
        //folderCache.Clear();

        //var drivesCollection = new ObservableRangeCollection<TreeViewItem>();
        //var thisComputerNode = new TreeViewItem
        //{
        //    Header = CreateHeader(ThisComputerText, IconManager.GetComputerIcon()),
        //    Tag = "MyComputer"
        //};

        //TreeViewItem? selectedItem = null;
        //IEnumerable<DriveInfo> drives = DriveInfo.GetDrives().Where(d => d.IsReady);
        //foreach (DriveInfo drive in drives)
        //{
        //    string name = string.IsNullOrEmpty(drive.VolumeLabel)
        //        ? $"{DriverText} ({drive.Name.TrimEnd('\\')})"
        //        : $"{drive.VolumeLabel} ({drive.Name.TrimEnd('\\')})";

        //    var driveNode = new TreeViewItem
        //    {
        //        // --- 修改这里：传递整个 drive 对象 ---
        //        Header = CreateHeader(name, IconManager.GetDriveIcon(drive)),
        //        Tag = drive.Name
        //    };

        //    if (selectedItem == null && drive.DriveType == DriveType.Removable)
        //    {
        //        selectedItem = driveNode;
        //    }

        //    _ = driveNode.Items.Add(null);
        //    driveNode.Expanded += DrivesTree_Expanded;
        //    _ = thisComputerNode.Items.Add(driveNode);
        //}

        //drivesCollection.Add(thisComputerNode);
        //thisComputerNode.IsExpanded = true;
        //Drives = drivesCollection;
        //if (selectedItem != null)
        //{
        //    selectedItem.IsExpanded = true;
        //}
        //else if (thisComputerNode.Items.Count > 0)
        //{
        //    ((TreeViewItem)thisComputerNode.Items[0]).IsExpanded = true;
        //}
        //else
        //{
        //    FileSystemItems.Clear();
        //}
    }

    private async void LoadDirectoryAsync(string? path)
    {
        // 1. 取消之前的任务，并为新操作创建取消令牌
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            FileSystemItems.Clear();
            return;
        }

        SetValue(IsLoadingPropertyKey, true);
        try
        {
            // 2. 获取有效的子文件夹列表（可能来自缓存，也可能通过新计算）
            List<DirectoryInfo> validSubFolders = await GetValidSubdirectoriesAsync(path, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            // 3. 在启动后台任务之前，从UI线程获取所有需要的值.
            string currentFileFilter = this.FileFilter;
            string folderText = this.FolderText; // 也包括本地化字符串
            string fileText = this.FileText;
            DirectoryInfo? pathDir = new(path);

            // 4. 在后台线程中准备要在UI上显示的项目列表，将文件I/O操作放到后台线程执行，避免UI卡顿
            List<FileSystemItemInfo> items = await Task.Run(
                () =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return new List<FileSystemItemInfo>();
                    }

                    // 使用已经计算好的文件夹列表
                    var uiItems = new List<FileSystemItemInfo>();
                    foreach (DirectoryInfo dir in validSubFolders)
                    {
                        uiItems.Add(new FileSystemItemInfo
                        {
                            Name = dir.Name,
                            FullPath = dir.FullName,
                            Icon = IconManager.GetFolderIcon(),
                            LastModified = dir.LastWriteTime,
                            Type = FolderText,
                            Size = string.Empty,
                            IsDirectory = true
                        });
                    }

                    // 只需获取顶层文件
                    var dirInfo = new DirectoryInfo(path);
                    foreach (FileInfo file in dirInfo.EnumerateFiles(currentFileFilter))
                    {
                        if ((file.Attributes & FileAttributes.Hidden) == 0 && (file.Attributes & FileAttributes.System) == 0)
                        {
                            uiItems.Add(new FileSystemItemInfo
                            {
                                Name = file.Name,
                                FullPath = file.FullName,
                                Icon = IconManager.GetFileIcon(file.FullName),
                                LastModified = file.LastWriteTime,
                                Type = FolderText,
                                Size = FormatFileSize(file.Length),
                                IsDirectory = false
                            });
                        }
                    }

                    return uiItems;
                },
                token
             );

            if (token.IsCancellationRequested)
            {
                return;
            }

            // 5. 在UI线程上更新列表
            FileSystemItems.ReplaceRange(items);
        }
        catch (OperationCanceledException)
        {
            /* 任务被取消，正常退出 */
        }
        catch (UnauthorizedAccessException)
        {
            // 处理特定的、可预见的异常，可以考虑给用户提示
            FileSystemItems.Clear();
        }
        catch (IOException) // 包含了网络错误等
        {
            FileSystemItems.Clear();
        }
        catch (Exception)// 其他异常清空列表
        {
            FileSystemItems.Clear();
        }
        finally
        {
            // 确保加载状态总是被重置
            this.SetValue(IsLoadingPropertyKey, false);
        }
    }

    /// <summary>
    /// 窗口过程（WndProc）钩子，用于接收所有窗口消息
    /// </summary>
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // 只关心设备变更消息
        if (msg == NativeMethods.WM_DEVICECHANGE)
        {
            switch ((int)wParam)
            {
                case NativeMethods.DBT_DEVICEARRIVAL: // 当设备插入并可用时
                case NativeMethods.DBT_DEVICEREMOVECOMPLETE: // 当设备被移除时
                    // 检查设备类型是否为逻辑卷（驱动器）
                    // Marshal.PtrToStructure 用于将非托管内存指针转换为托管对象
                    DevBroadcastHDR? header = (DevBroadcastHDR?)Marshal.PtrToStructure(lParam, typeof(DevBroadcastHDR));
                    if (header != null && header.Value.DeviceType == NativeMethods.DBT_DEVTYP_VOLUME)
                    {
                        System.Diagnostics.Debug.WriteLine("检测到驱动器变更，正在刷新列表...");

                        // 使用 Dispatcher 在UI线程上安全地调用 LoadDrives
                        // 因为这个消息可能在非UI线程上被处理
                        _ = Dispatcher.InvokeAsync(LoadDrives);
                    }

                    break;
            }
        }

        // 返回 IntPtr.Zero 表示我们没有完全处理该消息，它应该继续传递
        return IntPtr.Zero;
    }

    /// <summary>
    /// 控件加载时调用的方法
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!isLoaded)
        {
            // 首次加载驱动器
            LoadDrives();

            // 开始监听设备变更消息
            var window = Window.GetWindow(this);
            if (window != null)
            {
                hwndSource = HwndSource.FromVisual(window) as HwndSource;
                hwndSource?.AddHook(WndProc); // 添加消息钩子
            }

            isLoaded = true;
        }
    }

    /// <summary>
    /// 控件卸载时调用的方法
    /// </summary>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // 确保任务被取消
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

        // 确保取消令牌源被释放
        folderCache.Clear();

        // 停止监听，防止内存泄漏
        if (hwndSource != null)
        {
            hwndSource.RemoveHook(WndProc);
            hwndSource.Dispose();
            hwndSource = null;
        }
    }

    /// <summary>
    /// 当用户展开 TreeView 中的一个节点时异步加载其子节点.
    /// </summary>
    private async void DrivesTree_Expanded(object sender, RoutedEventArgs e)
    {
        // 1. 初始检查和防御
        // e.OriginalSource 是最初触发事件的元素，它就是我们需要的 TreeViewItem.
        if (e.OriginalSource is not TreeViewItem item || item.Tag is not string path || !item.IsExpanded)
        {
            return;
        }

        // 如果节点已经加载过（没有虚拟子项了）则直接返回
        if (item.Items.Count != 1 || item.Items[0] != null)
        {
            return;
        }

        // 2. 设置取消令牌
        cts?.Cancel();
        cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        // 3. 准备加载UI
        e.Handled = true;
        item.Items.Clear(); // 移除虚拟子项
        item.Cursor = Cursors.Wait; // 提供即时反馈：正在加载

        try
        {
            // 4. 调用辅助方法获取有效子目录
            List<DirectoryInfo> validSubDirs = await GetValidSubdirectoriesAsync(path, token);

            // 5. 检查任务是否在获取数据后被取消
            if (token.IsCancellationRequested)
            {
                // 如果被取消，将节点恢复到初始状态，重新添加虚拟项
                RestoreVirtualNodeAndCollapse(item);
                return;
            }

            // 6. 使用获取到的数据填充UI
            if (validSubDirs.Count > 0)
            {
                foreach (DirectoryInfo subDir in validSubDirs)
                {
                    var subNode = new TreeViewItem
                    {
                        Header = CreateHeader(subDir.Name, IconManager.GetFolderIcon()),
                        Tag = subDir.FullName
                    };

                    _ = subNode.Items.Add(null); // 为下一级展开添加占位符
                                                 //  subNode.Expanded += DrivesTree_Expanded;
                    _ = item.Items.Add(subNode);
                }

                if (item.Items.Count > 0 && item.Items[0] != null)
                {
                    _ = Dispatcher.BeginInvoke(
                        () =>
                        {
                            // item.IsExpanded = true;
                            item.IsSelected = true;
                        },
                        DispatcherPriority.Loaded);
                }
            }
            else
            {
                // 如果没有找到任何有效的子目录，可以添加一个提示节点
                // 这比保持一个空的展开节点体验更好.
                _ = item.Items.Add(new TreeViewItem { Header = "无匹配的子文件夹", IsEnabled = false });
            }
        }
        catch (OperationCanceledException)
        {
            // 捕获取消异常，同样恢复UI状态
            RestoreVirtualNodeAndCollapse(item);
        }
        catch (UnauthorizedAccessException)
        {
            _ = item.Items.Add(new TreeViewItem { Header = "访问被拒绝", IsEnabled = false });
        }
        finally
        {
            // 无论成功、失败还是取消，最后都恢复鼠标指针
            item.Cursor = null;
        }
    }

    private void DrivesTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem { Tag: string path } && path != "MyComputer")
        {
            CurrentPath = path;
        }
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView listView)
        {
            // 更新 SelectedItems 属性

            // 步骤 1: 更新我们自己的只读 SelectedItems 集合
            var currentSelectedItems = listView.SelectedItems.OfType<FileSystemItemInfo>().ToList();
            SelectedItems.ReplaceRange(currentSelectedItems);

            // 执行 SelectionChangedCommand
            ICommand? command = SelectionChangedCommand;
            if (command != null)
            {
                if (command.CanExecute(SelectedItems))
                {
                    command.Execute(SelectedItems);
                }
                else
                {
                    // 如果命令不可执行，可以考虑抛出异常或记录日志
                    System.Diagnostics.Debug.WriteLine("SelectionChangedCommand 无法执行");
                }
            }

            // 触发 SelectionChanged 路由事件
            var args = new FileSelectionChangedEventArgs(SelectionChangedEvent, this, SelectedItems.ToArray());
            RaiseEvent(args);
        }
    }

    private void ItemsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (this.itemsList?.SelectedItem is not FileSystemItemInfo selectedItem)
        {
            return;
        }

        if (selectedItem.IsDirectory)
        {
            CurrentPath = selectedItem.FullPath;
        }
        else
        {
            // 1. 执行命令 (如果已绑定)
            ICommand command = FileDoubleClickCommand;
            if (command != null && command.CanExecute(selectedItem))
            {
                command.Execute(selectedItem);
            }

            // 2. 触发路由事件
            var args = new FileDoubleClickEventArgs(FileDoubleClickEvent, this, e, selectedItem);
            RaiseEvent(args);
        }
    }

    private object CreateHeader(string text, ImageSource? iconSource)
    {
        var stackPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal
        };

        _ = stackPanel.Children.Add(new Image { Source = iconSource, Width = 16, Height = 16, Margin = new Thickness(0, 0, 5, 0) });
        _ = stackPanel.Children.Add(new TextBlock { Text = text });
        return stackPanel;
    }

    /// <summary>
    /// 新增的辅助方法：将一个 TreeViewItem 恢复到其初始的、未加载的状态并折叠它.
    /// </summary>
    private void RestoreVirtualNodeAndCollapse(TreeViewItem item)
    {
        // 确保在UI线程上执行，因为可能从 catch 块调用
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => RestoreVirtualNodeAndCollapse(item));
            return;
        }

        item.Items.Clear();
        _ = item.Items.Add(null); // 重新添加虚拟项

        // 使用 Dispatcher.BeginInvoke 将折叠操作推迟到当前UI周期之后，确保操作成功.
        _ = Dispatcher.BeginInvoke(() => item.IsExpanded = false, System.Windows.Threading.DispatcherPriority.Input);
    }

    private static void OnFilterOrPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 回调的职责非常单一：如果控件已加载，就触发一次目录加载.
        // 所有复杂的逻辑都移到 LoadDirectoryAsync 和 DrivesTree_Expanded 内部.
        if (d is FileExplorer control && control.isLoaded)
        {
            // 当 FileFilter, 或 IsFilterFolder 改变时，清空缓存
            if (e.Property == IsFilterFolderProperty || e.Property == FileFilterProperty)
            {
                // 过滤器改变，缓存失效，必须清除
                control.folderCache.Clear();
            }

            // 当 CurrentPath, FileFilter, 或 IsFilterFolder 改变时，重新加载当前目录
            control.LoadDirectoryAsync(control.CurrentPath);
        }
    }

    private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FileExplorer control && control != null && control.IsInitialized)
        {
            if (control.itemsList != null)
            {
                // 更新 ListView 的 SelectionMode
                control.itemsList.SelectionMode = (SelectionMode)e.NewValue;
            }
            else
            {
                // 如果 itemsList 还未初始化，可以考虑在 OnApplyTemplate 中处理
                control.SelectionMode = (SelectionMode)e.OldValue; // 保持一致性
            }
        }
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 0)
        {
            return string.Empty;
        }

        if (bytes == 0)
        {
            return "0 B";
        }

        var suffixes = new[] { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1 && counter < suffixes.Length - 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:n1} {suffixes[counter]}";
    }

    private async Task<List<DirectoryInfo>> GetValidSubdirectoriesAsync(string path, CancellationToken token)
    {
        // 检查缓存
        if (folderCache.TryGetValue(path, out List<DirectoryInfo>? cachedDirs))
        {
            return cachedDirs;
        }

        List<DirectoryInfo> validDirs;
        var dirInfo = new DirectoryInfo(path);

        // 解析过滤器
        string[]? searchPatterns = null;
        if (IsFilterFolder && !string.IsNullOrEmpty(FileFilter))
        {
            searchPatterns = FileFilter.Split(',')
                                       .Select(f => $"*{f.Trim().TrimStart('*')}")
                                       .Where(f => f.Length > 1)
                                       .ToArray();
        }

        if (searchPatterns != null && searchPatterns.Length > 0)
        {
            // 需要过滤：进行深度扫描
            HashSet<string> validCache = await dirInfo.BuildValidDirectoryCacheAsync(searchPatterns, token);
            if (token.IsCancellationRequested)
            {
                return new List<DirectoryInfo>();
            }

            validDirs = new List<DirectoryInfo>();
            foreach (DirectoryInfo subDir in dirInfo.EnumerateDirectories())
            {
                if ((subDir.Attributes & FileAttributes.Hidden) == 0 && (subDir.Attributes & FileAttributes.System) == 0)
                {
                    if (validCache.Contains(subDir.FullName))
                    {
                        validDirs.Add(subDir);
                    }
                }
            }
        }
        else
        {
            // 无需过滤：获取顶层目录
            validDirs = await Task.Run(
                () =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return new List<DirectoryInfo>();
                    }

                    return dirInfo.EnumerateDirectories()
                                  .Where(d => (d.Attributes & FileAttributes.Hidden) == 0 && (d.Attributes & FileAttributes.System) == 0)
                                  .ToList();
                },
                token
            );
        }

        if (!token.IsCancellationRequested)
        {
            folderCache[path] = validDirs; // 更新缓存
        }

        return validDirs;
    }

    /// <summary>
    /// 封装了获取系统图标的P/Invoke调用.
    /// 采用混合策略以兼容 Windows 7 到 Windows 11 的所有版本.
    /// </summary>
    private static class IconManager
    {
        // 于在程序启动时检测一次操作系统版本.
        private static readonly bool IsWindows10OrGreater = Environment.OSVersion.Version.Major >= 10;

        /// <summary>
        /// // 获取文件夹图标 (SHGetFileInfo 是所有系统版本通用的最佳选择)
        /// </summary>
        public static ImageSource? GetFolderIcon() => GetIconForFileAttributes(FileAttributes.Directory);

        /// <summary>
        /// 获取“此电脑”图标 (SHGetStockIconInfo 在这里仍然是最佳选择)
        /// </summary>
        public static ImageSource? GetComputerIcon() => GetIconFromKnownFolder(NativeMethods.FOLDERID_ComputerFolder);

        /// <summary>
        /// 获取文件图标 (SHGetFileInfo 是所有系统版本通用的最佳选择)
        /// </summary>
        public static ImageSource? GetFileIcon(string filePath) => GetIcon(filePath, true);

        /// <summary>
        /// 获取驱动器图标的混合方法.
        /// </summary>
        /// <param name="driveInfo">包含驱动器信息的 DriveInfo 对象.</param>
        public static ImageSource? GetDriveIcon(DriveInfo driveInfo)
        {
            if (IsWindows10OrGreater)
            {
                // 如果是 Windows 10/11 或更新版本，使用 SHGetFileInfo 获取现代图标.
                return GetIcon(driveInfo.Name, true);
            }
            else
            {
                // 如果是 Windows 7/8/8.1，使用 SHGetStockIconInfo 获取经典图标.
                StockIconId id = driveInfo.DriveType switch
                {
                    DriveType.Removable => StockIconId.Usb,   // U盘
                    DriveType.Fixed => StockIconId.HardDrive, // 本地硬盘, 移动硬盘
                    DriveType.Network => StockIconId.NetworkDrive,
                    DriveType.CDRom => StockIconId.CdRom,
                    _ => StockIconId.HardDrive,
                };

                return GetStockIcon(id, SHGSI.SmallIcon);
            }
        }

        private static ImageSource? GetIcon(string path, bool isSmall)
        {
            SHGFI flags = SHGFI.Icon | SHGFI.UseFileAttributes;
            flags |= isSmall ? SHGFI.SmallIcon : SHGFI.LargeIcon;

            SHFileInfo shfi = new();
            var hImgList = NativeMethods.SHGetFileInfo(path, 0, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

            if (hImgList != IntPtr.Zero && shfi.IconHandle != IntPtr.Zero)
            {
                try
                {
                    ImageSource icon = Imaging.CreateBitmapSourceFromHIcon(shfi.IconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    icon.Freeze();
                    return icon;
                }
                finally
                {
                    _ = NativeMethods.DestroyIcon(shfi.IconHandle);
                }
            }

            return null;
        }

        private static ImageSource? GetIconForFileAttributes(FileAttributes attributes, bool isSmall = true)
        {
            SHGFI flags = SHGFI.Icon | SHGFI.UseFileAttributes;
            flags |= isSmall ? SHGFI.SmallIcon : SHGFI.LargeIcon;

            SHFileInfo shfi = new();

            // 给一个虚拟路径，因为我们只关心属性
            var hImgList = NativeMethods.SHGetFileInfo("dummy", attributes, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

            if (hImgList != IntPtr.Zero && shfi.IconHandle != IntPtr.Zero)
            {
                try
                {
                    ImageSource icon = Imaging.CreateBitmapSourceFromHIcon(shfi.IconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    icon.Freeze();
                    return icon;
                }
                finally
                {
                    _ = NativeMethods.DestroyIcon(shfi.IconHandle);
                }
            }

            return null;
        }

        private static ImageSource? GetStockIcon(StockIconId id, SHGSI flags)
        {
            // 1. 创建一个 SHStockIconInfo 结构体实例 , 并初始化其大小
            SHStockIconInfo info = new();
            info.CbSize = (uint)Marshal.SizeOf(info);

            // 2. 获取系统图标信息
            int hResult = NativeMethods.SHGetStockIconInfo(id, flags, ref info);
            if (hResult != 0 || info.HIcon == IntPtr.Zero)
            {
                // 如果调用失败或没有返回有效的图标句柄，则直接返回 null
                // 调用者需要处理 null情况（例如，显示一个默认图标）
                return null;
            }

            // 3. 使用 try...finally 确保资源被释放
            try
            {
                // 从有效的句柄创建WPF ImageSource
                ImageSource icon = Imaging.CreateBitmapSourceFromHIcon(info.HIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                // [最佳实践] 冻结ImageSource以提高性能并使其线程安全
                icon.Freeze();
                return icon;
            }
            finally
            {
                // 4. 无论成功与否，都必须销毁GDI图标句柄以防止资源泄漏
                _ = NativeMethods.DestroyIcon(info.HIcon);
            }
        }

        /// <summary>
        /// 使用 SHGetFileInfo 和 "此电脑" 的 CLSID 来获取图标.
        /// </summary>
        private static ImageSource? GetComputerIconWithShell()
        {
            // "此电脑" 的官方 CLSID 路径
            const string computerClsid = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";

            // 注意：这里我们不使用 UseFileAttributes 标志，因为我们要解析路径
            SHGFI flags = SHGFI.Icon | SHGFI.SmallIcon;

            SHFileInfo shfi = new();

            // 直接将 CLSID 路径传递给 SHGetFileInfo
            var hImgList = NativeMethods.SHGetFileInfo(computerClsid, 0, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

            if (hImgList != IntPtr.Zero && shfi.IconHandle != IntPtr.Zero)
            {
                try
                {
                    ImageSource icon = Imaging.CreateBitmapSourceFromHIcon(shfi.IconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    icon.Freeze();
                    return icon;
                }
                finally
                {
                    _ = NativeMethods.DestroyIcon(shfi.IconHandle);
                }
            }

            return null;
        }

        /// <summary>
        /// 新增：从一个已知的系统文件夹ID（GUID）获取图标.
        /// </summary>
        public static ImageSource? GetIconFromKnownFolder(Guid knownFolderId)
        {
            // 步骤1: 获取指向已知文件夹的 PIDL
            int hResult = NativeMethods.SHGetKnownFolderIDList(ref knownFolderId, 0, IntPtr.Zero, out IntPtr pidl);
            if (hResult != 0) // S_OK is 0
            {
                return null;
            }

            try
            {
                // 步骤2: 使用 PIDL 获取图标信息
                SHFileInfo shfi = new();
                SHGFI flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.PIDL; // 关键：使用 PIDL 标志
                var hImgList = NativeMethods.SHGetFileInfo(pidl, 0, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

                if (hImgList != IntPtr.Zero && shfi.IconHandle != IntPtr.Zero)
                {
                    try
                    {
                        ImageSource icon = Imaging.CreateBitmapSourceFromHIcon(shfi.IconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        icon.Freeze();
                        return icon;
                    }
                    finally
                    {
                        // 必须销毁 SHGetFileInfo 返回的图标句柄
                        _ = NativeMethods.DestroyIcon(shfi.IconHandle);
                    }
                }
            }
            finally
            {
                // 关键：无论成功与否，都必须释放由 SHGetKnownFolderIDList 分配的 PIDL 内存
                if (pidl != IntPtr.Zero)
                {
                    NativeMethods.CoTaskMemFree(pidl);
                }
            }

            return null;
        }
    }

    private class NativeMethods
    {
        public const int WM_DEVICECHANGE = 0x0219; // 系统消息常量
        public const int DBT_DEVTYP_VOLUME = 0x00000002; // 这个值用于标识设备类型为逻辑卷（Logical Volume）
        public const int DBT_DEVICEARRIVAL = 0x8000; // 当设备插入并可用时
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // 当设备被移除时
        public static readonly Guid FOLDERID_ComputerFolder = new Guid("0AC0837C-BBF8-452A-850D-79D08E667CA7");

        // --- 新增：常用系统文件夹的 GUID ---
        public static readonly Guid FOLDERID_Desktop = new("B4BFCC3A-DB2C-424C-B029-7FE99A87C641");
        public static readonly Guid FOLDERID_Documents = new("FDD39AD0-238F-46AF-ADB4-6C85480369C7");
        public static readonly Guid FOLDERID_Downloads = new("374DE290-123F-4565-9164-39C4925E467B");
        public static readonly Guid FOLDERID_Music = new("4BD8D571-6D19-48D3-BE97-422220080E43");
        public static readonly Guid FOLDERID_Pictures = new("33E28130-4E1E-4676-835A-98395C3BC3BB");
        public static readonly Guid FOLDERID_Videos = new("18989B1D-99B5-455B-841C-AB7C74E4DDFC");

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHGetFileInfo(string pszPath, FileAttributes dwFileAttributes, ref SHFileInfo psfi, uint cbFileInfo, SHGFI uFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHGetFileInfo")]
        public static extern IntPtr SHGetFileInfo(IntPtr pidl, uint dwFileAttributes, ref SHFileInfo psfi, uint cbFileInfo, SHGFI uFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHGetStockIconInfo(StockIconId iconId, SHGSI uFlags, ref SHStockIconInfo iconInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll")]
        public static extern int SHGetKnownFolderIDList(ref Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppidl);

        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr pv);

    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DevBroadcastHDR
    {
        public int Size;
        public int DeviceType;
        public int Reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFileInfo
    {
        public IntPtr IconHandle;
        public int Icon;
        public uint Attributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string TypeName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHStockIconInfo
    {
        public uint CbSize;
        public IntPtr HIcon;
        public int SysIconIndex;
        public int Icon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string SzPath;
    }

    [Flags]
    private enum SHGFI : uint
    {
        Icon = 0x100,
        DisplayName = 0x200,
        LargeIcon = 0x0,
        SmallIcon = 0x1,
        UseFileAttributes = 0x10,

        /// <summary>
        /// 告诉 SHGetFileInfo 我们传递的是一个 PIDL
        /// </summary>
        PIDL = 0x8,
    }

    [Flags]
    private enum SHGSI : uint
    {
        Icon = 0x000000100,
        SmallIcon = 0x000000001
    }

    private enum StockIconId : uint
    {
        File = 0,
        Folder = 3,
        HardDrive = 8,
        Usb = 7,
        Computer = 15,
        NetworkDrive = 9,
        CdRom = 11,
    }
}