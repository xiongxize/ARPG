// 所属模块: Dialogue.Editor, 职责: GraphView ↔ ScriptableObject 序列化
using ARPG.Data.Dialogue;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPG.DialogueEditor
{
    /// <summary>
    /// 在 GraphView 和 DialogueGraph ScriptableObject 之间转换
    /// </summary>
    public static class DialogueGraphSerializer
    {
        // ───────────────────── 保存 ─────────────────────

        public static void SaveToAsset(DialogueGraphView graphView, DialogueGraph asset)
        {
            if (asset == null) return;

            // 1. 从节点视图同步数据
            var nodes = new List<DialogueNodeData>();
            var startNodeGuid = string.Empty;

            foreach (var nodeView in graphView.nodes.ToList().OfType<DialogueNodeView>())
            {
                nodeView.UpdateDataFromView();
                nodes.Add(nodeView.NodeData);

                if (nodeView.NodeData is StartNodeData)
                    startNodeGuid = nodeView.NodeData.guid;
            }

            foreach (var choiceNode in nodes.OfType<ChoiceNodeData>())
            {
                foreach (var choice in choiceNode.choices)
                    choice.targetNodeId = null;
            }

            // 2. 从连线提取链接数据
            var links = new List<DialogueLinkData>();
            foreach (var edge in graphView.edges.ToList())
            {
                if (edge.output == null || edge.input == null) continue;

                var sourceView = edge.output.node as DialogueNodeView;
                var targetView = edge.input.node as DialogueNodeView;
                if (sourceView == null || targetView == null) continue;

                var sourcePortName = DialoguePort.GetId(edge.output);
                var targetPortName = DialoguePort.GetId(edge.input);
                links.Add(new DialogueLinkData
                {
                    sourceNodeId = sourceView.NodeGuid,
                    sourcePortName = sourcePortName,
                    targetNodeId = targetView.NodeGuid,
                    targetPortName = targetPortName
                });

                if (sourceView.NodeData is ChoiceNodeData choiceNode &&
                    ChoiceNodeView.TryGetChoiceIndex(sourcePortName, out var choiceIndex) &&
                    choiceIndex >= 0 &&
                    choiceIndex < choiceNode.choices.Count)
                {
                    choiceNode.choices[choiceIndex].targetNodeId = targetView.NodeGuid;
                }
            }

            // 3. 从黑板同步变量
            var variables = graphView.Blackboard?.GetVariables() ?? new List<DialogueVariable>();

            // 4. 写入资产
            asset.nodes = nodes;
            asset.links = links;
            asset.variables = variables;
            asset.startNodeId = string.IsNullOrEmpty(asset.startNodeId) ? startNodeGuid : asset.startNodeId;

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        // ───────────────────── 加载 ─────────────────────

        public static void LoadFromAsset(DialogueGraphView graphView, DialogueGraph asset)
        {
            if (asset == null) return;

            graphView.ClearGraph();

            var nodeViewMap = new Dictionary<string, DialogueNodeView>();

            // 1. 创建节点视图
            foreach (var nodeData in asset.nodes)
            {
                var nodeView = CreateNodeViewForData(nodeData);
                if (nodeView == null) continue;

                nodeView.SetPosition(new Rect(nodeData.position, Vector2.zero));
                graphView.AddNodeView(nodeView);
                nodeViewMap[nodeData.guid] = nodeView;
            }

            // 2. 创建连线
            foreach (var link in asset.links)
            {
                if (!nodeViewMap.TryGetValue(link.sourceNodeId, out var sourceView)) continue;
                if (!nodeViewMap.TryGetValue(link.targetNodeId, out var targetView)) continue;

                var outputPort = sourceView.FindPort(link.sourcePortName, Direction.Output);
                var inputPort = targetView.FindPort(link.targetPortName, Direction.Input);
                if (outputPort == null || inputPort == null) continue;

                var edge = new Edge
                {
                    output = outputPort,
                    input = inputPort
                };
                outputPort.Connect(edge);
                inputPort.Connect(edge);
                edge.RegisterCallback<GeometryChangedEvent>(_ => edge.UpdateEdgeControl());
                graphView.AddElement(edge);

                if (sourceView.NodeData is ChoiceNodeData choiceNode &&
                    ChoiceNodeView.TryGetChoiceIndex(link.sourcePortName, out var choiceIndex) &&
                    choiceIndex >= 0 &&
                    choiceIndex < choiceNode.choices.Count)
                {
                    choiceNode.choices[choiceIndex].targetNodeId = targetView.NodeGuid;
                }
            }

            // 3. 恢复黑板变量
            graphView.Blackboard?.SetVariables(asset.variables);
        }

        // ───────────────────── 节点工厂 ─────────────────────

        private static DialogueNodeView CreateNodeViewForData(DialogueNodeData data)
        {
            DialogueNodeView view = data switch
            {
                StartNodeData d => new StartNodeView(d),
                DialogueLineNodeData d => new DialogueLineNodeView(d),
                ChoiceNodeData d => new ChoiceNodeView(d),
                BranchNodeData d => new BranchNodeView(d),
                EventNodeData d => new EventNodeView(d),
                EndNodeData d => new EndNodeView(d),
                _ => null
            };

            view?.PopulateFromData();
            return view;
        }

        // ───────────────────── 验证 ─────────────────────

        public static void ValidateGraph(DialogueGraph graph, out List<string> warnings, out List<string> errors)
        {
            warnings = new List<string>();
            errors = new List<string>();

            if (graph == null)
            {
                errors.Add("对话图为空");
                return;
            }

            // 检查节点数
            if (graph.nodes.Count == 0)
            {
                errors.Add("对话图没有节点");
                return;
            }

            // 检查起始节点
            var startNodes = graph.nodes.OfType<StartNodeData>().ToList();
            if (startNodes.Count == 0)
                errors.Add("对话图中没有起始节点 (StartNode)");
            else if (startNodes.Count > 1 && string.IsNullOrEmpty(graph.startNodeId))
                warnings.Add("存在多个起始节点，但未指定入口；将使用第一个 StartNode");

            // 环检测 (DFS)
            var outgoing = BuildOutgoingMap(graph);
            var visited = new HashSet<string>();
            var recStack = new HashSet<string>();

            bool HasCycle(string id)
            {
                if (recStack.Contains(id)) return true;
                if (visited.Contains(id)) return false;
                visited.Add(id);
                recStack.Add(id);
                if (outgoing.TryGetValue(id, out var links))
                    foreach (var l in links)
                        if (HasCycle(l.targetNodeId)) return true;
                recStack.Remove(id);
                return false;
            }

            var startId = graph.startNodeId ?? startNodes.FirstOrDefault()?.guid;
            if (startId != null && HasCycle(startId))
                warnings.Add("对话图中存在环，运行时可能导致无限循环");

            // 孤儿节点检测（从起点 BFS）
            var reachable = new HashSet<string>();
            var queue = new Queue<string>();
            if (startId != null) queue.Enqueue(startId);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!reachable.Add(cur)) continue;
                if (outgoing.TryGetValue(cur, out var links))
                    foreach (var l in links) queue.Enqueue(l.targetNodeId);
            }

            foreach (var node in graph.nodes)
            {
                if (!reachable.Contains(node.guid) && node is not StartNodeData)
                    warnings.Add($"节点 [{node.nodeName ?? node.guid}] 不可达（孤儿节点）");
            }

            // 检查 EndNode
            var endNodes = graph.nodes.OfType<EndNodeData>().ToList();
            if (endNodes.Count == 0)
                warnings.Add("对话图中没有结束节点 (EndNode)，对话可能无法正常结束");
        }

        private static Dictionary<string, List<DialogueLinkData>> BuildOutgoingMap(DialogueGraph graph)
        {
            var map = new Dictionary<string, List<DialogueLinkData>>();
            foreach (var link in graph.links)
            {
                if (!map.ContainsKey(link.sourceNodeId))
                    map[link.sourceNodeId] = new List<DialogueLinkData>();
                map[link.sourceNodeId].Add(link);
            }
            return map;
        }
    }
}
