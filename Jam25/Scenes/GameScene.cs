using HDT.Gaming.Audio;
using HDT.Gaming.Physics;
using Jam25.Entities;
using Jam25.Entities.Enemies;
using Jam25.Graphics;
using Jam25.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

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

        private int playerAttackState = 0;

        public void Update(GameTime gameTime)
        {
            EnemySpawner.Update(this, gameTime);

            List<Enemy> enemiesToRemove = new();

            bool attacking = false;
            if (Player.IsAttacking != playerAttackState)
            {
                playerAttackState = Player.IsAttacking;

                attacking = playerAttackState > 0;
            }

            foreach (var enemy in Enemies)
            {
                enemy.EnemyController?.Update(this, enemy, gameTime.ElapsedGameTime);
                enemy.CurrentSprite.Update(Direction.Down, gameTime);

                float distFromPlayer = Vector2.Distance(enemy.Body.Position, Player.Body.Position);

                if (attacking)
                {
                    if (distFromPlayer < 50)
                    {
                        enemy.TakeDamage(2);
                        AudioManager.PlaySound("MetalHit");
                    }
                    else
                    {
                        AudioManager.PlaySound("Miss");
                    }
                }

                if (distFromPlayer < 30 && enemy.CanAttack && Player.LastState != Player.PlayerState.Dying)
                {
                    enemy.StartCooldown();
                    Player.TakeDamage(20);
                    enemy.CurrentState = Enemy.EnemyState.Attacking;
                    AudioManager.PlaySound("MetalHit");
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
