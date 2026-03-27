namespace BgCommon.UI.Designer.Security
{
    /// <summary>
    /// 安全服务
    /// </summary>
    public class SecurityService
    {
        private readonly SecurityManager _securityManager;
        
        /// <summary>
        /// 安全管理器
        /// </summary>
        public SecurityManager SecurityManager => _securityManager;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SecurityService()
        {
            _securityManager = new SecurityManager();
        }
    }
}