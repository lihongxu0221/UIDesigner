using ICSharpCode.WpfDesign;

namespace BgCommon.UI.Designer.Export
{
    /// <summary>
    /// 导出服务
    /// </summary>
    public class ExportService
    {
        private readonly ExportManager _exportManager;
        
        /// <summary>
        /// 导出管理器
        /// </summary>
        public ExportManager ExportManager => _exportManager;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">设计上下文</param>
        public ExportService(DesignContext context)
        {
            _exportManager = new ExportManager(context);
        }
    }
}