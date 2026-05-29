// 所属模块: Core.Module, 职责: 通过程序集扫描自动发现并注册模块
using System;
using System.Collections.Generic;
using System.Linq;

namespace ARPG.Core.Module
{
    /// <summary>
    /// 模块注册表
    /// 用于自动发现并实例化项目中所有模块入口
    /// </summary>
    public static class ModuleRegistry
    {
        /// <summary>
        /// 扫描所有已加载的程序集，找出所有实现了 <see cref="IModuleBootstrap"/> 的类，
        /// 实例化后按照优先级排序返回
        /// </summary>
        /// <returns>排序后的模块引导实例列表</returns>
        public static List<IModuleBootstrap> DiscoverModules()
        {
            // 1. 获取目标接口的 Type 对象
            var moduleType = typeof(IModuleBootstrap);
            
            // 2. 获取当前应用程序域中所有已加载的程序集
            var types = AppDomain.CurrentDomain.GetAssemblies()
                // 3. 将每个程序集中的所有类型展平成一个序列
                .SelectMany(a => a.GetTypes())
                // 4. 筛选条件：
                //    - 实现了 IModuleBootstrap 接口
                //    - 不是接口且不是抽象类（确保可以被实例化）
                .Where(t => moduleType.IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });
            
            // 5. 准备一个列表存放实例
            var modules = new List<IModuleBootstrap>();
            
            // 6. 遍历筛选出的类型
            foreach (var type in types)
            {
                try
                {
                    // 使用反射创建该类型的实例，参数为无参构造函数
                    if (Activator.CreateInstance(type) is IModuleBootstrap module)
                    {
                        modules.Add(module);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"创建模块实例失败: {type.FullName}: {e.Message}");
                }
            }
            
            // 7. 按模块声明的优先级升序排序，并返回列表
            // 优先级通常数值越小，初始化越早
            return modules.OrderBy(m => m.Priority).ToList();
        }
    }
}
