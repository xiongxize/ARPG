// 所属模块: Combat, 职责: 战斗模块初始化
using ARPG.Core.Module;
using ARPG.Core;
using ARPG.Interfaces;
using ARPG.System.Combat;
using UnityEngine;

namespace ARPG.System.Combat
{
    public class CombatBootstrap : IModuleBootstrap
    {
        public int Priority => 10;

        public void Init()
        {
            // TODO: 初始化战斗模块
            // 示例：注册服务到 ServiceLocator
            ServiceLocator.Register<ICombatService>(new CombatSystem());
            Debug.Log($"[{nameof(CombatBootstrap)}] 优先级： {Priority} 初始化音频模块");
            
        }
    }
}
