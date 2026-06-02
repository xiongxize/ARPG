// 所属模块: Dialogue.Editor, 职责: 对话图 GraphView 画布
using ARPG.Data.Dialogue;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace ARPG.DialogueEditor
{
    /// <summary>
    /// GraphView 画布 — 节点编辑、连线、缩放、搜索
    /// </summary>
    public class DialogueGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueGraphView> { }

        public DialogueBlackboard Blackboard { get; private set; }
        public Action<DialogueNodeData> OnNodeSelected;

        private readonly Dictionary<string, DialogueNodeView> _nodeMap = new();
        private DialogueSearchWindow _searchWindow;
        private MiniMap _miniMap;
        private bool _suppressChoiceTargetSync;

        public DialogueGraphView()
        {
            // ---- 网格背景 ----
            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            // ---- 缩放 & 拖拽 ----
            SetupZoom(0.1f, 2f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            // ---- MiniMap ----
            _miniMap = new MiniMap { anchored = true };
            _miniMap.SetPosition(new Rect(10, 10, 200, 140));
            Add(_miniMap);

            // ---- 搜索窗口 ----
            _searchWindow = ScriptableObject.CreateInstance<DialogueSearchWindow>();
            _searchWindow.Initialize(this);
            nodeCreationRequest += OnNodeCreationRequest;

            // ---- 连线兼容性过滤 ----
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                // 确保在面板附加后再注册其他回调
            });

            // ---- 节点选择回调 ----
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            // ---- 节点/边 变化追踪 ----
            graphViewChanged += OnGraphViewChanged;
        }

        // ──────── 节点创建 ────────

        private void OnNodeCreationRequest(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public DialogueNodeView CreateNodeAt(string typeName, Vector2 position)
        {
            DialogueNodeData data = typeName switch
            {
                "Start" => new StartNodeData(),
                "DialogueLine" => new DialogueLineNodeData(),
                "Choice" => new ChoiceNodeData
                {
                    choices = new List<ChoiceOption>
                    {
                        new ChoiceOption { text = "选项 1" },
                        new ChoiceOption { text = "选项 2" }
                    }
                },
                "Branch" => new BranchNodeData(),
                "Event" => new EventNodeData(),
                "End" => new EndNodeData(),
                _ => null
            };

            if (data == null) return null;

            data.guid = Guid.NewGuid().ToString();
            data.position = position;

            DialogueNodeView view = typeName switch
            {
                "Start" => new StartNodeView((StartNodeData)data),
                "DialogueLine" => new DialogueLineNodeView((DialogueLineNodeData)data),
                "Choice" => new ChoiceNodeView((ChoiceNodeData)data),
                "Branch" => new BranchNodeView((BranchNodeData)data),
                "Event" => new EventNodeView((EventNodeData)data),
                "End" => new EndNodeView((EndNodeData)data),
                _ => null
            };

            if (view == null) return null;

            view.SetPosition(new Rect(position, Vector2.zero));
            view.PopulateFromData();

            AddNodeView(view);

            // 选中新创建的节点
            ClearSelection();
            AddToSelection(view);

            return view;
        }

        // ──────── 删除 ────────

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
            {
                DeleteSelection();
                evt.StopPropagation();
            }
        }

        // ──────── 连线过滤 ────────

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatible = new List<Port>();

            foreach (var port in ports.ToList())
            {
                if (port.node == startPort.node) continue;          // 不能连自己
                if (port.direction == startPort.direction) continue; // 方向必须相反
                compatible.Add(port);
            }

            return compatible;
        }

        // ──────── 变化追踪（用于 Undo/Redo） ────────

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (!_suppressChoiceTargetSync && element is Edge edge)
                        UpdateChoiceTargetFromEdge(edge, false);

                    if (element is DialogueNodeView nodeView)
                        _nodeMap.Remove(nodeView.NodeGuid);
                }
            }

            // 边创建时自动记录
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    if (!_suppressChoiceTargetSync)
                        UpdateChoiceTargetFromEdge(edge, true);

                    // Undo 由 EditorWindow 统一处理
                }
            }

            return change;
        }

        // ──────── 黑板 ────────

        public void SetBlackboard(DialogueBlackboard blackboard)
        {
            Blackboard = blackboard;
        }

        // ──────── 节点映射 ────────

        public DialogueNodeView GetNodeView(string guid)
        {
            _nodeMap.TryGetValue(guid, out var view);
            return view;
        }

        public void ClearGraph()
        {
            DeleteElements(edges.ToList());
            DeleteElements(nodes.ToList());
            _nodeMap.Clear();
        }

        public void AddNodeView(DialogueNodeView nodeView)
        {
            if (nodeView == null) return;

            AddElement(nodeView);
            _nodeMap[nodeView.NodeGuid] = nodeView;
        }

        public void RefreshNodeView(DialogueNodeData nodeData)
        {
            if (nodeData == null || !_nodeMap.TryGetValue(nodeData.guid, out var nodeView))
                return;

            if (nodeView is ChoiceNodeView choiceNodeView)
            {
                RefreshChoiceNodeView(choiceNodeView);
                return;
            }

            nodeView.PopulateFromData();
        }

        private void RefreshChoiceNodeView(ChoiceNodeView choiceNodeView)
        {
            var outgoingEdges = edges.ToList()
                .Where(edge => edge.output?.node == choiceNodeView)
                .ToList();

            _suppressChoiceTargetSync = true;
            try
            {
                DeleteElements(outgoingEdges);
            }
            finally
            {
                _suppressChoiceTargetSync = false;
            }

            choiceNodeView.PopulateFromData();

            RestoreChoiceTargets(choiceNodeView);
        }

        private void RestoreChoiceTargets(ChoiceNodeView choiceNodeView)
        {
            if (choiceNodeView.NodeData is not ChoiceNodeData choiceNode)
                return;

            for (int i = 0; i < choiceNode.choices.Count; i++)
            {
                var targetNodeId = choiceNode.choices[i].targetNodeId;
                if (string.IsNullOrEmpty(targetNodeId))
                    continue;

                if (!_nodeMap.TryGetValue(targetNodeId, out var targetNodeView))
                    continue;

                var outputPort = choiceNodeView.FindPort(ChoiceNodeView.GetChoicePortId(i), Direction.Output);
                var inputPort = targetNodeView.FindPort("Input", Direction.Input);
                if (outputPort == null || inputPort == null)
                    continue;

                AddConnectedEdge(outputPort, inputPort);
            }
        }

        private void AddConnectedEdge(Port outputPort, Port inputPort)
        {
            var edge = new Edge
            {
                output = outputPort,
                input = inputPort
            };
            outputPort.Connect(edge);
            inputPort.Connect(edge);
            AddElement(edge);
            UpdateChoiceTargetFromEdge(edge, true);
        }

        private void UpdateChoiceTargetFromEdge(Edge edge, bool connected)
        {
            if (edge?.output?.node is not ChoiceNodeView choiceNodeView)
                return;

            if (choiceNodeView.NodeData is not ChoiceNodeData choiceNode)
                return;

            if (!ChoiceNodeView.TryGetChoiceIndex(DialoguePort.GetId(edge.output), out var choiceIndex))
                return;

            if (choiceIndex < 0 || choiceIndex >= choiceNode.choices.Count)
                return;

            var targetView = edge.input?.node as DialogueNodeView;
            var targetNodeId = targetView?.NodeGuid;

            if (connected)
            {
                choiceNode.choices[choiceIndex].targetNodeId = targetNodeId;
            }
            else if (string.IsNullOrEmpty(targetNodeId) ||
                     choiceNode.choices[choiceIndex].targetNodeId == targetNodeId)
            {
                choiceNode.choices[choiceIndex].targetNodeId = null;
            }
        }

        /// <summary>
        /// 选中节点时通知 Inspector 面板
        /// </summary>
        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);

            if (selectable is DialogueNodeView nodeView)
            {
                OnNodeSelected?.Invoke(nodeView.NodeData);
            }
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            OnNodeSelected?.Invoke(null);
        }
    }
}
