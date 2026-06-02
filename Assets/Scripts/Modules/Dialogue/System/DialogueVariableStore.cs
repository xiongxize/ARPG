// 所属模块: Dialogue.System, 职责: 运行时变量存储
using ARPG.Data.Dialogue;
using System.Collections.Generic;
using UnityEngine;

namespace ARPG.System.Dialogue
{
    /// <summary>
    /// 对话运行时变量存储，按类型分三组
    /// </summary>
    public class DialogueVariableStore
    {
        private readonly Dictionary<string, int> _ints = new();
        private readonly Dictionary<string, bool> _bools = new();
        private readonly Dictionary<string, string> _strings = new();

        /// <summary>
        /// 从 Variable 定义列表初始化默认值
        /// </summary>
        public void InitializeFrom(List<DialogueVariable> variables)
        {
            _ints.Clear();
            _bools.Clear();
            _strings.Clear();

            foreach (var v in variables)
            {
                switch (v.type)
                {
                    case DialogueVariableType.Int:
                        if (int.TryParse(v.defaultValue, out var iVal))
                            _ints[v.name] = iVal;
                        break;
                    case DialogueVariableType.Bool:
                        if (bool.TryParse(v.defaultValue, out var bVal))
                            _bools[v.name] = bVal;
                        break;
                    case DialogueVariableType.String:
                        _strings[v.name] = v.defaultValue ?? "";
                        break;
                }
            }
        }

        public void Set(string name, int value)
        {
            _ints[name] = value;
        }

        public void Set(string name, bool value)
        {
            _bools[name] = value;
        }

        public void Set(string name, string value)
        {
            _strings[name] = value;
        }

        public int GetInt(string name)
        {
            return _ints.TryGetValue(name, out var v) ? v : 0;
        }

        public bool GetBool(string name)
        {
            return _bools.TryGetValue(name, out var v) && v;
        }

        public string GetString(string name)
        {
            return _strings.TryGetValue(name, out var v) ? v : "";
        }

        /// <summary>
        /// 获取变量的原始 object 值，用于条件求值
        /// </summary>
        public bool TryGetValue(string name, out object value)
        {
            if (_ints.TryGetValue(name, out var i)) { value = i; return true; }
            if (_bools.TryGetValue(name, out var b)) { value = b; return true; }
            if (_strings.TryGetValue(name, out var s)) { value = s; return true; }
            value = null;
            return false;
        }
    }
}
