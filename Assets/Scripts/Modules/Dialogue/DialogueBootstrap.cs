// 所属模块: Dialogue, 职责: 对话模块初始化
using ARPG.Core;
using ARPG.Core.Module;
using ARPG.Interfaces;
using ARPG.System.Dialogue;
using UnityEngine;

namespace ARPG.System.Dialogue
{
    public class DialogueBootstrap : IModuleBootstrap
    {
        public int Priority => 15;

        public void Init()
        {
            var dialogueSystem = new DialogueSystem();
            ServiceLocator.Register<IDialogueService>(dialogueSystem);
            Debug.Log($"[{nameof(DialogueBootstrap)}] 对话模块初始化完成，Priority={Priority}");
        }
    }
}
