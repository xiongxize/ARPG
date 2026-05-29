// 所属模块: Enemy.Data, 职责: 敌人运行时数据
using System;

namespace ARPG.Data.Enemy
{
    public class EnemyRuntimeData
    {
        public int EntityId;
        public int CurrentHp;
        public bool IsDead;

        public EnemyRuntimeData(int entityId, int maxHp)
        {
            EntityId = entityId;
            CurrentHp = maxHp;
            IsDead = false;
        }

        // TODO: 数据变更逻辑
    }
}
