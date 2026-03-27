using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BgCommon.UI.Designer.UIConfigurationFramework;

namespace UIDesignerDemo
{
    public partial class MainWindow : Window
    {
        private readonly ConfigurationEditor _configurationEditor;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // 初始化配置编辑器
            _configurationEditor = new ConfigurationEditor();
            
            // 设置数据上下文
            DataContext = _configurationEditor;
        }
        
        private void ComponentItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ComponentDefinition component)
            {
                // 处理组件拖放开始
                StartComponentDrag(component);
            }
        }
        
        private void StartComponentDrag(ComponentDefinition component)
        {
            // 这里实现组件拖放逻辑
            // 实际实现会使用框架的拖放服务
            MessageBox.Show($"开始拖放组件: {component.DisplayName}", "拖放操作", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}