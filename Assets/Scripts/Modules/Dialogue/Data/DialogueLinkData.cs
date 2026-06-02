// 所属模块: Dialogue.Data, 职责: 节点连接线数据
using System;

namespace ARPG.Data.Dialogue
{
    /// <summary>
    /// 节点之间的连线数据
    /// </summary>
    [Serializable]
    public struct DialogueLinkData
    {
        public string sourceNodeId;
        public string sourcePortName;   // "Output", "True", "False", "Choice_0", ...
        public string targetNodeId;
        public string targetPortName;   // "Input"
    }
}
