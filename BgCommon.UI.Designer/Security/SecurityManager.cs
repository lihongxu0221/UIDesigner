using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BgCommon.UI.Designer.Security
{
    /// <summary>
    /// 安全管理器
    /// </summary>
    public class SecurityManager
    {
        /// <summary>
        /// 验证插件
        /// </summary>
        /// <param name="pluginPath">插件路径</param>
        /// <returns>是否验证通过</returns>
        public bool ValidatePlugin(string pluginPath)
        {
            try
            {
                // 检查插件文件是否存在
                if (!File.Exists(pluginPath))
                {
                    return false;
                }
                
                // 检查插件文件大小
                var fileInfo = new FileInfo(pluginPath);
                if (fileInfo.Length > 10 * 1024 * 1024) // 10MB
                {
                    return false;
                }
                
                // 检查插件文件类型
                var extension = Path.GetExtension(pluginPath).ToLower();
                if (extension != ".dll")
                {
                    return false;
                }
                
                // 这里可以添加更多验证逻辑，如数字签名验证等
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 验证输入
        /// </summary>
        /// <param name="input">输入内容</param>
        /// <param name="inputType">输入类型</param>
        /// <returns>是否验证通过</returns>
        public bool ValidateInput(string input, string inputType)
        {
            switch (inputType)
            {
                case "email":
                    return ValidateEmail(input);
                case "url":
                    return ValidateUrl(input);
                case "path":
                    return ValidatePath(input);
                case "name":
                    return ValidateName(input);
                default:
                    return !string.IsNullOrEmpty(input);
            }
        }
        
        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>是否验证通过</returns>
        private bool ValidateEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 验证URL
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>是否验证通过</returns>
        private bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
        
        /// <summary>
        /// 验证路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>是否验证通过</returns>
        private bool ValidatePath(string path)
        {
            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 验证名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>是否验证通过</returns>
        private bool ValidateName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.Length <= 100;
        }
        
        /// <summary>
        /// 计算文件哈希值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>哈希值</returns>
        public string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        
        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="key">加密密钥</param>
        /// <returns>加密后的数据</returns>
        public string Encrypt(string data, string key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
                aes.IV = new byte[16];
                
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(data);
                    sw.Flush();
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        
        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="encryptedData">加密的数据</param>
        /// <param name="key">解密密钥</param>
        /// <returns>解密后的数据</returns>
        public string Decrypt(string encryptedData, string key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
                aes.IV = new byte[16];
                
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(Convert.FromBase64String(encryptedData)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        
        /// <summary>
        /// 检查权限
        /// </summary>
        /// <param name="permission">权限名称</param>
        /// <param name="userId">用户ID</param>
        /// <returns>是否有权限</returns>
        public bool CheckPermission(string permission, string userId)
        {
            // 这里可以实现权限检查逻辑
            // 例如，从权限数据库中检查用户是否有指定权限
            return true;
        }
        
        /// <summary>
        /// 记录安全事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="message">事件消息</param>
        public void LogSecurityEvent(string eventType, string message)
        {
            // 这里可以实现安全事件日志记录逻辑
            Console.WriteLine($"[{eventType}] {message}");
        }
    }
}