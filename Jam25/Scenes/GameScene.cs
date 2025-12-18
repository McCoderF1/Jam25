using HDT.Gaming.Audio;
using HDT.Gaming.Physics;
using Jam25.Entities;
using Jam25.Entities.Enemies;
using Jam25.Entities.Pickups;
using Jam25.Graphics;
using Jam25.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Jam25.Scenes
{
    /// <summary>
    /// Class representing a game scene.
    /// </summary>
    public class GameScene
    {
        public GameMap GameMap { get; set; }

        public Player Player { get; }

        public List<Enemy> Enemies { get; } = [];

        public List<IPickup> Pickups { get; } = [];

        public PhysicsWorld PhysicsWorld { get; } = new();

        public IEnemySpawner EnemySpawner { get; set; }

        public GameScene(GameMap gameMap, Player player)
        {
            GameMap = gameMap;
            Player = player;
        }

        private int playerAttackState = 0;

        public void Update(GameTime gameTime)
        {
            EnemySpawner?.Update(this, gameTime);

            List<Enemy> enemiesToRemove = new();

            bool attacking = false;
            bool hitSomething = false;
            if (Player.IsAttacking != playerAttackState)
            {
                playerAttackState = Player.IsAttacking;

                attacking = playerAttackState > 0;
            }

            foreach (var enemy in Enemies)
            {
                // Update the projectiles
                List<Projectile> toRemove = new();
                foreach (Projectile p in enemy.Projectiles)
                {
                    p.Update(gameTime.ElapsedGameTime.Milliseconds);
                    if (Vector2.Distance(p.Position, Player.Body.Position) < 20)
                    {
                        toRemove.Add(p);
                        Player.TakeDamage(p.Damage);
                    }
                    if (!p.Alive)
                    {
                        toRemove.Add(p);
                    }
                    if (GameMap.tiles[(int)p.Position.X / 32, (int)p.Position.Y / 32].Type != TileType.Floor)
                    {
                        toRemove.Add(p);
                    }
                }
                enemy.Projectiles.RemoveAll(i => toRemove.Contains(i));

                if (enemy.CanMove
                    && Player.LastState != Player.PlayerState.Dying)
                {
                    MoveEnemy(enemy, gameTime);
                }
                enemy.EnemyController?.Update(this, enemy, gameTime.ElapsedGameTime);
                enemy.CurrentSprite.Update(GetDirection(enemy), gameTime);

                float distFromPlayer = Vector2.Distance(enemy.Body.Position, Player.Body.Position);

                if (attacking)
                {
                    if (distFromPlayer < 50)
                    {
                        enemy.TakeDamage(4);
                        hitSomething = true;
                    }
                }
                else if (distFromPlayer < enemy.AttackRange && enemy.CanAttack && Player.LastState != Player.PlayerState.Dying)
                {
                    enemy.Attack(Player);
                }

                if (enemy.CanMove && Player.LastState != Player.PlayerState.Dying)
                {
                    MoveEnemy(enemy, gameTime);
                }

                if (enemy.CurrentState == Enemy.EnemyState.Dead)
                {
                    enemiesToRemove.Add(enemy);
                }

                enemy.Update(gameTime);
            }

            if (attacking)
            {
                if (hitSomething)
                    AudioManager.PlaySound("MetalHit");
                else
                    AudioManager.PlaySound("Miss");
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
