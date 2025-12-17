using HDT.Gaming.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Entities
{
    public class Projectile : IDisposable
    {
        public Vector2 Position;
        public double Direction;
        public double Velocity;
        public Texture2D Texture;
        public int Damage;
        public float Lifespan;
        public bool Alive = true;

        public void Update(float deltaMS)
        {
            Lifespan -= deltaMS;
            if (Lifespan <= 0)
            {
                Dispose();
            }

            Position = Vector2.Add(
                Position,
                new Vector2(
                    (float)(Velocity * deltaMS * Math.Cos(Direction) / 1000),
                    (float)(Velocity * deltaMS * Math.Sin(Direction) / 1000)
                )
            );
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Alive)
            {
                var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
            }
        }

        public void Dispose()
        {
            Alive = false;
        }
    }
}
