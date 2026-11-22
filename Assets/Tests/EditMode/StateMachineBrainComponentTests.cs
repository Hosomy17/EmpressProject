using System.Collections.Generic;
using Com.Voobox.Project.AI;
using Com.Voobox.Project.Data;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class StateMachineBrainComponentTests
    {
        private readonly List<Object> m_createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = m_createdObjects.Count - 1; i >= 0; i--)
            {
                if (m_createdObjects[i] != null)
                {
                    Object.DestroyImmediate(m_createdObjects[i]);
                }
            }

            m_createdObjects.Clear();
        }

        private GameObject CreateTrackedGameObject(string name, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            m_createdObjects.Add(go);
            return go;
        }

        private BrainProfileScriptableObject CreateProfile(
            string initialStateId,
            List<AIStateDefinition> states,
            List<GlobalTransitionDefinition> globalTransitions = null)
        {
            var profile = ScriptableObject.CreateInstance<BrainProfileScriptableObject>();
            profile.Configure(initialStateId, states, globalTransitions);
            m_createdObjects.Add(profile);
            return profile;
        }

        private BrainProfileScriptableObject CreateEnemy1Profile(bool useGlobalExitTransition = false)
        {
            var detectionRangeCondition = new TargetWithinDetectionRangeCondition();

            var idleTransitions = new List<AITransitionDefinition>
            {
                new("Combat", new TargetEnteredZoneCondition())
            };

            var combatTransitions = new List<AITransitionDefinition>
            {
                new("LungeAttack", new AllOfCondition(
                    new TargetWithinDistanceCondition(3f),
                    new BodyFlagCondition(BodyFlag.CanDash, true)))
            };

            if (!useGlobalExitTransition)
            {
                combatTransitions.Add(new AITransitionDefinition("Hunting", new TargetExitedZoneCondition()));
            }

            var huntingTransitions = new List<AITransitionDefinition>
            {
                new("Combat", new TargetEnteredZoneCondition()),
                new("Idle", new StateElapsedCondition(3f))
            };

            var lungeTransitions = new List<AITransitionDefinition>
            {
                new("Waiting", new AllOfCondition(
                    new StateElapsedCondition(0.2f),
                    new BodyFlagCondition(BodyFlag.IsDashing, false)))
            };

            var waitingTransitions = new List<AITransitionDefinition>
            {
                new("Combat", new AllOfCondition(
                    new StateElapsedCondition(1f),
                    detectionRangeCondition)),
                new("Idle", new AllOfCondition(
                    new StateElapsedCondition(1f),
                    new NotCondition(detectionRangeCondition)))
            };

            var states = new List<AIStateDefinition>
            {
                new("Idle", new List<IAIBehavior> { new IdleBehavior() }, idleTransitions),
                new("Combat", new List<IAIBehavior> { new MoveTowardTargetBehavior(1f, 0.5f) }, combatTransitions),
                new("Hunting", new List<IAIBehavior> { new MoveTowardTargetBehavior(1.6f, 0.25f) }, huntingTransitions),
                new("LungeAttack", new List<IAIBehavior> { new RequestDashBehavior() }, lungeTransitions),
                new("Waiting", new List<IAIBehavior> { new IdleBehavior() }, waitingTransitions)
            };

            List<GlobalTransitionDefinition> globalTransitions = null;
            if (useGlobalExitTransition)
            {
                globalTransitions = new List<GlobalTransitionDefinition>
                {
                    new("Hunting", new TargetExitedZoneCondition(), new List<string> { "Idle", "Hunting", "LungeAttack", "Waiting" })
                };
            }

            return CreateProfile("Idle", states, globalTransitions);
        }

        [Test]
        public void Enemy1Loop_TransitionsThroughIdleCombatLungeWaitingAndDetectionRangeBranch()
        {
            BrainProfileScriptableObject profile = CreateEnemy1Profile();
            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;

            GameObject hero = CreateTrackedGameObject("Hero", new Vector2(8f, 0f));
            var body = new EnemyBodyState
            {
                Position = Vector2.zero,
                CanDash = true,
                IsGrounded = true,
                HasDetectedTarget = true,
                DetectedTargetPosition = new Vector2(8f, 0f),
                DetectedTargetDistance = 8f,
                DetectionRange = 6f
            };

            // 1. Starts in Idle with zero movement
            Assert.AreEqual("Idle", brain.CurrentStateId);
            EnemyIntent idleIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Idle", brain.CurrentStateId);
            Assert.AreEqual(Vector2.zero, idleIntent.MoveAxis);

            // 2. Enter zone -> Combat at 1.0x speed
            brain.NotifyTargetEntered(hero.transform);
            EnemyIntent combatIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Combat", brain.CurrentStateId);
            Assert.AreEqual(1f, combatIntent.MoveAxis.x, 0.0001f);
            Assert.AreEqual(1f, combatIntent.SpeedMultiplier, 0.0001f);

            // 3. Move within lunge range -> LungeAttack (requests dash toward target)
            hero.transform.position = new Vector2(2.5f, 0f);
            body.DetectedTargetPosition = new Vector2(2.5f, 0f);
            body.DetectedTargetDistance = 2.5f;
            EnemyIntent lungeIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("LungeAttack", brain.CurrentStateId);
            Assert.IsTrue(lungeIntent.DashPressed);
            Assert.AreEqual(1f, lungeIntent.MoveAxis.x, 0.0001f);

            // 4. While dashing / timer < 0.2s -> stays in LungeAttack
            body.IsDashing = true;
            body.CanDash = false;
            EnemyIntent midLungeIntent = controller.SampleIntent(in body, 0.1f);
            Assert.AreEqual("LungeAttack", brain.CurrentStateId);
            Assert.IsFalse(midLungeIntent.DashPressed);

            // 5. Dash ends and >= 0.2s elapsed -> Waiting
            body.IsDashing = false;
            controller.SampleIntent(in body, 0.15f);
            Assert.AreEqual("Waiting", brain.CurrentStateId);

            // 6. Wait expires with HasDetectedTarget && DetectedTargetDistance <= DetectionRange -> Combat
            controller.SampleIntent(in body, 0.5f);
            Assert.AreEqual("Waiting", brain.CurrentStateId);
            controller.SampleIntent(in body, 0.5f);
            Assert.AreEqual("Combat", brain.CurrentStateId);

            // 7. Re-enter LungeAttack -> Waiting, then Wait expires with HasDetectedTarget == false -> Idle
            body.CanDash = true;
            controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("LungeAttack", brain.CurrentStateId);

            controller.SampleIntent(in body, 0.25f);
            Assert.AreEqual("Waiting", brain.CurrentStateId);

            body.HasDetectedTarget = false;
            controller.SampleIntent(in body, 1.0f);
            Assert.AreEqual("Idle", brain.CurrentStateId);
        }

        [Test]
        public void Combat_OnTriggerExit_TransitionsToHunting_AndContinuesTrackingFleeingTargetWithoutDetection()
        {
            BrainProfileScriptableObject profile = CreateEnemy1Profile();
            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;
            GameObject hero = CreateTrackedGameObject("Hero", new Vector2(6f, 0f));
            var body = new EnemyBodyState
            {
                Position = Vector2.zero,
                CanDash = false,
                HasDetectedTarget = false
            };

            brain.NotifyTargetEntered(hero.transform);
            controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Combat", brain.CurrentStateId);

            // Hero moves out of zone at x = 10
            hero.transform.position = new Vector2(10f, 0f);
            brain.NotifyTargetExited(hero.transform);

            EnemyIntent huntIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Hunting", brain.CurrentStateId);
            Assert.AreEqual(1.6f, huntIntent.SpeedMultiplier, 0.0001f);
            Assert.AreEqual(1f, huntIntent.MoveAxis.x, 0.0001f);

            // Hero keeps fleeing to x = 15 while enemy moves past x = 10 to x = 11;
            // MoveTowardTargetBehavior must still track the fleeing Target even with HasDetectedTarget == false
            hero.transform.position = new Vector2(15f, 0f);
            body.Position = new Vector2(11f, 0f);
            body.HasDetectedTarget = false;
            EnemyIntent huntTickIntent = controller.SampleIntent(in body, 0.1f);
            Assert.AreEqual("Hunting", brain.CurrentStateId);
            Assert.AreEqual(1.6f, huntTickIntent.SpeedMultiplier, 0.0001f);
            Assert.AreEqual(1f, huntTickIntent.MoveAxis.x, 0.0001f);

            // Hero re-enters zone -> returns to Combat
            hero.transform.position = new Vector2(13f, 0f);
            brain.NotifyTargetEntered(hero.transform);
            EnemyIntent reCombatIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Combat", brain.CurrentStateId);
            Assert.AreEqual(1f, reCombatIntent.SpeedMultiplier, 0.0001f);
        }

        [Test]
        public void OnTriggerExit_IsIgnoredDuringLungeAttackAndWaiting_IncludingGlobalTransitionExclusions()
        {
            BrainProfileScriptableObject profile = CreateEnemy1Profile(useGlobalExitTransition: true);
            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;
            GameObject hero = CreateTrackedGameObject("Hero", new Vector2(2f, 0f));
            var body = new EnemyBodyState
            {
                Position = Vector2.zero,
                CanDash = true,
                HasDetectedTarget = true,
                DetectedTargetPosition = new Vector2(2f, 0f),
                DetectedTargetDistance = 2f,
                DetectionRange = 6f
            };

            brain.NotifyTargetEntered(hero.transform);
            controller.SampleIntent(in body, 0.05f); // Idle -> Combat
            controller.SampleIntent(in body, 0.05f); // Combat -> LungeAttack
            Assert.AreEqual("LungeAttack", brain.CurrentStateId);

            // Exit during LungeAttack is ignored
            brain.NotifyTargetExited(hero.transform);
            controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("LungeAttack", brain.CurrentStateId);

            // Advance to Waiting
            controller.SampleIntent(in body, 0.2f);
            Assert.AreEqual("Waiting", brain.CurrentStateId);

            // Exit during Waiting is ignored
            brain.NotifyTargetExited(hero.transform);
            controller.SampleIntent(in body, 0.25f);
            Assert.AreEqual("Waiting", brain.CurrentStateId);

            // Verify global exit transition still fires from Combat
            controller.SampleIntent(in body, 0.8f); // Waiting -> Combat
            Assert.AreEqual("Combat", brain.CurrentStateId);

            body.CanDash = false;
            brain.NotifyTargetExited(hero.transform);
            controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Hunting", brain.CurrentStateId);
        }

        [Test]
        public void Waiting_VisibilityDebuffShrinksDetectionRangeBelowDistance_TransitionsToIdle()
        {
            BrainProfileScriptableObject profile = CreateEnemy1Profile();
            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;
            GameObject hero = CreateTrackedGameObject("Hero", new Vector2(4f, 0f));

            var body = new EnemyBodyState
            {
                Position = Vector2.zero,
                CanDash = true,
                HasDetectedTarget = true,
                DetectedTargetPosition = new Vector2(2f, 0f),
                DetectedTargetDistance = 2f,
                DetectionRange = 6f
            };

            brain.NotifyTargetEntered(hero.transform);
            controller.SampleIntent(in body, 0.05f); // Idle -> Combat
            controller.SampleIntent(in body, 0.05f); // Combat -> LungeAttack
            controller.SampleIntent(in body, 0.25f); // LungeAttack -> Waiting
            Assert.AreEqual("Waiting", brain.CurrentStateId);

            // Visibility debuff shrinks DetectionRange below DetectedTargetDistance while target is still detected
            body.DetectedTargetPosition = new Vector2(4f, 0f);
            body.DetectedTargetDistance = 4f;
            body.DetectionRange = 3f;

            controller.SampleIntent(in body, 1.0f);
            Assert.AreEqual("Idle", brain.CurrentStateId);
        }

        [Test]
        public void DecoupledPerception_BodyDetectedTargetDrivesCombatMovement_WithoutTransformOrPhysics2D()
        {
            BrainProfileScriptableObject profile = CreateEnemy1Profile();
            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;

            brain.NotifyTargetEntered(null);
            var body = new EnemyBodyState
            {
                Position = new Vector2(2f, 0f),
                CanDash = false,
                HasDetectedTarget = true,
                DetectedTargetPosition = new Vector2(-4f, 0f),
                DetectedTargetDistance = 6f,
                DetectionRange = 10f
            };

            EnemyIntent combatLeftIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Combat", brain.CurrentStateId);
            Assert.IsNull(brain.Context.Target);
            Assert.IsTrue(brain.Context.HasTarget);
            Assert.AreEqual(new Vector2(-4f, 0f), brain.Context.TargetPosition);
            Assert.AreEqual(6f, brain.Context.DistanceToTarget, 0.0001f);
            Assert.AreEqual(-1f, combatLeftIntent.MoveAxis.x, 0.0001f);
            Assert.AreEqual(1f, combatLeftIntent.SpeedMultiplier, 0.0001f);

            body.DetectedTargetPosition = new Vector2(8f, 0f);
            body.DetectedTargetDistance = 6f;
            EnemyIntent combatRightIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Combat", brain.CurrentStateId);
            Assert.AreEqual(1f, combatRightIntent.MoveAxis.x, 0.0001f);

            body.HasDetectedTarget = false;
            brain.ClearTarget();
            controller.SampleIntent(in body, 0.05f);
            Assert.IsFalse(brain.Context.HasTarget);
            Assert.IsFalse(brain.Context.IsTargetInZone);
        }

        [Test]
        public void Composability_SupportsTwoDistinctHuntingStates_WithoutInheritance()
        {
            var profile = CreateProfile(
                "Idle",
                new List<AIStateDefinition>
                {
                    new("Idle", new List<IAIBehavior> { new IdleBehavior() }, new List<AITransitionDefinition>
                    {
                        new("FastHunt", new TargetEnteredZoneCondition())
                    }),
                    new("FastHunt", new List<IAIBehavior> { new MoveTowardTargetBehavior(2.0f, 0f) }, new List<AITransitionDefinition>
                    {
                        new("CautiousHunt", new StateElapsedCondition(0.5f))
                    }),
                    new("CautiousHunt", new List<IAIBehavior> { new MoveToLastKnownPositionBehavior(0.6f, 0.1f) })
                });

            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;
            var body = new EnemyBodyState { Position = Vector2.zero };
            brain.Context.SetTargetPosition(new Vector2(10f, 0f));
            brain.NotifyTargetEntered(null);

            EnemyIntent fastIntent = controller.SampleIntent(in body, 0.1f);
            Assert.AreEqual("FastHunt", brain.CurrentStateId);
            Assert.AreEqual(2.0f, fastIntent.SpeedMultiplier, 0.0001f);
            Assert.AreEqual(1f, fastIntent.MoveAxis.x, 0.0001f);

            EnemyIntent cautiousIntent = controller.SampleIntent(in body, 0.5f);
            Assert.AreEqual("CautiousHunt", brain.CurrentStateId);
            Assert.AreEqual(0.6f, cautiousIntent.SpeedMultiplier, 0.0001f);
            Assert.AreEqual(1f, cautiousIntent.MoveAxis.x, 0.0001f);
        }

        [Test]
        public void Composability_EnemyWithoutCombatState_TransitionsDirectlyIdleToHuntingToIdle()
        {
            var profile = CreateProfile(
                "Idle",
                new List<AIStateDefinition>
                {
                    new("Idle", new List<IAIBehavior> { new IdleBehavior() }, new List<AITransitionDefinition>
                    {
                        new("Hunting", new TargetEnteredZoneCondition())
                    }),
                    new("Hunting", new List<IAIBehavior> { new MoveToLastKnownPositionBehavior(1.25f, 0.5f) }, new List<AITransitionDefinition>
                    {
                        new("Idle", new TargetExitedZoneCondition())
                    })
                });

            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;
            var body = new EnemyBodyState { Position = Vector2.zero };
            brain.Context.SetTargetPosition(new Vector2(-5f, 0f));
            brain.NotifyTargetEntered(null);

            EnemyIntent huntIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Hunting", brain.CurrentStateId);
            Assert.AreEqual(-1f, huntIntent.MoveAxis.x, 0.0001f);
            Assert.AreEqual(1.25f, huntIntent.SpeedMultiplier, 0.0001f);

            brain.NotifyTargetExited(null);
            EnemyIntent backIdleIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Idle", brain.CurrentStateId);
            Assert.AreEqual(Vector2.zero, backIdleIntent.MoveAxis);
        }

        [Test]
        public void Composability_StationaryEnemy_FacesTargetWithoutMovementOrAttack()
        {
            var profile = CreateProfile(
                "Idle",
                new List<AIStateDefinition>
                {
                    new("Idle", new List<IAIBehavior> { new IdleBehavior() }, new List<AITransitionDefinition>
                    {
                        new("Watch", new AnyOfCondition(
                            new TargetEnteredZoneCondition(),
                            new TargetWithinDistanceCondition(5f)))
                    }),
                    new("Watch", new List<IAIBehavior> { new FaceTargetBehavior() }, new List<AITransitionDefinition>
                    {
                        new("Idle", new TargetExitedZoneCondition())
                    })
                });

            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;
            var body = new EnemyBodyState { Position = new Vector2(4f, 0f), CanAttack = true, CanDash = true };
            brain.Context.SetTargetPosition(new Vector2(1f, 0f));

            EnemyIntent watchIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Watch", brain.CurrentStateId);
            Assert.AreEqual(-1f, watchIntent.MoveAxis.x, 0.0001f);
            Assert.AreEqual(0f, watchIntent.SpeedMultiplier, 0.0001f);
            Assert.IsFalse(watchIntent.AttackPressed);
            Assert.IsFalse(watchIntent.DashPressed);
        }

        [Test]
        public void RequestAttackBehavior_AndDisableEnable_RespectBodyFlagsAndControllerLifecycle()
        {
            var profile = CreateProfile(
                "Idle",
                new List<AIStateDefinition>
                {
                    new("Idle", new List<IAIBehavior> { new IdleBehavior() }, new List<AITransitionDefinition>
                    {
                        new("Strike", new BodyFlagCondition(BodyFlag.CanAttack, true))
                    }),
                    new("Strike", new List<IAIBehavior> { new RequestAttackBehavior() })
                });

            var brain = new StateMachineBrainComponent(profile);
            IEnemyController controller = brain;
            var body = new EnemyBodyState { CanAttack = false };

            controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Idle", brain.CurrentStateId);

            body.CanAttack = true;
            EnemyIntent attackIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Strike", brain.CurrentStateId);
            Assert.IsTrue(attackIntent.AttackPressed);

            // Tick in Strike does not re-trigger AttackPressed
            EnemyIntent subsequentIntent = controller.SampleIntent(in body, 0.05f);
            Assert.IsFalse(subsequentIntent.AttackPressed);

            // Disable returns default intent; Enable resumes
            controller.Disable();
            EnemyIntent disabledIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual(EnemyIntent.Default, disabledIntent);

            controller.Enable();
            controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Strike", brain.CurrentStateId);
        }

        [Test]
        public void HeroBrainController_MapsHeroBodyStateToHeroIntent_IncludingAttackDashAndMoveFields()
        {
            var profile = CreateProfile(
                "Idle",
                new List<AIStateDefinition>
                {
                    new("Idle", new List<IAIBehavior> { new IdleBehavior() }, new List<AITransitionDefinition>
                    {
                        new("Chase", new AllOfCondition(
                            new BodyFlagCondition(BodyFlag.IsGrounded, true),
                            new TargetWithinDetectionRangeCondition()))
                    }),
                    new("Chase", new List<IAIBehavior>
                    {
                        new MoveTowardTargetBehavior(1.5f, 0.5f)
                    }, new List<AITransitionDefinition>
                    {
                        new("BurstStrike", new AllOfCondition(
                            new BodyFlagCondition(BodyFlag.CanAttack, true),
                            new BodyFlagCondition(BodyFlag.CanDash, true)))
                    }),
                    new("BurstStrike", new List<IAIBehavior>
                    {
                        new RequestAttackBehavior(),
                        new RequestDashBehavior()
                    })
                });

            var brain = new HeroBrainController(profile);
            IHeroController controller = brain;

            var body = new HeroBodyState
            {
                Position = new Vector2(1f, 0f),
                IsGrounded = true,
                IsAttacking = false,
                IsDashing = false,
                CanAttack = false,
                CanDash = false,
                IsDead = false,
                HealthNormalized = 0.75f,
                HasDetectedTarget = true,
                DetectedTargetPosition = new Vector2(6f, 0f),
                DetectedTargetDistance = 5f,
                DetectionRange = 8f
            };

            // 1. Transitions Idle -> Chase; maps MoveAxis and SpeedMultiplier, leaves Jump/Attack/Dash false
            HeroIntent chaseIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("Chase", brain.CurrentStateId);
            Assert.AreEqual(1f, chaseIntent.MoveAxis.x, 0.0001f);
            Assert.AreEqual(1.5f, chaseIntent.SpeedMultiplier, 0.0001f);
            Assert.IsFalse(chaseIntent.JumpPressed);
            Assert.IsFalse(chaseIntent.JumpReleased);
            Assert.IsFalse(chaseIntent.AttackPressed);
            Assert.IsFalse(chaseIntent.DashPressed);
            Assert.AreEqual(0.75f, brain.Context.Body.HealthNormalized, 0.0001f);
            Assert.AreEqual(8f, brain.Context.Body.DetectionRange, 0.0001f);

            // 2. CanAttack & CanDash -> BurstStrike; maps AttackPressed, DashPressed, and MoveAxis
            body.CanAttack = true;
            body.CanDash = true;
            HeroIntent strikeIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("BurstStrike", brain.CurrentStateId);
            Assert.IsTrue(strikeIntent.AttackPressed);
            Assert.IsTrue(strikeIntent.DashPressed);
            Assert.IsFalse(strikeIntent.JumpPressed);
            Assert.IsFalse(strikeIntent.JumpReleased);
            Assert.AreEqual(1f, strikeIntent.MoveAxis.x, 0.0001f);

            // 3. Disable returns HeroIntent.Default; Enable resumes
            controller.Disable();
            HeroIntent disabledIntent = controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual(HeroIntent.Default, disabledIntent);

            controller.Enable();
            controller.SampleIntent(in body, 0.05f);
            Assert.AreEqual("BurstStrike", brain.CurrentStateId);
        }
    }
}
