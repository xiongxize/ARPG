// 所属模块: Dialogue.System, 职责: 对话图核心遍历引擎
using ARPG.Data.Dialogue;
using ARPG.Events;
using ARPG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Core.EventBus;
using ARPG.Interfaces;
using UnityEngine;


namespace ARPG.System.Dialogue
{
    /// <summary>
    /// 对话图遍历引擎
    /// 纯 C# 类，无 MonoBehaviour 依赖
    /// </summary>
    public class DialogueSystem : IDialogueService
    {
        private DialogueGraph _graph;
        private string _currentNodeId;
        private DialogueVariableStore _variables = new();
        private Dictionary<string, List<DialogueLinkData>> _outgoingLinks;

        public bool IsDialogueActive => _graph != null && _currentNodeId != null;

        public event Action<DialogueLineEvent> OnDialogueLine;
        public event Action<DialogueChoiceEvent> OnChoicesPresented;
        public event Action OnDialogueEnded;

        // ───────────────────── 公开接口 ─────────────────────

        public void StartDialogue(DialogueGraph graph)
        {
            if (graph == null)
            {
                Debug.LogError("[DialogueSystem] 尝试启动空的对话图");
                return;
            }

            _graph = graph;
            _variables.InitializeFrom(graph.variables);
            BuildAdjacency(graph);

            // 验证图
            ValidateGraph(graph);

            // 从起始节点开始
            _currentNodeId = graph.startNodeId;
            if (string.IsNullOrEmpty(_currentNodeId))
            {
                // 自动查找第一个 StartNode
                var start = graph.nodes.OfType<StartNodeData>().FirstOrDefault();
                _currentNodeId = start?.guid;
            }

            if (string.IsNullOrEmpty(_currentNodeId))
            {
                Debug.LogError("[DialogueSystem] 对话图没有起始节点");
                _graph = null;
                return;
            }

            // 发布开始事件
            EventBus<ARPG.Events.DialogueStartedEvent>.Publish(new ARPG.Events.DialogueStartedEvent
            {
                GraphName = graph.name
            });

            ProcessNode(_currentNodeId);
        }

        public void Advance()
        {
            if (!IsDialogueActive) return;
            FollowOutputPort(_currentNodeId, "Output");
        }

        public void MakeChoice(int choiceIndex)
        {
            if (!IsDialogueActive) return;

            var node = GetCurrentNode<ChoiceNodeData>();
            if (node == null || choiceIndex < 0 || choiceIndex >= node.choices.Count)
                return;

            var portName = $"Choice_{choiceIndex}";
            FollowOutputPort(_currentNodeId, portName);
        }

        public void SetVariable(string name, object value)
        {
            switch (value)
            {
                case int i: _variables.Set(name, i); break;
                case bool b: _variables.Set(name, b); break;
                case string s: _variables.Set(name, s); break;
                default: _variables.Set(name, value?.ToString() ?? ""); break;
            }
        }

        public T GetVariable<T>(string name)
        {
            var val = _variables.TryGetValue(name, out var v) ? v : null;
            if (val == null) return default;
            return (T)val;
        }

        public void StopDialogue()
        {
            _graph = null;
            _currentNodeId = null;
            EventBus<ARPG.Events.DialogueEndedEvent>.Publish(default);
            OnDialogueEnded?.Invoke();
        }

        // ───────────────────── 内部遍历逻辑 ─────────────────────

        private void ProcessNode(string nodeId)
        {
            _currentNodeId = nodeId;
            var node = _graph.nodes.FirstOrDefault(n => n.guid == nodeId);
            if (node == null)
            {
                Debug.LogWarning($"[DialogueSystem] 找不到节点 {nodeId}，对话结束");
                EndDialogue();
                return;
            }

            switch (node)
            {
                case StartNodeData _:
                    // 自动前进到下一个节点
                    FollowOutputPort(nodeId, "Output");
                    break;

                case DialogueLineNodeData line:
                    EventBus<ARPG.Events.DialogueLineEvent>.Publish(new ARPG.Events.DialogueLineEvent
                    {
                        Speaker = line.speakerName,
                        Text = line.dialogueText
                    });
                    OnDialogueLine?.Invoke(new DialogueLineEvent
                    {
                        Speaker = line.speakerName,
                        Text = line.dialogueText
                    });
                    // 等待外部调用 Advance()
                    break;

                case ChoiceNodeData choice:
                    var texts = choice.choices.Select(c => c.text).ToArray();
                    EventBus<ARPG.Events.DialogueChoiceEvent>.Publish(new ARPG.Events.DialogueChoiceEvent
                    {
                        Choices = texts
                    });
                    OnChoicesPresented?.Invoke(new DialogueChoiceEvent
                    {
                        Choices = texts
                    });
                    // 等待外部调用 MakeChoice()
                    break;

                case BranchNodeData branch:
                    var result = ConditionEvaluator.Evaluate(branch.condition, _variables);
                    var portName = result ? "True" : "False";
                    FollowOutputPort(nodeId, portName);
                    break;

                case EventNodeData evt:
                    EventBus<ARPG.Events.DialogueEventTrigger>.Publish(new ARPG.Events.DialogueEventTrigger
                    {
                        EventType = evt.eventTypeName,
                        JsonData = evt.jsonParameters
                    });
                    FollowOutputPort(nodeId, "Output");
                    break;

                case EndNodeData _:
                    EndDialogue();
                    break;
            }
        }

        private void FollowOutputPort(string sourceId, string portName)
        {
            if (_outgoingLinks == null) return;

            if (_outgoingLinks.TryGetValue(sourceId, out var links))
            {
                var link = links.FirstOrDefault(l => l.sourcePortName == portName);
                if (link.targetNodeId != null)
                {
                    ProcessNode(link.targetNodeId);
                    return;
                }
            }

            Debug.LogWarning($"[DialogueSystem] 节点 {sourceId} 没有从 [{portName}] 出发的连线");
            EndDialogue();
        }

        private void EndDialogue()
        {
            EventBus<ARPG.Events.DialogueEndedEvent>.Publish(default);
            OnDialogueEnded?.Invoke();
            _graph = null;
            _currentNodeId = null;
        }

        // ───────────────────── 辅助 ─────────────────────

        private T GetCurrentNode<T>() where T : DialogueNodeData
        {
            return _graph?.nodes.FirstOrDefault(n => n.guid == _currentNodeId) as T;
        }

        private void BuildAdjacency(DialogueGraph graph)
        {
            _outgoingLinks = new Dictionary<string, List<DialogueLinkData>>();
            foreach (var link in graph.links)
            {
                if (!_outgoingLinks.ContainsKey(link.sourceNodeId))
                    _outgoingLinks[link.sourceNodeId] = new List<DialogueLinkData>();
                _outgoingLinks[link.sourceNodeId].Add(link);
            }
        }

        private void ValidateGraph(DialogueGraph graph)
        {
            // 环检测（DFS）
            var visited = new HashSet<string>();
            var recStack = new HashSet<string>();
            bool HasCycle(string id)
            {
                if (recStack.Contains(id)) return true;
                if (visited.Contains(id)) return false;
                visited.Add(id);
                recStack.Add(id);
                if (_outgoingLinks.TryGetValue(id, out var links))
                {
                    foreach (var l in links)
                        if (HasCycle(l.targetNodeId)) return true;
                }
                recStack.Remove(id);
                return false;
            }

            if (HasCycle(graph.startNodeId))
                Debug.LogWarning("[DialogueSystem] 对话图中检测到环，可能导致无限循环");

            // 孤儿节点检测（从起点 BFS）
            var reachable = new HashSet<string>();
            var queue = new Queue<string>();
            if (graph.startNodeId != null) queue.Enqueue(graph.startNodeId);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!reachable.Add(cur)) continue;
                if (_outgoingLinks.TryGetValue(cur, out var links))
                    foreach (var l in links) queue.Enqueue(l.targetNodeId);
            }

            foreach (var node in graph.nodes)
            {
                if (!reachable.Contains(node.guid) && node is not StartNodeData)
                    Debug.LogWarning($"[DialogueSystem] 节点 [{node.nodeName ?? node.guid}] 不可达（孤儿节点）");
            }
        }
    }
}
