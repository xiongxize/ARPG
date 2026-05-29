// 所属模块: Inventory, 职责: 背包模块初始化
using ARPG.Core.Module;
using UnityEngine;

namespace ARPG.System.Inventory
{
    public class InventoryBootstrap : IModuleBootstrap
    {
        public int Priority => 20;

        public void Init()
        {  
            // TODO: 初始化背包模块逻辑
            Debug.Log($"[{nameof(InventoryBootstrap)}] 优先级： {Priority} 初始化音频模块");
        }
    }
}
