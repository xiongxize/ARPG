// 所属模块: Dialogue.Editor, 职责: 对话节点视图基类
using ARPG.Data.Dialogue;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using UnityEngine.UIElements;

namespace ARPG.DialogueEditor
{
    /// <summary>
    /// 节点视图抽象基类 — 所有具体节点类型的视觉表现
    /// </summary>
    public abstract class DialogueNodeView : Node
    {
        public string NodeGuid => NodeData.guid;
        public DialogueNodeData NodeData { get; protected set; }

        protected DialogueNodeView(DialogueNodeData data)
        {
            NodeData = data;

            // 生成 GUID（如果还没有）
            if (string.IsNullOrEmpty(data.guid))
                data.guid = Guid.NewGuid().ToString();

            userData = data.guid;

            // 标题
            title = GetTitle();
            SetPosition(new Rect(data.position, Vector2.zero));

            // 添加样式类
            AddToClassList($"dialogue-node-{data.NodeType.ToLower()}");
            style.minWidth = 180;

            // 创建端口
            CreatePorts();
            RefreshExpandedState();
            RefreshPorts();
        }

        /// <summary>
        /// 节点标题
        /// </summary>
        protected abstract string GetTitle();

        /// <summary>
        /// 创建输入/输出端口（子类实现具体端口配置）
        /// </summary>
        protected abstract void CreatePorts();

        /// <summary>
        /// 从 NodeData 刷新视图
        /// </summary>
        public virtual void PopulateFromData()
        {
            title = GetTitle();
            SetPosition(new Rect(NodeData.position, Vector2.zero));
        }

        /// <summary>
        /// 从视图同步数据回 NodeData（保存时调用）
        /// </summary>
        public virtual void UpdateDataFromView()
        {
            NodeData.position = GetPosition().position;
        }

        /// <summary>
        /// 按名称和方向查找端口
        /// </summary>
        public Port FindPort(string portName, Direction direction)
        {
            var container = direction == Direction.Input ? inputContainer : outputContainer;
            foreach (var port in container.Children())
            {
                if (port is Port p && DialoguePort.GetId(p) == portName)
                    return p;
            }
            return null;
        }

        // ──────── 端口创建辅助 ────────

        protected Port CreateInputPort()
        {
            var port = DialoguePort.Create(Direction.Input, Port.Capacity.Single, "Input");
            inputContainer.Add(port);
            RefreshPorts();
            return port;
        }

        protected Port CreateOutputPort(string name = "Output")
        {
            var port = DialoguePort.Create(Direction.Output, Port.Capacity.Single, name);
            outputContainer.Add(port);
            RefreshPorts();
            return port;
        }
    }
}
