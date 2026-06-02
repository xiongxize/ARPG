// 所属模块: Dialogue.Editor, 职责: 右键添加节点的搜索窗口
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPG.DialogueEditor
{
    /// <summary>
    /// 右键点击画布空白处时弹出的节点类型搜索窗口
    /// </summary>
    public class DialogueSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueGraphView _graphView;

        public void Initialize(DialogueGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("创建对话节点"), 0),

                new SearchTreeEntry(new GUIContent("▶ 开始节点"))   { level = 1, userData = "Start" },
                new SearchTreeEntry(new GUIContent("💬 对话行节点"))  { level = 1, userData = "DialogueLine" },
                new SearchTreeEntry(new GUIContent("❓ 选择节点"))    { level = 1, userData = "Choice" },
                new SearchTreeEntry(new GUIContent("🔀 条件分支节点")) { level = 1, userData = "Branch" },
                new SearchTreeEntry(new GUIContent("⚡ 事件节点"))    { level = 1, userData = "Event" },
                new SearchTreeEntry(new GUIContent("■ 结束节点"))    { level = 1, userData = "End" },
            };

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (_graphView == null) return false;

            var typeName = entry.userData as string;
            var mousePos = _graphView.ChangeCoordinatesTo(
                _graphView.contentViewContainer,
                context.screenMousePosition - _graphView.panel.visualTree.worldBound.position
            );

            _graphView.CreateNodeAt(typeName, mousePos);
            return true;
        }
    }
}
