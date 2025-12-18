using HDT.Gaming.Models;
using HDT.Gaming.Physics;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Entities.Enemies
{
    public class Boss
    {
        private float hitFlashDuration = 0.1f; // seconds
        private float hitFlashTimer = 0f;

        public bool IsHitFlashing => hitFlashTimer > 0f;

        public enum Stage
        {
            Phase1,
            Phase2,
            Dead
        }
        public Stage CurrentStage;

        public Health Health;
        Texture2D Texture;
        Texture2D ProjectileTexture;
        List<Texture2D> ExplosionTextures;
        public Vector2 Position;
        public List<Projectile> Projectiles = new();
        private readonly Texture2D whitePixel;
        float attackBlockedUntil = 0f;
        float attackCooldown = 500f;
        float moveBlockedUntil = 0f;
        float moveCooldown = 2000f;

        float vel = 10f;
        float dir;

        Random rand = new Random();


        public Boss(ContentManager content)
        {
            Health = new Health(1000);

            whitePixel = content.Load<Texture2D>("Textures/WhiteRectangle");
            Texture = content.Load<Texture2D>($"Boss/secondphase");
            ProjectileTexture = content.Load<Texture2D>("Images/projectile");

            ExplosionTextures = new List<Texture2D>();
            for (int i = 1; i <= 10; i++)
            {
                ExplosionTextures.Add(content.Load<Texture2D>($"Images/explosion/Circle_explosion{i}"));
            }
        }

        public void TakeDamage(int amount)
        {
            Health.TakeDamage(amount);
            if (Health.Current <= 0)
            {
                if (CurrentStage == Stage.Phase1)
                {
                    Health.Current = Health.Max;
                    CurrentStage = Stage.Phase2;
                }
                else
                {
                    CurrentStage = Stage.Dead;
                }
            }

            hitFlashTimer = hitFlashDuration;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentStage == Stage.Dead)
                return;

            float t = hitFlashTimer / hitFlashDuration;
            Color flash = Color.Lerp(Color.White, Color.Red, t);

            var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
            spriteBatch.Draw(Texture, Position, null, flash, 0, origin, 0.2f, SpriteEffects.None, 0f);
        }

        private void StartAttackCooldown()
        {
            attackBlockedUntil = attackCooldown;
        }
        private void StartMoveCooldown()
        {
            moveBlockedUntil = moveCooldown;
        }

        public void DrawHealthBar(SpriteBatch spriteBatch, int screenWidth)
        {
            float healthPercent = Math.Clamp((float)Health.Current / (float)Health.Max, 0f, 1f);

            int barWidth = screenWidth / 2;
            int barHeight = 30;
            int barX = screenWidth / 4;
            int barY = 20;

            // Background
            var bgRect = new Rectangle(barX, barY, barWidth, barHeight);
            spriteBatch.Draw(whitePixel, bgRect, Color.Black * 0.75f);

            // Fill
            int fillWidth = (int)(barWidth * healthPercent);
            var fillRect = new Rectangle(barX + 1, barY + 1, Math.Max(0, fillWidth - 2), barHeight - 2);
            var fillColor = Color.Lerp(Color.Red, Color.Green, healthPercent);
            spriteBatch.Draw(whitePixel, fillRect, fillColor);
        }

        public void Update(GameTime gameTime, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (hitFlashTimer > 0f)
                hitFlashTimer -= delta;

            Vector2 playerPos = player.Body.Position;
            float distFromPlayer = Vector2.Distance(Position, playerPos);


            if (CurrentStage == Stage.Phase1)
            {
                Position = new Vector2(
                    (float)(Position.X + Math.Min(vel, distFromPlayer) * Math.Cos(dir) / gameTime.ElapsedGameTime.TotalMilliseconds),
                    (float)(Position.Y + Math.Min(vel, distFromPlayer) * Math.Sin(dir) / gameTime.ElapsedGameTime.TotalMilliseconds));
            }
            vel = Math.Max(vel * 0.95f, 5);


            if (attackBlockedUntil > 0f)
            {
                attackBlockedUntil -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                return;
            }




            switch (CurrentStage)
            {
                case Stage.Dead:
                    return;

                // meele phase
                case Stage.Phase1:

                    if (moveBlockedUntil > 0f)
                    {
                        moveBlockedUntil -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                    else
                    {
                        StartMoveCooldown();
                        // jump towards player

                        dir = (float)Math.Atan2(playerPos.Y - Position.Y, playerPos.X - Position.X);
                        vel = 100f;
                    }

                    if (distFromPlayer < 50)
                    {
                        attackCooldown = 1500;
                        StartAttackCooldown();
                        player.TakeDamage(5);
                    }
                    break;

                // Projectile phase
                case Stage.Phase2:
                    if (distFromPlayer < 50)
                    {
                        attackCooldown = 1000;
                        for (double dTheta = -Math.PI; dTheta < Math.PI; dTheta += Math.PI / 24)
                        {
                            StartAttackCooldown();
                            Projectiles.Add(new Projectile()
                            {
                                Position = Position,
                                Direction = Math.Atan2(playerPos.Y - Position.Y, playerPos.X - Position.X) + dTheta,
                                Velocity = 100,
                                Texture = ProjectileTexture,
                                ExplosionTextures = this.ExplosionTextures,
                                Damage = 5,
                                Lifespan = 1000  // ms before removed
                            });
                        }
                    }
                    else
                    {
                        attackCooldown = 2000;
                        for (double dTheta = -Math.PI / 6; dTheta < Math.PI / 6; dTheta += Math.PI / 24)
                        {
                            StartAttackCooldown();
                            if (rand.Next(2) == 0)
                            {
                                Projectiles.Add(new Projectile()
                                {
                                    Position = Position,
                                    Direction = Math.Atan2(playerPos.Y - Position.Y, playerPos.X - Position.X) + dTheta,
                                    Velocity = 100,
                                    Texture = ProjectileTexture,
                                    ExplosionTextures = this.ExplosionTextures,
                                    Damage = 5,
                                    Lifespan = 10000  // ms before removed
                                });
                            }
                        }
                    }
                    break;

            }
        }
    }
}
