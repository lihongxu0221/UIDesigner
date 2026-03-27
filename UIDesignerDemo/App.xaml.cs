using System.Windows;

namespace UIDesignerDemo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 初始化UI组态框架
            InitializeUIFramework();
        }
        
        private void InitializeUIFramework()
        {
            // 这里可以初始化框架的全局设置
            // 例如：注册默认插件、设置默认数据源等
        }
    }
}