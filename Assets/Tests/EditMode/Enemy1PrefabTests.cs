using System.Reflection;
using Com.Voobox.Project.AI;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Entity;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class Enemy1PrefabTests
    {
        private const string Enemy1DataPath = "Assets/_Project/ScriptableObject/Enemy1Data.asset";
        private const string Enemy1BrainPath = "Assets/_Project/ScriptableObject/Enemy1Brain.asset";
        private const string Enemy1PrefabPath = "Assets/_Project/Prefabs/Units/enemy1.prefab";

        private const int HeroLayer = 3;
        private const int EnemyLayer = 7;
        private const int AttackAreaLayer = 8;
        private const int SensorLayer = 9;

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, args);
        }

        [Test]
        public void Enemy1DataAsset_ExistsAndHasExpectedStats()
        {
            var enemyDataSo = AssetDatabase.LoadAssetAtPath<EnemyDataScriptableObject>(Enemy1DataPath);
            Assert.IsNotNull(enemyDataSo, $"Expected EnemyDataScriptableObject at '{Enemy1DataPath}'.");

            EnemyData data = enemyDataSo.EnemyData;
            Assert.IsNotNull(data);
            Assert.AreEqual(3, data.MaxHitPoints);
            Assert.AreEqual(1 << AttackAreaLayer, data.HurtLayer.value);
            Assert.AreEqual(3.5f, data.Speed, 0.0001f);
            Assert.AreEqual(12f, data.DashForce, 0.0001f);
            Assert.AreEqual(0.25f, data.DashDuration, 0.0001f);
            Assert.AreEqual(0.5f, data.DashCooldown, 0.0001f);
            Assert.AreEqual(5.5f, data.DetectionRange, 0.0001f);
            Assert.AreEqual(1 << HeroLayer, data.DetectionTargetLayer.value);
        }

        [Test]
        public void Enemy1BrainAsset_ExistsAndConfiguresFiveStateGraph()
        {
            var brain = AssetDatabase.LoadAssetAtPath<BrainProfileScriptableObject>(Enemy1BrainPath);
            Assert.IsNotNull(brain, $"Expected BrainProfileScriptableObject at '{Enemy1BrainPath}'.");

            Assert.AreEqual("Idle", brain.InitialStateId);
            Assert.AreEqual(5, brain.States.Count);

            // 1. Idle
            AIStateDefinition idle = brain.States[0];
            Assert.AreEqual("Idle", idle.StateId);
            Assert.AreEqual(1, idle.Behaviors.Count);
            Assert.IsInstanceOf<IdleBehavior>(idle.Behaviors[0]);
            Assert.AreEqual(1, idle.Transitions.Count);
            Assert.AreEqual("Combat", idle.Transitions[0].TargetStateId);
            Assert.IsInstanceOf<TargetEnteredZoneCondition>(idle.Transitions[0].Condition);

            // 2. Combat
            AIStateDefinition combat = brain.States[1];
            Assert.AreEqual("Combat", combat.StateId);
            Assert.AreEqual(1, combat.Behaviors.Count);
            var combatMove = combat.Behaviors[0] as MoveTowardTargetBehavior;
            Assert.IsNotNull(combatMove);
            Assert.AreEqual(1.0f, combatMove.SpeedMultiplier, 0.0001f);
            Assert.AreEqual(2.5f, combatMove.StopDistance, 0.0001f);
            Assert.AreEqual(2, combat.Transitions.Count);
            Assert.AreEqual("Hunting", combat.Transitions[0].TargetStateId);
            Assert.IsInstanceOf<TargetExitedZoneCondition>(combat.Transitions[0].Condition);
            Assert.AreEqual("LungeAttack", combat.Transitions[1].TargetStateId);
            var lungeCondition = combat.Transitions[1].Condition as AllOfCondition;
            Assert.IsNotNull(lungeCondition);
            Assert.AreEqual(2, lungeCondition.Conditions.Count);
            var distCond = lungeCondition.Conditions[0] as TargetWithinDistanceCondition;
            Assert.IsNotNull(distCond);
            Assert.AreEqual(2.5f, distCond.MinDistance, 0.0001f);
            var canDashCond = lungeCondition.Conditions[1] as BodyFlagCondition;
            Assert.IsNotNull(canDashCond);
            Assert.AreEqual(BodyFlag.CanDash, canDashCond.Flag);
            Assert.IsTrue(canDashCond.ExpectedValue);

            // 3. LungeAttack
            AIStateDefinition lunge = brain.States[2];
            Assert.AreEqual("LungeAttack", lunge.StateId);
            Assert.AreEqual(1, lunge.Behaviors.Count);
            Assert.IsInstanceOf<RequestDashBehavior>(lunge.Behaviors[0]);
            Assert.AreEqual(1, lunge.Transitions.Count);
            Assert.AreEqual("Waiting", lunge.Transitions[0].TargetStateId);
            var notDashingCond = lunge.Transitions[0].Condition as BodyFlagCondition;
            Assert.IsNotNull(notDashingCond);
            Assert.AreEqual(BodyFlag.IsDashing, notDashingCond.Flag);
            Assert.IsFalse(notDashingCond.ExpectedValue);

            // 4. Waiting
            AIStateDefinition waiting = brain.States[3];
            Assert.AreEqual("Waiting", waiting.StateId);
            Assert.AreEqual(1, waiting.Behaviors.Count);
            Assert.IsInstanceOf<IdleBehavior>(waiting.Behaviors[0]);
            Assert.AreEqual(2, waiting.Transitions.Count);

            Assert.AreEqual("Combat", waiting.Transitions[0].TargetStateId);
            var waitToCombat = waiting.Transitions[0].Condition as AllOfCondition;
            Assert.IsNotNull(waitToCombat);
            Assert.AreEqual(2, waitToCombat.Conditions.Count);
            var waitElapsed1 = waitToCombat.Conditions[0] as StateElapsedCondition;
            Assert.IsNotNull(waitElapsed1);
            Assert.AreEqual(1.5f, waitElapsed1.Duration, 0.0001f);
            Assert.IsInstanceOf<TargetWithinDetectionRangeCondition>(waitToCombat.Conditions[1]);

            Assert.AreEqual("Idle", waiting.Transitions[1].TargetStateId);
            var waitToIdle = waiting.Transitions[1].Condition as AllOfCondition;
            Assert.IsNotNull(waitToIdle);
            Assert.AreEqual(2, waitToIdle.Conditions.Count);
            var waitElapsed2 = waitToIdle.Conditions[0] as StateElapsedCondition;
            Assert.IsNotNull(waitElapsed2);
            Assert.AreEqual(1.5f, waitElapsed2.Duration, 0.0001f);
            var notInRange = waitToIdle.Conditions[1] as NotCondition;
            Assert.IsNotNull(notInRange);
            Assert.IsInstanceOf<TargetWithinDetectionRangeCondition>(notInRange.Condition);

            // 5. Hunting
            AIStateDefinition hunting = brain.States[4];
            Assert.AreEqual("Hunting", hunting.StateId);
            Assert.AreEqual(1, hunting.Behaviors.Count);
            var huntMove = hunting.Behaviors[0] as MoveTowardTargetBehavior;
            Assert.IsNotNull(huntMove);
            Assert.AreEqual(1.6f, huntMove.SpeedMultiplier, 0.0001f);
            Assert.AreEqual(0f, huntMove.StopDistance, 0.0001f);
            Assert.AreEqual(1, hunting.Transitions.Count);
            Assert.AreEqual("Combat", hunting.Transitions[0].TargetStateId);
            Assert.IsInstanceOf<TargetEnteredZoneCondition>(hunting.Transitions[0].Condition);
        }

        [Test]
        public void Enemy1Prefab_RootComponentsChildrenAndSerializedReferencesAreValid()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Enemy1PrefabPath);
            Assert.IsNotNull(prefab, $"Expected prefab at '{Enemy1PrefabPath}'.");

            var expectedData = AssetDatabase.LoadAssetAtPath<EnemyDataScriptableObject>(Enemy1DataPath);
            var expectedBrain = AssetDatabase.LoadAssetAtPath<BrainProfileScriptableObject>(Enemy1BrainPath);

            Assert.AreEqual("enemy1", prefab.name);
            Assert.AreEqual(EnemyLayer, prefab.layer);
            Assert.IsTrue(prefab.CompareTag("Enemy"));

            var spriteRenderer = prefab.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(spriteRenderer);
            Assert.IsNotNull(spriteRenderer.sprite);
            Assert.AreEqual("Stage", spriteRenderer.sortingLayerName);

            var rb = prefab.GetComponent<Rigidbody2D>();
            Assert.IsNotNull(rb);
            Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType);
            Assert.AreEqual(RigidbodyConstraints2D.FreezeRotation, rb.constraints);
            Assert.AreEqual(1f, rb.gravityScale, 0.0001f);

            var rootCollider = prefab.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(rootCollider);
            Assert.IsFalse(rootCollider.isTrigger);
            Assert.AreEqual(new Vector2(1f, 1f), rootCollider.size);

            Transform hitboxTransform = prefab.transform.Find("Hitbox");
            Assert.IsNotNull(hitboxTransform, "Expected child 'Hitbox'.");
            Assert.AreEqual(EnemyLayer, hitboxTransform.gameObject.layer);
            var hitboxCollider = hitboxTransform.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(hitboxCollider);
            Assert.IsTrue(hitboxCollider.isTrigger);
            Assert.AreEqual(new Vector2(1.05f, 1.05f), hitboxCollider.size);

            Transform detectionTransform = prefab.transform.Find("DetectionZone");
            Assert.IsNotNull(detectionTransform, "Expected child 'DetectionZone'.");
            Assert.AreEqual(SensorLayer, detectionTransform.gameObject.layer);
            var detectionCollider = detectionTransform.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(detectionCollider);
            Assert.IsTrue(detectionCollider.isTrigger);
            Assert.AreEqual(new Vector2(10f, 4f), detectionCollider.size);
            Assert.AreEqual(0, detectionTransform.GetComponents<MonoBehaviour>().Length);
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(SensorLayer, EnemyLayer));

            var composer = prefab.GetComponent<EnemyComposer>();
            Assert.IsNotNull(composer);
            Assert.AreSame(expectedData, GetPrivateField<EnemyDataScriptableObject>(composer, "m_enemyData"));
            Assert.AreSame(expectedBrain, GetPrivateField<BrainProfileScriptableObject>(composer, "m_brainProfile"));
            Assert.AreSame(detectionCollider, GetPrivateField<BoxCollider2D>(composer, "m_detectionCollider"));
            Assert.AreSame(spriteRenderer, GetPrivateField<SpriteRenderer>(composer, "m_spriteRenderer"));
        }

        [Test]
        public void Enemy1Prefab_AwakeAndOnEnable_InitializesBrainInIdleState()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Enemy1PrefabPath);
            Assert.IsNotNull(prefab);

            var expectedData = AssetDatabase.LoadAssetAtPath<EnemyDataScriptableObject>(Enemy1DataPath);
            GameObject instance = Object.Instantiate(prefab);
            Object clonedData = null;

            try
            {
                var composer = instance.GetComponent<EnemyComposer>();
                Assert.IsNotNull(composer);

                InvokePrivateMethod(composer, "Awake");
                clonedData = GetPrivateField<Object>(composer, "m_enemyData");
                InvokePrivateMethod(composer, "OnEnable");

                Assert.AreEqual("Idle", composer.CurrentBrainStateId);

                InvokePrivateMethod(composer, "OnDisable");
            }
            finally
            {
                if (clonedData != null && clonedData != expectedData)
                {
                    Object.DestroyImmediate(clonedData);
                }

                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }
    }
}
