using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using BgCommon.UI.Designer.Plugins;

namespace UIDesignerDemo.Plugins
{
    /// <summary>
    /// 基础控件插件 - 演示插件系统功能
    /// </summary>
    public class BasicControlsPlugin : IUIComponentPlugin
    {
        public string Name => "基础控件插件";
        public string Version => "1.0.0";
        public string Author => "UIDesignerDemo";
        public string Description => "提供基础WPF控件的插件";
        public string Category => "基础控件";
        
        public void Initialize(ICSharpCode.WpfDesign.DesignContext context)
        {
            // 插件初始化逻辑
            Console.WriteLine($"插件 {Name} 初始化完成");
        }
        
        public void Unload()
        {
            // 插件卸载逻辑
            Console.WriteLine($"插件 {Name} 已卸载");
        }
        
        public List<Type> GetComponentTypes()
        {
            // 返回插件提供的组件类型
            return new List<Type>
            {
                typeof(Button),
                typeof(TextBox),
                typeof(Label),
                typeof(CheckBox),
                typeof(RadioButton),
                typeof(ComboBox),
                typeof(ListBox),
                typeof(Slider),
                typeof(ProgressBar),
                typeof(Image)
            };
        }
    }
}