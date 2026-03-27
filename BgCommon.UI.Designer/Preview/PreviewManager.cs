using System;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.WpfDesign;

namespace BgCommon.UI.Designer.Preview
{
    /// <summary>
    /// 设备尺寸
    /// </summary>
    public class DeviceSize
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        
        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; set; }
    }
    
    /// <summary>
    /// 预览管理器
    /// </summary>
    public class PreviewManager
    {
        private readonly DesignContext _context;
        private Window _previewWindow;
        private ContentControl _previewContent;
        
        /// <summary>
        /// 设备尺寸列表
        /// </summary>
        public List<DeviceSize> DeviceSizes { get; } = new List<DeviceSize>
        {
            new DeviceSize { Name = "桌面", Width = 1920, Height = 1080 },
            new DeviceSize { Name = "平板", Width = 768, Height = 1024 },
            new DeviceSize { Name = "手机", Width = 375, Height = 667 }
        };
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">设计上下文</param>
        public PreviewManager(DesignContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// 显示预览
        /// </summary>
        /// <param name="deviceSize">设备尺寸</param>
        public void ShowPreview(DeviceSize deviceSize = null)
        {
            if (_previewWindow == null)
            {
                _previewWindow = new Window
                {
                    Title = "预览",
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                _previewContent = new ContentControl
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                
                _previewWindow.Content = _previewContent;
            }
            
            // 生成预览内容
            var previewContent = GeneratePreviewContent();
            _previewContent.Content = previewContent;
            
            // 设置窗口大小
            if (deviceSize != null)
            {
                _previewWindow.Width = deviceSize.Width + 20;
                _previewWindow.Height = deviceSize.Height + 40;
            }
            
            _previewWindow.Show();
        }
        
        /// <summary>
        /// 生成预览内容
        /// </summary>
        /// <returns>预览内容</returns>
        private UIElement GeneratePreviewContent()
        {
            // 创建一个容器来显示预览
            var container = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
            
            // 复制设计内容到预览容器
            if (_context.RootItem != null)
            {
                // 创建一个副本以避免修改原始设计
                var rootCopy = _context.RootItem.View as UIElement;
                if (rootCopy != null)
                {
                    // 创建元素的深拷贝
                    var clonedElement = CloneElement(rootCopy);
                    container.Child = clonedElement;
                }
            }
            
            return container;
        }
        
        /// <summary>
        /// 克隆UI元素
        /// </summary>
        /// <param name="element">要克隆的元素</param>
        /// <returns>克隆的元素</returns>
        private UIElement CloneElement(UIElement element)
        {
            // 使用XamlWriter和XamlReader来克隆元素
            var xaml = System.Windows.Markup.XamlWriter.Save(element);
            return (UIElement)System.Windows.Markup.XamlReader.Parse(xaml);
        }
        
        /// <summary>
        /// 关闭预览
        /// </summary>
        public void ClosePreview()
        {
            if (_previewWindow != null)
            {
                _previewWindow.Close();
                _previewWindow = null;
                _previewContent = null;
            }
        }
        
        /// <summary>
        /// 模拟交互
        /// </summary>
        /// <param name="element">要交互的元素</param>
        /// <param name="interactionType">交互类型</param>
        public void SimulateInteraction(UIElement element, string interactionType)
        {
            // 这里可以实现交互模拟逻辑
            // 例如，模拟按钮点击、文本输入等
        }
    }
}