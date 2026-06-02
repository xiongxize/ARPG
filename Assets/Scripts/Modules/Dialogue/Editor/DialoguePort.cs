// 所属模块: Dialogue.Editor, 职责: 自定义 GraphView 端口
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ARPG.DialogueEditor
{
    /// <summary>
    /// 对话节点端口
    /// </summary>
    public static class DialoguePort
    {
        /// <summary>
        /// 创建一个对话端口
        /// </summary>
        public static Port Create(Direction direction, Port.Capacity capacity = Port.Capacity.Single, string portId = "Input")
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, direction, capacity, typeof(bool));
            port.portName = portId;
            port.name = portId;
            port.tooltip = portId;
            port.userData = portId;

            // 端口视觉样式
            var styleColor = direction == Direction.Input
                ? new Color(0.2f, 0.6f, 1f)    // 输入端口：蓝色
                : new Color(1f, 0.6f, 0.2f);    // 输出端口：橙色

            port.portColor = styleColor;
            return port;
        }

        public static string GetId(Port port)
        {
            if (port == null) return string.Empty;
            if (port.userData is string id && !string.IsNullOrEmpty(id)) return id;
            if (!string.IsNullOrEmpty(port.name)) return port.name;
            return port.portName;
        }
    }
}
