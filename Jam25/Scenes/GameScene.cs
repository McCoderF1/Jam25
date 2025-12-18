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
        private int playerAttackState = 0;


        public GameMap GameMap { get; set; }

        public Player Player { get; }

        public List<Enemy> Enemies { get; } = [];

        public List<IPickup> Pickups { get; } = [];

        public PhysicsWorld PhysicsWorld { get; } = new();

        public IEnemySpawner EnemySpawner { get; set; }

        public int GameLevel { get; set; } = 1;

        public GameScene(GameMap gameMap, Player player)
        {
            GameMap = gameMap;
            Player = player;
        }

        public void Reset()
        {
            Enemies.Clear();
            Pickups.Clear();
            PhysicsWorld.ClearBodies();
        }

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

            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemy enemy = Enemies[i];

                // Update the projectiles
                List<Projectile> toRemove = new();
                foreach (Projectile p in enemy.Projectiles)
                {
                    p.Update(gameTime.ElapsedGameTime.Milliseconds);
                    if (Vector2.Distance(p.Position, Player.Body.Position) < 20 && p.CurrentState == Projectile.ProjectileState.Alive)
                    {
                        p.HitSomething();
                        Player.TakeDamage(p.Damage);
                    }
                    if (GameMap.tiles[(int)p.Position.X / 32, (int)p.Position.Y / 32].Type != TileType.Floor)
                    {
                        p.HitSomething();
                    }
                    if (p.CurrentState == Projectile.ProjectileState.Dead)
                    {
                        toRemove.Add(p);
                    }
                }
                enemy.Projectiles.RemoveAll(i => toRemove.Contains(i));

                enemy.EnemyController?.Update(this, enemy, gameTime.ElapsedGameTime);
                enemy.CurrentSprite.Update(GetDirection(enemy), gameTime);

                float distFromPlayer = Vector2.Distance(enemy.Body.Position, Player.Body.Position);

                if (attacking)
                {
                    if (distFromPlayer < Player.AttackRange)
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
            Vector2 proposedPosition = enemy.Body.Position + movement;

            const float enemyRadius = 10f;
            const float minDistance = enemyRadius * 2f;

            // First, move to proposed position
            enemy.Body.Position = proposedPosition;

            // Then apply simple separation from other enemies
            foreach (var other in Enemies)
            {
                if (ReferenceEquals(other, enemy))
                {
                    continue;
                }

                Vector2 delta = enemy.Body.Position - other.Body.Position;
                float distance = delta.Length();
                if (distance <= 0.0001f)
                {
                    continue;
                }

                if (distance < minDistance)
                {
                    float overlap = minDistance - distance;
                    Vector2 pushDir = delta / distance;

                    // Move this enemy away by half the overlap
                    enemy.Body.Position += pushDir * (overlap * 0.5f);
                    // Optionally also push the other enemy:
                    // other.Body.Position -= pushDir * (overlap * 0.5f);
                }
            }
        }
    }
}
