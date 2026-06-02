// 所属模块: Dialogue.Data, 职责: 对话节点数据抽象基类
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ARPG.Data.Dialogue
{
    /// <summary>
    /// 所有对话节点的数据基类
    /// </summary>
    [Serializable]
    public abstract class DialogueNodeData
    {
        [HideInInspector]
        public string guid;

        [HideInInspector]
        public Vector2 position;

        /// <summary>
        /// 节点在编辑器中显示的名称（可选）
        /// </summary>
        [BoxGroup("基本")]
        [LabelText("节点名称")]
        public string nodeName;

        /// <summary>
        /// 节点类型标识，用于编辑器创建对应视图
        /// </summary>
        public abstract string NodeType { get; }
    }
}
