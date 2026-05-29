// 所属模块: Combat.System, 职责: 战斗系统逻辑处理
using ARPG.Interfaces;
using ARPG.Events;
using ARPG.Core.EventBus;
using UnityEngine;

namespace ARPG.System.Combat
{
    public class CombatSystem : ICombatService
    {
        public CombatSystem()
        {
            // TODO: 构造函数中可以订阅必要的系统层事件
            EventBus<PlayerAttackEvent>.Subscribe(OnPlayerAttack);
        }

        public void RequestAttack(int attackerId, int targetId, int skillId)
        {
            // TODO: 处理攻击请求逻辑
            Debug.Log($"System: Processing attack from {attackerId} to {targetId}");
            
            // 示例：发布受伤事件
            EventBus<EnemyHurtEvent>.Publish(new EnemyHurtEvent 
            { 
                EnemyId = targetId, 
                DamageAmount = 10,
                CurrentHp = 90 // 实际应从数据层获取
            });
        }

        private void OnPlayerAttack(PlayerAttackEvent evt)
        {
            // TODO: 处理玩家攻击事件
            RequestAttack(evt.PlayerId, evt.TargetId, evt.SkillId);
        }
    }
}
