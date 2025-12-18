using HDT.Gaming.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Jam25.Entities
{
    public class Projectile
    {
        public enum ProjectileState
        {
            Alive, Animating, Dead
        }
        public ProjectileState CurrentState = ProjectileState.Alive;

        public Vector2 Position;
        public double Direction;
        public double Velocity;
        public Texture2D Texture;
        public List<Texture2D> ExplosionTextures;
        public int Damage;
        public float Lifespan;
        public Jam25.Entities.Enemies.Enemy Target;

        private float animationTime = 500;
        


        public void Update(float deltaMS)
        {
            Lifespan -= deltaMS;
            if (Lifespan <= 0)
            {
                if (CurrentState == ProjectileState.Alive)
                {
                    HitSomething();
                }
                else
                {
                    CurrentState = ProjectileState.Dead;
                }
            }

            if (CurrentState == ProjectileState.Alive)
            {
                Position = Vector2.Add(
                    Position,
                    new Vector2(
                        (float)(Velocity * deltaMS * Math.Cos(Direction) / 1000),
                        (float)(Velocity * deltaMS * Math.Sin(Direction) / 1000)
                    )
                );
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentState == ProjectileState.Alive)
            {
                var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
                spriteBatch.Draw(Texture, Position, null, Color.White, (float)(Direction + Math.PI / 2f), origin, 1f, SpriteEffects.None, 0f);
            }
            else if (CurrentState == ProjectileState.Animating)
            {
                if (ExplosionTextures != null && ExplosionTextures.Count > 0)
                {
                    int state = Math.Clamp(10 - (int)(Lifespan / animationTime * 10), 0, ExplosionTextures.Count - 1);
                    Texture2D texture = ExplosionTextures[state];
                    var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                    spriteBatch.Draw(texture, Position, null, Color.White, (float)Direction, origin, 0.25f, SpriteEffects.None, 0f);
                }
            }
        }

        public void HitSomething()
        {
            if (CurrentState == ProjectileState.Alive)
            {
                CurrentState = ProjectileState.Animating;
                Lifespan = animationTime;
            }
        }
    }
}
