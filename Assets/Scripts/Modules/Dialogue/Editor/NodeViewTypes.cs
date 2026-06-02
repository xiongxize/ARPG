// 所属模块: Dialogue.Editor, 职责: 各类型节点视图实现
using ARPG.Data.Dialogue;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPG.DialogueEditor
{
    // ─── StartNode（绿色入口）───
    public class StartNodeView : DialogueNodeView
    {
        public StartNodeView(StartNodeData data) : base(data) { }

        protected override string GetTitle() => "▶ 开始";
        protected override void CreatePorts() { CreateOutputPort("Output"); }

        public override void PopulateFromData()
        {
            base.PopulateFromData();
            titleContainer.style.backgroundColor = new Color(0.2f, 0.7f, 0.2f, 0.7f);
        }
    }

    // ─── DialogueLineNode（蓝色对话行）───
    public class DialogueLineNodeView : DialogueNodeView
    {
        private Label _previewLabel;

        public DialogueLineNodeView(DialogueLineNodeData data) : base(data) { }

        protected override string GetTitle() => "💬 对话行";

        protected override void CreatePorts()
        {
            CreateInputPort();
            CreateOutputPort("Output");
        }

        public override void PopulateFromData()
        {
            base.PopulateFromData();
            titleContainer.style.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.7f);

            // 清理旧的预览
            if (_previewLabel != null)
            {
                extensionContainer.Remove(_previewLabel);
                _previewLabel = null;
            }

            // 显示预览文本
            var data = NodeData as DialogueLineNodeData;
            if (data != null && !string.IsNullOrEmpty(data.dialogueText))
            {
                _previewLabel = new Label($"{data.speakerName}: {data.dialogueText}");
                _previewLabel.style.whiteSpace = WhiteSpace.Normal;
                _previewLabel.style.maxWidth = 250;
                _previewLabel.style.fontSize = 11;
                _previewLabel.style.marginLeft = 5;
                _previewLabel.style.marginRight = 5;
                _previewLabel.style.paddingTop = 4;
                _previewLabel.style.paddingBottom = 4;
                extensionContainer.Add(_previewLabel);
            }

            RefreshExpandedState();
        }
    }

    // ─── ChoiceNode（橙色选择）───
    public class ChoiceNodeView : DialogueNodeView
    {
        public ChoiceNodeView(ChoiceNodeData data) : base(data) { }

        protected override string GetTitle() => "❓ 选择";

        protected override void CreatePorts()
        {
            CreateInputPort();

            var data = NodeData as ChoiceNodeData;
            if (data != null)
            {
                for (int i = 0; i < data.choices.Count; i++)
                {
                    CreateChoiceOutputPort(data.choices[i], i);
                }
            }
        }

        public override void PopulateFromData()
        {
            base.PopulateFromData();
            titleContainer.style.backgroundColor = new Color(0.9f, 0.5f, 0.1f, 0.7f );

            // 同步端口数与 choice 数
            SyncChoicePorts();
        }

        private void SyncChoicePorts()
        {
            var data = NodeData as ChoiceNodeData;
            if (data == null) return;

            // 清空并重建输出端口
            outputContainer.Clear();
            for (int i = 0; i < data.choices.Count; i++)
            {
                CreateChoiceOutputPort(data.choices[i], i);
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        public static string GetChoicePortId(int index)
        {
            return $"{ChoicePortPrefix}{index}";
        }

        public static bool TryGetChoiceIndex(string portId, out int index)
        {
            index = -1;
            if (string.IsNullOrEmpty(portId) || !portId.StartsWith(ChoicePortPrefix))
                return false;

            return int.TryParse(portId.Substring(ChoicePortPrefix.Length), out index);
        }

        private Port CreateChoiceOutputPort(ChoiceOption choice, int index)
        {
            var port = CreateOutputPort(GetChoicePortId(index));
            port.portName = GetChoicePortLabel(choice, index);
            port.portColor = new Color(0.9f, 0.7f, 0.3f, 0.7f);
            return port;
        }

        private static string GetChoicePortLabel(ChoiceOption choice, int index)
        {
            var text = choice?.text?.Replace("\r", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrEmpty(text))
                return $"Choice {index + 1}";

            const int maxLength = 24;
            if (text.Length > maxLength)
                text = text.Substring(0, maxLength) + "...";

            return $"{index + 1}. {text}";
        }

        private const string ChoicePortPrefix = "Choice_";

        public override void UpdateDataFromView()
        {
            base.UpdateDataFromView();
            // choice 的 targetNodeId 由连线决定，在保存时由 Serializer 填充
        }
    }

    // ─── BranchNode（紫色条件分支）───
    public class BranchNodeView : DialogueNodeView
    {
        public BranchNodeView(BranchNodeData data) : base(data) { }

        protected override string GetTitle() => "🔀 分支";

        protected override void CreatePorts()
        {
            CreateInputPort();
            var truePort = CreateOutputPort("True");
            truePort.portColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);  // 绿色
            truePort.portName = "True";
            var falsePort = CreateOutputPort("False");
            falsePort.portColor = new Color(0.8f, 0.2f, 0.2f, 0.7f); // 红色
            falsePort.portName = "False";
        }

        public override void PopulateFromData()
        {
            base.PopulateFromData();
            titleContainer.style.backgroundColor = new Color(0.5f, 0.2f, 0.7f, 0.7f);
        }
    }

    // ─── EventNode（黄色事件触发）───
    public class EventNodeView : DialogueNodeView
    {
        public EventNodeView(EventNodeData data) : base(data) { }

        protected override string GetTitle()
        {
            var data = NodeData as EventNodeData;
            return string.IsNullOrEmpty(data?.eventTypeName) ? "⚡ 事件" : $"⚡ {data.eventTypeName}";
        }

        protected override void CreatePorts()
        {
            CreateInputPort();
            CreateOutputPort("Output");
        }

        public override void PopulateFromData()
        {
            base.PopulateFromData();
            titleContainer.style.backgroundColor = new Color(0.8f, 0.7f, 0.1f, 0.7f);
        }
    }

    // ─── EndNode（红色结束）───
    public class EndNodeView : DialogueNodeView
    {
        public EndNodeView(EndNodeData data) : base(data) { }

        protected override string GetTitle() => "■ 结束";

        protected override void CreatePorts() { CreateInputPort(); }

        public override void PopulateFromData()
        {
            base.PopulateFromData();
            titleContainer.style.backgroundColor = new Color(0.7f, 0.2f, 0.2f, 0.7f);
        }
    }
}
