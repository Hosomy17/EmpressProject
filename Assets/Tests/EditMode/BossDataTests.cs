using System.Reflection;
using Com.Voobox.Project.Data;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class BossDataTests
    {
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        [Test]
        public void BossData_ImplementsIHealthDataAndExposesConfiguredValues()
        {
            var bossData = new BossData();

            SetPrivateField(bossData, "m_maxHitPoints", 25);
            SetPrivateField(bossData, "m_hurtLayer", (LayerMask)(1 << 8));
            SetPrivateField(bossData, "m_knockbackForce", 7.25f);

            var bossDataSo = ScriptableObject.CreateInstance<BossDataScriptableObject>();
            try
            {
                SetPrivateField(bossDataSo, "m_bossData", bossData);

                BossData exposed = bossDataSo.BossData;
                Assert.AreSame(bossData, exposed);
                Assert.IsInstanceOf<IHealthData>(exposed);

                IHealthData healthData = exposed;
                Assert.AreEqual(25, healthData.MaxHitPoints);
                Assert.AreEqual((LayerMask)(1 << 8), healthData.HurtLayer);
                Assert.AreEqual(7.25f, healthData.KnockbackForce, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(bossDataSo);
            }
        }
    }
}
