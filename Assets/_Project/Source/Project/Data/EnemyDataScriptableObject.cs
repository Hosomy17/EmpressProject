using UnityEngine;

namespace Com.Voobox.Project.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Data/Enemy Data")]
    public class EnemyDataScriptableObject : ScriptableObject
    {
        [SerializeField] private EnemyData m_enemyData;

        public EnemyData EnemyData => m_enemyData;
    }
}
