using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HDT.Gaming.Models;
using HDT.Gaming.Physics;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;
using Microsoft.Xna.Framework;

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
            Dying,
            Dead
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

        /// <summary>
        /// Updates the enemy's state based on the current game time.
        /// </summary>
        /// <remarks>If the enemy is in the dying state and its dying animation has completed, this method
        /// transitions the enemy to the dead state.</remarks>
        /// <param name="gameTime">The current game time, used to determine state transitions and animation progress.</param>
        public void Update(GameTime gameTime)
        {
            // If we are dying and the dying animation has completed, set state to Dead
            if (CurrentState == EnemyState.Dying && CurrentSprite.LoopCompleted)
            {
                CurrentState = EnemyState.Dead;
            }
        }
    }
}
