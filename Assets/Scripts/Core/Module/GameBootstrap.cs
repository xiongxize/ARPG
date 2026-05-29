// 所属模块: Core.Module, 职责: 游戏启动器，负责加载并初始化所有模块
using UnityEngine;
using System.Collections.Generic;

namespace ARPG.Core.Module
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            InitializeModules();
        }

        private void InitializeModules()
        {
            List<IModuleBootstrap> modules = ModuleRegistry.DiscoverModules();

            if (modules.Count == 0)
            {
                Debug.LogWarning("没有通过程序集扫描发现任何模块");
                return;
            }

            foreach (var module in modules)
            {
                module.Init();
            }
        }
    }
}
