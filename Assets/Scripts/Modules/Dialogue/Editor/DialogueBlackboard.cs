// 所属模块: Dialogue.Editor, 职责: 对话黑板变量管理面板
using ARPG.Data.Dialogue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPG.DialogueEditor
{
    /// <summary>
    /// 右侧黑板面板 — 管理对话中的全局变量
    /// </summary>
    public class DialogueBlackboard : VisualElement
    {
        private readonly List<DialogueVariable> _variables = new();
        private readonly VisualElement _rowsContainer;

        public DialogueBlackboard()
        {
            style.flexDirection = FlexDirection.Column;
            style.flexGrow = 0;
            style.flexShrink = 0;
            style.minHeight = 0;
            style.overflow = Overflow.Hidden;
            style.paddingLeft = 6;
            style.paddingRight = 6;
            style.paddingTop = 6;
            style.paddingBottom = 6;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.height = 26;
            header.style.flexShrink = 0;

            var titleLabel = new Label("黑板变量");
            titleLabel.style.flexGrow = 1;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(titleLabel);

            var addButton = new Button(AddNewVariable)
            {
                text = "＋",
                tooltip = "添加变量"
            };
            addButton.style.width = 26;
            addButton.style.height = 22;
            addButton.style.flexShrink = 0;
            header.Add(addButton);

            Add(header);

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            scrollView.style.flexShrink = 1;
            scrollView.style.minHeight = 0;

            _rowsContainer = new VisualElement();
            _rowsContainer.style.flexDirection = FlexDirection.Column;
            scrollView.Add(_rowsContainer);

            Add(scrollView);
        }

        private void AddNewVariable()
        {
            var variable = new DialogueVariable
            {
                name = $"newVariable_{_variables.Count}",
                type = DialogueVariableType.String,
                defaultValue = ""
            };
            _variables.Add(variable);
            RebuildRows();
        }

        private void RebuildRows()
        {
            _rowsContainer.Clear();

            foreach (var variable in _variables)
            {
                _rowsContainer.Add(CreateRowForVariable(variable));
            }
        }

        private VisualElement CreateRowForVariable(DialogueVariable variable)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Column;
            row.style.marginTop = 4;
            row.style.marginBottom = 4;
            row.style.paddingLeft = 4;
            row.style.paddingRight = 4;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;
            row.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);

            var topLine = new VisualElement();
            topLine.style.flexDirection = FlexDirection.Row;
            topLine.style.alignItems = Align.Center;

            var nameField = new TextField("名称")
            {
                value = variable.name,
                tooltip = "变量名"
            };
            nameField.style.flexGrow = 1;
            nameField.style.minWidth = 0;
            nameField.RegisterValueChangedCallback(evt => variable.name = evt.newValue);
            topLine.Add(nameField);

            var deleteButton = new Button(() =>
            {
                _variables.Remove(variable);
                RebuildRows();
            })
            {
                text = "✕",
                tooltip = "删除变量"
            };
            deleteButton.style.width = 24;
            deleteButton.style.height = 20;
            deleteButton.style.marginLeft = 4;
            deleteButton.style.flexShrink = 0;
            deleteButton.style.color = Color.red;
            topLine.Add(deleteButton);

            var typeField = new EnumField("类型", variable.type);
            typeField.style.marginTop = 3;
            typeField.RegisterValueChangedCallback(evt => variable.type = (DialogueVariableType)evt.newValue);

            var defaultField = new TextField("默认值")
            {
                value = variable.defaultValue,
                tooltip = "默认值"
            };
            defaultField.style.marginTop = 3;
            defaultField.RegisterValueChangedCallback(evt => variable.defaultValue = evt.newValue);

            row.Add(topLine);
            row.Add(typeField);
            row.Add(defaultField);

            return row;
        }

        /// <summary>
        /// 获取所有变量
        /// </summary>
        public List<DialogueVariable> GetVariables()
        {
            return new List<DialogueVariable>(_variables);
        }

        /// <summary>
        /// 设置变量列表（从加载的资产恢复）
        /// </summary>
        public void SetVariables(List<DialogueVariable> variables)
        {
            _variables.Clear();
            if (variables != null)
                _variables.AddRange(variables);
            RebuildRows();
        }
    }
}
