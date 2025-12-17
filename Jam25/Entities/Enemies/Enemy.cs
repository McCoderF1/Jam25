using HDT.Gaming.Models;
using HDT.Gaming.Physics;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jam25.Entities.Enemies
{
    public class Enemy
    {
        #region Private Members

        private EnemyState currentState = EnemyState.Idle;
        private float attackBlockedUntil;

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


        public bool CanAttack => (CurrentState == EnemyState.Idle || CurrentState == EnemyState.Running) && attackBlockedUntil <= 0f;

        public Vector2 MovementDirection { get; set; } = Vector2.Zero;

        public Enemy()
        {
            Body.Owner = this;
            Health = new Health(50);
            //StartAttackCooldown();
        }

        public void TakeDamage(int amount)
        {
            Health.TakeDamage(amount);
            if (Health.Current == 0)
            {
                CurrentState = EnemyState.Dying;
            }
            else
            {
                CurrentState = EnemyState.Hurt;
            }
        }

        public void StartCooldown()
        {
            attackBlockedUntil = 1000f;
        }

        /// <summary>
        /// Updates the enemy's state based on the current game time.
        /// </summary>
        /// <remarks>If the enemy is in the dying state and its dying animation has completed, this method
        /// transitions the enemy to the dead state.</remarks>
        /// <param name="gameTime">The current game time, used to determine state transitions and animation progress.</param>
        public void Update(GameTime gameTime)
        {
            // When the current animation loop is completed
            if (!CurrentSprite.LoopCompleted)
            {
                return;
            }

            if(attackBlockedUntil > 0f)
            {
                attackBlockedUntil -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            switch (CurrentState)
            {
                case EnemyState.Hurt:
                case EnemyState.Attacking:
                    CurrentState = EnemyState.Idle;
                    break;
                case EnemyState.Dying:
                    CurrentState = EnemyState.Dead;
                    break;
            }
        }
    }
}
