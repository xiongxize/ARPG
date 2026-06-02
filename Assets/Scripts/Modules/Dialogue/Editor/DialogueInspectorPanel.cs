// 所属模块: Dialogue.Editor, 职责: Odin 驱动的节点属性面板
using ARPG.Data.Dialogue;
using Sirenix.OdinInspector.Editor;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPG.DialogueEditor
{
    /// <summary>
    /// 选中节点时在右侧面板显示其详细属性（Odin Inspector 渲染）
    /// </summary>
    public class DialogueInspectorPanel : VisualElement
    {
        private DialogueNodeData _selectedNodeData;
        private DialogueGraph _graphAsset;
        private ScrollView _scrollView;
        private IMGUIContainer _imguiContainer;
        private PropertyTree _propertyTree;

        public Action<DialogueNodeData> OnNodeDataChanged;

        public DialogueInspectorPanel()
        {
            style.flexGrow = 1;
            style.flexShrink = 1;
            style.minHeight = 0;
            style.overflow = Overflow.Hidden;
            style.paddingLeft = 5;
            style.paddingRight = 5;
            style.paddingTop = 5;
            style.paddingBottom = 5;

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1;
            _scrollView.style.flexShrink = 1;
            _scrollView.style.minHeight = 0;
            Add(_scrollView);

            // 使用 IMGUIContainer 内嵌 Odin PropertyTree 绘制
            _imguiContainer = new IMGUIContainer(OnIMGUI);
            _imguiContainer.style.flexGrow = 0;
            _imguiContainer.style.flexShrink = 0;
            _scrollView.Add(_imguiContainer);
        }

        /// <summary>
        /// 绑定要显示的节点数据
        /// </summary>
        public void Bind(DialogueNodeData nodeData, DialogueGraph graphAsset = null)
        {
            // 清理旧的 PropertyTree
            if (_propertyTree != null)
            {
                _propertyTree.Dispose();
                _propertyTree = null;
            }

            _selectedNodeData = nodeData;
            _graphAsset = graphAsset;

            // 用图资产（UnityEngine.Object）创建 PropertyTree，以获得自动 Undo 支持
            // 通过 [SerializeReference] 导航到具体节点，Odin 属性（BoxGroup 等）仍然生效
            if (_selectedNodeData != null && _graphAsset != null)
            {
                _propertyTree = PropertyTree.Create(_graphAsset);
            }

            // 强制重绘
            _imguiContainer?.MarkDirtyRepaint();
        }

        /// <summary>
        /// 清空面板
        /// </summary>
        public void Clear()
        {
            Bind(null);
        }

        private void OnIMGUI()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            if (_selectedNodeData == null)
            {
                var rect = GUILayoutUtility.GetRect(0, 40);
                EditorGUI.LabelField(rect, "未选中节点\n右键画布创建节点", EditorStyles.centeredGreyMiniLabel);
                GUILayout.EndVertical();
                return;
            }

            if (_graphAsset == null || _propertyTree == null)
            {
                EditorGUILayout.HelpBox("无法创建属性视图", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            // 找到选中节点在列表中的索引
            int nodeIndex = _graphAsset.nodes.IndexOf(_selectedNodeData);
            if (nodeIndex < 0)
            {
                EditorGUILayout.HelpBox("节点已从图中移除", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            // 与 Unity 序列化同步
            _propertyTree.UpdateTree();

            var nodesProp = _propertyTree.RootProperty.Children["nodes"];
            if (nodesProp == null || nodeIndex >= nodesProp.Children.Count)
            {
                GUILayout.EndVertical();
                return;
            }

            var nodeProp = nodesProp.Children[nodeIndex];

            // 标题 + GUID（只读）
            EditorGUILayout.LabelField("节点属性", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("GUID", _selectedNodeData.guid);
            EditorGUILayout.Space();

            // 用 Odin 绘制该节点的属性（通过图资产自动获得 Undo 支持）
            EditorGUI.BeginChangeCheck();
            nodeProp.Draw();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_graphAsset);
                OnNodeDataChanged?.Invoke(_selectedNodeData);
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public new void Dispose()
        {
            _propertyTree?.Dispose();
            _propertyTree = null;
            _scrollView = null;
            _imguiContainer = null;
        }
    }
}
