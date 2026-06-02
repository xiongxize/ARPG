# ARPG 可视化对话系统

## 概述

ARPG 的可视化对话系统是一个基于 **Unity UI Toolkit (GraphView)** 构建的节点图编辑器 + 运行时遍历引擎，支持设计者通过可视化节点图的方式创建游戏对话流程，而无需编写代码。

系统采用 **编辑器资产 → 运行时数据 → 事件驱动** 的分层架构，与项目整体的事件总线（EventBus）和服务定位器（ServiceLocator）模式无缝集成。

---

## 目录结构

```
Assets/Scripts/Modules/Dialogue/
├── ARPG.Modules.Dialogue.asmdef              # 运行时程序集
├── DialogueBootstrap.cs                       # 模块初始化入口（优先级15）
│
├── Data/                                      # 数据层 — ScriptableObject 资产
│   ├── DialogueGraph.cs                       # 根资产容器
│   ├── DialogueNodeData.cs                    # 节点数据抽象基类
│   ├── DialogueLinkData.cs                    # 连接线数据结构体
│   ├── NodeDataTypes.cs                       # 所有具体节点类型
│   ├── DialogueCondition.cs                   # 分支条件表达式
│   └── DialogueVariable.cs                    # 黑板变量定义
│
├── Events/                                    # 事件层
│   └── DialogueEvents.cs                      # 5个对话事件结构体
│
├── System/                                    # 系统层 — 运行时逻辑
│   ├── IDialogueService.cs                    # 公开服务接口
│   ├── DialogueSystem.cs                      # 核心遍历引擎（状态机）
│   ├── DialogueVariableStore.cs               # 运行时变量存储
│   └── ConditionEvaluator.cs                  # 条件表达式求值器
│
└── Editor/                                    # 编辑器层 — 仅编辑器域
    ├── ARPG.Modules.Dialogue.Editor.asmdef    # 编辑器程序集
    ├── DialogueGraphEditorWindow.cs           # 主窗口（三栏布局）
    ├── DialogueGraphView.cs                   # GraphView 画布
    ├── DialogueNodeView.cs                    # 节点视图基类
    ├── NodeViewTypes.cs                       # 6种具体节点视图
    ├── DialoguePort.cs                        # 端口工厂
    ├── DialogueGraphSerializer.cs             # 序列化/反序列化
    ├── DialogueSearchWindow.cs                # 右键搜索创建面板
    ├── DialogueBlackboard.cs                  # 黑板变量管理
    └── DialogueInspectorPanel.cs              # Odin 驱动的检查器

Assets/Data/Dialogue/                          # 对话图资产存放目录
└── NewDialogueGraph.asset                     # 示例对话图
```

---

## 程序集依赖

```
ARPG.Modules.Dialogue（运行时）
  ├── ARPG.Core            — ServiceLocator, EventBus
  ├── ARPG.Interfaces      — IDialogueService
  └── ARPG.Events          — 事件结构体

ARPG.Modules.Dialogue.Editor（编辑器）
  ├── ARPG.Modules.Dialogue  — 核心数据/系统
  ├── ARPG.Core
  └── ARPG.Events
```

---

## 数据层（Data Layer）

### DialogueGraph — 根资产容器

`ScriptableObject`，通过 `[CreateAssetMenu]` 创建。使用 `[SerializeReference]` 实现多态序列化，确保具体节点类型能正确持久化。

| 字段 | 类型 | 说明 |
|---|---|---|
| `nodes` | `List<DialogueNodeData>` | 所有节点数据（多态） |
| `links` | `List<DialogueLinkData>` | 所有连接线数据 |
| `variables` | `List<DialogueVariable>` | 黑板变量定义 |
| `startNodeId` | `string` | 入口起始节点 GUID |

### DialogueNodeData — 节点数据抽象基类

所有节点类型的基类：

```
DialogueNodeData（抽象）
  ├── StartNodeData        — 对话入口
  ├── DialogueLineNodeData — 说话者台词
  ├── ChoiceNodeData       — 玩家选择
  ├── BranchNodeData       — 条件分支
  ├── EventNodeData        — 游戏事件触发器
  └── EndNodeData          — 对话结束
```

| 字段 | 类型 | 说明 |
|---|---|---|
| `guid` | `string` | 节点唯一标识（HideInInspector） |
| `position` | `Vector2` | 画布位置（HideInInspector） |
| `nodeName` | `string` | 节点名称（Odin BoxGroup） |
| `NodeType` | `string`（抽象） | 类型标识字符串 |

### 节点类型详情

| 节点类型 | NodeType | 端口 | 运行时行为 |
|---|---|---|---|
| **StartNode** | `"Start"` | 1 × 输出 ("Output") | 自动前进到下一个节点 |
| **DialogueLineNode** | `"DialogueLine"` | 1 × 输入, 1 × 输出 | 发布 `DialogueLineEvent`，暂停等待 `Advance()` |
| **ChoiceNode** | `"Choice"` | 1 × 输入, N × 输出 | 发布 `DialogueChoiceEvent`，暂停等待 `MakeChoice(n)` |
| **BranchNode** | `"Branch"` | 1 × 输入, 2 × 输出 ("True"/"False") | 自动评估条件并沿 True/False 端口前进 |
| **EventNode** | `"Event"` | 1 × 输入, 1 × 输出 | 发布 `DialogueEventTrigger`，自动前进 |
| **EndNode** | `"End"` | 1 × 输入 | 触发对话结束 |

### 节点专有数据

**DialogueLineNodeData**
- `speakerName`（string）— 说话者名称
- `dialogueText`（string）— 台词文本
- `voiceOverClip`（AudioClip）— 配音剪辑

**ChoiceNodeData**
- `choices`（`List<ChoiceOption>`）— 选项列表
  - `text`（string）— 选项显示文本
  - `targetNodeId`（string）— 目标节点 GUID（由序列化器维护）

**BranchNodeData**
- `condition`（`DialogueCondition`）— 分支条件

**EventNodeData**
- `eventTypeName`（string）— 事件类型标识
- `jsonParameters`（string）— JSON 格式参数

### DialogueLinkData — 连接线数据结构体

```csharp
public struct DialogueLinkData
{
    public string sourceNodeId;      // 源节点 GUID
    public string sourcePortName;    // "Output" / "True" / "False" / "Choice_0"...
    public string targetNodeId;      // 目标节点 GUID
    public string targetPortName;    // 始终为 "Input"
}
```

### DialogueCondition — 分支条件

三部分表达式：

- `leftOperand`（string）— 左操作数（变量名或字面量）
- `op`（ConditionOperator）— 运算符：`Equals` / `NotEquals` / `GreaterThan` / `LessThan` / `GreaterThanOrEqual` / `LessThanOrEqual` / `Contains`
- `rightOperand`（string）— 右操作数（变量名或字面量）

### DialogueVariable — 黑板变量

| 字段 | 类型 | 说明 |
|---|---|---|
| `name` | `string` | 变量名称 |
| `type` | `DialogueVariableType` | 类型：`String` / `Int` / `Bool` |
| `defaultValue` | `string` | 默认值（TextArea） |

---

## 事件层（Events Layer）

所有事件为 `struct` 值类型，通过全局泛型 `EventBus<T>` 发布：

| 事件结构体 | 触发时机 | 负载数据 |
|---|---|---|
| `DialogueStartedEvent` | `StartDialogue()` 调用时 | `GraphName` |
| `DialogueLineEvent` | 到达对话行节点时 | `Speaker`, `Text` |
| `DialogueChoiceEvent` | 到达选择节点时 | `Choices`（string[]） |
| `DialogueEndedEvent` | 对话结束时 | — |
| `DialogueEventTrigger` | 到达事件节点时 | `EventType`, `JsonData` |

此外，`DialogueSystem` 还通过 C# 事件（`OnDialogueLine`、`OnChoicesPresented`、`OnDialogueEnded`）提供双重分发机制。

---

## 系统层（System Layer）

### 模块注册

```csharp
// DialogueBootstrap.cs（Priority = 15）
public void Init()
{
    var dialogueSystem = new DialogueSystem();
    ServiceLocator.Register<IDialogueService>(dialogueSystem);
}
```

### IDialogueService — 公开接口

```csharp
public interface IDialogueService
{
    void StartDialogue(DialogueGraph graph);
    void Advance();                                  // 推进到下一节点
    void MakeChoice(int choiceIndex);                // 做出选择
    void SetVariable(string name, object value);
    T GetVariable<T>(string name);
    bool IsDialogueActive { get; }
    void StopDialogue();
    event Action<DialogueLineEvent> OnDialogueLine;
    event Action<DialogueChoiceEvent> OnChoicesPresented;
    event Action OnDialogueEnded;
}
```

### DialogueSystem — 核心遍历引擎

纯 C# 类，无 MonoBehaviour 依赖。以**状态机**方式驱动对话流程：

```
StartDialogue(graph)
  ├── 存储图引用，初始化变量存储
  ├── 构建邻接表（BuildAdjacency）
  ├── 图验证（DFSCycleDetection + BFS孤儿节点检测）
  ├── 找到起始节点（startNodeId 或第一个 StartNodeData）
  ├── 发布 DialogueStartedEvent
  └── ProcessNode(startNodeId)

ProcessNode(nodeId) — 节点类型分发
  ├── StartNode   → FollowOutputPort("Output")
  ├── DialogueLine → 发布 DialogueLineEvent → 暂停（等待 Advance()）
  ├── Choice      → 发布 DialogueChoiceEvent → 暂停（等待 MakeChoice(n)）
  ├── Branch      → ConditionEvaluator.Evaluate()
  │                 → FollowOutputPort("True" 或 "False")
  ├── Event       → 发布 DialogueEventTrigger → FollowOutputPort("Output")
  └── EndNode     → EndDialogue()

EndDialogue()
  ├── 发布 DialogueEndedEvent
  └── 清空图引用
```

### DialogueVariableStore — 运行时变量存储

- 按类型分为三个字典：`_dictInts`、`_dictBools`、`_dictStrings`
- 从 `List<DialogueVariable>` 初始化默认值
- 提供类型安全的 Get/Set 和 `TryGetValue(name, out object)` 用于条件求值

### ConditionEvaluator — 条件求值器

- `ResolveValue(operand, vars)` — 优先解析为变量名，回退为字面量（int → bool → string）
- 根据左操作数类型分发比较器
- int/string 支持全部运算符；bool 仅支持等于/不等于

---

## 编辑器层（Editor Layer）

基于 **Unity UI Toolkit（UnityEditor.Experimental.GraphView）** 构建的节点图编辑器。

### DialogueGraphEditorWindow — 主窗口

三栏布局：

```
┌─────────────────────────────────────────────────────┐
│ [工具栏]  打开  保存  验证  新建   |  图名称         │
├─────────────────────────────────────┬───────────────┤
│                                     │ DialogueBlackboard │
│     DialogueGraphView               │ (变量列表，180px) │
│     (节点画布，弹性宽度)              ├───────────────┤
│                                     │ DialogueInspectorPanel │
│                                     │ (Odin属性面板，弹性) │
│                                     │               │
└─────────────────────────────────────┴───────────────┘
```

- 通过 `[OnOpenAsset(1)]` 支持双击 `.asset` 文件打开
- `SessionState` 在领域重载时恢复活跃图
- Ctrl+S 快捷键保存，`Undo.RecordObject` 支持撤销
- 新建图时自动插入一个 StartNodeData

### DialogueGraphView — 画布

继承 `GraphView`，包含：

- **网格背景**（GridBackground）
- **缩放** 0.1× ~ 2×，内容拖拽，选择拖拽，矩形选择
- **MiniMap** 锚定右上角 200×140
- **右键搜索创建**（DialogueSearchWindow）
- **graphViewChanged 回调**追踪变化
- **连线过滤**（`GetCompatiblePorts`）：禁止自连和同向连
- **选择节点特殊刷新**（`RefreshNodeView`）：重建端口和边以匹配选项数量变化

### DialogueNodeView — 节点视图基类

继承 `UnityEditor.Experimental.GraphView.Node`。

- 构造函数：生成 GUID（如无）、设置标题/位置/CSS 类/最小宽度 180
- 抽象方法 `CreatePorts()` 由子类实现
- `FindPort(portName, direction)` — 按 ID 查找端口
- `PopulateFromData()` — 从 NodeData 刷新视图（子类重写添加预览标签）
- `UpdateDataFromView()` — 反向同步位置（保存时调用）

### 具体节点视图（NodeViewTypes）

| 视图类 | 标题 | 颜色 | 端口 | 特殊逻辑 |
|---|---|---|---|---|
| `StartNodeView` | "▶ 开始" | 绿色 | 仅输出 "Output" | — |
| `DialogueLineNodeView` | "💬 对话行" | 蓝色 | 输入 + 输出 | 扩展容器中显示 `speakerName: dialogueText` 预览 |
| `ChoiceNodeView` | "❓ 选择" | 橙色 | 输入 + N×输出 | 端口标签 `"N. {text}"`（截断24字符）；`SyncChoicePorts()` 重建端口 |
| `BranchNodeView` | "🔀 分支" | 紫色 | 输入 + "True"(绿) + "False"(红) | — |
| `EventNodeView` | "⚡ 事件/⚡ {名称}" | 黄色 | 输入 + 输出 | — |
| `EndNodeView` | "■ 结束" | 红色 | 仅输入 | — |

### DialoguePort — 端口工厂

- 创建 `Port.Create<Edge>(Direction, Capacity.Single, typeof(bool))`
- 输入端口蓝色 `(0.2, 0.6, 1.0)`，输出端口橙色 `(1.0, 0.6, 0.2)`
- `GetId(port)` 通过 `userData → name → portName` 回退链提取端口标识

### DialogueGraphSerializer — 序列化/反序列化

**保存流程** (`SaveToAsset`)：
1. 遍历所有 `DialogueNodeView`，调用 `UpdateDataFromView()` 同步位置
2. 重置所有 `ChoiceOption.targetNodeId` 为 null
3. 遍历所有 `Edge`，提取 `DialogueLinkData`，设置 `ChoiceOption.targetNodeId`
4. 从黑板获取变量
5. 将 nodes/links/variables 写入资产并标记脏

**加载流程** (`LoadFromAsset`)：
1. 清除整个图
2. 遍历节点数据，通过 `CreateNodeViewForData` 创建视图
3. 遍历链接，按端口名查找端口并创建 Edge
4. 恢复黑板变量

**验证** (`ValidateGraph`)：
- 检查是否有起始节点
- 多起始节点警告
- DFS 循环检测
- BFS 孤儿节点检测
- 缺少结束节点警告

### DialogueSearchWindow — 搜索创建面板

实现 `ISearchWindowProvider`，提供 6 个节点类型条目。选中后将屏幕坐标转为内容容器坐标，调用 `CreateNodeAt()`。

### DialogueBlackboard — 黑板变量管理

基于 UI Toolkit 的 `VisualElement`，滚动列表每行包含：
- 名称字段（TextField）
- 删除按钮（红色 "✕"）
- 类型枚举（EnumField：String/Int/Bool）
- 默认值字段（TextField）

提供 `GetVariables()` / `SetVariables()` 用于序列化交互。

### DialogueInspectorPanel — Odin 检查器

基于 Odin Inspector 的属性面板，使用 `IMGUIContainer` 嵌入 Odin 的 `PropertyTree`。

- 绑定选中的节点数据（利用 Odin 的 `[SerializeReference]` 导航）
- 渲染节点标题、GUID（只读）、专有字段
- 属性变化时：`EditorUtility.SetDirty()` + 触发 `OnNodeDataChanged` 回调刷新视图

---

## 运行时流程图

```
外部调用方
  │
  ▼
ServiceLocator.Get<IDialogueService>()
  │  .StartDialogue(graph)
  ▼
DialogueSystem.StartDialogue()
  ├─ EventBus<DialogueStartedEvent>.Publish()
  └─ ProcessNode(startNodeId)
       │
       ▼
  ┌───────────┐
  │ StartNode │──► FollowOutputPort("Output") ──► ProcessNode(next)
  └───────────┘
       │
  ┌──────────────────┐
  │ DialogueLineNode │──► EventBus<DialogueLineEvent>.Publish()
  └──────────────────┘    ──► 暂停 ──► 等待外部 Advance()
       │
  ┌────────────┐
  │ ChoiceNode │──► EventBus<DialogueChoiceEvent>.Publish()
  └────────────┘    ──► 暂停 ──► 等待外部 MakeChoice(n)
       │
  ┌─────────────┐
  │ BranchNode  │──► ConditionEvaluator.Evaluate()
  └─────────────┘    ──► FollowOutputPort("True"/"False") ──► ProcessNode(next)
       │
  ┌────────────┐
  │ EventNode  │──► EventBus<DialogueEventTrigger>.Publish()
  └────────────┘    ──► FollowOutputPort("Output") ──► ProcessNode(next)
       │
  ┌──────────┐
  │ EndNode  │──► EventBus<DialogueEndedEvent>.Publish() ──► 停止
  └──────────┘
```

**对话节点类型与 UI 层的交互**：

```
DialogueLineNode          →  EventBus<DialogueLineEvent>
                              ↓
                          [UI 对话面板] 显示说话者名称 + 台词文本
                          [用户点击"继续"] → dialogueService.Advance()

ChoiceNode                →  EventBus<DialogueChoiceEvent>
                              ↓
                          [UI 选项面板] 显示 N 个选项按钮
                          [用户点击选项 n] → dialogueService.MakeChoice(n)

EventNode                 →  EventBus<DialogueEventTrigger>
                              ↓
                          [其他模块] 订阅事件 → 执行游戏逻辑
                          （自动前进，无需用户操作）

BranchNode                →  内部评估 ConditionEvaluator.Evaluate()
                              ↓
                          [对话系统] 自动选择 True/False 分支
                          （不发布事件，不由 UI 控制）
```

---

## 特性清单

### 已实现
- [x] 基于 UI Toolkit GraphView 的节点图编辑器
- [x] 6 种节点类型（开始/对话行/选择/分支/事件/结束）
- [x] 多态序列化（`[SerializeReference]`）确保持久化
- [x] 运行时遍历引擎（状态机模式）
- [x] 条件分支系统（变量名/字面量求值）
- [x] 黑板变量系统（String/Int/Bool）
- [x] 事件系统集成（EventBus 双分发）
- [x] Odin Inspector 属性面板
- [x] MiniMap 导航
- [x] 右键搜索创建节点
- [x] 图验证（循环/孤儿节点检测）
- [x] Undo/Redo 支持
- [x] 节点颜色编码
- [x] 选择节点端口动态重建

### 待扩展
- [ ] 节点复制/粘贴
- [ ] 子对话/嵌套图（SubGraph）
- [ ] 对话预览模式（PlayMode 预览）
- [ ] 导入/导出（JSON/CSV）
- [ ] 对话日志/调试工具

---

## 使用流程

1. **创建对话图资产**：在 Project 窗口中右键 → Create → Dialogue → Dialogue Graph
2. **打开编辑器**：双击 `.asset` 文件
3. **构建对话流程**：
   - 右键搜索添加节点
   - 拖拽连接线建立关系
   - 在 Odin 检查器中编辑节点属性
   - 在黑板上定义变量
4. **触发对话**：
   ```csharp
   var dialogue = ServiceLocator.Get<IDialogueService>();
   var graph = Resources.Load<DialogueGraph>("MyDialogue");
   dialogue.StartDialogue(graph);
   ```
5. **监听事件**：各模块通过 `EventBus<T>` 订阅对话事件驱动 UI/逻辑

---

*文档版本：1.0 | 最后更新：2026-06-02*
