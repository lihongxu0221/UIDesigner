using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace BgCommon.UI.Designer.Theming
{
    /// <summary>
    /// 主题信息
    /// </summary>
    public class ThemeInfo
    {
        /// <summary>
        /// 主题名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 主题描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 主题文件路径
        /// </summary>
        public string Path { get; set; }
    }
    
    /// <summary>
    /// 主题管理器
    /// </summary>
    public class ThemeManager
    {
        private readonly List<ThemeInfo> _themes = new List<ThemeInfo>();
        private ThemeInfo _currentTheme;
        
        /// <summary>
        /// 主题集合
        /// </summary>
        public IReadOnlyList<ThemeInfo> Themes => _themes.AsReadOnly();
        
        /// <summary>
        /// 当前主题
        /// </summary>
        public ThemeInfo CurrentTheme => _currentTheme;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public ThemeManager()
        {
            // 加载内置主题
            LoadBuiltInThemes();
        }
        
        /// <summary>
        /// 加载内置主题
        /// </summary>
        private void LoadBuiltInThemes()
        {
            // 添加默认主题
            _themes.Add(new ThemeInfo
            {
                Name = "默认主题",
                Description = "系统默认主题",
                Path = null
            });
            
            // 可以在这里添加其他内置主题
        }
        
        /// <summary>
        /// 加载主题目录
        /// </summary>
        /// <param name="themeDirectory">主题目录</param>
        public void LoadThemes(string themeDirectory)
        {
            if (!Directory.Exists(themeDirectory))
            {
                Directory.CreateDirectory(themeDirectory);
                return;
            }
            
            var themeFiles = Directory.GetFiles(themeDirectory, "*.xaml");
            foreach (var file in themeFiles)
            {
                try
                {
                    // 尝试加载主题文件以获取主题信息
                    var themeInfo = new ThemeInfo
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Description = "自定义主题",
                        Path = file
                    };
                    
                    _themes.Add(themeInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载主题文件 {file} 失败: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 应用主题
        /// </summary>
        /// <param name="theme">主题信息</param>
        public void ApplyTheme(ThemeInfo theme)
        {
            if (theme == null)
                return;
            
            try
            {
                // 清除现有的主题资源
                ClearThemeResources();
                
                // 应用新主题
                if (!string.IsNullOrEmpty(theme.Path))
                {
                    using (var stream = new FileStream(theme.Path, FileMode.Open))
                    {
                        var resourceDictionary = (ResourceDictionary)XamlReader.Load(stream);
                        Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                    }
                }
                
                _currentTheme = theme;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"应用主题 {theme.Name} 失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 清除主题资源
        /// </summary>
        private void ClearThemeResources()
        {
            // 清除自定义主题资源
            var customThemes = Application.Current.Resources.MergedDictionaries.FindAll(rd => rd.Source != null && rd.Source.OriginalString.EndsWith(".xaml"));
            foreach (var rd in customThemes)
            {
                Application.Current.Resources.MergedDictionaries.Remove(rd);
            }
        }
        
        /// <summary>
        /// 获取主题
        /// </summary>
        /// <param name="name">主题名称</param>
        /// <returns>主题信息</returns>
        public ThemeInfo GetTheme(string name)
        {
            return _themes.Find(t => t.Name == name);
        }
    }
}