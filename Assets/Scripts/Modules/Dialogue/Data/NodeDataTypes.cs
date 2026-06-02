// 所属模块: Dialogue.Data, 职责: 所有具体节点类型数据
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ARPG.Data.Dialogue
{
    // ────────────────────────────────────────────
    // StartNode — 对话入口
    // ────────────────────────────────────────────
    [Serializable]
    public class StartNodeData : DialogueNodeData
    {
        public override string NodeType => "Start";
    }

    // ────────────────────────────────────────────
    // DialogueLineNode — 对话行（说话者 + 文本）
    // ────────────────────────────────────────────
    [Serializable]
    public class DialogueLineNodeData : DialogueNodeData
    {
        public override string NodeType => "DialogueLine";

        [BoxGroup("说话者")]
        [LabelText("名称")]
        public string speakerName;

        [BoxGroup("对话内容")]
        [LabelText("文本")]
        [TextArea(3, 8)]
        public string dialogueText;

        [BoxGroup("语音")]
        [LabelText("配音剪辑")]
        public AudioClip voiceOverClip;
    }

    // ────────────────────────────────────────────
    // ChoiceNode — 玩家选择
    // ────────────────────────────────────────────
    [Serializable]
    public class ChoiceNodeData : DialogueNodeData
    {
        public override string NodeType => "Choice";

        [BoxGroup("选项")]
        [ListDrawerSettings(ShowPaging = false, DraggableItems = true)]
        public List<ChoiceOption> choices = new();
    }

    [Serializable]
    public class ChoiceOption
    {
        [LabelText("选项文本")]
        [TextArea(1, 3)]
        public string text;

        [HideInInspector]
        public string targetNodeId; // 由连线决定
    }

    // ────────────────────────────────────────────
    // BranchNode — 条件分支
    // ────────────────────────────────────────────
    [Serializable]
    public class BranchNodeData : DialogueNodeData
    {
        public override string NodeType => "Branch";

        [BoxGroup("条件")]
        public DialogueCondition condition;
    }

    // ────────────────────────────────────────────
    // EventNode — 触发游戏事件
    // ────────────────────────────────────────────
    [Serializable]
    public class EventNodeData : DialogueNodeData
    {
        public override string NodeType => "Event";

        [BoxGroup("事件")]
        [LabelText("事件类型名称")]
        public string eventTypeName;

        [BoxGroup("事件")]
        [LabelText("JSON 参数")]
        [TextArea(2, 5)]
        public string jsonParameters;
    }

    // ────────────────────────────────────────────
    // EndNode — 对话结束
    // ────────────────────────────────────────────
    [Serializable]
    public class EndNodeData : DialogueNodeData
    {
        public override string NodeType => "End";
    }
}
