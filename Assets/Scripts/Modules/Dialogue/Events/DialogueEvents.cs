// 所属模块: Dialogue.Events, 职责: 对话系统事件定义
namespace ARPG.Events
{
    /// <summary>
    /// 对话开始时触发
    /// </summary>
    public struct DialogueStartedEvent
    {
        public string GraphName;
    }

    /// <summary>
    /// 播放对话行时触发
    /// </summary>
    public struct DialogueLineEvent
    {
        public string Speaker;
        public string Text;
    }

    /// <summary>
    /// 到达选择节点时触发
    /// </summary>
    public struct DialogueChoiceEvent
    {
        public string[] Choices;
    }

    /// <summary>
    /// 对话结束时触发
    /// </summary>
    public struct DialogueEndedEvent
    {
    }

    /// <summary>
    /// 事件节点触发时发布，由其他模块通过 EventBus 订阅处理
    /// </summary>
    public struct DialogueEventTrigger
    {
        public string EventType;
        public string JsonData;
    }
}
