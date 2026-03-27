namespace BgCommon.UI.Designer.Theming
{
    /// <summary>
    /// 主题服务
    /// </summary>
    public class ThemeService
    {
        private readonly ThemeManager _themeManager;
        
        /// <summary>
        /// 主题管理器
        /// </summary>
        public ThemeManager ThemeManager => _themeManager;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public ThemeService()
        {
            _themeManager = new ThemeManager();
        }
        
        /// <summary>
        /// 初始化主题服务
        /// </summary>
        /// <param name="themeDirectory">主题目录</param>
        public void Initialize(string themeDirectory = "Themes")
        {
            _themeManager.LoadThemes(themeDirectory);
        }
    }
}