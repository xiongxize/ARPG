// 所属模块: Events, 职责: 战斗相关事件定义
namespace ARPG.Events
{
    public struct PlayerAttackEvent
    {
        public int PlayerId;
        public int TargetId;
        public int SkillId;
    }

    public struct EnemyHurtEvent
    {
        public int EnemyId;
        public int DamageAmount;
        public int CurrentHp;
    }

    public struct EnemyDiedEvent
    {
        public int EnemyId;
    }
}
