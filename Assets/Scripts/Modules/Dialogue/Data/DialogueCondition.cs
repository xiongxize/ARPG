// 所属模块: Dialogue.Data, 职责: 条件表达式数据
using Sirenix.OdinInspector;
using System;

namespace ARPG.Data.Dialogue
{
    /// <summary>
    /// 分支节点的条件表达式
    /// leftOperand op rightOperand
    /// 操作数可以是变量名或字面量
    /// </summary>
    [Serializable]
    public class DialogueCondition
    {
        [LabelText("左操作数")]
        public string leftOperand;

        [LabelText("运算符")]
        public ConditionOperator op;

        [LabelText("右操作数")]
        public string rightOperand;
    }

    public enum ConditionOperator
    {
        [LabelText("等于")]
        Equals,
        [LabelText("不等于")]
        NotEquals,
        [LabelText("大于")]
        GreaterThan,
        [LabelText("小于")]
        LessThan,
        [LabelText("大于等于")]
        GreaterOrEqual,
        [LabelText("小于等于")]
        LessOrEqual,
        [LabelText("包含")]
        Contains
    }
}
