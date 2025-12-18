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
        int phase;

        public Health Health;
        Texture2D Texture;
        Texture2D ProjectileTexture;
        List<Texture2D> ExplosionTextures;
        public Vector2 Position;
        public List<Projectile> Projectiles = new();
        private readonly Texture2D whitePixel;
        float attackBlockedUntil = 0f;
        float attackCooldown = 500f;
        bool Alive = true;


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
                Alive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Alive)
            {
                return;
            }
            var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
            spriteBatch.Draw(Texture, Position, null, Color.White, 0, origin, 0.2f, SpriteEffects.None, 0f);
        }

        private void StartCooldown()
        {
            attackBlockedUntil = attackCooldown;
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

        public void Update(GameTime gameTime, Vector2 playerPos)
        {

            if (!Alive)
            {
                return;
            }


            if (attackBlockedUntil > 0f)
            {
                attackBlockedUntil -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                return;
            }


            float distFromPlayer = Vector2.Distance(Position, playerPos);

            if (distFromPlayer < 100)
            {
                attackCooldown = 500;
                for (double dTheta = -Math.PI / 4; dTheta < Math.PI / 4; dTheta += Math.PI / 24)
                {
                    StartCooldown();
                    Projectiles.Add(new Projectile()
                    {
                        Position = Position,
                        Direction = Math.Atan2(playerPos.Y - Position.Y, playerPos.X - Position.X) + dTheta,
                        Velocity = 500,
                        Texture = ProjectileTexture,
                        ExplosionTextures = this.ExplosionTextures,
                        Damage = 5,
                        Lifespan = 2000  // ms before removed
                    });
                }
            }
            else
            {
                attackCooldown = 1000;
                for (double dTheta = -Math.PI / 6; dTheta < Math.PI / 6; dTheta += Math.PI / 16)
                {
                    StartCooldown();
                    Projectiles.Add(new Projectile()
                    {
                        Position = Position,
                        Direction = Math.Atan2(playerPos.Y - Position.Y, playerPos.X - Position.X) + dTheta,
                        Velocity = 500,
                        Texture = ProjectileTexture,
                        ExplosionTextures = this.ExplosionTextures,
                        Damage = 5,
                        Lifespan = 2000  // ms before removed
                    });
                }
            }


        }
    }
}
