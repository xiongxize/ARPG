// 所属模块: Interfaces, 职责: 战斗系统服务接口
namespace ARPG.Interfaces
{
    public interface ICombatService
    {
        // TODO: 定义战斗系统核心能力
        void RequestAttack(int attackerId, int targetId, int skillId);
    }
}
