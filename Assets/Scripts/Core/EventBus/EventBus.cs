// 所属模块: Core.EventBus, 职责: 泛型静态事件总线，提供高性能零反射的事件分发
using System;
using System.Collections.Generic;

namespace ARPG.Core.EventBus
{
    /// <summary>
    /// 泛型事件总线，每个事件类型 (T) 会生成一个独立的静态订阅者列表
    /// </summary>
    /// <typeparam name="T">事件数据结构，限定为值类型 (struct)，减少 GC 开销</typeparam>
    public static class EventBus<T> where T : struct
    {
        private static readonly List<Action<T>> _subscribers = new ();
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="callback"></param>
        public static void Subscribe(Action<T> callback)
        {
            if (!_subscribers.Contains(callback))
            {
                _subscribers.Add(callback);
            }
        }
        
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <param name="callback"></param>
        public static void Unsubscribe(Action<T> callback)
        {
            if (_subscribers.Contains(callback))
            {
                _subscribers.Remove(callback);
            }
        }
        
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public static void Publish(T eventData)
        {
            // 倒序遍历以防在回调中取消订阅导致的问题
            for (int i = _subscribers.Count - 1; i >= 0; i--)
            {
                _subscribers[i]?.Invoke(eventData);
            }
        }
    }

    /// <summary>
    /// 事件订阅标记特性，可应用于方法上，供自动化扫描系统识别
    /// </summary>
    /// <remarks>
    /// 使用方法：将此特性放在无参或特定签名的方法上，
    /// 然后由专门的初始化系统通过反射查找并自动完成订阅。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventSubscriberAttribute : Attribute { }
}
