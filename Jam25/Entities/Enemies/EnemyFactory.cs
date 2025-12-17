using System;
using System.Collections.Generic;
using HDT.Gaming.Audio;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
                EnemyController = enemyController,
            };
        }
    }
}
