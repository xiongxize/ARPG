// 所属模块: Enemy.UI, 职责: 敌人血条 UI 逻辑
using UnityEngine;
using UnityEngine.UI;
using ARPG.Core.EventBus;
using ARPG.Events;

namespace ARPG.UI.HUD
{
    public class EnemyHealthBarUI : AutoEventView
    {
        [SerializeField] private int _entityId;
        [SerializeField] private Slider _hpSlider;

        [EventSubscriber]
        private void OnEnemyHurt(EnemyHurtEvent evt)
        {
            if (evt.EnemyId != _entityId) return;

            // TODO: 更新血条 UI
            if (_hpSlider != null)
            {
                // _hpSlider.value = (float)evt.CurrentHp / maxHp; 
            }
            Debug.Log($"UI: Updated health bar for Enemy {_entityId} to {evt.CurrentHp}");
        }
    }
}
