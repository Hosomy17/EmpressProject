using UnityEngine;

namespace Com.Voobox.Project.Data
{
    [CreateAssetMenu(fileName = "BossData", menuName = "Data/Boss Data")]
    public class BossDataScriptableObject : ScriptableObject
    {
        [SerializeField] private BossData m_bossData;

        public BossData BossData => m_bossData;
    }
}