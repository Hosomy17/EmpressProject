using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class EnemyPhysicsLayersTests
    {
        private const int HERO_LAYER = 3;
        private const int GROUND_LAYER = 6;
        private const int ENEMY_LAYER = 7;
        private const int ATTACK_AREA_LAYER = 8;
        private const int SENSOR_LAYER = 9;

        [Test]
        public void SensorLayer_IsConfiguredAtIndex9()
        {
            Assert.AreEqual(SENSOR_LAYER, LayerMask.NameToLayer("Sensor"));
        }

        [Test]
        public void Physics2DLayers_SensorCollisionMatrix_IsConfigured()
        {
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(SENSOR_LAYER, HERO_LAYER), "Sensor (9) should collide with Hero (3).");
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(SENSOR_LAYER, ENEMY_LAYER), "Sensor (9) should collide with Enemy (7).");
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(SENSOR_LAYER, SENSOR_LAYER), "Sensor (9) should ignore Sensor (9).");
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(SENSOR_LAYER, ATTACK_AREA_LAYER), "Sensor (9) should ignore AttackArea (8).");
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(SENSOR_LAYER, GROUND_LAYER), "Sensor (9) should ignore Ground (6).");
        }

        [Test]
        public void Physics2DLayers_EnemyCollidesWithGround()
        {
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(ENEMY_LAYER, GROUND_LAYER), "Enemy (7) should collide with Ground (6).");
        }

        [Test]
        public void Physics2DLayers_HeroIgnoresEnemy_AndCombatLayersCollide()
        {
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(HERO_LAYER, ENEMY_LAYER), "Hero (3) should ignore Enemy (7).");
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(HERO_LAYER, GROUND_LAYER), "Hero (3) should collide with Ground (6).");
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(ATTACK_AREA_LAYER, ENEMY_LAYER), "AttackArea (8) should collide with Enemy (7).");
        }
    }
}
