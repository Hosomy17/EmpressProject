using UnityEngine;

namespace Com.Voobox.Project.Data
{
    [CreateAssetMenu(fileName = "HeroData", menuName = "Data/Hero Data")]
    public class HeroDataScriptableObject : ScriptableObject
    {
        [SerializeField] private HeroData m_heroData;

        public HeroData HeroData => m_heroData;
    }
}