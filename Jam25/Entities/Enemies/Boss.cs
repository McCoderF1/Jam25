
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
using static System.TimeZoneInfo;

namespace Jam25.Entities.Enemies
{
    public class Boss
    {
        public enum Stage
        {
            Phase1,
            Transition,
            Phase2,
            Dead
        }
        public Stage CurrentStage;

        public Health Health;
        Texture2D CurrentTexture;
        Texture2D Phase1Texture;
        Texture2D Phase2Texture;
        Texture2D TransitionTexture;
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
        private readonly float transitionDurationMs = 6000f;
        private float transitionTimer = 0f;

        private readonly List<Texture2D> TransitionFrames = new();
        private bool HasTransitionFrames => TransitionFrames.Count > 0;
        private int transitionFrameIndex = 0;
        private float transitionFrameTimerMs = 0f;
        // duration per frame (ms). Adjust as needed or load from metadata if available.
        private readonly float transitionFrameDurationMs = 100f;


        public Boss(ContentManager content)
        {
            Health = new Health(1000);

            whitePixel = content.Load<Texture2D>("Textures/WhiteRectangle");
            Phase1Texture = content.Load<Texture2D>($"Boss/firstphase");
            Phase2Texture = content.Load<Texture2D>($"Boss/secondphase");
            //TransitionTexture = content.Load<Texture2D>($"Boss/transition");
            CurrentTexture = Phase1Texture;
            CurrentStage = Stage.Phase1;
            ProjectileTexture = content.Load<Texture2D>("Images/projectile");

            ExplosionTextures = new List<Texture2D>();
            for (int i = 1; i <= 10; i++)
            {
                ExplosionTextures.Add(content.Load<Texture2D>($"Images/explosion/Circle_explosion{i}"));
            }

            for (int i = 0; i < 52; i++)
            {
                try
                {
                    var frame = content.Load<Texture2D>($"Boss/transition/frame_{i.ToString("00")}_delay-0.1s");
                    if (frame != null)
                        TransitionFrames.Add(frame);
                }
                catch
                {
                    // stop at first missing frame resource
                    break;
                }
            }
           Console.WriteLine(TransitionFrames.Count);
        }

        public void TakeDamage(int amount)
        {
            Health.TakeDamage(amount);
            if (Health.Current <= 0)
            {
                if (CurrentStage == Stage.Phase1)
                {
                    Health.Current = Health.Max;
                    EnterTransition();
                }
                else if (CurrentStage == Stage.Phase2)
                {
                    CurrentStage = Stage.Dead;
                }
            }
        }

        private void EnterTransition()
        {
            CurrentStage = Stage.Transition;
            transitionTimer = transitionDurationMs;
            // choose appropriate visual: if frames available, start at 0, otherwise use static texture
            if (HasTransitionFrames)
            {
                transitionFrameIndex = 0;
                transitionFrameTimerMs = 0f;
            }
            CurrentTexture = TransitionTexture;
            // block attacks/movement during transition
            attackBlockedUntil = float.MaxValue;
            moveBlockedUntil = float.MaxValue;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentStage == Stage.Dead)
            {
                return;
            }

            Texture2D toDraw;

            if (CurrentStage == Stage.Transition && HasTransitionFrames)
            {
                // Clamp index defensively
                int index = transitionFrameIndex;
                if (index < 0)
                {
                    index = 0;
                }
                if (index >= TransitionFrames.Count)
                {
                    index = TransitionFrames.Count - 1;
                }

                toDraw = TransitionFrames[index];
            }
            else
            {
                toDraw = CurrentTexture;
            }

            var origin = new Vector2(toDraw.Width / 2f, toDraw.Height / 2f);
            spriteBatch.Draw(toDraw, Position, null, Color.White, 0, origin, 0.2f, SpriteEffects.None, 0f);
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
            float deltaMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // If in transition, advance frames & timer then return (no attacks/movement)
            if (CurrentStage == Stage.Transition)
            {
                if (HasTransitionFrames && TransitionFrames.Count > 0)
                {
                    transitionFrameTimerMs += deltaMs;
                    while (transitionFrameTimerMs >= transitionFrameDurationMs)
                    {
                        transitionFrameTimerMs -= transitionFrameDurationMs;
                        transitionFrameIndex++;
                        if (transitionFrameIndex >= TransitionFrames.Count)
                            transitionFrameIndex = 0; // loop while transitioning
                    }
                }

                transitionTimer -= deltaMs;
                if (transitionTimer <= 0f)
                {
                    // finish transition -> Phase2
                    CurrentStage = Stage.Phase2;
                    CurrentTexture = Phase2Texture;

                    // allow attacks/movement again, reset cooldowns
                    attackBlockedUntil = 0f;
                    moveBlockedUntil = 0f;
                }
                return;
            }

            Vector2 playerPos = player.Body.Position;
            float distFromPlayer = Vector2.Distance(Position, playerPos);

            // decrement timers (attack/move)
            if (attackBlockedUntil > 0f && attackBlockedUntil < float.MaxValue)
            {
                attackBlockedUntil -= deltaMs;
            }
            if (moveBlockedUntil > 0f && moveBlockedUntil < float.MaxValue)
            {
                moveBlockedUntil -= deltaMs;
            }

            // Movement while in Phase1 (still allowed)
            if (CurrentStage == Stage.Phase1)
            {
                Position = new Vector2(
                    (float)(Position.X + Math.Min(vel, distFromPlayer) * Math.Cos(dir) / Math.Max(1, deltaMs)),
                    (float)(Position.Y + Math.Min(vel, distFromPlayer) * Math.Sin(dir) / Math.Max(1, deltaMs)));
            }
            vel = Math.Max(vel * 0.95f, 5);

            if (attackBlockedUntil > 0f)
            {
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
                        // already decrementing above
                    }
                    else
                    {
                        StartMoveCooldown();
                        // jump towards player

                        dir = (float)Math.Atan2(playerPos.Y - Position.Y, playerPos.X - Position.X);
                        vel = 100f;
                    }


                    if (distFromPlayer < 100)
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