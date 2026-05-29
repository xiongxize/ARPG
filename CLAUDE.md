# CLAUDE.md

本文档为 Claude Code（claude.ai/code）在处理本代码库中的代码时提供指导。

## 项目概述

ARPG 是一款基于 Unity 6000.0.28f1（URP 通用渲染管线）、C# 9.0 开发的模块化动作角色扮演游戏，目标平台为 Windows 独立版。代码库按独立程序集组织，具有清晰的依赖分层结构。

## 架构

### 程序集依赖图（严格自底向上）

```
ARPG.Interfaces （无依赖）
     ↑
ARPG.Events      （无依赖）
     ↑
ARPG.Core        （事件总线、服务定位器、模块系统）
     ↑
ARPG.Modules.Enemy   （依赖 Core、Events）
ARPG.Modules.Combat  （依赖 Core、Interfaces、Events）
```

**核心原则**：模块之间从不直接引用——它们通过事件总线（EventBus）和服务定位器（ServiceLocator）进行通信。

### 核心基础设施（`Assets/Scripts/Core/`）

- **EventBus`<T>`**（`Core/EventBus/EventBus.cs`）—— 泛型静态事件总线。事件采用`struct`（值类型）以最大限度减少垃圾回收（GC）。发布事件时会反向遍历订阅者，以支持在回调期间安全取消订阅。
- **AutoEventView**（`Core/EventBus/AutoEventView.cs`）—— 抽象的`MonoBehaviour`基类。在`OnEnable`阶段通过反射扫描带有`[EventSubscriber]`特性的方法，自动订阅`EventBus<T>`，并在`OnDisable`阶段取消订阅。
- **ServiceLocator**（`Core/ServiceLocator.cs`）—— 全局`Dictionary<Type, object>`服务注册表。在初始化时注册服务，通过接口检索。该组件已标记为待替换为依赖注入（DI）框架。
- **模块系统**（`Core/Module/`）—— `GameBootstrap`（MonoBehaviour，在 Awake 阶段运行）通过反射扫描所有已加载程序集中的`IModuleBootstrap`实现，实例化这些实现，并按`Priority`（优先级）顺序调用`Init()`方法。
    - `ModuleRegistry.DiscoverModules()` 扫描`AppDomain.CurrentDomain.GetAssemblies()`中所有`IModuleBootstrap`类型（非抽象、非接口），通过`Activator.CreateInstance`实例化，并按`Priority`排序。
- **当前优先级**：战斗模块（Combat）=10，背包模块（Inventory）=20

### 接口（`Assets/Scripts/Interfaces/`）

- `ICombatService` —— `RequestAttack(attackerId, targetId, skillId)`（请求攻击，参数：攻击者ID、目标ID、技能ID）
- `IDamageable` —— `TakeDamage(int amount)`（承受伤害，参数：伤害数值）

### 事件（`Assets/Scripts/Events/`）

所有战斗事件均为`struct`值类型：
- `PlayerAttackEvent` —— 包含 PlayerId（玩家ID）、TargetId（目标ID）、SkillId（技能ID）
- `EnemyHurtEvent` —— 包含 EnemyId（敌人ID）、DamageAmount（伤害数值）、CurrentHp（当前生命值）
- `EnemyDiedEvent` —— 包含 EnemyId（敌人ID）

### 模块（`Assets/Scripts/Modules/`）

每个模块遵循以下结构：一个实现`IModuleBootstrap`的`*Bootstrap`类，以及`System`（系统）、`Data`（数据）、`Presentation`（表现层）、`UI`（界面）子文件夹。

**战斗模块（Combat Module）**（`Modules/Combat/`）：
- `CombatBootstrap`（优先级=10）—— 将`CombatSystem`注册为服务定位器中的`ICombatService`
- `CombatSystem` —— 订阅`PlayerAttackEvent`（玩家攻击事件），发布`EnemyHurtEvent`（敌人受击事件）

**敌人模块（Enemy Module）**（`Modules/Enemy/`）：
- `EnemyConfig` —— 可编写脚本对象（ScriptableObject）数据（包含 EnemyName 敌人名称、MaxHp 最大生命值、MoveSpeed 移动速度）
- `EnemyRuntimeData` —— 运行时数据（包含 EntityId 实体ID、CurrentHp 当前生命值、IsDead 是否死亡）
- `EnemyView`（MonoBehaviour，继承 AutoEventView）—— 监听`EnemyHurtEvent`/`EnemyDiedEvent`（敌人受击/死亡事件），播放动画/特效
- `EnemyHealthBarUI`（MonoBehaviour，继承 AutoEventView）—— 监听`EnemyHurtEvent`（敌人受击事件），更新生命值滑块

**背包模块（Inventory Module）**：
- `InventoryBootstrap`（优先级=20）—— 占位符（待实现）

### 数据流模式

```
输入 → 触发 PlayerAttackEvent → 执行 CombatSystem.OnPlayerAttack
  → 调用 CombatSystem.RequestAttack → 发布 EnemyHurtEvent
    → 执行 EnemyView.OnEnemyHurt（播放动画/特效）
    → 执行 EnemyHealthBarUI.OnEnemyHurt（更新UI）
```

## 核心约定

- 事件：始终为`struct`类型，命名空间为`ARPG.Events`
- 启动类命名：`{ModuleName}Bootstrap`（如CombatBootstrap），命名空间为`ARPG.System.{ModuleName}`
- 视图类继承`AutoEventView`，并使用`[EventSubscriber]`特性实现事件自动绑定
- 配置数据使用带有`[CreateAssetMenu]`特性的`ScriptableObject`

## 第三方插件

- **Odin Inspector**（Sirenix）—— 编辑器工具
- **DOTween Pro**（Demigiant）—— 补间动画插件
- **Unity Input System** —— 现役输入处理系统

## 命令

- **构建**：在 Unity 编辑器中打开 → 文件 → 构建设置 → 构建
- **代码编译**：Unity 在保存时自动编译；解决方案文件为`ARPG.sln`
- **测试**：通过 Unity 测试运行器执行（窗口 → 常规 → 测试运行器）。未配置命令行测试命令。