// 所属模块: Enemy.Presentation, 职责: 敌人表现层逻辑（动画、特效、受击反馈）
using UnityEngine;
using ARPG.Core.EventBus;
using ARPG.Events;

namespace ARPG.Presentation.Enemy
{
    public class EnemyView : AutoEventView
    {
        [SerializeField] private int _entityId;

        // TODO: 引用 Animator, ParticleSystem 等

        [EventSubscriber]
        private void OnEnemyHurt(EnemyHurtEvent evt)
        {
            if (evt.EnemyId != _entityId) return;

            // TODO: 播放受击动画或特效
            Debug.Log($"Presentation: Enemy {_entityId} played hurt animation.");
        }

        [EventSubscriber]
        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.EnemyId != _entityId) return;

            // TODO: 播放死亡动画
            Debug.Log($"Presentation: Enemy {_entityId} died.");
        }
    }
}
