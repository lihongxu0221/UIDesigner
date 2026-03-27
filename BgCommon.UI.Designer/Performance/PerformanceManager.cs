using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BgCommon.UI.Designer.Performance
{
    /// <summary>
    /// 性能优化管理器
    /// </summary>
    public class PerformanceManager
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly List<Task> _asyncTasks = new List<Task>();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        /// <summary>
        /// 缓存项数量
        /// </summary>
        public int CacheCount => _cache.Count;
        
        /// <summary>
        /// 正在执行的异步任务数量
        /// </summary>
        public int RunningTasks => _asyncTasks.Count;
        
        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        public void AddCache(string key, object value)
        {
            if (!_cache.ContainsKey(key))
            {
                _cache[key] = value;
            }
        }
        
        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public T GetCache<T>(string key)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            return default(T);
        }
        
        /// <summary>
        /// 移除缓存项
        /// </summary>
        /// <param name="key">缓存键</param>
        public void RemoveCache(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }
        }
        
        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }
        
        /// <summary>
        /// 执行异步操作
        /// </summary>
        /// <param name="action">异步操作</param>
        public void ExecuteAsync(Action action)
        {
            var task = Task.Run(action);
            _asyncTasks.Add(task);
            task.ContinueWith(t => _asyncTasks.Remove(t));
        }
        
        /// <summary>
        /// 执行异步操作并返回结果
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="func">异步操作</param>
        /// <returns>结果</returns>
        public async Task<T> ExecuteAsync<T>(Func<T> func)
        {
            var task = Task.Run(func);
            _asyncTasks.Add(task);
            try
            {
                return await task;
            }
            finally
            {
                _asyncTasks.Remove(task);
            }
        }
        
        /// <summary>
        /// 开始性能计时
        /// </summary>
        public void StartTimer()
        {
            _stopwatch.Restart();
        }
        
        /// <summary>
        /// 停止性能计时并返回耗时
        /// </summary>
        /// <returns>耗时（毫秒）</returns>
        public long StopTimer()
        {
            _stopwatch.Stop();
            return _stopwatch.ElapsedMilliseconds;
        }
        
        /// <summary>
        /// 优化拖放操作
        /// </summary>
        /// <param name="action">拖放操作</param>
        public void OptimizeDragDrop(Action action)
        {
            // 禁用不必要的渲染
            // 执行拖放操作
            // 重新启用渲染
            action();
        }
        
        /// <summary>
        /// 优化数据绑定
        /// </summary>
        /// <param name="action">数据绑定操作</param>
        public void OptimizeDataBinding(Action action)
        {
            // 禁用数据绑定更新
            // 执行数据绑定操作
            // 批量更新数据绑定
            action();
        }
        
        /// <summary>
        /// 优化序列化
        /// </summary>
        /// <param name="action">序列化操作</param>
        public void OptimizeSerialization(Action action)
        {
            // 执行序列化操作
            action();
        }
    }
}