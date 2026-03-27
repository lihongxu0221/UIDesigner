using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace BgCommon.UI.Designer.UIConfigurationFramework.Demo
{
    /// <summary>
    /// UI组态框架演示类
    /// </summary>
    public static class FrameworkDemo
    {
        /// <summary>
        /// 运行框架演示
        /// </summary>
        public static void RunDemo()
        {
            Console.WriteLine("=== UI组态框架演示开始 ===\n");
            
            DemoBasicUsage();
            DemoConfigurationManagement();
            DemoComponentRendering();
            DemoAdvancedFeatures();
            
            Console.WriteLine("\n=== UI组态框架演示完成 ===");
        }
        
        /// <summary>
        /// 演示基本用法
        /// </summary>
        private static void DemoBasicUsage()
        {
            Console.WriteLine("1. 基本用法演示");
            Console.WriteLine("----------------");
            
            // 创建框架实例
            var framework = new UIConfigurationFramework();
            
            // 查看已注册的组件
            var categories = framework.ComponentRegistry.GetCategories();
            Console.WriteLine($"可用分类: {string.Join(", ", categories)}");
            
            foreach (var category in categories)
            {
                var components = framework.ComponentRegistry.GetComponentsByCategory(category);
                Console.WriteLine($"\n{category}:");
                foreach (var component in components)
                {
                    Console.WriteLine($"  - {component.DisplayName} ({component.Type.Name})");
                }
            }
            
            Console.WriteLine("\n✓ 基本用法演示完成");
        }
        
        /// <summary>
        /// 演示配置管理
        /// </summary>
        private static void DemoConfigurationManagement()
        {
            Console.WriteLine("\n2. 配置管理演示");
            Console.WriteLine("----------------");
            
            var framework = new UIConfigurationFramework();
            
            // 创建复杂配置
            var complexConfig = new UIConfiguration
            {
                Name = "复杂界面配置",
                Description = "包含多种组件的复杂界面配置示例",
                Version = "1.0",
                Layout = new LayoutConfiguration
                {
                    LayoutType = "Grid",
                    Width = 600,
                    Height = 400,
                    Properties = new Dictionary<string, object>
                    {
                        { "Background", "#F5F5F5" }
                    }
                },
                Theme = new ThemeConfiguration
                {
                    ThemeName = "现代主题",
                    Colors = new Dictionary<string, string>
                    {
                        { "Primary", "#3498DB" },
                        { "Secondary", "#2C3E50" }
                    }
                }
            };
            
            // 添加多个组件
            complexConfig.Components.AddRange(new[]
            {
                new ComponentConfiguration
                {
                    ComponentType = typeof(Button).FullName,
                    Name = "主按钮",
                    Properties = new Dictionary<string, object>
                    {
                        { "Content", "主要操作" },
                        { "Width", 120.0 },
                        { "Height", 40.0 },
                        { "Background", "#3498DB" },
                        { "Foreground", "White" },
                        { "FontSize", 14.0 },
                        { "Margin", "20,20,0,0" }
                    }
                },
                new ComponentConfiguration
                {
                    ComponentType = typeof(TextBox).FullName,
                    Name = "搜索框",
                    Properties = new Dictionary<string, object>
                    {
                        { "Text", "请输入搜索内容..." },
                        { "Width", 300.0 },
                        { "Height", 30.0 },
                        { "Margin", "20,70,0,0" }
                    }
                },
                new ComponentConfiguration
                {
                    ComponentType = typeof(CheckBox).FullName,
                    Name = "选项1",
                    Properties = new Dictionary<string, object>
                    {
                        { "Content", "启用选项一" },
                        { "IsChecked", true },
                        { "Margin", "20,120,0,0" }
                    }
                },
                new ComponentConfiguration
                {
                    ComponentType = typeof(ComboBox).FullName,
                    Name = "下拉选择",
                    Properties = new Dictionary<string, object>
                    {
                        { "Width", 200.0 },
                        { "Height", 30.0 },
                        { "Margin", "20,170,0,0" }
                    }
                }
            });
            
            // 加载配置
            framework.LoadConfiguration(complexConfig);
            Console.WriteLine("✓ 复杂配置创建成功");
            
            // 演示JSON导出
            var json = JsonSerializer.Serialize(complexConfig, 
                new JsonSerializerOptions { WriteIndented = true });
            
            Console.WriteLine($"\n配置JSON预览 (前200字符):");
            Console.WriteLine(json.Length > 200 ? json.Substring(0, 200) + "..." : json);
            
            Console.WriteLine("\n✓ 配置管理演示完成");
        }
        
        /// <summary>
        /// 演示组件渲染
        /// </summary>
        private static void DemoComponentRendering()
        {
            Console.WriteLine("\n3. 组件渲染演示");
            Console.WriteLine("----------------");
            
            var framework = new UIConfigurationFramework();
            var renderer = new ComponentRenderer(framework.ComponentRegistry, framework.DesignSurface);
            
            // 测试各种组件配置
            var testConfigs = new[]
            {
                new ComponentConfiguration
                {
                    ComponentType = typeof(Button).FullName,
                    Properties = new Dictionary<string, object>
                    {
                        { "Content", "带样式的按钮" },
                        { "Background", "#E74C3C" },
                        { "Foreground", "White" },
                        { "FontWeight", "Bold" }
                    }
                },
                new ComponentConfiguration
                {
                    ComponentType = typeof(TextBlock).FullName,
                    Properties = new Dictionary<string, object>
                    {
                        { "Text", "这是文本块" },
                        { "FontSize", 16.0 },
                        { "Foreground", "#2C3E50" }
                    }
                },
                new ComponentConfiguration
                {
                    ComponentType = typeof(ProgressBar).FullName,
                    Properties = new Dictionary<string, object>
                    {
                        { "Value", 75.0 },
                        { "Width", 200.0 },
                        { "Height", 20.0 }
                    }
                }
            };
            
            Console.WriteLine("组件配置验证结果:");
            foreach (var config in testConfigs)
            {
                var isValid = renderer.ValidateComponentConfiguration(config);
                Console.WriteLine($"  - {config.ComponentType}: {(isValid ? "✓ 有效" : "✗ 无效")}");
            }
            
            Console.WriteLine("\n✓ 组件渲染演示完成");
        }
        
        /// <summary>
        /// 演示高级功能
        /// </summary>
        private static void DemoAdvancedFeatures()
        {
            Console.WriteLine("\n4. 高级功能演示");
            Console.WriteLine("----------------");
            
            var framework = new UIConfigurationFramework();
            
            // 演示程序集扫描
            Console.WriteLine("程序集扫描功能:");
            
            // 扫描当前程序集中的组件
            var currentAssembly = typeof(FrameworkDemo).Assembly;
            framework.ComponentRegistry.ScanAssemblyForComponents(currentAssembly, "演示组件");
            
            var demoComponents = framework.ComponentRegistry.GetComponentsByCategory("演示组件");
            Console.WriteLine($"扫描到 {demoComponents.Count()} 个演示组件");
            
            // 演示配置版本管理
            Console.WriteLine("\n配置版本管理:");
            
            var versionedConfig = new UIConfiguration
            {
                Name = "版本化配置",
                Version = "2.1.0",
                CreatedDate = DateTime.Now.AddDays(-30),
                ModifiedDate = DateTime.Now
            };
            
            Console.WriteLine($"配置版本: {versionedConfig.Version}");
            Console.WriteLine($"创建时间: {versionedConfig.CreatedDate:yyyy-MM-dd}");
            Console.WriteLine($"修改时间: {versionedConfig.ModifiedDate:yyyy-MM-dd HH:mm}");
            
            // 演示布局约束
            Console.WriteLine("\n布局约束功能:");
            
            var constrainedComponent = new ComponentConfiguration
            {
                ComponentType = typeof(Button).FullName,
                LayoutConstraints = new LayoutConstraints
                {
                    MinWidth = 80.0,
                    MaxWidth = 200.0,
                    MinHeight = 30.0,
                    CanResize = true,
                    CanMove = true
                }
            };
            
            Console.WriteLine("布局约束设置:");
            Console.WriteLine($"  - 最小宽度: {constrainedComponent.LayoutConstraints.MinWidth}");
            Console.WriteLine($"  - 最大宽度: {constrainedComponent.LayoutConstraints.MaxWidth}");
            Console.WriteLine($"  - 可调整大小: {constrainedComponent.LayoutConstraints.CanResize}");
            
            Console.WriteLine("\n✓ 高级功能演示完成");
        }
        
        /// <summary>
        /// 生成使用指南
        /// </summary>
        public static void GenerateUsageGuide()
        {
            var guide = $"""
UI组态框架使用指南
================

快速开始
--------

1. 安装和引用
```csharp
// 添加项目引用
// 使用命名空间
using BgCommon.UI.Designer.UIConfigurationFramework;
```

2. 基本使用
```csharp
// 创建框架实例
var framework = new UIConfigurationFramework();

// 创建配置
var config = new UIConfiguration
{
    Name = "我的界面",
    Description = "示例配置"
};

// 添加组件
config.Components.Add(new ComponentConfiguration
{
    ComponentType = typeof(Button).FullName,
    Name = "按钮1",
    Properties = new Dictionary<string, object>
    {
        { "Content", "点击我" },
        { "Width", 100.0 },
        { "Height", 30.0 }
    }
});

// 加载配置
framework.LoadConfiguration(config);
```

核心功能
--------

### 组件管理
```csharp
// 注册自定义组件
var registry = framework.ComponentRegistry;
registry.RegisterComponent(new ComponentDefinition
{
    Type = typeof(MyCustomControl),
    Category = "自定义",
    DisplayName = "我的控件"
});

// 扫描程序集
registry.ScanAssemblyForComponents(assembly, "第三方组件");
```

### 配置管理
```csharp
// 保存配置
var manager = framework.ConfigurationManager;
manager.SaveToJsonFile("myconfig.json");

// 加载配置
var loadedConfig = manager.LoadFromJsonFile("myconfig.json");
```

### 组件渲染
```csharp
var renderer = new ComponentRenderer(registry, designSurface);

// 验证配置
bool isValid = renderer.ValidateComponentConfiguration(config);

// 创建预览
var preview = renderer.CreateComponentPreview(config);
```

高级特性
--------

### 布局约束
```csharp
var component = new ComponentConfiguration
{
    LayoutConstraints = new LayoutConstraints
    {
        MinWidth = 100,
        MaxWidth = 300,
        CanResize = true
    }
};
```

### 主题支持
```csharp
config.Theme = new ThemeConfiguration
{
    ThemeName = "深色主题",
    Colors = new Dictionary<string, string>
    {
        { "Primary", "#2C3E50" },
        { "Background", "#34495E" }
    }
};
```

### 事件处理
```csharp
component.Events = new Dictionary<string, string>
{
    { "Click", "OnButtonClick" }
};
```

最佳实践
--------

1. **组件分类**
   - 按功能对组件进行分类
   - 使用有意义的显示名称

2. **配置版本化**
   - 为配置添加版本信息
   - 记录创建和修改时间

3. **错误处理**
   - 验证组件配置的有效性
   - 处理文件操作异常

4. **性能优化**
   - 避免频繁的配置重载
   - 使用合适的布局约束

示例应用
--------

框架包含完整的示例应用，展示:
- 可视化配置编辑器
- 实时预览功能
- 组件库管理
- 导入导出功能

参考文档
--------

- 框架API文档
- 示例代码库
- 测试用例参考

""";
            
            var guidePath = Path.Combine(Path.GetTempPath(), "UIConfigurationFramework_UsageGuide.txt");
            File.WriteAllText(guidePath, guide);
            Console.WriteLine($"使用指南已生成: {guidePath}");
        }
    }
}