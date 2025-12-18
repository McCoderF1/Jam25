using HDT.Gaming.Models;
using HDT.Gaming.Physics;
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

        Health Health;
        Texture2D Texture;
        Texture2D ProjectileTexture;
        List<Texture2D> ExplosionTextures;
        public Vector2 Position;
        public List<Projectile> Projectiles = new();
        float attackBlockedUntil = 0f;
        float attackCooldown = 500f;


        public Boss(ContentManager content)
        {
            Health = new Health(1000);

            Texture = content.Load<Texture2D>($"Boss/secondphase");
            ProjectileTexture = content.Load<Texture2D>("Images/projectile");

            ExplosionTextures = new List<Texture2D>();
            for (int i = 1; i <= 10; i++)
            {
                ExplosionTextures.Add(content.Load<Texture2D>($"Images/explosion/Circle_explosion{i}"));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                Texture,
                Position,
                Color.White);
        }

        private void StartCooldown()
        {
            attackBlockedUntil = attackCooldown;
        }

        public void Update(GameTime gameTime, Vector2 playerPos)
        {
            if (attackBlockedUntil > 0f)
            {
                attackBlockedUntil -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                return;
            }


            StartCooldown();
            Projectiles.Add(new Projectile()
            {
                Position = Position,
                Direction = Math.Atan2(playerPos.Y - Position.Y, playerPos.X - Position.X),
                Velocity = 500,
                Texture = ProjectileTexture,
                ExplosionTextures = this.ExplosionTextures,
                Damage = 5,
                Lifespan = 2000  // ms before removed
            });
        }
    }
}
