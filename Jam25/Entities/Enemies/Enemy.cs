using HDT.Gaming.Audio;
using HDT.Gaming.Models;
using HDT.Gaming.Physics;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;
using Jam25.Stores;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Jam25.Entities.Enemies
{
    public class Enemy
    {
        #region Private Members

        private const int DEFAULT_SIGHT_RANGE = 200;
        private static readonly TimeSpan DefaultChaseMemoryDuration = TimeSpan.FromSeconds(2);

        private EnemyState currentState = EnemyState.Idle;
        private float playerSightMemoryTimerMs = 0f;
        private float attackBlockedUntil;
        private readonly float attackCooldown = 2000f;
        private float moveBlockedUntil;
        private readonly float stunCooldown = 3000f;

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


        public bool CanMove => (CurrentState != EnemyState.Dead) && (CurrentState != EnemyState.Dying) && (moveBlockedUntil <= 0f);
        public bool CanAttack => (CurrentState == EnemyState.Idle || CurrentState == EnemyState.Running) && attackBlockedUntil <= 0f;

        public Vector2 MovementDirection { get; set; } = Vector2.Zero;

        public int SightRange { get; set; } = DEFAULT_SIGHT_RANGE;

        public TimeSpan ChaseMemoryDuration { get; set; } = DefaultChaseMemoryDuration;

        public bool HasRecentPlayerSighting => playerSightMemoryTimerMs > 0f;

        public int AttackRange { get; set; }

        public bool UseProjectiles { get; set; }
        public List<Projectile> Projectiles { get; } = [];
        public Texture2D ProjectileTexture { get; set; }


        public Enemy()
        {
            Body.Owner = this;
            Health = new Health(50);
            //StartAttackCooldown();
        }

        public void TakeDamage(int amount)
        {
            if (Player.DebugInvincibleMode)
            {
                Health.TakeDamage(Health.Current);
                CurrentState = EnemyState.Dying;
                PlayerTracker.RecordKill();
                return;
            }

            Health.TakeDamage(amount);
            if (Health.Current <= 0)
            {
                CurrentState = EnemyState.Dying;
                PlayerTracker.RecordKill();
            }
            else
            {
                StartStun();
                CurrentState = EnemyState.Hurt;
                PlayerTracker.CollectEmber();
            }
        }

        public void StartCooldown()
        {
            attackBlockedUntil = attackCooldown;
        }

        public void StartStun()
        {
            moveBlockedUntil = stunCooldown;
        }

        public void Attack(Player player)
        {
            if (UseProjectiles)
            {
                StartCooldown();
                Projectiles.Add(new Projectile()
                {
                    Position = Body.Position,
                    Direction = Math.Atan2(player.Body.Position.Y - Body.Position.Y, player.Body.Position.X - Body.Position.X),
                    Velocity = 300,
                    Texture = ProjectileTexture,
                    Damage = 5,
                    Lifespan = 2000
                });
                CurrentState = Enemy.EnemyState.Attacking;
            }
            else
            {
                StartCooldown();
                player.TakeDamage(10);
                CurrentState = Enemy.EnemyState.Attacking;
                AudioManager.PlaySound("MetalHit");
            }
        }


        public void RefreshPlayerSighting()
        {
            playerSightMemoryTimerMs = (float)ChaseMemoryDuration.TotalMilliseconds;
        }

        /// <summary>
        /// Updates the enemy's state based on the current game time.
        /// </summary>
        /// <remarks>If the enemy is in the dying state and its dying animation has completed, this method
        /// transitions the enemy to the dead state.</remarks>
        /// <param name="gameTime">The current game time, used to determine state transitions and animation progress.</param>
        public void Update(GameTime gameTime)
        {
            UpdatePlayerSightingTimer(gameTime);
            if (attackBlockedUntil > 0f)
            {
                attackBlockedUntil -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (moveBlockedUntil > 0f)
            {
                moveBlockedUntil -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            // When the current animation loop is completed
            if (!CurrentSprite.LoopCompleted)
            {
                return;
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

        private void UpdatePlayerSightingTimer(GameTime gameTime)
        {
            if (playerSightMemoryTimerMs <= 0f)
            {
                return;
            }

            playerSightMemoryTimerMs -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (playerSightMemoryTimerMs < 0f)
            {
                playerSightMemoryTimerMs = 0f;
            }
        }
    }
}
