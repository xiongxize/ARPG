// 所属模块: Dialogue.Data, 职责: 黑板变量定义
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace ARPG.Data.Dialogue
{
    /// <summary>
    /// 对话黑板中的变量定义
    /// </summary>
    [Serializable]
    public class DialogueVariable
    {
        public string name;
        public DialogueVariableType type;
        [TextArea]
        public string defaultValue;
    }

    public enum DialogueVariableType
    {
        String,
        Int,
        Bool
    }
}
