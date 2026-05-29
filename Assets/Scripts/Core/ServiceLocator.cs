// 所属模块: Core, 职责: 简单的服务定位器，用于系统层内部解耦
// TODO: 使用依赖注入框架替代
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPG.Core
{
    /// <summary>
    /// 全局服务定位器
    /// 以类型为键存储所有系统级服务的单例实例
    /// 允许项目内任意代码直接获取所需服务，避免显式传递引用
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        /// <summary>
        /// 注册服务实例
        /// 如果对应类型已注册过，则忽略（不覆盖）
        /// </summary>
        /// <typeparam name="T">服务类型，通常为接口</typeparam>
        /// <param name="service">服务实例</param>
        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (!_services.ContainsKey(type))
            {
                _services.Add(type, service);
            }
            else
            {
                Debug.LogWarning($"重复注册 {type.Name}");
            }
        }

        /// <summary>
        /// 获取指定类型的服务实例
        /// 如果服务未注册，则抛出异常，强制调用方确保正确的初始化顺序。
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>注册的服务实例</returns>
        /// <exception cref="Exception">服务未注册时抛出</exception>
        public static T Get<T>()
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            throw new Exception($"{type.Name} 服务未注册");
        }
        
        /// <summary>
        /// 移除已注册的服务实例
        /// 常用于场景卸载或手动清理时
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        public static void Unregister<T>()
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }
        
        
        // 注意：
        // 1. 全局静态状态不利于单元测试，可增加 Clear() 方法用于测试重置
        // 2. 字典非线程安全，多线程环境需改用 ConcurrentDictionary 或加锁
        // 3. 服务定位器模式应谨慎使用，避免演变为“上帝对象”。优先考虑依赖注入或事件驱动
    }
}
