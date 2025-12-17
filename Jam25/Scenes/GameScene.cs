using System;
using System.Collections.Generic;
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
                if (enemy.CurrentState != Enemy.EnemyState.Dead && enemy.CurrentState != Enemy.EnemyState.Dying)
                {
                    enemy.EnemyController?.Update(this, enemy, gameTime.ElapsedGameTime);
                    MoveEnemy(enemy, gameTime);
                    enemy.CurrentSprite.Update(GetDirection(enemy), gameTime);
                    enemy.CurrentSprite.Update(Direction.Down, gameTime);
                }

                float distFromPlayer = Vector2.Distance(enemy.Body.Position, Player.Body.Position);

                if (attacking)
                {
                    if (distFromPlayer < 50)
                    {
                        enemy.TakeDamage(3);
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

        private Direction GetDirection(Enemy enemy)
        {
            Vector2 dir = enemy.MovementDirection;
            if (dir.Length() < 0.0001f)
            {
                return Direction.Down;
            }

            dir.Normalize();

            if (Math.Abs(dir.X) <= Math.Abs(dir.Y))
            {
                return dir.Y > 0 ? Direction.Down : Direction.Up;
            }
            else
            {
                return dir.X > 0 ? Direction.Right : Direction.Left;
            }
        }

        private void MoveEnemy(Enemy enemy, GameTime gameTime)
        {
            float speed = enemy.MovementSpeed;
            Vector2 movement = enemy.MovementDirection * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            enemy.Body.Position += movement;
        }
    }
}
