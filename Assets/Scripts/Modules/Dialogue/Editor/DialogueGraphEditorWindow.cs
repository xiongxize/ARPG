// 所属模块: Dialogue.Editor, 职责: 对话图编辑器主窗口（三栏布局）
using ARPG.Data.Dialogue;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace ARPG.DialogueEditor
{
    /// <summary>
    /// 可视化对话编辑器主窗口
    /// 布局：工具栏 | [GraphView 画布 | 黑板 + 属性面板]
    /// </summary>
    public class DialogueGraphEditorWindow : EditorWindow
    {
        [MenuItem("ARPG/Dialogue/对话图编辑器 #D")]
        public static void OpenWindow()
        {
            var window = GetWindow<DialogueGraphEditorWindow>();
            window.titleContent = new GUIContent("对话编辑器");
            window.minSize = new Vector2(800, 500);
            window.Show();
        }

        /// <summary>
        /// 双击 .asset 文件时打开编辑器
        /// </summary>
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is DialogueGraph graph)
            {
                var window = GetWindow<DialogueGraphEditorWindow>();
                window.titleContent = new GUIContent(graph.name);
                window.minSize = new Vector2(800, 500);
                window.LoadGraph(graph);
                return true;
            }
            return false;
        }

        // ──────── 序列化状态（域重载保持） ────────

        [SerializeField]
        private DialogueGraph _currentGraph;

        // ──────── 视图组件 ────────

        private DialogueGraphView _graphView;
        private DialogueInspectorPanel _inspectorPanel;
        private DialogueBlackboard _blackboard;

        // ──────── 生命周期 ────────

        private void OnEnable()
        {
            // 从域重载恢复（读取 SessionState）
            var savedGuid = SessionState.GetString("ARPG_Dialogue_ActiveGraphGUID", "");
            if (!string.IsNullOrEmpty(savedGuid) && _currentGraph == null)
            {
                var path = AssetDatabase.GUIDToAssetPath(savedGuid);
                if (!string.IsNullOrEmpty(path))
                {
                    var graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
                    if (graph != null) _currentGraph = graph;
                }
            }

            BuildUI();
        }

        private void OnDisable()
        {
            SaveGraph();
        }

        private void OnDestroy()
        {
            SaveGraph();
        }

        // ──────── UI 构建 ────────

        private void BuildUI()
        {
            rootVisualElement.Clear();

            // ---- 工具栏 ----
            var toolbar = new Toolbar();
            toolbar.style.minHeight = 24;

            var loadButton = new ToolbarButton(() => SelectAndLoadGraph()) { text = "📂 打开" };
            var saveButton = new ToolbarButton(() => SaveGraph()) { text = "💾 保存" };
            var validateButton = new ToolbarButton(() => ValidateGraph()) { text = "✓ 验证" };
            var createButton = new ToolbarButton(() => CreateNewGraph()) { text = "✚ 新建" };

            toolbar.Add(createButton);
            toolbar.Add(loadButton);
            toolbar.Add(saveButton);
            toolbar.Add(validateButton);

            // 当前图名称
            if (_currentGraph != null)
            {
                var label = new Label(_currentGraph.name);
                label.style.marginLeft = 10;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                toolbar.Add(label);
            }

            rootVisualElement.Add(toolbar);

            // ---- 分栏布局：右侧固定宽度，GraphView 占据剩余空间 ----
            const float rightPanelWidth = 340f;
            const float blackboardHeight = 180f;
            var splitView = new TwoPaneSplitView(1, rightPanelWidth, TwoPaneSplitViewOrientation.Horizontal);
            splitView.style.flexGrow = 1;

            // 左中：GraphView 画布
            _graphView = new DialogueGraphView();
            _graphView.style.flexGrow = 1;
            _graphView.style.minWidth = 420;
            _graphView.OnNodeSelected = OnNodeSelected;
            splitView.Add(_graphView);

            // 右侧：黑板（上）+ 属性面板（下）
            var rightPanel = new VisualElement();
            rightPanel.style.flexDirection = FlexDirection.Column;
            rightPanel.style.width = rightPanelWidth;
            rightPanel.style.minWidth = 300;
            rightPanel.style.minHeight = 0;
            rightPanel.style.flexGrow = 0;
            rightPanel.style.flexShrink = 0;
            rightPanel.style.overflow = Overflow.Hidden;

            _blackboard = new DialogueBlackboard();
            _blackboard.style.height = blackboardHeight;
            _blackboard.style.flexGrow = 0;
            _blackboard.style.flexShrink = 0;
            _graphView.SetBlackboard(_blackboard);
            rightPanel.Add(_blackboard);

            var rightSeparator = new VisualElement();
            rightSeparator.style.height = 1;
            rightSeparator.style.flexShrink = 0;
            rightSeparator.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            rightPanel.Add(rightSeparator);

            _inspectorPanel = new DialogueInspectorPanel();
            _inspectorPanel.style.flexGrow = 1;
            _inspectorPanel.style.flexShrink = 1;
            _inspectorPanel.style.minHeight = 0;
            _inspectorPanel.OnNodeDataChanged = OnNodeDataChanged;
            rightPanel.Add(_inspectorPanel);

            splitView.Add(rightPanel);

            rootVisualElement.Add(splitView);

            // 如果有当前图，加载它
            if (_currentGraph != null)
                LoadGraphIntoView(_currentGraph);

            // 注册键盘快捷键
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            // Ctrl+S 保存
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                SaveGraph();
                evt.StopPropagation();
            }
        }

        // ──────── 图加载/保存 ────────

        public void LoadGraph(DialogueGraph graph)
        {
            _currentGraph = graph;

            if (graph != null)
            {
                var path = AssetDatabase.GetAssetPath(graph);
                var guid = AssetDatabase.AssetPathToGUID(path);
                SessionState.SetString("ARPG_Dialogue_ActiveGraphGUID", guid);
                titleContent = new GUIContent(graph.name);
            }

            if (_graphView != null)
                LoadGraphIntoView(graph);
        }

        private void LoadGraphIntoView(DialogueGraph graph)
        {
            if (_graphView == null) return;
            DialogueGraphSerializer.LoadFromAsset(_graphView, graph);
        }

        private void SaveGraph()
        {
            if (_currentGraph == null || _graphView == null) return;

            Undo.RecordObject(_currentGraph, "保存对话图");
            DialogueGraphSerializer.SaveToAsset(_graphView, _currentGraph);
        }

        // ──────── 选择图资产 ────────

        private void SelectAndLoadGraph()
        {
            var path = EditorUtility.OpenFilePanel("选择对话图", "Assets/Data/Dialogue", "asset");
            if (string.IsNullOrEmpty(path)) return;

            var relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            var graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(relativePath);
            if (graph != null)
                LoadGraph(graph);
            else
                EditorUtility.DisplayDialog("错误", "所选文件不是有效的对话图资产", "好的");
        }

        // ──────── 节点选择回调 ────────

        private void OnNodeSelected(DialogueNodeData nodeData)
        {
            _inspectorPanel?.Bind(nodeData, _currentGraph);
        }

        private void OnNodeDataChanged(DialogueNodeData nodeData)
        {
            if (_graphView == null || nodeData == null) return;
            _graphView.schedule.Execute(() => _graphView.RefreshNodeView(nodeData));
        }

        // ──────── 验证 ────────

        private void ValidateGraph()
        {
            if (_currentGraph == null)
            {
                EditorUtility.DisplayDialog("验证", "请先打开一个对话图", "好的");
                return;
            }

            SaveGraph();
            DialogueGraphSerializer.ValidateGraph(_currentGraph, out var warnings, out var errors);

            var msg = $"验证结果：{_currentGraph.name}\n\n";
            if (errors.Count > 0)
                msg += $"❌ 错误 ({errors.Count}):\n" + string.Join("\n", errors);
            if (warnings.Count > 0)
                msg += $"\n\n⚠️ 警告 ({warnings.Count}):\n" + string.Join("\n", warnings);
            if (errors.Count == 0 && warnings.Count == 0)
                msg += "✅ 无问题";

            EditorUtility.DisplayDialog("图验证", msg, "好的");
        }

        // ──────── 创建新图 ────────

        private void CreateNewGraph()
        {
            var path = EditorUtility.SaveFilePanelInProject("新建对话图", "NewDialogueGraph", "asset", "创建新的对话图");
            if (string.IsNullOrEmpty(path)) return;

            var graph = ScriptableObject.CreateInstance<DialogueGraph>();

            // 自动添加一个 StartNode
            var startNode = new StartNodeData
            {
                guid = Guid.NewGuid().ToString(),
                nodeName = "对话开始",
                position = new Vector2(200, 200)
            };
            graph.nodes.Add(startNode);
            graph.startNodeId = startNode.guid;

            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.SaveAssets();

            LoadGraph(graph);
        }
    }
}
