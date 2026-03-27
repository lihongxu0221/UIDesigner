using ICSharpCode.WpfDesign;

namespace BgCommon.UI.Designer.DataBinding
{
    /// <summary>
    /// 数据绑定服务
    /// </summary>
    public class DataBindingService
    {
        private readonly DataBindingManager _bindingManager;
        
        /// <summary>
        /// 数据绑定管理器
        /// </summary>
        public DataBindingManager BindingManager => _bindingManager;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">设计上下文</param>
        public DataBindingService(DesignContext context)
        {
            _bindingManager = new DataBindingManager(context);
        }
    }
}