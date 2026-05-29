// 所属模块: Core.Module, 职责: 模块初始化接口定义
namespace ARPG.Core.Module
{
    public interface IModuleBootstrap
    {
        /// <summary>
        /// 模块初始化优先级，值越小越早初始化
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 初始化逻辑
        /// </summary>
        void Init();
    }
}
