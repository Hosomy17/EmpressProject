using System.Reflection;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Others;
using FMODUnity;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class HeroDataTests
    {
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        [Test]
        public void HeroData_ImplementsExpectedInterfacesAndExposesConfiguredValues()
        {
            var heroData = new HeroData();
            var sfxDashRef = new EventReference();
            var sfxJumpRef = new EventReference();

            SetPrivateField(heroData, "m_attackDuration", 0.35f);
            SetPrivateField(heroData, "m_dashForce", 16.5f);
            SetPrivateField(heroData, "m_dashDuration", 0.25f);
            SetPrivateField(heroData, "m_dashCooldown", 0.9f);
            SetPrivateField(heroData, "m_sfxDashEventReference", sfxDashRef);
            SetPrivateField(heroData, "m_maxHitPoints", 5);
            SetPrivateField(heroData, "m_hurtLayer", (LayerMask)(1 << 7));
            SetPrivateField(heroData, "m_knockbackForce", 4.75f);
            SetPrivateField(heroData, "m_jumpForce", 13.5f);
            SetPrivateField(heroData, "m_jumpBufferTime", 0.18f);
            SetPrivateField(heroData, "m_groundLayer", (LayerMask)(1 << 6));
            SetPrivateField(heroData, "m_sfxJumpEventReference", sfxJumpRef);
            SetPrivateField(heroData, "m_speed", 8.25f);
            SetPrivateField(heroData, "m_recoilBackForce", 5.5f);
            SetPrivateField(heroData, "m_recoilUpForce", 3.25f);
            SetPrivateField(heroData, "m_recoilDuration", 0.12f);
            SetPrivateField(heroData, "m_smoothSpeed", 12.5f);
            SetPrivateField(heroData, "m_screenShakeForce", 1.75f);
            SetPrivateField(heroData, "m_screenShakeDirection", new Vector2(1f, -0.5f));

            var bodyGo = new GameObject("HeroDataTestBody");
            var attackUpGo = new GameObject("HeroDataTestAttackUp");
            var attackDownGo = new GameObject("HeroDataTestAttackDown");
            var attackLeftGo = new GameObject("HeroDataTestAttackLeft");
            var attackRightGo = new GameObject("HeroDataTestAttackRight");
            var cameraGo = new GameObject("HeroDataTestCamera");
            var heroDataSo = ScriptableObject.CreateInstance<HeroDataScriptableObject>();

            try
            {
                var rb = bodyGo.AddComponent<Rigidbody2D>();

                attackUpGo.AddComponent<BoxCollider2D>();
                var attackUp = attackUpGo.AddComponent<AttackArea>();

                attackDownGo.AddComponent<BoxCollider2D>();
                var attackDown = attackDownGo.AddComponent<AttackArea>();

                attackLeftGo.AddComponent<BoxCollider2D>();
                var attackLeft = attackLeftGo.AddComponent<AttackArea>();

                attackRightGo.AddComponent<BoxCollider2D>();
                var attackRight = attackRightGo.AddComponent<AttackArea>();

                var impulseSource = cameraGo.AddComponent<CinemachineImpulseSource>();
                var positionComposer = cameraGo.AddComponent<CinemachinePositionComposer>();

                heroData.AttackUp = attackUp;
                heroData.AttackDown = attackDown;
                heroData.AttackLeft = attackLeft;
                heroData.AttackRight = attackRight;
                heroData.ImpulseSource = impulseSource;
                heroData.PositionComposer = positionComposer;
                heroData.Rigidbody2D = rb;

                SetPrivateField(heroDataSo, "m_heroData", heroData);

                HeroData exposed = heroDataSo.HeroData;
                Assert.AreSame(heroData, exposed);
                Assert.IsInstanceOf<IAttackerData>(exposed);
                Assert.IsInstanceOf<IDasherData>(exposed);
                Assert.IsInstanceOf<IHealthData>(exposed);
                Assert.IsInstanceOf<IJumpData>(exposed);
                Assert.IsInstanceOf<IMovementData>(exposed);
                Assert.IsInstanceOf<IScreenShakerData>(exposed);

                IAttackerData attackerData = exposed;
                Assert.AreEqual(0.35f, attackerData.AttackDuration, 0.0001f);
                Assert.AreSame(attackUp, attackerData.AttackUp);
                Assert.AreSame(attackDown, attackerData.AttackDown);
                Assert.AreSame(attackLeft, attackerData.AttackLeft);
                Assert.AreSame(attackRight, attackerData.AttackRight);

                IDasherData dasherData = exposed;
                Assert.AreEqual(16.5f, dasherData.DashForce, 0.0001f);
                Assert.AreEqual(0.25f, dasherData.DashDuration, 0.0001f);
                Assert.AreEqual(0.9f, dasherData.DashCooldown, 0.0001f);
                Assert.AreEqual(sfxDashRef, dasherData.SFXDashEventReference);
                Assert.AreSame(rb, dasherData.Rigidbody2D);

                IHealthData healthData = exposed;
                Assert.AreEqual(5, healthData.MaxHitPoints);
                Assert.AreEqual((LayerMask)(1 << 7), healthData.HurtLayer);
                Assert.AreEqual(4.75f, healthData.KnockbackForce, 0.0001f);

                IJumpData jumpData = exposed;
                Assert.AreEqual(13.5f, jumpData.JumpForce, 0.0001f);
                Assert.AreEqual(0.18f, jumpData.JumpBufferTime, 0.0001f);
                Assert.AreEqual((LayerMask)(1 << 6), jumpData.GroundLayer);
                Assert.AreEqual(sfxJumpRef, jumpData.SFXJumpEventReference);
                Assert.AreSame(rb, jumpData.Rigidbody2D);

                IMovementData movementData = exposed;
                Assert.AreEqual(8.25f, movementData.Speed, 0.0001f);
                Assert.AreEqual(5.5f, movementData.RecoilBackForce, 0.0001f);
                Assert.AreEqual(3.25f, movementData.RecoilUpForce, 0.0001f);
                Assert.AreEqual(0.12f, movementData.RecoilDuration, 0.0001f);
                Assert.AreSame(rb, movementData.Rigidbody2D);

                IScreenShakerData screenShakerData = exposed;
                Assert.AreEqual(12.5f, screenShakerData.SmoothSpeed, 0.0001f);
                Assert.AreEqual(1.75f, screenShakerData.ScreenShakeForce, 0.0001f);
                Assert.AreEqual(new Vector2(1f, -0.5f), screenShakerData.ScreenShakeDirection);
                Assert.AreSame(impulseSource, screenShakerData.ImpulseSource);
                Assert.AreSame(positionComposer, screenShakerData.PositionComposer);
                Assert.AreSame(rb, screenShakerData.Rigidbody2D);
            }
            finally
            {
                Object.DestroyImmediate(heroDataSo);
                Object.DestroyImmediate(cameraGo);
                Object.DestroyImmediate(attackRightGo);
                Object.DestroyImmediate(attackLeftGo);
                Object.DestroyImmediate(attackDownGo);
                Object.DestroyImmediate(attackUpGo);
                Object.DestroyImmediate(bodyGo);
            }
        }
    }
}
