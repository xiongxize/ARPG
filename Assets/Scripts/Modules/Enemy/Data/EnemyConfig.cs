// 所属模块: Enemy.Data, 职责: 敌人静态配置数据
using UnityEngine;

namespace ARPG.Data.Enemy
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "ARPG/Data/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public string EnemyName;
        public int MaxHp;
        public float MoveSpeed;
    }
}
