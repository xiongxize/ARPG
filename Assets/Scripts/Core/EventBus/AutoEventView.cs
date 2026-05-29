// 所属模块: Core.EventBus, 职责: 自动管理事件订阅的基类，简化 View 和 UI 层开发
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace ARPG.Core.EventBus
{
    /// <summary>
    /// 自动事件订阅组件基类。
    /// 继承此类的脚本只需在方法上添加 [EventSubscriber] 特性，
    /// 即可自动订阅对应类型的全局事件，并在 OnDisable 时自动取消订阅。
    /// </summary>
    public abstract class AutoEventView : MonoBehaviour
    {
        // 存储所有取消订阅的操作，每个 Action 封装了一次 Unsubscribe 调用
        private List<Action> _unsubscriptionActions = new List<Action>();
        
        /// <summary>
        /// 组件启用时自动扫描并注册订阅
        /// </summary>
        protected virtual void OnEnable()
        {
            RegisterSubscribers();
        }
        
        /// <summary>
        /// 组件禁用时自动取消所有注册的订阅
        /// </summary>
        protected virtual void OnDisable()
        {
            UnregisterSubscribers();
        }

        /// <summary>
        /// 通过反射查找当前类中所有标记了 [EventSubscriber] 的方法，并完成订阅
        /// </summary>
        private void RegisterSubscribers()
        {
            // 获取当前实例的所有公开和非公开实例方法
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var method in methods)
            {
                // 检查方法是否具有 EventSubscriberAttribute 特性
                var attr = method.GetCustomAttribute<EventSubscriberAttribute>();
                if (attr != null)
                {
                    var parameters = method.GetParameters();
                    // 要求方法必须有且只有一个参数，该参数类型即事件类型
                    if (parameters.Length == 1)
                    {
                        var eventType = parameters[0].ParameterType;
                        // 执行反射订阅
                        SubscribeToEventBus(eventType, method);
                    }
                    // TODO：可以增加对无参或参数个数不对的警告日志
                }
            }
        }

        
        /// <summary>
        /// 通过反射将当前实例的指定方法订阅到 EventBus;
        /// </summary>
        /// <param name="eventType">事件数据结构类型（约束为 struct）</param>
        /// <param name="methodInfo">要订阅的方法信息</param
        private void SubscribeToEventBus(Type eventType, MethodInfo methodInfo)
        {
            // 通过反射调用 EventBus<T>.Subscribe
            
            // 构造封闭泛型类型 EventBus<eventType>
            var busType = typeof(EventBus<>).MakeGenericType(eventType);
            // 获取静态 Subscribe 方法
            var subscribeMethod = busType.GetMethod("Subscribe", BindingFlags.Public | BindingFlags.Static);
            
            // 构造 Action<eventType> 委托，绑定到当前对象的此方法上
            var actionType = typeof(Action<>).MakeGenericType(eventType);
            var handler = Delegate.CreateDelegate(actionType, this, methodInfo);
            
            // 调用 EventBus<T>.Subscribe(handler)
            subscribeMethod.Invoke(null, new object[] { handler });

            // 记录反注册操作：当组件禁用时调用 EventBus<T>.Unsubscribe(handler)
            _unsubscriptionActions.Add(() =>
            {
                var unsubscribeMethod = busType.GetMethod("Unsubscribe", BindingFlags.Public | BindingFlags.Static);
                unsubscribeMethod.Invoke(null, new object[] { handler });
            });
        }

        
        /// <summary>
        /// 执行所有已记录的取消订阅操作，并清空列表
        /// </summary>
        private void UnregisterSubscribers()
        {
            foreach (var unregister in _unsubscriptionActions)
            {
                unregister?.Invoke();
            }
            _unsubscriptionActions.Clear();
        }
    }
}
