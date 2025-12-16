using System.Collections.Generic;
using HDT.Gaming.Physics;
using Jam25.Entities;
using Jam25.Entities.Enemies;
using Jam25.Graphics;
using Jam25.Models;
using Microsoft.Xna.Framework;

namespace Jam25.Scenes
{
    /// <summary>
    /// Class representing a game scene.
    /// </summary>
    public class GameScene
    {
        public GameMap GameMap { get; }

        public Player Player { get; }

        public List<Enemy> Enemies { get; } = [];

        public PhysicsWorld PhysicsWorld { get; } = new();

        public IEnemySpawner EnemySpawner { get; internal set; }

        public GameScene(GameMap gameMap, Player player, IEnemySpawner enemySpawner)
        {
            GameMap = gameMap;
            Player = player;
            EnemySpawner = enemySpawner;
        }

        public void Update(GameTime gameTime)
        {
            EnemySpawner.Update(this, gameTime);

            List<Enemy> enemiesToRemove = new();
            foreach (var enemy in Enemies)
            {
                enemy.EnemyController?.Update(this, enemy, gameTime.ElapsedGameTime);
                enemy.CurrentSprite.Update(Direction.Down, gameTime);

                float distFromPlayer = Vector2.Distance(enemy.Body.Position, Player.Body.Position);

                if (distFromPlayer < 50 && Player.IsAttacking)
                {
                    enemy.TakeDamage(2);

                }
                if (distFromPlayer < 30 && enemy.CurrentState != Enemy.EnemyState.Dying && enemy.CanAttack)
                {
                    Player.TakeDamage(20);
                    _ = enemy.StartAttackCooldown();
                }

                if (enemy.CurrentState == Enemy.EnemyState.Dead)
                {
                    enemiesToRemove.Add(enemy);
                }

                enemy.Update(gameTime);
            }

            foreach (var enemy in enemiesToRemove)
            {
                Enemies.Remove(enemy);
                PhysicsWorld.RemoveBody(enemy.Body);
            }

            PhysicsWorld.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
