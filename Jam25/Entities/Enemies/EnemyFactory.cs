using HDT.Gaming.Audio;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Jam25.Entities.Enemies
{
    /// <summary>
    /// Factory class for creating Enemy instances.
    /// </summary>
    public class EnemyFactory
    {
        #region Private Members

        private const int FRAME_WIDTH = 64;
        private readonly TimeSpan FRAME_TIME = TimeSpan.FromMilliseconds(100);

        private readonly ContentManager content;
        private readonly AudioController audioController;
        private readonly BasicEnemyController enemyController;

        private readonly Texture2D slimeAttackTexture;
        private readonly Texture2D slimeDeathTexture;
        private readonly Texture2D slimeHurtTexture;
        private readonly Texture2D slimeIdleTexture;
        private readonly Texture2D slimeWalkTexture;
        private readonly Texture2D slimeRunTexture;

        private readonly Texture2D lavaSlimeAttackTexture;
        private readonly Texture2D lavaSlimeDeathTexture;
        private readonly Texture2D lavaSlimeHurtTexture;
        private readonly Texture2D lavaSlimeIdleTexture;
        private readonly Texture2D lavaSlimeWalkTexture;
        private readonly Texture2D lavaSlimeRunTexture;

        private readonly Texture2D vampireAttackTexture;
        private readonly Texture2D vampireDeathTexture;
        private readonly Texture2D vampireHurtTexture;
        private readonly Texture2D vampireIdleTexture;
        private readonly Texture2D vampireWalkTexture;
        private readonly Texture2D vampireRunTexture;

        private readonly Texture2D projectileTexture;
        private readonly List<Texture2D> explosionTextures;

        #endregion Private Members

        public EnemyFactory(ContentManager content, AudioController audioController)
        {
            this.content = content;
            this.audioController = audioController;

            enemyController = new();

            slimeAttackTexture = content.Load<Texture2D>("EnemySprite/Slime/Slime2_Attack_with_shadow");
            slimeDeathTexture = content.Load<Texture2D>("EnemySprite/Slime/Slime2_Death_with_shadow");
            slimeHurtTexture = content.Load<Texture2D>("EnemySprite/Slime/Slime2_Hurt_with_shadow");
            slimeIdleTexture = content.Load<Texture2D>("EnemySprite/Slime/Slime2_Idle_with_shadow");
            slimeWalkTexture = content.Load<Texture2D>("EnemySprite/Slime/Slime2_Walk_with_shadow");
            slimeRunTexture = content.Load<Texture2D>("EnemySprite/Slime/Slime2_Run_with_shadow");

            lavaSlimeAttackTexture = content.Load<Texture2D>("EnemySprite/LavaSlime/Slime3_Attack_with_shadow");
            lavaSlimeDeathTexture = content.Load<Texture2D>("EnemySprite/LavaSlime/Slime3_Death_with_shadow");
            lavaSlimeHurtTexture = content.Load<Texture2D>("EnemySprite/LavaSlime/Slime3_Hurt_with_shadow");
            lavaSlimeIdleTexture = content.Load<Texture2D>("EnemySprite/LavaSlime/Slime3_Idle_with_shadow");
            lavaSlimeWalkTexture = content.Load<Texture2D>("EnemySprite/LavaSlime/Slime3_Walk_with_shadow");
            lavaSlimeRunTexture = content.Load<Texture2D>("EnemySprite/LavaSlime/Slime3_Run_with_shadow");

            vampireAttackTexture = content.Load<Texture2D>("EnemySprite/Vampire/Vampires2_Attack_with_shadow");
            vampireDeathTexture = content.Load<Texture2D>("EnemySprite/Vampire/Vampires2_Death_with_shadow");
            vampireHurtTexture = content.Load<Texture2D>("EnemySprite/Vampire/Vampires2_Hurt_with_shadow");
            vampireIdleTexture = content.Load<Texture2D>("EnemySprite/Vampire/Vampires2_Idle_with_shadow");
            vampireWalkTexture = content.Load<Texture2D>("EnemySprite/Vampire/Vampires2_Walk_with_shadow");
            vampireRunTexture = content.Load<Texture2D>("EnemySprite/Vampire/Vampires2_Run_with_shadow");

            projectileTexture = content.Load<Texture2D>("Images/projectile");

            explosionTextures = new List<Texture2D>();
            for (int i = 1; i <= 10; i++)
            {
                explosionTextures.Add(content.Load<Texture2D>($"Images/explosion/Circle_explosion{i}"));
            }
        }

        public Enemy CreateSlimeEnemy(Vector2 position)
        {
            return new Enemy()
            {
                Sprites = new Dictionary<Enemy.EnemyState, AnimatedDirectionalSprite>
                {
                    {Enemy.EnemyState.Idle, new AnimatedDirectionalSprite(slimeIdleTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Attacking, new AnimatedDirectionalSprite(slimeAttackTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Hurt, new AnimatedDirectionalSprite(slimeHurtTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Dying, new AnimatedDirectionalSprite(slimeDeathTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Running, new AnimatedDirectionalSprite(slimeRunTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Dead, new AnimatedDirectionalSprite(slimeDeathTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                },
                Body =
                {
                    Position = position,
                    LocalBounds = new (0, 0, FRAME_WIDTH, FRAME_WIDTH),
                    PositionOffset = new Vector2(FRAME_WIDTH * 0.5f, FRAME_WIDTH)
                },
                Health = new HDT.Gaming.Models.Health(10),
                MovementSpeed = 30,
                ChaseMemoryDuration = TimeSpan.FromSeconds(3),
                SightRange = 250,
                EnemyController = enemyController,
                AttackRange = 30,
                UseProjectiles = false,
                StopsToAttack = false,
                AttackDamage = 10
            };
        }

        public Enemy CreateLavaSlimeEnemy(Vector2 position)
        {
            return new Enemy()
            {
                Sprites = new Dictionary<Enemy.EnemyState, AnimatedDirectionalSprite>
                {
                    {Enemy.EnemyState.Idle, new AnimatedDirectionalSprite(lavaSlimeIdleTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Attacking, new AnimatedDirectionalSprite(lavaSlimeAttackTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Hurt, new AnimatedDirectionalSprite(lavaSlimeHurtTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Dying, new AnimatedDirectionalSprite(lavaSlimeDeathTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Running, new AnimatedDirectionalSprite(lavaSlimeRunTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Dead, new AnimatedDirectionalSprite(lavaSlimeDeathTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                },
                Body =
                {
                    Position = position,
                    LocalBounds = new (0, 0, FRAME_WIDTH, FRAME_WIDTH),
                    PositionOffset = new Vector2(FRAME_WIDTH * 0.5f, FRAME_WIDTH)
                },
                Health = new HDT.Gaming.Models.Health(15),
                MovementSpeed = 25,
                ChaseMemoryDuration = TimeSpan.FromSeconds(3),
                SightRange = 250,
                EnemyController = enemyController,
                AttackRange = 40,
                UseProjectiles = false,
                StopsToAttack = true,
                AttackWindUpMs = 800f, // 0.8 second delay before attack
                AttackDamage = 35
            };
        }

        public Enemy CreateVampireEnemy(Vector2 position)
        {
            return new Enemy()
            {
                Sprites = new Dictionary<Enemy.EnemyState, AnimatedDirectionalSprite>
                {
                    {Enemy.EnemyState.Idle, new AnimatedDirectionalSprite(vampireIdleTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Attacking, new AnimatedDirectionalSprite(vampireAttackTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Hurt, new AnimatedDirectionalSprite(vampireHurtTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Dying, new AnimatedDirectionalSprite(vampireDeathTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Running, new AnimatedDirectionalSprite(vampireRunTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                    {Enemy.EnemyState.Dead, new AnimatedDirectionalSprite(vampireDeathTexture, FRAME_WIDTH, new[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right }, FRAME_TIME)},
                },
                Body =
                {
                    Position = position,
                    LocalBounds = new (0, 0, FRAME_WIDTH, FRAME_WIDTH),
                    PositionOffset = new Vector2(FRAME_WIDTH * 0.5f, FRAME_WIDTH)
                },
                Health = new HDT.Gaming.Models.Health(20),
                MovementSpeed = 10,
                ChaseMemoryDuration = TimeSpan.FromSeconds(3),
                SightRange = 250,
                EnemyController = enemyController,
                AttackRange = 200,
                UseProjectiles = true,
                ProjectileTexture = projectileTexture,
                ExplosionTextures = explosionTextures
            };
        }
    }
}
