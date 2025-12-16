using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HDT.Gaming.Models;
using HDT.Gaming.Physics;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;

namespace Jam25.Entities.Enemies
{
    public class Enemy
    {
        #region Private Members

        private EnemyState currentState = EnemyState.Idle;

        #endregion Private Members

        public enum EnemyState
        {
            Idle,
            Running,
            Attacking,
            Hurt,
            Dying
        }
        public EnemyState CurrentState { get { return currentState; } set { currentState = value; } }

        public AnimatedDirectionalSprite CurrentSprite => Sprites[currentState];

        public Body Body { get; } = new Body();

        public Dictionary<EnemyState, AnimatedDirectionalSprite> Sprites { get; init; }

        public int MovementSpeed { get; init; }

        public IEnemyController EnemyController { get; init; }

        public Health Health { get; init; }


        public bool CanAttack { get; private set; }

        public Enemy()
        {
            Body.Owner = this;
            CanAttack = true;
            //StartAttackCooldown();
        }

        public void TakeDamage(int amount)
        {
            Health.TakeDamage(amount);
            if (Health.Current == 0)
            {
                CurrentState = EnemyState.Dying;
            }
        }

        public async Task StartAttackCooldown()
        {
            if (!CanAttack)
            {
                return;
            }
            CanAttack = false;
            await Task.Delay(5000);
            CanAttack = true;
        }
    }
}
