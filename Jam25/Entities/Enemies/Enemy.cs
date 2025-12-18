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
        private float attackWindUpTimer = 0f;
        private bool isWindingUp = false;
        private Player windUpTarget = null;

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


        public bool CanMove => (CurrentState != EnemyState.Dead) 
            && (CurrentState != EnemyState.Dying) 
            && (CurrentState != EnemyState.Attacking || !StopsToAttack)
            && !isWindingUp
            && (moveBlockedUntil <= 0f);
        public bool CanAttack => (CurrentState == EnemyState.Idle || CurrentState == EnemyState.Running) 
            && attackBlockedUntil <= 0f
            && !isWindingUp;

        public bool IsWindingUp => isWindingUp;

        public Vector2 MovementDirection { get; set; } = Vector2.Zero;

        public int SightRange { get; set; } = DEFAULT_SIGHT_RANGE;

        public TimeSpan ChaseMemoryDuration { get; set; } = DefaultChaseMemoryDuration;

        public bool HasRecentPlayerSighting => playerSightMemoryTimerMs > 0f;

        public int AttackRange { get; set; }

        public bool UseProjectiles { get; set; }
        public List<Projectile> Projectiles { get; } = [];
        public Texture2D ProjectileTexture { get; set; }
        public List<Texture2D> ExplosionTextures { get; set; }

        public bool StopsToAttack { get; set; } = false;

        public float AttackWindUpMs { get; set; } = 0f;

        public int AttackDamage { get; set; } = 10;


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
                PlayerTracker.CollectEmber();
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
                // Shoot projectile
                StartCooldown();
                Projectiles.Add(new Projectile()
                {
                    Position = Body.Position,
                    Direction = Math.Atan2(player.Body.Position.Y - Body.Position.Y, player.Body.Position.X - Body.Position.X),
                    Velocity = 300,
                    Texture = ProjectileTexture,
                    ExplosionTextures = this.ExplosionTextures,
                    Damage = 5,
                    Lifespan = 1000  // ms before removed
                });
                CurrentState = Enemy.EnemyState.Attacking;
            }
            else
            {
                // Meele attack the player
                if (AttackWindUpMs > 0f && !isWindingUp)
                {
                    isWindingUp = true;
                    attackWindUpTimer = AttackWindUpMs;
                    windUpTarget = player;
                    return;
                }

                StartCooldown();
                CurrentState = Enemy.EnemyState.Attacking;
                CurrentSprite.Reset();
                player.TakeDamage(AttackDamage);
            }
        }

        /// <summary>
        /// Executes the actual attack after windup completes
        /// </summary>
        private void ExecuteAttack()
        {
            if (windUpTarget == null) return;

            StartCooldown();
            CurrentState = Enemy.EnemyState.Attacking;
            CurrentSprite.Reset();
            
            float distFromPlayer = Vector2.Distance(Body.Position, windUpTarget.Body.Position);
            if (distFromPlayer < AttackRange + 10)
            {
                windUpTarget.TakeDamage(AttackDamage);
            }
            else
            {
                AudioManager.PlaySound("Miss");
            }

            windUpTarget = null;
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

            if (isWindingUp)
            {
                attackWindUpTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (attackWindUpTimer <= 0f)
                {
                    isWindingUp = false;
                    attackWindUpTimer = 0f;
                    ExecuteAttack();
                }
                return;
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
