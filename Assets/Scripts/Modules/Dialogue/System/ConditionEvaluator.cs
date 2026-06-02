// 所属模块: Dialogue.System, 职责: 条件表达式求值器
using ARPG.Data.Dialogue;
using System;

namespace ARPG.System.Dialogue
{
    /// <summary>
    /// 评估 BranchNode 的条件表达式
    /// </summary>
    public static class ConditionEvaluator
    {
        /// <summary>
        /// 求值条件表达式
        /// </summary>
        public static bool Evaluate(DialogueCondition condition, DialogueVariableStore vars)
        {
            if (condition == null) return true;

            var left = ResolveValue(condition.leftOperand, vars);
            var right = ResolveValue(condition.rightOperand, vars);

            if (left == null || right == null) return false;

            // 按左操作数类型比较
            return left switch
            {
                int iLeft => CompareInt(iLeft, right, condition.op),
                bool bLeft => CompareBool(bLeft, right, condition.op),
                string sLeft => CompareString(sLeft, right, condition.op),
                _ => false
            };
        }

        private static object ResolveValue(string operand, DialogueVariableStore vars)
        {
            // 先尝试解析为变量名
            if (vars.TryGetValue(operand, out var val))
                return val;

            // 否则作为字面量解析
            if (int.TryParse(operand, out var i)) return i;
            if (bool.TryParse(operand, out var b)) return b;
            return operand; // 视为字符串
        }

        private static bool CompareInt(int left, object right, ConditionOperator op)
        {
            var r = Convert.ToInt32(right);
            return op switch
            {
                ConditionOperator.Equals => left == r,
                ConditionOperator.NotEquals => left != r,
                ConditionOperator.GreaterThan => left > r,
                ConditionOperator.LessThan => left < r,
                ConditionOperator.GreaterOrEqual => left >= r,
                ConditionOperator.LessOrEqual => left <= r,
                _ => false
            };
        }

        private static bool CompareBool(bool left, object right, ConditionOperator op)
        {
            var r = Convert.ToBoolean(right);
            return op switch
            {
                ConditionOperator.Equals => left == r,
                ConditionOperator.NotEquals => left != r,
                _ => false
            };
        }

        private static bool CompareString(string left, object right, ConditionOperator op)
        {
            var r = right.ToString();
            return op switch
            {
                ConditionOperator.Equals => left == r,
                ConditionOperator.NotEquals => left != r,
                ConditionOperator.Contains => left.Contains(r),
                _ => false
            };
        }
    }
}
