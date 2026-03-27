using ICSharpCode.WpfDesign;

namespace BgCommon.UI.Designer.Templates
{
    /// <summary>
    /// 模板服务
    /// </summary>
    public class TemplateService
    {
        private readonly TemplateManager _templateManager;
        
        /// <summary>
        /// 模板管理器
        /// </summary>
        public TemplateManager TemplateManager => _templateManager;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">设计上下文</param>
        public TemplateService(DesignContext context)
        {
            _templateManager = new TemplateManager(context);
        }
        
        /// <summary>
        /// 初始化模板服务
        /// </summary>
        /// <param name="templateDirectory">模板目录</param>
        public void Initialize(string templateDirectory = "Templates")
        {
            _templateManager.LoadTemplates(templateDirectory);
        }
    }
}