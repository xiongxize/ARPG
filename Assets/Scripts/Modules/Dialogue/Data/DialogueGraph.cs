// 所属模块: Dialogue.Data, 职责: 对话图资产容器
using System.Collections.Generic;
using UnityEngine;

namespace ARPG.Data.Dialogue
{
    /// <summary>
    /// 对话图资产 — 存储所有节点、连线、变量定义
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueGraph", menuName = "ARPG/Dialogue/Dialogue Graph")]
    public class DialogueGraph : ScriptableObject
    {
        [SerializeReference]
        public List<DialogueNodeData> nodes = new();

        public List<DialogueLinkData> links = new();

        public List<DialogueVariable> variables = new();

        /// <summary>
        /// 起始节点 GUID（如果有多个 StartNode 时，指定哪一个为入口）
        /// </summary>
        [HideInInspector]
        public string startNodeId;
    }
}
