// 所属模块: Dialogue.System, 职责: 对话服务公开接口
using ARPG.Data.Dialogue;
using ARPG.Events;
using System;

namespace ARPG.Interfaces
{
    /// <summary>
    /// 对话系统对外服务接口
    /// </summary>
    public interface IDialogueService
    {
        /// <summary>
        /// 开始一段对话
        /// </summary>
        void StartDialogue(DialogueGraph graph);

        /// <summary>
        /// 推进到下一个节点（在对话行播放完毕后调用）
        /// </summary>
        void Advance();

        /// <summary>
        /// 做出选择（在选择节点时调用）
        /// </summary>
        void MakeChoice(int choiceIndex);

        /// <summary>
        /// 设置变量值
        /// </summary>
        void SetVariable(string name, object value);

        /// <summary>
        /// 获取变量值
        /// </summary>
        T GetVariable<T>(string name);

        bool IsDialogueActive { get; }

        void StopDialogue();

        event Action<DialogueLineEvent> OnDialogueLine;
        event Action<DialogueChoiceEvent> OnChoicesPresented;
        event Action OnDialogueEnded;
    }
}
